import type { Credentials, OAuth2Client } from 'google-auth-library';
import { type drive_v3, google } from 'googleapis';
import node_fs from 'node:fs';
import { appendFile } from 'node:fs/promises';
import { Readable } from 'node:stream';

import { Run } from 'lib/ericchase/Platform/Bun/Child Process.js';
import { CopyFile } from 'lib/ericchase/Platform/Bun/Fs.js';
import { DeleteFile } from 'lib/ericchase/Platform/Node/Fs.js';
import { Path } from 'lib/ericchase/Platform/Node/Path.js';
import { ConsoleError, ConsoleLog } from 'lib/ericchase/Utility/Console.js';
import { HasProperty } from 'lib/ericchase/Utility/Guard.js';
import { ConnectionChecker } from 'src/ConnectionChecker.js';
import { CredentialsPath, LogsPath, ResourceIDPath, ResourcePath, SetupScriptPath, TokenPath } from 'src/constants.js';
import { U8StreamReadSome } from 'src/lib/ericchase/Algorithm/Stream.js';
import { StdinTextReader } from 'src/lib/ericchase/Platform/Node/Process.js';
import { CreateOAuthClient, RefreshAccessToken, VerifyAccessToken } from 'src/OAuth2.js';
import { WebSocketClient } from 'src/WebSocketClient.js';

if (Bun.argv[2] === undefined) {
  throw 'Missing database file path in command line arguments.';
}
const DatabasePath = new Path(Bun.argv[2]);

const CredentialsFile = Bun.file(CredentialsPath.path);
if ((await CredentialsFile.exists()) === false) {
  throw `Missing credentials file at ${CredentialsPath.path}. It must be downloaded from the Google Cloud Account of the creator of this OAuth2 App.`;
}

// Function to authenticate with OAuth2
async function getOAuthClient() {
  try {
    const oauth2Client = await CreateOAuthClient(await CredentialsFile.json());
    try {
      oauth2Client.setCredentials(await getCredentials(oauth2Client));
    } catch (error) {
      await err(error);
      await runSetupScript();
    }
    try {
      oauth2Client.setCredentials(await getCredentials(oauth2Client));
    } catch (error) {
      await err(error);
    }
    return oauth2Client;
  } catch (error) {
    await err(error);
  }
  await err('OAuth2 client could not be authenticated. Please try again later.');
  process.exit();
}

async function getCredentials(oauth2Client: OAuth2Client): Promise<Credentials> {
  try {
    const credentials = await Bun.file(TokenPath.path).json();
    // verify access token
    if ((await VerifyAccessToken(credentials.access_token)).valid === true) {
      return credentials;
    }
    // refresh access token
    const newCredentials = await RefreshAccessToken(oauth2Client, credentials);
    await Bun.write(TokenPath.path, JSON.stringify(newCredentials));
    return credentials;
  } catch (error) {
    await err(error);
  }
  throw 'Invalid Access and Refresh Tokens.';
}

async function getDrive() {
  const oauth2Client = await getOAuthClient();
  return google.drive({
    version: 'v3',
    auth: oauth2Client,
  });
}

async function runSetupScript() {
  await log('Starting authentication process.');
  try {
    // works
    const cmd = Bun.which('cmd') ?? undefined; // windows
    if (cmd) {
      // `SetupScriptPath` cannot contain spaces. I don't have the energy or patience to figure out cmd voodoo.
      await Run.Silent(cmd, '/c', `start /wait cmd /k bun ${SetupScriptPath}`).exited;
      return;
    }

    // ! don't know if works
    const osascript = Bun.which('osascript') ?? undefined; // mac
    if (osascript) {
      // Based on how cmd works, I assume that other terminals can't handle spaces either.
      await Run.Silent(osascript, '-e', `tell application "Terminal" to do script "bun ${SetupScriptPath}"`).exited;
      return;
    }

    // ! don't know if works
    const sh = Bun.which('sh') ?? undefined; // linux / mac
    const xterm = Bun.which('xterm') ?? undefined;
    if (sh && xterm) {
      // Based on how cmd works, I assume that other terminals can't handle spaces either.
      await Run.Silent(sh, '-c', `xterm -e sh -c "bun ${SetupScriptPath}; exec bash"`).exited;
      return;
    }
  } catch (error) {
    await err(error);
  }
  await err('Unable to open a new terminal for authentication process.');
  process.exit();
}

// Function to pull a file from Google Drive
async function pullDatabase() {
  try {
    const drive = await getDrive();
    const resource_json = await tryParseJSONFile(ResourceIDPath);
    if (resource_json !== undefined) {
      const params: drive_v3.Params$Resource$Files$Get = {
        alt: 'media',
        fileId: resource_json.data,
      };
      const response = await drive.files.get(params, { responseType: 'stream' });
      await log('Get database file. Status:', response.status);
      try {
        await DeleteFile(ResourcePath);
      } catch (error) {}
      for await (const chunk of response.data) {
        await appendFile(ResourcePath.path, chunk);
      }
      await CopyFile({ from: ResourcePath, to: DatabasePath });
    }
  } catch (error) {
    await err('Error pulling database file:', error);
  }
}

// Function to push a file to Google Drive
async function pushDatabase() {
  try {
    const drive = await getDrive();
    const database_as_readable: Readable = await createReadable(DatabasePath);
    const resource_json = await tryParseJSONFile(ResourceIDPath);
    const params: drive_v3.Params$Resource$Files$Update = {
      media: {
        body: database_as_readable,
        mimeType: 'application/octet-stream',
      },
      requestBody: {
        mimeType: 'application/octet-stream',
        name: DatabasePath.base,
      },
    };
    if (resource_json === undefined) {
      const response = await drive.files.create(params);
      await log('Created database file. Status:', response.status);
      Bun.write(ResourceIDPath.path, JSON.stringify(response.data.id));
    } else {
      params.fileId = resource_json.data;
      const response = await drive.files.update(params);
      await log('Updated database file. Status:', response.status);
    }
  } catch (error) {
    await err('Error pushing database file:', error);
  }
}

// Convert Bun file stream into ReadableStream
async function createReadable(path: Path) {
  try {
    const stream = Bun.file(path.path).stream();
    return new Readable({
      async read(size) {
        const u8 = await U8StreamReadSome(stream, size);
        if (u8.byteLength === 0) {
          this.push(null);
        } else {
          this.push(u8);
        }
      },
    });
  } catch (error) {
    throw `Could not create Readable from file at ${path.path}: ${error}`;
  }
}
async function tryParseJSONFile(path: Path) {
  try {
    return { data: await Bun.file(path.path).json() };
  } catch (error) {
    return undefined;
  }
}

async function OnExit() {
  await log('Sync Server Stopping');
  process.exit();
}

const connection = new ConnectionChecker(
  30000,
  (message: string) => {
    ConsoleLog(message);
  },
  async () => {
    await log('No communication since last timeout.');
    OnExit();
  },
);

const stdin = new StdinTextReader();
stdin.addHandler(async (text) => {
  switch (text.trim()) {
    case 'exit':
      OnExit();
      break;
    case 'ping':
      connection.ProcessPing();
      break;
    case 'pong':
      connection.ProcessPong();
      break;
    case 'pull':
      await pullDatabase();
      await log('pulled');
      break;
    case 'push':
      await pushDatabase();
      await log('pushed');
      break;
  }
});
await stdin.start();
await log('Sync Server Running');
await getOAuthClient();

//                                                                           \\

// Push Server

// const notification_client = new WebSocketClient({
//   address: 'ws://localhost:3000',
//   auto_reconnect: true,
// });
// notification_client.onClose(async (event) => {
//   await log('Push Server Disconnected');
// });
// notification_client.onError(async (event) => {
//   if (HasProperty(event, 'message')) {
//     await log(`Push Server: ${event.message}`);
//   } else {
//     await log('Push Server Unknown Error');
//   }
// });
// notification_client.onMessage(async (event) => {
//   switch (event.data.trim()) {
//     case 'notify':
//       await log('notify');
//       break;
//     default:
//       await log(`Push Server: ${event.data}`);
//       break;
//   }
// });
// notification_client.onOpen(async (event) => {
//   await log('Push Server Connected');
// });
// notification_client.connect();

//                                                                           \\

// Utility

export async function filelog(...items: any[]) {
  await node_fs.promises.appendFile(LogsPath.path, `[L] ${items}\n`);
}

export async function log(...items: any[]) {
  ConsoleLog(...items);
  await node_fs.promises.appendFile(LogsPath.path, `[L] ${items}\n`);
}
export async function err(...items: any[]) {
  ConsoleError(...items);
  await node_fs.promises.appendFile(LogsPath.path, `[E]${items}\n`);
}

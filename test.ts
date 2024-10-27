import type { OAuth2Client } from 'google-auth-library';
import { google } from 'googleapis';

import { Path } from 'lib/ericchase/Platform/Node/Path.js';
import { Defer } from 'lib/ericchase/Utility/Defer.js';
import { FetchAccessToken, FetchRefreshToken, GenerateOfflineAuthUrl, GenerateOnlineAuthUrl } from 'src/OAuth2.js';
import { CredentialsPath, OAuthScope, ScriptPath, TokenPath } from 'src/constants.js';
import { StdinTextReader } from 'src/lib/ericchase/Platform/Node/Process.js';

//                                   \\
// Stdin
const stdin = new StdinTextReader();
let stdin_next = Defer<string>();
stdin.addHandler(async (text) => {
  stdin_next.resolve(text.trim());
  stdin_next = Defer<string>();
});
await stdin.start();
//                                   \\

const DatabasePath = new Path(Bun.argv[2]);
// const DatabaseFile = Bun.file(DatabasePath.path);
// const DatabaseFilename = path.basename(DatabasePath.path);
const LogPath = ScriptPath.newBase('logs.txt');

const CredentialsFile = Bun.file(CredentialsPath.path);
const CredentialsData = await CredentialsFile.json();
const { client_secret, client_id, redirect_uris } = CredentialsData.web;
const oauth2Client = new google.auth.OAuth2(client_id, client_secret, redirect_uris[0]);

// Steps

// access token
async function GetAccessToken(oauth2Client: OAuth2Client) {
  const authUrl = GenerateOnlineAuthUrl(oauth2Client, OAuthScope);
  console.log(authUrl);
  console.log();
  console.log('Obtaining Access Token. Visit the link above and authorize your account. When redirected to the final page, copy the value under "code" and paste it here. Then press enter.');
  {
    const code = await stdin_next.promise;
    const credentials = await FetchAccessToken(oauth2Client, code);
    await Bun.write(TokenPath.path, JSON.stringify(credentials));
    console.log({ credentials });
    if (credentials.access_token) {
      return credentials;
    }
  }
  throw 'Error Fetching Access Token';
}

// refresh token
async function GetRefreshToken(oauth2Client: OAuth2Client) {
  const authUrl = GenerateOfflineAuthUrl(oauth2Client, OAuthScope);
  console.log(authUrl);
  console.log();
  console.log('Obtaining Refresh Token. Visit the link above and authorize your account. When redirected to the final page, copy the value under "code" and paste it here. Then press enter.');
  {
    const code = await stdin_next.promise;
    const credentials = await FetchRefreshToken(oauth2Client, code);
    await Bun.write(TokenPath.path, JSON.stringify(credentials));
    console.log({ credentials });
    if (credentials.refresh_token) {
      return credentials;
    }
  }
  throw 'Error Fetching Refresh Token';
}

// const credentials = await GetAccessToken(oauth2Client);
// if (credentials.access_token) {
//   const { valid, error, error_description } = await VerifyAccessToken(credentials.access_token);
//   if (valid === true) {
//     console.log('success');
//   } else if (error || error_description) {
//     console.error(error, error_description);
//   }
// }
// process.exit();

// const credentials = await GetRefreshToken(oauth2Client);
// try {
//   const new_credentials = await RefreshAccessToken(oauth2Client, credentials);
//   console.log({ new_credentials });
// } catch (error) {
//   console.error(error);
// }


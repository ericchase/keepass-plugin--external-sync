import type { OAuth2Client } from 'google-auth-library';

import { StdinTextReader } from 'lib/ericchase/Platform/Node/Process.js';
import { Defer } from 'lib/ericchase/Utility/Defer.js';
import { CredentialsPath, OAuthScope, TokenPath } from 'src/constants.js';
import { CreateOAuthClient, FetchAccessToken, FetchRefreshToken, GenerateOfflineAuthUrl, GenerateOnlineAuthUrl } from 'src/OAuth2.js';

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

async function GetAccessToken(oauth2Client: OAuth2Client) {
  const authUrl = GenerateOnlineAuthUrl(oauth2Client, OAuthScope);
  console.log(authUrl);
  console.log();
  console.log('Obtaining Access Token. Visit the link above and authorize your account. When redirected to the final page, copy the value under "code" and paste it here. Then press enter.');
  {
    const oauth_code = await stdin_next.promise;
    const new_token = await FetchAccessToken(oauth2Client, oauth_code);
    await Bun.write(TokenPath.path, JSON.stringify(new_token));
  }
}

async function GetRefreshToken(oauth2Client: OAuth2Client) {
  const authUrl = GenerateOfflineAuthUrl(oauth2Client, OAuthScope);
  console.log(authUrl);
  console.log();
  console.log('Obtaining Refresh Token. Visit the link above and authorize your account. When redirected to the final page, copy the value under "code" and paste it here. Then press enter.');
  {
    const oauth_code = await stdin_next.promise;
    const new_token = await FetchRefreshToken(oauth2Client, oauth_code);
    await Bun.write(TokenPath.path, JSON.stringify(new_token));
  }
}

// main

const CredentialsFile = Bun.file(CredentialsPath.path);
if ((await CredentialsFile.exists()) === false) {
  throw `Missing credentials file at ${CredentialsPath.path}. It must be downloaded from the Google Cloud Account of the creator of this OAuth2 App.`;
}
const credentials = await CredentialsFile.json();
const oauth2Client = await CreateOAuthClient(credentials);

try {
  try {
    await GetRefreshToken(oauth2Client);
  } catch (error) {
    console.error('Error retrieving refresh token:', error);
    console.log();
    await GetAccessToken(oauth2Client);
  }
} catch (error) {
  console.error('Error retrieving access token:', error);
}
console.log('\nPlease close this window.');

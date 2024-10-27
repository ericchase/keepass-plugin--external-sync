import type { OAuth2Client, Credentials } from 'google-auth-library';
import { google } from 'googleapis';

export async function CreateOAuthClient(credentials: any) {
  const { client_id, client_secret, redirect_uris } = credentials.web;
  return new google.auth.OAuth2(client_id, client_secret, redirect_uris[0]);
}

export function FetchAccessToken(oauth2Client: OAuth2Client, code: string) {
  return new Promise<Credentials>((resolve, reject) => {
    oauth2Client.getToken(code, (err, token, res) => {
      if (err) {
        reject(err);
      } else if (token) {
        resolve(token);
      } else {
        reject('Null Token');
      }
    });
  });
}

export function FetchRefreshToken(oauth2Client: OAuth2Client, code: string) {
  return new Promise<Credentials>((resolve, reject) => {
    oauth2Client.getToken(code, (err, token, res) => {
      if (err) {
        reject(err);
      } else if (token) {
        resolve(token);
      } else {
        reject('Null Token');
      }
    });
  });
}

export function GenerateOfflineAuthUrl(oauth2Client: OAuth2Client, scope: string[]) {
  return oauth2Client.generateAuthUrl({ access_type: 'offline', prompt: 'consent', scope });
}

export function GenerateOnlineAuthUrl(oauth2Client: OAuth2Client, scope: string[]) {
  return oauth2Client.generateAuthUrl({ access_type: 'online', scope });
}

export function RefreshAccessToken(oauth2Client: OAuth2Client, refreshCredentials: Credentials) {
  // the Refresh Token credentials should contain a `refresh_token` property,
  // which is required for calling the refresh api
  if (refreshCredentials.refresh_token) {
    oauth2Client.setCredentials(refreshCredentials);
    return new Promise<Credentials>((resolve, reject) => {
      oauth2Client.refreshAccessToken((err, credentials, res) => {
        if (err) {
          reject(err);
        } else if (credentials) {
          resolve(credentials);
        } else {
          reject('Null Credentials');
        }
      });
    });
  }
  return Promise.reject('Please provide Refresh Token credentials.');
}

export async function VerifyAccessToken(token: string): Promise<{ error?: string; error_description?: string; valid: boolean }> {
  const response = await fetch(`https://oauth2.googleapis.com/tokeninfo?access_token=${token}`);
  const { error, error_description } = (await response.json()) ?? {
    error: 'connection_error',
    error_description: 'Could not connect to google api.',
  };
  if (error || error_description) {
    return { error, error_description, valid: false };
  }
  return { valid: true };
}

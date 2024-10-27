import { Path } from 'lib/ericchase/Platform/Node/Path.js';

export const BunPath = new Path(Bun.argv[0]);
export const ScriptPath = new Path(Bun.argv[1]);
export const SetupScriptPath = new Path('./setup.ts');

export const LogsPath = ScriptPath.newBase('./logs.txt');

export const CredentialsPath = ScriptPath.newBase('./secrets/credentials.json');
export const ResourcePath = ScriptPath.newBase('./secrets/resource');
export const ResourceIDPath = ScriptPath.newBase('./secrets/resource.id');
export const TokenPath = ScriptPath.newBase('./secrets/token.json');

export const OAuthScope = ['https://www.googleapis.com/auth/drive.file'];

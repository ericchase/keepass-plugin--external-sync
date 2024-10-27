# About

This is an extremely involved process of setting up basic synchronization of a database file. Such synchronization is a basic feature expected of password managers, but for lots of reasons, KeePass does not have such a feature. This is my personal attempt at conjuring some code to sync between my pc and my Google Drive. These projects are very muchly hacked together. Setting up a Google Cloud app that can be used with the client can be considered a 3rd project. Details of the Google Cloud app are not provided here. Please consult your local AI for information on how to do that, or send me a message.

https://github.com/PhilippC/keepass2android/issues/1998#issuecomment-2429501151

P.S. The client requires Bun (https://bun.sh/)

## FAQ

Q. Does this actually work?  
A. Yes, most of the time.

Q. Will this continue working for future KeePass versions?  
A. No idea. Probably not.

Q. What version of KeePass does this work with right now?  
A. It is working with `KeePass` version `2.57.1` and `KeePassLibC` version `1.42 - 0x01D0`.

Q. What version of Visual Studio are you using?  
A. Visual Studio 2022 | .NET Framework 4.6

Q. Can the database automatically sync when remote file is changed?  
A. No. This requires a Push Notification server on top of everything else so far. I _have_ started working on such a server. You can see that code commented out in the client. All that was left was to start a watch using the Google Drive apis. However, I changed my mind about automatic syncing. There's just too many potential problems that could occur, so I feel safer clicking the `Trigger Ex Sync` to manually do the syncing process.

## Safety Step

You should probably setup a trigger for backing up your database whenever it saves. The plugin will save the database before starting the syncing process, expressely for this purpose.

> ![Backup](<Picture Guide/Backup.png>)

Believe it or not, I also wrote a script for backing up my database file for me.

> ![BackupScript](<Picture Guide/BackupScript.png>)

It's a simple script, so I'll include it here.

```ts
// database_backup.ts
const dateTimeString = new Date()
  .toLocaleString('en-CA', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hour12: false,
  })
  .replaceAll(',', '')
  .replaceAll(':', '.');

await Bun.write(`[${dateTimeString}] Root.old.kdbx`, Bun.file('Root.kdbx'));
```

You can use any method you want for making backups of your database file. I highly suggest you do so.

# ExSync Plugin

- copy `ExSyncPlugin.dll` into the `Plugins` folder of your `KeePass 2` installation

> ![01](<Picture Guide/01.png>)

---

- run KeePass 2

- open the Tools > Options window

> ![02](<Picture Guide/02.png>)

---

- under the `Ex Sync` options tab, add the required values
- to use this specific project:

  - set `Executable` to `bun`
  - set `Args` to `main.ts`
  - set `Working Directory` to this project's root directory
  - click the `Restart` button and `OK`

> ![03](<Picture Guide/03.png>)

# ExSync Client

- create a google account, signin to google cloud, setup a google cloud project, setup an oauth client for that project, create credentials, download the credentials and save to `secrets/credentials.json`
- run `main.ts` with the KeePass plugin (above instructions)
- when prompted, open the URL shown in terminal window, authenticate with your google drive account, then paste the code and press enter

> ![04](<Picture Guide/04.png>)

---

> ![05](<Picture Guide/05.png>)

---

> ![06](<Picture Guide/06.png>)

---

> ![07](<Picture Guide/07.png>)

---

> ![08](<Picture Guide/08.png>)

- restart the client if needed

> ![09](<Picture Guide/09.png>)

- trigger a sync by clicking the strip menu button

> ![10](<Picture Guide/10.png>)

- there's a popup that gives some status updates

> ![11](<Picture Guide/11.png>)

- if the popup never finishes, then there was probably an error somewhere
  - close the popup, check the logs under options and try again

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

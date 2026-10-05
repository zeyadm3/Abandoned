# THE LAST PERSON — Setup Guide
### Everything to download, in order (macOS, Apple Silicon)

Do these steps in order. Each one depends on the one before it. Total time: about 1–2 hours, mostly waiting for downloads.

---

## The Stack (decided)

| Part | Choice | Cost |
|---|---|---|
| Engine | Unity 6.3 LTS + URP | Free (Unity Personal) |
| Language | C# | Free |
| Multiplayer | Netcode for GameObjects (NGO) | Free |
| Local testing transport | Unity Transport (UTP) + Multiplayer Play Mode | Free |
| Steam transport | Facepunch Transport for NGO | Free |
| Steam API | Facepunch.Steamworks | Free |
| Proximity voice | Dissonance Voice Chat + Dissonance for NGO | Paid asset (check current price) |
| Code editor | VS Code + Unity extension | Free |
| Version control | Git + Git LFS + GitHub (private repo) | Free |
| AI coding | Claude Code | Needs Claude Pro or Max |
| Steam release | Steam Direct App ID | $100 (later, not now) |

**Why this stack:** no servers to rent or pay for (Steam connects players and the host's game acts as the server), it's the same combination Lethal Company shipped with, and it's all well documented so Claude Code knows it well.

---

## Step 1 — Free up disk space

You need roughly **60–80 GB free**: Unity editor (~15 GB with modules), Xcode command line tools, project files and the Library cache.

---

## Step 2 — Homebrew (Mac package manager)

Open **Terminal** (Cmd+Space → "Terminal") and paste:

```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

It will ask for your Mac password and may install the Xcode Command Line Tools automatically. When it finishes, follow the "Next steps" lines it prints (they add `brew` to your PATH), then check:

```bash
brew --version
```

---

## Step 3 — Git and Git LFS

Git tracks every change so you can undo anything Claude Code (or you) breaks. Git LFS handles big files like models, textures and audio.

```bash
brew install git git-lfs
git lfs install
git config --global user.name "Your Name"
git config --global user.email "you@example.com"
```

Then:
1. Create a free account at **https://github.com** if you don't have one.
2. Install the GitHub CLI and log in (makes pushing easy):

```bash
brew install gh
gh auth login
```

Choose GitHub.com → HTTPS → log in with browser.

---

## Step 4 — Unity Hub

1. Download **Unity Hub** from **https://unity.com/download**
2. Open it, sign in or create a Unity account.
3. Activate a **Unity Personal** license when it asks (free while you earn under Unity's revenue threshold).

---

## Step 5 — Unity Editor 6.3 LTS

In Unity Hub → **Installs** → **Install Editor** → pick the newest **Unity 6.3 (LTS)** build.

Tick these modules:

- ✅ **Windows Build Support (Mono)** — lets you build Windows .exe files from your Mac for friend playtests
- ✅ **Mac Build Support (IL2CPP)** — usually included by default on Mac
- ✅ **Documentation** (optional)
- ❌ Skip Android, iOS, WebGL, Visual Studio for Mac (discontinued; we use VS Code)

> **Why 6.3 LTS and not the newest 6.x?** LTS gets two years of bug fixes (6.3 is supported into late 2027). Changing engine versions mid-project is painful. Stay on 6.3 LTS until launch unless a specific bug forces an update.

> **Windows build note:** From a Mac you can only build Windows versions with the **Mono** scripting backend. That's fine for prototypes and friend playtests. For the real Steam release you'll want a Windows PC (borrowed, cheap, or a cloud build service) to make IL2CPP builds, which run faster and are harder to cheat.

---

## Step 6 — VS Code

1. Download from **https://code.visualstudio.com** and drag it into Applications.
2. Install the .NET SDK (VS Code's C# tools need it):

```bash
brew install --cask dotnet-sdk
```

3. Open VS Code → Extensions (Cmd+Shift+X) → install **"Unity"** by Microsoft. It pulls in C# Dev Kit and C# automatically.
4. Later, in Unity: **Unity → Settings → External Tools → External Script Editor → Visual Studio Code**.

---

## Step 7 — Claude Code

You need a **Claude Pro or Max** subscription (the free plan doesn't include Claude Code). For a project this size, Max is worth it because you'll hit Pro's usage limits quickly during long build sessions.

Install (native installer, auto-updates):

```bash
curl -fsSL https://claude.ai/install.sh | bash
```

Open a **new** Terminal window and check:

```bash
claude --version
claude doctor
```

The first time you run `claude` it opens a browser to log in.

Prefer not to use Terminal? The **Claude desktop app** also runs Claude Code. Either works; see the Playbook for how to use it.

---

## Step 8 — Create the Unity project

1. Unity Hub → **Projects** → **New project**
2. Editor version: **6.3 LTS**
3. Template: **Universal 3D** (this is URP)
4. Project name: `TheLastPerson`
5. Location: somewhere simple like `~/Projects/`
6. **Uncheck** "Connect to Unity Cloud" / Version Control for now (we use GitHub)
7. Create. First open takes a few minutes.

---

## Step 9 — Put the project in Git (do this before any code)

In Terminal:

```bash
cd ~/Projects/TheLastPerson
```

Create a Unity `.gitignore` (stops Git from tracking huge generated folders):

```bash
curl -o .gitignore https://raw.githubusercontent.com/github/gitignore/main/Unity.gitignore
```

Set up LFS for big asset types and push to a private GitHub repo:

```bash
git init
git lfs track "*.psd" "*.png" "*.jpg" "*.tga" "*.fbx" "*.blend" "*.wav" "*.mp3" "*.ogg" "*.unitypackage"
git add .gitattributes .gitignore
git add .
git commit -m "Initial Unity 6.3 URP project"
gh repo create TheLastPerson --private --source=. --push
```

In Unity: **Edit → Project Settings → Editor**:
- **Version Control Mode:** Visible Meta Files
- **Asset Serialization Mode:** Force Text

(These make Unity files readable text, which also helps Claude Code understand scenes and prefabs.)

Commit again:

```bash
git add . && git commit -m "Editor settings for version control" && git push
```

---

## Step 10 — Install Unity packages

In Unity: **Window → Package Manager**.

### From the Unity Registry (search by name → Install)

| Package | Why |
|---|---|
| **Netcode for GameObjects** | The multiplayer framework |
| **Unity Transport** | Local/LAN networking for testing (installs with NGO) |
| **Multiplayer Play Mode** | Run up to 4 players inside the editor on one Mac |
| **Input System** | Modern keyboard/mouse/controller input (may already be installed) |
| **Cinemachine** | Camera effects (head bob, shake) |
| **ProBuilder** | Build greybox levels directly in Unity |
| **Multiplayer Tools** | Network profiler and debug views |

### From a Git URL (Package Manager → **+** → **Install package from git URL…**)

**Facepunch Transport (Steam connections for NGO):**

```
https://github.com/Unity-Technologies/multiplayer-community-contributions.git?path=/Transports/com.community.netcode.transport.facepunch
```

> If Unity reports missing `Steamworks` types after installing, download the latest **Facepunch.Steamworks** release from https://github.com/Facepunch/Facepunch.Steamworks/releases and copy its Unity folder into `Assets/Plugins/`. Check the transport's README for the currently required version.

### From the Asset Store (buy/claim on the website → Package Manager → **My Assets** → Download → Import)

- **Dissonance Voice Chat** (paid) — proximity voice, occlusion, push-to-talk, radio channels
- **Dissonance for Netcode for GameObjects** (free integration) — https://assetstore.unity.com/packages/tools/integration/dissonance-for-netcode-for-gameobjects-206514

> **Buy Dissonance when you reach Milestone 3 (voice), not today.** You don't need it to start.

Commit after installing packages:

```bash
git add . && git commit -m "Add networking, input and tooling packages" && git push
```

---

## Step 11 — Steam test setup (free)

You do **not** need to pay Valve yet.

- During development, use **Spacewar (App ID 480)**, Valve's free public test app. Facepunch.Steamworks initializes with `SteamClient.Init(480)`.
- Have **Steam** installed and logged in on your Mac (https://store.steampowered.com/about/) — Steam must be running for Steam lobbies to work.
- Your friends also need Steam running. Invites will show up as "Spacewar" — that's normal.

**One Steam account per machine.** You can't run two Steam clients on one Mac, so for day-to-day testing you'll use **Unity Transport + Multiplayer Play Mode** locally, and switch to the Steam transport only when testing with friends. The Playbook explains how Claude Code should build that switch.

---

## Step 12 — Optional but useful

| Tool | Why | Link |
|---|---|---|
| **Blender** | Make or tweak low-poly models | https://www.blender.org |
| **Audacity** | Edit sound effects | https://www.audacityteam.org |
| **OBS Studio** | Record playtests (you'll want clips) | https://obsproject.com |
| **Discord** | Coordinate playtests | https://discord.com |
| Free low-poly assets | Kenney (kenney.nl), Quaternius (quaternius.com) — free, commercial use allowed | — |
| **Steamworks Partner account** | Only when ready for a store page: $100 Steam Direct fee per game | https://partner.steamgames.com |

---

## Step 13 — Final check

You're ready when all of these are true:

- [ ] `git --version`, `git lfs --version`, `gh auth status` all work
- [ ] Unity 6.3 LTS opens `TheLastPerson` with no errors in the Console
- [ ] Package Manager shows Netcode for GameObjects, Unity Transport, Multiplayer Play Mode, Facepunch Transport
- [ ] VS Code opens C# files from Unity with autocomplete working
- [ ] `claude --version` prints a version and you've logged in
- [ ] Project is pushed to a **private** GitHub repo
- [ ] Steam is installed and logged in

Next: open **03_CLAUDE_CODE_PLAYBOOK.md** and start Milestone 0.

# ABANDONED — Start Here

This folder is your whole project. Everything is already set up except Unity itself.

```
~/Documents/Abandoned/
├── START_HERE.md      ← you are here
├── CLAUDE.md          ← rules Claude Code reads every session (already filled in)
├── .gitignore         ← already set up for Unity
├── .gitattributes     ← already set up for Git LFS
├── Docs/
│   ├── GDD.md         ← the game design
│   ├── PLAYBOOK.md    ← how to build it with Claude Code
│   └── SETUP_GUIDE.md ← what to install
└── Game/              ← YOU create this in step 2 (the Unity project)
```

---

## 1. Install everything

Follow **Docs/SETUP_GUIDE.md, steps 1–7** (Homebrew, Git + LFS, GitHub, Unity Hub,
Unity 6.3 LTS, VS Code, Claude Code). Skip its steps 8 and 9 — they're replaced below.

---

## 2. Create the Unity project inside this folder

In Unity Hub → **Projects → New project**:

- Editor: **6.3 LTS**
- Template: **Universal 3D**
- Project name: **Game** ← must be exactly this
- Location: **~/Documents/Abandoned** (click the folder icon and pick this folder)
- Leave Unity Cloud / Version Control unticked

Click Create. Unity makes `~/Documents/Abandoned/Game/`.

When it opens: **Edit → Project Settings → Editor**
- Version Control Mode: **Visible Meta Files**
- Asset Serialization Mode: **Force Text**

---

## 3. Put it on GitHub

Open Terminal and paste:

```bash
cd ~/Documents/Abandoned
git init
git lfs install
git add .
git commit -m "Initial project: docs, CLAUDE.md, Unity 6.3 URP project"
gh repo create Abandoned --private --source=. --push
```

(The `.gitignore` and LFS settings are already in this folder, so don't run the
`curl` or `git lfs track` lines from the setup guide.)

---

## 4. Install Unity packages

Follow **Docs/SETUP_GUIDE.md, step 10**, plus install **AI Navigation** from the
Unity Registry. Skip Dissonance until Milestone 4.

Then:

```bash
cd ~/Documents/Abandoned
git add . && git commit -m "Add packages" && git push
```

---

## 5. Steam

Follow **Docs/SETUP_GUIDE.md, step 11** (install Steam, log in — App ID 480 for testing).

---

## 6. Start building with Claude Code

Always start Claude Code from **this folder**, not from inside Game/:

```bash
cd ~/Documents/Abandoned
claude
```

Then open **Docs/PLAYBOOK.md**. Part 2 steps 1 and 2 are already done for you —
start at **Part 2, Step 3** and send the first message it gives you.

Good luck. Make the floor collapse.

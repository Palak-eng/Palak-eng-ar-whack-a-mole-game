# 🐾 AR Whack-a-Mole

An Augmented Reality Whack-a-Mole game built with **Unity** and **Vuforia**. Point your camera at the target image to start — moles pop out of portals on the tracked surface and you tap them to score points before the timer runs out!

---

## 🎮 Gameplay

- Point camera at the **image target** to start
- Moles pop out of portals on the tracked surface
- **Tap a mole** to score points
- Reach the **target score** before time runs out to win
- If tracking is lost mid-game, the game pauses and resumes when target is re-detected

---

## 🛠️ Built With

- [Unity](https://unity.com/) 2021.3 LTS or newer
- [Vuforia Engine](https://developer.vuforia.com/) — AR image tracking
- [TextMeshPro](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html) — UI text

---

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── ARGameActivator.cs     — Vuforia tracking events, starts/pauses game
│   ├── GameManager.cs         — Core game logic, spawning, scoring, timer
│   ├── Mole.cs                — Mole pop animation, hit detection, scoring
│   ├── Portal.cs              — Portal show/hide logic
│   ├── PortalAnimation.cs     — Portal rotation and pulse animation
│   ├── TouchHitDetector.cs    — Raycast-based tap/click detection
│   └── SpawnArea.cs           — Utility for random spawn positions
├── Prefabs/
│   ├── Moles/                 — Mole prefabs (good and bad types)
│   └── Portal/                — Portal prefab with visual and spawn point
└── Scenes/
    └── MainScene.unity        — Main game scene
```

---

## ⚙️ Setup Instructions

### 1. Clone the repository
```bash
git clone https://github.com/YOUR_USERNAME/ar-whack-a-mole.git
cd ar-whack-a-mole
```

### 2. Open in Unity
- Open **Unity Hub** → Add → select the cloned folder
- Open with **Unity 2021.3 LTS** or newer

### 3. Install required packages
- **Vuforia Engine** — download from [developer.vuforia.com](https://developer.vuforia.com/) and import
- **TextMeshPro** — Window → Package Manager → TextMeshPro → Install → Import TMP Essentials

### 4. Add your Vuforia license key
- Create a free account at [developer.vuforia.com](https://developer.vuforia.com/)
- Create a License Key and copy it
- In Unity: Window → Vuforia Configuration → paste your key

### 5. Add your image target
- Upload your target image at developer.vuforia.com → Target Manager
- Download the Unity package and import it
- On the **ImageTarget** GameObject → Observer Behaviour → assign your database

### 6. Build and Run
- File → Build Settings → switch to Android or iOS
- Enable Camera permission in Player Settings
- Click Build and Run

---

## 🎯 Key Inspector Settings

| Script | Field | Value |
|---|---|---|
| GameManager | Spawn Delay | 1.5 |
| GameManager | Game Duration | 45 |
| GameManager | Target Score | 30 |
| Mole | Pop Height | 0.08 |
| Mole | Move Speed | 3 |
| Mole | Visible Time | 2.0 |
| Mole | Pop Direction | X=0, Y=1, Z=0 |

---

## 📄 License

This project is for educational and personal use.
Vuforia is subject to its own license at [developer.vuforia.com](https://developer.vuforia.com/).

# Main Menu Setup Guide

## Scripts Created
1. **MainMenuController.cs** - Handles main menu UI interactions
2. **SceneLoader.cs** - Singleton for scene management with optional loading screen
3. **PauseMenuController.cs** - In-game pause menu (bonus!)

## How to Set Up in Unity

### Create Main Menu Scene
1. Create a new scene: `File > New Scene`
2. Save it as `MainMenu` in `Assets/Scenes/`
3. Add to Build Settings: `File > Build Settings > Add Open Scenes`

### Main Menu UI Setup

#### 1. Create Canvas
- Right-click in Hierarchy: `UI > Canvas`
- Set Canvas Scaler to "Scale with Screen Size" (recommended: 1920x1080)

#### 2. Create Main Panel
- Right-click Canvas: `UI > Panel`
- Rename to "MainPanel"
- Add child objects:
  - **Title** - TextMeshPro or Text component
  - **Start Button** - Button with "Start Game" text
  - **Credits Button** - Button with "Credits" text
  - **Quit Button** - Button with "Quit" text

#### 3. Create Credits Panel (Optional)
- Right-click Canvas: `UI > Panel`
- Rename to "CreditsPanel"
- Add child objects:
  - **Credits Text** - Your team credits
  - **Back Button** - Button with "Back" text

#### 4. Add MainMenuController Component
- Create an empty GameObject in the scene: `GameObject > Create Empty`
- Rename it to "MainMenuController"
- Add the `MainMenuController` component
- Assign references:
  - Main Panel → MainPanel object
  - Credits Panel → CreditsPanel object
  - Start Button → Start button reference
  - Credits Button → Credits button reference
  - Quit Button → Quit button reference
  - Back Button → Back button reference
  - Game Scene Name → "DevScene" (or your game scene name)

#### 5. Add SceneLoader (Singleton)
- Create another empty GameObject: `GameObject > Create Empty`
- Rename it to "SceneLoader"
- Add the `SceneLoader` component
- This will persist across scenes (DontDestroyOnLoad)

### Optional: Loading Screen
1. Create a Panel for loading screen
2. Assign it to SceneLoader's "Loading Screen" field
3. Check "Use Loading Screen" checkbox

### Pause Menu Setup (In Game Scene)

#### 1. In DevScene (or your game scene)
- Add a Canvas if not present
- Create a Panel for pause menu (usually centered, dark background)
- Add buttons:
  - Resume
  - Restart
  - Main Menu

#### 2. Add PauseMenuController
- Create empty GameObject: "PauseMenuController"
- Add `PauseMenuController` component
- Assign:
  - Pause Menu Panel
  - All buttons
  - Main Menu Scene Name → "MainMenu"

#### 3. Test
- Press ESC to pause/unpause
- Buttons should work properly

## Build Settings Checklist
Make sure both scenes are in Build Settings in the correct order:
1. MainMenu (Index 0)
2. DevScene (Index 1)

## Notes
- SceneLoader is a singleton and persists across scenes
- Pause menu uses `Time.timeScale = 0` for pausing
- Quit button works in builds, stops play in Editor
- ESC key toggles pause menu in game

## Customization Tips
- Add fade transitions in SceneLoader
- Add sound effects on button clicks
- Add settings panel with volume controls
- Add background music manager
- Style with UI sprites and animations


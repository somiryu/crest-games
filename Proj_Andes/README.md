# Proj_Andes - Unity Project

This is the Unity project for **CREST / MiniSEL**, developed by **HiHatGames**.

## Getting Started & Requirements

### Unity Version
- **Unity Version:** `2022.3.13f1`
- **Platform Compatibility:** Android / iOS (configured packages suggest mobile targeting).

### Installation Instructions
1. **Download Unity Hub**: Install the latest version of Unity Hub if you haven't already.
2. **Search the Unity Archive**:
   - Go to the Unity download archive.
   - Search for version `2022.3.13f1` (LTS).
   - Install it via Unity Hub.
3. **Storage Requirements**: Note that the installation with necessary build modules will take approximately **20 GB** of disk space.

---

## Project Analysis & Structure

Based on an inspection of the project directory and configurations, here is an analysis of the game's components and architecture.

### Build Configurations & Packages
According to [AppBuildingInfo.txt](file:///Users/freetoplay/Dev/projects/crest/crest-games/Proj_Andes/Assets/Proj_Andes/AppBuildingInfo.txt), this codebase builds two main configurations/apps:
1. **miniSEL - Minijuegos** (`com.HiHatGames.CREST`): Focused on interactive gameplay, cognitive challenges, and emotional regulation.
2. **miniSel - Historias** (`com.HiHatGames.MiniSelNarratives`): Focused on storytelling and narratives.

### Key Modules & Asset Structure
The main game assets are located in [`Assets/Proj_Andes/`](file:///Users/freetoplay/Dev/projects/crest/crest-games/Proj_Andes/Assets/Proj_Andes/):

1. **[`MiniGames/`](file:///Users/freetoplay/Dev/projects/crest/crest-games/Proj_Andes/Assets/Proj_Andes/MiniGames/)**:
   Contains various educational or psychological minigames geared towards **Social-Emotional Learning (SEL)**:
   - **Impulse Control (Control de Impulsos)**:
     - `CntrlImpulsos_EstrellasYCorazones` (Stars & Hearts test)
     - `CntrlImpulsos_FightTheAlien`
     - `CntrlImpulsos_VoiceStarOrFlower`
   - **Frustration Management**:
     - `Frustration_BoostersAndScape`
     - `Frustration_MechanicHand`
   - **Gratification Delay**:
     - `Gratification_SizeRockets`
     - `Gratification_TurboRocket`
   - **Other**: `Buggy_Magnets`, `Game1`.

2. **[`Scripts/`](file:///Users/freetoplay/Dev/projects/crest/crest-games/Proj_Andes/Assets/Proj_Andes/Scripts/)**:
   Houses core gameplay and utility scripts:
   - `AudioManager.cs`: Global audio manager.
   - `TimeManager.cs`: Time limits and countdowns.
   - `Spawner.cs` & `Pool.cs`: Object pooling for game object spawn optimization.
   - `Utility.cs` & `SceneManagement.cs`: Scene loading and helper functions.

3. **Other Subsystems**:
   - `MostersMarket/` & `SkinSystem/`: Likely a reward shop or customization system where players spend earned points.
   - `UserData/`: Local/online persistence of player profiles and progress.
   - `NarrativeScenes/` & `Menu/`: Main menu UI and visual novel/narrative sequences.
   - `FireBaseTesting/`: Integration tests or components for Firebase analytics/database.

### Dependencies
From [`manifest.json`](file:///Users/freetoplay/Dev/projects/crest/crest-games/Proj_Andes/Packages/manifest.json):
- **UI Particle Effects**: Integrates `com.coffee.ui-particle` (ParticleEffectForUGUI) for high-performance rendering of particles inside canvas layouts.
- **Unity 2D Feature Pack**: Out-of-the-box support for 2D sprites, physics, and tilemaps.
- **Visual Scripting & Timeline**: Leverages visual scripting nodes (`com.unity.visualscripting`) and timelines for cinematic or narrative timing.

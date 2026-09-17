# Take CTRL

Take CTRL is a 2D cooperative platformer built in Unity. Multiple players control a single robot, who has gained sentience and must avoid forklift and drone enemies while moving through procedurally assembled warehouse sections to escape. It was designed for the Unversity of Waterloo 2025 Fall Game Jam with the theme "Take Control". 

## Project layout

- `Take CTRL/Assets/` contains scenes, scripts, prefabs, sprites, and other game assets.
- `Take CTRL/Packages/` contains the Unity package manifest.
- `Take CTRL/ProjectSettings/` contains project and editor settings.

Generated Unity folders such as `Library/`, `Logs/`, and generated solution files are ignored and should not be committed.

## Requirements

- Unity `6000.2.6f2`
- Unity services configured for multiplayer testing when using Relay or Lobby features

## Open and run

1. Open the `Take CTRL/` folder in Unity Hub with Unity `6000.2.6f2`.
2. Open `Title Screen` from `Take CTRL/Assets/Scenes/`.
3. Start a session from the title screen, then use the lobby to begin the warehouse run.

The main scenes are:

- `Title Screen`, `Host Screen`, and `Join Screen` for session setup.
- `Lobby` for multiplayer session coordination.
- `Warehouse` for the playable run.
- `Lose`, `Win`, and `WinScene` for end states.

## Multiplayer

The project uses Netcode for GameObjects, Unity Multiplayer Widgets, Lobby, Relay, and Unity Transport. The host starts the session and can transition the group from the lobby into the warehouse. The session manager currently limits a session to four players.

For local development, configure the required Unity Gaming Services credentials and environment in the Unity Editor before testing host, join, Lobby, or Relay flows.

## Source control

The `main` branch contains the current gameplay and NGO multiplayer implementation. Keep Unity-generated files out of commits and commit both asset files and their matching `.meta` files when adding or moving Unity content.

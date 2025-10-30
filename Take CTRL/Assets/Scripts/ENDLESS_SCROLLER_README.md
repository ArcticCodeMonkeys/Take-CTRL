# Endless Background Scroller - Setup Instructions

## Overview
The `EndlessBackgroundScroller` script creates an endless side-scrolling effect for the Warehouse scene by:
1. Automatically spawning "Background+floor" prefab chunks as the DroneSwarm moves forward
2. Cleaning up old chunks that fall too far behind the DroneSwarm

## Setup Instructions

### 1. Create the Manager GameObject
In the Warehouse scene:
1. Create an empty GameObject (Right-click in Hierarchy → Create Empty)
2. Rename it to "BackgroundScrollManager"
3. Add the `EndlessBackgroundScroller` component to it

### 2. Configure the Script
Select the "BackgroundScrollManager" object and configure these settings in the Inspector:

#### References
- **Spawn Point**: Drag the "SpawnPoint" object from the scene hierarchy
- **Drone Swarm**: Drag the "DroneSwarm" object from the scene hierarchy  
- **Background Floor Prefab**: Drag the "Background+floor" prefab from `Assets/Prefabs/`

#### Spawn Settings
- **Chunk Width**: 20 (adjust based on your prefab's actual width)
  - To measure: Select the Background+floor prefab, check its width in the Sprite Renderer or BoxCollider2D
- **Initial Chunks**: 5 (number of chunks to spawn at scene start)
- **Spawn Distance**: 50 (spawn new chunks when DroneSwarm gets within this distance)

#### Cleanup Settings
- **Delete Distance**: 100 (delete chunks when they're this far behind DroneSwarm)

### 3. Auto-Detection Features
The script will automatically try to find:
- **DroneSwarm**: Searches for GameObject named "DroneSwarm"
- **SpawnPoint**: Searches for GameObject named "SpawnPoint"

If these objects are found automatically, you don't need to manually assign them.

### 4. Testing in Unity Editor

#### Visual Debugging
When you select the "BackgroundScrollManager" in the scene:
- **Green line/sphere**: Shows where new chunks will spawn (ahead of DroneSwarm)
- **Red line/sphere**: Shows where chunks get deleted (behind DroneSwarm)
- **Yellow sphere**: Shows the current DroneSwarm position

#### Context Menu Commands
Right-click the `EndlessBackgroundScroller` component in the Inspector to access:
- **Spawn Additional Chunk**: Manually spawn one more chunk
- **Clear All Chunks**: Remove all spawned chunks and reset

#### Testing Steps
1. Enter Play Mode
2. Watch the Console for spawn/delete messages
3. Observe chunks spawning ahead and being cleaned up behind
4. Check that the DroneSwarm moves smoothly with continuous background

### 5. Adjusting Parameters

#### If chunks appear too early/late:
- Increase/decrease **Spawn Distance**

#### If chunks aren't being deleted:
- Check that **Delete Distance** > chunk width
- Verify DroneSwarm is moving forward (check SwarmController.moveSpeed)

#### If chunks have gaps or overlap:
- Adjust **Chunk Width** to match your prefab's actual width
- Use Unity's Grid Snap while positioning the first chunk

#### If performance is an issue:
- Reduce **Initial Chunks**
- Increase **Delete Distance** (keeps fewer active chunks)

### 6. Troubleshooting

**Problem**: "DroneSwarm not found" error
- **Solution**: Make sure there's a GameObject named exactly "DroneSwarm" in the scene, or manually assign it in the Inspector

**Problem**: "SpawnPoint not found" error  
- **Solution**: Make sure there's a GameObject named exactly "SpawnPoint" in the scene, or manually assign it in the Inspector

**Problem**: Chunks are spawning at wrong Y position
- **Solution**: Adjust the SpawnPoint's Y position in the scene

**Problem**: Chunks aren't being deleted
- **Solution**: Verify the DroneSwarm has a Transform component and is actually moving (check SwarmController script)

**Problem**: Too many/few chunks visible
- **Solution**: Adjust the ratio between **Spawn Distance** and **Delete Distance**

### 7. Integration Notes

The script is designed to work alongside your existing systems:
- **LevelBuild.cs**: Handles chunk-based level obstacles (remains unchanged)
- **SwarmController.cs**: Moves the DroneSwarm (used as reference point)
- **SharedRobotController.cs**: Controls the robot (unaffected)

The endless background scrolling is independent and won't interfere with these systems.

### 8. Performance Optimization

The script uses several optimizations:
- Only spawns chunks when needed (not every frame)
- Efficiently tracks and removes old chunks
- Parents chunks to the manager for clean hierarchy
- Removes null entries from tracking list automatically

Expected performance:
- 5-10 active chunks at any time
- Minimal CPU usage (only checking distances each frame)

### 9. Customization Ideas

You can extend this script to:
- Spawn different background variants for visual variety
- Add parallax scrolling by creating multiple layers at different speeds
- Trigger events when certain chunk counts are reached
- Sync across network for multiplayer (using Netcode for GameObjects)

## Quick Start Checklist

- [ ] Add `EndlessBackgroundScroller` component to a manager GameObject
- [ ] Assign SpawnPoint reference (or let it auto-find)
- [ ] Assign DroneSwarm reference (or let it auto-find)
- [ ] Assign Background+floor prefab
- [ ] Adjust Chunk Width to match your prefab
- [ ] Enter Play Mode and verify chunks spawn/delete correctly
- [ ] Fine-tune spawn and delete distances
- [ ] Test with actual gameplay

## Script Location
`Assets/Scripts/EndlessBackgroundScroller.cs`

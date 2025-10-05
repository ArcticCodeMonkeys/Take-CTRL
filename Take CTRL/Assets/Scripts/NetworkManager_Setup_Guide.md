# NetworkManager Configuration Guide for Take CTRL

## Step 2: Set Up NetworkManager in Each Scene

### Scenes that need NetworkManager:
1. **Host Screen** - Where players create sessions
2. **Join Screen** - Where players join by code  
3. **Lobby** - Where players wait before game starts
4. **Warehouse** - The actual game scene

### NetworkManager Configuration for ALL scenes:

#### 1. Create NetworkManager GameObject (if not exists):
- Create empty GameObject named "NetworkManager"
- Add Component: `NetworkManager` (Unity.Netcode)
- Add Component: `UnityTransport` (Unity.Netcode.Transports.UTP)

#### 2. NetworkManager Settings:
```
✅ Enable Scene Management: TRUE
✅ Enable Connection Approval: TRUE (CRITICAL for 4-player limit)

Connection Approval Settings:
- Timeout: 60 seconds
- Default Connection Payload Size: 1024

Player Prefab: 
- Leave EMPTY (we're using shared robot, not individual players)

Network Prefabs:
- Add your robot prefab from DefaultNetworkPrefabs.asset
```

#### 3. UnityTransport Settings:
```
Protocol Type: Unity Transport
Connection Data:
- Address: 127.0.0.1 (for local testing)
- Port: 7777
- Server Listen Address: 0.0.0.0
```

#### 4. Add Connection Approval Script:
Attach the SessionManager script (we'll create this next) to handle:
- 4-player limit
- Session state management
- Automatic scene transitions

### Scene-Specific Setup:

#### Host Screen & Join Screen:
- NetworkManager should be DontDestroyOnLoad: FALSE (let each scene have its own)
- Multiplayer Widgets should reference this NetworkManager

#### Lobby & Warehouse:
- Add our RobotManager script to spawn the shared robot
- Configure robot prefab reference in RobotManager

### Next Steps:
After configuring NetworkManager, we'll:
1. Create SessionManager for connection approval
2. Set up proper scene transitions
3. Test the robot spawning

### Common Issues to Avoid:
❌ Don't have multiple NetworkManagers active at once
❌ Don't start NetworkManager manually (let Widgets do it)
❌ Don't forget Connection Approval (causes unlimited players)
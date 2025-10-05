# Step-by-Step Unity Editor Setup Guide

## 1. SessionManager Placement (SIMPLIFIED APPROACH)

### Where to put SessionManager:
- **ONLY in Lobby and Warehouse scenes** (attached to NetworkManager GameObject)
- **NOT needed in Host/Join screens** (Multiplayer Widgets handle this)

### Steps for Lobby and Warehouse scenes:
1. Open scene (Lobby or Warehouse)
2. Create NetworkManager GameObject
3. Add Component → Scripts → SessionManager
4. Configure SessionManager settings in Inspector:
   - Max Players: 4
   - Auto Start Game When Full: ✅ (for Lobby scene)
   - Game Scene Name: "Warehouse"
   - Lobby Scene Name: "Lobby"

## 2. Simplified Setup (RECOMMENDED)

### Host and Join Screens:
- **NO NetworkManager needed!**
- Multiplayer Widgets automatically handle networking
- Widgets create temporary NetworkManager for session creation/joining
- Automatically transition to Lobby scene when successful

### Lobby and Warehouse Scenes:
- **NetworkManager GameObject required**
- SessionManager component handles persistence automatically
- RobotManager component for robot spawning

## 3. Multiplayer Widgets → NetworkManager Reference

### Automatic Reference (Default):
- Multiplayer Widgets automatically find NetworkManager in same scene
- No manual setup needed if NetworkManager exists

### Manual Reference (if needed):
1. Select your Multiplayer Widget (Create Session Button, etc.)
2. In Inspector, find "Network Manager" field
3. Drag your NetworkManager GameObject into this field

### Common Widgets Setup:
**Host Screen:**
- Create Session Button → Auto-references NetworkManager
- Session Info Panel → Auto-references NetworkManager

**Join Screen:**
- Join Session Input → No special setup
- Join Session Button → Auto-references NetworkManager

## 4. RobotManager Script Placement

### Where to add RobotManager:
- **Create new GameObject** in Lobby and Warehouse scenes
- Name it "Robot Manager" or "Shared Robot Manager"

### Steps:
1. In Lobby scene:
   - Create Empty GameObject → Name: "Robot Manager"
   - Add Component → Scripts → RobotManager
   - Configure in Inspector:
     - Robot Prefab: Drag your robot prefab here
     - Spawn Point: Create empty GameObject for spawn location

2. In Warehouse scene:
   - Repeat same setup
   - Position spawn point where robot should appear in warehouse

## 5. Robot Prefab Configuration

### Robot Prefab Requirements:
Your robot prefab MUST have:
- NetworkObject component
- SharedRobotController component (your existing script)
- Rigidbody2D, Animator, etc.

### Setup Robot Prefab:
1. Find your robot prefab in Project window
2. Select it and check Inspector:
   - ✅ NetworkObject component exists
   - ✅ SharedRobotController component exists
   - Set NetworkObject → "Spawn With Observers" = TRUE
   - Set NetworkObject → "Don't Destroy With Owner" = TRUE

### Add to Network Prefabs List:
1. Open DefaultNetworkPrefabs.asset
2. Add your robot prefab to the list
3. OR in NetworkManager Inspector → Network Prefabs → Add robot prefab

## 6. Scene Flow Setup

### Recommended Scene Structure:
```
Title Screen (no NetworkManager, no networking)
    ↓
Host Screen (ONLY Multiplayer Widgets - no NetworkManager)
    ↓
Lobby (NetworkManager + SessionManager + RobotManager)
    ↓
Warehouse (NetworkManager + SessionManager + RobotManager)
```

### Scene Transition:
- Multiplayer Widgets handle Host/Join → Lobby transition
- SessionManager handles Lobby → Warehouse transition
- Use NetworkManager.SceneManager.LoadScene() for networked transitions

## 7. Quick Checklist

### Host Screen:
- [ ] Create Session Button widget
- [ ] Session Info Panel widget
- [ ] ❌ NO NetworkManager needed
- [ ] ❌ NO SessionManager needed

### Join Screen:
- [ ] Join Session Input widget
- [ ] Join Session Button widget
- [ ] ❌ NO NetworkManager needed
- [ ] ❌ NO SessionManager needed

### Lobby:
- [ ] NetworkManager GameObject (DontDestroyOnLoad = true)
- [ ] SessionManager component
- [ ] Robot Manager GameObject
- [ ] RobotManager component with robot prefab assigned

### Warehouse:
- [ ] NetworkManager GameObject (DontDestroyOnLoad = true)
- [ ] SessionManager component  
- [ ] Robot Manager GameObject
- [ ] RobotManager component with robot prefab assigned

### Robot Prefab:
- [ ] NetworkObject component
- [ ] SharedRobotController component
- [ ] Added to DefaultNetworkPrefabs.asset

Would you like me to create any additional helper scripts or clarify any of these steps?
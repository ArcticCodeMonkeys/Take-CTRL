# Simplified NGO Lobby Setup with Multiplayer Widgets

## What We're Using Instead:
NGO's **Multiplayer Widgets** package provides ready-made UI components that handle most of the lobby functionality automatically!

## Step 1: Basic NetworkManager Setup (Simplified)

### In Unity Editor:
1. Create empty GameObject → name it "NetworkManager"
2. Add these components:
   - `NetworkManager` (from NGO)
   - `UnityTransport` (from NGO)

### Configure NetworkManager:
- **Connection Approval**: ✅ Enable (IMPORTANT: This allows our script to limit players)
- **Player Prefab**: Your existing PlayerPrefab from Assets/NGO_Minimal_Setup/
- **Network Prefabs**: Your existing NetworkPrefabsList
- **Enable Scene Management**: ✅ Enable

### Configure UnityTransport:
- **Protocol Type**: **Unity Transport** (for local/direct connections)
  - Use "Relay Unity Transport" if you want to use Unity Relay service for internet play
- **Connection Data**:
  - **Address**: 127.0.0.1 (for local testing)
  - **Port**: 7777
  - **Server Listen Address**: 0.0.0.0

## Step 2: Add Multiplayer Widgets to Your Scenes

### For "Host Screen" Scene:
Add these UI widgets (found in Component > Netcode > Multiplayer Widgets):
- **Create Session Button** - Automatically creates and hosts a session
- **Session Info Panel** - Shows session details and player count
- **Leave Session Button** - Leaves the current session

### For "Join Screen" Scene:
Add these UI widgets:
- **Join Session Input** - Input field for session codes
- **Join Session Button** - Joins session by code
- **Session Browser** (optional) - Shows available sessions
- **Leave Session Button**

## Step 3: Simple Session Manager Script

We only need ONE simple script (`SimpleSessionManager.cs` - already created) to handle:
- 4-player limit detection
- Automatic scene transition when lobby is full
- Manual game start option

### Create Session Manager GameObject:
**IMPORTANT**: SimpleSessionManager cannot be on the same GameObject as NetworkManager!

1. Create a **new empty GameObject** → name it "SessionManager"
2. Add `SimpleSessionManager` component to this new GameObject
3. Set **Gameplay Scene Name** to your main game scene (probably "SampleScene")
4. Set **Max Players** to 4 (this is where you control the player limit!)
5. ✅ Enable **Don't Destroy On Load** in the script (already done automatically)

## Step 4: Create Prefabs (IMPORTANT!)

### Create NetworkManager Prefab:
1. Drag your configured **NetworkManager GameObject** from Hierarchy to Assets/Prefabs/
2. Name it "NetworkManager"
3. Delete the original from the scene (we'll add prefab instances instead)

### Create SessionManager Prefab:
1. Drag your configured **SessionManager GameObject** from Hierarchy to Assets/Prefabs/
2. Name it "SessionManager" 
3. Delete the original from the scene

## Step 5: Widget Setup in Unity Editor

**CRITICAL**: NGO Multiplayer Widgets need references to the NetworkManager!

### Host Screen Scene:
1. Add your **NetworkManager prefab** to the scene
2. Add your **SessionManager prefab** to the scene
3. Add UI Canvas if not present
4. Add these widgets from **Component > Netcode > Multiplayer Widgets**:
   - **Create Session Button** 
     - Automatically creates session and generates shareable code
     - No manual configuration needed
   - **Session Info Display**
     - Shows session code and player count once session is created
     - No manual configuration needed
   - **Leave Session Button**
     - Automatically handles session cleanup
     - No manual configuration needed

### Join Screen Scene:
1. Add your **NetworkManager prefab** to the scene
2. Add your **SessionManager prefab** to the scene
3. Add UI Canvas if not present
4. Add these widgets:
   - **Session Code Input Field** - Where players enter the session code
     - **Widget Configurations**: Leave default
     - **Join Session Events**: You can leave empty or add custom logic
     - **Joined Session**: Event triggered when successfully joined
     - **Failed to Join Session**: Event triggered when join fails
   - **Join Session Button** 
     - This automatically works with the input field above
   - **Session Info Display**
     - Shows current session status once connected
   - **Leave Session Button**
     - Automatically handles disconnection

## Step 6: How NGO Multiplayer Widgets Work

### 🔗 **Widget Auto-Discovery:**
The NGO Multiplayer Widgets automatically find the NetworkManager in the scene:
- No manual Network Manager references needed!
- They use `NetworkManager.Singleton` to find the active NetworkManager
- Just ensure there's exactly one NetworkManager in each scene

### 🎮 **Widget Event System:**
The widgets use Unity Events for customization:
- **Join Session Events**: Triggered during join process
- **Joined Session**: Triggered when successfully connected
- **Failed to Join Session**: Triggered when connection fails
- You can hook custom methods to these events if needed

### 📋 **Widget Requirements:**
- ✅ NetworkManager prefab in scene
- ✅ SessionManager prefab in scene  
- ✅ UI Canvas for the widgets
- ✅ Widgets automatically find and use NetworkManager.Singleton

## Step 7: Testing the Setup
✅ **Much simpler** - NGO widgets handle networking automatically
✅ **Session codes** - Players share codes instead of IP addresses  
✅ **Built-in UI** - No need to create custom lobby lists
✅ **Automatic connection** - Widgets handle host/client setup
✅ **Password protection** - Built into session codes
✅ **4-player limit** - Handled by our simple script

## Testing:
1. Build or run two instances
2. Host creates session → gets a code
3. Others join with the code
4. Game starts automatically at 4 players

## Step 7: Testing the Setup

### Build Test:
1. Build your project (File > Build Settings > Build)
2. Run one instance as Host (Create Session)
3. Run another instance as Client (Join with code)
4. Verify 4-player limit and auto-start works

### Console Messages to Look For:
- "Session configured for max 4 players"
- "Connection approved. Total players: X/4"
- "Lobby full! Starting game..." (at 4 players)
- "Transitioning to gameplay scene..."

## Benefits of This Approach:
✅ **Much simpler** - NGO widgets handle networking automatically
✅ **Session codes** - Players share codes instead of IP addresses  
✅ **Built-in UI** - No need to create custom lobby lists
✅ **Automatic connection** - Widgets handle host/client setup
✅ **Password protection** - Built into session codes
✅ **4-player limit** - Handled by our simple script
✅ **Prefab system** - Consistent setup across scenes

## What's Next:
After this setup works, we can create the shared character controller for gameplay!
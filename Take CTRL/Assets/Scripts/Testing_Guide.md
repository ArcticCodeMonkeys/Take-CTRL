# Testing Your Multiplayer Lobby System

## 🚀 **Testing Options**

### Option 1: Build + Editor (Recommended for Quick Testing)
1. **Build your project** (File > Build Settings > Build)
2. **Run the built executable** (this will be Player 1)
3. **Press Play in Unity Editor** (this will be Player 2)
4. **Test with 2 players** to verify basic functionality

### Option 2: Multiple Builds (Full 4-Player Testing)
1. **Build your project** 4 times or copy the build folder
2. **Run 4 separate instances**
3. **Test full 4-player lobby experience**

### Option 3: Unity Editor Only (Development Testing)
1. **Install "ParrelSync"** (free Unity asset for multiple editor instances)
2. **Clone your project** for testing
3. **Run multiple Unity editors** simultaneously

## 📋 **Step-by-Step Testing Process**

### Step 1: Basic Connection Test
1. **Start Instance 1** (Build or Editor)
   - Go to **Host Screen**
   - Click **"Create Session"** button
   - Note the **session code** that appears

2. **Start Instance 2** (Build or Editor)
   - Go to **Join Screen**
   - Enter the **session code** from Instance 1
   - Click **"Join Session"** button

### Step 2: Verify Connection
**Check Console Messages:**
- ✅ "Session configured for max 4 players"
- ✅ "Connection approved. Total players: X/4"
- ✅ "Player joined. Total players: X"

**Check UI:**
- ✅ Session Info should show connected player count
- ✅ Both players should see session details

### Step 3: Test 4-Player Auto-Start
1. **Get 4 total players** connected to the same session
2. **Verify auto-transition** to gameplay scene (Warehouse)
3. **Check Console** for "Lobby full! Starting game..."

### Step 4: Test Shared Character Control
**In the gameplay scene:**
1. **Each player presses movement keys** (WASD or arrow keys)
2. **Verify the robot moves** based on combined input
3. **Test individual actions:**
   - Any player can jump
   - Any player can attack
   - Any player can dodge

### Step 5: Test Sprint Logic
1. **1 player holds sprint** → Robot should NOT sprint (minority)
2. **3+ players hold sprint** → Robot SHOULD sprint (majority)

## 🐛 **Common Issues & Solutions**

### Issue: "No NetworkManager found"
**Solution:** Make sure NetworkManager prefab is in both Host and Join scenes

### Issue: "Connection failed"
**Solution:** 
- Both instances on same network
- Firewall not blocking Unity
- Check port 7777 is available

### Issue: "Session code doesn't work"
**Solution:**
- Make sure host created session first
- Session codes are case-sensitive
- Try restarting both instances

### Issue: "Robot doesn't move"
**Solution:**
- Check SharedRobot has NetworkObject component
- Verify input action references are assigned
- Check robot is spawned in gameplay scene

### Issue: "Players see different things"
**Solution:**
- Make sure enemies have NetworkObject components
- Verify all prefabs are in NetworkPrefabsList
- Check server authority is working

## ✅ **Success Indicators**

### Lobby System Working:
- ✅ Session codes generate and work
- ✅ Players can join with codes
- ✅ Auto-transition at 4 players
- ✅ Session info shows correct player count

### Shared Control Working:
- ✅ All players can influence robot movement
- ✅ Movement feels responsive (25% weight per player)
- ✅ Any player can trigger jump/attack/dodge
- ✅ Sprint requires majority of players

### Networking Working:
- ✅ All players see same robot position
- ✅ Actions sync between players
- ✅ Enemies behave consistently (if networked)
- ✅ No lag or desync issues

## 🎯 **Quick Test Checklist**

1. ⬜ Build project successfully
2. ⬜ Host can create session
3. ⬜ Client can join with code
4. ⬜ Session info updates correctly
5. ⬜ Auto-start works at 4 players
6. ⬜ Shared robot spawns in gameplay
7. ⬜ All players can move robot
8. ⬜ Actions work from any player
9. ⬜ Sprint logic works (majority rule)
10. ⬜ No errors in console

**Ready to start testing? Begin with Option 1 (Build + Editor) for the quickest verification!** 🚀
# Freecam Troubleshooting Guide

## ❌ Issue: Camera not moving after pressing TAB

### Common Causes & Solutions:

---

## ✅ **Solution 1: Make Sure Game Window Has Focus**

**The most common issue!**

1. After pressing F5 to run the game
2. **Click inside the game window** with your mouse
3. Then press TAB
4. You should see "[Movement ON]" appear
5. Now try arrow keys

**Why?** The game window needs input focus. If you pressed F5 and the focus is still in the editor, inputs won't work.

---

## ✅ **Solution 2: Check Console for Messages**

When you press TAB, look at the console output. You should see:
```
[Movement ON]
```

If you don't see this message:
- The freecam script isn't running
- The camera doesn't have the freecam.gd script attached
- Input actions aren't registered

---

## ✅ **Solution 3: Verify Camera Setup**

In your scene tree, check:
1. Camera3D node exists
2. Camera3D has `freecam.gd` script attached
3. Camera3D is marked as "Current" (check the Inspector)

---

## ✅ **Solution 4: Input Event Priority Fixed**

**This has been fixed in the latest version!**

Changed `_Input()` to `_UnhandledInput()` in Test01_AddPoint.cs so it doesn't interfere with freecam's input handling.

The freecam (GDScript) now processes inputs first, then the Test01 script (C#).

---

## ✅ **Solution 5: Try Debug Mode**

Add this temporary debug code to check if inputs are working:

In your scene, select the Camera3D and check the "Overlay Text" export variable is ON.

When freecam is active, you should see:
- "Debug Camera" at the top
- Movement messages when you press keys

---

## 🎮 **Correct Usage Sequence:**

1. **Run scene** (F5)
2. **Click in game window** ← IMPORTANT!
3. **Press TAB** → See "[Movement ON]"
4. **Press Arrow Keys** → Camera should move
5. **Move Mouse** → Camera should rotate (mouse captured)
6. **Mouse Wheel** → Speed up/down
7. **Press TAB again** → "[Movement OFF]", mouse released

---

## 🐛 **Still Not Working? Try This:**

### Test in main_test.tscn instead:
1. Open `TestAssets/main_test.tscn`
2. Make sure the Camera3D there has freecam.gd attached
3. Run that scene
4. Click window → TAB → Arrow keys

If it works there but not in test01, there's a scene-specific issue.

---

## 📋 **Quick Checklist:**

- [ ] Game window has focus (clicked inside)
- [ ] Camera3D has freecam.gd script
- [ ] Camera3D is set as "Current"
- [ ] Console shows "[Movement ON]" when pressing TAB
- [ ] No error messages in console
- [ ] Arrow keys (not WASD) for movement
- [ ] Freecam is enabled with overlay_text = true

---

## 🔧 **Alternative: Verify Freecam Works Independently**

Create a minimal test:
1. New scene
2. Add Camera3D (mark as Current)
3. Attach freecam.gd to it
4. Add a CSGBox3D (something to see)
5. Run → Click window → TAB → Arrow keys

If this works, the issue is in test01_add_point.tscn setup.

---

## 💡 **Current Controls:**

Remember, controls changed to avoid conflicts:

| Key | Action |
|-----|--------|
| TAB | Toggle freecam ON/OFF |
| ↑↓←→ | Move (Arrow Keys) |
| Page Up | Fly up |
| Page Down | Fly down |
| Mouse | Look around |
| Mouse Wheel | Speed |

**NOT** using WASD/SPACE/SHIFT anymore!

---

## 🚨 **Last Resort: Check Godot Version**

Freecam3D might have compatibility issues with Godot 4.5. If nothing works:
1. Check addon version compatibility
2. Update freecam plugin if available
3. Report issue to freecam plugin developer

---

## ✅ **What Was Already Fixed:**

1. ✅ Changed freecam keys from WASD to Arrow Keys
2. ✅ Changed up/down from SPACE/SHIFT to PageUp/PageDown
3. ✅ Changed Test01 from `_Input()` to `_UnhandledInput()`
4. ✅ Added proper input event handling

These fixes ensure no conflicts between:
- River extension (E key)
- Cube spawning (SPACE)
- Freecam movement (Arrow keys)

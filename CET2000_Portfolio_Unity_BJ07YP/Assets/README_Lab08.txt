CET2000 – Lab 08 STARTER PACK: NPC Navigation with NavMesh, Waypoints and Steering (Unity 6.0 to 6.5)
=====================================================================================================

Quick start
1. Unity Hub -> Add -> Add project from disk -> this folder. Open with any Unity 6 version
   (6.0 LTS up to 6.5). If Hub asks to upgrade the project, click Continue / Upgrade.
2. Click "Create scenes" in the CET2000 dialog, or use CET2000 -> Create Lab08 Scenes.
   If asked, click "Switch to URP" (removes Unity 6.5's Built-in Render Pipeline warning).
3. Open Documentation/Lab08_Instructions.md and follow Part A to Part G.

Your scripts (Assets/Scripts)
  ChaseNPC.cs         TODO 1  the blue NPC chases the player with a NavMeshAgent
  Patrol.cs           TODO 2-3  the red guard patrols waypoints (loop and ping-pong), Expert E3
  Steering.cs         TODO 4-7  Seek, Flee, Arrive, Pursuit, Evade, Expert E1 Wander
  HybridNavigator.cs  TODO 8  NavMesh path to the player + evade the guard, Expert E2

Provided (Assets/Scripts/Provided, no changes needed)
  NavMeshBaker.cs     bakes the NavMesh at Play and draws it; change the agent radius and bake again
  NavLabHud.cs        panel for the NavMesh scene      SteeringLabHud.cs  panel for the Steering arena
  SteeringDemo.cs     runs a behaviour and draws the velocity, desired and steering arrows
  PlayerClickMover.cs, TargetMover.cs, LabDraw.cs
  Editor/SetupLab08.cs      builds both scenes        Editor/Lab08Checks.cs  CET2000 -> Check My Steering Maths
  Editor/Lab08URPSetup.cs   switches the project to URP

Scenes
  Lab08_NavMesh   storeroom with crates, an inner wall and a door; player (green), NPC (blue), guard (red)
  Lab08_Steering  open arena; NPC (blue), target (green), threat (red)

Compatibility
  Written for Unity 6.0 LTS to 6.5 (saved as 6000.0.23f1). Uses only built-in modules: the NavMesh is baked from
  code with UnityEngine.AI.NavMeshBuilder, so the AI Navigation package is not required (but can be installed).
  Mouse and keyboard are read through OnGUI events, so the lab works with both the old Input Manager and the new
  Input System.

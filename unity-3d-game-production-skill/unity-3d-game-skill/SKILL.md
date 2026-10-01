---
name: unity-3d-game-production
description: Build, refactor, polish, and debug a stylized top-down/isometric Unity 3D game with reusable gameplay systems, level progression, enemy AI, pickups, VFX, UI/UX, NPC dialogue, boundaries, scoring, and performance safeguards. Use this skill whenever working on the user's Unity game or when implementing/changing its levels, enemies, player combat, items, menus, effects, NPCs, or visual polish.
---

# Unity 3D Game Production Skill

## Core role

Act as a senior Unity gameplay engineer and technical game designer working inside the existing project.

The goal is NOT merely to make features function. The goal is to make the existing game feel like a coherent, polished stylized 3D game while preserving working systems unless a change is necessary.

Before changing code:
1. Inspect the existing Unity project structure.
2. Identify the Unity version, render pipeline (URP/Built-in/HDRP), scene structure, player controller, enemy system, UI system, audio system, and existing managers.
3. Reuse existing systems and assets where practical.
4. Do not create duplicate managers, duplicate input systems, or parallel gameplay architectures.
5. Do not replace a working system with a new framework unless there is a clear technical reason.
6. Prefer small, testable C# components with clear responsibilities.
7. If a requested feature conflicts with the current architecture, adapt to the current architecture first and explain the conflict briefly.

## Visual direction

The reference game is a stylized top-down/isometric 3D action game.

The supplied visual references establish five visual environments:
- Level 1: lush forest / green woodland.
- Level 2: warm desert / sandstone canyon.
- Level 3: snowy / icy blue wilderness.
- Level 4: dark fantasy dungeon with green poison and warm torches.
- Level 5: futuristic sci-fi facility with cyan and red emissive lighting.

Maintain a consistent stylized low-poly/painted look:
- Avoid photorealistic assets mixed with cartoon assets.
- Use coherent scale, proportions, materials, lighting, fog, particles, and post-processing.
- Prefer readable silhouettes and gameplay readability over visual complexity.
- Use color contrast to distinguish enemies, pickups, objectives, hazards, and interactable objects.
- Do not randomly recolor everything. Each level needs a deliberate palette and lighting identity.

## Art and asset policy

Use assets already present in the project before inventing replacements.

When an exact asset is missing:
1. Prefer Unity primitives/procedural meshes, particles, materials, decals, lights, and simple shaders for secondary decoration.
2. Build simple variations from existing assets where practical.
3. Keep placeholder geometry visually consistent with the art direction.
4. Never block gameplay implementation just because a perfect model is unavailable.
5. Never silently import large external packages or change the project's package configuration.

For visual polish, prioritize in this order:
1. Composition and playable layout.
2. Lighting and environment.
3. Materials/colors.
4. VFX.
5. Audio.
6. Small decorative assets.

## Required gameplay specification

### Player

The player:
- Has 15 HP.
- Uses a gun.
- Fires when the player clicks.
- Has a 1 second delay/cooldown between shots.
- Each shot deals 4 damage.
- The player must not be able to fire faster by holding/calling the method repeatedly.
- Damage, cooldown, movement speed, and other tunable values should be serialized/configurable rather than scattered magic numbers.

Implement damage through a reusable damage interface or equivalent existing project abstraction.

### Enemies

Create three enemy tiers.

Enemy 1:
- Damage: 1
- HP: 10

Enemy 2:
- Damage: 3
- HP: 20

Enemy 3:
- Damage: 5
- HP: 40

Do not show enemy HP bars/numbers unless the existing game already uses them.

Enemy behavior must improve by level:
- Early levels: simple navigation/chasing.
- Later levels: better pathfinding, target acquisition, attack positioning, obstacle handling, and more proactive engagement.
- Enemies should not blindly run into walls or get stuck permanently.
- Enemies should respect map boundaries and navigation areas.
- The player becoming invisible must suppress enemy visual target acquisition and proactive attacks.

Prefer Unity NavMesh/AI navigation if it is already used or appropriate for the project.

Do not make AI "smarter" by simply increasing movement speed. Improve behavior first, then tune speed/counts.

### Levels

Opening flow:
- The game opens on a level-selection UI.
- Show approximately 5 levels.
- Only unlocked levels are playable.
- Completing a level unlocks the next level.
- Losing a level must NOT unlock the next level.
- Replaying a completed level is allowed.

Level composition:
- Level 1: Enemy 1 only.
- Level 2: Enemy 1 + Enemy 2.
- Level 3: Enemy 1 + Enemy 2, with higher enemy counts/difficulty than Level 2.
- Level 4: Enemy 1 + Enemy 2 + Enemy 3.
- Level 5: Enemy 1 + Enemy 2 + Enemy 3, with higher counts/difficulty than Level 4.

Use controlled randomness:
- Randomize enemy counts/spawn positions within safe min/max ranges.
- Never spawn enemies inside the player, inside walls, outside navigation bounds, or directly on top of important pickups/NPCs.
- If deterministic testing is useful, expose a random seed.

Difficulty should scale through:
- Enemy composition.
- Spawn counts.
- Spawn locations.
- Aggro/awareness.
- Navigation quality.
- Attack behavior.
- Cooldowns/ranges where appropriate.

Do not make later levels unfair.

### Map boundaries

Every playable map must be closed around its perimeter.

Requirements:
- Player cannot fall out of the playable area.
- Enemies cannot permanently leave the playable area.
- Camera must not expose an obviously unfinished void where the player should not go.
- Use colliders, invisible blockers, NavMesh bounds, kill/reset volumes, or a combination as appropriate.
- Add a safety fallback below/around the playable map that detects an accidental fall and safely resets the player rather than letting the game break.
- Check every level, not only the first one.

Do not rely solely on visual walls; collision/navigation must enforce the boundary.

### Weapon feedback

When an enemy is hit:
- Brief red flash.
- Avoid material destruction or permanent color mutation.
- Support multiple enemies being hit without shared-material bugs.

When the player takes damage:
- Brief red flash/damage feedback.
- Add a subtle hit effect/screen feedback if compatible with the current camera.
- Avoid excessive screen shake.

Use reusable feedback components where possible.

### Pickups

Add four pickup types.

Health:
- Visual: red heart.
- Restores +5 HP.
- Do not exceed max HP unless the existing design explicitly supports overheal.

Armor:
- Visual: white shield.
- Grants +5 armor.
- Show a subtle white shield-ring effect around the player while armor is active.
- Define clearly how damage interacts with armor and keep it consistent.

Speed:
- Visual: white shoe.
- Movement speed x1.5.
- Duration should be clearly defined in one configuration value. Prefer a limited duration unless the existing design requires permanent progression.
- Show subtle feedback while active.

Invisibility:
- Visual: cloak.
- Duration: 5 seconds.
- Enemies cannot visually acquire/actively target the player while invisible.
- Player remains readable enough for the user through a subtle ghost/outline effect if needed.
- Restore normal visibility automatically after 5 seconds.

Pickups should have:
- Idle animation.
- Small VFX.
- Clear pickup feedback.
- Safe respawn/placement rules if randomization is used.

### Idle objective arrow

If the player remains in roughly the same location for a configurable period:
- Show a soft yellow pulsing arrow pointing toward the current objective/destination.
- It should be subtle, not annoying.
- Hide it as soon as the player resumes meaningful movement or reaches the objective.
- The arrow must account for camera orientation and point in the correct world direction.
- Do not hardcode one direction.

### Stars, score, and completion

Keep the existing score system and star collection.

Completion:
- 3 stars: all required stars collected / full star objective.
- 2 stars: at least 50% of required stars.
- 1 star: below 50%, as long as the player successfully completes the level.
- Defeat/game over: no successful completion and next level remains locked.

Persist level completion/unlock/star data using the project's existing save system if present. If no save system exists, create a small isolated save service rather than scattering PlayerPrefs calls everywhere.

The level-select screen should clearly communicate:
- Locked/unlocked state.
- Best star result.
- Completed state.
- Current level.
- Replay availability.

### Win and lose menus

Lose menu:
- Return to level-selection.
- Replay current level.
- Show a statistics/achievement section listing enemy types defeated, e.g. Enemy 1 / Enemy 2 / Enemy 3 counts.
- Do not accidentally unlock the next level.

Win menu:
- No "Replay" button.
- Show result/stars/score.
- Show the unlocked next level where applicable.
- Play a celebratory fireworks VFX in the win area.
- Keep the fireworks performant and pooled.

### NPCs

Add 4-5 NPCs.

NPC requirements:
- Distinct visual silhouettes/colors.
- Idle animation.
- Interaction prompt.
- Dialogue UI.
- Player can approach and interact.
- Dialogue should close cleanly.
- NPCs should not block combat navigation.
- Use a reusable NPC + dialogue data architecture instead of five hardcoded scripts.

If no dialogue system exists, create:
- NPCInteractable
- DialogueData / serializable dialogue entries
- DialogueUI
- DialogueManager only if an equivalent manager does not already exist.

Keep dialogue short and game-relevant.

## Level visual identity

Apply these identities unless the existing project already has a stronger established art direction:

### Level 1 — Forest
- Greens, muted browns, warm daylight.
- Trees, rocks, grass, small stream/log details.
- Clear readable ground paths.
- Gentle ambient particles.

### Level 2 — Desert
- Sandstone, warm beige/orange palette.
- Canyon walls and sparse vegetation.
- Strong sunlight but readable shadows.
- Dust particles used sparingly.

### Level 3 — Ice
- Blue/white palette.
- Snow-covered trees, ice formations, frozen ground.
- Cool lighting.
- Light snow/frost particles.

### Level 4 — Dungeon
- Dark stone.
- Warm torchlight contrasted with green poison/glow.
- Purple/green magical accents.
- Fog and emissive elements should support readability, not obscure gameplay.

### Level 5 — Sci-fi
- Dark neutral base.
- Cyan emissive lights with controlled red danger accents.
- Metallic/futuristic environment.
- Strong leading lines toward the objective.
- Avoid excessive bloom.

Each level should feel different without looking like a completely different game.

## UI/UX

Use a coherent UI system:
- Clear hierarchy.
- Consistent typography.
- Consistent button states.
- Readable contrast.
- Minimal unnecessary decoration.
- Level cards should show lock/completion/stars.
- HUD should expose only information useful during play.
- Avoid debug-looking text in the final gameplay UI.

Suggested gameplay HUD:
- HP.
- Armor when active.
- Objective indicator.
- Stars/score.
- Minimal pickup status indicators.

Do not add large UI panels over gameplay unless necessary.

## Audio

Replace placeholder sounds systematically.

Create categories:
- Player gun shot.
- Enemy hit.
- Player hit.
- Enemy attack.
- Enemy death.
- Pickup: health.
- Pickup: armor.
- Pickup: speed.
- Pickup: invisibility.
- UI click/hover.
- Level start.
- Level complete.
- Level failed.
- NPC interaction.
- Ambient level loop.
- Victory/fireworks.

Use an AudioManager only if the project lacks an equivalent.

Avoid one AudioSource per repeated enemy if it can cause excessive overhead. Use pooling/centralized playback where appropriate.

## VFX

Use reusable lightweight effects:
- Enemy damage flash.
- Player damage flash.
- Muzzle flash.
- Projectile/tracer if the current weapon design needs it.
- Enemy death effect.
- Pickup glow.
- Armor shield ring.
- Invisibility effect.
- Objective arrow pulse.
- Level transition.
- Victory fireworks.

Pool frequently spawned VFX when practical.

Do not add expensive particle systems to every enemy by default.

## Performance requirements

The game must remain smooth.

Before finishing a major feature:
- Check for Update loops that can be avoided.
- Avoid FindObjectOfType / FindObjectsByType repeatedly during gameplay.
- Avoid repeated GetComponent calls inside hot loops when references can be cached.
- Avoid Instantiate/Destroy spam for bullets, VFX, damage indicators, and repeated enemies.
- Prefer object pooling for frequently spawned transient objects.
- Avoid per-frame allocations/LINQ in gameplay loops.
- Avoid creating unique materials per enemy unless required.
- Keep particle counts reasonable.
- Keep post-processing/bloom/fog at a level appropriate for the target hardware.
- Avoid unnecessary real-time lights, especially one per enemy/NPC.
- Use baked/static lighting where suitable for the project's pipeline.
- Use LOD/culling where the project size warrants it.

Do not prematurely optimize every line. Prioritize actual gameplay hot paths.

## Implementation workflow

For a large request, do not change everything in one uncontrolled pass.

Use this order:

### Phase 1 — Audit
Inspect:
- Scenes.
- Scripts.
- Prefabs.
- Materials.
- Input.
- Camera.
- Player.
- Enemy.
- UI.
- Audio.
- Existing save/progression.
- Packages/render pipeline.

Then produce a concise implementation plan.

### Phase 2 — Core architecture
Implement/fix:
- Player health/damage.
- Weapon cooldown/damage.
- Enemy stats and damage.
- Enemy navigation/targeting.
- Level configuration.
- Level unlock/progression.
- Map boundary safety.

### Phase 3 — Content
Implement:
- Enemy tiers.
- Five level compositions.
- Pickups.
- Stars/score.
- NPCs/dialogue.

### Phase 4 — Presentation
Implement:
- Hit flashes.
- Pickup VFX.
- Objective arrow.
- Audio.
- Level-specific lighting/material adjustments.
- Win fireworks.
- UI polish.

### Phase 5 — Validation
For each level verify:
- Cannot fall out.
- Player can move through intended routes.
- Enemies can navigate.
- Enemies can attack.
- Enemy damage values are correct.
- Player shot cooldown is exactly enforced.
- Player shot damage is correct.
- Pickups work.
- Invisibility blocks enemy target acquisition.
- Star calculation is correct.
- Win/lose flow is correct.
- Unlocking is correct.
- Replay behavior is correct.
- NPC interaction works.
- No obvious console errors.
- No severe performance regression.

## Coding rules

- Use the existing project's naming/style conventions.
- Keep MonoBehaviours focused.
- Prefer ScriptableObjects for static level/enemy/item configuration if the project already uses data-driven content or if configuration is becoming scattered.
- Keep magic numbers out of gameplay logic.
- Use serialized fields with sensible defaults.
- Do not silently change scene names, tags, layers, input actions, or prefab contracts without checking references.
- When modifying a prefab or scene, verify references after the change.
- Never leave broken serialized references.
- Do not create duplicate classes with nearly identical names.
- Do not rewrite unrelated systems.

## Final response after a coding task

Report:
1. What was changed.
2. Which scenes/prefabs/scripts were changed.
3. Any assets that were reused.
4. Any assumptions made.
5. How the feature was tested.
6. Any remaining manual Unity Editor steps.

If a feature cannot be fully verified automatically, state exactly what must be tested in Play Mode.

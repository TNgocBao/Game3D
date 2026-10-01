# Vanguard reference rebuild

Entry scene: `Assets/3D.unity`. Build Settings contains that scene only. `Assets/Scenes/Start.unity` and `Assets/Scenes/SampleScene.unity` now share the Vanguard startup preview with camp artwork and rounded Vietnamese typography. The original Mario SampleScene is preserved at `Assets/Legacy/MarioSampleScene.unity`.

## Implemented

- Seven supplied JPGs are copied into `Assets/Resources/LumiReference`: five environment references plus the camp and portal artwork.
- Mission selection uses reference images, five selectable cards, unlock state, best stars, a mission briefing, and a deployment button.
- Loading and replay/next-mission transitions use the camp/portal references, preparation stages, a gameplay tip, and a short fade. The world is constructed synchronously; this is not an asynchronous scene-download percentage.
- Results use a mission artwork banner, stars, score, three enemy defeat counts, and the appropriate continuation buttons.
- Five compact procedural 3D arenas replace the long platforming course. Trees, rocks, ground cover, trails, water/ice, chamber walls, poison and cyan lighting establish the five palettes.
- Perspective camera with a 48-degree field of view, a closer view and bounded following. Hold right mouse to orbit, scroll to zoom and press Home to reset. Mouse position aims the weapon; pointing directly at an enemy targets its collider. The cursor remains unlocked and the reticle follows the mouse.
- Infantry silhouettes use helmets, vests, boots, faces and backpacks. Enemy tiers differ in size and uniform color. These are procedurally constructed characters, not the original models used to make the supplied illustrations.
- Built-in runtime NavMesh construction supports enemy path calculation. Spawn positions require clearance, a NavMesh location and a complete route from the start. Failed placements are skipped with a warning.
- Boundary collision, fall recovery, health/armor, weapon cooldown, pickups, five NPCs distributed across five levels, score, stars and unlock persistence remain integrated with the existing Lumi system.
- Reusable projectile/impact pools, property-block damage flash, visible ghost silhouette, matte materials, terrain grain, anti-aliasing and an animated portal shader improve presentation.
- Build preprocessing records referenced Kenney weapons into a Resources catalog so standalone builds retain the assets. No extra Unity packages were installed.

## Validation status

The scripts are checked with Unity's bundled C# compiler against the installed Unity 2021.3.13f1 assemblies. This verifies type/API compatibility, not rendering or gameplay behavior.

Unity batch startup reports that no valid license is available, but the running Editor is accessible through Computer Use after the user enabled Any App. Actual Editor screenshots now verify startup preview, menu typography, audio sliders and gameplay without the white Gizmo lines. The five-level Play Mode smoke check passed and wrote `Logs/VanguardPlayModeValidation.txt`. Standalone build, full playthroughs and exact visual parity remain unverified. The 3D scenes reconstruct the supplied artwork; they are not pixel-identical reproductions.

## Unity checks still required

1. Let Unity import the updated assets and open `Assets/3D.unity`.
2. Enter Play Mode. Check the mission screen at 1920×1080, 1280×720 and a narrow window. Confirm Vietnamese text, crop, button focus/hover and layout.
3. Use `Vanguard > Validate five levels in Play Mode` for the automated smoke checks. It writes `Logs/VanguardPlayModeValidation.txt`; it does not unlock levels or claim to test the entire game.
4. Play every level to check route readability, obstruction handling, aiming, 1-second cooldown/4 damage, 5-second invisibility, pickup collection, dialogue, boundaries and fall recovery.
5. Finish with 0, 2, 3 and 5 collected stars to check the 1/2/3-star thresholds. Lose and confirm the next level remains locked. Confirm replay is available from defeat/mission selection and absent from the victory report.
6. Run `Vanguard > Prepare production assets` if inspecting the catalog manually; builds run this step automatically.
7. Build a Windows player and inspect the menu artwork, weapons and portal shader outside the Editor.

A final visual QA pass and finer art work remain necessary before calling this a polished finished game matching every detail of the references.


## Follow-up verification

- Runtime and Editor C# compilation are repeated after the fixes.
- QA/GameRulesChecks.cs runs against the actual production progression/cooldown helpers. All checks passed for star thresholds, defeat, replay preservation and one-second cooldown boundaries. This is a rules test, not a Play Mode test.
- Fixed aspect-preserving artwork after window resizing, a fitted mission layout at non-16:9 ratios, fireworks configuration/state, dialogue dismissal, stale dialogue/toasts, spawn transform synchronization and repeated ambient-clip allocation.
- Camera changed to perspective following feedback that the orthographic view looked flat. Lower ambient lighting strengthens directional shading.
- Computer Use access now succeeds. Game-view Gizmos were enabled and caused the reported long white lines; switching Gizmos off visibly removed them. This is an Editor display setting, not a runtime map object.
- Added six original melodic stereo loops, generated by `QA/build_presentation_assets.py`, and separate saved master/music/effects volume controls plus mute/default buttons. Actual slider dragging, reset and opening settings during gameplay were verified in the Editor. Speaker output has not been independently recorded or listened to by the agent.
- Nunito SemiBold/ExtraBold assets use the included SIL Open Font License. All Vietnamese accented characters in the scripts are covered; minimum UI text size is 24 at the 1920x1080 reference resolution.
- Editor startup-preview command preserves the original scene and saves a camp/title preview to all three former entry scenes. The before-Play screen now visibly shows Vanguard instead of Mario.


# Boss skills — 2026-10-04

User rejected all five newly authored character models: removed their FBX, Blender source and previews. Existing boss appearance remains, with joint pivots added for casting. No downloaded character assets were used.

| Boss | Normal skill | Added below 50% HP |
|---|---|---|
| Hashirama | Wood dragon eruption | Wood golem slam |
| Gaara | Sand burial vortex | Moving sand tsunami |
| Onoki | Particle release cube | Elongated particle release beam |
| Fourth Raikage | Lightning armor charge | Wider, faster lightning lariat |
| Mei | Molten lava pool | Boil release mist field |

Second phase stays unlocked after healing, and alternates between the two skills. Casts have a 1.2-second ground warning; released zones damage player and clones. Lava/mist tick once per second; other releases hit each target once. Dodge/retreat cancels pending warning and attached lightning aura; death destroys the boss's released effects. Effects pause with gameplay.

Skill meshes are original Blender 4.5.3 bpy work, in `ArtSource/Bosses/*_Skill*.blend`, exported to `Assets/Art/Bosses/Models`. Runtime effects combine the imported meshes, animated transforms, chakra materials and particles. Character casting uses the existing procedural joint animation system, not purchased motion capture.

Validation: `Logs/BossAssetsQA/Import.txt`, `Logs/BossAssetsQA/Validation.txt`, ten Unity-rendered screenshots. Editor menus: Naruto > Build Blender boss assets. Test requests: `Temp/BossSkillsValidation.request`. Regenerate mesh sources: `uv run --python 3.11 --with bpy==4.5.3 python QA/build_boss_assets.py`.

Final Unity validation passed all ten player/clone damage, cast animation, pause, cancellation warning cleanup and death-owned effect cleanup checks. All five existing boss appearances retained.

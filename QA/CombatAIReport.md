# Combat AI and shadow clones

Shadow clones snapshot 30% of player HP, maximum HP, armor, basic attack damage and movement speed when summoned. Fractional HP/armor preserve exactly 30% of the 15 HP baseline. Existing 10-second summon lifetime and attack cadence remain. Clones acquire living enemies and bosses; boss attacks prefer nearby living clones. Enemy projectiles and melee can damage clones. Boss hazards use separate cooldowns per clone rather than applying damage every frame.

Each boss arena has a visible stone altar, green crystal and 12-meter healing boundary. Below 30% maximum HP, enemies/bosses retreat at 1.5 times their own normal speed, heal 10% maximum HP per completed second, and return to combat at 80%. Fractional healing is accumulated for integer HP. A player within the boundary plus 1.5 meters interrupts healing and attracts attacks. Pausing stops AI and healing; death cannot be reversed by healing.

AI reacts to nearby charged Rasengan, approaching Rasenshuriken trajectories and the full Rasenshuriken blast radius. Escape and retreat use existing NavMesh paths, recalculated at most every 0.3 seconds unless the destination changes materially. Evasion is movement, not invulnerability, so attacks can still connect before an enemy clears the area.

Validation: PASS in Unity Play Mode, including no runtime errors/exceptions during the test. Verified clone 30% stats, attacks against boss, boss/enemy attacks against clones, nonlethal and lethal damage, enemy/boss healing, contested altar and projectile/blast escape decisions. Runtime/editor C# compilation also passed. The Play Mode harness in Assets/Editor/LumiCombatAIValidation.cs. Latest results are saved to Logs/CombatAIQA/Validation.txt.

Enemy HP updated: Sprout 15, Ranger 30, Golem 60 (1.5x). Clone fractional attack damage accumulates across hits to preserve 30% average damage (1.2 per attack). Relaxed stance ankle spacing is 0.19m, with arms angled slightly outwards.

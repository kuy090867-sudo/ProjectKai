# QA Bug Report — Project Kai v0.1
**Date:** 2026-04-06
**Tester:** QA (AI)
**Build:** 79 scripts, 12 scenes, Unity 6 + URP 2D
**Method:** Full codebase static analysis + Unity MCP live test

---

## Summary

| Severity | Count | Description |
|----------|-------|-------------|
| CRITICAL | 9 | Game crash, data loss, soft-lock |
| HIGH | 12 | Exploits, logic errors, memory leaks |
| MEDIUM | 10 | Performance, visual, minor logic |
| LOW | 5 | Edge cases, polish |
| **TOTAL** | **36** | |

---

## CRITICAL (9)

### C1. Gravity Scale Accumulation — WallSlideState
- **File:** `Scripts/Player/States/WallSlideState.cs:21`
- **Code:** `Player.SetGravityScale(Player.Rb.gravityScale * 0.3f)`
- **Bug:** Multiplies CURRENT gravity (already modified) instead of DEFAULT. Repeated wall-slide entries cause gravity to shrink toward 0.
- **Repro:** Wall slide -> fall -> wall slide -> fall (repeat 5x) -> player floats
- **Fix:** Use `Player._defaultGravityScale * 0.3f` or call `ResetGravity()` first

### C2. Gravity Scale Accumulation — FallState
- **File:** `Scripts/Player/States/FallState.cs:13`
- **Code:** `Player.SetGravityScale(Player.Rb.gravityScale * Player.FallGravityMultiplier)`
- **Bug:** Same as C1. Jump->Fall->Jump->Fall causes fall speed to compound exponentially.
- **Repro:** Rapid jump-fall cycles (10x) -> fall speed becomes extreme or near-zero
- **Fix:** Same — use stored default gravity scale

### C3. MeleeAttackState Recursive Enter()
- **File:** `Scripts/Player/States/MeleeAttackState.cs:103-104`
- **Code:** `StateTimer = 0f; Enter();` (direct call instead of state machine transition)
- **Bug:** Exit() never called between combo steps -> state cleanup skipped, potential memory leak, stale references
- **Repro:** Input 3-hit combo rapidly (J J J)
- **Fix:** Use `Player.StateMachine.ChangeState(Player.MeleeAttackState)` instead

### C4. SaveSystem Load() Incomplete
- **File:** `Scripts/Core/SaveSystem.cs:51-58`
- **Bug:** Save() stores Level, Exp, StatPoints, STR, DEX, INT to PlayerPrefs but Load() only restores Gold. All other progression data is lost on reload.
- **Repro:** Level up to Lv5 -> Save -> Quit -> Load -> Player is Lv1 with 0 stats, only gold preserved
- **Fix:** Add PlayerPrefs.GetInt for each field in Load()

### C5. ManaSystem Event Parameter Mismatch
- **File:** `Scripts/Core/ManaSystem.cs:55`
- **Code:** `OnManaChanged?.Invoke(CurrentMana, _maxMana)` — uses raw `_maxMana` instead of `actualMax` (which includes INT bonus)
- **Bug:** Mana UI bar shows wrong max capacity when player has INT stat points
- **Repro:** Allocate INT stat points -> use mana -> observe UI bar scale mismatch
- **Fix:** Calculate `actualMax` before invoking, same as in the regen path

### C6. BossGoblin Summoned Minion Initialization
- **File:** `Scripts/Enemy/BossGoblin.cs:280`
- **Bug:** Dynamically created minions get `EnemyBase` component added but Awake/Start may not properly initialize before `DamageReceiver` processes damage. Empty OnDeath lambda (line 285) does nothing.
- **Repro:** Boss Phase 3 summon -> attack minions -> potential NullRef
- **Fix:** Ensure EnemyBase fields are explicitly set after AddComponent

### C7. EnemyBase Cached Player Reference Destroyed
- **File:** `Scripts/Enemy/EnemyBase.cs:142`
- **Bug:** `_playerTransform` cached once in Start(). If player GameObject is destroyed (scene change, death), all enemy chase/attack logic throws NullReferenceException.
- **Repro:** Start chase state -> trigger scene transition -> NullRef crash
- **Fix:** Null-check before every `_playerTransform` access, or re-find on null

### C8. EtherMage Projectile During Player Destruction
- **File:** `Scripts/Enemy/EtherMage.cs:95`
- **Bug:** `FireProjectile()` coroutine uses `_player` reference. If player destroyed mid-coroutine, direction calculation crashes.
- **Repro:** EtherMage starts attack sequence -> player dies/scene changes mid-attack
- **Fix:** Add null check after each `yield` in attack coroutine

### C9. ComboSystem Array Out of Bounds
- **File:** `Scripts/Combat/ComboSystem.cs:32`
- **Code:** `var step = _comboData.steps[_currentStep]`
- **Bug:** No bounds check before array access. If `_comboData` is null or empty, crashes with IndexOutOfRangeException.
- **Repro:** Equip weapon with no combo data -> press attack
- **Fix:** Add `if (_comboData?.steps == null || _currentStep >= _comboData.steps.Length) return null;`

---

## HIGH (12)

### H1. Dash + Attack Exploit
- **File:** `Scripts/Player/States/DashState.cs`
- **Bug:** DashState doesn't block attack input. Player can attack while invincible during dash.
- **Repro:** Hold Shift (dash) + press J (attack) simultaneously
- **Impact:** Invincible attacking breaks game balance
- **Fix:** Block attack input in DashState.Execute() or cancel dash on attack

### H2. Dead Enemies Can Still Attack
- **Files:** `Scripts/Enemy/OrcWarrior.cs:54`, `Scripts/Enemy/SkeletonArcher.cs:54`
- **Bug:** OnDeath sets state to Dead and schedules Destroy with delay, but running attack coroutines continue executing during the delay.
- **Repro:** Kill OrcWarrior mid-shield-bash -> player takes damage from dead enemy
- **Fix:** StopAllCoroutines() in OnDeath callback

### H3. KnightCommander No Damage Feedback
- **File:** `Scripts/Enemy/KnightCommander.cs:239`
- **Code:** `private void OnDamaged(float damage, Vector2 dir) { }` — empty method
- **Bug:** Boss shows zero visual/audio feedback when hit. Player cannot tell if attacks are connecting.
- **Repro:** Attack KnightCommander boss -> no flash, no sound, no knockback
- **Fix:** Add hit flash, sound, and SpriteAnimator.Play("hit") like BossGoblin does

### H4. Dash Death Leaves Zero Gravity
- **File:** `Scripts/Player/States/DashState.cs:13,38`
- **Bug:** Enter() sets gravity to 0. If player dies during dash, Exit() may not be called, leaving permanent zero gravity.
- **Repro:** Dash into lethal damage -> respawn -> player floats
- **Fix:** Reset gravity in PlayerController death handler

### H5. InGameHUD Hardcoded Max HP
- **File:** `Scripts/UI/InGameHUD.cs:45`
- **Code:** `_hpFill.fillAmount = _player.CurrentHealth / 100f`
- **Bug:** Assumes max HP is always 100. With STR bonus (STR * 5f), actual max can be 120+. HP bar shows wrong percentage.
- **Repro:** Allocate 4 STR -> max HP = 120 -> take 20 damage -> bar shows 100/100 instead of 100/120
- **Fix:** Get actual max HP from HealthSystem or ProgressionSystem

### H6. Event Subscription Leaks (6 locations)
- **Files & Lines:**
  - `StageManager.cs:45` — `dr.OnDeath += OnEnemyKilled` (no -=)
  - `GameSetup.cs:66` — `dr.OnHealthChanged += lambda` (no -=)
  - `LevelUpPopup.cs:23` — `OnLevelUp += Show` (no -=)
  - `EnemyReward.cs:19` — `dr.OnDeath += lambda` (no -=)
  - `ItemDrop.cs:18` — `dr.OnDeath += lambda` (no -=)
  - `DialogueSystem.cs` — `OnDialogueComplete` (no -=)
- **Bug:** Events subscribed but never unsubscribed. On scene reload, handlers fire multiple times. Memory leak from retained references.
- **Impact:** Double rewards, double kill counts, stacking UI elements
- **Fix:** Add OnDestroy() with -= for each subscription

### H7. Time.timeScale Conflict
- **File:** `Scripts/Core/GameFeel.cs:73,99`
- **Bug:** HitStop sets timeScale=0.02f, KillSlowMotion sets timeScale=0.2f. When both trigger simultaneously (killing blow), they overwrite each other unpredictably.
- **Repro:** Kill an enemy with a melee hit -> both HitStop and KillSlowMo activate
- **Fix:** Implement priority-based timeScale system or queue effects

### H8. WallCheck Includes Trigger Colliders
- **File:** `Scripts/Core/WallCheck.cs:33-38`
- **Bug:** Wall detection raycast doesn't filter out trigger colliders. Player can wall-jump on spike traps and other trigger zones.
- **Repro:** Stand next to FloorSpikes trigger -> attempt wall jump -> succeeds
- **Fix:** Add `&& !hit.collider.isTrigger` check

### H9. Coyote Time Double-Use
- **Files:** `Scripts/Player/States/FallState.cs:34-35`, `Scripts/Player/States/JumpState.cs:14`
- **Bug:** Coyote jump can be consumed in FallState, then immediately re-consumed if player quickly enters JumpState. Allows double coyote jumps.
- **Repro:** Walk off ledge -> jump (coyote) -> rapidly input jump again
- **Fix:** Ensure ConsumeCoyote() is called atomically and checked in both states

### H10. EtherMage Teleport Out of Bounds
- **File:** `Scripts/Enemy/EtherMage.cs:130`
- **Code:** `float offsetX = Random.Range(-_teleportRange, _teleportRange)`
- **Bug:** No boundary check after teleport. Mage can teleport off-screen, into walls, or into unreachable areas.
- **Repro:** Fight EtherMage near level boundary -> mage teleports outside playable area
- **Fix:** Clamp position to level bounds after teleport

### H11. HealthBarUI Memory Leak on Target Death
- **File:** `Scripts/UI/HealthBarUI.cs:34`
- **Bug:** When target (enemy) is destroyed, health bar UI object persists in scene. LateUpdate checks `_target != null` but doesn't destroy self.
- **Repro:** Kill enemy -> observe orphaned health bar floating where enemy died
- **Fix:** Add `if (_target == null) { Destroy(gameObject); return; }` in LateUpdate

### H12. BossHealthBar Persists Across Scenes
- **File:** `Scripts/UI/BossHealthBar.cs:18-28`
- **Bug:** Uses DontDestroyOnLoad but is never removed when boss dies. Persists into next scene. Second boss fight shows two boss health bars.
- **Repro:** Kill boss -> return to Hub -> enter new boss stage -> old bar visible
- **Fix:** Subscribe to boss OnDeath and call Remove()

---

## MEDIUM (10)

### M1. Frame-Dependent Hit Detection
- **File:** `Scripts/Enemy/EnemyBase.cs:173`
- **Code:** `attackTime >= duration*0.3f && attackTime < duration*0.3f + Time.deltaTime`
- **Bug:** Hit window is exactly 1 frame (Time.deltaTime). At low FPS, the window gets longer; at high FPS, it gets shorter. Inconsistent damage.
- **Fix:** Use a flag-based approach: set `_hasHitThisAttack` once, check range

### M2. Texture2D Memory Leak (3 locations)
- **Files:** `SkeletonArcher.cs:139`, `BossGoblin.cs:262`, `EtherMage.cs:104`
- **Bug:** Runtime-created Texture2D objects are never explicitly destroyed. Material holds reference even after GameObject is destroyed.
- **Impact:** ~4KB per texture * arrows/projectiles = gradual memory increase
- **Fix:** `Destroy(texture)` before or during OnDestroy

### M3. Parallax Jump on Camera Snap
- **File:** `Scripts/Core/ParallaxLayer.cs:17-23`
- **Bug:** `_startCamX` cached at Start(). Camera teleport (scene load, checkpoint) makes parallax calculation use stale base position. Background visually jumps.
- **Fix:** Track camera delta per frame instead of absolute offset

### M4. Camera Jitter on Direction Change
- **File:** `Scripts/Camera/CameraFollow.cs:38-45`
- **Bug:** Double smoothing (look-ahead Lerp + position Lerp) causes delayed response when player quickly changes direction (A/D spam).
- **Fix:** Increase look-ahead speed or use separate smoothing curves

### M5. All Combo Steps Use Same Animation
- **File:** `Scripts/Player/States/MeleeAttackState.cs:46`
- **Code:** `Player.SpriteAnim?.Play("hit", 12f, false)` for ALL combo steps
- **Bug:** "attack", "attack2", "attack3" animations are registered but never played. All 3 combo hits look identical.
- **Fix:** Select animation based on combo step index

### M6. Attack Scale Not Reset After Combo
- **File:** `Scripts/Core/SpriteAnimator.cs:161-166`
- **Bug:** Attack3 applies scale (1.1, 1.1). MeleeAttackState.Exit() doesn't call ResetToIdle(). Scale carries over to idle/run.
- **Repro:** Complete 3-hit combo -> character appears 10% larger in idle
- **Fix:** Call `ResetToIdle()` in MeleeAttackState.Exit()

### M7. DialogueSystem ScriptableObject Leak
- **File:** `Scripts/UI/DialogueSystem.cs:71-73`
- **Bug:** `ScriptableObject.CreateInstance<DialogueDataSO>()` created at runtime but never destroyed. Each dialogue trigger allocates permanent memory.
- **Fix:** Store reference and `Destroy()` in OnDestroy or after dialogue completes

### M8. DifficultyScaler Heal() Ineffective
- **File:** `Scripts/Core/DifficultyScaler.cs:31`
- **Bug:** Uses `Heal()` to add bonus HP, but if Heal() is capped at MaxHealth, the bonus never applies. Enemy spawns with base HP, not scaled HP.
- **Fix:** Directly set `_maxHealth += bonusHP; _currentHealth = _maxHealth;`

### M9. EnemySpawnPoint Plays Wrong Sound
- **File:** `Scripts/Core/EnemySpawnPoint.cs:38`
- **Code:** `AudioManager.Instance?.PlaySFX("enemy_death", 0.3f)`
- **Bug:** Plays death sound when enemies SPAWN. Should play spawn/alert sound or no sound.
- **Fix:** Change to spawn-appropriate SFX or remove

### M10. Invincibility Frame Race Condition
- **File:** `Scripts/Combat/DamageReceiver.cs:42-83`
- **Bug:** If two damage sources hit on the exact same frame, both pass the `_isInvincible` check before either sets it to true.
- **Repro:** Two enemies attack player simultaneously
- **Fix:** Set `_isInvincible = true` BEFORE processing damage, not after

---

## LOW (5)

### L1. HealthBarUI Division by Zero
- **File:** `Scripts/UI/HealthBarUI.cs:29`
- **Code:** `_fillImage.fillAmount = current / max`
- **Risk:** If max=0, result is NaN. UI breaks.
- **Fix:** `max > 0 ? current/max : 0f`

### L2. SaveSystem No Version Control
- **File:** `Scripts/Core/SaveSystem.cs`
- **Risk:** No save version stored. Format changes break existing saves silently.
- **Fix:** Add `PlayerPrefs.SetInt("SaveVersion", 1)` and migration logic

### L3. ProgressionSystem Level=0 Edge Case
- **File:** `Scripts/Core/ProgressionSystem.cs:32`
- **Code:** `ExpToNextLevel => Level * 100`
- **Risk:** If Level=0, requires 0 XP to level up. Infinite level loop.
- **Fix:** `Math.Max(Level, 1) * 100`

### L4. GameSetup.AutoAssignLayers() Dead Code
- **File:** `Scripts/Core/GameSetup.cs:127-151`
- **Bug:** Method defined but never called. Layers may not be assigned properly.
- **Fix:** Call in Awake() or delete if redundant

### L5. HealthPickup Fixed 30HP
- **File:** `Scripts/Core/HealthPickup.cs:12`
- **Risk:** Always heals 30HP regardless of max HP. At high STR (max HP 200+), 30HP is 15%. Too weak late-game.
- **Suggestion:** Scale with max HP: `healAmount = Mathf.Max(30f, maxHP * 0.25f)`

---

---

## LIVE TEST FINDINGS (Unity MCP Inspection — Stage1_1)

### LT1. Player & Enemy Layers = Default (0) — NOT Player(7)/Enemy(8)
- **Confirmed:** Player.layer = 0, Goblin_1.layer = 0
- **Cause:** `GameSetup.AutoAssignLayers()` exists but is NEVER called (dead code, see L4)
- **Impact:** Layer-based collision detection (`_playerLayer` LayerMask in EnemyBase) may fail. Only works because of tag-based fallback.
- **Severity:** HIGH — layer masks are unreliable

### LT2. AutoPlayTest Component Still Active
- **Object:** Managers (Stage1_1)
- **Bug:** `AutoPlayTest` debug component is still attached. This auto-plays the game in test mode.
- **Impact:** May interfere with normal gameplay or cause unexpected behavior
- **Fix:** Remove AutoPlayTest component from all stage scenes

### LT3. Player SpriteRenderer Has No Sprite in Editor
- **Object:** Player/Sprite — bounds size = (0,0,0)
- **Cause:** GameSetup loads sprites at runtime. If GameSetup fails or is missing, player is invisible.
- **Impact:** Runtime dependency — fragile

### LT4. Goblin BoxCollider2D isTrigger = true
- **Object:** Goblin_1 — BoxCollider2D.isTrigger = true
- **Impact:** Enemy doesn't physically block player. Player walks through enemies. May be intentional (Kinematic body) but looks wrong visually.

### LT5. StageManager Dialogues = null
- **Object:** Managers/StageManager — `_introDialogue` and `_clearDialogue` are null
- **Impact:** Stage1_1_Init handles dialogue at runtime, so this is OK. But if Init script fails, no fallback.

---

## Balance Feedback (GD)

### Combat
- Combo 3-hit damage multiplier needs documentation and testing
- Dash cooldown (0.5s) vs dash duration (0.15s) — gap feels too long?
- Invincibility frame duration (0.5s) — standard for the genre but feels long for DMC-style fast combat

### Enemies
- SkeletonArcher arrow trajectory depends on framerate (M1) — needs normalization
- OrcWarrior shield bash has very narrow hit window — may frustrate players
- EtherMage teleport range unbounded (H10) — balance concern beyond bug

### Progression
- 30HP potion is static — doesn't scale (L5)
- STR gives +5 HP per point — may be too low at higher levels
- Save system doesn't persist stats (C4) — critical for progression

---

## Testing Environment Notes
- Unity 6 (6000.3.3f1) + URP 2D
- Windows 11
- Console error on launch: "Cannot import package in play mode" (not from project code)

---

## Priority Fix Order (DEV)
1. **C4** SaveSystem Load — data loss (player progression destroyed)
2. **C1+C2** Gravity accumulation — movement breaks
3. **C3** MeleeAttackState recursion — state machine corruption
4. **H6** Event leaks (6 locations) — cascading bugs
5. **H1** Dash+Attack exploit — balance breaking
6. **H5** InGameHUD hardcoded HP — wrong UI info
7. **H7** TimeScale conflicts — game freeze/speed issues
8. **C5** ManaSystem event mismatch — UI desync
9. **H2** Dead enemy attacks — unfair damage
10. **H3** KnightCommander no feedback — unreadable boss fight

---

## CLEAN 정리 보고 (2026-04-06)

### 완료된 정리
- DungeonTileset 원본 폴더 삭제 (2.2MB, 740파일) — Resources에 복사본 있어 미사용
- Lit2DSceneTemplate.scenetemplate 삭제 (3.8MB) — 미사용 URP 템플릿
- SampleScene.unity 삭제 + EditorBuildSettings에서 제거
- 미사용 패키지 6개 제거: cinemachine, aseprite, psdimporter, multiplayer-center, recorder, visualscripting
- 빈 폴더 20개+ 삭제 (Animations, Art/UI, Audio, Data, Prefabs, Resources 하위)

### DEV에게 전달: 죽은 코드 정리 필요
| ID | 파일 | 내용 | 비고 |
|----|------|------|------|
| CL1 | Stage1_1.unity, TestStage.unity | AutoPlayTest 유령 참조 | Missing Script 경고 유발 |
| CL2 | GameSetup.cs:127-152 | AutoAssignLayers() 미호출 메서드 | 삭제 또는 호출 추가 |
| CL3 | StatusEffect.cs | StatusEffectHandler 클래스 미참조 | 사용 계획 없으면 삭제 |
| CL4 | QARecorder.cs | 디버그 전용 | #if UNITY_EDITOR 가드 추가 |
| CL5 | GameFeel.cs:2 | unused using UnityEngine.UI | 제거 |

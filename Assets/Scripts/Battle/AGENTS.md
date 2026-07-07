<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-06-23 | Updated: 2026-06-23 -->

# Battle (턴제 전투 시스템)

## Purpose
턴제 전투의 전체 로직을 담당. 추상 엔티티 기반(`BattleEntity`) 위에 플레이어(`Player`)와 몬스터(`Monster`)를 구현하고,
`BattleManager`가 코루틴 턴 루프를 구동한다. 스킬은 `SkillBase`(SO)에서 파생된다.
UI(`BattleUI`)는 전투 판정 로직을 여기에 위임하고 이벤트만 구독한다.

## Key Files
| 파일 | 설명 |
|------|------|
| `BattleEntity.cs` | **추상 클래스**(순수 C#). 전투 엔티티 공통 기반. `RuntimeStats`/`StatusEffect`/`Skills` 관리, `TakeDamage`/`Heal`/`ApplyEffect`/`TickEffects`. 추상 `TakeTurn`. `BattleContext`, `StatusEffect`, `StatusEffectType`도 정의. |
| `BattleManager.cs` | MonoBehaviour. 코루틴 턴 루프. `OnBattleStarted/OnTurnStarted/OnTurnEnded/OnBattleEnded/OnBattleLog` 이벤트, 플레이어 입력 API(`OnPlayerChooseBasicAttack/Skill/Item`), 선공 결정·보상 지급. |
| `Monster.cs` | `BattleEntity` 구현. `MonsterData` 기반 생성, `DecideAction()` AI(20% 스킬/30%HP 이하 10% 방어), 드롭 계산(`GetGoldDrop`/`RollDrops`). `MonsterAIAction` 열거형. |
| `Player.cs` | `BattleEntity` 구현. `QueueAction` 입력 큐, 기본공격/스킬/아이템 사용, 인벤토리(`List<ItemData>`)·골드 관리. `ApplyItemEffect`로 `ItemEffectType` 분기. |
| `SkillBase.cs` | **ScriptableObject**. 스킬 공통 기반. `Execute()` 가상 디스패치(Attack/Heal/Buff/Debuff) + 상태이상 적용 + 쿨다운. `SkillType` 열거형. |

## Subdirectories
| 디렉터리 | 목적 |
|-----------|------|
| `Skills/` | `SkillBase` 파생 구체 스킬 클래스들 (see `Skills/AGENTS.md`) |

## For AI Agents

### Working In This Directory
- **엔티티는 순수 C#**: `BattleEntity`/`Player`/`Monster`는 `MonoBehaviour`가 아니다. 씬에 직접 붙일 수 없고 `BattleManager`가 인스턴스를 생성·보관한다. `new Player(data)` / `new Monster(monsterData)`로 생성.
- **피해/회복은 반드시 `TakeDamage`/`Heal` 경유**: 방어력 감산·최소 1 보장·`ignoreDefense`·사망 이벤트 발행이 여기서 통합 처리된다. HP를 직접 건드리지 말 것.
- **플레이어 행동 큐**: 플레이어 턴은 `QueueAction`으로 행동을 등록하고 `BattleManager`의 `WaitUntil(!_waitingForPlayerInput)` 이후 `TakeTurn`에서 소비된다. 새 행동을 추가하려면 `BattleManager.OnPlayerChoose*` → `Player.QueueAction` 경로를 따를 것.
- **스킬 쿨다운 저장 주의**: `SkillBase.currentCooldown`은 `[NonSerialized]`이고 `Player.AddSkill`이 `Object.Instantiate`으로 복제본을 만든다. 원본 SO 에셋에는 쿨다운이 저장되지 않는다(루트 AGENTS.md의 안티패턴 참고 — 런타임 래퍼 도입이 장기 과제).
- **Monster AI 스킬은 미구현**: `Monster.TakeTurn`의 `UseSkill` 분기는 현재 기본 공격으로 폴백(TODO). `skillIds` 로딩도 TODO.

### Testing Requirements
- 테스트 프레임워크 없음. 전투 흐름은 Play Mode로 검증 — `DemoSceneBuilder`로 데모 씬을 빌드 후 `RoguelikeRPG/Demo/Open Playable Demo And Play`.
- `BattleEntity.TakeDamage`/`RNGSystem.RollDamage`는 EditMode 유닛 테스트 1순위.

### Common Patterns
- C# `event Action<T>`로 UI에 상태 전파. UI는 역방향으로 Manager 메서드만 호출(단방향).
- 크리티컬 판정: `context.RNG.Roll(critChance)` → `RollDamage` → `critMultiplier` 곱.
- `foreach (Transform child in container) Destroy(child.gameObject)` 패턴으로 동적 버튼 갱신(UI에서).

## Dependencies

### Internal
- `BattleManager` → `Core.GameManager`(RNG, 사망 통지), `Data.MonsterData`, `Core.RNGSystem`.
- `Monster`/`Player` → `Data.*`(`MonsterData`/`PlayerData`/`ItemData`/`RuntimeStats`), `Core.RNGSystem`.
- `SkillBase` → `RuntimeStats`, `StatusEffect`(같은 네임스페이스).
- `BattleEntity`는 `Data.RuntimeStats`를 사용 — Data 계층으로의 의존 존재(역방향 주의).

### External
- `UnityEngine`(`Mathf`, `Debug`).

<!-- MANUAL: -->

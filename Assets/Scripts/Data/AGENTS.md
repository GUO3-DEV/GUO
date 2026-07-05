<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-06-23 | Updated: 2026-06-23 -->

# Data (데이터 정의 계층)

## Purpose
게임 콘텐츠를 데이터로 정의하는 최하위 계층. 몬스터·아이템·플레이어 스탯을 `ScriptableObject`(또는 `[Serializable]`)로 기술하여
기획자가 코드 수정 없이 콘텐츠를 추가할 수 있게 한다. 어떤 도메인 계층(Core/Battle/Event/UI)도 이 계층에 의존할 수 있지만,
Data 계층은 상위 계층을 참조하지 않는다(단방향).

## Key Files
| 파일 | 타입 | 설명 |
|------|------|------|
| `MonsterData.cs` | ScriptableObject | 몬스터 한 종류의 스탯·드롭 테이블·`skillIds`. `RollGoldReward(rng)`. `MonsterType`(Normal/Elite/Boss), `DropEntry` 정의. |
| `ItemData.cs` | ScriptableObject | 아이템 속성·효과 목록(`List<ItemEffect>`). `ItemType`/`ItemRarity`/`ItemEffect`/`ItemEffectType` 열거형·클래스 정의. |
| `PlayerData.cs` | `[Serializable]` (SO 아님) | 플레이어 기본 스탯·시작 골드·`startingSkillIds`. **전투 런타임 스탯 래퍼 `RuntimeStats` 구조체도 여기에 정의**(`BattleEntity`가 사용). |

## For AI Agents

### Working In This Directory
- **SO vs Serializable 구분**: `MonsterData`/`ItemData`는 SO(`[CreateAssetMenu]`, `Assets/Data/`에 에셋). `PlayerData`는 `[Serializable]` 일반 클래스(GameManager의 `SerializeField`에 임베드). 새 콘텐츠 타입 추가 시 이 기준 따를 것.
- **데이터에 런타임 상태 금지**: SO 필드에 런타임 변동값(현재 HP·쿨다운 등)을 넣지 말 것 — 에디터 에셋이 오염됨. 런타임 값은 `RuntimeStats`/`currentCooldown`(`[NonSerialized]`)처럼 별도 래퍼로.
- **`RuntimeStats` 위치 주의**: `RuntimeStats`는 Data 계층에 있지만 `Battle.BattleEntity`가 소비한다. 버프(`bonusAttack`/`bonusDefense`) 적용은 `FinalAttack`/`FinalDefense`로만 읽을 것. 이 구조체 필드를 바꾸면 Battle 전체에 영향.
- **열거형 확장 시 동기화**: `ItemEffectType`/`ConditionType`/`OutcomeType` 등을 추가하면 소비자(`Player.ApplyItemEffect`, `ChoiceHandler`)의 `switch`에도 분기를 추가해야 한다(누락 시 `default` 경고).
- **CreateAssetMenu order**: MonsterData=10, ItemData=11, EventData=12, SkillBase=20, Skills=21~23. 새 SO는 빈 order 구간을 피해 배정.

### Testing Requirements
- 테스트 프레임워크 없음. `RuntimeStats.FinalAttack`/`IsDead` 계산은 EditMode 유닛테스트 후보.
- SO 에셋은 인스펙터에서 값 검증, `DemoSceneBuilder.Validate*`로 씬 내 참조 무결성 검증.

### Common Patterns
- `[Header]/[Tooltip]/[Range]/[TextArea]`로 인스펙터 노출 제어.
- 데이터 주도 효과: `ItemEffect`(effectType+value+duration) 조합으로 다양한 효과 표현.
- RNG가 필요한 유틸(`RollGoldReward`)은 `RNGSystem`을 인자로 받아 재현성 보장.

## Dependencies

### Internal
- `MonsterData.RollGoldReward` → `Core.RNGSystem`(인자로 수신, 직접 참조 아님).
- `RuntimeStats`는 Battle 계층(`BattleEntity`)에서 사용 — **Data→Battle 의존은 없지만 Battle→Data 의존이 존재**.

### External
- `UnityEngine`(`ScriptableObject`, `Vector2Int`, `Sprite`).

<!-- MANUAL: -->

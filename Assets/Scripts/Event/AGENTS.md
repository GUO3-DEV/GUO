<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-06-23 | Updated: 2026-06-23 -->

# Event (던전 이벤트 시스템)

## Purpose
던전 탐색 중 발생하는 텍스트 이벤트(선택지 분기) 시스템. `EventData`(SO)로 이벤트를 데이터 기술하고,
`EventManager`가 이벤트 풀 관리·가중치 추첨·진행을 총괄하며, `ChoiceHandler`가 조건 평가·결과 실행을 담당한다.
기획자가 코드 없이 이벤트를 제작할 수 있는 데이터 주도 구조.

## Key Files
| 파일 | 설명 |
|------|------|
| `EventManager.cs` | MonoBehaviour. `OnEventStarted/OnEventEnded/OnChoiceResolved/OnBattleRequested` 이벤트. 가중치 추첨(`TriggerRandomEvent(floor)`), ID 조회(`TriggerEventById`), 선택지 처리 + 이벤트 체인(`nextEventId`). |
| `ChoiceHandler.cs` | 순수 C# 서비스. `EvaluateConditions`(HP%/골드/아이템 보유), `ExecuteOutcomes`(결과 실행 후 전투 트리거 `MonsterData` 반환). |
| `EventData.cs` | **ScriptableObject**. 이벤트 하나 정의(제목/본문/선택지/`linkedMonster`/`spawnWeight`). `EventType`/`EventChoice`/`ChoiceCondition`/`ConditionType`/`ChoiceOutcome`/`OutcomeType` 모두 여기에 정의. |

## For AI Agents

### Working In This Directory
- **초기화 순서 준수**: `EventManager.Initialize(player, rng)`는 `Player` 생성 이후에 호출해야 한다(`ChoiceHandler`가 Player를 필요로 함). 현재 `DemoGameBootstrap`가 이 순서를 책임진다. Awake에서는 이벤트 풀 lookup만 빌드한다.
- **전투 트리거 한계(공식 트레이드오프)**: `ChoiceOutcome`에서 `linkedMonster`를 직접 참조할 수 없어 `ChoiceHandler.ExecuteOutcomes`의 `TriggerBattle`은 현재 `null` 반환. 실제 몬스터는 `EventManager`가 `CurrentEvent.linkedMonster` 또는 결과 반환값으로 해결(`SelectChoice` 로직 참고). 이벤트→전투 연결을 손댈 때 이 중간 처리를 유지할 것.
- **조건 UI 접근**: `EventUI`는 `ChoiceHandler`에 직접 접근 못함 → `EventManager.CanSelectChoice(choice)` 경유. 새 조건 타입 추가 시 `ConditionType` 열거형 + `ChoiceHandler.EvaluateSingleCondition` 분기 + `EventUI` 표시를 모두 갱신.
- **이벤트 체인**: 선택지 `nextEventId`가 있으면 현재 이벤트 종료 후 해당 ID 이벤트로 연속 진행. `BuildEventLookup`이 ID→EventData 사전을 캐시(중복 ID는 경고).
- **결과 요약 미구현**: `BuildResultSummary`는 임시 텍스트(`"[선택] 선택 완료"`). `ChoiceOutcome` 목록을 보기 좋은 한국어 문장으로 변환하는 것은 TODO.

### Testing Requirements
- 테스트 프레임워크 없음. `ChoiceHandler.EvaluateConditions`/`ExecuteOutcomes`(순수 로직)은 EditMode 유닛테스트 1순위.
- 이벤트 흐름은 데모(`Event_DemoFirstEncounter.asset`, 전투 트리거 + 잠금 조건 버튼 2개)로 Play Mode 검증.

### Common Patterns
- 가중치 추첨은 `RNGSystem.WeightedRandom`에 위임(`PickEventForFloor`).
- 결과/조건 모두 열거형 + 값 조합으로 데이터 주도 표현(`OutcomeType`+`value`+`probability`).
- 이중 검증: UI에서 버튼을 막아도 `SelectChoice`가 `CanSelectChoice`로 재검증.

## Dependencies

### Internal
- `EventManager` → `Battle.Player`(초기화 인자), `Battle.MonsterData`(전투 트리거), `Core.RNGSystem`.
- `ChoiceHandler` → `Battle.Player`(스탯/인벤토리/골드 직접 변경), `Data.ItemData`.
- `EventData` → `Data.MonsterData`(`linkedMonster`), `Data.ItemData`(`ChoiceOutcome.relatedItem`).

### External
- `UnityEngine`.

<!-- MANUAL: -->

<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-06-23 | Updated: 2026-06-23 -->

# UI (프레젠테이션 계층 / Presenter)

## Purpose
3-레이어 아키텍처의 최상위 Presentation 계층. Manager의 이벤트를 구독해 화면을 갱신하고,
플레이어 입력(버튼 클릭)을 Manager 메서드로 전달한다. **UI 로직만 포함** — 게임 판정 로직은 일절 없고
`BattleManager`/`EventManager`에 위임한다. Unity uGUI + TextMeshPro 사용.

## Key Files
| 파일 | 설명 |
|------|------|
| `BattleUI.cs` | MonoBehaviour Presenter. `BattleManager` 이벤트 구독 → 플레이어/몬스터 HUD, 전투 로그(`ScrollRect`), 행동 패널(기본공격 + 동적 스킬/아이템 버튼), 결과 패널 갱신. |
| `EventUI.cs` | MonoBehaviour Presenter. `EventManager` 이벤트 구독 → 이벤트 패널(제목/본문/배경) + 선택지 버튼 동적 생성 + 결과 오버레이. ⚠️ 파일 주석이 인코딩 손상(mojibake)됨 — 아래 주의사항 참고. |
| `ChoiceButtonItem.cs` | 선택지 버튼 재사용 컴포넌트. `Setup(텍스트, 힌트, 사용가능여부, onClick)` / `SetInteractable`. 조건 미충족 시 힌트에 `[조건 미충족]` 접두. |

## For AI Agents

### Working In This Directory
- **단방향 의존**: UI → Manager 메서드 호출만 허용. Manager가 UI를 참조하지 않도록(event로만 통신). UI에서 게임 상태를 직접 변경 금지.
- **버튼은 매 턴 재생성**: `BattleUI.BuildSkillButtons`/`BuildItemButtons`는 `_player.Skills`/`Inventory`를 순회하며 프리팹을 인스턴스화하고 기존 자식을 `Destroy`한다. (루트 AGENTS.md의 "TODO" 기재는 현재 **구현 완료** 상태로 코드가 바뀜 — 루트 문서 갱신 필요.) 캡처(closure) 변수는 루프 안에서 지역에 복사(`capturedSkill`/`capturedItem`)해 클로저 함정을 피할 것.
- **`EventUI.cs` 인코딩 경고**: 이 파일의 한국어 주석이 깨져 보임(`?대깽?...`). UTF-8이 아닌 CP949/EUC-KR으로 저장된 것으로 추정. 코드 기능은 정상 동작하지만, 주석을 수정하거나 편집할 때 인코딩을 UTF-8으로 변환 저장하는 것을 권장 — 그렇지 않으면 편집 시 손상 확대. `FindFirstObjectByType<EventManager>()`로 매니저를 찾고 `CanSelectChoice`로 선택지 가용성을 묻는다.
- **선택지 가용성**: `EventUI.BuildChoiceButtons`는 `_eventManager.CanSelectChoice(choice)` 결과로 버튼 활성화/힌트 색을 결정한다. 판정 로직은 Event 계층에 있음.
- **씬 내 직접 연결**: `BattleUI`/`EventUI`는 `Awake`에서 `FindFirstObjectByType`로 매니저를 찾는다. 매니저가 없으면 `LogError` 후 동작 안 함 — 씬 구성 시 매니저 배치 확인. 이 연결은 `DemoSceneBuilder`가 자동화.

### Testing Requirements
- 테스트 프레임워크 없음. UI는 Unity Play Mode로만 검증 가능.
- 데모 씬 빌드(`RoguelikeRPG/Demo/Rebuild Playable Demo Scene`) 후 전투/이벤트 흐름 수동 확인. 선택지 조건 미충족 버튼 비활성화도 데모로 확인.

### Common Patterns
- `Subscribe*`/`Unsubscribe*` 쌍으로 이벤트 구독/해제(`OnDestroy`에서 누수 방지).
- HUD 갱신은 엔티티 단위 이벤트(`OnDamageTaken`/`OnHealReceived`)에도 구독해 즉시 반영.
- 동적 리스트: 컨테이너 자식 전체 `Destroy` 후 재생성 + `VerticalLayoutGroup` 자동 배치.
- TextMeshPro(`TMP_FontAsset`) 사용 — 데모 폰트는 `Assets/Fonts/MalgunGothic.asset`.

## Dependencies

### Internal
- `BattleUI` → `Battle.BattleManager`, `Battle.Player`/`Monster`, `Battle.SkillBase`, `Data.ItemData`, `Core.GameManager`(결과 확인 시).
- `EventUI` → `Event.EventManager`, `Event.EventData`/`EventChoice`.
- `ChoiceButtonItem` → 독립적(uGUI만).

### External
- `UnityEngine`, `UnityEngine.UI`(`Button`/`Slider`/`ScrollRect`/`Image`), `TMPro`(`TextMeshProUGUI`).

<!-- MANUAL: -->

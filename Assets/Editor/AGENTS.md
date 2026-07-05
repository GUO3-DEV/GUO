<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-06-23 | Updated: 2026-06-23 -->

# Editor (Unity 에디터 도구)

## Purpose
Unity 에디터 전용(Editor-only) 도구. 이 디렉터리의 스크립트는 빌드에 포함되지 않고 에디터 환경에서만 동작한다.
현재는 플레이 가능한 데모 씬을 프로그램적으로 빌드·검증·실행하는 도구를 제공한다.

## Key Files
| 파일 | 설명 |
|------|------|
| `DemoSceneBuilder.cs` | 정적 에디터 도구(`RoguelikeRPG.EditorTools`). 콘텐츠 빌드 호출 + `SampleScene`을 코드로 빌드(GameManager/BattleManager/EventManager/**DungeonManager/SkillRegistry**/Bootstrap + 카메라/EventSystem/Canvas/UI) + 전체 스킬·이벤트·보스 wiring + Play Mode 스크린샷 캡처. |
| `RoguelikeRPGContentBuilder.cs` | MVP 콘텐츠 일괄 생성 도구. 스킬 8·아이템 6·몬스터 7·이벤트 15를 `Assets/Data/{Skills,Items,Monsters,Events}/`에 SO로 생성. 메뉴 `RoguelikeRPG/Content/Build All MVP Content`. |

## For AI Agents

### Working In This Directory
- **Editor-only**: 이 디렉터리 코드는 `UnityEditor` API를 사용하므로 반드시 에디터 전용 폴더(`Editor/` 또는 `UnityEditor` 가드)에 있어야 빌드 에러가 나지 않는다.
- **private 필드 wiring**: UI/Bootstrap의 `[SerializeField] private` 필드를 코드에서 채울 때 `SerializedObject`+`FindProperty`+`ApplyModifiedPropertiesWithoutUndo` 패턴을 사용(`SetPrivate`). 새 private 필드를 UI에 추가하면 빌더의 `SetPrivate` 호출도 추가해야 데모에 반영됨.
- **메뉴 진입점**:
  - `RoguelikeRPG/Demo/Rebuild Playable Demo Scene` — 에셋+씬 재생성(파괴적: 씬을 비움).
  - `RoguelikeRPG/Demo/Validate Playable Demo Scene` — 에셋/참조 무결성 검증(예외로 실패 보고).
  - `RoguelikeRPG/Demo/Open Playable Demo And Play` — 검증 후 Play Mode 진입 + 4초 후 스크린샷(`.omo/evidence/unity-game-view-playmode.png`).
- **폰트 생성**: `GetDemoFont`는 `malgun.ttf`에서 TMP 폰트를 생성(Windows 시스템 폰트에서 복사). 폰트 에셋이 손상되면 삭제 후 재생성 로직 포함.
- **씬 재생성은 파괴적**: `ClearScene`이 씬의 모든 루트 GameObject를 제거한다. 수동 씬 편집 내용은 빌드 시 사라짐 — 영구 씬 작업은 별도 씬에서 할 것.

### Testing Requirements
- 빌드 후 반드시 `Validate Playable Demo Scene`으로 참조 누락을 잡을 것(에셋/매니저/UI/EventSystem/Canvas 모두 검사).
- Play Mode 진입 후 Console 로그(`[BattleManager]`/`[EventManager]` 등)로 흐름 정상 여부 확인.

## Dependencies

### Internal
- `Scripts/Core/*`, `Scripts/Battle/*`, `Scripts/Event/*`, `Scripts/UI/*`, `Scripts/Data/*` — 거의 전 계층 참조(씬을 조립하므로).

### External
- `UnityEditor`, `UnityEditor.SceneManagement`, `TMPro`, `UnityEngine.UI`, `UnityEngine.InputSystem.UI`.

<!-- MANUAL: -->

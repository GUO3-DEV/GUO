<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-06-23 | Updated: 2026-06-23 -->

# UI Prefabs (UI 버튼 프리팹)

## Purpose
전투/이벤트 UI에서 동적 생성에 사용하는 버튼 프리팹. `DemoSceneBuilder`가 생성하며,
해당 UI 컴포넌트의 `[SerializeField]` 프리팹 필드에 연결된다.

## Key Files
| 파일 | 소비자 | 설명 |
|------|--------|------|
| `SkillButton.prefab` | `BattleUI.skillButtonPrefab` | 전투 중 스킬 버튼 동적 생성. 라벨에 스킬명(또는 쿨다운 표시). |
| `ItemButton.prefab` | `BattleUI.itemButtonPrefab` | 전투 중 아이템 버튼 동적 생성. 라벨에 아이템명. |
| `ChoiceButtonItem.prefab` | `EventUI.choiceButtonPrefab` | 이벤트 선택지 버튼. `ChoiceButtonItem` 컴포넌트 + 선택 텍스트/힌트 텍스트. |

## For AI Agents

### Working In This Directory
- **생성 주체는 코드**: 이 프리팹들의 구조(레이아웃/색/컴포넌트)는 `DemoSceneBuilder.Button`/`CreateChoicePrefab`에 하드코딩. 에디터에서 수동 수정해도 재빌드 시 덮어씌워짐.
- **라벨 자동 설정**: 동적 생성 시 `SetButtonLabel`(`BattleUI`)이 자식 `TextMeshProUGUI`를 찾아 라벨을 설정. 프리팹에 TMP 텍스트 자식이 있어야 동작.
- **`ChoiceButtonItem` 매칭**: `ChoiceButtonItem.prefab`은 `button`/`choiceText`/`hintText` private 필드를 가져야 하며 빌더가 `SetPrivate`로 채운다. 컴포넌트 구조를 바꾸면 빌더 동기화 필수.

## Dependencies

### Internal
- `Scripts/UI/BattleUI`·`EventUI`·`ChoiceButtonItem`(소비), `Editor/DemoSceneBuilder`(생성).

<!-- MANUAL: -->

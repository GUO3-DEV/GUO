<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-06-23 | Updated: 2026-06-23 -->

# Scenes (Unity 씬)

## Purpose
Unity 씬 파일을 보관. 현재 단일 씬만 존재하며, 데모가 모든 기능(이벤트 + 전투)을 한 씬에서 구동한다.
`SceneController`는 Boot/Town/Dungeon/Battle 씬 분리를 전제로 설계되었으나 아직 미분리 상태.

## Key Files
| 파일 | 설명 |
|------|------|
| `SampleScene.unity` | 유일한 씬. `DemoSceneBuilder.RebuildPlayableDemoScene`이 Systems(GameManager/BattleManager/EventManager/Bootstrap) + 카메라 + EventSystem + Canvas(BattleUI/EventUI)로 완전 재구성한다. |

## For AI Agents

### Working In This Directory
- **수동 편집 주의**: `SampleScene.unity`은 빌더로 재생성 시 `ClearScene`이 내용을 모두 비운다. 영구 씬 작업은 별도 씬(예: `Town.unity`)을 만들 것.
- **씬 분리 로드맵**: `SceneController.SceneName`(Boot/Town/Dungeon/Battle) 상수가 존재하지만 씬 파일은 없음. 분리 시 (1) 씬 파일 생성 (2) Build Settings에 등록 (3) 이름이 `SceneName` 상수와 일치 — 3단계 필요.
- **씬은 텍스트가 아님**: `.unity`는 YAML 직렬화 포맷. 직접 텍스트 편집보다 Unity 에디터 또는 빌더 도구로 조작.

## Dependencies

### Internal
- `Editor/DemoSceneBuilder`(생성/검증), `Scripts/Core/SceneController`(전환, TODO 연결).

<!-- MANUAL: -->

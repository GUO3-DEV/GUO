<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-06-23 | Updated: 2026-06-23 -->

# Prefabs (재사용 프리팹)

## Purpose
씬에 인스턴스화하여 재사용하는 UI 프리팹을 보관. 전투/이벤트 UI에서 동적으로 버튼을 생성할 때 사용된다.
현재는 데모 UI용 버튼 프리팹만 존재하며, `Editor/DemoSceneBuilder`가 생성한다.

## Subdirectories
| 디렉터리 | 목적 |
|-----------|------|
| `UI/` | UI 버튼 프리팹 (see `UI/AGENTS.md`) |

## For AI Agents

### Working In This Directory
- **빌더가 생성**: 현재 프리팹은 `DemoSceneBuilder.CreateButtonPrefab`/`CreateChoicePrefab`이 코드로 생성·저장한다. 프리팹 구조를 바꾸면 다음 빌드에서 덮어씌워질 수 있음 — 영구 변경은 빌더 코드를 수정.
- **참조 무결성**: UI 스크립트(`BattleUI.skillButtonPrefab` 등)가 이 프리팹을 참조. 프리팹 삭제/이동 시 UI 연결 끊김 — `ValidatePlayableDemoScene`으로 검증 가능.

<!-- MANUAL: -->

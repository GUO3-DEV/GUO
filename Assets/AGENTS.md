<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-06-23 | Updated: 2026-06-23 -->

# Assets (Unity 에셋 루트)

## Purpose
Unity 프로젝트의 모든 게임 콘텐츠 루트. 소스 코드(`Scripts/`), 데이터 에셋(`Data/`), 에디터 도구(`Editor/`),
프리팹(`Prefabs/`), 씬(`Scenes/`), 렌더링 설정(`Settings/`), 폰트(`Fonts/`)가 여기에 위치한다.
Unity가 관리하는 폴더이므로 `.meta` 파일은 버전 관리에 함께 포함해야 참조가 깨지지 않는다.

## Subdirectories
| 디렉터리 | 목적 |
|-----------|------|
| `Scripts/` | 모든 C# 게임 코드 (see `Scripts/AGENTS.md`) |
| `Data/` | ScriptableObject 데이터 에셋(런타임 데이터가 아닌 콘텐츠 정의) (see `Data/AGENTS.md`) |
| `Editor/` | Unity 에디터 전용 도구(데모 씬 빌더 등) (see `Editor/AGENTS.md`) |
| `Prefabs/` | 재사용 프리팹 (see `Prefabs/AGENTS.md`) |
| `Scenes/` | Unity 씬 파일 (see `Scenes/AGENTS.md`) |
| `Settings/` | URP 2D 렌더링 설정 (see `Settings/AGENTS.md`) |
| `Fonts/` | 폰트 파일 및 TMP 폰트 에셋 (see `Fonts/AGENTS.md`) |
| `TextMesh Pro/` | TMP 표준 리소스(자동 생성). 문서 생략 — 건드리지 말 것. |

## Key Files
| 파일 | 설명 |
|------|------|
| `README.md` | 프로젝트 상세 가이드(약 520줄). 구조·규칙·확장 방법 정리. |
| `InputSystem_Actions.inputactions` | Unity Input System 기본 액션 맵. |
| `DefaultVolumeProfile.asset` | URP 볼륨 프로필 기본값. |
| `UniversalRenderPipelineGlobalSettings.asset` | URP 글로벌 설정. |

## For AI Agents

### Working In This Directory
- **`.meta` 파일 필수**: 모든 에셋/스크립트는 `.meta`가 쌍으로 존재하며 GUID를 보관한다. 파일 이동/삭제 시 `.meta`도 함께 처리하지 않으면 참조가 끊어진다. 절대 수동으로 GUID를 변경하지 말 것.
| **데이터는 `Data/`, 코드는 `Scripts/Data/`**: `Scripts/Data/`는 데이터 *클래스 정의*(SO 스키마), `Assets/Data/`는 그 클래스의 *인스턴스 에셋*. 헷갈리지 말 것.
- **코드 재빌드**: `.cs` 수정 후에는 Unity 에디터가 자동 재컴파일. 에디터가 닫혀있으면 변경 사항이 씬에 반영되지 않을 수 있음.
- **데모 재구성 필요 시**: 에셋 구조를 크게 바꾸면 `RoguelikeRPG/Demo/Rebuild Playable Demo Scene`(`Editor/DemoSceneBuilder`)로 재생성.

### Common Patterns
- 모든 디렉터리는 한글/영문 혼용 네이밍. SO 에셋은 `Type_Name` 규칙(예: `Monster_DemoSlime.asset`).
- 인스펙터 노출용 `[Header]/[Tooltip]` 메타데이터를 적극 사용(기획자/UI 담당 친화).

## Dependencies

### External
- Unity 6, URP 2D, TextMeshPro, Input System, 2D Animation/Tilemap 패키지.

<!-- MANUAL: -->

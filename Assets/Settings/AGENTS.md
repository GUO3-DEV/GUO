<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-06-23 | Updated: 2026-06-23 -->

# Settings (URP 2D 렌더링 설정)

## Purpose
Universal Render Pipeline(URP) 2D 설정 에셋과 씬 템플릿을 보관. 프로젝트의 렌더링 파이프라인 동작을 정의한다.
Unity/URP가 관리하는 에셋이므로 통상적으로 수동 수정하지 않는다.

## Key Files
| 파일 | 설명 |
|------|------|
| `UniversalRP.asset` | URP 렌더 파이프라인 에셋(프로젝트에 할당된 마스터 설정). |
| `Renderer2D.asset` | 2D 렌더러 데이터(라이트 2D, 정렬 등). |
| `DefaultVolumeProfile.asset` | 루트(`Assets/`)에 있는 볼륨 프로필 기본값. |
| `Lit2DSceneTemplate.scenetemplate` | 새 2D 씬 생성 템플릿. |
| `Scenes/URP2DSceneTemplate.unity` | 씬 템플릿 기반 씬. |

## For AI Agents

### Working In This Directory
- **자동 관리 에셋**: URP 설정은 Unity 에디터(Project Settings > Graphics/URP)에서 변경. 텍스트 편집 금지.
- **할당 확인**: `UniversalRP.asset`은 ProjectSettings의 Graphics 설정에 할당되어야 렌더링이 정상 동작.

## Dependencies

### External
- Unity URP 2D 패키지.

<!-- MANUAL: -->

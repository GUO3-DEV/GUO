<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-06-23 | Updated: 2026-06-23 -->

# Fonts (폰트)

## Purpose
한국어 표시용 폰트 소스 파일과 TextMeshPro 폰트 에셋을 보관. 데모 UI 텍스트는 이 폰트로 렌더링된다.

## Key Files
| 파일 | 설명 |
|------|------|
| `malgun.ttf` | 맑은 고딕 원본 TTF. Windows 시스템 폰트에서 `DemoSceneBuilder.GetDemoFont`가 복사. |
| `MalgunGothic.asset` | `malgun.ttf`에서 생성한 TMP 폰트 에셋(Dynamic atlas, SDFAA). 데모 UI의 모든 `TextMeshProUGUI`가 사용. |

## For AI Agents

### Working In This Directory
- **자동 생성**: `MalgunGothic.asset`은 `DemoSceneBuilder.GetDemoFont`가 `TMP_FontAsset.CreateFontAsset`로 생성(손상 시 삭제 후 재생성 로직 포함). 수동으로 에셋을 바꾸면 재빌드 시 덮어씌워질 수 있음.
- **한국어 깨짐 주의**: TMP 폰트가 Dynamic atlas 모드라 글리프가 요청 시 생성됨. 폰트 교체 시 모든 한국어 문자가 렌더링되는지 확인.
- **EventUI 인코딩 참고**: `Scripts/UI/EventUI.cs`의 주석이 mojibake — 폰트 문제가 아니라 소스 파일 인코딩 문제(`UI/AGENTS.md` 참고).

## Dependencies

### Internal
- `Editor/DemoSceneBuilder`(생성), `Scripts/UI/*`(소비).

<!-- MANUAL: -->

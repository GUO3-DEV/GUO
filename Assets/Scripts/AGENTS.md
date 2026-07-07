<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-06-23 | Updated: 2026-06-23 -->

# Scripts (게임 코드 루트)

## Purpose
모든 C# 게임 로직 컨테이너. 3-레이어 단방향 아키텍처를 따른다:
`UI(Presentation)` → `Core/Battle/Event(Domain Manager)` → `Data(Data)`.
네임스페이스는 `RoguelikeRPG.{Core|Battle|Event|Data|UI}`로 계층별 분리.

## Subdirectories
| 디렉터리 | 계층 | 목적 |
|-----------|------|------|
| `Core/` | 인프라 | 게임 상태 머신·씬 전환·RNG (see `Core/AGENTS.md`) |
| `Battle/` | 도메인 | 턴제 전투 시스템 (see `Battle/AGENTS.md`) |
| `Event/` | 도메인 | 던전 이벤트/선택지 시스템 (see `Event/AGENTS.md`) |
| `Data/` | 데이터 | 데이터 클래스 정의(SO 스키마 + RuntimeStats) (see `Data/AGENTS.md`) |
| `UI/` | 프레젠테이션 | Presenter(BattleUI/EventUI) (see `UI/AGENTS.md`) |

## For AI Agents

### Working In This Directory
- **계층 의존성 규칙**: 의존은 `UI → Core/Battle/Event → Data` 방향만 허용. `Data` 계층에서 상위를 참조하거나 `Battle`↔`Event` 직접 참조 금지(루트 AGENTS.md 안티패턴). Manager 간 통신은 `GameManager` 경유 또는 C# event.
- **네임스페이스 준수**: 새 클래스는 소속 디렉터리의 네임스페이스 사용. Battle/Skills만 `RoguelikeRPG.Battle.Skills`로 중첩.
- **MonoBehaviour vs 순수 C# 구분**: Manager/UI는 `MonoBehaviour`(씬에 배치), `BattleEntity`/`Player`/`Monster`/`ChoiceHandler`/`RNGSystem`은 순수 C#(`new`로 생성).
- **TODO 항목 다수**: 여러 파일에 미구현 스텁(상태 전환·스킬 로딩·보스 패턴·결과 텍스트 등)이 TODO로 표시됨. 루트 AGENTS.md의 ANTI-PATTERNS/KNOWN TRADE-OFFS 참고.

### Common Patterns
- `// ────` 구분선 + `///` Korean XML doc comments.
- C# `event Action<T>`로 UI에 상태 전파(단방향).
- Singleton(`Instance` + `DontDestroyOnLoad`) 보일러플레이트.
- 코루틴 기반 비동기.

## Dependencies

### External
- `UnityEngine`, uGUI, TextMeshPro.

<!-- MANUAL: -->

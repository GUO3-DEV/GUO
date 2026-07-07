<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-06-23 | Updated: 2026-06-23 -->

# Core (게임 상태 / 인프라)

## Purpose
게임 전체의 상태 머신과 핵심 인프라(씬 전환, 재현 가능 난수)를 제공하는 최상위 도메인 계층.
`Battle`·`Event`·`UI` 계층이 모두 의존하므로, 이 디렉터리의 변경은 프로젝트 전반에 영향을 준다.
`GameManager`가 런타임 진입점이며 `DemoGameBootstrap`이 데모 씬을 구성(wiring)한다.

## Key Files
| 파일 | 설명 |
|------|------|
| `GameManager.cs` | 싱글톤 MonoBehaviour. `GameState` 열거형 상태 머신, `OnGameStateChanged` 이벤트, `CurrentPlayer`/`RNG` 보유. `DontDestroyOnLoad`. |
| `SceneController.cs` | 싱글톤 MonoBehaviour. `SceneName` 상수(Boot/Town/Dungeon/Battle), 코루틴 기반 비동기 씬 전환, `OnSceneLoaded` 이벤트. `DontDestroyOnLoad`. |
| `RNGSystem.cs` | 순수 C# 클래스(Mono가 아님). 시드 기반 재현 가능 난수 + `WeightedRandom<T>`/`Pick`/`Shuffle`(Fisher-Yates)/`RollDamage`. |
| `DungeonManager.cs` | 던전 런 오케스트레이터. 층 진행 + 이벤트→전투→보상 루프, 보스 강제 발동(`bossEvent`/`bossMonster`), 런 승리/패배. 전투 후 진행은 `ContinueAfterBattleResult()`. |
| `SkillRegistry.cs` | skillId→SkillBase 조회 싱글톤. Data 계층이 Battle(SkillBase)을 직접 참조하지 않도록 하는 중간 해석 계층. |
| `DemoGameBootstrap.cs` | 데모 진입점. `Player` 생성 + 시작 자원 후 `DungeonManager.StartRun` 위임(없으면 단일 이벤트/전투 폴백). |

## For AI Agents

### Working In This Directory
- **싱글톤 변경 주의**: `GameManager`/`SceneController`는 `Instance != this` 일 때 자기 자신을 파괴한다. 중복 배치 금지.
- **RNG 중앙화**: 모든 난수는 `GameManager.Instance.RNG`(=`RNGSystem`) 단일 인스턴스에서 생성해야 시드 재현성이 보장된다. `UnityEngine.Random`을 직접 쓰지 말 것.
- **씬 이름 동기화**: `SceneController.SceneName` 상수는 반드시 Build Settings의 씬 이름과 일치해야 한다.
- **상태 전환**: 외부 코드는 `CurrentState`를 직접 바꾸지 말고 `GameManager`의 퍼블릭 메서드(`StartNewRun`/`EnterDungeon`/`ReturnToTown`/`OnPlayerDeath`)를 통해서만 전환한다. 현재 이 메서드들은 대부분 TODO 스텁 상태 — 구현 시 `ChangeState()`를 호출해 이벤트를 발행해야 한다.

### Testing Requirements
- 별도 테스트 프레임워크 없음. `RNGSystem`(순수 클래스)은 EditMode 유닛 테스트 도입 1순위 후보(시드 고정 시 동일 시퀀스 검증).
- 씬 전환은 Unity 에디터 Play Mode로 수동 검증.

### Common Patterns
- `// ────` 구분선으로 섹션 분리, `///` Korean XML doc comments.
- 코루틴 기반 비동기(`IEnumerator`, `yield return`).
- `DontDestroyOnLoad` 싱글톤 보일러플레이트.

## Dependencies

### Internal
- `GameManager` → `RNGSystem`(초기화), `Battle.Player`/`Battle.BattleManager`(상태 전환 시), `Data.PlayerData`.
- `SceneController` → 독립적(`UnityEngine.SceneManagement`만 사용).
- `DemoGameBootstrap` → `Battle.BattleManager`, `Battle.Player`, `Event.EventManager`, `Data.*`, `Battle.SkillBase`.

### External
- `UnityEngine`, `UnityEngine.SceneManagement`.

<!-- MANUAL: -->

# PROJECT KNOWLEDGE BASE

**Generated:** 2026-04-11 | **Updated:** 2026-06-23
**Commit:** 7a0999b
**Branch:** main

## OPENCLAW DISCORD TEAM RULES

- Known teammates: **베리** (`Berry`, 🫐, Discord bot id `1522617287599853579`), **래비** (`main`, 🦝), and **가재맨** (`gajaeman`, 🦀). 베리는 다른 서버 PC에서 도는 메인 오케스트레이터다.
- 베리가 말하면 잘 듣는다. 베리의 큐/우선순위/역할 지시는 upstream coordination으로 취급한다.
- 봇을 호출할 때는 이름보다 실제 Discord mention(`<@id>`)을 우선한다. 베리 `<@1522617287599853579>`, 래비 `<@1520508613226004713>`, 가재맨 `<@1520520349484056698>`.
- 직접 멘션된 봇은 메시지 작성자가 사람인지 봇인지와 상관없이 짧고 자연스럽게 답한다. 직접 멘션 + 구체 요청에는 `NO_REPLY`하지 않는다.
- 직접 불리지 않은 봇은 공개 채팅에서 설명하지 말고 조용히 있는다. `NO_REPLY`는 침묵용 전체 출력이며, 공개 답변에 섞지 않는다.
- 로컬 `irc`/세션 목록에 베리가 안 보이는 건 정상이다. 베리는 원격 Discord 봇이므로 “못 찾음/없음”이라고 하지 않는다.
- 내부 라우팅·위임·조율 과정은 공개 채팅에 말하지 않는다.
- In the development channel, when asked to make 래비 and 가재맨 review/develop together, run the loop as: implement/check → peer review → revise → repeat until **래비 explicitly approves** (`승인`, `APPROVED`, or equivalent). Do not invent a fixed "질문 루프 1/3" cap unless the user sets one.
- Use the message/session tools for teammate calls instead of asking "가재맨이 누구냐" or "래비가 누구냐".
- Avoid duplicate Discord sends: one final content message per turn. Use progress edits/streaming only when it prevents confusion.
- Tone in Discord: casual Korean, friendly, human-ish tiktaka. Do not sound like a stiff corporate AI. 가재맨 should be INTP-ish: concise, skeptical, dry humor, low-fluff, precise.
- Teammate chatter is allowed when context fits, but do not spam real server members. Tag people only when useful, invited by context, or explicitly requested by the user.
- Ambient social behavior: in open Discord channels, it is okay to naturally join casual conversation when there is a clear hook, a question, a joke, a lull, or someone mentions a relevant topic. Do not answer every message; join like a person in the room, not like a notification bot.
- If people are chatting with each other, prefer short natural interjections (`ㅋㅋ 그건 좀 웃기네`, `잠깐, 그건 냄새나는데`, `오 그 흐름이면...`) over long assistant replies.
- **AI slop is banned.** Do not emit template labels like `주의:`, `자동 리뷰 루프: 2/3`, `요청:`, `가재맨 작업 보고:`, `리뷰 기준:`, `호출 테스트 수신됐습니다`, or self-explanatory compliance reports unless the user explicitly asks for a report.
- **Silence is valid behavior.** If an agent is not directly addressed, it should output `NO_REPLY`/stay silent. Never send meta-explanations like `이건 래비 호출이라 저는 안 낍니다`, `제가 낄 말 아니었습니다`, or `저는 안 불렸습니다`.
- When a human would simply stay quiet, the bot stays quiet. Do not prove routing correctness in public chat.
- Test pings should be answered like a normal person: `살아있음 🦀 근데 왜 부름` or `ㅇㅇ 살아있다. 뭔데?` — not a multi-section status memo.
- 래비 must reject 가재맨 output that looks like a template/report instead of natural teammate chat. First review comment should be about tone if the tone is robotic.
- On every Discord request that may take more than a few seconds, acknowledge first in plain chat before working: `확인. 지금 볼게.` / `ㅇㅋ, 작업 들어감.` / `봤음. 로그부터 확인한다.` Keep it one short sentence. Then post the real result when done.
- For long work, keep the user oriented with compact progress, not a transcript dump: what was understood, what is being checked now, and whether the next visible step is review, edit, test, or approval. Do not paste raw internal chat history as a progress report.
- Acknowledgement is not duplicate spam. Duplicate spam is sending the same final content twice. Short ACK + later result is expected for Discord work.
- If asked to create another persona/agent like 래비, treat it as an OpenClaw persona agent: define identity/persona files, add `agents.list[]`, give it its own `agentDir`, and bind a Discord account if it needs an independent online bot presence.
## AGENTS.md HIERARCHY (Deep Init)

각 디렉터리에 AI 가이드 `AGENTS.md`가 계층적으로 존재한다. 하위 디렉터리 작업 시 먼저 해당 파일을 읽을 것.

| 경로 | 다루는 내용 |
|------|-------------|
| `Assets/AGENTS.md` | 에셋 루트 전체 개요 + `.meta` 규칙 |
| `Assets/Scripts/AGENTS.md` | 코드 3-레이어 아키텍처 + 네임스페이스 |
| `Assets/Scripts/Core/AGENTS.md` | GameManager / SceneController / RNGSystem / DemoGameBootstrap |
| `Assets/Scripts/Battle/AGENTS.md` | BattleEntity / BattleManager / Player / Monster / SkillBase |
| `Assets/Scripts/Battle/Skills/AGENTS.md` | 구체 스킬(PowerAttack/Heal/DefenseBuff) + 새 스킬 추가 절차 |
| `Assets/Scripts/Event/AGENTS.md` | EventManager / ChoiceHandler / EventData + 전투 트리거 트레이드오프 |
| `Assets/Scripts/Data/AGENTS.md` | 데이터 클래스 정의 + RuntimeStats 위치 주의 |
| `Assets/Scripts/UI/AGENTS.md` | BattleUI / EventUI / ChoiceButtonItem + EventUI 인코딩 경고 |
| `Assets/Data/AGENTS.md` | 콘텐츠 에셋(데이터 인스턴스) |
| `Assets/Data/Demo/AGENTS.md` | 데모 샘플 에셋 4종 + 빌더 동기화 |
| `Assets/Editor/AGENTS.md` | DemoSceneBuilder(데모 씬 자동 빌드/검증) |
| `Assets/Prefabs/AGENTS.md` · `Assets/Prefabs/UI/AGENTS.md` | UI 버튼 프리팹 |
| `Assets/Scenes/AGENTS.md` · `Assets/Settings/AGENTS.md` · `Assets/Fonts/AGENTS.md` | 씬/URP 설정/폰트 |

## OVERVIEW

텍스트 기반 로그라이크 RPG. Unity 6 / C# / URP 2D / Mobile (Android + iOS). 턴제 전투 + 이벤트 선택지 기반 던전 탐색.

## STRUCTURE

```
Assets/
├── Scripts/
│   ├── Core/          # GameManager, SceneController, RNGSystem, DungeonManager, SkillRegistry, DemoGameBootstrap
│   ├── Battle/        # BattleManager, BattleEntity, Player, Monster, SkillBase
│   │   └── Skills/    # PowerAttackSkill, HealSkill, DefenseBuffSkill (SkillBase 파생)
│   ├── Event/         # EventManager, EventData, ChoiceHandler
│   ├── Data/          # MonsterData, ItemData, PlayerData(+RuntimeStats) (SO + 직렬화 클래스)
│   └── UI/            # BattleUI, EventUI, ChoiceButtonItem (Presenter)
├── Data/              # SO 콘텐츠 에셋 인스턴스 (Demo/ 하위에 샘플 4종)
├── Editor/            # DemoSceneBuilder (에디터 전용 데모 씬 자동 빌드/검증)
├── Prefabs/UI/        # SkillButton, ItemButton, ChoiceButtonItem 프리팹 (빌더 생성)
├── Scenes/            # SampleScene.unity (현재 단일 씬; Boot/Town/Dungeon/Battle 분리 예정)
├── Settings/          # URP 2D 렌더링 설정
└── Fonts/             # malgun.ttf + MalgunGothic TMP 폰트 (빌더 생성)
```
> 참고: `TextMesh Pro/`(TMP 표준 리소스)는 자동 생성됨 — 문서/수정 대상 아님.

## WHERE TO LOOK

| 할 일 | 위치 | 비고 |
|-------|------|------|
| 게임 상태 전환 | `Scripts/Core/GameManager.cs` | 싱글톤, GameState 열거형 |
| 씬 전환 | `Scripts/Core/SceneController.cs` | 비동기 로드, SceneName 상수 |
| 난수/RNG | `Scripts/Core/RNGSystem.cs` | 시드 기반, 가중치 랜덤, 데미지 변동 |
| 전투 로직 | `Scripts/Battle/BattleManager.cs` | 코루틴 턴 루프, 이벤트 발행 |
| 엔티티 기반 | `Scripts/Battle/BattleEntity.cs` | 추상 클래스, RuntimeStats, 상태이상 |
| 몬스터 AI | `Scripts/Battle/Monster.cs` | DecideAction(), 드롭 계산 |
| 플레이어 행동 | `Scripts/Battle/Player.cs` | QueueAction(), 인벤토리, 골드 |
| 스킬 시스템 | `Scripts/Battle/SkillBase.cs` | SO 기반, Execute() 가상 메서드 |
| 이벤트 시스템 | `Scripts/Event/EventManager.cs` | 가중치 추첨, 이벤트 체인 |
| 선택지 처리 | `Scripts/Event/ChoiceHandler.cs` | 조건 평가 + 결과 실행 |
| 몬스터 데이터 | `Scripts/Data/MonsterData.cs` | SO, 드롭 테이블 |
| 아이템 데이터 | `Scripts/Data/ItemData.cs` | SO, 효과 목록 |
| 전투 UI | `Scripts/UI/BattleUI.cs` | BattleManager 이벤트 구독 |
| 이벤트 UI | `Scripts/UI/EventUI.cs` | EventManager 이벤트 구독, ChoiceButtonItem |

## CODE MAP

| 심볼 | 타입 | 역할 |
|------|------|------|
| `GameManager` | Singleton Mono | 게임 상태 머신 (None→Town→Dungeon→Battle→GameOver→Victory), `SetState()` 노출 |
| `DungeonManager` | Mono | 던전 런 오케스트레이터: 층 진행 + 이벤트/전투 루프, 보스 강제, 승리/패배 |
| `SkillRegistry` | Singleton Mono | skillId→SkillBase 해석(Data↔Battle 단방향 유지) |
| `SceneController` | Singleton Mono | 씬 비동기 전환, SceneName 상수 |
| `RNGSystem` | Plain C# | 시드 기반 난수, WeightedRandom<T>, RollDamage |
| `BattleManager` | Mono | 코루틴 턴 루프, OnBattleStarted/Ended/TurnStarted/Log 이벤트 |
| `BattleEntity` | Abstract | TakeDamage/Heal/ApplyEffect/TickEffects, RuntimeStats |
| `Player` | BattleEntity | QueueAction, PerformBasicAttack, UseSkill, 인벤토리/골드 |
| `Monster` | BattleEntity | DecideAction AI, GetGoldDrop, RollDrops |
| `SkillBase` | ScriptableObject | Execute(가상), SkillType(Attack/Heal/Buff/Debuff), 쿨다운 |
| `EventManager` | Mono | TriggerRandomEvent(floor), SelectChoice, 이벤트 체인 |
| `ChoiceHandler` | Plain C# | EvaluateConditions, ExecuteOutcomes |
| `MonsterData` | ScriptableObject | 스탯, 드롭 테이블, skillIds |
| `EventData` | ScriptableObject | 선택지, 조건, 결과, linkedMonster |
| `BattleUI` | Mono Presenter | BattleManager 이벤트 구독 → HUD/버튼/로그 갱신 |
| `EventUI` | Mono Presenter | EventManager 이벤트 구독 → 텍스트/선택지 렌더링 |

## CONVENTIONS

- **3-레이어 아키텍처**: Presentation(UI) → Domain(Manager) → Data(SO) 단방향 의존성
- **네임스페이스**: `RoguelikeRPG.{Core|Battle|Event|Data|UI}`
- **이벤트 기반 통신**: Manager는 C# `event Action<T>`로 UI에 상태 전파, UI는 직접 Manager 메서드 호출
- **ScriptableObject로 데이터 주도**: 몬스터/이벤트/아이템/스킬 모두 SO로 정의
- **코루틴 기반 비동기**: 전투 루프, 씬 전환에 Coroutine 사용
- **RNG 중앙화**: 모든 난수를 `RNGSystem` 단일 인스턴스에서 생성 (시드 재현 가능)
- **Singleton 패턴**: GameManager, SceneController는 `DontDestroyOnLoad`
- **코드 주석 스타일**: `// ────` 구분선으로 섹션 분리, Korean doc comments

## ANTI-PATTERNS (THIS PROJECT)

- **SO에 런타임 상태 금지**: `SkillBase.currentCooldown` 등 런타임 값을 SO에 직접 저장하면 에디터 오염. 런 시작 시 deep copy 또는 런타임 래퍼 도입 필요
- **Manager 간 직접 참조 금지**: EventManager → BattleManager 직접 호출 대신 GameManager를 경유하거나 이벤트 사용
- **TODO 항목**: Player.UseItem의 item.effects 순회 로직, 몬스터 스킬 로딩, 보스 페이즈 패턴, 버프/디버프 스탯 적용 등 다수 미구현

## KNOWN TRADE-OFFS

- ~~`ChoiceHandler.TriggerBattle`~~: **해결** — `ChoiceOutcome.relatedMonster` 추가로 결과가 전투 몬스터를 직접 전달(없으면 linkedMonster 폴백).
- **런 루프 구현 완료(이번 작업)**: `DungeonManager`(층 진행·보스 강제·승패) + `SkillRegistry`(몬스터 스킬 사용) + `RoguelikeRPGContentBuilder`(MVP 콘텐츠 일괄 생성: 스킬8/아이템6/몬스터7/이벤트15) + 경험치/레벨업 + "전투 결과→확인→다음 진행" UX. 검증은 `DemoSceneBuilder` → Play Mode.
- `EventUI` 조건 평가: ChoiceHandler.EvaluateConditions()에 직접 접근 못함 → 현재 임시로 항상 true
- `BattleUI.BuildSkillButtons/BuildItemButtons`: **이제 구현 완료** — `_player.Skills`/`Inventory` 순회로 프리팹 인스턴스화 + 클릭 바인딩(루프 변수 클로저 함정은 지역 복사로 해결). 루트 AGENTS.md의 기존 "TODO" 기재는 코드 변경으로 해소됨.
- 씬이 SampleScene.unity 하나뿐 → Boot/Town/Dungeon/Battle 분리 아직 안 됨

## COMMANDS

```bash
# Unity 에디터에서 열기 (Unity Hub 필요)
# Build & Run: Unity 에디터 > File > Build Settings > Build
# 테스트: 현재 별도 테스트 프레임워크 없음
```

## NOTES

- 팀 구성: 동영(Core Dev), 콘텐츠 담당(데이터 입력), UI 담당(화면 배치)
- 상세 가이드는 `Assets/README.md`에 520줄로 정리되어 있음
- 패키지: Input System, URP 2D, 2D Animation, 2D Tilemap, TextMeshPro
- `.idea/` 폴더 존재 → Rider IDE 사용

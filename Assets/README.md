# 텍스트 기반 로그라이크 RPG - 코어 엔진 구조

Unity 6 / C# / Mobile (Android + iOS)
작성일: 2026-03-02
담당: 동영 (1~2주차 코어 엔진)

---

## 목차

1. [폴더 구조](#폴더-구조)
2. [씬 구성](#씬-구성)
3. [아키텍처 개요](#아키텍처-개요)
4. [레이어별 파일 설명](#레이어별-파일-설명)
5. [데이터 흐름](#데이터-흐름)
6. [ScriptableObject 에셋 생성 가이드](#scriptableobject-에셋-생성-가이드)
7. [콘텐츠 담당자 협업 가이드](#콘텐츠-담당자-협업-가이드)
8. [핵심 루프 실행 순서](#핵심-루프-실행-순서)
9. [TODO / 확장 포인트](#todo--확장-포인트)

---

## 폴더 구조

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs        # 싱글톤, 게임 상태 머신 (Town/Dungeon/Battle/Event/GameOver)
│   │   ├── SceneController.cs    # 페이드 인/아웃 씬 전환 담당
│   │   └── RNGSystem.cs          # 시드 기반 난수, 가중치 랜덤, 데미지 롤
│   ├── Battle/
│   │   ├── BattleEntity.cs       # Player/Monster 공통 기반 (스탯, 상태이상, 피해/회복)
│   │   ├── Player.cs             # 플레이어 엔티티 (인벤토리, 골드, 입력 큐)
│   │   ├── Monster.cs            # 몬스터 엔티티 (AI 행동 결정, 드롭 계산)
│   │   ├── SkillBase.cs          # 스킬 ScriptableObject 기반 클래스
│   │   ├── Skills/               # 구체적인 스킬 구현들
│   │   │   ├── PowerAttackSkill.cs      # 강공격 (공격력 1.5배)
│   │   │   ├── HealSkill.cs             # 회복 스킬 (최대 HP 30%)
│   │   │   └── DefenseBuffSkill.cs      # 방어 버프 (방어력 +5, 3턴)
│   │   └── BattleManager.cs      # 턴제 전투 루프, 이벤트 발행, 보상 지급
│   ├── Event/
│   │   ├── EventData.cs          # 이벤트 ScriptableObject (텍스트, 선택지, 결과)
│   │   ├── EventManager.cs       # 이벤트 풀 추첨, 선택지 처리 총괄
│   │   └── ChoiceHandler.cs      # 선택지 조건 평가 + 결과 실행 서비스
│   ├── Data/
│   │   ├── MonsterData.cs        # 몬스터 ScriptableObject (스탯, 드롭 테이블)
│   │   ├── ItemData.cs           # 아이템 ScriptableObject (효과, 희귀도, 가격)
│   │   └── PlayerData.cs         # 플레이어 기본 데이터 + RuntimeStats 구조체
│   └── UI/
│       ├── BattleUI.cs           # 전투 HUD, 버튼 → BattleManager 연결 Presenter
│       └── EventUI.cs            # 이벤트 텍스트/선택지 렌더링 Presenter
└── Data/                         # ScriptableObject 에셋 폴더
    ├── Monsters/
    ├── Items/
    ├── Events/
    └── Skills/
```

---

## 씬 구성

| 씬 이름   | 역할                                           | Build Index |
|-----------|------------------------------------------------|-------------|
| `Boot`    | 시스템 초기화, GameManager/SceneController 배치 | 0           |
| `Town`    | 마을 화면 (런 시작, 업그레이드, 상점)            | 1           |
| `Dungeon` | 던전 탐색, 이벤트 처리 (EventManager 배치)       | 2           |
| `Battle`  | 턴제 전투 (BattleManager, BattleUI 배치)         | 3           |

### 씬 배치 규칙

- **Boot 씬**: `GameManager`, `SceneController`를 DontDestroyOnLoad로 생성
- **Dungeon 씬**: `EventManager` 배치, 던전 층 진행 관리자(DungeonManager) 추가 예정
- **Battle 씬**: `BattleManager`, `BattleUI` 배치, 전투 종료 후 Dungeon 씬으로 복귀

---

## 아키텍처 개요

```
[Presentation Layer]      [Domain Layer]           [Data Layer]

BattleUI         ──────▶  BattleManager  ──────▶  MonsterData (SO)
EventUI          ──────▶  EventManager   ──────▶  EventData   (SO)
                           ChoiceHandler  ──────▶  ItemData    (SO)
                           GameManager    ──────▶  PlayerData
                           SceneController
                           RNGSystem
```

- **의존성 방향**: 항상 Presentation → Domain → Data (단방향)
- **ScriptableObject**: 모든 게임 데이터는 SO로 관리하여 기획자가 코드 없이 수정 가능
- **이벤트 기반 통신**: Manager는 C# event를 통해 UI에 상태를 전파 (강한 결합 방지)
- **RNGSystem 중앙화**: 모든 난수를 단일 시스템에서 생성하여 시드 재현 가능

---

## 레이어별 파일 설명

### Core 레이어

#### `GameManager.cs`
- 전체 게임 상태 머신 (Town → Dungeon → Battle → GameOver)
- 싱글톤, DontDestroyOnLoad
- `CurrentPlayer` (런타임 Player 인스턴스) 보유
- 상태 전환 시 `OnGameStateChanged` 이벤트 발행

#### `SceneController.cs`
- 페이드 인/아웃 포함 씬 비동기 전환
- `SceneName` 상수 클래스로 씬 이름 관리 (오타 방지)

#### `RNGSystem.cs`
- `SetSeed(int)`: 시드 설정 (같은 시드 → 같은 결과)
- `Roll(float)`: 확률 판정 (0.0~1.0)
- `WeightedRandom<T>()`: 가중치 랜덤 추첨
- `RollDamage(int, float)`: 데미지 변동 계산 (±10% 기본)

### Battle 레이어

#### `BattleEntity.cs` (추상 클래스)
- `RuntimeStats` 구조체: 버프 포함 최종 스탯 계산
- `TakeDamage()`: 방어력 차감, 최소 1 피해 보장
- `Heal()`: 최대 체력 초과 방지
- `ApplyEffect()` / `TickEffects()`: 상태이상 관리

#### `BattleManager.cs`
- `StartBattle(Player, MonsterData)`: 전투 시작
- `OnPlayerChooseBasicAttack()` / `OnPlayerChooseSkill()` / `OnPlayerUseItem()`: BattleUI 연결 포인트
- 코루틴 기반 턴 루프 (`BattleLoop`)
- 스피드 기반 선공 결정

### Event 레이어

#### `EventData.cs` (ScriptableObject)
- 이벤트 본문 텍스트, 선택지 목록, 배경 이미지
- `EventChoice`: 선택지 텍스트, 조건 목록, 결과 목록, 다음 이벤트 ID (체인)
- `ChoiceCondition`: HP%, 골드, 아이템 보유 조건
- `ChoiceOutcome`: HP 변화, 골드 변화, 아이템 지급/소모, 전투 발동

#### `EventManager.cs`
- 이벤트 풀 → 가중치 랜덤 추첨 → 이벤트 시작
- `SelectChoice(int)`: 선택지 선택 처리 (ChoiceHandler에 위임)
- 이벤트 체인: `nextEventId`로 연속 이벤트 진행

#### `ChoiceHandler.cs`
- `EvaluateConditions()`: 선택지 활성화 조건 평가
- `ExecuteOutcomes()`: 결과 목록 실행 (확률 판정 포함)

---

## 데이터 흐름

### 전투 시작 흐름

```
EventManager ──(TriggerBattle)──▶ BattleManager.StartBattle(player, monsterData)
                                         │
                            ┌────────────▼────────────┐
                            │   Monster 인스턴스 생성   │
                            │   스피드로 턴 순서 결정   │
                            └────────────┬────────────┘
                                         │ OnBattleStarted 이벤트
                                         ▼
                                      BattleUI
                                  (HUD 초기화, 버튼 구성)
```

### 턴 실행 흐름

```
BattleManager (코루틴)
  │
  ├─ 플레이어 턴
  │    ├── BattleUI에 행동 패널 표시 (OnTurnStarted 이벤트)
  │    ├── 플레이어 버튼 클릭 → BattleManager.OnPlayerChooseXxx()
  │    ├── _waitingForPlayerInput = false
  │    └── Player.TakeTurn(context) 실행
  │
  └─ 몬스터 턴
       ├── Monster.TakeTurn(context) 직접 실행
       └── DecideAction() → AI 행동 선택
```

### 이벤트 선택 흐름

```
EventUI ──(OnClickChoice)──▶ EventManager.SelectChoice(index)
                                      │
                          ChoiceHandler.EvaluateConditions() ──▶ [조건 미충족 → 차단]
                                      │
                          ChoiceHandler.ExecuteOutcomes()
                            (HP 변화, 골드, 아이템, 전투 발동)
                                      │
                          OnChoiceResolved 이벤트 ──▶ EventUI 결과 오버레이 표시
```

---

## ScriptableObject 에셋 생성 가이드

Unity 메뉴: **Assets > Create > RoguelikeRPG > Data > ...**

### MonsterData 생성 예시

```
monsterId:    "slime_basic"
displayName:  "슬라임"
maxHp:        30
attackPower:  8
defense:      2
speed:        6
expReward:    10
goldRewardRange: (3, 6)
dropTable:
  - item: [포션 ItemData 에셋]
    dropChance: 0.3
    quantityRange: (1, 1)
```

### EventData 생성 예시

```
eventId:   "event_abandoned_chest"
title:     "낡은 상자"
bodyText:  "먼지 쌓인 상자가 있다. 무언가 들어 있을지도 모른다."
choices:
  - choiceText: "열어본다"
    outcomes:
      - outcomeType: GainGold, value: 30, probability: 0.7
      - outcomeType: LoseHp, value: 10, probability: 0.3
  - choiceText: "그냥 지나친다"
    outcomes:
      - outcomeType: Nothing, probability: 1.0
```

### SkillBase 생성 예시 (기본 타입)

기본 스킬은 **코드 수정 없이** 인스펙터에서만 값을 입력하면 됩니다.

```
Assets > Create > RoguelikeRPG/Battle/Skill

skillId:          "basic_attack"
displayName:      "기본 공격"
skillType:        Attack
power:            1.0        # 공격력 1배
scaledByAttack:   true       # 공격력에 비례
cooldownTurns:    0          # 매 턴 사용 가능
```

---

## 콘텐츠 담당자 협업 가이드

### 팀 구성 및 담당 범위

| 담당자 | 역할 | 협업 방식 |
|--------|------|---------|
| **동영** (Core Dev) | 전투로직, 이벤트엔진, RNG, 프로젝트 구조 | ❌ 코드 작성 |
| **콘텐츠 담당** | 몬스터/이벤트/아이템/스킬 데이터 설계 | ✅ 인스펙터 입력 또는 간단한 코드 |
| **UI 담당** | 화면 배치, 버튼 연결, 연출 | ✅ Button OnClick, Canvas 레이아웃 |

### 1️⃣ 몬스터 설계 (코드 없음)

**위치**: `Assets > Create > RoguelikeRPG/Data/MonsterData`

| 항목 | 설명 | 기준값 |
|------|------|--------|
| **monsterId** | 영문 고유ID | `goblin_normal` |
| **displayName** | 화면 표시명 | `고블린` |
| **description** | 이벤트 설명 | `녹슨 단검을 든 고블린이 나타났다!` |
| **monsterType** | 등급 | Normal / Elite / Boss |
| **maxHp** | 최대 체력 | 20~50 (Normal), 80~120 (Elite) |
| **attackPower** | 공격력 | 5~10 (Normal), 15~25 (Elite) |
| **defense** | 방어력 | 1~3 (Normal), 5~8 (Elite) |
| **speed** | 스피드 (높을수록 먼저 행동) | 5~10 |
| **critChance** | 크리티컬 확률 | 0.05 = 5% |
| **expReward** | 처치 경험치 | 10~30 |
| **goldRewardRange** | 골드 범위 | (최소, 최대) |
| **dropTable** | 드롭 아이템 목록 | 아이템 SO 참조 + 확률 |
| **skillIds** | 스킬 ID 목록 | `["basic_attack", "power_attack"]` |

**MVP 목표**: Normal 몬스터 4종, Elite 1종

---

### 2️⃣ 스킬 추가 (2가지 방법)

#### 방법 1: 기본 타입으로 추가 (코드 없음) ⭐ 추천

**위치**: `Assets > Create > RoguelikeRPG/Battle/Skill`

**예시**:
```
skillId:        "power_attack"
displayName:    "강공격"
skillType:      Attack
power:          1.5          # 공격력 1.5배
cooldownTurns:  2            # 2턴 쿨다운
```

→ 이 파일을 MonsterData의 `skillIds`에 등록하면 완료

#### 방법 2: 특수 로직 추가 (간단한 코드)

PowerAttackSkill.cs를 **복사**해서 새 파일 만들기:

```csharp
// Scripts/Battle/Skills/YourSkillName.cs
public class YourSkillNameSkill : SkillBase
{
    protected override void ExecuteAttack(BattleEntity caster, BattleEntity target, BattleContext context)
    {
        // TODO: 여기만 수정하면 됩니다
        // 예: 낮은 HP일수록 더 강한 공격, 상대 행동 방해 등

        int baseDamage = Mathf.RoundToInt(caster.Stats.FinalAttack * 1.5f);
        target.TakeDamage(baseDamage);
    }
}
```

그 다음:
1. `[CreateAssetMenu]` 데코레이터의 menuName 수정
2. `Assets > Create > RoguelikeRPG/Battle/Skill_YourSkill` 메뉴 자동 생성
3. 인스펙터에서 데이터 입력

**제공되는 예제 스킬**:
- `PowerAttackSkill.cs`: 공격력 1.5배 강공격
- `HealSkill.cs`: 최대 HP 30% 회복
- `DefenseBuffSkill.cs`: 방어력 +5 (3턴)

---

### 3️⃣ 이벤트 설계 (코드 없음)

**위치**: `Assets > Create > RoguelikeRPG/Data/EventData`

**주요 항목**:

| 항목 | 설명 |
|------|------|
| **eventId** | `mystery_altar` (영문 고유ID) |
| **eventType** | Random (선택지), Battle (전투), Rest (회복), Treasure (보물) |
| **title** | "신비한 제단" (화면 제목) |
| **bodyText** | 이벤트 본문 (플레이어가 읽는 부분) |
| **spawnWeight** | 1.0 (1.0보다 크면 자주 나옴, 작으면 드물게) |
| **choices** (배열) | 선택지 목록 |

**선택지 구성**:

```
choiceText:     "접근한다"
conditions:     [조건 목록 - 선택 가능 조건]
outcomes:       [결과 목록 - 선택 시 일어나는 일]
nextEventId:    "follow_up_event" (다음 이벤트 ID, 체인)
```

**조건 종류**:
- `MinHpPercent`: 현재 체력이 X% 이상일 때만 선택 가능
- `MaxHpPercent`: 현재 체력이 X% 이하일 때만 선택 가능
- `MinGold`: 골드가 X 이상일 때만 선택 가능
- `HasItem`: 특정 아이템을 보유했을 때만 선택 가능

**결과 종류**:
- `HealHp`: 체력 회복 (value = 회복량)
- `LoseHp`: 체력 손실
- `GainGold`: 골드 획득
- `LoseGold`: 골드 소모
- `GainItem`: 아이템 획득
- `LoseItem`: 아이템 소모
- `TriggerBattle`: 몬스터와 전투 발동
- `Nothing`: 아무 일도 없음 (낚시 선택지)

**MVP 목표**: 20개 이벤트 (전투/선택/보상/위험 등 다양한 타입)

---

### 4️⃣ 몬스터 AI 로직 수정 (간단한 코드)

**파일**: `Scripts/Battle/Monster.cs`

**DecideAction() 메서드** (현재 TODO):

```csharp
public override void TakeTurn(BattleContext context)
{
    DecideAction(context);  // TODO: AI 행동 결정
    // ...
}

// TODO: 간단한 AI 규칙 구현
// 예:
// - HP가 50% 이하면 회복 스킬 사용
// - 아니면 가장 강한 공격 스킬 사용
// - 보스는 HP 30% 이하에서만 필살기 사용
```

콘텐츠 담당자가 이 부분에 조건문을 추가해서 몬스터별 AI 패턴을 구현 가능합니다.

---

### 협업 체크리스트

#### 1주차 (동영)
- ✅ 전투 기본 로직
- ✅ 이벤트 표시 시스템
- ✅ 선택지 처리
- ✅ RNG 시스템

#### 1~2주차 (콘텐츠 담당)
- [ ] 몬스터 5종 데이터 작성
  - [ ] Normal 슬라임
  - [ ] Normal 오크
  - [ ] Normal 고블린
  - [ ] Normal 박쥐
  - [ ] Elite 마법사
- [ ] 이벤트 20개 작성
  - [ ] 전투 이벤트 (몬스터 조우)
  - [ ] 선택 이벤트 (신비한 제단, 낡은 상자 등)
  - [ ] 보상 이벤트 (보물, 골드)
  - [ ] 위험 이벤트 (함정, 저주)
- [ ] 스킬 10개 기본 설정
  - [ ] 기본 공격 3종
  - [ ] 회복 2종
  - [ ] 버프/디버프 5종

---

## 핵심 루프 실행 순서

1. **Boot 씬** 로드 → `GameManager.Initialize()`, `SceneController` 준비
2. `GameManager.StartNewRun()` → Player 인스턴스 생성
3. `SceneController.LoadScene("Town")` → 마을 화면
4. 플레이어가 던전 입장 선택 → `GameManager.EnterDungeon()`
5. `SceneController.LoadScene("Dungeon")` → `EventManager` 초기화
6. 층 이동 시 `EventManager.TriggerRandomEvent(floor)` 호출
7. 전투 이벤트 → `BattleManager.StartBattle()` → Battle 씬 전환
8. 전투 종료 → 승리: 보상 지급 + Dungeon 씬 복귀 / 패배: `GameManager.OnPlayerDeath()`

---

## TODO / 확장 포인트

### 2주차 이후 구현 예정

| 항목 | 관련 파일 | 확장 방법 |
|------|-----------|-----------|
| 던전 층 진행 | `DungeonManager.cs` (신규) | 층마다 EventData 풀 분리, 층별 보스 배치 |
| 레벨업 시스템 | `Player.cs`, `PlayerData.cs` | `expReward` 누적 → 레벨업 이벤트 발행 |
| 메타 진행 (영구 업그레이드) | `GameManager.cs` | PlayerPrefs 또는 JSON으로 영구 데이터 저장 |
| 상점 시스템 | `ShopManager.cs` (신규) | `EventType.Shop` 처리, ItemData 구매/판매 |
| 카드 시스템 확장 | `SkillBase.cs` | `DrawCard` OutcomeType 구현 |
| 보스 페이즈 패턴 | `Monster.cs` | `MonsterType.Boss` 분기의 페이즈 로직 구현 |
| 조건 UI 노출 | `EventUI.cs`, `EventManager.cs` | `EvaluateConditions()` 접근 메서드 public으로 노출 |
| 페이드 연출 | `SceneController.cs` | CanvasGroup 알파 코루틴 구현 |

### 알려진 트레이드오프

- **ChoiceHandler의 TriggerBattle**: 현재 EventData의 `linkedMonster`를 ChoiceOutcome에서 직접 참조하는 방법이 없어 EventManager에서 중간 처리가 필요. 이벤트 체인 완성 후 개선 예정.
- **BattleUI 조건 평가**: `EventUI`에서 `ChoiceHandler.EvaluateConditions()`에 직접 접근하지 못해 조건 미충족 버튼 비활성화가 임시 처리됨. EventManager에 퍼블릭 메서드 노출 필요.
- **ScriptableObject 런타임 공유**: SO는 에셋 원본이므로 `SkillBase.currentCooldown`처럼 런타임 상태를 SO에 직접 저장하면 에디터에서 오염 가능. 런 시작 시 deep copy 또는 런타임 래퍼 클래스 도입 권장.

---

## 게임 개발 용어 사전

게임 개발에 낯선 분들을 위한 용어 설명입니다.

### 핵심 시스템

| 용어 | 뜻 | 예시 |
|------|-----|------|
| **씬 (Scene)** | 게임의 화면/장면. 마을, 던전, 전투 같이 분리된 영역 | Boot, Town, Dungeon, Battle |
| **엔티티 (Entity)** | 게임에 등장하는 "주체" (플레이어, 몬스터, NPC 등) | Player, Monster |
| **턴제 전투** | 플레이어와 적이 번갈아가며 행동하는 전투 방식 | 플레이어 턴 → 몬스터 턴 → 반복 |
| **스킬 (Skill)** | 캐릭터가 사용할 수 있는 특수 능력 | 강공격, 회복, 버프 |
| **이벤트 (Event)** | 게임 중 발생하는 상황/상호작용 | 신비한 제단, 낡은 상자 |
| **선택지 (Choice)** | 플레이어가 고를 수 있는 선택 항목 | "접근한다", "무시한다" |

### 데이터 개념

| 용어 | 뜻 | 예시 |
|------|-----|------|
| **ScriptableObject** | Unity의 데이터 저장 방식. 코드 없이 인스펙터로 수정 가능 | MonsterData, EventData |
| **프리팹 (Prefab)** | 게임 오브젝트의 템플릿. 복사해서 여러 개 만들 수 있음 | 버튼, UI 패널 |
| **인스펙터** | Unity 우측 창. 객체의 속성을 시각적으로 수정하는 곳 | 몬스터 체력, 공격력 입력 |

### 스탯/게임플레이

| 용어 | 뜻 | 단위/예시 |
|------|-----|----------|
| **HP (체력)** | 캐릭터의 생명력. 0이 되면 사망 | 30 (슬라임) |
| **공격력** | 피해를 얼마나 주는지 | 8 |
| **방어력** | 받는 피해를 얼마나 줄이는지 | 2 (피해 1~2 감소) |
| **스피드** | 행동 빠르기. 높을수록 먼저 행동 | 8 |
| **크리티컬** | 특수 공격. 추가 피해를 줌 | 5% 확률로 2배 피해 |
| **상태이상** | 임시 상태 변화 | 중독(매턴 피해), 석화(행동 불가) |

### 아키텍처/코드

| 용어 | 뜻 | 이 프로젝트에서 |
|------|-----|----------------|
| **매니저** | 특정 시스템을 관리하는 클래스 | BattleManager, EventManager |
| **싱글톤** | 게임 내내 하나만 존재하는 객체 | GameManager |
| **DontDestroyOnLoad** | 씬 전환 후에도 삭제되지 않음 | GameManager, SceneController |
| **코루틴** | 시간에 따라 실행되는 코드 | 턴 루프, 페이드 효과 |
| **이벤트 (Event)** | 특정 일이 일어났을 때 다른 부분에 알려주는 기능 | OnBattleStarted → UI 업데이트 |
| **네임스페이스** | 코드 정리 폴더. 같은 이름 피하기 위함 | RoguelikeRPG.Battle |

### 자주 헷갈리는 개념

| 구분 | 뜻 |
|------|-----|
| **Data (데이터)** | 숫자 정보 (몬스터 체력 30, 공격력 8 등) |
| **Logic (로직)** | 작동 방식 (체력 30 – 공격력 8 = 피해 계산) |
| **Presentation (화면)** | UI (체력바, 버튼, 텍스트) |

---

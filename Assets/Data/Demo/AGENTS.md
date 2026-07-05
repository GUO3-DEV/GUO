<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-06-23 | Updated: 2026-06-23 -->

# Demo (데모 샘플 콘텐츠)

## Purpose
플레이 가능한 데모(첫 전투 + 선택지 이벤트)를 구성하는 최소 콘텐츠 에셋 4종.
`Editor/DemoSceneBuilder.cs`가 이 에셋들을 생성/갱신하고 데모 씬에 연결한다.
이 디렉터리의 값은 곧 데모 플레이의 밸런스·텍스트임.

## Key Files
| 파일 | 타입 | 설명 |
|------|------|------|
| `Monster_DemoSlime.asset` | `MonsterData` | "동굴 슬라임". HP 48 / 공격 9 / 방어 2 / 속도 8. 첫 전투용 약한 몬스터. |
| `Item_DemoPotion.asset` | `ItemData` | "회복 물약". 전투 중 HP 30 회복(소비형). |
| `Skill_DemoPowerAttack.asset` | `SkillBase` | "강공격". 공격력 1.6배, 2턴 쿨다운. |
| `Event_DemoFirstEncounter.asset` | `EventData` | "동굴 입구" 이벤트. 전투 트리거 선택지 + 5골드 조건(잠금) 선택지 2개. |

## For AI Agents

### Working In This Directory
- **값은 코드에서 설정됨**: 이 에셋들의 구체 수치는 `DemoSceneBuilder.CreateSlime`/`CreatePotion`/`CreatePowerAttack`/`CreateFirstEvent`에서 하드코딩된다. 에셋 값을 바꾸면 다음 빌드에서 덮어씌워지므로, 의도한 변경은 빌더 코드를 수정할 것.
- **이벤트 ID 동기화**: `Event_DemoFirstEncounter.eventId = "demo_first_encounter"`는 `DemoGameBootstrap.firstEventId`와 일치해야 데모가 시작된다. ID를 바꾸면 양쪽 모두 갱신.
- **잠금 조건 테스트**: 두 번째 선택지("황금 부적을 산다", `MinGold` 5)는 시작 골드 0이므로 비활성화로 표시됨 — 조건 평가(`ChoiceHandler.EvaluateConditions`) + UI 비활성화(`ChoiceButtonItem`) 검증용.

## Dependencies

### Internal
- `Editor/DemoSceneBuilder`(생성 주체), `Scripts/Core/DemoGameBootstrap`(소비), `Scripts/Data/*`(스키마).

<!-- MANUAL: -->

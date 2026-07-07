<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-06-23 | Updated: 2026-06-23 -->

# Data (콘텐츠 에셋)

## Purpose
`Scripts/Data/`에 정의된 ScriptableObject 클래스들의 인스턴스 에셋(실제 게임 콘텐츠 데이터).
몬스터·아이템·스킬·이벤트의 구체 수치/텍스트를 보관한다. **코드 스키마가 아닌 데이터 인스턴스**임에 주의.
기획/콘텐츠 담당이 주로 편집하는 영역.

## Subdirectories
| 디렉터리 | 목적 |
|-----------|------|
| `Demo/` | 초기 데모 샘플 에셋 (see `Demo/AGENTS.md`) |
| `Skills/` `Items/` `Monsters/` `Events/` | MVP 콘텐츠 에셋 — `RoguelikeRPGContentBuilder`가 생성(빌드 후 존재). |

## For AI Agents

### Working In This Directory
- **스키마는 `Scripts/Data/`**: 이 디렉터리는 데이터 *값*만 보관. 필드를 추가/변경하려면 대응하는 C# 클래스(`MonsterData.cs`/`ItemData.cs` 등)를 먼저 수정해야 인스펙터에 노출됨.
- **GUID 안정성**: 에셋은 GUID로 참조된다. 삭제/이동 시 `.meta`까지 처리하고, `DemoSceneBuilder`의 경로 참조(`Assets/Data/Demo/...`)가 깨지지 않는지 확인.
- **에디터 자동 생성**: 데모 에셋은 `DemoSceneBuilder`가 프로그램적으로 생성/갱신한다. 직접 편집한 값을 빌드 시 덮어씌워지지 않으려면 빌더 로직과 동기화할 것.

### Common Patterns
- 파일명 규칙: `Type_Name.asset` (`Monster_DemoSlime`, `Item_DemoPotion`, `Skill_DemoPowerAttack`, `Event_DemoFirstEncounter`).
- CreateAssetMenu 메뉴로 생성 → 인스펙터에서 수치 입력.

## Dependencies

### Internal
- 각 에셋은 `Scripts/Data/`의 대응 클래스에 의존.

<!-- MANUAL: -->

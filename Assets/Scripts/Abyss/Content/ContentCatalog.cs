using System.Collections.Generic;
using static AbyssSurvivor.DefinitionGuard;

namespace AbyssSurvivor
{
    public sealed class ContentCatalog
    {
        public ContentCatalog(
            PlayerDefinition player,
            IEnumerable<ClassDefinition> classes,
            IEnumerable<MonsterDefinition> monsters,
            IEnumerable<ItemDefinition> items,
            IEnumerable<SkillDefinition> skills,
            IEnumerable<EventDefinition> events)
        {
            Player = player;
            Classes = ToReadOnlyList(classes, nameof(classes));
            Monsters = ToReadOnlyList(monsters, nameof(monsters));
            Items = ToReadOnlyList(items, nameof(items));
            Skills = ToReadOnlyList(skills, nameof(skills));
            Events = ToReadOnlyList(events, nameof(events));
        }

        public PlayerDefinition Player { get; }
        public IReadOnlyList<ClassDefinition> Classes { get; }
        public IReadOnlyList<MonsterDefinition> Monsters { get; }
        public IReadOnlyList<ItemDefinition> Items { get; }
        public IReadOnlyList<SkillDefinition> Skills { get; }
        public IReadOnlyList<EventDefinition> Events { get; }

        public static ContentCatalog CreateDefault()
        {
            return new ContentCatalog(
                new PlayerDefinition("Adventurer", new StatsBlock(100, 10, 5, 10, 15)),
                CreateClasses(),
                CreateMonsters(),
                CreateItems(),
                CreateSkills(),
                CreateEvents());
        }

        private static IReadOnlyList<ClassDefinition> CreateClasses()
        {
            return new[]
            {
                new ClassDefinition("berserker", "Berserker", "🗡️", TraitCategory.Attack, "HP 50% 미만 시 공격력 증가"),
                new ClassDefinition("paladin", "Paladin", "🛡️", TraitCategory.Defense, "받는 피해 감소와 반사 피해"),
                new ClassDefinition("assassin", "Assassin", "🗡️", TraitCategory.Evasion, "회피율과 치명타 강화"),
                new ClassDefinition("archmage", "Archmage", "🔮", TraitCategory.Magic, "마법 피해 증폭"),
                new ClassDefinition("necromancer", "Necromancer", "💀", TraitCategory.Curse, "저주와 지속 피해"),
                new ClassDefinition("summoner", "Summoner", "🐱", TraitCategory.Summon, "전투 시작 시 소환수 호출"),
                new ClassDefinition("vampire_lord", "Vampire Lord", "🧛", TraitCategory.Lifesteal, "흡혈과 1회 부활"),
                new ClassDefinition("alchemist", "Alchemist", "🧪", TraitCategory.Potion, "포션 효과 증폭")
            };
        }

        private static IReadOnlyList<MonsterDefinition> CreateMonsters()
        {
            return new[]
            {
                new MonsterDefinition("slime", "Slime", "🐀", MonsterTier.Normal, new StatsBlock(30, 5, 0, 0, 0), "점액: 방어력 감소"),
                new MonsterDefinition("spider", "Spider", "🕷️", MonsterTier.Normal, new StatsBlock(40, 8, 2, 0, 0), "독: 3턴 지속 피해"),
                new MonsterDefinition("bat", "Bat", "🦇", MonsterTier.Normal, new StatsBlock(25, 6, 1, 20, 0), "초음파: 회피율 감소"),
                new MonsterDefinition("wolf", "Wolf", "🐺", MonsterTier.Normal, new StatsBlock(50, 10, 3, 0, 10), "울음: 공격력 감소"),
                new MonsterDefinition("lizard", "Lizard", "🦎", MonsterTier.Normal, new StatsBlock(35, 7, 2, 20, 0), "꼬리 회피"),
                new MonsterDefinition("warrior_ghost", "Warrior Ghost", "👁️", MonsterTier.Elite, new StatsBlock(80, 15, 8, 0, 10), "소환: 좀비 1체"),
                new MonsterDefinition("fire_spirit", "Fire Spirit", "🔥", MonsterTier.Elite, new StatsBlock(70, 18, 5, 0, 10), "불길: 지속 화상"),
                new MonsterDefinition("golem", "Golem", "🌿", MonsterTier.Elite, new StatsBlock(120, 12, 15, 0, 0), "돌면: 피해 감소"),
                new MonsterDefinition("dragon", "Dragon", "🐉", MonsterTier.Boss, new StatsBlock(200, 20, 12, 0, 15), "용의 숨결"),
                new MonsterDefinition("necromancer_boss", "Necromancer", "💀", MonsterTier.Boss, new StatsBlock(250, 22, 8, 0, 15), "죽음의 저주와 흡혈"),
                new MonsterDefinition("dungeon_master", "Dungeon Master", "👹", MonsterTier.Boss, new StatsBlock(350, 25, 20, 0, 20), "최후의 일격")
            };
        }

        private static IReadOnlyList<ItemDefinition> CreateItems()
        {
            return new[]
            {
                new ItemDefinition("rusty_sword", "Rusty Sword", ItemType.Weapon, 5, "기본 공격력 증가"),
                new ItemDefinition("steel_sword", "Steel Sword", ItemType.Weapon, 12, "결빙 15%"),
                new ItemDefinition("flame_dagger", "Flame Dagger", ItemType.Weapon, 10, "발화 3턴"),
                new ItemDefinition("leather_armor", "Leather Armor", ItemType.Armor, 3, "기본 방어력 증가"),
                new ItemDefinition("steel_shield", "Steel Shield", ItemType.Armor, 8, "반사 피해 20%"),
                new ItemDefinition("dragon_scale_armor", "Dragon Scale Armor", ItemType.Armor, 12, "HP +50"),
                new ItemDefinition("life_potion", "Life Potion", ItemType.Potion, 30, "HP 30 회복"),
                new ItemDefinition("greater_life_potion", "Greater Life Potion", ItemType.Potion, 60, "HP 60 회복"),
                new ItemDefinition("haste_potion", "Haste Potion", ItemType.Potion, 20, "2턴 회피율 증가"),
                new ItemDefinition("healing_scroll", "Healing Scroll", ItemType.Scroll, 40, "HP 40 회복"),
                new ItemDefinition("protection_scroll", "Protection Scroll", ItemType.Scroll, 15, "3턴 방어력 증가")
            };
        }

        private static IReadOnlyList<SkillDefinition> CreateSkills()
        {
            return new[]
            {
                new SkillDefinition("smash", "Smash", SkillType.Physical, 15, 0, 2, "강한 물리 공격"),
                new SkillDefinition("double_strike", "Double Strike", SkillType.Physical, 20, 1, 3, "2회 공격"),
                new SkillDefinition("aimed_shot", "Aimed Shot", SkillType.Physical, 25, 2, 4, "높은 치명타 확률"),
                new SkillDefinition("fireball", "Fireball", SkillType.Magic, 25, 3, 2, "화염 피해"),
                new SkillDefinition("ice_shard", "Ice Shard", SkillType.Magic, 20, 2, 2, "얼음 피해"),
                new SkillDefinition("dark_arrow", "Dark Arrow", SkillType.Magic, 22, 3, 2, "지속 피해"),
                new SkillDefinition("warriors_rage", "Warrior's Rage", SkillType.Buff, 30, 2, 4, "3턴 공격력 증가"),
                new SkillDefinition("steel_guard", "Steel Guard", SkillType.Buff, 30, 2, 3, "3턴 방어력 증가"),
                new SkillDefinition("heal", "Life Recovery", SkillType.Heal, 30, 2, 3, "HP 30 회복"),
                new SkillDefinition("greater_heal", "Greater Recovery", SkillType.Heal, 50, 3, 4, "HP 50 회복"),
                new SkillDefinition("poison_sting", "Poison Sting", SkillType.Debuff, 5, 2, 2, "3턴 독 피해"),
                new SkillDefinition("summon_companion", "Summon Companion", SkillType.Summon, 1, 4, 5, "소환수 호출")
            };
        }

        private static IReadOnlyList<EventDefinition> CreateEvents()
        {
            return new[]
            {
                new EventDefinition("combat", "Monster Encounter", "몬스터와 조우했다.", 50, new[]
                {
                    Choice("싸운다", Outcome("combat", 100, "전투 시작"))
                }),
                new EventDefinition("abandoned_house", "Abandoned House", "버려진 집 안에서 무언가 반짝인다.", 20, new[]
                {
                    Choice("안에 들어가서 뒤져보자", Outcome("item", 50, "아이템 획득"), Outcome("trap", 50, "HP 피해")),
                    Choice("문을 부수고 들어간다", Outcome("item", 70, "아이템 획득"), Outcome("trap", 30, "HP -10"))
                }),
                new EventDefinition("treasure_chest", "Treasure Chest", "둥근 보물 상자가 놓여 있다.", 15, new[]
                {
                    Choice("상자를 연다", Outcome("gold", 70, "골드 획득"), Outcome("item", 30, "랜덤 아이템"))
                }),
                new EventDefinition("trap", "Trap", "함정 발판을 밟았다.", 10, new[]
                {
                    Choice("피해를 감수한다", Outcome("damage", 100, "HP 10-30 감소"))
                }),
                new EventDefinition("unknown_altar", "Unknown Altar", "신비로운 제단이 제안을 속삭인다.", 5, new[]
                {
                    Choice("HP 30을 바친다", Outcome("legend_trait", 70, "전설 특성"), Outcome("curse", 30, "저주")),
                    Choice("골드 100을 바친다", Outcome("rare_item", 80, "희귀 아이템"), Outcome("nothing", 20, "아무 일 없음"))
                })
            };
        }

        private static EventChoiceDefinition Choice(string text, params EventOutcomeDefinition[] outcomes)
        {
            return new EventChoiceDefinition(text, outcomes);
        }

        private static EventOutcomeDefinition Outcome(string type, int weight, string effect)
        {
            return new EventOutcomeDefinition(type, weight, effect);
        }
    }
}

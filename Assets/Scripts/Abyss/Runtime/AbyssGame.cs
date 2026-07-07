using System;
using System.Collections.Generic;
using System.Linq;

namespace AbyssSurvivor
{
    public enum AbyssPhase
    {
        Title,
        Town,
        Dungeon,
        Combat,
        Event,
        Treasure,
        LevelUp,
        Death,
        Clear
    }

    public enum TownBuilding
    {
        Blacksmith,
        Training,
        Tavern,
        MagicLibrary
    }

    public enum MonsterIntent
    {
        Attack,
        PowerAttack,
        Guard,
        Special
    }

    public interface IAbyssRandom
    {
        int Next(int exclusiveMax);
    }

    public sealed class FixedAbyssRandom : IAbyssRandom
    {
        private readonly Queue<int> _values;

        public FixedAbyssRandom(params int[] values)
        {
            _values = new Queue<int>(values ?? Array.Empty<int>());
        }

        public int Next(int exclusiveMax)
        {
            if (exclusiveMax <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            }

            int value = _values.Count == 0 ? 0 : _values.Dequeue();
            return Math.Abs(value) % exclusiveMax;
        }
    }

    public sealed class SystemAbyssRandom : IAbyssRandom
    {
        private readonly Random _random;

        public SystemAbyssRandom(int seed)
        {
            _random = new Random(seed);
        }

        public int Next(int exclusiveMax)
        {
            return _random.Next(exclusiveMax);
        }
    }

    public sealed class EventResolution
    {
        public EventResolution(string type, string effect)
        {
            Type = DefinitionGuard.RequireText(type, nameof(type));
            Effect = DefinitionGuard.RequireText(effect, nameof(effect));
        }

        public string Type { get; }
        public string Effect { get; }
    }

    public sealed class CombatLogEntry
    {
        public CombatLogEntry(string text, int playerDamage, int monsterDamage)
        {
            Text = DefinitionGuard.RequireText(text, nameof(text));
            PlayerDamage = Math.Max(0, playerDamage);
            MonsterDamage = Math.Max(0, monsterDamage);
        }

        public string Text { get; }
        public int PlayerDamage { get; }
        public int MonsterDamage { get; }
    }

    public sealed class CombatantState
    {
        private int _guardTurns;

        public CombatantState(MonsterDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Health = definition.Stats.MaxHealth;
        }

        public MonsterDefinition Definition { get; }
        public int Health { get; private set; }
        public bool IsDead => Health <= 0;
        public bool IsGuarding => _guardTurns > 0;

        public void TakeDamage(int amount)
        {
            int finalAmount = IsGuarding ? Math.Max(1, amount / 2) : amount;
            Health = Math.Max(0, Health - Math.Max(0, finalAmount));
            _guardTurns = 0;
        }

        public void Guard()
        {
            _guardTurns = 1;
        }
    }

    public sealed class PlayerRuntimeState
    {
        private readonly Dictionary<string, int> _inventory = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _cooldowns = new Dictionary<string, int>();
        private readonly Dictionary<TraitCategory, int> _traits = new Dictionary<TraitCategory, int>();
        private readonly List<ClassDefinition> _unlockedClasses = new List<ClassDefinition>();
        private ItemDefinition _weapon;
        private ItemDefinition _armor;
        private int _guardTurns;

        public PlayerRuntimeState(PlayerDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Health = definition.BaseStats.MaxHealth;
            Mana = 6;
        }

        public PlayerDefinition Definition { get; }
        public int Health { get; private set; }
        public int Mana { get; private set; }
        public int Gold { get; private set; }
        public int Experience { get; private set; }
        public int Level => Math.Max(1, 1 + Experience / 100);
        public int Attack => Definition.BaseStats.Attack + (_weapon?.Power ?? 0) + (Level - 1) * 2;
        public int Defense => Definition.BaseStats.Defense + (_armor?.Power ?? 0) + (Level - 1);
        public bool IsDead => Health <= 0;
        public bool IsGuarding => _guardTurns > 0;
        public IReadOnlyDictionary<string, int> Inventory => _inventory;
        public IReadOnlyList<ClassDefinition> UnlockedClasses => _unlockedClasses;

        public void TakeDamage(int amount)
        {
            int finalAmount = IsGuarding ? Math.Max(1, amount / 2) : amount;
            Health = Math.Max(0, Health - Math.Max(0, finalAmount));
            _guardTurns = 0;
        }

        public void Guard()
        {
            _guardTurns = 1;
        }

        public void Heal(int amount)
        {
            Health = Math.Min(Definition.BaseStats.MaxHealth, Health + Math.Max(0, amount));
        }

        public void RestoreMana(int amount)
        {
            Mana = Math.Min(12, Mana + Math.Max(0, amount));
        }

        public void SpendMana(int amount)
        {
            if (Mana < amount)
            {
                throw new InvalidOperationException("Not enough mana.");
            }

            Mana -= amount;
        }

        public void AddGold(int amount)
        {
            Gold += Math.Max(0, amount);
        }

        public void SpendGold(int amount)
        {
            if (Gold < amount)
            {
                throw new InvalidOperationException("Not enough gold.");
            }

            Gold -= amount;
        }

        public void AddExperience(int amount)
        {
            Experience += Math.Max(0, amount);
        }

        public void AddItem(string itemId)
        {
            DefinitionGuard.RequireText(itemId, nameof(itemId));
            _inventory[itemId] = _inventory.TryGetValue(itemId, out int count) ? count + 1 : 1;
        }

        public void RemoveItem(string itemId)
        {
            if (!_inventory.TryGetValue(itemId, out int count))
            {
                throw new InvalidOperationException("Item is not in inventory.");
            }

            if (count <= 1)
            {
                _inventory.Remove(itemId);
                return;
            }

            _inventory[itemId] = count - 1;
        }

        public void Equip(ItemDefinition item)
        {
            if (item.Type == ItemType.Weapon)
            {
                _weapon = item;
                return;
            }

            if (item.Type == ItemType.Armor)
            {
                _armor = item;
                return;
            }

            throw new InvalidOperationException("Only weapons and armor can be equipped.");
        }

        public void SetCooldown(string skillId, int turns)
        {
            _cooldowns[skillId] = Math.Max(0, turns);
        }

        public int GetCooldown(string skillId)
        {
            return _cooldowns.TryGetValue(skillId, out int turns) ? turns : 0;
        }

        public void TickCooldowns()
        {
            string[] keys = _cooldowns.Keys.ToArray();
            foreach (string key in keys)
            {
                _cooldowns[key] = Math.Max(0, _cooldowns[key] - 1);
            }

            RestoreMana(1);
        }

        public void AddTrait(TraitCategory trait)
        {
            _traits[trait] = GetTraitCount(trait) + 1;
        }

        public int GetTraitCount(TraitCategory trait)
        {
            return _traits.TryGetValue(trait, out int count) ? count : 0;
        }

        public void UnlockClass(ClassDefinition classDefinition)
        {
            if (_unlockedClasses.All(entry => entry.Id != classDefinition.Id))
            {
                _unlockedClasses.Add(classDefinition);
            }
        }
    }

    public sealed class TownProgressState
    {
        private readonly Dictionary<TownBuilding, int> _levels = new Dictionary<TownBuilding, int>
        {
            { TownBuilding.Blacksmith, 1 },
            { TownBuilding.Training, 0 },
            { TownBuilding.Tavern, 0 },
            { TownBuilding.MagicLibrary, 0 }
        };

        public bool IsUnlocked(TownBuilding building)
        {
            return _levels[building] > 0;
        }

        public int GetLevel(TownBuilding building)
        {
            return _levels[building];
        }

        public void Upgrade(TownBuilding building)
        {
            if (!IsUnlocked(building))
            {
                throw new InvalidOperationException("Building is locked.");
            }

            _levels[building]++;
            UnlockNext(building);
        }

        private void UnlockNext(TownBuilding building)
        {
            if (building == TownBuilding.Blacksmith)
            {
                _levels[TownBuilding.Training] = Math.Max(1, _levels[TownBuilding.Training]);
            }
            else if (building == TownBuilding.Training)
            {
                _levels[TownBuilding.Tavern] = Math.Max(1, _levels[TownBuilding.Tavern]);
            }
            else if (building == TownBuilding.Tavern)
            {
                _levels[TownBuilding.MagicLibrary] = Math.Max(1, _levels[TownBuilding.MagicLibrary]);
            }
        }
    }

    public sealed class AbyssGame
    {
        private readonly ContentCatalog _catalog;
        private readonly IAbyssRandom _random;
        private EventDefinition _currentEvent;

        private AbyssGame(ContentCatalog catalog, IAbyssRandom random)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            Phase = AbyssPhase.Title;
        }

        public AbyssPhase Phase { get; private set; }
        public int Floor { get; private set; }
        public PlayerRuntimeState Player { get; private set; }
        public CombatantState CurrentMonster { get; private set; }
        public TownProgressState Town { get; private set; }
        public IReadOnlyList<MonsterDefinition> Monsters => _catalog.Monsters;
        public IReadOnlyList<ItemDefinition> Items => _catalog.Items;
        public IReadOnlyList<SkillDefinition> Skills => _catalog.Skills;
        public IReadOnlyList<EventDefinition> Events => _catalog.Events;
        public EventDefinition CurrentEvent => _currentEvent;

        public static AbyssGame CreateForTests(ContentCatalog catalog, IAbyssRandom random)
        {
            return new AbyssGame(catalog, random);
        }

        public static AbyssGame CreateDefault(int seed = 0)
        {
            return new AbyssGame(ContentCatalog.CreateDefault(), new SystemAbyssRandom(seed));
        }

        public void StartNewRun()
        {
            Player = new PlayerRuntimeState(_catalog.Player);
            Town = new TownProgressState();
            CurrentMonster = null;
            _currentEvent = null;
            Floor = 0;
            Phase = AbyssPhase.Town;
        }

        public void EnterDungeon()
        {
            EnsurePlayer();
            Floor = Math.Max(1, Floor);
            Phase = AbyssPhase.Dungeon;
        }

        public void AdvanceFloor()
        {
            EnsurePlayer();
            Floor = Math.Max(1, Floor + 1);
            Phase = Floor >= 10 ? AbyssPhase.Combat : AbyssPhase.Dungeon;
            if (Floor >= 10)
            {
                StartBossCombatForCurrentFloor();
            }
        }

        public void StartCombat(string monsterId)
        {
            EnsurePlayer();
            MonsterDefinition monster = FindMonster(monsterId);
            CurrentMonster = new CombatantState(monster);
            Phase = AbyssPhase.Combat;
        }

        public void StartRandomCombatForCurrentFloor()
        {
            EnsurePlayer();
            MonsterDefinition monster = PickMonsterForFloor();
            StartCombat(monster.Id);
        }

        public void StartBossCombatForCurrentFloor()
        {
            MonsterDefinition boss = _catalog.Monsters.First(entry => entry.Tier == MonsterTier.Boss);
            if (Floor >= 10)
            {
                boss = _catalog.Monsters.First(entry => entry.Id == "dungeon_master");
            }

            StartCombat(boss.Id);
        }

        public CombatLogEntry UseBasicAttack()
        {
            EnsureCombat();
            int damage = RollPlayerDamage(0);
            CurrentMonster.TakeDamage(damage);
            string text = "기본 공격으로 " + CurrentMonster.Definition.Name + "에게 " + damage + " 피해.";
            if (CurrentMonster.IsDead)
            {
                return FinishCombat(text, damage, 0);
            }

            CombatLogEntry monsterTurn = ResolveMonsterTurn();
            return new CombatLogEntry(text + "\n" + monsterTurn.Text, damage, monsterTurn.MonsterDamage);
        }

        public CombatLogEntry UseSkill(string skillId)
        {
            EnsureCombat();
            SkillDefinition skill = _catalog.Skills.First(entry => entry.Id == skillId);
            if (Player.GetCooldown(skill.Id) > 0)
            {
                throw new InvalidOperationException("Skill is cooling down.");
            }

            Player.SpendMana(skill.ManaCost);
            int damage = 0;
            string text;
            if (skill.Type == SkillType.Heal)
            {
                Player.Heal(skill.Power);
                text = skill.Name + " 사용. HP " + skill.Power + " 회복.";
            }
            else if (skill.Type == SkillType.Buff)
            {
                Player.Guard();
                text = skill.Name + " 사용. 다음 피해를 감소.";
            }
            else
            {
                damage = RollPlayerDamage(skill.Power);
                CurrentMonster.TakeDamage(damage);
                text = skill.Name + " 사용. " + CurrentMonster.Definition.Name + "에게 " + damage + " 피해.";
            }

            Player.SetCooldown(skill.Id, skill.CooldownTurns);
            if (CurrentMonster.IsDead)
            {
                return FinishCombat(text, damage, 0);
            }

            CombatLogEntry monsterTurn = ResolveMonsterTurn();
            return new CombatLogEntry(text + "\n" + monsterTurn.Text, damage, monsterTurn.MonsterDamage);
        }

        public CombatLogEntry Defend()
        {
            EnsureCombat();
            Player.Guard();
            CombatLogEntry monsterTurn = ResolveMonsterTurn();
            return new CombatLogEntry("방어 자세.\n" + monsterTurn.Text, 0, monsterTurn.MonsterDamage);
        }

        public void WinCurrentCombat()
        {
            EnsureCombat();
            FinishCombat("전투 승리.", 0, 0);
        }

        public void ResolvePlayerDeath()
        {
            EnsurePlayer();
            if (Player.IsDead)
            {
                Phase = AbyssPhase.Death;
            }
        }

        public void SetFloorForTests(int floor)
        {
            Floor = Math.Max(1, floor);
        }

        public void TriggerEvent(string eventId)
        {
            EnsurePlayer();
            _currentEvent = _catalog.Events.First(entry => entry.Id == eventId);
            Phase = AbyssPhase.Event;
        }

        public void TriggerWeightedEvent()
        {
            EnsurePlayer();
            _currentEvent = PickWeighted(_catalog.Events);
            Phase = _currentEvent.Id == "combat" ? AbyssPhase.Combat : AbyssPhase.Event;
            if (_currentEvent.Id == "combat")
            {
                StartRandomCombatForCurrentFloor();
            }
        }

        public EventResolution ResolveEventChoice(int choiceIndex)
        {
            if (_currentEvent == null)
            {
                throw new InvalidOperationException("No event is active.");
            }

            EventChoiceDefinition choice = _currentEvent.Choices[choiceIndex];
            EventOutcomeDefinition outcome = PickWeighted(choice.Outcomes);
            ApplyOutcome(outcome);
            Phase = Player.IsDead ? AbyssPhase.Death : AbyssPhase.Dungeon;
            return new EventResolution(outcome.Type, outcome.Effect);
        }

        public string UseItem(string itemId)
        {
            EnsurePlayer();
            ItemDefinition item = FindItem(itemId);
            if (item.Type != ItemType.Potion && item.Type != ItemType.Scroll)
            {
                throw new InvalidOperationException("Item is not consumable.");
            }

            Player.RemoveItem(itemId);
            Player.Heal(item.Power);
            return item.Name + " 사용. HP " + item.Power + " 회복.";
        }

        public string EquipItem(string itemId)
        {
            EnsurePlayer();
            ItemDefinition item = FindItem(itemId);
            if (!Player.Inventory.ContainsKey(itemId))
            {
                throw new InvalidOperationException("Item must be owned before equipping.");
            }

            Player.Equip(item);
            return item.Name + " 장착.";
        }

        public string BuyItem(string itemId)
        {
            EnsurePlayer();
            ItemDefinition item = FindItem(itemId);
            int cost = Math.Max(20, item.Power * 8);
            Player.SpendGold(cost);
            Player.AddItem(item.Id);
            return item.Name + " 구매 완료. -" + cost + "G";
        }

        public void UpgradeTownBuilding(TownBuilding building)
        {
            EnsurePlayer();
            Player.SpendGold(50);
            Town.Upgrade(building);
        }

        public ClassDefinition TryUnlockClass()
        {
            EnsurePlayer();
            ClassDefinition unlocked = _catalog.Classes.FirstOrDefault(entry =>
                Player.GetTraitCount(entry.RequiredTrait) >= entry.RequiredTraitCount &&
                Player.UnlockedClasses.All(existing => existing.Id != entry.Id));

            if (unlocked != null)
            {
                Player.UnlockClass(unlocked);
                Phase = AbyssPhase.LevelUp;
            }

            return unlocked;
        }

        private CombatLogEntry FinishCombat(string prefix, int playerDamage, int monsterDamage)
        {
            CurrentMonster.TakeDamage(9999);
            Player.AddGold(20 + Floor * 5 + (int)CurrentMonster.Definition.Tier * 20);
            Player.AddExperience(10 + Floor * 3 + (int)CurrentMonster.Definition.Tier * 15);
            Player.AddItem(_catalog.Items[_random.Next(_catalog.Items.Count)].Id);
            Player.TickCooldowns();

            if (CurrentMonster.Definition.Tier == MonsterTier.Boss && Floor >= 10)
            {
                Phase = AbyssPhase.Clear;
                return new CombatLogEntry(prefix + "\n최종 보스를 쓰러뜨렸습니다. 던전 클리어!", playerDamage, monsterDamage);
            }

            CurrentMonster = null;
            Phase = AbyssPhase.Treasure;
            return new CombatLogEntry(prefix + "\n전리품을 획득했습니다.", playerDamage, monsterDamage);
        }

        private CombatLogEntry ResolveMonsterTurn()
        {
            MonsterIntent intent = DecideMonsterIntent();
            int damage = 0;
            string text;
            if (intent == MonsterIntent.Guard)
            {
                CurrentMonster.Guard();
                text = CurrentMonster.Definition.Name + "이 방어 자세를 취했습니다.";
            }
            else
            {
                int bonus = intent == MonsterIntent.PowerAttack ? 5 : intent == MonsterIntent.Special ? 8 : 0;
                damage = Math.Max(1, CurrentMonster.Definition.Stats.Attack + bonus - Player.Defense + _random.Next(4));
                Player.TakeDamage(damage);
                text = CurrentMonster.Definition.Name + "의 " + IntentName(intent) + ". " + damage + " 피해.";
            }

            Player.TickCooldowns();
            ResolvePlayerDeath();
            return new CombatLogEntry(text, 0, damage);
        }

        private MonsterIntent DecideMonsterIntent()
        {
            if (CurrentMonster.Health <= CurrentMonster.Definition.Stats.MaxHealth / 3 && _random.Next(100) < 30)
            {
                return MonsterIntent.Guard;
            }

            int roll = _random.Next(100);
            if (CurrentMonster.Definition.Tier == MonsterTier.Boss && roll < 40)
            {
                return MonsterIntent.Special;
            }

            if (CurrentMonster.Definition.Tier == MonsterTier.Elite && roll < 30)
            {
                return MonsterIntent.PowerAttack;
            }

            return roll < 18 ? MonsterIntent.PowerAttack : MonsterIntent.Attack;
        }

        private int RollPlayerDamage(int skillPower)
        {
            int variance = _random.Next(5);
            return Math.Max(1, Player.Attack + skillPower + variance - CurrentMonster.Definition.Stats.Defense);
        }

        private MonsterDefinition PickMonsterForFloor()
        {
            if (Floor >= 10)
            {
                return _catalog.Monsters.First(entry => entry.Id == "dungeon_master");
            }

            MonsterTier tier = Floor >= 7 && _random.Next(100) < 45 ? MonsterTier.Elite : MonsterTier.Normal;
            if (Floor >= 4 && Floor < 7 && _random.Next(100) < 30)
            {
                tier = MonsterTier.Elite;
            }

            MonsterDefinition[] pool = _catalog.Monsters.Where(entry => entry.Tier == tier).ToArray();
            return pool[_random.Next(pool.Length)];
        }

        private EventDefinition PickWeighted(IReadOnlyList<EventDefinition> events)
        {
            int total = events.Sum(entry => entry.Weight);
            int roll = _random.Next(total);
            int cursor = 0;
            foreach (EventDefinition entry in events)
            {
                cursor += entry.Weight;
                if (roll < cursor)
                {
                    return entry;
                }
            }

            return events[events.Count - 1];
        }

        private EventOutcomeDefinition PickWeighted(IReadOnlyList<EventOutcomeDefinition> outcomes)
        {
            int total = outcomes.Sum(entry => entry.Weight);
            int roll = _random.Next(total);
            int cursor = 0;
            foreach (EventOutcomeDefinition outcome in outcomes)
            {
                cursor += outcome.Weight;
                if (roll < cursor)
                {
                    return outcome;
                }
            }

            return outcomes[outcomes.Count - 1];
        }

        private void ApplyOutcome(EventOutcomeDefinition outcome)
        {
            if (outcome.Type == "item" || outcome.Type == "rare_item")
            {
                Player.AddItem(_catalog.Items[_random.Next(_catalog.Items.Count)].Id);
            }
            else if (outcome.Type == "gold")
            {
                Player.AddGold(50);
            }
            else if (outcome.Type == "trap" || outcome.Type == "damage")
            {
                Player.TakeDamage(10 + _random.Next(21));
                ResolvePlayerDeath();
            }
            else if (outcome.Type == "legend_trait")
            {
                Player.TakeDamage(30);
                ResolvePlayerDeath();
                Player.AddTrait(TraitCategory.Magic);
            }
            else if (outcome.Type == "curse")
            {
                Player.AddTrait(TraitCategory.Curse);
            }
        }

        private MonsterDefinition FindMonster(string monsterId)
        {
            return _catalog.Monsters.First(entry => entry.Id == monsterId);
        }

        private ItemDefinition FindItem(string itemId)
        {
            return _catalog.Items.First(entry => entry.Id == itemId);
        }

        private static string IntentName(MonsterIntent intent)
        {
            if (intent == MonsterIntent.PowerAttack)
            {
                return "강공격";
            }

            if (intent == MonsterIntent.Special)
            {
                return "특수 패턴";
            }

            return "공격";
        }

        private void EnsurePlayer()
        {
            if (Player == null)
            {
                throw new InvalidOperationException("Start a run first.");
            }
        }

        private void EnsureCombat()
        {
            EnsurePlayer();
            if (Phase != AbyssPhase.Combat || CurrentMonster == null)
            {
                throw new InvalidOperationException("No combat is active.");
            }
        }
    }
}

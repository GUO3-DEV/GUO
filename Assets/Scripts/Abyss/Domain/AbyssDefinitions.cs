using System;
using System.Collections.Generic;
using System.Linq;
using static AbyssSurvivor.DefinitionGuard;

namespace AbyssSurvivor
{
    public enum MonsterTier
    {
        Normal,
        Elite,
        Boss
    }

    public enum ItemType
    {
        Weapon,
        Armor,
        Potion,
        Scroll,
        Material
    }

    public enum SkillType
    {
        Physical,
        Magic,
        Buff,
        Debuff,
        Heal,
        Summon
    }

    public enum TraitCategory
    {
        Attack,
        Defense,
        Evasion,
        Magic,
        Curse,
        Summon,
        Lifesteal,
        Potion
    }

    public sealed class StatsBlock
    {
        public StatsBlock(int maxHealth, int attack, int defense, int evasionPercent, int critPercent)
        {
            MaxHealth = maxHealth;
            Attack = attack;
            Defense = defense;
            EvasionPercent = evasionPercent;
            CritPercent = critPercent;
        }

        public int MaxHealth { get; }
        public int Attack { get; }
        public int Defense { get; }
        public int EvasionPercent { get; }
        public int CritPercent { get; }
    }

    public sealed class PlayerDefinition
    {
        public PlayerDefinition(string name, StatsBlock baseStats)
        {
            Name = RequireText(name, nameof(name));
            BaseStats = baseStats ?? throw new ArgumentNullException(nameof(baseStats));
        }

        public string Name { get; }
        public StatsBlock BaseStats { get; }
    }

    public sealed class ClassDefinition
    {
        public ClassDefinition(string id, string name, string icon, TraitCategory requiredTrait, string passive)
        {
            Id = RequireText(id, nameof(id));
            Name = RequireText(name, nameof(name));
            Icon = RequireText(icon, nameof(icon));
            RequiredTrait = requiredTrait;
            Passive = RequireText(passive, nameof(passive));
        }

        public string Id { get; }
        public string Name { get; }
        public string Icon { get; }
        public TraitCategory RequiredTrait { get; }
        public int RequiredTraitCount => 2;
        public string Passive { get; }
    }

    public sealed class MonsterDefinition
    {
        public MonsterDefinition(string id, string name, string icon, MonsterTier tier, StatsBlock stats, string special)
        {
            Id = RequireText(id, nameof(id));
            Name = RequireText(name, nameof(name));
            Icon = RequireText(icon, nameof(icon));
            Tier = tier;
            Stats = stats ?? throw new ArgumentNullException(nameof(stats));
            Special = RequireText(special, nameof(special));
        }

        public string Id { get; }
        public string Name { get; }
        public string Icon { get; }
        public MonsterTier Tier { get; }
        public StatsBlock Stats { get; }
        public string Special { get; }
    }

    public sealed class ItemDefinition
    {
        public ItemDefinition(string id, string name, ItemType type, int power, string effect)
        {
            Id = RequireText(id, nameof(id));
            Name = RequireText(name, nameof(name));
            Type = type;
            Power = power;
            Effect = RequireText(effect, nameof(effect));
        }

        public string Id { get; }
        public string Name { get; }
        public ItemType Type { get; }
        public int Power { get; }
        public string Effect { get; }
    }

    public sealed class SkillDefinition
    {
        public SkillDefinition(string id, string name, SkillType type, int power, int manaCost, int cooldownTurns, string effect)
        {
            Id = RequireText(id, nameof(id));
            Name = RequireText(name, nameof(name));
            Type = type;
            Power = power;
            ManaCost = manaCost;
            CooldownTurns = cooldownTurns;
            Effect = RequireText(effect, nameof(effect));
        }

        public string Id { get; }
        public string Name { get; }
        public SkillType Type { get; }
        public int Power { get; }
        public int ManaCost { get; }
        public int CooldownTurns { get; }
        public string Effect { get; }
    }

    public sealed class EventOutcomeDefinition
    {
        public EventOutcomeDefinition(string type, int weight, string effect)
        {
            Type = RequireText(type, nameof(type));
            Weight = weight;
            Effect = RequireText(effect, nameof(effect));
        }

        public string Type { get; }
        public int Weight { get; }
        public string Effect { get; }
    }

    public sealed class EventChoiceDefinition
    {
        public EventChoiceDefinition(string text, IEnumerable<EventOutcomeDefinition> outcomes)
        {
            Text = RequireText(text, nameof(text));
            Outcomes = ToReadOnlyList(outcomes, nameof(outcomes));
        }

        public string Text { get; }
        public IReadOnlyList<EventOutcomeDefinition> Outcomes { get; }
    }

    public sealed class EventDefinition
    {
        public EventDefinition(string id, string name, string description, int weight, IEnumerable<EventChoiceDefinition> choices)
        {
            Id = RequireText(id, nameof(id));
            Name = RequireText(name, nameof(name));
            Description = RequireText(description, nameof(description));
            Weight = weight;
            Choices = ToReadOnlyList(choices, nameof(choices));
        }

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int Weight { get; }
        public IReadOnlyList<EventChoiceDefinition> Choices { get; }
    }

    internal static class DefinitionGuard
    {
        public static string RequireText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value must contain text.", parameterName);
            }

            return value;
        }

        public static IReadOnlyList<T> ToReadOnlyList<T>(IEnumerable<T> values, string parameterName)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            T[] items = values.ToArray();
            if (items.Length == 0)
            {
                throw new ArgumentException("Collection must not be empty.", parameterName);
            }

            return Array.AsReadOnly(items);
        }
    }
}

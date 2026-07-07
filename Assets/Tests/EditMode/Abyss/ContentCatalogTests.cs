using System.Linq;
using AbyssSurvivor;
using NUnit.Framework;

namespace AbyssSurvivor.Tests
{
    public sealed class ContentCatalogTests
    {
        [Test]
        public void CreateDefault_ReturnsNotionContentCatalog_whenRequested()
        {
            // Given: the Notion contract defines the Wave 1 static content catalog.
            // When: the runtime catalog is created.
            var catalog = ContentCatalog.CreateDefault();

            // Then: the catalog exposes the required player, class, monster, item, skill, and event data.
            Assert.That(catalog.Player.BaseStats.MaxHealth, Is.EqualTo(100));
            Assert.That(catalog.Player.BaseStats.Attack, Is.EqualTo(10));
            Assert.That(catalog.Player.BaseStats.Defense, Is.EqualTo(5));
            Assert.That(catalog.Player.BaseStats.EvasionPercent, Is.EqualTo(10));
            Assert.That(catalog.Player.BaseStats.CritPercent, Is.EqualTo(15));
            Assert.That(catalog.Classes, Has.Count.EqualTo(8));
            Assert.That(catalog.Classes.Select(entry => entry.Name), Does.Contain("Berserker"));
            Assert.That(catalog.Classes.Select(entry => entry.Name), Has.No.Member("Dungeon Master"));
            Assert.That(catalog.Monsters.Count(entry => entry.Tier == MonsterTier.Normal), Is.EqualTo(5));
            Assert.That(catalog.Monsters.Count(entry => entry.Tier == MonsterTier.Elite), Is.EqualTo(3));
            Assert.That(catalog.Monsters.Count(entry => entry.Tier == MonsterTier.Boss), Is.EqualTo(3));
            Assert.That(catalog.Monsters.Select(entry => entry.Name), Does.Contain("Dragon"));
            Assert.That(catalog.Monsters.Select(entry => entry.Name), Does.Contain("Necromancer"));
            Assert.That(catalog.Monsters.Select(entry => entry.Name), Does.Contain("Dungeon Master"));
            Assert.That(catalog.Items.Any(entry => entry.Type == ItemType.Weapon), Is.True);
            Assert.That(catalog.Items.Any(entry => entry.Type == ItemType.Armor), Is.True);
            Assert.That(catalog.Items.Any(entry => entry.Type == ItemType.Potion), Is.True);
            Assert.That(catalog.Items.Any(entry => entry.Type == ItemType.Scroll), Is.True);
            Assert.That(catalog.Skills, Is.Not.Empty);
            Assert.That(catalog.Skills.All(entry => entry.ManaCost >= 0), Is.True);
            Assert.That(catalog.Skills.All(entry => entry.CooldownTurns >= 0), Is.True);
            Assert.That(catalog.Skills.Select(entry => entry.Type), Does.Contain(SkillType.Physical));
            Assert.That(catalog.Skills.Select(entry => entry.Type), Does.Contain(SkillType.Magic));
            Assert.That(catalog.Skills.Select(entry => entry.Type), Does.Contain(SkillType.Heal));
            Assert.That(catalog.Events.Sum(entry => entry.Weight), Is.EqualTo(100));
            Assert.That(catalog.Events.Select(entry => entry.Name), Does.Contain("Abandoned House"));
            Assert.That(catalog.Events.SelectMany(entry => entry.Choices).Any(choice => choice.Outcomes.Sum(outcome => outcome.Weight) == 100), Is.True);
        }
    }
}

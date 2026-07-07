using System.Linq;
using AbyssSurvivor;
using NUnit.Framework;

namespace AbyssSurvivor.Tests
{
    public sealed class AbyssRuntimeTests
    {
        [Test]
        public void CombatLoop_AppliesDamageSkillCooldownRewardsDeathAndClear()
        {
            var game = AbyssGame.CreateForTests(ContentCatalog.CreateDefault(), new FixedAbyssRandom(0, 0, 0));

            game.StartNewRun();
            game.EnterDungeon();
            game.StartCombat("golem");

            int monsterHealth = game.CurrentMonster.Health;
            game.UseBasicAttack();
            Assert.That(game.CurrentMonster.Health, Is.LessThan(monsterHealth));

            int manaBeforeSkill = game.Player.Mana;
            game.UseSkill("fireball");
            Assert.That(game.Player.Mana, Is.LessThan(manaBeforeSkill));
            Assert.That(game.Player.GetCooldown("fireball"), Is.EqualTo(1));

            game.WinCurrentCombat();
            Assert.That(game.Phase, Is.EqualTo(AbyssPhase.Treasure));
            Assert.That(game.Player.Gold, Is.GreaterThan(0));
            Assert.That(game.Player.Experience, Is.GreaterThan(0));
            Assert.That(game.Player.Inventory, Is.Not.Empty);

            game.StartCombat("dragon");
            game.Player.TakeDamage(999);
            game.ResolvePlayerDeath();
            Assert.That(game.Phase, Is.EqualTo(AbyssPhase.Death));

            game.StartNewRun();
            game.EnterDungeon();
            game.SetFloorForTests(10);
            game.StartBossCombatForCurrentFloor();
            Assert.That(game.CurrentMonster.Definition.Tier, Is.EqualTo(MonsterTier.Boss));
            game.WinCurrentCombat();
            Assert.That(game.Phase, Is.EqualTo(AbyssPhase.Clear));
        }

        [Test]
        public void MonsterAI_CanCounterAttackAndBossUsesPatterns()
        {
            var game = AbyssGame.CreateForTests(ContentCatalog.CreateDefault(), new FixedAbyssRandom(99, 0, 0, 0, 0));

            game.StartNewRun();
            game.EnterDungeon();
            game.SetFloorForTests(10);
            game.StartBossCombatForCurrentFloor();

            int healthBefore = game.Player.Health;
            CombatLogEntry log = game.UseBasicAttack();

            Assert.That(log.MonsterDamage, Is.GreaterThan(0));
            Assert.That(game.Player.Health, Is.LessThan(healthBefore));
            Assert.That(log.Text, Does.Contain("특수 패턴"));
            Assert.That(game.Phase, Is.EqualTo(AbyssPhase.Combat));
        }

        [Test]
        public void EventInventoryTownAndClassSystems_ApplyNotionProgressionRules()
        {
            var game = AbyssGame.CreateForTests(ContentCatalog.CreateDefault(), new FixedAbyssRandom(0, 0, 99, 0, 0));

            game.StartNewRun();
            game.Player.TakeDamage(40);
            game.Player.AddItem("life_potion");
            game.UseItem("life_potion");
            Assert.That(game.Player.Health, Is.EqualTo(90));

            game.Player.AddItem("rusty_sword");
            game.EquipItem("rusty_sword");
            Assert.That(game.Player.Attack, Is.EqualTo(15));

            game.Player.AddItem("leather_armor");
            game.EquipItem("leather_armor");
            Assert.That(game.Player.Defense, Is.EqualTo(8));

            game.TriggerEvent("abandoned_house");
            EventResolution reward = game.ResolveEventChoice(0);
            Assert.That(reward.Type, Is.EqualTo("item"));
            Assert.That(game.Player.Inventory.Count, Is.GreaterThanOrEqualTo(1));

            game.TriggerEvent("abandoned_house");
            int healthBeforeTrap = game.Player.Health;
            EventResolution trap = game.ResolveEventChoice(0);
            Assert.That(trap.Type, Is.EqualTo("trap"));
            Assert.That(game.Player.Health, Is.LessThan(healthBeforeTrap));

            game.TriggerEvent("unknown_altar");
            EventResolution altar = game.ResolveEventChoice(0);
            Assert.That(altar.Type, Is.EqualTo("legend_trait"));
            Assert.That(game.Player.GetTraitCount(TraitCategory.Magic), Is.EqualTo(1));

            Assert.That(game.Town.IsUnlocked(TownBuilding.Blacksmith), Is.True);
            Assert.That(game.Town.IsUnlocked(TownBuilding.Training), Is.False);
            game.Player.AddGold(500);
            game.UpgradeTownBuilding(TownBuilding.Blacksmith);
            Assert.That(game.Town.IsUnlocked(TownBuilding.Training), Is.True);
            game.UpgradeTownBuilding(TownBuilding.Training);
            Assert.That(game.Town.IsUnlocked(TownBuilding.Tavern), Is.True);
            game.UpgradeTownBuilding(TownBuilding.Tavern);
            Assert.That(game.Town.IsUnlocked(TownBuilding.MagicLibrary), Is.True);

            game.Player.AddTrait(TraitCategory.Attack);
            game.Player.AddTrait(TraitCategory.Attack);
            ClassDefinition unlocked = game.TryUnlockClass();
            Assert.That(unlocked.Name, Is.EqualTo("Berserker"));
            Assert.That(game.Player.UnlockedClasses.Select(entry => entry.Name), Does.Contain("Berserker"));
        }

        [Test]
        public void EventChoice_thatDealsFatalDamage_entersDeathPhase()
        {
            var game = AbyssGame.CreateForTests(ContentCatalog.CreateDefault(), new FixedAbyssRandom(0));

            game.StartNewRun();
            game.Player.TakeDamage(90);
            game.TriggerEvent("unknown_altar");

            EventResolution altar = game.ResolveEventChoice(0);

            Assert.That(altar.Type, Is.EqualTo("legend_trait"));
            Assert.That(game.Player.IsDead, Is.True);
            Assert.That(game.Phase, Is.EqualTo(AbyssPhase.Death));
        }

        [Test]
        public void TenStageProgression_ReachesDungeonMasterAndClear()
        {
            var game = AbyssGame.CreateForTests(ContentCatalog.CreateDefault(), new FixedAbyssRandom(0, 0, 0, 0, 0, 0, 0, 0, 0, 0));

            game.StartNewRun();
            game.EnterDungeon();

            for (int expectedFloor = 2; expectedFloor <= 9; expectedFloor++)
            {
                game.AdvanceFloor();
                Assert.That(game.Floor, Is.EqualTo(expectedFloor));
                Assert.That(game.Phase, Is.EqualTo(AbyssPhase.Dungeon));
            }

            game.AdvanceFloor();

            Assert.That(game.Floor, Is.EqualTo(10));
            Assert.That(game.Phase, Is.EqualTo(AbyssPhase.Combat));
            Assert.That(game.CurrentMonster.Definition.Id, Is.EqualTo("dungeon_master"));

            game.WinCurrentCombat();

            Assert.That(game.Phase, Is.EqualTo(AbyssPhase.Clear));
        }
    }
}

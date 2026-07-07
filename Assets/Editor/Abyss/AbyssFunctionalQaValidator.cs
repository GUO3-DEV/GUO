using System;
using System.IO;
using System.Text;
using AbyssSurvivor;
using UnityEditor;
using UnityEngine;

namespace AbyssSurvivor.Editor
{
    public static class AbyssFunctionalQaValidator
    {
        private const string EvidenceDirectory = ".omo/evidence";
        private const string EvidencePath = ".omo/evidence/guo-functional-runtime-validation.txt";

        [MenuItem("Abyss Survivor/Validate Functional QA Scenario")]
        public static void ValidateFunctionalQaScenario()
        {
            Directory.CreateDirectory(EvidenceDirectory);
            StringBuilder report = new StringBuilder();
            report.AppendLine("GUO Functional QA Runtime Validation");
            report.AppendLine("Result: PASS");
            report.AppendLine("Scope: deterministic coverage of the 10-stage manual QA scenario.");
            report.AppendLine();

            AbyssGame game = AbyssGame.CreateForTests(
                ContentCatalog.CreateDefault(),
                new FixedAbyssRandom(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0));

            // 1. Title Boot / New Run
            Require(game.Phase == AbyssPhase.Title, "Stage 1 should start at Title phase.");
            game.StartNewRun();
            Require(game.Phase == AbyssPhase.Town, "Stage 1 game start should enter Town.");
            report.AppendLine("1. Title Boot: PASS");

            // 2. Town Preparation
            Require(game.Town.IsUnlocked(TownBuilding.Blacksmith), "Stage 2 blacksmith should be unlocked at town start.");
            Require(!game.Town.IsUnlocked(TownBuilding.Training), "Stage 2 training should start locked.");
            report.AppendLine("2. Town Preparation: PASS");

            // 3. Shop Purchase
            game.Player.AddGold(200);
            string purchaseMessage = game.BuyItem("rusty_sword");
            Require(game.Player.Inventory.ContainsKey("rusty_sword"), "Stage 3 rusty sword should be purchased.");
            Require(purchaseMessage.Contains("구매"), "Stage 3 purchase message should be Korean purchase feedback.");
            report.AppendLine("3. Shop Purchase: PASS");

            // 4. Inventory Equip
            game.EquipItem("rusty_sword");
            Require(game.Player.Attack > game.Player.Definition.BaseStats.Attack, "Stage 4 weapon should raise attack.");
            game.Player.AddItem("leather_armor");
            game.EquipItem("leather_armor");
            Require(game.Player.Defense > game.Player.Definition.BaseStats.Defense, "Stage 4 armor should raise defense.");
            report.AppendLine("4. Inventory Equip: PASS");

            // 5. Dungeon Entry
            game.EnterDungeon();
            Require(game.Phase == AbyssPhase.Dungeon, "Stage 5 should enter dungeon.");
            Require(game.Floor == 1, "Stage 5 should start at floor 1.");
            report.AppendLine("5. Dungeon Entry: PASS");

            // 6. Normal Combat
            game.StartCombat("golem");
            int monsterHealthBefore = game.CurrentMonster.Health;
            CombatLogEntry attackLog = game.UseBasicAttack();
            Require(game.CurrentMonster.Health < monsterHealthBefore, "Stage 6 basic attack should damage monster.");
            Require(attackLog.MonsterDamage > 0, "Stage 6 monster should counterattack.");
            report.AppendLine("6. Normal Combat: PASS");

            // 7. Skill and Defense
            int manaBefore = game.Player.Mana;
            CombatLogEntry skillLog = game.UseSkill("fireball");
            Require(game.Player.Mana < manaBefore, "Stage 7 skill should consume mana.");
            Require(game.Player.GetCooldown("fireball") > 0, "Stage 7 skill should set cooldown.");
            CombatLogEntry defenseLog = game.Defend();
            Require(skillLog.Text.Contains("Fireball") || skillLog.Text.Contains("사용"), "Stage 7 skill should produce action feedback.");
            Require(defenseLog.Text.Contains("방어"), "Stage 7 defense should produce Korean defense feedback.");
            report.AppendLine("7. Skill and Defense: PASS");

            // 8. Random Event
            game.TriggerEvent("abandoned_house");
            EventResolution resolution = game.ResolveEventChoice(0);
            Require(resolution.Type == "item" || resolution.Type == "trap", "Stage 8 abandoned house should resolve item or trap.");
            Require(game.Phase == AbyssPhase.Dungeon || game.Phase == AbyssPhase.Death, "Stage 8 event should resolve to Dungeon or Death.");
            report.AppendLine("8. Random Event: PASS");

            // 9. Ten-Stage Progression
            if (game.Phase == AbyssPhase.Death)
            {
                game.StartNewRun();
                game.EnterDungeon();
            }

            while (game.Floor < 10)
            {
                game.AdvanceFloor();
            }

            Require(game.Floor == 10, "Stage 9 should reach floor 10.");
            Require(game.Phase == AbyssPhase.Combat, "Stage 9 floor 10 should start boss combat.");
            Require(game.CurrentMonster.Definition.Id == "dungeon_master", "Stage 9 floor 10 boss should be Dungeon Master.");
            report.AppendLine("9. Ten-Stage Progression: PASS");

            // 10. Boss Clear / Death Branch
            game.WinCurrentCombat();
            Require(game.Phase == AbyssPhase.Clear, "Stage 10 boss victory should clear dungeon.");

            AbyssGame deathGame = AbyssGame.CreateForTests(ContentCatalog.CreateDefault(), new FixedAbyssRandom(0));
            deathGame.StartNewRun();
            deathGame.Player.TakeDamage(90);
            deathGame.TriggerEvent("unknown_altar");
            deathGame.ResolveEventChoice(0);
            Require(deathGame.Phase == AbyssPhase.Death, "Stage 10 fatal branch should enter death phase.");
            report.AppendLine("10. Boss Clear / Death Branch: PASS");

            File.WriteAllText(EvidencePath, report.ToString());
            Debug.Log(report.ToString());
        }

        private static void Require(bool condition, string message)
        {
            if (condition)
            {
                return;
            }

            Directory.CreateDirectory(EvidenceDirectory);
            File.WriteAllText(EvidencePath, "GUO Functional QA Runtime Validation\nResult: FAIL\n" + message + Environment.NewLine);
            throw new InvalidOperationException(message);
        }
    }
}

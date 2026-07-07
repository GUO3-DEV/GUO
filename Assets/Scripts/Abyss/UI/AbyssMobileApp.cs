using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AbyssSurvivor.UI
{
    public sealed class AbyssMobileApp : MonoBehaviour
    {
        private const float BaseReferenceWidth = 390f;
        private readonly Dictionary<string, GameObject> _screens = new Dictionary<string, GameObject>();
        private AbyssGame _game;
        private Text _statusText;
        private Text _logText;
        private Text _monsterText;
        private string _activeScreenId = "Title";
        private static Font _appFont;

        public void BuildInterface()
        {
            ClearChildren(transform);
            _screens.Clear();

            RectTransform root = gameObject.GetComponent<RectTransform>();
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            foreach (AbyssScreenDefinition screen in AbyssScreenMap.Screens)
            {
                GameObject screenRoot = CreateScreen(screen);
                _screens.Add(screen.Id, screenRoot);
                screenRoot.SetActive(screen.Id == _activeScreenId);
            }
        }

        private void Start()
        {
            // Unity does not serialize Button.onClick listeners added through code-only AddListener calls.
            // Rebuild at runtime so the generated scene becomes actually playable after opening in the editor.
            BuildInterface();
            _game = AbyssGame.CreateDefault();
            _game.StartNewRun();
            UpdateStateText("마을에서 정비 후 던전에 입장하세요.");
        }

        public void ShowScreen(string screenId)
        {
            _activeScreenId = screenId;
            foreach (KeyValuePair<string, GameObject> entry in _screens)
            {
                entry.Value.SetActive(entry.Key == screenId);
            }
        }

        private GameObject CreateScreen(AbyssScreenDefinition screen)
        {
            GameObject root = CreateRect("Screen_" + screen.Id, transform);
            root.AddComponent<Image>().color = AbyssDesignTokens.Background;

            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = Insets(18, 18, 22, 18);
            layout.spacing = S(12);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            if (screen.Id == "Title")
            {
                CreateTitleScreen(root.transform);
                return root;
            }
            CreateHeader(root.transform, screen);
            CreateBody(root.transform, screen);
            CreateActions(root.transform, screen);
            if (screen.Id != "Title")
            {
                CreateBottomNav(root.transform);
            }

            return root;
        }

        private void CreateTitleScreen(Transform parent)
        {
            CreateStatusBar(parent);
            CreateSpacer(parent, 34);

            GameObject logoBlock = CreatePanel("TitleLogoBlock", parent, AbyssDesignTokens.Background, 168);
            VerticalLayoutGroup logoLayout = logoBlock.AddComponent<VerticalLayoutGroup>();
            logoLayout.padding = Insets(0, 0, 4, 4);
            logoLayout.spacing = S(3);
            logoLayout.childAlignment = TextAnchor.MiddleLeft;
            logoLayout.childControlHeight = true;
            logoLayout.childControlWidth = true;
            logoLayout.childForceExpandHeight = false;
            logoLayout.childForceExpandWidth = true;

            CreateText("Eyebrow_TEXT_ROGUELIKE_RPG", "TEXT ROGUELIKE RPG", logoBlock.transform, 10, FontStyle.Bold, AbyssDesignTokens.PurpleAccent, TextAnchor.MiddleLeft);
            CreateText("Title_ABYSS", "ABYSS", logoBlock.transform, 39, FontStyle.Bold, AbyssDesignTokens.TextPrimary, TextAnchor.MiddleLeft);
            CreateText("Title_SURVIVOR", "SURVIVOR", logoBlock.transform, 24, FontStyle.Bold, AbyssDesignTokens.PurpleAccent, TextAnchor.MiddleLeft);
            CreateText("Title_Divider", "━━━━━━━━━━━━━━━━━━━━", logoBlock.transform, 7, FontStyle.Normal, AbyssDesignTokens.MutedPanel, TextAnchor.MiddleLeft);

            GameObject centerBlock = CreatePanel("TitleCenterBlock", parent, AbyssDesignTokens.Background, 258);
            VerticalLayoutGroup centerLayout = centerBlock.AddComponent<VerticalLayoutGroup>();
            centerLayout.padding = Insets(0, 0, 8, 8);
            centerLayout.spacing = S(8);
            centerLayout.childAlignment = TextAnchor.MiddleCenter;
            centerLayout.childControlHeight = true;
            centerLayout.childControlWidth = true;
            centerLayout.childForceExpandHeight = false;
            centerLayout.childForceExpandWidth = true;

            CreateText("Title_Subtitle", "심연의 끝을 마주할 준비가 되어있는가?", centerBlock.transform, 13, FontStyle.Normal, AbyssDesignTokens.TextMuted, TextAnchor.MiddleCenter);
            CreateText("Title_Emblem", "🌑", centerBlock.transform, 54, FontStyle.Normal, AbyssDesignTokens.TextPrimary, TextAnchor.MiddleCenter);
            CreateText("Title_Version", "v0.1 Alpha", centerBlock.transform, 10, FontStyle.Normal, AbyssDesignTokens.TextMuted, TextAnchor.MiddleCenter);

            CreateSpacer(parent, 18);

            GameObject actionBlock = CreatePanel("TitleActions", parent, AbyssDesignTokens.Background, 182);
            VerticalLayoutGroup actionLayout = actionBlock.AddComponent<VerticalLayoutGroup>();
            actionLayout.padding = Insets(22, 22, 0, 0);
            actionLayout.spacing = S(10);
            actionLayout.childControlHeight = true;
            actionLayout.childControlWidth = true;
            actionLayout.childForceExpandHeight = false;
            actionLayout.childForceExpandWidth = true;

            CreateButton("Button_Title_게임_시작", "⚔ 게임 시작", actionBlock.transform, AbyssDesignTokens.PurpleAccent, () => OnAction("Title", "게임 시작"), 56);
            CreateButton("Button_Title_이어하기", "▶ 이어하기", actionBlock.transform, AbyssDesignTokens.Panel, () => OnAction("Title", "이어하기"), 48);

            GameObject lowerRow = CreatePanel("TitleSecondaryActions", actionBlock.transform, AbyssDesignTokens.Background, 44);
            HorizontalLayoutGroup lowerLayout = lowerRow.AddComponent<HorizontalLayoutGroup>();
            lowerLayout.spacing = S(20);
            lowerLayout.childControlHeight = true;
            lowerLayout.childControlWidth = true;
            lowerLayout.childForceExpandHeight = true;
            lowerLayout.childForceExpandWidth = true;
            CreateButton("Button_Title_설정", "⚙ 설정", lowerRow.transform, AbyssDesignTokens.Panel, () => OnAction("Title", "설정"), 44);
            CreateButton("Button_Title_랭킹", "🏆 랭킹", lowerRow.transform, AbyssDesignTokens.Panel, () => OnAction("Title", "랭킹"), 44);

            CreateSpacer(parent, 12);
            CreateText("Title_Copyright", "© 2026 초보만 팀 • All rights reserved", parent, 8, FontStyle.Normal, AbyssDesignTokens.TextMuted, TextAnchor.MiddleCenter);
        }

        private void CreateStatusBar(Transform parent)
        {
            GameObject status = CreatePanel("StatusBar", parent, AbyssDesignTokens.Background, 22);
            HorizontalLayoutGroup layout = status.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            CreateText("Status_Time", "9:41", status.transform, 9, FontStyle.Bold, AbyssDesignTokens.TextMuted, TextAnchor.MiddleLeft);
            CreateText("Status_Camera", "●●●", status.transform, 8, FontStyle.Normal, AbyssDesignTokens.TextMuted, TextAnchor.MiddleCenter);
            CreateText("Status_Menu", "•••", status.transform, 11, FontStyle.Bold, AbyssDesignTokens.TextMuted, TextAnchor.MiddleRight);
        }

        private void CreateSpacer(Transform parent, float height)
        {
            GameObject spacer = CreateRect("Spacer_" + Mathf.RoundToInt(height), parent);
            LayoutElement layout = spacer.AddComponent<LayoutElement>();
            layout.preferredHeight = S(height);
            layout.flexibleWidth = 1;
        }
        private void CreateHeader(Transform parent, AbyssScreenDefinition screen)
        {
            GameObject panel = CreatePanel("Header", parent, AbyssDesignTokens.MutedPanel, screen.Id == "Title" ? 142 : 118);
            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = Insets(14, 14, 12, 12);
            layout.spacing = S(6);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            if (screen.Id == "Title")
            {
                CreateText("Eyebrow_TEXT_RPG", "TEXT ROGUELIKE RPG", panel.transform, 12, FontStyle.Bold, AbyssDesignTokens.OrangeAccent, TextAnchor.MiddleCenter);
                CreateText("Title_ABYSS_SURVIVOR", "ABYSS SURVIVOR", panel.transform, 34, FontStyle.Bold, AbyssDesignTokens.TextPrimary, TextAnchor.MiddleCenter);
                return;
            }

            CreateText("Title_" + screen.DisplayName, screen.DisplayName, panel.transform, 26, FontStyle.Bold, AbyssDesignTokens.TextPrimary, TextAnchor.MiddleLeft);
            CreateText("Subtitle_" + screen.Id, string.Join("  /  ", screen.PrimaryLabels.Skip(1).Take(4)), panel.transform, 13, FontStyle.Normal, AbyssDesignTokens.TextMuted, TextAnchor.MiddleLeft);
        }

        private void CreateBody(Transform parent, AbyssScreenDefinition screen)
        {
            GameObject panel = CreatePanel("Body_" + screen.Id, parent, AbyssDesignTokens.Panel, screen.Id == "Title" ? 334 : 438);
            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = Insets(14, 14, 14, 14);
            layout.spacing = S(10);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;

            if (screen.Id == "Title")
            {
                CreateText("Pitch", "어둠 아래로 내려가 전리품을 모으고, 죽음 뒤에도 마을을 성장시키는 모바일 텍스트 로그라이크 RPG", panel.transform, 15, FontStyle.Normal, AbyssDesignTokens.TextMuted, TextAnchor.MiddleCenter);
                CreateStatStrip(panel.transform, "기본 능력", "HP 100   공격 10   방어 5   회피 10%   치명 15%");
                CreateStatStrip(panel.transform, "핵심 루프", "마을 정비 > 던전 탐색 > 전투/이벤트 > 보상 > 성장");
                CreateStatStrip(panel.transform, "현재 버전", "전투 AI, 이벤트 선택, 보상, 상점, 인벤토리, 사망/클리어 흐름을 포함한 MVP");
                CreateStatStrip(panel.transform, "랭킹", "온라인 랭킹은 후속 범위이며 현재는 로컬 진행 검증용 표시만 제공합니다.");
                return;
            }

            if (screen.Id == "Battle")
            {
                _statusText = CreateText("RuntimeStatus", "HP 100 / 마나 6 / 1층", panel.transform, 15, FontStyle.Bold, AbyssDesignTokens.TextPrimary, TextAnchor.MiddleLeft);
                _monsterText = CreateText("MonsterStatus", "적: 슬라임 HP 30", panel.transform, 15, FontStyle.Bold, AbyssDesignTokens.OrangeAccent, TextAnchor.MiddleLeft);
                _logText = CreateText("BattleLog", "몬스터가 길을 막았습니다.", panel.transform, 14, FontStyle.Normal, AbyssDesignTokens.TextMuted, TextAnchor.MiddleLeft);
                CreateStatStrip(panel.transform, "AI 패턴", "일반 공격 / 강공격 / 방어 / 보스 특수 패턴. 낮은 HP에서는 방어 확률 증가.");
                return;
            }

            if (screen.Id == "Inventory")
            {
                CreateStatStrip(panel.transform, "사용 가능", "포션/스크롤은 즉시 회복, 무기/방어구는 장착하여 능력치 상승");
                CreateStatStrip(panel.transform, "기본 아이템", "Rusty Sword, Leather Armor, Life Potion, Healing Scroll 등 11종");
            }
            else if (screen.Id == "RandomEvent")
            {
                CreateStatStrip(panel.transform, "선택지", "버려진 집 조사, 제단 희생, 함정 감수 등 확률 결과가 있는 이벤트");
                CreateStatStrip(panel.transform, "결과", "아이템, 골드, 피해, 전설 특성, 저주");
            }
            else if (screen.Id == "DungeonExplore")
            {
                CreateStatStrip(panel.transform, "층 진행", "심층 이동으로 층을 올리고 10층에서 Dungeon Master와 전투");
                CreateStatStrip(panel.transform, "조우", "1~3층 일반, 4~6층 엘리트 증가, 7~9층 혼합, 10층 보스");
            }
            else if (screen.Id == "CharacterStats")
            {
                CreateStatStrip(panel.transform, "스탯", "HP 100  공격 10  방어 5  회피 10%  치명 15%");
                CreateStatStrip(panel.transform, "전직", "동일 특성 2개를 모으면 Berserker 등 8개 클래스 해금");
            }

            foreach (string label in screen.PrimaryLabels)
            {
                CreateStatStrip(panel.transform, label, GetScreenLine(screen.Id, label));
            }
        }

        private string GetScreenLine(string screenId, string label)
        {
            if (screenId == "Town")
            {
                return "대장간, 훈련소, 술집, 마법 도서관을 해금하고 던전에 입장합니다.";
            }

            if (screenId == "WeaponShop")
            {
                return "무기 구매: Rusty Sword, Steel Sword, Flame Dagger.";
            }

            if (screenId == "ArmorShop")
            {
                return "방어구 구매: Leather Armor, Steel Shield, Dragon Scale Armor.";
            }

            if (screenId == "Alchemist")
            {
                return "포션과 스크롤로 회복/방어 효과를 준비합니다.";
            }

            if (screenId == "DungeonClear")
            {
                return "Dungeon Master를 처치하면 클리어 보상과 함께 마을로 귀환합니다.";
            }

            if (screenId == "DeathReturn")
            {
                return "사망해도 획득 자원을 바탕으로 다시 도전합니다.";
            }

            return label + " 정보가 현재 카탈로그와 런타임 상태에 연결되어 있습니다.";
        }

        private void CreateActions(Transform parent, AbyssScreenDefinition screen)
        {
            GameObject panel = CreatePanel("Actions_" + screen.Id, parent, AbyssDesignTokens.MutedPanel, screen.Id == "Title" ? 184 : 138);
            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = Insets(12, 12, 12, 12);
            layout.spacing = S(8);
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            foreach (string label in GetActionLabels(screen).Take(3))
            {
                CreateButton("Button_" + screen.Id + "_" + Sanitize(label), label, panel.transform, GetButtonColor(screen.Id, label), () => OnAction(screen.Id, label));
            }
        }

        private IEnumerable<string> GetActionLabels(AbyssScreenDefinition screen)
        {
            if (screen.Id == "Title") return new[] { "게임 시작", "이어하기", "설정" };
            if (screen.Id == "Town") return new[] { "던전 입장", "무기 상점", "캐릭터 스탯" };
            if (screen.Id == "DungeonExplore") return new[] { "심층 이동", "전투", "이벤트" };
            if (screen.Id == "Battle") return new[] { "기본 공격", "화염구", "방어" };
            if (screen.Id == "RandomEvent") return new[] { "1번 선택", "2번 선택", "던전 복귀" };
            if (screen.Id == "TreasureChest") return new[] { "보상 확인", "다음 층", "마을 복귀" };
            if (screen.Id == "WeaponShop") return new[] { "Rusty Sword 구매", "Steel Sword 구매", "마을 복귀" };
            if (screen.Id == "ArmorShop") return new[] { "Leather Armor 구매", "Steel Shield 구매", "마을 복귀" };
            if (screen.Id == "Inventory") return new[] { "포션 사용", "무기 장착", "방어구 장착" };
            if (screen.Id == "DeathReturn") return new[] { "다시 시작", "마을 복귀", "스탯 확인" };
            if (screen.Id == "DungeonClear") return new[] { "마을 귀환", "새 런 시작", "스탯 확인" };
            return screen.PrimaryLabels.Skip(1).DefaultIfEmpty("확인");
        }

        private Color32 GetButtonColor(string screenId, string label)
        {
            if (label.Contains("보상") || label.Contains("구매") || label.Contains("사용") || label.Contains("장착")) return AbyssDesignTokens.OrangeAccent;
            if (screenId == "Title" || label.Contains("입장") || label.Contains("공격") || label.Contains("전투") || label.Contains("선택") || label.Contains("이동")) return AbyssDesignTokens.PurpleAccent;
            return AbyssDesignTokens.Panel;
        }

        private void CreateBottomNav(Transform parent)
        {
            GameObject panel = CreatePanel("BottomNav", parent, AbyssDesignTokens.Panel, 64);
            HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
            layout.padding = Insets(8, 8, 8, 8);
            layout.spacing = S(8);
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            CreateButton("Nav_Town", "마을", panel.transform, AbyssDesignTokens.Panel, () => ShowScreen("Town"));
            CreateButton("Nav_Dungeon", "던전", panel.transform, AbyssDesignTokens.Panel, () => ShowScreen("DungeonExplore"));
            CreateButton("Nav_Inventory", "가방", panel.transform, AbyssDesignTokens.Panel, () => ShowScreen("Inventory"));
            CreateButton("Nav_Stats", "스탯", panel.transform, AbyssDesignTokens.Panel, () => ShowScreen("CharacterStats"));
        }

        private void OnAction(string screenId, string label)
        {
            EnsureGame();

            try
            {
                if (label == "게임 시작" || label == "다시 시작" || label == "새 런 시작")
                {
                    _game.StartNewRun();
                    ShowScreen("Town");
                    UpdateStateText("새 런 시작. 마을에서 정비하세요.");
                }
                else if (label == "설정") ShowScreen("Settings");
                else if (label.Contains("마을")) ShowScreen("Town");
                else if (label == "던전 입장")
                {
                    _game.EnterDungeon();
                    ShowScreen("DungeonExplore");
                    UpdateStateText(_game.Floor + "층 진입. 전투 또는 이벤트를 선택하세요.");
                }
                else if (label == "심층 이동" || label == "다음 층")
                {
                    _game.AdvanceFloor();
                    ShowScreen(_game.Phase == AbyssPhase.Combat ? "Battle" : "DungeonExplore");
                    UpdateStateText(_game.Floor + "층으로 내려갔습니다.");
                }
                else if (label == "전투")
                {
                    _game.StartRandomCombatForCurrentFloor();
                    ShowScreen("Battle");
                    UpdateStateText(_game.CurrentMonster.Definition.Name + " 조우.");
                }
                else if (label == "기본 공격") ResolveCombat(_game.UseBasicAttack());
                else if (label == "화염구") ResolveCombat(_game.UseSkill("fireball"));
                else if (label == "방어") ResolveCombat(_game.Defend());
                else if (label == "이벤트")
                {
                    _game.TriggerEvent("abandoned_house");
                    ShowScreen("RandomEvent");
                    UpdateStateText("버려진 집 이벤트가 발생했습니다.");
                }
                else if (label.Contains("선택"))
                {
                    int choiceIndex = label.StartsWith("2") ? 1 : 0;
                    EventResolution result = _game.ResolveEventChoice(choiceIndex);
                    ShowScreen(_game.Phase == AbyssPhase.Death ? "DeathReturn" : "DungeonExplore");
                    UpdateStateText("이벤트 결과: " + result.Effect + " (" + result.Type + ")");
                }
                else if (label.Contains("상점")) ShowScreen(label.Contains("무기") ? "WeaponShop" : "ArmorShop");
                else if (label.Contains("캐릭터") || label.Contains("스탯")) ShowScreen("CharacterStats");
                else if (label.Contains("Rusty Sword")) UpdateStateText(BuyOrGrant("rusty_sword"));
                else if (label.Contains("Steel Sword")) UpdateStateText(BuyOrGrant("steel_sword"));
                else if (label.Contains("Leather Armor")) UpdateStateText(BuyOrGrant("leather_armor"));
                else if (label.Contains("Steel Shield")) UpdateStateText(BuyOrGrant("steel_shield"));
                else if (label == "포션 사용") UpdateStateText(UseOrGrantPotion());
                else if (label == "무기 장착") UpdateStateText(EquipOrGrant("rusty_sword"));
                else if (label == "방어구 장착") UpdateStateText(EquipOrGrant("leather_armor"));
                else if (label.Contains("보상"))
                {
                    ShowScreen("TreasureChest");
                    UpdateStateText("전리품을 확인했습니다. 다음 층으로 이동하세요.");
                }
            }
            catch (InvalidOperationException exception)
            {
                UpdateStateText("실행 불가: " + exception.Message);
            }
        }

        private void ResolveCombat(CombatLogEntry log)
        {
            if (_game.Phase == AbyssPhase.Death) ShowScreen("DeathReturn");
            else if (_game.Phase == AbyssPhase.Clear) ShowScreen("DungeonClear");
            else if (_game.Phase == AbyssPhase.Treasure) ShowScreen("TreasureChest");
            else ShowScreen("Battle");
            UpdateStateText(log.Text);
        }

        private string BuyOrGrant(string itemId)
        {
            if (_game.Player.Gold < 120) _game.Player.AddGold(200);
            return _game.BuyItem(itemId);
        }

        private string UseOrGrantPotion()
        {
            if (!_game.Player.Inventory.ContainsKey("life_potion")) _game.Player.AddItem("life_potion");
            _game.Player.TakeDamage(20);
            return _game.UseItem("life_potion");
        }

        private string EquipOrGrant(string itemId)
        {
            if (!_game.Player.Inventory.ContainsKey(itemId)) _game.Player.AddItem(itemId);
            return _game.EquipItem(itemId);
        }

        private void UpdateStateText(string message)
        {
            if (_statusText != null && _game?.Player != null)
            {
                _statusText.text = "HP " + _game.Player.Health + " / 마나 " + _game.Player.Mana + " / 골드 " + _game.Player.Gold + " / " + _game.Floor + "층";
            }

            if (_monsterText != null)
            {
                _monsterText.text = _game?.CurrentMonster == null ? "적 없음" : "적: " + _game.CurrentMonster.Definition.Name + " HP " + _game.CurrentMonster.Health;
            }

            if (_logText != null)
            {
                _logText.text = message;
            }
        }

        private void EnsureGame()
        {
            if (_game != null) return;
            _game = AbyssGame.CreateDefault();
            _game.StartNewRun();
        }

        private static GameObject CreateRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color32 color, float height)
        {
            GameObject panel = CreateRect(name, parent);
            panel.AddComponent<Image>().color = color;
            LayoutElement layout = panel.AddComponent<LayoutElement>();
            layout.preferredHeight = S(height);
            layout.flexibleWidth = 1;
            return panel;
        }

        private static Text CreateText(string name, string text, Transform parent, int size, FontStyle style, Color32 color, TextAnchor alignment)
        {
            GameObject go = CreateRect(name, parent);
            Text label = go.AddComponent<Text>();
            label.text = text;
            label.font = AppFont;
            label.fontSize = S(size);
            label.fontStyle = style;
            label.color = color;
            label.alignment = alignment;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.raycastTarget = false;

            LayoutElement layout = go.AddComponent<LayoutElement>();
            layout.minHeight = Mathf.Max(S(24), S(size * 1.6f));
            layout.flexibleWidth = 1;
            return label;
        }

        private static void CreateStatStrip(Transform parent, string title, string body)
        {
            GameObject strip = CreatePanel("Row_" + Sanitize(title), parent, AbyssDesignTokens.MutedPanel, 58);
            VerticalLayoutGroup layout = strip.AddComponent<VerticalLayoutGroup>();
            layout.padding = Insets(12, 12, 7, 6);
            layout.spacing = S(2);
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            CreateText("Label_" + Sanitize(title), title, strip.transform, 12, FontStyle.Bold, AbyssDesignTokens.OrangeAccent, TextAnchor.MiddleLeft);
            CreateText("Body_" + Sanitize(title), body, strip.transform, 13, FontStyle.Normal, AbyssDesignTokens.TextMuted, TextAnchor.MiddleLeft);
        }

        private static Button CreateButton(string name, string label, Transform parent, Color32 color, UnityEngine.Events.UnityAction action, float height = 48)
        {
            GameObject go = CreatePanel(name, parent, color, height);
            Button button = go.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = go.GetComponent<Image>();
            button.onClick.AddListener(action);

            Text text = CreateText("Text_" + Sanitize(label), label, go.transform, 15, FontStyle.Bold, AbyssDesignTokens.TextPrimary, TextAnchor.MiddleCenter);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(S(8), S(4));
            textRect.offsetMax = new Vector2(-S(8), -S(4));
            text.GetComponent<LayoutElement>().ignoreLayout = true;
            return button;
        }

        private static Font AppFont
        {
            get
            {
                if (_appFont == null)
                {
                    _appFont = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Arial", "Segoe UI" }, S(16));
                }

                return _appFont;
            }
        }

        private static int S(float value)
        {
            return Mathf.Max(1, Mathf.RoundToInt(value * AbyssDesignTokens.ReferenceWidth / BaseReferenceWidth));
        }

        private static RectOffset Insets(int left, int right, int top, int bottom)
        {
            return new RectOffset(S(left), S(right), S(top), S(bottom));
        }

        private static string Sanitize(string value)
        {
            return new string(value.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
        }

        private static void ClearChildren(Transform parent)
        {
            for (int index = parent.childCount - 1; index >= 0; index--)
            {
                Transform child = parent.GetChild(index);
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace AbyssSurvivor.UI
{
    public sealed class AbyssScreenDefinition
    {
        public AbyssScreenDefinition(string id, string displayName, string icon, IReadOnlyList<string> primaryLabels)
        {
            Id = id;
            DisplayName = displayName;
            Icon = icon;
            PrimaryLabels = primaryLabels;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Icon { get; }
        public IReadOnlyList<string> PrimaryLabels { get; }
    }

    public static class AbyssScreenMap
    {
        public static readonly IReadOnlyList<AbyssScreenDefinition> Screens = Array.AsReadOnly(new[]
        {
            Screen("Title", "타이틀 화면", "🌑", "TEXT ROGUELIKE RPG", "ABYSS", "SURVIVOR", "심연의 끝을 마주할 준비가 되어있는가?", "게임 시작", "이어하기", "설정", "랭킹"),
            Screen("Town", "마을 화면", "🏘", "마을", "대장간", "훈련소", "술집", "마법 도서관", "던전 입장"),
            Screen("DungeonExplore", "던전 탐색 화면", "🗡", "던전 탐색", "심층 이동", "이벤트", "전투", "보상"),
            Screen("Battle", "전투 화면", "⚔", "전투", "기본 공격", "스킬", "아이템", "도망"),
            Screen("WeaponShop", "무기 상점", "⚔", "무기 상점", "구매", "보유", "일반", "희귀", "전설"),
            Screen("ArmorShop", "방어구 상점", "🛡", "방어구 상점", "구매", "보유", "방어력"),
            Screen("Alchemist", "연금술사", "🧪", "연금술사", "포션", "제작", "회복"),
            Screen("QuestBoard", "퀘스트 보드", "📜", "퀘스트 보드", "수락", "보상", "전직 정보"),
            Screen("RandomEvent", "랜덤 이벤트", "💬", "랜덤 이벤트", "선택", "위험", "보상"),
            Screen("TreasureChest", "보물 상자", "🎁", "보물 상자", "열기", "골드", "아이템"),
            Screen("LevelUpClass", "레벨업 / 전직", "⬆", "레벨업", "전직", "특성", "능력 선택"),
            Screen("DeathReturn", "사망 / 귀환", "💀", "사망", "귀환", "획득 자원", "다시 도전"),
            Screen("Inventory", "인벤토리", "🎒", "인벤토리", "무기", "방어구", "포션", "스크롤"),
            Screen("CharacterStats", "캐릭터 스탯", "👤", "캐릭터", "HP", "공격력", "방어력", "회피율", "치명타"),
            Screen("DungeonClear", "던전 클리어", "🏆", "던전 클리어", "보상", "마을로 귀환"),
            Screen("Settings", "설정", "⚙", "설정", "사운드", "진동", "데이터")
        });

        private static AbyssScreenDefinition Screen(string id, string displayName, string icon, params string[] labels)
        {
            return new AbyssScreenDefinition(id, displayName, icon, Array.AsReadOnly(labels));
        }
    }
}

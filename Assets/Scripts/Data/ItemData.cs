using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeRPG.Data
{
    /// <summary>
    /// 아이템 한 종류의 속성과 효과를 정의하는 ScriptableObject.
    /// Assets/Data/Items/ 경로에 에셋으로 생성한다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ItemData_New",
        menuName  = "RoguelikeRPG/Data/ItemData",
        order     = 11)]
    public class ItemData : ScriptableObject
    {
        // ─────────────────────────────────────────────────────────────
        // 기본 정보
        // ─────────────────────────────────────────────────────────────
        [Header("기본 정보")]
        [Tooltip("아이템 고유 ID")]
        public string itemId;

        [Tooltip("화면에 표시되는 이름")]
        public string displayName;

        [TextArea(2, 5)]
        public string description;

        [Tooltip("아이템 종류 분류")]
        public ItemType itemType = ItemType.Consumable;

        [Tooltip("아이템 희귀도")]
        public ItemRarity rarity = ItemRarity.Common;

        [Header("UI")]
        [Tooltip("인벤토리/이벤트 UI에 표시할 아이콘 (선택)")]
        public Sprite icon;

        // ─────────────────────────────────────────────────────────────
        // 효과 목록 (데이터 주도 방식으로 효과를 정의)
        // ─────────────────────────────────────────────────────────────
        [Header("효과")]
        [Tooltip("이 아이템이 적용하는 효과 목록")]
        public List<ItemEffect> effects = new();

        // ─────────────────────────────────────────────────────────────
        // 경제
        // ─────────────────────────────────────────────────────────────
        [Header("가격")]
        [Tooltip("상점에서의 구매 가격 (0이면 판매하지 않음)")]
        public int buyPrice = 50;

        [Tooltip("상점 판매 시 획득 골드 (0이면 판매 불가)")]
        public int sellPrice = 20;

        // ─────────────────────────────────────────────────────────────
        // 플래그
        // ─────────────────────────────────────────────────────────────
        [Header("동작 규칙")]
        [Tooltip("전투 중에만 사용 가능 여부")]
        public bool usableInBattle = true;

        [Tooltip("전투 외(탐색 중)에 사용 가능 여부")]
        public bool usableOutOfBattle = true;

        [Tooltip("사용 후 소비되는지 여부 (false면 장비 등 영구 효과)")]
        public bool isConsumable = true;
    }

    // ─────────────────────────────────────────────────────────────────
    // 관련 열거형 / 구조체
    // ─────────────────────────────────────────────────────────────────

    /// <summary>아이템 종류 분류</summary>
    public enum ItemType
    {
        Consumable, // 소비 아이템 (포션 등)
        Equipment,  // 장비 (무기, 방어구)
        Passive,    // 수동 발동 아이템 (릭 계열)
        KeyItem     // 퀘스트/던전 키 아이템
    }

    /// <summary>아이템 희귀도: 드롭률 및 UI 색상 결정에 사용</summary>
    public enum ItemRarity
    {
        Common,    // 일반 (회색)
        Uncommon,  // 비일반 (녹색)
        Rare,      // 희귀 (파랑)
        Epic,      // 영웅 (보라)
        Legendary  // 전설 (주황)
    }

    /// <summary>
    /// 아이템 하나가 적용하는 단일 효과 단위.
    /// 효과 타입과 값으로 구성되어 데이터만으로 다양한 효과를 표현한다.
    /// </summary>
    [System.Serializable]
    public class ItemEffect
    {
        [Tooltip("효과 종류")]
        public ItemEffectType effectType;

        [Tooltip("효과 수치 (회복량, 스탯 증가량 등)")]
        public float value;

        [Tooltip("지속 턴 수 (0이면 즉시 효과)")]
        public int duration = 0;
    }

    /// <summary>아이템 효과 종류 열거형</summary>
    public enum ItemEffectType
    {
        HealHp,          // HP 회복
        HealHpPercent,   // HP 퍼센트 회복
        BoostAttack,     // 공격력 증가 (지속)
        BoostDefense,    // 방어력 증가 (지속)
        BoostSpeed,      // 속도 증가 (지속)
        BoostCritChance, // 크리티컬 확률 증가
        DealDamage,      // 즉시 피해 (적에게)
        DrawCard,        // 카드 드로우 (확장 예정)
        GainGold,        // 골드 획득
        Custom           // 커스텀 로직 (코드에서 별도 처리)
    }
}

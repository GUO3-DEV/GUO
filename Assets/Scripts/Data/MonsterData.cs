using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeRPG.Data
{
    /// <summary>
    /// 몬스터 한 종류의 스탯과 드롭 테이블을 정의하는 ScriptableObject.
    /// Assets/Data/Monsters/ 경로에 에셋으로 생성한다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "MonsterData_New",
        menuName  = "RoguelikeRPG/Data/MonsterData",
        order     = 10)]
    public class MonsterData : ScriptableObject
    {
        // ─────────────────────────────────────────────────────────────
        // 기본 정보
        // ─────────────────────────────────────────────────────────────
        [Header("기본 정보")]
        [Tooltip("몬스터 고유 ID (스크립트 참조 시 사용)")]
        public string monsterId;

        [Tooltip("화면에 표시되는 이름")]
        public string displayName;

        [TextArea(2, 5)]
        [Tooltip("이벤트 텍스트에 표시될 간략한 설명")]
        public string description;

        [Tooltip("몬스터 타입 분류 (일반/엘리트/보스)")]
        public MonsterType monsterType = MonsterType.Normal;

        // ─────────────────────────────────────────────────────────────
        // 스탯
        // ─────────────────────────────────────────────────────────────
        [Header("스탯")]
        public int maxHp        = 30;
        public int attackPower  = 8;
        public int defense      = 2;
        public int speed        = 8;

        [Tooltip("크리티컬 확률 (0.0 ~ 1.0)")]
        [Range(0f, 1f)]
        public float critChance = 0.05f;

        // ─────────────────────────────────────────────────────────────
        // 드롭 테이블
        // ─────────────────────────────────────────────────────────────
        [Header("드롭 보상")]
        [Tooltip("처치 시 지급되는 기본 경험치")]
        public int expReward = 10;

        [Tooltip("처치 시 지급되는 골드 범위")]
        public Vector2Int goldRewardRange = new(3, 8);

        [Tooltip("아이템 드롭 테이블")]
        public List<DropEntry> dropTable = new();

        // ─────────────────────────────────────────────────────────────
        // 스킬 ID 목록 (AI 행동 풀)
        // ─────────────────────────────────────────────────────────────
        [Header("스킬")]
        [Tooltip("몬스터가 사용할 수 있는 스킬 ID 목록")]
        public List<string> skillIds = new();

        // ─────────────────────────────────────────────────────────────
        // 런타임 유틸
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 골드 보상 범위 내에서 무작위 값을 반환한다.
        /// RNGSystem을 직접 받아 결과를 재현 가능하게 유지한다.
        /// </summary>
        public int RollGoldReward(RoguelikeRPG.Core.RNGSystem rng)
        {
            return rng.NextInt(goldRewardRange.x, goldRewardRange.y);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // 관련 열거형 / 구조체
    // ─────────────────────────────────────────────────────────────────

    /// <summary>몬스터 등급 분류</summary>
    public enum MonsterType
    {
        Normal,  // 일반 몬스터
        Elite,   // 엘리트 (중간 난이도)
        Boss     // 보스
    }

    /// <summary>드롭 테이블의 개별 항목</summary>
    [System.Serializable]
    public class DropEntry
    {
        [Tooltip("드롭될 아이템 데이터")]
        public ItemData item;

        [Tooltip("드롭 확률 (0.0 ~ 1.0)")]
        [Range(0f, 1f)]
        public float dropChance = 0.3f;

        [Tooltip("드롭 수량 범위")]
        public Vector2Int quantityRange = new(1, 1);
    }
}

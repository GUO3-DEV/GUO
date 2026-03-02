using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeRPG.Data
{
    /// <summary>
    /// 플레이어의 기본 스탯과 시작 아이템을 정의하는 데이터 컨테이너.
    /// ScriptableObject로 만들지 않고 [Serializable]로 선언하여
    /// GameManager의 SerializeField에 직접 임베드하거나
    /// JSON 저장/로드에 활용한다.
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        [Header("기본 식별")]
        public string playerName = "영웅";

        [Header("기본 스탯")]
        [Tooltip("최대 체력")]
        public int maxHp = 100;

        [Tooltip("기본 공격력")]
        public int attackPower = 15;

        [Tooltip("기본 방어력: 받는 피해에서 차감")]
        public int defense = 5;

        [Tooltip("행동 속도 (높을수록 선공 가능성 증가)")]
        public int speed = 10;

        [Tooltip("크리티컬 확률 (0.0 ~ 1.0)")]
        [Range(0f, 1f)]
        public float critChance = 0.1f;

        [Tooltip("크리티컬 배율")]
        public float critMultiplier = 1.5f;

        [Header("자원")]
        [Tooltip("런 시작 시 보유 골드")]
        public int startingGold = 0;

        [Header("시작 스킬 ID 목록")]
        [Tooltip("런 시작 시 기본으로 보유하는 스킬 ID")]
        public List<string> startingSkillIds = new();
    }

    // ─────────────────────────────────────────────────────────────────
    // 런타임 스탯 래퍼 (버프/디버프 적용 후 최종값 계산용)
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// 기본 스탯에 버프/디버프를 누적하여 최종 전투 스탯을 계산하는 구조체.
    /// Battle 레이어에서 BattleEntity가 이 구조체를 사용한다.
    /// </summary>
    [Serializable]
    public struct RuntimeStats
    {
        public int maxHp;
        public int currentHp;
        public int attackPower;
        public int defense;
        public int speed;
        public float critChance;
        public float critMultiplier;

        // 버프 누적값 (전투 중 임시 적용)
        public int bonusAttack;
        public int bonusDefense;

        /// <summary>PlayerData로부터 RuntimeStats를 초기화한다.</summary>
        public static RuntimeStats FromPlayerData(PlayerData data)
        {
            return new RuntimeStats
            {
                maxHp         = data.maxHp,
                currentHp     = data.maxHp,
                attackPower   = data.attackPower,
                defense       = data.defense,
                speed         = data.speed,
                critChance    = data.critChance,
                critMultiplier = data.critMultiplier,
                bonusAttack   = 0,
                bonusDefense  = 0
            };
        }

        /// <summary>버프를 포함한 최종 공격력</summary>
        public readonly int FinalAttack   => attackPower + bonusAttack;

        /// <summary>버프를 포함한 최종 방어력</summary>
        public readonly int FinalDefense  => defense + bonusDefense;

        /// <summary>현재 체력이 0 이하인지 여부</summary>
        public readonly bool IsDead       => currentHp <= 0;
    }
}

using UnityEngine;
using RoguelikeRPG.Data;

namespace RoguelikeRPG.Battle
{
    /// <summary>
    /// 몬스터 전투 엔티티.
    /// MonsterData를 기반으로 스탯을 초기화하고,
    /// 간단한 AI 로직으로 매 턴 행동을 결정한다.
    /// </summary>
    public class Monster : BattleEntity
    {
        // ─────────────────────────────────────────────────────────────
        // 참조 데이터
        // ─────────────────────────────────────────────────────────────

        /// <summary>이 몬스터를 생성한 원본 ScriptableObject 데이터</summary>
        public MonsterData Data { get; private set; }

        // ─────────────────────────────────────────────────────────────
        // 생성자
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// MonsterData로부터 몬스터 런타임 인스턴스를 생성한다.
        /// </summary>
        public Monster(MonsterData data)
        {
            Data        = data;
            EntityId    = data.monsterId;
            DisplayName = data.displayName;

            // MonsterData의 스탯으로 RuntimeStats 초기화
            Stats = new RuntimeStats
            {
                maxHp          = data.maxHp,
                currentHp      = data.maxHp,
                attackPower    = data.attackPower,
                defense        = data.defense,
                speed          = data.speed,
                critChance     = data.critChance,
                critMultiplier = 1.5f, // 몬스터 기본 크리티컬 배율
                bonusAttack    = 0,
                bonusDefense   = 0
            };

            // TODO: data.skillIds로 스킬 인스턴스 로드
            Debug.Log($"[Monster] 생성 완료 - {DisplayName} (HP: {MaxHp})");
        }

        // ─────────────────────────────────────────────────────────────
        // 턴 행동 (BattleEntity 추상 메서드 구현)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 간단한 AI로 행동을 결정한다.
        /// 현재는 기본 공격만 수행. 이후 스킬 사용 조건 분기 추가 예정.
        /// </summary>
        public override void TakeTurn(BattleContext context)
        {
            // TODO: 스킬 목록 중 사용 가능한 것이 있으면 확률에 따라 선택
            // TODO: HP가 낮을 때 특수 행동 분기 (광폭화 등)

            MonsterAIAction action = DecideAction(context);

            switch (action)
            {
                case MonsterAIAction.BasicAttack:
                    PerformBasicAttack(context);
                    break;
                case MonsterAIAction.UseSkill:
                    // TODO: 스킬 선택 및 실행
                    PerformBasicAttack(context); // 임시: 스킬 대신 기본 공격
                    break;
                case MonsterAIAction.Defend:
                    // TODO: 방어 자세 (일시적 방어력 증가)
                    Debug.Log($"[Monster] {DisplayName} 방어 자세");
                    break;
                default:
                    PerformBasicAttack(context);
                    break;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // AI 결정 로직
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 현재 상황을 고려하여 다음 행동을 결정한다.
        /// </summary>
        private MonsterAIAction DecideAction(BattleContext context)
        {
            // 보스 몬스터는 더 복잡한 패턴을 사용 (확장 포인트)
            if (Data.monsterType == MonsterType.Boss)
            {
                // TODO: 보스 페이즈 패턴 구현
            }

            // 스킬이 있고 20% 확률로 스킬 사용
            if (skills.Count > 0 && context.RNG.RollPercent(20f))
                return MonsterAIAction.UseSkill;

            // HP가 30% 이하면 10% 확률로 방어
            float hpRatio = (float)CurrentHp / MaxHp;
            if (hpRatio < 0.3f && context.RNG.RollPercent(10f))
                return MonsterAIAction.Defend;

            return MonsterAIAction.BasicAttack;
        }

        /// <summary>
        /// 기본 공격을 수행한다.
        /// </summary>
        private void PerformBasicAttack(BattleContext context)
        {
            bool isCritical  = context.RNG.Roll(Stats.critChance);
            int  baseDamage  = context.RNG.RollDamage(Stats.FinalAttack);
            int  finalDamage = isCritical
                ? UnityEngine.Mathf.RoundToInt(baseDamage * Stats.critMultiplier)
                : baseDamage;

            Debug.Log($"[Monster] {DisplayName} 기본 공격: {finalDamage} (크리: {isCritical})");
            context.Opponent.TakeDamage(finalDamage, isCritical);
        }

        // ─────────────────────────────────────────────────────────────
        // 드롭 보상 계산
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 몬스터 처치 시 드롭 골드를 계산하여 반환한다.
        /// </summary>
        public int GetGoldDrop(Core.RNGSystem rng)
        {
            return Data.RollGoldReward(rng);
        }

        /// <summary>
        /// 드롭 테이블을 순회하며 드롭된 아이템 목록을 반환한다.
        /// </summary>
        public System.Collections.Generic.List<ItemData> RollDrops(Core.RNGSystem rng)
        {
            var drops = new System.Collections.Generic.List<ItemData>();

            foreach (DropEntry entry in Data.dropTable)
            {
                if (entry.item == null) continue;
                if (!rng.Roll(entry.dropChance)) continue;

                int qty = rng.NextInt(entry.quantityRange.x, entry.quantityRange.y);
                for (int i = 0; i < qty; i++)
                    drops.Add(entry.item);
            }

            return drops;
        }
    }

    /// <summary>몬스터 AI가 선택할 수 있는 행동 종류</summary>
    public enum MonsterAIAction
    {
        BasicAttack,
        UseSkill,
        Defend
    }
}

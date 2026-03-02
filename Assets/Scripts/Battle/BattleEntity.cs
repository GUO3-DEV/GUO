using System;
using System.Collections.Generic;
using UnityEngine;
using RoguelikeRPG.Data;

namespace RoguelikeRPG.Battle
{
    /// <summary>
    /// 전투에 참여하는 모든 엔티티(플레이어, 몬스터)의 공통 기반 클래스.
    /// 스탯, 상태이상, 스킬 목록을 관리하며 피해/회복의 핵심 로직을 담당한다.
    /// </summary>
    public abstract class BattleEntity
    {
        // ─────────────────────────────────────────────────────────────
        // 식별
        // ─────────────────────────────────────────────────────────────
        public string EntityId   { get; protected set; }
        public string DisplayName { get; protected set; }

        // ─────────────────────────────────────────────────────────────
        // 런타임 스탯
        // ─────────────────────────────────────────────────────────────
        public RuntimeStats Stats { get; protected set; }

        public int  CurrentHp  => Stats.currentHp;
        public int  MaxHp      => Stats.maxHp;
        public bool IsDead     => Stats.IsDead;

        // ─────────────────────────────────────────────────────────────
        // 상태이상 목록
        // ─────────────────────────────────────────────────────────────
        protected List<StatusEffect> activeEffects = new();
        public IReadOnlyList<StatusEffect> ActiveEffects => activeEffects;

        // ─────────────────────────────────────────────────────────────
        // 스킬 목록
        // ─────────────────────────────────────────────────────────────
        protected List<SkillBase> skills = new();
        public IReadOnlyList<SkillBase> Skills => skills;

        // ─────────────────────────────────────────────────────────────
        // 이벤트
        // ─────────────────────────────────────────────────────────────

        /// <summary>피해를 받았을 때 발행. (실제 피해량, 크리티컬 여부)</summary>
        public event Action<int, bool> OnDamageTaken;

        /// <summary>체력이 회복되었을 때 발행. (회복량)</summary>
        public event Action<int> OnHealReceived;

        /// <summary>사망했을 때 발행.</summary>
        public event Action OnDeath;

        // ─────────────────────────────────────────────────────────────
        // 피해 / 회복 처리
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 피해를 받는다. 방어력 감산 후 최소 1의 피해를 보장한다.
        /// </summary>
        /// <param name="rawDamage">방어력 감산 전 원본 피해량</param>
        /// <param name="isCritical">크리티컬 피해 여부</param>
        /// <param name="ignoreDefense">방어력 무시 여부 (관통 효과)</param>
        public virtual void TakeDamage(int rawDamage, bool isCritical = false, bool ignoreDefense = false)
        {
            if (IsDead) return;

            // 방어력 적용 (최소 1 피해 보장)
            int effectiveDefense = ignoreDefense ? 0 : Stats.FinalDefense;
            int actualDamage     = Mathf.Max(1, rawDamage - effectiveDefense);

            // 크리티컬 배율 적용은 호출 전에 이미 처리되어 있어야 한다
            var mutableStats = Stats;
            mutableStats.currentHp = Mathf.Max(0, mutableStats.currentHp - actualDamage);
            Stats = mutableStats;

            Debug.Log($"[BattleEntity] {DisplayName} 피해: {actualDamage} (원본: {rawDamage}, 방어: {effectiveDefense}, 크리: {isCritical})");
            OnDamageTaken?.Invoke(actualDamage, isCritical);

            if (IsDead)
            {
                Debug.Log($"[BattleEntity] {DisplayName} 사망");
                OnDeath?.Invoke();
            }
        }

        /// <summary>
        /// 체력을 회복한다. 최대 체력을 초과하지 않는다.
        /// </summary>
        /// <param name="amount">회복량</param>
        public virtual void Heal(int amount)
        {
            if (IsDead) return;

            int actual = Mathf.Min(amount, MaxHp - CurrentHp);
            var mutableStats = Stats;
            mutableStats.currentHp += actual;
            Stats = mutableStats;

            Debug.Log($"[BattleEntity] {DisplayName} 회복: {actual}");
            OnHealReceived?.Invoke(actual);
        }

        // ─────────────────────────────────────────────────────────────
        // 상태이상 관리
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 상태이상을 적용한다. 같은 타입이 이미 있으면 지속 시간을 갱신한다.
        /// </summary>
        public void ApplyEffect(StatusEffect effect)
        {
            // 같은 타입의 기존 효과 제거 후 재적용 (중첩 방지)
            activeEffects.RemoveAll(e => e.effectType == effect.effectType);
            activeEffects.Add(effect);

            Debug.Log($"[BattleEntity] {DisplayName}에 상태이상 적용: {effect.effectType} ({effect.remainingTurns}턴)");
            // TODO: 스탯 즉시 적용이 필요한 효과 처리 (예: 공격력 버프)
        }

        /// <summary>
        /// 턴 종료 시 모든 상태이상을 한 턴씩 줄이고, 만료된 것을 제거한다.
        /// </summary>
        public void TickEffects()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                activeEffects[i].remainingTurns--;

                // TODO: 지속 효과 발동 (독 피해, 재생 등)
                ProcessTickEffect(activeEffects[i]);

                if (activeEffects[i].remainingTurns <= 0)
                {
                    Debug.Log($"[BattleEntity] {DisplayName} 상태이상 만료: {activeEffects[i].effectType}");
                    // TODO: 스탯 버프/디버프 원복
                    activeEffects.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 상태이상 지속 효과를 처리한다. 서브클래스에서 재정의 가능.
        /// </summary>
        protected virtual void ProcessTickEffect(StatusEffect effect)
        {
            switch (effect.effectType)
            {
                case StatusEffectType.Poison:
                    // 독: 매 턴 최대 체력의 일정 비율 피해
                    TakeDamage(Mathf.Max(1, Mathf.RoundToInt(MaxHp * effect.value)), ignoreDefense: true);
                    break;
                case StatusEffectType.Regeneration:
                    // 재생: 매 턴 체력 회복
                    Heal(Mathf.Max(1, Mathf.RoundToInt(effect.value)));
                    break;
                default:
                    break;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 추상 메서드 (서브클래스 구현 필수)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 이 엔티티의 AI/플레이어 행동을 선택하고 실행한다.
        /// BattleManager의 턴 루프에서 호출된다.
        /// </summary>
        public abstract void TakeTurn(BattleContext context);
    }

    // ─────────────────────────────────────────────────────────────────
    // 관련 구조체 / 열거형
    // ─────────────────────────────────────────────────────────────────

    /// <summary>전투 중 엔티티에 부여되는 상태이상</summary>
    [Serializable]
    public class StatusEffect
    {
        public StatusEffectType effectType;

        [Tooltip("남은 지속 턴 수")]
        public int remainingTurns;

        [Tooltip("효과 수치 (독 피해율, 회복량 등)")]
        public float value;

        public StatusEffect(StatusEffectType type, int turns, float value)
        {
            this.effectType     = type;
            this.remainingTurns = turns;
            this.value          = value;
        }
    }

    /// <summary>상태이상 종류</summary>
    public enum StatusEffectType
    {
        None,
        Poison,        // 독: 매 턴 지속 피해
        Burn,          // 화상: 매 턴 지속 피해 (방어력 무시)
        Stun,          // 기절: 행동 불가
        Weakness,      // 약화: 공격력 감소
        Shield,        // 보호막: 피해 흡수
        Regeneration,  // 재생: 매 턴 체력 회복
        AttackUp,      // 공격 버프
        DefenseUp,     // 방어 버프
    }

    /// <summary>
    /// 턴 실행 시 BattleEntity에 전달되는 전투 컨텍스트.
    /// 현재 전투에 참여 중인 모든 엔티티 참조와 RNG를 포함한다.
    /// </summary>
    public class BattleContext
    {
        public BattleEntity Self      { get; }
        public BattleEntity Opponent  { get; }
        public Core.RNGSystem RNG     { get; }

        public BattleContext(BattleEntity self, BattleEntity opponent, Core.RNGSystem rng)
        {
            Self     = self;
            Opponent = opponent;
            RNG      = rng;
        }
    }
}

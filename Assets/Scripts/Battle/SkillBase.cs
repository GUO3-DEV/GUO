using UnityEngine;

namespace RoguelikeRPG.Battle
{
    /// <summary>
    /// 모든 스킬의 공통 기반 ScriptableObject.
    /// 스킬 데이터(이름, 설명, 쿨다운)와 실행 로직을 함께 보유한다.
    ///
    /// ===== 새 스킬 추가 방법 =====
    /// [방법 1] 기본 스킬 타입으로 추가 (코드 수정 없음)
    ///  1. Assets > Create > RoguelikeRPG/Battle/Skill
    ///  2. 파일명: Skill_강공격 (예시)
    ///  3. 인스펙터에서:
    ///     - skillId: "power_attack"
    ///     - displayName: "강공격"
    ///     - skillType: Attack
    ///     - power: 1.5 (공격력 1.5배)
    ///     - cooldownTurns: 2 (2턴 쿨다운)
    ///  4. 이 파일을 MonsterData나 Player의 스킬 목록에 등록
    ///
    /// [방법 2] 특수 스킬 로직 추가 (간단한 코드 수정)
    ///  1. Scripts/Battle/Skills/ 폴더 생성
    ///  2. 새 파일: Scripts/Battle/Skills/PowerAttackSkill.cs
    ///     public class PowerAttackSkill : SkillBase
    ///     {
    ///         protected override void ExecuteAttack(BattleEntity caster, BattleEntity target, BattleContext context)
    ///         {
    ///             // TODO: 특수 로직 작성
    ///             // 예: 낮은 HP일수록 더 강한 공격
    ///         }
    ///     }
    ///  3. [CreateAssetMenu] 데코레이터 추가하면 Unity 메뉴에 자동 등록
    /// </summary>
    [CreateAssetMenu(
        fileName = "Skill_New",
        menuName  = "RoguelikeRPG/Battle/Skill",
        order     = 20)]
    public class SkillBase : ScriptableObject
    {
        // ─────────────────────────────────────────────────────────────
        // 기본 정보
        // ─────────────────────────────────────────────────────────────
        [Header("기본 정보")]
        [Tooltip("스킬 고유 ID")]
        public string skillId;

        [Tooltip("화면에 표시되는 스킬 이름")]
        public string displayName;

        [TextArea(2, 5)]
        [Tooltip("스킬 설명 (전투 UI에 표시)")]
        public string description;

        // ─────────────────────────────────────────────────────────────
        // 스킬 속성
        // ─────────────────────────────────────────────────────────────
        [Header("스킬 속성")]
        [Tooltip("스킬 종류 (공격/방어/버프/디버프)")]
        public SkillType skillType = SkillType.Attack;

        [Tooltip("쿨다운 턴 수 (0이면 매 턴 사용 가능)")]
        public int cooldownTurns = 0;

        [Tooltip("스킬 위력 (공격력 배율 또는 고정 수치)")]
        public float power = 1.0f;

        [Tooltip("공격력 비례 배율 여부. true면 power를 배율로, false면 고정 값으로 사용")]
        public bool scaledByAttack = true;

        [Tooltip("적용할 상태이상 (선택)")]
        public StatusEffectType applyStatusEffect = StatusEffectType.None;

        [Tooltip("상태이상 지속 턴 수")]
        public int statusEffectDuration = 0;

        [Tooltip("상태이상 수치 (독 피해율 등)")]
        public float statusEffectValue = 0f;

        // ─────────────────────────────────────────────────────────────
        // 런타임 쿨다운 (씬 로드 시 초기화)
        // ─────────────────────────────────────────────────────────────

        /// <summary>현재 남은 쿨다운 턴 수. 0이면 사용 가능.</summary>
        [System.NonSerialized]
        public int currentCooldown = 0;

        public bool IsReady => currentCooldown <= 0;

        // ─────────────────────────────────────────────────────────────
        // 실행 API
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 스킬을 실행한다. 서브클래스에서 재정의하여 특수 효과를 구현한다.
        /// </summary>
        /// <param name="caster">스킬 사용자</param>
        /// <param name="target">대상 엔티티</param>
        /// <param name="context">전투 컨텍스트 (RNG 포함)</param>
        public virtual void Execute(BattleEntity caster, BattleEntity target, BattleContext context)
        {
            if (!IsReady)
            {
                Debug.LogWarning($"[SkillBase] {displayName} 쿨다운 중 ({currentCooldown}턴 남음)");
                return;
            }

            // 기본 공격 스킬 실행
            switch (skillType)
            {
                case SkillType.Attack:
                    ExecuteAttack(caster, target, context);
                    break;
                case SkillType.Heal:
                    ExecuteHeal(caster, context);
                    break;
                case SkillType.Buff:
                    ExecuteBuff(caster, context);
                    break;
                case SkillType.Debuff:
                    ExecuteDebuff(target, context);
                    break;
            }

            // 상태이상 적용
            if (applyStatusEffect != StatusEffectType.None && statusEffectDuration > 0)
            {
                BattleEntity effectTarget = (skillType == SkillType.Debuff) ? target : caster;
                effectTarget.ApplyEffect(new StatusEffect(applyStatusEffect, statusEffectDuration, statusEffectValue));
            }

            // 쿨다운 설정
            currentCooldown = cooldownTurns;

            Debug.Log($"[SkillBase] {caster.DisplayName}이(가) {displayName} 사용");
        }

        /// <summary>턴 종료 시 쿨다운을 1 감소한다.</summary>
        public void TickCooldown()
        {
            if (currentCooldown > 0)
                currentCooldown--;
        }

        // ─────────────────────────────────────────────────────────────
        // 내부 실행 로직 (서브클래스에서 재정의 가능)
        // ─────────────────────────────────────────────────────────────

        protected virtual void ExecuteAttack(BattleEntity caster, BattleEntity target, BattleContext context)
        {
            int baseDamage  = scaledByAttack
                ? Mathf.RoundToInt(caster.Stats.FinalAttack * power)
                : Mathf.RoundToInt(power);

            int finalDamage = context.RNG.RollDamage(baseDamage);
            bool isCrit     = context.RNG.Roll(caster.Stats.critChance);

            if (isCrit)
                finalDamage = Mathf.RoundToInt(finalDamage * caster.Stats.critMultiplier);

            target.TakeDamage(finalDamage, isCrit);
        }

        protected virtual void ExecuteHeal(BattleEntity caster, BattleContext context)
        {
            int amount = scaledByAttack
                ? Mathf.RoundToInt(caster.Stats.maxHp * power)
                : Mathf.RoundToInt(power);

            caster.Heal(amount);
        }

        protected virtual void ExecuteBuff(BattleEntity caster, BattleContext context)
        {
            // TODO: 스탯 버프 적용 로직
            Debug.Log($"[SkillBase] 버프 스킬 실행 (미구현): {displayName}");
        }

        protected virtual void ExecuteDebuff(BattleEntity target, BattleContext context)
        {
            // TODO: 스탯 디버프 적용 로직
            Debug.Log($"[SkillBase] 디버프 스킬 실행 (미구현): {displayName}");
        }
    }

    /// <summary>스킬 종류 열거형</summary>
    public enum SkillType
    {
        Attack, // 공격 (상대에게 피해)
        Heal,   // 회복 (자신 또는 아군 체력 회복)
        Buff,   // 버프 (자신 스탯 강화)
        Debuff  // 디버프 (상대 스탯 약화)
    }
}

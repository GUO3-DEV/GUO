using UnityEngine;
using RoguelikeRPG.Battle;

namespace RoguelikeRPG.Battle.Skills
{
    /// <summary>
    /// 강공격 스킬 - 기본 공격의 1.5배 위력
    ///
    /// 복사해서 새 스킬 만드는 방법:
    /// 1. 이 파일을 Skill_YourSkillName.cs로 복사
    /// 2. 클래스명을 YourSkillNameSkill로 변경
    /// 3. ExecuteAttack()의 계산식을 수정
    /// 4. [CreateAssetMenu]의 fileName과 menuName 수정
    /// 5. Assets > Create > RoguelikeRPG/Battle/Skill_YourSkillName 으로 생성
    /// </summary>
    [CreateAssetMenu(
        fileName = "Skill_PowerAttack",
        menuName = "RoguelikeRPG/Battle/Skill/PowerAttack",
        order = 21)]
    public class PowerAttackSkill : SkillBase
    {
        [Header("강공격 설정")]
        [Tooltip("기본 공격력에 곱할 배율")]
        public float attackMultiplier = 1.5f;

        /// <summary>
        /// 강공격 실행 로직을 재정의한다.
        /// </summary>
        protected override void ExecuteAttack(BattleEntity caster, BattleEntity target, BattleContext context)
        {
            // TODO: 강공격 로직
            // 예시:
            // - 공격력의 1.5배 데미지
            // - 30% 확률로 상대방 행동 방해
            // - HP가 낮을수록 더 강한 공격 (보스 패턴 추가 가능)

            int baseDamage = Mathf.RoundToInt(caster.Stats.FinalAttack * attackMultiplier);
            int finalDamage = context.RNG.RollDamage(baseDamage);
            bool isCrit = context.RNG.Roll(caster.Stats.critChance);

            if (isCrit)
                finalDamage = Mathf.RoundToInt(finalDamage * caster.Stats.critMultiplier);

            target.TakeDamage(finalDamage, isCrit);

            Debug.Log($"[PowerAttack] {caster.DisplayName}이(가) {target.DisplayName}에게 {finalDamage} 피해!");
        }
    }
}

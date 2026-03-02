using UnityEngine;
using RoguelikeRPG.Battle;

namespace RoguelikeRPG.Battle.Skills
{
    /// <summary>
    /// 회복 스킬 - 자신의 체력을 회복
    ///
    /// 복사해서 새 스킬 만드는 방법:
    /// 1. 이 파일을 Skill_YourSkillName.cs로 복사
    /// 2. 클래스명을 YourSkillNameSkill로 변경
    /// 3. ExecuteHeal()의 계산식을 수정
    /// 4. [CreateAssetMenu]의 fileName과 menuName 수정
    /// 5. Assets > Create > RoguelikeRPG/Battle/Skill_YourSkillName 으로 생성
    /// </summary>
    [CreateAssetMenu(
        fileName = "Skill_Heal",
        menuName = "RoguelikeRPG/Battle/Skill/Heal",
        order = 22)]
    public class HealSkill : SkillBase
    {
        [Header("회복 설정")]
        [Tooltip("최대 체력에 대한 회복 비율 (0.3 = 30%)")]
        [Range(0f, 1f)]
        public float healPercent = 0.3f;

        /// <summary>
        /// 회복 스킬 실행 로직을 재정의한다.
        /// </summary>
        protected override void ExecuteHeal(BattleEntity caster, BattleContext context)
        {
            // TODO: 회복 로직
            // 예시:
            // - 최대 체력의 30% 회복
            // - 체력이 50% 이하일 때 1.5배 회복량
            // - 쿨다운 2턴
            // - 확률 기반 추가 회복 (럭키 회복 20% 확률)

            int healAmount = Mathf.RoundToInt(caster.Stats.maxHp * healPercent);

            // 낮은 체력에서 회복량 증가
            if (caster.CurrentHp < caster.Stats.maxHp * 0.5f)
            {
                healAmount = Mathf.RoundToInt(healAmount * 1.5f);
            }

            caster.Heal(healAmount);

            Debug.Log($"[Heal] {caster.DisplayName}이(가) {healAmount} 회복!");
        }
    }
}

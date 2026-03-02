using UnityEngine;
using RoguelikeRPG.Battle;

namespace RoguelikeRPG.Battle.Skills
{
    /// <summary>
    /// 방어 버프 스킬 - 자신의 방어력을 일시적으로 증가
    ///
    /// 복사해서 새 스킬 만드는 방법:
    /// 1. 이 파일을 Skill_YourSkillName.cs로 복사
    /// 2. 클래스명을 YourSkillNameSkill로 변경
    /// 3. ExecuteBuff()의 계산식을 수정
    /// 4. [CreateAssetMenu]의 fileName과 menuName 수정
    /// 5. Assets > Create > RoguelikeRPG/Battle/Skill_YourSkillName 으로 생성
    /// </summary>
    [CreateAssetMenu(
        fileName = "Skill_DefenseBuff",
        menuName = "RoguelikeRPG/Battle/Skill/DefenseBuff",
        order = 23)]
    public class DefenseBuffSkill : SkillBase
    {
        [Header("방어 버프 설정")]
        [Tooltip("방어력 증가량")]
        public int defenseBonus = 5;

        [Tooltip("버프 지속 턴 수")]
        public int buffDuration = 3;

        /// <summary>
        /// 방어 버프 실행 로직을 재정의한다.
        /// </summary>
        protected override void ExecuteBuff(BattleEntity caster, BattleContext context)
        {
            // TODO: 버프 로직
            // 예시:
            // - 방어력 +5 (3턴 지속)
            // - 공격 적중률 -20% (적 공격 회피)
            // - 강화된 버프: HP 50% 이상 시 방어력 +8
            // - 반사 효과: 받은 피해의 20% 반대로 적에게

            // 현재는 상태이상 시스템을 통해 구현
            caster.ApplyEffect(
                new StatusEffect(
                    StatusEffectType.DefenseUp,
                    buffDuration,
                    defenseBonus));

            Debug.Log($"[DefenseBuff] {caster.DisplayName}의 방어력이 {defenseBonus} 증가! ({buffDuration}턴 지속)");
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using RoguelikeRPG.Core;
using RoguelikeRPG.Battle;
using RoguelikeRPG.Data;

namespace RoguelikeRPG.Event
{
    /// <summary>
    /// 이벤트 선택지의 조건 평가와 결과 실행을 담당하는 서비스 클래스.
    /// EventManager가 이 클래스에 실행 위임을 하며,
    /// Player에 직접 접근하여 스탯/인벤토리/골드를 변경한다.
    /// </summary>
    public class ChoiceHandler
    {
        // ─────────────────────────────────────────────────────────────
        // 의존성
        // ─────────────────────────────────────────────────────────────
        private readonly Player    _player;
        private readonly RNGSystem _rng;

        public ChoiceHandler(Player player, RNGSystem rng)
        {
            _player = player;
            _rng    = rng;
        }

        // ─────────────────────────────────────────────────────────────
        // 조건 평가
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 선택지의 모든 조건을 평가하여 선택 가능 여부를 반환한다.
        /// 조건 목록이 비어 있으면 항상 true를 반환한다.
        /// </summary>
        public bool EvaluateConditions(EventChoice choice)
        {
            foreach (ChoiceCondition condition in choice.conditions)
            {
                if (!EvaluateSingleCondition(condition))
                    return false;
            }

            return true;
        }

        private bool EvaluateSingleCondition(ChoiceCondition condition)
        {
            switch (condition.conditionType)
            {
                case ConditionType.None:
                    return true;

                case ConditionType.MinHpPercent:
                {
                    float hpPercent = (float)_player.CurrentHp / _player.MaxHp * 100f;
                    return hpPercent >= condition.value;
                }

                case ConditionType.MaxHpPercent:
                {
                    float hpPercent = (float)_player.CurrentHp / _player.MaxHp * 100f;
                    return hpPercent <= condition.value;
                }

                case ConditionType.MinGold:
                    return _player.Gold >= condition.value;

                case ConditionType.HasItem:
                {
                    // TODO: 인벤토리에서 itemId에 해당하는 아이템 존재 여부 확인
                    foreach (var item in _player.Inventory)
                    {
                        if (item.itemId == condition.itemId)
                            return true;
                    }
                    return false;
                }

                default:
                    Debug.LogWarning($"[ChoiceHandler] 알 수 없는 조건 타입: {condition.conditionType}");
                    return true;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 결과 실행
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 선택지의 결과 목록을 순서대로 실행한다.
        /// 확률 결과는 RNG로 판정한다.
        /// </summary>
        /// <returns>전투 발생이 필요한 경우 해당 MonsterData, 없으면 null</returns>
        public MonsterData ExecuteOutcomes(EventChoice choice)
        {
            MonsterData battleTrigger = null;

            foreach (ChoiceOutcome outcome in choice.outcomes)
            {
                // 확률 판정
                if (!_rng.Roll(outcome.probability)) continue;

                MonsterData result = ExecuteSingleOutcome(outcome);
                if (result != null)
                    battleTrigger = result;
            }

            return battleTrigger;
        }

        private MonsterData ExecuteSingleOutcome(ChoiceOutcome outcome)
        {
            switch (outcome.outcomeType)
            {
                case OutcomeType.HealHp:
                    _player.Heal(Mathf.RoundToInt(outcome.value));
                    LogOutcome($"HP {outcome.value} 회복");
                    break;

                case OutcomeType.HealHpPercent:
                {
                    int amount = Mathf.RoundToInt(_player.MaxHp * outcome.value / 100f);
                    _player.Heal(amount);
                    LogOutcome($"HP {outcome.value}% 회복 ({amount})");
                    break;
                }

                case OutcomeType.LoseHp:
                    // 전투 외 피해는 방어력 무시로 처리
                    _player.TakeDamage(Mathf.RoundToInt(outcome.value), ignoreDefense: true);
                    LogOutcome($"HP {outcome.value} 손실");
                    break;

                case OutcomeType.GainGold:
                    _player.AddGold(Mathf.RoundToInt(outcome.value));
                    LogOutcome($"골드 {outcome.value} 획득");
                    break;

                case OutcomeType.LoseGold:
                    _player.SpendGold(Mathf.RoundToInt(outcome.value));
                    LogOutcome($"골드 {outcome.value} 소모");
                    break;

                case OutcomeType.GainItem:
                    if (outcome.relatedItem != null)
                    {
                        _player.AddItem(outcome.relatedItem);
                        LogOutcome($"아이템 획득: {outcome.relatedItem.displayName}");
                    }
                    break;

                case OutcomeType.LoseItem:
                    // TODO: 인벤토리에서 relatedItem 제거
                    LogOutcome($"아이템 소모: {outcome.relatedItem?.displayName}");
                    break;

                case OutcomeType.TriggerBattle:
                    LogOutcome("전투 발생!");
                    // EventManager가 이 반환값을 받아 BattleManager를 시작함
                    return null; // TODO: 이벤트 데이터에서 linkedMonster를 전달받는 방법 구현

                case OutcomeType.Nothing:
                    LogOutcome("아무 일도 일어나지 않았다.");
                    break;

                default:
                    Debug.LogWarning($"[ChoiceHandler] 알 수 없는 결과 타입: {outcome.outcomeType}");
                    break;
            }

            return null;
        }

        // ─────────────────────────────────────────────────────────────
        // 헬퍼
        // ─────────────────────────────────────────────────────────────

        private void LogOutcome(string message)
        {
            Debug.Log($"[ChoiceHandler] 결과: {message}");
        }
    }
}

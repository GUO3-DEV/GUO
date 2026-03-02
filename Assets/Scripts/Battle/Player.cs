using System.Collections.Generic;
using UnityEngine;
using RoguelikeRPG.Data;

namespace RoguelikeRPG.Battle
{
    /// <summary>
    /// 플레이어 전투 엔티티.
    /// PlayerData를 기반으로 런타임 상태를 관리하며,
    /// 플레이어 입력(UI에서 받은 선택)에 따라 행동을 실행한다.
    /// </summary>
    public class Player : BattleEntity
    {
        // ─────────────────────────────────────────────────────────────
        // 플레이어 전용 상태
        // ─────────────────────────────────────────────────────────────

        /// <summary>현재 보유 골드 (던전 전체 런에서 공유)</summary>
        public int Gold { get; private set; }

        /// <summary>현재 인벤토리 아이템 목록</summary>
        private List<ItemData> inventory = new();
        public IReadOnlyList<ItemData> Inventory => inventory;

        // 턴 행동 큐: BattleManager가 입력을 큐에 넣고 TakeTurn에서 소비
        private System.Action<BattleContext> _pendingAction;

        // ─────────────────────────────────────────────────────────────
        // 생성자
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// PlayerData로부터 플레이어 런타임 인스턴스를 생성한다.
        /// </summary>
        public Player(PlayerData data)
        {
            EntityId    = "player";
            DisplayName = data.playerName;
            Stats       = RuntimeStats.FromPlayerData(data);
            Gold        = data.startingGold;

            // TODO: data.startingSkillIds로 스킬 인스턴스 로드 및 추가
            Debug.Log($"[Player] 생성 완료 - {DisplayName} (HP: {MaxHp})");
        }

        // ─────────────────────────────────────────────────────────────
        // 턴 행동 (BattleEntity 추상 메서드 구현)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// BattleManager 턴 루프에서 호출된다.
        /// 플레이어는 UI를 통해 미리 큐잉된 행동을 실행한다.
        /// </summary>
        public override void TakeTurn(BattleContext context)
        {
            if (_pendingAction != null)
            {
                _pendingAction.Invoke(context);
                _pendingAction = null;
            }
            else
            {
                // 행동이 없으면 방어(기본 행동)
                Debug.LogWarning("[Player] 큐에 행동이 없어 기본 공격을 수행합니다.");
                PerformBasicAttack(context);
            }
        }

        /// <summary>
        /// UI로부터 플레이어의 다음 행동을 큐에 등록한다.
        /// BattleUI가 버튼 클릭 시 이 메서드를 호출한다.
        /// </summary>
        public void QueueAction(System.Action<BattleContext> action)
        {
            _pendingAction = action;
        }

        // ─────────────────────────────────────────────────────────────
        // 전투 행동
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 기본 공격을 수행한다. RNG를 통해 크리티컬 여부를 판정한다.
        /// </summary>
        public void PerformBasicAttack(BattleContext context)
        {
            bool isCritical = context.RNG.Roll(Stats.critChance);
            int  baseDamage = context.RNG.RollDamage(Stats.FinalAttack);
            int  finalDamage = isCritical
                ? Mathf.RoundToInt(baseDamage * Stats.critMultiplier)
                : baseDamage;

            Debug.Log($"[Player] 기본 공격: {finalDamage} (크리: {isCritical})");
            context.Opponent.TakeDamage(finalDamage, isCritical);
        }

        /// <summary>
        /// 지정한 스킬을 사용한다.
        /// </summary>
        public void UseSkill(SkillBase skill, BattleContext context)
        {
            if (skill == null) return;

            // TODO: 스킬 사용 가능 조건 확인 (쿨다운, 마나 등)
            skill.Execute(this, context.Opponent, context);
        }

        // ─────────────────────────────────────────────────────────────
        // 인벤토리
        // ─────────────────────────────────────────────────────────────

        /// <summary>아이템을 인벤토리에 추가한다.</summary>
        public void AddItem(ItemData item)
        {
            if (item == null) return;
            inventory.Add(item);
            Debug.Log($"[Player] 아이템 획득: {item.displayName}");
        }

        /// <summary>
        /// 아이템을 사용한다. 소비형 아이템이면 인벤토리에서 제거한다.
        /// </summary>
        /// <param name="item">사용할 아이템</param>
        /// <param name="context">현재 전투 컨텍스트 (null이면 전투 외)</param>
        public bool UseItem(ItemData item, BattleContext context = null)
        {
            if (!inventory.Contains(item))
            {
                Debug.LogWarning($"[Player] 인벤토리에 없는 아이템: {item?.displayName}");
                return false;
            }

            // 사용 가능 여부 체크
            bool inBattle = context != null;
            if (inBattle  && !item.usableInBattle)     return false;
            if (!inBattle && !item.usableOutOfBattle)  return false;

            // TODO: item.effects를 순회하며 각 ItemEffectType별 로직 실행
            Debug.Log($"[Player] 아이템 사용: {item.displayName}");

            if (item.isConsumable)
                inventory.Remove(item);

            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // 골드 관리
        // ─────────────────────────────────────────────────────────────

        public void AddGold(int amount)
        {
            Gold += amount;
            Debug.Log($"[Player] 골드 획득: +{amount} (합계: {Gold})");
        }

        public bool SpendGold(int amount)
        {
            if (Gold < amount)
            {
                Debug.Log($"[Player] 골드 부족: {Gold} / {amount}");
                return false;
            }

            Gold -= amount;
            return true;
        }
    }
}

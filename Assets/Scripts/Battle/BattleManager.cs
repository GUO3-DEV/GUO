using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoguelikeRPG.Core;
using RoguelikeRPG.Data;

namespace RoguelikeRPG.Battle
{
    /// <summary>
    /// 턴제 전투 전체를 진행하는 매니저.
    /// 전투 시작/종료, 턴 순서 결정, 전투 루프를 담당하며
    /// BattleUI에 이벤트를 발행한다.
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────
        // 이벤트 (BattleUI가 구독)
        // ─────────────────────────────────────────────────────────────

        /// <summary>전투가 시작될 때 발행. (플레이어, 몬스터)</summary>
        public event Action<Player, Monster> OnBattleStarted;

        /// <summary>새 턴이 시작될 때 발행. (현재 턴 번호, 행동할 엔티티)</summary>
        public event Action<int, BattleEntity> OnTurnStarted;

        /// <summary>턴이 종료될 때 발행.</summary>
        public event Action<int> OnTurnEnded;

        /// <summary>전투가 종료될 때 발행. (승리 여부)</summary>
        public event Action<bool> OnBattleEnded;

        /// <summary>전투 로그 텍스트 발행 (UI 텍스트 피드 업데이트용)</summary>
        public event Action<string> OnBattleLog;

        // ─────────────────────────────────────────────────────────────
        // 전투 상태
        // ─────────────────────────────────────────────────────────────
        public bool IsInBattle   { get; private set; }
        public int  CurrentTurn  { get; private set; }

        private Player  _player;
        private Monster _monster;
        private List<BattleEntity> _turnOrder = new();

        // 플레이어 행동 입력을 기다리는 동안 코루틴을 일시 정지시키기 위한 플래그
        private bool _waitingForPlayerInput;

        // ─────────────────────────────────────────────────────────────
        // 의존성
        // ─────────────────────────────────────────────────────────────
        private RNGSystem _rng;

        // ─────────────────────────────────────────────────────────────
        // Unity 라이프사이클
        // ─────────────────────────────────────────────────────────────
        private void Awake()
        {
            // GameManager에서 RNGSystem을 받아온다 (GameManager가 씬에 존재한다고 가정)
            _rng = GameManager.Instance?.RNG ?? new RNGSystem();
        }

        // ─────────────────────────────────────────────────────────────
        // 전투 시작 API
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 전투를 시작한다. GameManager 또는 EventManager가 호출한다.
        /// </summary>
        /// <param name="player">플레이어 런타임 인스턴스</param>
        /// <param name="monsterData">조우한 몬스터 데이터</param>
        public void StartBattle(Player player, MonsterData monsterData)
        {
            if (IsInBattle)
            {
                Debug.LogWarning("[BattleManager] 이미 전투 중입니다.");
                return;
            }

            _player  = player;
            _monster = new Monster(monsterData);

            // 사망 이벤트 구독
            _player.OnDeath  += OnPlayerDied;
            _monster.OnDeath += OnMonsterDied;

            IsInBattle   = true;
            CurrentTurn  = 0;

            // 스피드 기반 선공 결정
            DetermineFirstTurn();

            Log($"전투 시작! {_player.DisplayName} vs {_monster.DisplayName}");
            OnBattleStarted?.Invoke(_player, _monster);

            StartCoroutine(BattleLoop());
        }

        // ─────────────────────────────────────────────────────────────
        // 플레이어 입력 수신 (BattleUI에서 호출)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 플레이어가 기본 공격 버튼을 눌렀을 때 BattleUI가 호출한다.
        /// </summary>
        public void OnPlayerChooseBasicAttack()
        {
            if (!_waitingForPlayerInput) return;

            _player.QueueAction(ctx => _player.PerformBasicAttack(ctx));
            _waitingForPlayerInput = false;
        }

        /// <summary>
        /// 플레이어가 스킬을 선택했을 때 BattleUI가 호출한다.
        /// </summary>
        public void OnPlayerChooseSkill(SkillBase skill)
        {
            if (!_waitingForPlayerInput) return;

            _player.QueueAction(ctx => _player.UseSkill(skill, ctx));
            _waitingForPlayerInput = false;
        }

        /// <summary>
        /// 플레이어가 아이템을 사용했을 때 BattleUI가 호출한다.
        /// </summary>
        public void OnPlayerUseItem(ItemData item)
        {
            if (!_waitingForPlayerInput) return;

            _player.QueueAction(ctx => _player.UseItem(item, ctx));
            _waitingForPlayerInput = false;
        }

        // ─────────────────────────────────────────────────────────────
        // 전투 루프 (코루틴)
        // ─────────────────────────────────────────────────────────────

        private IEnumerator BattleLoop()
        {
            while (IsInBattle)
            {
                CurrentTurn++;

                foreach (BattleEntity entity in _turnOrder)
                {
                    if (!IsInBattle) yield break;
                    if (entity.IsDead)    continue;

                    OnTurnStarted?.Invoke(CurrentTurn, entity);
                    Log($"--- {entity.DisplayName}의 턴 (턴 {CurrentTurn}) ---");

                    // 상태이상 처리 (턴 시작 시)
                    entity.TickEffects();
                    if (entity.IsDead) continue;

                    BattleContext ctx = BuildContext(entity);

                    if (entity is Player)
                    {
                        // 플레이어 입력 대기
                        _waitingForPlayerInput = true;
                        yield return new WaitUntil(() => !_waitingForPlayerInput);
                    }

                    // 행동 실행
                    entity.TakeTurn(ctx);

                    // 스킬 쿨다운 감소
                    foreach (SkillBase skill in entity.Skills)
                        skill.TickCooldown();

                    OnTurnEnded?.Invoke(CurrentTurn);

                    // 연출을 위한 짧은 딜레이
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 전투 종료 처리
        // ─────────────────────────────────────────────────────────────

        private void OnPlayerDied()
        {
            EndBattle(playerWon: false);
        }

        private void OnMonsterDied()
        {
            // 보상 지급
            DistributeRewards();
            EndBattle(playerWon: true);
        }

        private void EndBattle(bool playerWon)
        {
            if (!IsInBattle) return;

            IsInBattle = false;
            StopAllCoroutines();

            // 이벤트 구독 해제
            if (_player  != null) _player.OnDeath  -= OnPlayerDied;
            if (_monster != null) _monster.OnDeath -= OnMonsterDied;

            string resultMsg = playerWon
                ? $"{_monster.DisplayName}을(를) 처치했습니다!"
                : $"{_player.DisplayName}이(가) 쓰러졌습니다...";

            Log(resultMsg);
            OnBattleEnded?.Invoke(playerWon);

            if (!playerWon)
                GameManager.Instance?.OnPlayerDeath();
        }

        // ─────────────────────────────────────────────────────────────
        // 내부 헬퍼
        // ─────────────────────────────────────────────────────────────

        /// <summary>스피드 스탯을 기반으로 턴 순서를 결정한다.</summary>
        private void DetermineFirstTurn()
        {
            _turnOrder.Clear();

            bool playerFirst = _player.Stats.speed >= _monster.Stats.speed;

            // 스피드가 같으면 RNG로 결정
            if (_player.Stats.speed == _monster.Stats.speed)
                playerFirst = _rng.Roll(0.5f);

            if (playerFirst)
            {
                _turnOrder.Add(_player);
                _turnOrder.Add(_monster);
            }
            else
            {
                _turnOrder.Add(_monster);
                _turnOrder.Add(_player);
            }

            Log($"선공: {_turnOrder[0].DisplayName}");
        }

        /// <summary>현재 엔티티를 기준으로 BattleContext를 생성한다.</summary>
        private BattleContext BuildContext(BattleEntity self)
        {
            BattleEntity opponent = (self is Player) ? (BattleEntity)_monster : _player;
            return new BattleContext(self, opponent, _rng);
        }

        /// <summary>몬스터 처치 보상을 플레이어에게 지급한다.</summary>
        private void DistributeRewards()
        {
            int gold  = _monster.GetGoldDrop(_rng);
            var items = _monster.RollDrops(_rng);

            _player.AddGold(gold);
            Log($"골드 획득: {gold}");

            foreach (var item in items)
            {
                _player.AddItem(item);
                Log($"아이템 획득: {item.displayName}");
            }

            // TODO: 경험치 적용 (레벨업 시스템 구현 후)
        }

        /// <summary>전투 로그를 출력하고 UI 이벤트를 발행한다.</summary>
        private void Log(string message)
        {
            Debug.Log($"[BattleManager] {message}");
            OnBattleLog?.Invoke(message);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeRPG.Battle;
using RoguelikeRPG.Data;

namespace RoguelikeRPG.UI
{
    /// <summary>
    /// 전투 씬의 UI를 담당하는 Presenter.
    /// BattleManager의 이벤트를 구독하여 화면을 갱신하고,
    /// 플레이어 입력(버튼 클릭)을 BattleManager에 전달한다.
    ///
    /// 주의: UI 로직만 포함. 전투 판정 로직은 BattleManager에 위임한다.
    /// </summary>
    public class BattleUI : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────
        // 인스펙터 참조 - 플레이어 HUD
        // ─────────────────────────────────────────────────────────────
        [Header("플레이어 HUD")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private Slider          playerHpSlider;
        [SerializeField] private TextMeshProUGUI playerHpText;

        // ─────────────────────────────────────────────────────────────
        // 인스펙터 참조 - 몬스터 HUD
        // ─────────────────────────────────────────────────────────────
        [Header("몬스터 HUD")]
        [SerializeField] private TextMeshProUGUI monsterNameText;
        [SerializeField] private Slider          monsterHpSlider;
        [SerializeField] private TextMeshProUGUI monsterHpText;

        // ─────────────────────────────────────────────────────────────
        // 인스펙터 참조 - 전투 로그
        // ─────────────────────────────────────────────────────────────
        [Header("전투 로그")]
        [SerializeField] private TextMeshProUGUI battleLogText;
        [SerializeField] private ScrollRect      logScrollRect;

        [Tooltip("전투 로그에 최대로 유지할 줄 수 (초과 시 오래된 줄 제거)")]
        [SerializeField] private int maxLogLines = 20;

        // ─────────────────────────────────────────────────────────────
        // 인스펙터 참조 - 행동 패널
        // ─────────────────────────────────────────────────────────────
        [Header("행동 버튼 패널")]
        [SerializeField] private GameObject actionPanel;
        [SerializeField] private Button     basicAttackButton;

        [Tooltip("스킬 버튼 프리팹 (동적 생성)")]
        [SerializeField] private Button     skillButtonPrefab;
        [SerializeField] private Transform  skillButtonContainer;

        [Tooltip("아이템 버튼 프리팹 (동적 생성)")]
        [SerializeField] private Button     itemButtonPrefab;
        [SerializeField] private Transform  itemButtonContainer;

        // ─────────────────────────────────────────────────────────────
        // 인스펙터 참조 - 전투 결과 패널
        // ─────────────────────────────────────────────────────────────
        [Header("전투 결과 패널")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultTitleText;
        [SerializeField] private TextMeshProUGUI resultDetailText;
        [SerializeField] private Button     resultConfirmButton;

        // ─────────────────────────────────────────────────────────────
        // 런타임 참조
        // ─────────────────────────────────────────────────────────────
        private BattleManager _battleManager;
        private Player        _player;
        private Monster       _monster;
        private List<string>  _logLines = new();

        // ─────────────────────────────────────────────────────────────
        // Unity 라이프사이클
        // ─────────────────────────────────────────────────────────────
        private void Awake()
        {
            _battleManager = FindFirstObjectByType<BattleManager>();

            if (_battleManager == null)
            {
                Debug.LogError("[BattleUI] BattleManager를 찾을 수 없습니다.");
                return;
            }

            SubscribeBattleEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeBattleEvents();
        }

        // ─────────────────────────────────────────────────────────────
        // 이벤트 구독 / 해제
        // ─────────────────────────────────────────────────────────────
        private void SubscribeBattleEvents()
        {
            _battleManager.OnBattleStarted  += HandleBattleStarted;
            _battleManager.OnTurnStarted    += HandleTurnStarted;
            _battleManager.OnTurnEnded      += HandleTurnEnded;
            _battleManager.OnBattleEnded    += HandleBattleEnded;
            _battleManager.OnBattleLog      += AppendLog;
        }

        private void UnsubscribeBattleEvents()
        {
            if (_battleManager == null) return;

            _battleManager.OnBattleStarted  -= HandleBattleStarted;
            _battleManager.OnTurnStarted    -= HandleTurnStarted;
            _battleManager.OnTurnEnded      -= HandleTurnEnded;
            _battleManager.OnBattleEnded    -= HandleBattleEnded;
            _battleManager.OnBattleLog      -= AppendLog;
        }

        // ─────────────────────────────────────────────────────────────
        // 이벤트 핸들러
        // ─────────────────────────────────────────────────────────────
        private void HandleBattleStarted(Player player, Monster monster)
        {
            _player  = player;
            _monster = monster;

            // 초기 HUD 구성
            RefreshPlayerHUD();
            RefreshMonsterHUD();

            // 스킬/아이템 버튼 동적 생성
            BuildSkillButtons();
            BuildItemButtons();

            // 버튼 클릭 리스너 등록
            basicAttackButton.onClick.AddListener(OnClickBasicAttack);

            // 결과 패널 숨김
            resultPanel.SetActive(false);

            // 행동 패널 비활성화 (플레이어 턴이 아닐 때)
            SetActionPanelActive(false);

            // 체력 변화 구독 (개별 엔티티 이벤트)
            _player.OnDamageTaken  += (_, _) => RefreshPlayerHUD();
            _player.OnHealReceived += _       => RefreshPlayerHUD();
            _monster.OnDamageTaken += (_, _) => RefreshMonsterHUD();
        }

        private void HandleTurnStarted(int turnNumber, BattleEntity entity)
        {
            // 플레이어 턴에만 행동 패널 활성화
            bool isPlayerTurn = entity is Player;
            SetActionPanelActive(isPlayerTurn);
        }

        private void HandleTurnEnded(int turnNumber)
        {
            SetActionPanelActive(false);
        }

        private void HandleBattleEnded(bool playerWon)
        {
            SetActionPanelActive(false);
            ShowResultPanel(playerWon);
        }

        // ─────────────────────────────────────────────────────────────
        // 버튼 클릭 핸들러 (플레이어 입력 → BattleManager 전달)
        // ─────────────────────────────────────────────────────────────

        private void OnClickBasicAttack()
        {
            _battleManager.OnPlayerChooseBasicAttack();
        }

        // TODO: 스킬 버튼 클릭 시 _battleManager.OnPlayerChooseSkill(skill) 호출
        // TODO: 아이템 버튼 클릭 시 _battleManager.OnPlayerUseItem(item) 호출

        // ─────────────────────────────────────────────────────────────
        // HUD 갱신
        // ─────────────────────────────────────────────────────────────

        private void RefreshPlayerHUD()
        {
            if (_player == null) return;

            playerNameText.text = _player.DisplayName;
            playerHpText.text   = $"{_player.CurrentHp} / {_player.MaxHp}";
            playerHpSlider.value = (float)_player.CurrentHp / _player.MaxHp;
        }

        private void RefreshMonsterHUD()
        {
            if (_monster == null) return;

            monsterNameText.text  = _monster.DisplayName;
            monsterHpText.text    = $"{_monster.CurrentHp} / {_monster.MaxHp}";
            monsterHpSlider.value = (float)_monster.CurrentHp / _monster.MaxHp;
        }

        // ─────────────────────────────────────────────────────────────
        // 전투 로그
        // ─────────────────────────────────────────────────────────────

        /// <summary>전투 로그에 새 줄을 추가한다. 최대 줄 수 초과 시 오래된 줄을 제거한다.</summary>
        private void AppendLog(string message)
        {
            _logLines.Add(message);

            if (_logLines.Count > maxLogLines)
                _logLines.RemoveAt(0);

            battleLogText.text = string.Join("\n", _logLines);

            // 스크롤을 항상 최하단으로 이동
            StartCoroutine(ScrollToBottom());
        }

        private IEnumerator ScrollToBottom()
        {
            yield return null; // 한 프레임 후 레이아웃이 갱신된 다음 스크롤
            logScrollRect.normalizedPosition = Vector2.zero;
        }

        // ─────────────────────────────────────────────────────────────
        // 동적 버튼 생성
        // ─────────────────────────────────────────────────────────────

        private void BuildSkillButtons()
        {
            // 기존 버튼 제거
            foreach (Transform child in skillButtonContainer)
                Destroy(child.gameObject);

            // TODO: _player.Skills를 순회하며 스킬 버튼 프리팹 인스턴스화
            // 각 버튼에 onClick으로 _battleManager.OnPlayerChooseSkill(skill) 연결
        }

        private void BuildItemButtons()
        {
            foreach (Transform child in itemButtonContainer)
                Destroy(child.gameObject);

            // TODO: _player.Inventory를 순회하며 아이템 버튼 프리팹 인스턴스화
        }

        // ─────────────────────────────────────────────────────────────
        // 결과 패널
        // ─────────────────────────────────────────────────────────────

        private void ShowResultPanel(bool playerWon)
        {
            resultPanel.SetActive(true);
            resultTitleText.text = playerWon ? "승리!" : "패배...";

            if (playerWon)
            {
                // TODO: 획득 보상 상세 표시
                resultDetailText.text = "전투에서 승리했습니다!";
            }
            else
            {
                resultDetailText.text = "다음에 다시 도전하세요.";
            }

            resultConfirmButton.onClick.RemoveAllListeners();
            resultConfirmButton.onClick.AddListener(() =>
            {
                // TODO: 씬 전환 (던전 탐색 씬 또는 게임오버 씬)
                Core.GameManager.Instance?.ReturnToTown();
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 헬퍼
        // ─────────────────────────────────────────────────────────────

        private void SetActionPanelActive(bool active)
        {
            if (actionPanel != null)
                actionPanel.SetActive(active);
        }
    }
}

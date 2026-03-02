using System;
using UnityEngine;
using RoguelikeRPG.Data;
using RoguelikeRPG.Battle;

namespace RoguelikeRPG.Core
{
    /// <summary>
    /// 게임 전체 상태를 관리하는 싱글톤 매니저.
    /// 씬 전환 시에도 파괴되지 않으며, 플레이어 상태와 게임 흐름을 총괄한다.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────
        // 싱글톤
        // ─────────────────────────────────────────────────────────────
        public static GameManager Instance { get; private set; }

        // ─────────────────────────────────────────────────────────────
        // 게임 상태 열거형
        // ─────────────────────────────────────────────────────────────
        public enum GameState
        {
            None,       // 초기 상태
            Town,       // 마을 화면
            Dungeon,    // 던전 탐색 중
            Battle,     // 전투 중
            Event,      // 이벤트 처리 중
            GameOver,   // 게임 오버 (사망)
            Victory     // 보스 처치 / 귀환
        }

        // ─────────────────────────────────────────────────────────────
        // 현재 상태
        // ─────────────────────────────────────────────────────────────
        public GameState CurrentState { get; private set; } = GameState.None;

        /// <summary>상태가 변경될 때 외부 시스템이 구독할 수 있는 이벤트</summary>
        public event Action<GameState, GameState> OnGameStateChanged;

        // ─────────────────────────────────────────────────────────────
        // 플레이어 런타임 데이터 (런 시작 시 초기화)
        // ─────────────────────────────────────────────────────────────
        [Header("플레이어 기본 데이터 (인스펙터에서 연결)")]
        [SerializeField] private PlayerData defaultPlayerData;

        /// <summary>현재 런의 플레이어 런타임 인스턴스</summary>
        public Player CurrentPlayer { get; private set; }

        // ─────────────────────────────────────────────────────────────
        // 의존 시스템 참조
        // ─────────────────────────────────────────────────────────────
        public RNGSystem RNG { get; private set; }

        // ─────────────────────────────────────────────────────────────
        // Unity 라이프사이클
        // ─────────────────────────────────────────────────────────────
        private void Awake()
        {
            // 싱글톤 보장: 중복 인스턴스 즉시 제거
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        /// <summary>
        /// 게임 시작 시 필요한 시스템을 초기화한다.
        /// </summary>
        private void Initialize()
        {
            RNG = new RNGSystem();
            Debug.Log("[GameManager] 초기화 완료");
        }

        // ─────────────────────────────────────────────────────────────
        // 게임 상태 전환 API
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 새 런을 시작한다. 플레이어 데이터를 초기화하고 던전으로 진입한다.
        /// </summary>
        public void StartNewRun()
        {
            // TODO: PlayerData를 기반으로 Player 런타임 인스턴스 생성
            // TODO: 영구 업그레이드(메타 진행) 적용
            ChangeState(GameState.Town);
        }

        /// <summary>
        /// 던전 입장 처리. 씬 전환 후 던전 탐색 상태로 진입한다.
        /// </summary>
        public void EnterDungeon()
        {
            if (CurrentState != GameState.Town)
            {
                Debug.LogWarning("[GameManager] 마을 상태에서만 던전에 입장할 수 있습니다.");
                return;
            }

            // TODO: SceneController를 통해 던전 씬으로 전환
            ChangeState(GameState.Dungeon);
        }

        /// <summary>
        /// 플레이어 사망 처리.
        /// </summary>
        public void OnPlayerDeath()
        {
            Debug.Log("[GameManager] 플레이어 사망 처리");
            // TODO: 획득 자원 정산
            // TODO: 사망 횟수 등 영구 데이터 저장
            ChangeState(GameState.GameOver);
        }

        /// <summary>
        /// 던전 귀환(클리어 또는 탈출) 처리.
        /// </summary>
        public void ReturnToTown()
        {
            // TODO: 획득 자원 정산 및 영구 데이터 저장
            ChangeState(GameState.Town);
        }

        // ─────────────────────────────────────────────────────────────
        // 내부 헬퍼
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 상태를 전환하고 구독자에게 변경 사실을 알린다.
        /// </summary>
        private void ChangeState(GameState nextState)
        {
            GameState prev = CurrentState;
            CurrentState = nextState;

            Debug.Log($"[GameManager] 상태 전환: {prev} → {nextState}");
            OnGameStateChanged?.Invoke(prev, nextState);
        }
    }
}

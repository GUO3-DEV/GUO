using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoguelikeRPG.Core
{
    /// <summary>
    /// 씬 전환을 담당하는 컨트롤러.
    /// 페이드 인/아웃 등 전환 연출을 처리하며, GameManager와 분리하여
    /// 씬 로딩 책임을 단일화한다.
    /// </summary>
    public class SceneController : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────
        // 씬 이름 상수 (Build Settings의 씬 이름과 반드시 일치해야 함)
        // ─────────────────────────────────────────────────────────────
        public static class SceneName
        {
            public const string Boot    = "Boot";     // 초기 로딩, 시스템 초기화
            public const string Town    = "Town";     // 마을 화면
            public const string Dungeon = "Dungeon";  // 던전 탐색 / 이벤트
            public const string Battle  = "Battle";   // 전투 화면
        }

        // ─────────────────────────────────────────────────────────────
        // 싱글톤
        // ─────────────────────────────────────────────────────────────
        public static SceneController Instance { get; private set; }

        // ─────────────────────────────────────────────────────────────
        // 전환 상태
        // ─────────────────────────────────────────────────────────────
        public bool IsTransitioning { get; private set; }

        /// <summary>씬 전환이 완료됐을 때 발행되는 이벤트 (전환된 씬 이름 전달)</summary>
        public event Action<string> OnSceneLoaded;

        [Header("전환 연출 설정")]
        [SerializeField] private float fadeDuration = 0.3f;

        // ─────────────────────────────────────────────────────────────
        // Unity 라이프사이클
        // ─────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ─────────────────────────────────────────────────────────────
        // 퍼블릭 API
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 지정한 씬으로 페이드 전환한다.
        /// 전환 중에는 중복 호출이 무시된다.
        /// </summary>
        /// <param name="sceneName">전환할 씬 이름 (SceneName 상수 사용 권장)</param>
        public void LoadScene(string sceneName)
        {
            if (IsTransitioning)
            {
                Debug.LogWarning("[SceneController] 이미 씬 전환 중입니다.");
                return;
            }

            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        // ─────────────────────────────────────────────────────────────
        // 코루틴
        // ─────────────────────────────────────────────────────────────

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            IsTransitioning = true;

            // TODO: 페이드 아웃 연출 (CanvasGroup.alpha 애니메이션)
            yield return new WaitForSeconds(fadeDuration);

            // 씬 비동기 로드
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            // 로딩이 90%까지 완료되면 (Unity 비동기 로딩의 기본 동작)
            while (op.progress < 0.9f)
            {
                yield return null;
            }

            // TODO: 로딩 완료 후 추가 초기화가 필요하면 여기서 처리
            op.allowSceneActivation = true;

            yield return op;

            // TODO: 페이드 인 연출
            yield return new WaitForSeconds(fadeDuration);

            IsTransitioning = false;
            OnSceneLoaded?.Invoke(sceneName);
            Debug.Log($"[SceneController] 씬 전환 완료: {sceneName}");
        }
    }
}

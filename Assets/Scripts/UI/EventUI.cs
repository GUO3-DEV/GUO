using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeRPG.Event;

namespace RoguelikeRPG.UI
{
    /// <summary>
    /// 이벤트 화면 UI를 담당하는 Presenter.
    /// EventManager의 이벤트를 구독하여 텍스트와 선택지 버튼을 렌더링하고,
    /// 플레이어 선택을 EventManager에 전달한다.
    ///
    /// 주의: UI 렌더링만 담당. 이벤트 로직은 EventManager에 위임한다.
    /// </summary>
    public class EventUI : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────
        // 인스펙터 참조 - 이벤트 패널
        // ─────────────────────────────────────────────────────────────
        [Header("이벤트 패널")]
        [SerializeField] private GameObject    eventPanel;
        [SerializeField] private Image         backgroundImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;

        // ─────────────────────────────────────────────────────────────
        // 인스펙터 참조 - 선택지 버튼
        // ─────────────────────────────────────────────────────────────
        [Header("선택지 버튼")]
        [Tooltip("선택지 버튼 프리팹 (ChoiceButtonItem 컴포넌트 포함)")]
        [SerializeField] private ChoiceButtonItem choiceButtonPrefab;

        [SerializeField] private Transform choiceContainer;

        // ─────────────────────────────────────────────────────────────
        // 인스펙터 참조 - 결과 텍스트
        // ─────────────────────────────────────────────────────────────
        [Header("결과 표시")]
        [SerializeField] private GameObject      resultOverlay;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button          continueButton;

        // ─────────────────────────────────────────────────────────────
        // 런타임 참조
        // ─────────────────────────────────────────────────────────────
        private EventManager            _eventManager;
        private List<ChoiceButtonItem>  _choiceButtons = new();

        // ─────────────────────────────────────────────────────────────
        // Unity 라이프사이클
        // ─────────────────────────────────────────────────────────────
        private void Awake()
        {
            _eventManager = FindFirstObjectByType<EventManager>();

            if (_eventManager == null)
            {
                Debug.LogError("[EventUI] EventManager를 찾을 수 없습니다.");
                return;
            }

            SubscribeEvents();

            // 초기 상태: 이벤트 패널 비활성화
            eventPanel.SetActive(false);
            resultOverlay.SetActive(false);
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        // ─────────────────────────────────────────────────────────────
        // 이벤트 구독 / 해제
        // ─────────────────────────────────────────────────────────────
        private void SubscribeEvents()
        {
            _eventManager.OnEventStarted    += HandleEventStarted;
            _eventManager.OnEventEnded      += HandleEventEnded;
            _eventManager.OnChoiceResolved  += HandleChoiceResolved;
        }

        private void UnsubscribeEvents()
        {
            if (_eventManager == null) return;

            _eventManager.OnEventStarted    -= HandleEventStarted;
            _eventManager.OnEventEnded      -= HandleEventEnded;
            _eventManager.OnChoiceResolved  -= HandleChoiceResolved;
        }

        // ─────────────────────────────────────────────────────────────
        // 이벤트 핸들러
        // ─────────────────────────────────────────────────────────────

        private void HandleEventStarted(EventData eventData)
        {
            eventPanel.SetActive(true);
            resultOverlay.SetActive(false);

            // 텍스트 렌더링
            titleText.text = eventData.title;
            bodyText.text  = eventData.bodyText;

            // 배경 이미지 (없으면 기본 유지)
            if (backgroundImage != null && eventData.backgroundImage != null)
                backgroundImage.sprite = eventData.backgroundImage;

            // 선택지 버튼 렌더링
            BuildChoiceButtons(eventData);
        }

        private void HandleEventEnded()
        {
            eventPanel.SetActive(false);
            ClearChoiceButtons();
        }

        private void HandleChoiceResolved(string resultDescription)
        {
            // 선택 결과 오버레이 표시
            resultOverlay.SetActive(true);
            resultText.text = resultDescription;

            // 선택지 버튼 잠금 (중복 선택 방지)
            SetChoiceButtonsInteractable(false);

            // 계속하기 버튼 활성화
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnClickContinue);
        }

        // ─────────────────────────────────────────────────────────────
        // 선택지 버튼 렌더링
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// EventData의 선택지 목록에 맞게 버튼을 동적 생성한다.
        /// 조건을 만족하지 않는 선택지는 비활성화된 상태로 표시한다.
        /// </summary>
        private void BuildChoiceButtons(EventData eventData)
        {
            ClearChoiceButtons();

            for (int i = 0; i < eventData.choices.Count; i++)
            {
                EventChoice choice = eventData.choices[i];
                ChoiceButtonItem btn = Instantiate(choiceButtonPrefab, choiceContainer);

                // 선택지 조건 평가는 EventManager의 ChoiceHandler에게 위임
                // UI는 결과만 받아서 버튼 상태를 설정
                bool isAvailable = _eventManager != null
                    // TODO: EventManager에서 EvaluateConditions 접근 방법 노출 (현재 내부 메서드)
                    // 임시로 항상 활성화
                    && true;

                int capturedIndex = i; // 클로저 캡처를 위한 지역 복사
                btn.Setup(choice.choiceText, choice.hintText, isAvailable, () =>
                {
                    OnClickChoice(capturedIndex);
                });

                _choiceButtons.Add(btn);
            }
        }

        private void ClearChoiceButtons()
        {
            foreach (ChoiceButtonItem btn in _choiceButtons)
            {
                if (btn != null)
                    Destroy(btn.gameObject);
            }

            _choiceButtons.Clear();
        }

        private void SetChoiceButtonsInteractable(bool interactable)
        {
            foreach (ChoiceButtonItem btn in _choiceButtons)
            {
                if (btn != null)
                    btn.SetInteractable(interactable);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 버튼 클릭 핸들러
        // ─────────────────────────────────────────────────────────────

        private void OnClickChoice(int choiceIndex)
        {
            _eventManager.SelectChoice(choiceIndex);
        }

        private void OnClickContinue()
        {
            resultOverlay.SetActive(false);
            // EventManager가 이미 다음 이벤트 체인 혹은 종료를 처리했으므로 UI만 닫는다.
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // 선택지 버튼 단일 아이템 컴포넌트
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// 선택지 버튼 하나를 구성하는 컴포넌트.
    /// 프리팹에 붙여서 사용한다.
    /// </summary>
    public class ChoiceButtonItem : MonoBehaviour
    {
        [SerializeField] private Button          button;
        [SerializeField] private TextMeshProUGUI choiceText;
        [SerializeField] private TextMeshProUGUI hintText;

        [Tooltip("선택 불가 상태의 색상")]
        [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f);

        /// <summary>
        /// 버튼을 초기화한다. 조건 미충족 시 비활성화 상태로 표시한다.
        /// </summary>
        public void Setup(string choice, string hint, bool isAvailable, System.Action onClick)
        {
            choiceText.text = choice;

            if (hintText != null)
            {
                hintText.text    = isAvailable ? hint : $"[조건 미충족] {hint}";
                hintText.color   = isAvailable ? Color.white : disabledColor;
            }

            button.interactable = isAvailable;

            button.onClick.RemoveAllListeners();
            if (isAvailable)
                button.onClick.AddListener(() => onClick?.Invoke());
        }

        /// <summary>버튼 상호작용 가능 여부를 설정한다. (선택 완료 후 잠금에 사용)</summary>
        public void SetInteractable(bool interactable)
        {
            button.interactable = interactable;
        }
    }
}

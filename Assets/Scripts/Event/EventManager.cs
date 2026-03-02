using System;
using System.Collections.Generic;
using UnityEngine;
using RoguelikeRPG.Core;
using RoguelikeRPG.Battle;
using RoguelikeRPG.Data;

namespace RoguelikeRPG.Event
{
    /// <summary>
    /// 던전 탐색 중 이벤트 풀 관리, 이벤트 추첨, 이벤트 진행을 총괄하는 매니저.
    /// EventUI에 이벤트 데이터를 전달하고 ChoiceHandler에 결과 실행을 위임한다.
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────
        // 이벤트 (EventUI가 구독)
        // ─────────────────────────────────────────────────────────────

        /// <summary>새 이벤트가 시작될 때 발행. (표시할 EventData)</summary>
        public event Action<EventData> OnEventStarted;

        /// <summary>이벤트가 종료될 때 발행.</summary>
        public event Action OnEventEnded;

        /// <summary>선택지 처리 결과를 UI에 전달할 때 발행. (결과 설명 텍스트)</summary>
        public event Action<string> OnChoiceResolved;

        // ─────────────────────────────────────────────────────────────
        // 이벤트 풀
        // ─────────────────────────────────────────────────────────────
        [Header("이벤트 데이터 풀 (인스펙터에서 연결)")]
        [SerializeField] private List<EventData> eventPool = new();

        // ─────────────────────────────────────────────────────────────
        // 런타임 상태
        // ─────────────────────────────────────────────────────────────
        public EventData  CurrentEvent   { get; private set; }
        public bool       IsEventActive  { get; private set; }

        // 이벤트 체인 진행 시 ID로 다음 이벤트를 빠르게 조회하기 위한 캐시
        private Dictionary<string, EventData> _eventLookup = new();

        // ─────────────────────────────────────────────────────────────
        // 의존성
        // ─────────────────────────────────────────────────────────────
        private ChoiceHandler _choiceHandler;
        private RNGSystem     _rng;

        // ─────────────────────────────────────────────────────────────
        // Unity 라이프사이클
        // ─────────────────────────────────────────────────────────────
        private void Awake()
        {
            BuildEventLookup();
        }

        /// <summary>
        /// BattleManager(또는 DungeonManager)로부터 초기화를 받는다.
        /// Player가 생성된 이후에 호출해야 한다.
        /// </summary>
        public void Initialize(Player player, RNGSystem rng)
        {
            _rng           = rng;
            _choiceHandler = new ChoiceHandler(player, rng);
            Debug.Log("[EventManager] 초기화 완료");
        }

        // ─────────────────────────────────────────────────────────────
        // 이벤트 추첨 및 시작
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 현재 던전 층에 맞는 이벤트를 가중치 기반으로 추첨하여 시작한다.
        /// </summary>
        /// <param name="currentFloor">현재 던전 층 (이벤트 필터링에 사용)</param>
        public void TriggerRandomEvent(int currentFloor)
        {
            EventData selected = PickEventForFloor(currentFloor);

            if (selected == null)
            {
                Debug.LogWarning($"[EventManager] 층 {currentFloor}에 적합한 이벤트가 없습니다.");
                return;
            }

            StartEvent(selected);
        }

        /// <summary>
        /// 특정 이벤트 ID로 직접 이벤트를 시작한다. (이벤트 체인, 퀘스트 등에서 사용)
        /// </summary>
        public void TriggerEventById(string eventId)
        {
            if (!_eventLookup.TryGetValue(eventId, out EventData data))
            {
                Debug.LogError($"[EventManager] 이벤트 ID를 찾을 수 없습니다: {eventId}");
                return;
            }

            StartEvent(data);
        }

        private void StartEvent(EventData eventData)
        {
            if (IsEventActive)
            {
                Debug.LogWarning("[EventManager] 이미 이벤트 진행 중입니다.");
                return;
            }

            CurrentEvent  = eventData;
            IsEventActive = true;

            Debug.Log($"[EventManager] 이벤트 시작: {eventData.title} ({eventData.eventId})");
            OnEventStarted?.Invoke(eventData);
        }

        // ─────────────────────────────────────────────────────────────
        // 선택지 처리 (EventUI가 호출)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 플레이어가 선택지를 선택했을 때 EventUI가 호출한다.
        /// 조건 재검증 후 결과를 실행하고, 다음 이벤트가 있으면 체인한다.
        /// </summary>
        /// <param name="choiceIndex">선택된 선택지 인덱스</param>
        public void SelectChoice(int choiceIndex)
        {
            if (!IsEventActive || CurrentEvent == null)
            {
                Debug.LogWarning("[EventManager] 활성 이벤트가 없습니다.");
                return;
            }

            if (choiceIndex < 0 || choiceIndex >= CurrentEvent.choices.Count)
            {
                Debug.LogError($"[EventManager] 유효하지 않은 선택지 인덱스: {choiceIndex}");
                return;
            }

            EventChoice choice = CurrentEvent.choices[choiceIndex];

            // 선택 가능 조건 재검증 (UI에서 버튼을 막더라도 이중 검증)
            if (!_choiceHandler.EvaluateConditions(choice))
            {
                Debug.LogWarning("[EventManager] 선택 조건 미충족");
                return;
            }

            Debug.Log($"[EventManager] 선택: {choice.choiceText}");

            // 결과 실행
            MonsterData battleTrigger = _choiceHandler.ExecuteOutcomes(choice);
            OnChoiceResolved?.Invoke(BuildResultSummary(choice));

            // 전투 발동 여부 확인
            if (battleTrigger != null || CurrentEvent.eventType == EventType.Battle)
            {
                MonsterData monster = battleTrigger ?? CurrentEvent.linkedMonster;
                EndEvent();
                // TODO: BattleManager.StartBattle(player, monster) 호출
                return;
            }

            // 다음 이벤트 체인이 있으면 이어서 진행
            if (!string.IsNullOrEmpty(choice.nextEventId))
            {
                EndEvent();
                TriggerEventById(choice.nextEventId);
            }
            else
            {
                EndEvent();
            }
        }

        /// <summary>현재 이벤트를 종료한다.</summary>
        public void EndEvent()
        {
            if (!IsEventActive) return;

            Debug.Log($"[EventManager] 이벤트 종료: {CurrentEvent?.eventId}");
            CurrentEvent  = null;
            IsEventActive = false;

            OnEventEnded?.Invoke();
        }

        // ─────────────────────────────────────────────────────────────
        // 내부 헬퍼
        // ─────────────────────────────────────────────────────────────

        /// <summary>이벤트 풀에서 현재 층에 맞는 이벤트를 가중치 기반으로 선택한다.</summary>
        private EventData PickEventForFloor(int floor)
        {
            // 현재 층에 등장 가능한 이벤트 필터링
            var candidates = new List<EventData>();
            var weights    = new List<float>();

            foreach (EventData ev in eventPool)
            {
                if (floor >= ev.floorRange.x && floor <= ev.floorRange.y)
                {
                    candidates.Add(ev);
                    weights.Add(ev.spawnWeight);
                }
            }

            if (candidates.Count == 0) return null;

            return _rng.WeightedRandom(candidates, weights);
        }

        /// <summary>이벤트 풀을 ID 기반 딕셔너리로 캐시한다.</summary>
        private void BuildEventLookup()
        {
            _eventLookup.Clear();

            foreach (EventData ev in eventPool)
            {
                if (string.IsNullOrEmpty(ev.eventId)) continue;

                if (!_eventLookup.TryAdd(ev.eventId, ev))
                    Debug.LogWarning($"[EventManager] 중복 이벤트 ID: {ev.eventId}");
            }
        }

        /// <summary>선택 결과에 대한 간략한 요약 텍스트를 생성한다.</summary>
        private string BuildResultSummary(EventChoice choice)
        {
            // TODO: ChoiceOutcome 목록을 보기 좋은 한국어 문장으로 변환
            return $"[{choice.choiceText}] 선택 완료";
        }
    }
}

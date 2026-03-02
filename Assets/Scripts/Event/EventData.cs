using System.Collections.Generic;
using UnityEngine;
using RoguelikeRPG.Data;

namespace RoguelikeRPG.Event
{
    /// <summary>
    /// 던전 탐색 중 발생하는 이벤트 하나를 정의하는 ScriptableObject.
    /// 텍스트, 선택지, 결과를 데이터로 기술하여 기획자가 코드 없이 이벤트를 제작할 수 있다.
    /// Assets/Data/Events/ 경로에 에셋으로 생성한다.
    ///
    /// ===== 새 이벤트 추가 방법 =====
    /// 1. Unity 에디터 메뉴: Assets > Create > RoguelikeRPG/Data/EventData
    /// 2. 파일명 설정: EventData_신비한제단 (예시)
    /// 3. 인스펙터에서 다음을 입력:
    ///    - eventId: "mystery_altar" (영문 고유ID)
    ///    - eventType: Random (이벤트 분류)
    ///    - title: "신비한 제단" (화면 표시 이름)
    ///    - bodyText: "어두운 제단 앞에 섰다. [접근할까, 무시할까?]"
    ///    - spawnWeight: 1f (등장 빈도, 높을수록 자주 나옴)
    /// 4. choices 리스트에 선택지 추가:
    ///    - 선택지마다 choiceText, conditions[], outcomes[] 설정
    ///    - outcomes에서 HealHp, LoseHp, GainGold, GainItem 등 결과 정의
    /// 5. 같은 프로젝트의 다른 이벤트/선택지의 nextEventId에 이 이벤트 ID 참조
    /// </summary>
    [CreateAssetMenu(
        fileName = "EventData_New",
        menuName  = "RoguelikeRPG/Data/EventData",
        order     = 12)]
    public class EventData : ScriptableObject
    {
        // ─────────────────────────────────────────────────────────────
        // 기본 정보
        // ─────────────────────────────────────────────────────────────
        [Header("기본 정보")]
        [Tooltip("이벤트 고유 ID (다른 이벤트/선택지 결과에서 참조할 때 사용)")]
        public string eventId;

        [Tooltip("이벤트 분류 (전투/상점/랜덤이벤트/보스)")]
        public EventType eventType = EventType.Random;

        [Tooltip("이벤트가 등장할 수 있는 최소/최대 던전 층")]
        public Vector2Int floorRange = new(1, 99);

        [Header("텍스트")]
        [Tooltip("이벤트 제목 (이벤트 패널 상단에 표시)")]
        public string title;

        [TextArea(3, 10)]
        [Tooltip("이벤트 본문 텍스트. {playerName} 등 플레이스홀더 지원 예정")]
        public string bodyText;

        [Header("배경 이미지 (선택)")]
        [Tooltip("이벤트 UI 배경에 표시할 이미지 (없으면 기본 배경 사용)")]
        public Sprite backgroundImage;

        // ─────────────────────────────────────────────────────────────
        // 선택지
        // ─────────────────────────────────────────────────────────────
        [Header("선택지")]
        [Tooltip("플레이어에게 표시할 선택지 목록 (최소 1개 이상)")]
        public List<EventChoice> choices = new();

        // ─────────────────────────────────────────────────────────────
        // 전투 이벤트 전용
        // ─────────────────────────────────────────────────────────────
        [Header("전투 연결 (EventType.Battle 전용)")]
        [Tooltip("이 이벤트가 Battle 타입일 때 생성할 몬스터 데이터")]
        public MonsterData linkedMonster;

        // ─────────────────────────────────────────────────────────────
        // 등장 가중치
        // ─────────────────────────────────────────────────────────────
        [Header("등장 가중치")]
        [Tooltip("던전 이벤트 풀에서 이 이벤트가 뽑힐 가중치. 높을수록 자주 등장")]
        [Range(0.1f, 10f)]
        public float spawnWeight = 1f;
    }

    // ─────────────────────────────────────────────────────────────────
    // 관련 열거형 / 클래스
    // ─────────────────────────────────────────────────────────────────

    /// <summary>이벤트 분류</summary>
    public enum EventType
    {
        Battle,     // 전투 이벤트 (BattleManager로 전환)
        Shop,       // 상점 이벤트
        Random,     // 랜덤 이벤트 (선택지 분기)
        Rest,       // 휴식 지점 (체력 회복)
        Boss,       // 보스 전투
        Treasure    // 보물 획득
    }

    /// <summary>
    /// 이벤트 화면에서 플레이어가 선택할 수 있는 선택지 하나.
    /// </summary>
    [System.Serializable]
    public class EventChoice
    {
        [Tooltip("버튼에 표시되는 선택지 텍스트")]
        public string choiceText;

        [TextArea(2, 4)]
        [Tooltip("선택지에 대한 플레이어 힌트 (잠금 조건 설명 등)")]
        public string hintText;

        [Tooltip("선택 가능 조건 목록. 모든 조건을 만족해야 선택 가능")]
        public List<ChoiceCondition> conditions = new();

        [Tooltip("이 선택지를 선택했을 때 실행할 결과 목록")]
        public List<ChoiceOutcome> outcomes = new();

        [Tooltip("이 선택지 클릭 후 이어서 표시할 다음 이벤트 ID (없으면 이벤트 종료)")]
        public string nextEventId;
    }

    /// <summary>
    /// 선택지 활성화 조건.
    /// </summary>
    [System.Serializable]
    public class ChoiceCondition
    {
        [Tooltip("조건 종류")]
        public ConditionType conditionType;

        [Tooltip("비교 기준 값")]
        public float value;

        [Tooltip("특정 아이템 ID 조건에 사용")]
        public string itemId;
    }

    /// <summary>조건 종류</summary>
    public enum ConditionType
    {
        None,
        MinHpPercent,   // 현재 체력이 value% 이상
        MaxHpPercent,   // 현재 체력이 value% 이하
        MinGold,        // 골드가 value 이상
        HasItem,        // 특정 아이템 보유
    }

    /// <summary>
    /// 선택지를 선택했을 때 발생하는 결과 하나.
    /// </summary>
    [System.Serializable]
    public class ChoiceOutcome
    {
        [Tooltip("결과 종류")]
        public OutcomeType outcomeType;

        [Tooltip("결과 수치 값")]
        public float value;

        [Tooltip("아이템 지급/소모에 사용할 아이템 데이터")]
        public ItemData relatedItem;

        [Tooltip("이 결과의 발생 확률 (0.0 ~ 1.0). 1.0이면 항상 발생")]
        [Range(0f, 1f)]
        public float probability = 1f;
    }

    /// <summary>결과 종류</summary>
    public enum OutcomeType
    {
        HealHp,         // 체력 회복 (value = 회복량)
        HealHpPercent,  // 체력 퍼센트 회복
        LoseHp,         // 체력 손실
        GainGold,       // 골드 획득
        LoseGold,       // 골드 소모
        GainItem,       // 아이템 획득
        LoseItem,       // 아이템 소모
        TriggerBattle,  // 전투 발생 (linkedMonster 필요)
        Nothing         // 아무 일도 없음 (낚시 선택지)
    }
}

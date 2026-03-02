using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeRPG.Core
{
    /// <summary>
    /// 게임 전체의 난수 생성을 중앙화하는 시스템.
    /// 시드 기반으로 재현 가능한 결과를 보장하며,
    /// 가중치 랜덤 추첨(Weighted Random) 기능을 제공한다.
    /// </summary>
    public class RNGSystem
    {
        // ─────────────────────────────────────────────────────────────
        // 내부 상태
        // ─────────────────────────────────────────────────────────────
        private System.Random _random;
        private int _currentSeed;

        // ─────────────────────────────────────────────────────────────
        // 생성자
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 기본 생성자: 현재 시각 기반의 시드를 사용한다.
        /// </summary>
        public RNGSystem()
        {
            SetSeed(Environment.TickCount);
        }

        /// <summary>
        /// 지정 시드로 초기화한다. 동일 시드라면 동일한 결과 시퀀스를 보장한다.
        /// </summary>
        public RNGSystem(int seed)
        {
            SetSeed(seed);
        }

        // ─────────────────────────────────────────────────────────────
        // 시드 관리
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 시드를 설정하고 난수 생성기를 재초기화한다.
        /// 동일 시드는 동일한 결과 시퀀스를 재현한다.
        /// </summary>
        public void SetSeed(int seed)
        {
            _currentSeed = seed;
            _random = new System.Random(seed);
            Debug.Log($"[RNGSystem] 시드 설정: {seed}");
        }

        /// <summary>현재 사용 중인 시드 값</summary>
        public int CurrentSeed => _currentSeed;

        // ─────────────────────────────────────────────────────────────
        // 기본 난수 API
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// [0.0, 1.0) 범위의 float 난수를 반환한다.
        /// </summary>
        public float NextFloat()
        {
            return (float)_random.NextDouble();
        }

        /// <summary>
        /// [min, max] 범위의 int 난수를 반환한다. (양 끝 포함)
        /// </summary>
        public int NextInt(int min, int max)
        {
            if (min > max)
                throw new ArgumentException($"[RNGSystem] min({min}) > max({max})");

            return _random.Next(min, max + 1);
        }

        /// <summary>
        /// 지정한 확률로 성공 여부를 반환한다.
        /// </summary>
        /// <param name="chance">성공 확률 (0.0 ~ 1.0)</param>
        public bool Roll(float chance)
        {
            return NextFloat() < Mathf.Clamp01(chance);
        }

        /// <summary>
        /// 퍼센트 단위 확률로 성공 여부를 반환한다. (0 ~ 100)
        /// </summary>
        public bool RollPercent(float percent)
        {
            return Roll(percent / 100f);
        }

        // ─────────────────────────────────────────────────────────────
        // 가중치 랜덤 추첨
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 각 항목에 가중치를 부여하여 랜덤으로 하나를 선택한다.
        /// 가중치가 높을수록 뽑힐 확률이 높다.
        /// </summary>
        /// <typeparam name="T">항목 타입</typeparam>
        /// <param name="items">항목 목록</param>
        /// <param name="weights">각 항목에 대응하는 가중치 (items와 길이가 같아야 함)</param>
        /// <returns>선택된 항목</returns>
        public T WeightedRandom<T>(IList<T> items, IList<float> weights)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("[RNGSystem] 항목 목록이 비어 있습니다.");

            if (items.Count != weights.Count)
                throw new ArgumentException("[RNGSystem] items와 weights의 길이가 다릅니다.");

            // 가중치 합계 계산
            float totalWeight = 0f;
            foreach (float w in weights)
            {
                if (w < 0)
                    throw new ArgumentException("[RNGSystem] 가중치는 0 이상이어야 합니다.");
                totalWeight += w;
            }

            if (totalWeight <= 0f)
                throw new ArgumentException("[RNGSystem] 가중치 합이 0입니다.");

            // 무작위 지점을 구하고 누적합으로 해당 구간 탐색
            float point = NextFloat() * totalWeight;
            float cumulative = 0f;

            for (int i = 0; i < items.Count; i++)
            {
                cumulative += weights[i];
                if (point < cumulative)
                    return items[i];
            }

            // 부동소수점 오차로 마지막 항목까지 통과된 경우 마지막 반환
            return items[items.Count - 1];
        }

        /// <summary>
        /// 리스트에서 무작위 하나를 균등 확률로 반환한다.
        /// </summary>
        public T Pick<T>(IList<T> items)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("[RNGSystem] 항목 목록이 비어 있습니다.");

            return items[NextInt(0, items.Count - 1)];
        }

        /// <summary>
        /// 리스트를 Fisher-Yates 알고리즘으로 제자리 셔플한다.
        /// </summary>
        public void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = NextInt(0, i);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 데미지 변동 계산 (전투에서 자주 사용)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 기준 데미지에 ±variance 범위의 무작위 변동을 적용한 결과를 반환한다.
        /// 예) baseDamage=100, variance=0.1 → [90, 110] 사이 정수
        /// </summary>
        /// <param name="baseDamage">기준 데미지</param>
        /// <param name="variance">변동 비율 (0.0 ~ 1.0)</param>
        public int RollDamage(int baseDamage, float variance = 0.1f)
        {
            float ratio = 1f + (NextFloat() * 2f - 1f) * Mathf.Clamp01(variance);
            return Mathf.Max(1, Mathf.RoundToInt(baseDamage * ratio));
        }
    }
}

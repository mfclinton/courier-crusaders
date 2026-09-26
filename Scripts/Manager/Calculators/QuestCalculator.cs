using UnityEngine;
using MathNet.Numerics.Distributions;

namespace Manager
{
    public class QuestCalculator : MonoBehaviour
    {
        public float baseReward;
        public float rewardPerDistance;
        public float baseDays;
        public float daysPerDistance;

        public int CalculateMaxDays(float distance)
        {
            Normal normalDist = new Normal(baseDays + (distance * daysPerDistance), 1.0);
            float maxDays = (float)normalDist.Sample();
            return Mathf.RoundToInt(Mathf.Max(maxDays, 1.0f)); // Ensure at least 1 day
        }

        public int CalculateTotalReward(float distance)
        {
            float rewardPerDistanceSquared = baseReward + (Mathf.Pow(distance, 2) * rewardPerDistance);
            Normal normalDist = new Normal(rewardPerDistanceSquared, 4.0);
            float totalReward = (float)normalDist.Sample();
            return Mathf.RoundToInt(Mathf.Max(totalReward, 1.0f)); // Ensure reward is non-negative
        }
    }

}

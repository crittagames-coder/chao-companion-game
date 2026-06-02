using System;
using UnityEngine;

namespace ChaoCompanion.Core
{
    [Serializable]
    public class CompanionStats
    {
        [Range(0f, 100f)] public float hunger = 25f;
        [Range(0f, 100f)] public float happiness = 60f;
        [Range(0f, 100f)] public float energy = 80f;
        [Range(0f, 100f)] public float trust = 20f;
        [Range(0f, 100f)] public float curiosity = 50f;

        public CompanionMood Mood { get; private set; } = CompanionMood.Calm;

        public void AddHunger(float amount)
        {
            hunger = ClampStat(hunger + amount);
            RefreshMood();
        }

        public void AddHappiness(float amount)
        {
            happiness = ClampStat(happiness + amount);
            RefreshMood();
        }

        public void AddEnergy(float amount)
        {
            energy = ClampStat(energy + amount);
            RefreshMood();
        }

        public void AddTrust(float amount)
        {
            trust = ClampStat(trust + amount);
            RefreshMood();
        }

        public void AddCuriosity(float amount)
        {
            curiosity = ClampStat(curiosity + amount);
            RefreshMood();
        }

        public void RefreshMood()
        {
            if (energy <= 20f)
            {
                Mood = CompanionMood.Sleepy;
            }
            else if (hunger >= 75f)
            {
                Mood = CompanionMood.Hungry;
            }
            else if (happiness <= 25f)
            {
                Mood = CompanionMood.Sad;
            }
            else if (happiness >= 75f && energy >= 45f)
            {
                Mood = CompanionMood.Playful;
            }
            else if (curiosity >= 70f)
            {
                Mood = CompanionMood.Curious;
            }
            else if (happiness >= 60f)
            {
                Mood = CompanionMood.Happy;
            }
            else
            {
                Mood = CompanionMood.Calm;
            }
        }

        private static float ClampStat(float value)
        {
            return Mathf.Clamp(value, 0f, 100f);
        }
    }
}

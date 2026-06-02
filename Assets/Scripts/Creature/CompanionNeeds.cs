using ChaoCompanion.Core;
using UnityEngine;

namespace ChaoCompanion.Creature
{
    public class CompanionNeeds : MonoBehaviour
    {
        [SerializeField] private CompanionStats stats = new();
        [SerializeField] private float hungerIncreasePerMinute = 4f;
        [SerializeField] private float happinessLossPerMinute = 2f;
        [SerializeField] private float energyLossPerMinute = 1.5f;
        [SerializeField] private float curiosityIncreasePerMinute = 1f;

        public CompanionStats Stats => stats;

        private void Awake()
        {
            stats.RefreshMood();
        }

        private void Update()
        {
            float minutes = Time.deltaTime / 60f;

            stats.AddHunger(hungerIncreasePerMinute * minutes);
            stats.AddHappiness(-happinessLossPerMinute * minutes);
            stats.AddEnergy(-energyLossPerMinute * minutes);
            stats.AddCuriosity(curiosityIncreasePerMinute * minutes);
        }
    }
}

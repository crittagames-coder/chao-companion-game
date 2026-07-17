using System;
using ChaoCompanion.AI;
using ChaoCompanion.Core;
using ChaoCompanion.Creature;
using UnityEngine;

namespace ChaoCompanion.Save
{
    public class CompanionSaveSystem : MonoBehaviour
    {
        private const string SaveKey = "chao_companion_save_v1";

        [SerializeField] private CompanionNeeds needs;
        [SerializeField] private CompanionBehaviorBrain brain;
        [SerializeField] private float autoSaveIntervalSeconds = 15f;

        private float nextAutoSaveTime;

        private void Start()
        {
            Load();
            nextAutoSaveTime = Time.unscaledTime + autoSaveIntervalSeconds;
        }

        private void Update()
        {
            if (Time.unscaledTime < nextAutoSaveTime)
            {
                return;
            }

            Save();
            nextAutoSaveTime = Time.unscaledTime + autoSaveIntervalSeconds;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Save();
            }
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        public void Bind(CompanionNeeds companionNeeds, CompanionBehaviorBrain companionBrain)
        {
            needs = companionNeeds;
            brain = companionBrain;
        }

        public void Save()
        {
            if (needs == null)
            {
                return;
            }

            CompanionStats stats = needs.Stats;
            SaveData data = new()
            {
                hunger = stats.hunger,
                happiness = stats.happiness,
                energy = stats.energy,
                trust = stats.trust,
                curiosity = stats.curiosity,
                lastReaction = brain != null ? brain.LastReaction : "Idle",
                savedAtUtcTicks = DateTime.UtcNow.Ticks
            };

            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public void Load()
        {
            if (needs == null || !PlayerPrefs.HasKey(SaveKey))
            {
                return;
            }

            SaveData data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey));
            CompanionStats stats = needs.Stats;
            stats.hunger = Mathf.Clamp(data.hunger, 0f, 100f);
            stats.happiness = Mathf.Clamp(data.happiness, 0f, 100f);
            stats.energy = Mathf.Clamp(data.energy, 0f, 100f);
            stats.trust = Mathf.Clamp(data.trust, 0f, 100f);
            stats.curiosity = Mathf.Clamp(data.curiosity, 0f, 100f);
            stats.RefreshMood();
        }

        [Serializable]
        private class SaveData
        {
            public float hunger;
            public float happiness;
            public float energy;
            public float trust;
            public float curiosity;
            public string lastReaction;
            public long savedAtUtcTicks;
        }
    }
}

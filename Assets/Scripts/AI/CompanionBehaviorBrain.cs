using ChaoCompanion.Core;
using ChaoCompanion.Creature;
using ChaoCompanion.Input;
using UnityEngine;

namespace ChaoCompanion.AI
{
    [RequireComponent(typeof(CompanionNeeds))]
    public class CompanionBehaviorBrain : MonoBehaviour
    {
        [SerializeField] private TouchGestureDetector gestureDetector;
        [SerializeField] private CompanionNeeds needs;

        public string LastReaction { get; private set; } = "Idle";

        private void Awake()
        {
            if (needs == null)
            {
                needs = GetComponent<CompanionNeeds>();
            }
        }

        private void OnEnable()
        {
            if (gestureDetector != null)
            {
                gestureDetector.InteractionDetected += HandleInteraction;
            }
        }

        private void OnDisable()
        {
            if (gestureDetector != null)
            {
                gestureDetector.InteractionDetected -= HandleInteraction;
            }
        }

        private void HandleInteraction(CompanionInteraction interaction)
        {
            CompanionStats stats = needs.Stats;

            switch (interaction.Type)
            {
                case CompanionInteractionType.Tap:
                    stats.AddHappiness(2f);
                    stats.AddCuriosity(1f);
                    LastReaction = stats.Mood == CompanionMood.Sleepy ? "Sleepy blink" : "Tiny hop";
                    break;

                case CompanionInteractionType.DoubleTap:
                    stats.AddHappiness(5f);
                    stats.AddEnergy(-2f);
                    LastReaction = "Excited bounce";
                    break;

                case CompanionInteractionType.LongPress:
                    stats.AddTrust(4f);
                    stats.AddHappiness(3f);
                    LastReaction = stats.trust >= 40f ? "Cuddle" : "Careful lean-in";
                    break;

                case CompanionInteractionType.Drag:
                    stats.AddCuriosity(2f * interaction.Intensity);
                    stats.AddEnergy(-1f * interaction.Intensity);
                    LastReaction = "Follow finger";
                    break;

                case CompanionInteractionType.Swipe:
                    stats.AddEnergy(-3f);
                    stats.AddHappiness(stats.Mood == CompanionMood.Sleepy ? -2f : 2f);
                    LastReaction = stats.Mood == CompanionMood.Sleepy ? "Annoyed wobble" : "Dash";
                    break;

                case CompanionInteractionType.Rub:
                    stats.AddTrust(2f);
                    stats.AddHappiness(2f);
                    LastReaction = "Petted";
                    break;
            }

            Debug.Log($"Companion reaction: {LastReaction} | Mood: {stats.Mood}");
        }
    }
}

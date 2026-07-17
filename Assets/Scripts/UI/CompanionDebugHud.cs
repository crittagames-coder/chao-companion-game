using ChaoCompanion.AI;
using ChaoCompanion.Creature;
using ChaoCompanion.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ChaoCompanion.UI
{
    public class CompanionDebugHud : MonoBehaviour
    {
        [SerializeField] private CompanionNeeds needs;
        [SerializeField] private CompanionBehaviorBrain brain;
        [SerializeField] private Text moodText;
        [SerializeField] private Text statsText;
        [SerializeField] private Text reactionText;

        private void Update()
        {
            if (needs == null)
            {
                return;
            }

            CompanionStats stats = needs.Stats;

            if (moodText != null)
            {
                moodText.text = $"Mood: {stats.Mood}";
            }

            if (statsText != null)
            {
                statsText.text =
                    $"Hunger: {stats.hunger:0}\n" +
                    $"Happiness: {stats.happiness:0}\n" +
                    $"Energy: {stats.energy:0}\n" +
                    $"Trust: {stats.trust:0}\n" +
                    $"Curiosity: {stats.curiosity:0}";
            }

            if (reactionText != null && brain != null)
            {
                reactionText.text = $"Reaction: {brain.LastReaction}";
            }
        }

        public void Bind(CompanionNeeds companionNeeds, CompanionBehaviorBrain companionBrain, Text mood, Text stats, Text reaction)
        {
            needs = companionNeeds;
            brain = companionBrain;
            moodText = mood;
            statsText = stats;
            reactionText = reaction;
        }
    }
}

using UnityEngine;

namespace ChaoCompanion.Core
{
    public enum CompanionInteractionType
    {
        Tap,
        DoubleTap,
        LongPress,
        Drag,
        Swipe,
        Rub
    }

    public readonly struct CompanionInteraction
    {
        public CompanionInteraction(
            CompanionInteractionType type,
            Vector2 screenPosition,
            Vector2 delta,
            float duration,
            float intensity)
        {
            Type = type;
            ScreenPosition = screenPosition;
            Delta = delta;
            Duration = duration;
            Intensity = intensity;
        }

        public CompanionInteractionType Type { get; }
        public Vector2 ScreenPosition { get; }
        public Vector2 Delta { get; }
        public float Duration { get; }
        public float Intensity { get; }
    }
}

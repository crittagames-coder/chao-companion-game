using System;
using ChaoCompanion.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ChaoCompanion.Input
{
    public class TouchGestureDetector : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Collider2D targetCollider;
        [SerializeField] private bool requireTargetTouch = true;

        [Header("Timing")]
        [SerializeField] private float tapMaxDuration = 0.25f;
        [SerializeField] private float doubleTapMaxGap = 0.3f;
        [SerializeField] private float longPressMinDuration = 0.65f;

        [Header("Distance")]
        [SerializeField] private float swipeMinDistance = 120f;
        [SerializeField] private float dragMinDistance = 24f;
        [SerializeField] private float rubMinTotalDistance = 220f;

        public event Action<CompanionInteraction> InteractionDetected;

        private Vector2 startPosition;
        private Vector2 lastPosition;
        private float startTime;
        private float lastTapTime = -10f;
        private float totalDragDistance;
        private bool dragging;
        private bool gestureActive;

        public Camera WorldCamera => worldCamera;

        public void SetTarget(Collider2D collider, Camera camera)
        {
            targetCollider = collider;
            worldCamera = camera;
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            if (Touch.activeTouches.Count > 0)
            {
                ProcessTouch(Touch.activeTouches[0]);
                return;
            }

            ProcessMouseForEditor();
        }

        private void ProcessTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    BeginGesture(touch.screenPosition);
                    break;

                case UnityEngine.InputSystem.TouchPhase.Moved:
                    MoveGesture(touch.screenPosition);
                    break;

                case UnityEngine.InputSystem.TouchPhase.Ended:
                case UnityEngine.InputSystem.TouchPhase.Canceled:
                    EndGesture(touch.screenPosition);
                    break;
            }
        }

        private void ProcessMouseForEditor()
        {
            if (Mouse.current == null)
            {
                return;
            }

            Vector2 position = Mouse.current.position.ReadValue();

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                BeginGesture(position);
            }
            else if (Mouse.current.leftButton.isPressed)
            {
                MoveGesture(position);
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                EndGesture(position);
            }
        }

        private void BeginGesture(Vector2 position)
        {
            gestureActive = CanStartGesture(position);
            if (!gestureActive)
            {
                return;
            }

            startPosition = position;
            lastPosition = position;
            startTime = Time.unscaledTime;
            totalDragDistance = 0f;
            dragging = false;
        }

        private void MoveGesture(Vector2 position)
        {
            if (!gestureActive)
            {
                return;
            }

            Vector2 step = position - lastPosition;
            totalDragDistance += step.magnitude;
            lastPosition = position;

            float distanceFromStart = Vector2.Distance(position, startPosition);
            if (distanceFromStart >= dragMinDistance)
            {
                dragging = true;
                float intensity = Mathf.Clamp01(distanceFromStart / swipeMinDistance);
                Emit(CompanionInteractionType.Drag, position, position - startPosition, Time.unscaledTime - startTime, intensity);
            }
        }

        private void EndGesture(Vector2 position)
        {
            if (!gestureActive)
            {
                return;
            }

            gestureActive = false;

            float duration = Time.unscaledTime - startTime;
            Vector2 delta = position - startPosition;
            float distance = delta.magnitude;

            if (dragging && totalDragDistance >= rubMinTotalDistance && distance < swipeMinDistance)
            {
                Emit(CompanionInteractionType.Rub, position, delta, duration, Mathf.Clamp01(totalDragDistance / rubMinTotalDistance));
                return;
            }

            if (distance >= swipeMinDistance)
            {
                Emit(CompanionInteractionType.Swipe, position, delta, duration, Mathf.Clamp01(distance / swipeMinDistance));
                return;
            }

            if (duration >= longPressMinDuration)
            {
                Emit(CompanionInteractionType.LongPress, position, delta, duration, Mathf.Clamp01(duration / longPressMinDuration));
                return;
            }

            if (duration <= tapMaxDuration)
            {
                bool isDoubleTap = Time.unscaledTime - lastTapTime <= doubleTapMaxGap;
                lastTapTime = Time.unscaledTime;
                Emit(isDoubleTap ? CompanionInteractionType.DoubleTap : CompanionInteractionType.Tap, position, delta, duration, 1f);
            }
        }

        private void Emit(CompanionInteractionType type, Vector2 position, Vector2 delta, float duration, float intensity)
        {
            InteractionDetected?.Invoke(new CompanionInteraction(type, position, delta, duration, intensity));
        }

        private bool CanStartGesture(Vector2 screenPosition)
        {
            if (!requireTargetTouch)
            {
                return true;
            }

            if (targetCollider == null)
            {
                return false;
            }

            Camera cameraToUse = worldCamera != null ? worldCamera : Camera.main;
            if (cameraToUse == null)
            {
                return false;
            }

            Vector3 worldPoint = cameraToUse.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -cameraToUse.transform.position.z));
            return targetCollider.OverlapPoint(worldPoint);
        }
    }
}

using ChaoCompanion.Core;
using ChaoCompanion.Input;
using UnityEngine;

namespace ChaoCompanion.Creature
{
    public class CompanionMotionController : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private float followSpeed = 12f;
        [SerializeField] private float squashAmount = 0.18f;
        [SerializeField] private float reactionDecaySpeed = 6f;
        [SerializeField] private float dragPlaneZ;

        private Vector3 homePosition;
        private Vector3 baseVisualScale = Vector3.one;
        private Vector3 visualOffset;
        private Vector3 targetPosition;
        private float bounceVelocity;
        private float wobbleTimer;
        private bool followingDrag;

        private void Awake()
        {
            if (visualRoot == null && transform.childCount > 0)
            {
                visualRoot = transform.GetChild(0);
            }

            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            homePosition = transform.position;
            targetPosition = transform.position;

            if (visualRoot != null)
            {
                baseVisualScale = visualRoot.localScale;
                visualOffset = visualRoot.localPosition;
            }
        }

        private void Update()
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

            if (followingDrag && Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                followingDrag = false;
            }

            if (!followingDrag)
            {
                targetPosition = Vector3.Lerp(targetPosition, homePosition, Time.deltaTime);
            }

            AnimateVisual();
        }

        public void SetVisualRoot(Transform root)
        {
            visualRoot = root;
            if (visualRoot != null)
            {
                baseVisualScale = visualRoot.localScale;
                visualOffset = visualRoot.localPosition;
            }
        }

        public void SetWorldCamera(Camera camera)
        {
            worldCamera = camera;
        }

        public void React(CompanionInteraction interaction, string reactionName)
        {
            switch (interaction.Type)
            {
                case CompanionInteractionType.Tap:
                    bounceVelocity = 0.45f;
                    wobbleTimer = 0.2f;
                    break;

                case CompanionInteractionType.DoubleTap:
                    bounceVelocity = 0.85f;
                    wobbleTimer = 0.5f;
                    break;

                case CompanionInteractionType.LongPress:
                    bounceVelocity = 0.15f;
                    wobbleTimer = 0.25f;
                    break;

                case CompanionInteractionType.Drag:
                    FollowScreenPoint(interaction.ScreenPosition);
                    break;

                case CompanionInteractionType.Swipe:
                    Dash(interaction.Delta);
                    break;

                case CompanionInteractionType.Rub:
                    bounceVelocity = 0.3f;
                    wobbleTimer = 0.8f;
                    break;
            }
        }

        private void FollowScreenPoint(Vector2 screenPosition)
        {
            Camera cameraToUse = worldCamera != null ? worldCamera : Camera.main;
            if (cameraToUse == null)
            {
                return;
            }

            Vector3 screenPoint = new(screenPosition.x, screenPosition.y, Mathf.Abs(cameraToUse.transform.position.z - dragPlaneZ));
            Vector3 worldPoint = cameraToUse.ScreenToWorldPoint(screenPoint);
            worldPoint.z = transform.position.z;
            targetPosition = worldPoint;
            followingDrag = true;
        }

        private void Dash(Vector2 screenDelta)
        {
            Vector3 direction = screenDelta.sqrMagnitude > 0.01f ? new Vector3(screenDelta.x, screenDelta.y, 0f).normalized : Vector3.right;
            targetPosition = homePosition + direction * 1.2f;
            bounceVelocity = 0.55f;
            wobbleTimer = 0.45f;
        }

        private void AnimateVisual()
        {
            if (visualRoot == null)
            {
                return;
            }

            bounceVelocity = Mathf.MoveTowards(bounceVelocity, 0f, reactionDecaySpeed * Time.deltaTime);
            wobbleTimer = Mathf.Max(0f, wobbleTimer - Time.deltaTime);

            float bounce = Mathf.Sin(bounceVelocity * Mathf.PI) * bounceVelocity;
            float wobble = wobbleTimer > 0f ? Mathf.Sin(Time.time * 28f) * wobbleTimer * 8f : 0f;
            float squash = Mathf.Abs(bounceVelocity) * squashAmount;

            visualRoot.localPosition = visualOffset + Vector3.up * bounce;
            visualRoot.localRotation = Quaternion.Euler(0f, 0f, wobble);
            visualRoot.localScale = new Vector3(
                baseVisualScale.x * (1f + squash),
                baseVisualScale.y * (1f - squash),
                baseVisualScale.z);
        }
    }
}

using ChaoCompanion.AI;
using ChaoCompanion.Creature;
using ChaoCompanion.Input;
using ChaoCompanion.Save;
using ChaoCompanion.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ChaoCompanion.Editor
{
    [InitializeOnLoad]
    public static class CompanionRoomAutoSetup
    {
        private const string ScenePath = "Assets/Scenes/CompanionRoom.unity";

        static CompanionRoomAutoSetup()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }

            if (SceneManager.GetActiveScene().path != ScenePath)
            {
                return;
            }

            if (Object.FindFirstObjectByType<CompanionBehaviorBrain>() != null
                && Object.FindFirstObjectByType<TouchGestureDetector>() != null
                && Object.FindFirstObjectByType<CompanionDebugHud>() != null)
            {
                return;
            }

            CompanionRoomSceneSetup.SetupCompanionRoom();
        }
    }

    public static class CompanionRoomSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/CompanionRoom.unity";

        [MenuItem("Chao Companion/Setup Companion Room")]
        public static void SetupCompanionRoom()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject environment = GetOrCreateRoot("Environment");
            GameObject companion = GetOrCreateRoot("Companion");
            GameObject systems = GetOrCreateRoot("Systems");
            GameObject uiRoot = GetOrCreateRoot("UI");

            GameObject typoGestureSystem = GameObject.Find("GuestureSystem");
            if (typoGestureSystem != null)
            {
                typoGestureSystem.name = "GestureSystem";
            }

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObject = GetOrCreateRoot("Main Camera");
                mainCamera = cameraObject.GetComponent<Camera>() ?? cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            ConfigureCamera(mainCamera);

            GameObject companionVisual = FindChildByName(companion.transform, "CompanionVisual")
                ?? FindChildByName(companion.transform, "Circle")
                ?? CreateDefaultVisual(companion.transform);
            companionVisual.name = "CompanionVisual";
            companionVisual.transform.SetParent(companion.transform, true);
            companion.transform.position = companionVisual.transform.position;
            companionVisual.transform.localPosition = Vector3.zero;
            companionVisual.transform.localScale = new Vector3(2f, 2f, 1f);

            SpriteRenderer spriteRenderer = companionVisual.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.35f, 0.85f, 1f, 1f);
                spriteRenderer.sortingOrder = 10;
            }

            CircleCollider2D targetCollider = companionVisual.GetComponent<CircleCollider2D>()
                ?? companionVisual.AddComponent<CircleCollider2D>();

            GameObject gestureSystem = GetOrCreateChild(systems.transform, "GestureSystem");
            GameObject gameManager = GetOrCreateChild(systems.transform, "GameManager");
            GameObject saveSystemObject = GetOrCreateChild(systems.transform, "SaveSystem");

            TouchGestureDetector gestureDetector = gestureSystem.GetComponent<TouchGestureDetector>()
                ?? gestureSystem.AddComponent<TouchGestureDetector>();
            gestureDetector.SetTarget(targetCollider, mainCamera);

            CompanionNeeds needs = companion.GetComponent<CompanionNeeds>() ?? companion.AddComponent<CompanionNeeds>();

            CompanionMotionController motion = companion.GetComponent<CompanionMotionController>()
                ?? companion.AddComponent<CompanionMotionController>();
            motion.SetVisualRoot(companionVisual.transform);
            motion.SetWorldCamera(mainCamera);

            CompanionBehaviorBrain brain = companion.GetComponent<CompanionBehaviorBrain>()
                ?? companion.AddComponent<CompanionBehaviorBrain>();
            SetObjectReference(brain, "gestureDetector", gestureDetector);
            SetObjectReference(brain, "needs", needs);
            SetObjectReference(brain, "motionController", motion);

            CompanionSaveSystem saveSystem = saveSystemObject.GetComponent<CompanionSaveSystem>()
                ?? saveSystemObject.AddComponent<CompanionSaveSystem>();
            saveSystem.Bind(needs, brain);

            Canvas canvas = ConfigureCanvas(uiRoot);
            GameObject debugPanel = ConfigureDebugPanel(canvas.transform);
            Text moodText = ConfigureText(debugPanel.transform, "MoodText", new Vector2(24f, -24f), "Mood: Calm", 34);
            Text statsText = ConfigureText(debugPanel.transform, "StatsText", new Vector2(24f, -72f), "Stats", 28);
            Text reactionText = ConfigureText(debugPanel.transform, "ReactionText", new Vector2(24f, -250f), "Reaction: Idle", 28);

            CompanionDebugHud hud = gameManager.GetComponent<CompanionDebugHud>() ?? gameManager.AddComponent<CompanionDebugHud>();
            hud.Bind(needs, brain, moodText, statsText, reactionText);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("CompanionRoom scene setup complete.");
        }

        private static void ConfigureCamera(Camera camera)
        {
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.backgroundColor = new Color(0.78f, 0.92f, 1f, 1f);
            camera.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static GameObject CreateDefaultVisual(Transform parent)
        {
            GameObject visual = new("CompanionVisual");
            visual.transform.SetParent(parent, false);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            visual.AddComponent<CircleCollider2D>();
            return visual;
        }

        private static Canvas ConfigureCanvas(GameObject uiRoot)
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new("Canvas");
                canvasObject.transform.SetParent(uiRoot.transform, false);
                canvas = canvasObject.AddComponent<Canvas>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>() ?? canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }

            return canvas;
        }

        private static GameObject ConfigureDebugPanel(Transform canvasTransform)
        {
            GameObject panel = FindChildByName(canvasTransform, "DebugPanel");
            if (panel == null)
            {
                panel = new GameObject("DebugPanel", typeof(RectTransform), typeof(Image));
                panel.transform.SetParent(canvasTransform, false);
            }

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(24f, -24f);
            rect.sizeDelta = new Vector2(420f, 330f);

            Image image = panel.GetComponent<Image>() ?? panel.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.35f);

            return panel;
        }

        private static Text ConfigureText(Transform parent, string name, Vector2 anchoredPosition, string initialText, int fontSize)
        {
            GameObject textObject = FindChildByName(parent, name);
            if (textObject == null)
            {
                textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
                textObject.transform.SetParent(parent, false);
            }

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(370f, 180f);

            Text text = textObject.GetComponent<Text>();
            text.text = initialText;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.raycastTarget = false;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            return text;
        }

        private static GameObject GetOrCreateRoot(string name)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null && existing.transform.parent == null)
            {
                return existing;
            }

            return new GameObject(name);
        }

        private static GameObject GetOrCreateChild(Transform parent, string name)
        {
            GameObject child = FindChildByName(parent, name);
            if (child != null)
            {
                return child;
            }

            child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static GameObject FindChildByName(Transform parent, string name)
        {
            foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}

using System;
using System.IO;
using System.Linq;
using AbyssSurvivor.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AbyssSurvivor.Editor
{
    [InitializeOnLoad]
    internal static class AbyssStartupSceneLoader
    {
        static AbyssStartupSceneLoader()
        {
            EditorApplication.delayCall += OpenDefaultSceneWhenEditorStartsEmpty;
        }

        private static void OpenDefaultSceneWhenEditorStartsEmpty()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(activeScene.path) || activeScene.rootCount > 0)
            {
                return;
            }

            if (!File.Exists(AbyssSceneBuilder.ScenePath))
            {
                return;
            }

            EditorSceneManager.OpenScene(AbyssSceneBuilder.ScenePath, OpenSceneMode.Single);
        }
    }
    public static class AbyssSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/AbyssSurvivor.unity";
        private const string EvidenceDirectory = ".omo/evidence";

        [MenuItem("Abyss Survivor/Build Scene")]
        public static void Build()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "AbyssSurvivor";

            CreateCamera();
            CreateCanvas();
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            WriteEvidence("task-7-scene-build.txt", "built " + ScenePath + " with 16 generated screen roots and build settings entry.");
        }

        public static void BuildAndValidate()
        {
            Build();
            Validate();
        }

        public static void CapturePreview()
        {
            if (SceneManager.GetActiveScene().path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            Camera camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            Require(canvas != null, "Canvas is missing for preview capture.");
            Require(camera != null, "Camera is missing for preview capture.");

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
            Canvas.ForceUpdateCanvases();

            const int width = AbyssDesignTokens.ReferenceWidth;
            const int height = AbyssDesignTokens.ReferenceHeight;
            RenderTexture renderTexture = new RenderTexture(width, height, 24);
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;

            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();

                Directory.CreateDirectory(EvidenceDirectory);
                string path = Path.Combine(EvidenceDirectory, "abyss-survivor-playmode.png");
                File.WriteAllBytes(path, texture.EncodeToPNG());
                WriteEvidence("abyss-survivor-preview.txt", "captured " + path + " from generated Unity scene canvas.");
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(renderTexture);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        public static void Validate()
        {
            if (SceneManager.GetActiveScene().path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            CanvasScaler scaler = UnityEngine.Object.FindFirstObjectByType<CanvasScaler>();
            Require(scaler != null, "CanvasScaler is missing.");
            Require(Mathf.Approximately(scaler.referenceResolution.x, AbyssDesignTokens.ReferenceWidth), "CanvasScaler width is not 390.");
            Require(Mathf.Approximately(scaler.referenceResolution.y, AbyssDesignTokens.ReferenceHeight), "CanvasScaler height is not 844.");

            AbyssMobileApp app = UnityEngine.Object.FindFirstObjectByType<AbyssMobileApp>();
            InputSystemUIInputModule inputModule = UnityEngine.Object.FindFirstObjectByType<InputSystemUIInputModule>();
            Require(inputModule != null, "EventSystem must use InputSystemUIInputModule.");
            Require(UnityEngine.Object.FindFirstObjectByType<StandaloneInputModule>() == null, "EventSystem must not use StandaloneInputModule with new Input System.");
            Require(app != null, "AbyssMobileApp is missing.");

            foreach (AbyssScreenDefinition screen in AbyssScreenMap.Screens)
            {
                GameObject root = FindObjectByName("Screen_" + screen.Id);
                Require(root != null, "Missing screen root " + screen.Id + ".");
                foreach (string label in screen.PrimaryLabels)
                {
                    bool found = root.GetComponentsInChildren<Text>(true).Any(text => text.text.Contains(label));
                    Require(found, "Missing label '" + label + "' on screen " + screen.Id + ".");
                }
            }

            Require(AbyssScreenMap.Screens.Count == 16, "Screen map must contain exactly 16 screens.");
            GameObject titleStartButton = FindObjectByName("Button_Title_게임_시작");
            GameObject titleContinueButton = FindObjectByName("Button_Title_이어하기");
            GameObject titleSettingsButton = FindObjectByName("Button_Title_설정");
            GameObject titleRankButton = FindObjectByName("Button_Title_랭킹");
            GameObject titleActions = FindObjectByName("TitleActions");
            Require(titleStartButton != null, "Title start button is missing.");
            Require(titleContinueButton != null, "Title continue button is missing.");
            Require(titleSettingsButton != null, "Title settings button is missing.");
            Require(titleRankButton != null, "Title rank button is missing.");
            Require(titleActions != null, "Title action block is missing.");
            RequirePreferredHeight(titleStartButton, 56, "Title start button must match Figma 310x56 proportion.");
            RequirePreferredHeight(titleContinueButton, 48, "Title continue button must match Figma 310x48 proportion.");
            RequirePreferredHeight(titleSettingsButton, 44, "Title settings button must match Figma 145x44 proportion.");
            RequirePreferredHeight(titleRankButton, 44, "Title rank button must match Figma 145x44 proportion.");
            RequireTitleActionPadding(titleActions);
            Require(FindObjectByName("Button_Town_던전_입장") != null, "Town dungeon button is missing.");
            Require(FindObjectByName("Button_Battle_기본_공격") != null, "Battle attack button is missing.");

            string summary = "PASS scene=" + ScenePath
                + " reference=" + scaler.referenceResolution.x + "x" + scaler.referenceResolution.y
                + " screens=" + AbyssScreenMap.Screens.Count
                + " buttons=title/town/battle";
            WriteEvidence("task-6-ui-validation.txt", summary);
            WriteEvidence("task-7-scene-validation.txt", summary);
            Debug.Log(summary);
        }

        private static void RequirePreferredHeight(GameObject target, float figmaHeight, string message)
        {
            LayoutElement layout = target.GetComponent<LayoutElement>();
            Require(layout != null, target.name + " layout is missing.");
            float expected = Scale(figmaHeight);
            Require(Mathf.Abs(layout.preferredHeight - expected) <= 1f, message + " expected=" + expected + " actual=" + layout.preferredHeight);
        }

        private static void RequireTitleActionPadding(GameObject target)
        {
            VerticalLayoutGroup layout = target.GetComponent<VerticalLayoutGroup>();
            Require(layout != null, "Title action block layout is missing.");
            Require(layout.padding.left == Scale(22) && layout.padding.right == Scale(22), "Title action block must inset buttons to Figma 310px width.");
        }

        private static int Scale(float value)
        {
            return Mathf.Max(1, Mathf.RoundToInt(value * AbyssDesignTokens.ReferenceWidth / 390f));
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = AbyssDesignTokens.Background;
            camera.orthographic = true;
            cameraObject.tag = "MainCamera";
        }

        private static void CreateCanvas()
        {
            GameObject canvasObject = new GameObject("AbyssCanvas", typeof(RectTransform));
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(AbyssDesignTokens.ReferenceWidth, AbyssDesignTokens.ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject appObject = new GameObject("AbyssMobileApp", typeof(RectTransform));
            appObject.transform.SetParent(canvasObject.transform, false);
            AbyssMobileApp app = appObject.AddComponent<AbyssMobileApp>();
            app.BuildInterface();
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                WriteEvidence("task-6-ui-validation.txt", "FAIL " + message);
                throw new InvalidOperationException(message);
            }
        }

        private static GameObject FindObjectByName(string objectName)
        {
            return Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(entry => entry.name == objectName);
        }

        private static void WriteEvidence(string fileName, string contents)
        {
            Directory.CreateDirectory(EvidenceDirectory);
            File.WriteAllText(Path.Combine(EvidenceDirectory, fileName), contents + Environment.NewLine);
        }
    }
}

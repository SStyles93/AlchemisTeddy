using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
namespace PxP.Tools.ScreenCapture
{
    public class ScreenCapture : EditorWindow
    {
        [SerializeField] private ImageFormat m_imageFormat = ImageFormat.PNG;
        [SerializeField] private TextureFormat m_textureFormat = TextureFormat.RGBAFloat;

        #region Camera
        private Camera m_camera = null;
        private Vector2Int m_customResolution = Vector2Int.one;
        private Resolutions m_resolutions = Resolutions.GameView;
        [SerializeField] private string m_folderName = "Screenshot";
        [SerializeField] private RenderView m_renderView = RenderView.Camera;
        #endregion

        #region Camera Linker
        static CameraLinker m_cameraLinker = null;
        #endregion

        #region Shading
        static ShadingMode m_shadingMode = null;
        #endregion

        #region Enums
        public enum ImageFormat
        {
            EXR,
            JPG,
            PNG,
            TGA
        }
        public enum RenderView
        {
            Camera,
            Scene
        }
        public enum Resolutions
        {
            GameView,
            Custom,
            _3840x2160,
            _1920x1080,
            _1950x1030,
            _420x280,
            _160x160,
        }
        #endregion

        private Vector2 scrollPosition = Vector2.zero;
        private bool m_debugMode = false;

        [MenuItem("PxPTools/Screen Capture")]
        static void Init()
        {
            ScreenCapture window = (ScreenCapture)GetWindow(typeof(ScreenCapture));
            window.Show();
        }

        #region Scene Init
        static void SceneOpenedCallback(Scene _scene, OpenSceneMode _mode)
        {
            // Re-initialize on scene change
            ScreenCapture window = (ScreenCapture)GetWindow(typeof(ScreenCapture));
            if (window != null)
            {
                window.InitializeState();
            }
        }

        private void OnEnable()
        {
            EditorSceneManager.sceneOpened += SceneOpenedCallback;
            EditorInspectorMode.OnInspectorModeChanged += OnInspectorModeChangedHandler;
            InitializeState();
        }

        private void OnDisable()
        {
            EditorSceneManager.sceneOpened -= SceneOpenedCallback;
            EditorInspectorMode.OnInspectorModeChanged -= OnInspectorModeChangedHandler;

            if (m_cameraLinker != null) DestroyImmediate(m_cameraLinker);
            if (m_shadingMode != null) DestroyImmediate(m_shadingMode);
        }

        private void InitializeState()
        {
            m_camera = null;
            m_customResolution = Vector2Int.one;
            m_resolutions = Resolutions.GameView;
            m_folderName = "Screenshot";
            m_renderView = RenderView.Camera;

            if (m_cameraLinker == null) m_cameraLinker = CreateInstance<CameraLinker>();
            m_cameraLinker.Init();

            if (m_shadingMode == null) m_shadingMode = CreateInstance<ShadingMode>();
            m_shadingMode.Init();

            m_debugMode = EditorInspectorMode.GetInspectorModeSafe() == InspectorMode.Debug;

            Repaint();
        }

        private void OnInspectorModeChangedHandler(InspectorMode newMode)
        {
            m_debugMode = (newMode == InspectorMode.Debug);
            if (m_cameraLinker != null) m_cameraLinker.DebugMode = m_debugMode;
            if (m_shadingMode != null) m_shadingMode.DebugMode = m_debugMode;
            Repaint();
        }
        #endregion

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, false);
            GUILayout.BeginVertical();

            // Begin checking for GUI changes to handle Undo/Redo
            EditorGUI.BeginChangeCheck();

            //=============== Rendering params ===================
            #region Rendering
            GUILayout.Label("Render type", EditorStyles.boldLabel);

            var newImageFormat = (ImageFormat)EditorGUILayout.EnumPopup(
                new GUIContent("Image Format", "Changes the extension of the screenshot \n" +
                "(ex: cameraViewCapture.png)"),
                m_imageFormat);

            var newTextureFormat = (TextureFormat)EditorGUILayout.EnumPopup(
                new GUIContent("Texture Format", "Changes the format of the texture used for the ScreenShot Render \n" +
                "(ex: RGBAFloat)"),
                m_textureFormat);

            var newRenderView = (RenderView)EditorGUILayout.EnumPopup(
                new GUIContent("Render View", "Defines what view is used for the ScreenShot\n\n" +
                "[Camera]\n" +
                "Uses the defined Camera in the scene for the render\n\n" +
                "[Scene]\n" +
                "Uses the Scene View for the render"),
                m_renderView);
            #endregion

            if (newRenderView == RenderView.Camera)
            {
                #region Camera Capture
                GUILayout.Space(10);
                if (GUILayout.Button(new GUIContent("Capture Camera View", "Captures the Camera View and saves the Shot in the Folder\n" +
                    "\"Assets/Screenshot/...\"\n\n" +
                    "/!\\ Will create a folder if none already exists /!\\")))
                {
                    CaptureCameraView();
                }

                if (m_debugMode)
                {
                    m_folderName = EditorGUILayout.TextField(new GUIContent("ScreenShot Folder", ""), m_folderName);
                }

                m_cameraLinker.ShowGUI();

                #region Resolution
                GUILayout.Space(10);
                GUILayout.Label("Render Options", EditorStyles.boldLabel);

                var newResolutions = (Resolutions)EditorGUILayout.EnumPopup(
                    new GUIContent("Resolution", "Changes the resolution of the ScreenShots\n\n" +
                    "[Game View]\n" +
                    "Uses the resolution defined next to \"Display N\"\n\n" +
                    "[Custom]\n" +
                    "Enable users to input a custom resolution value for X and Y"),
                    m_resolutions);

                var newCustomResolution = m_customResolution;
                if (newResolutions == Resolutions.Custom)
                {
                    newCustomResolution = EditorGUILayout.Vector2IntField(
                        new GUIContent("Custom Resolution", "Defines the Custom Resolution of the Screenshot you take"),
                        m_customResolution);
                    GUILayout.Space(10);
                }
                #endregion

                m_shadingMode.ShowGUI();
                #endregion

                // Apply changes if any occurred within the camera view section
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, "Screen Capture Settings Changed");
                    m_imageFormat = newImageFormat;
                    m_textureFormat = newTextureFormat;
                    m_renderView = newRenderView;
                    m_resolutions = newResolutions;
                    m_customResolution = newCustomResolution;
                    EditorUtility.SetDirty(this);
                    Repaint();
                }
            }
            else
            {
                // Apply changes if any occurred before switching to scene view section
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, "Screen Capture Settings Changed");
                    m_imageFormat = newImageFormat;
                    m_textureFormat = newTextureFormat;
                    m_renderView = newRenderView;
                    EditorUtility.SetDirty(this);
                    Repaint();
                }

                #region Scene Capture
                GUILayout.Space(10);
                if (GUILayout.Button(new GUIContent("Capture Scene View", "Captures the Scene View and saves the Shot in the Folder\n" +
                    "\"Assets/Screenshot/...\"\n\n" +
                    "/!\\ Will create a folder if none already exists /!\\")))
                {
                    CaptureSceneView();
                }
                #endregion
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void Update()
        {
            if (m_cameraLinker != null)
            {
                m_cameraLinker.Tick();
            }
        }

        #region Capture Methods

        void CaptureCameraView()
        {
            CreateScreenShotFolderIfNoneExists();

            Camera currentCamera = null;
            if (m_cameraLinker.UseCustomCamera)
            {
                if (m_cameraLinker.CustomCamera == null) m_cameraLinker.CustomCamera = Camera.main;
                currentCamera = m_cameraLinker.CustomCamera;
            }
            else
            {
                if (m_camera == null) m_camera = Camera.main;
                currentCamera = m_camera;
            }

            uint width = 0;
            uint height = 0;
            switch (m_resolutions)
            {
                case Resolutions.GameView:
                    UnityEditor.PlayModeWindow.GetRenderingResolution(out width, out height);
                    break;
                case Resolutions.Custom:
                    width = (uint)Mathf.Abs(m_customResolution.x);
                    height = (uint)Mathf.Abs(m_customResolution.y);
                    break;
                case Resolutions._3840x2160: width = 3840; height = 2160; break;
                case Resolutions._1950x1030: width = 1950; height = 1030; break;
                case Resolutions._1920x1080: width = 1920; height = 1080; break;
                case Resolutions._420x280: width = 420; height = 280; break;
                case Resolutions._160x160: width = 160; height = 160; break;
                default:
                    Debug.Log($"The resolution used for the screenshot \"{width}x{height}\" was not set conventionally, this is not supposed to happen");
                    break;
            }

            RenderTexture renderTexture = new RenderTexture((int)width, (int)height, 24);
            currentCamera.targetTexture = renderTexture;

            Texture2D capture = new Texture2D((int)width, (int)height, m_textureFormat, false);

            currentCamera.Render();
            RenderTexture.active = renderTexture;
            capture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            capture.Apply();

            currentCamera.targetTexture = null;
            RenderTexture.active = null;

            DestroyImmediate(renderTexture);

            string fileName = $"{SceneManager.GetActiveScene().name}_{width}x{height}";
            int screenshotCount = 0;
            string formatSuffix = "";

            byte[] bytes = null;
            switch (m_imageFormat)
            {
                case ImageFormat.EXR:
                    bytes = capture.EncodeToEXR();
                    formatSuffix = ".exr";
                    break;
                case ImageFormat.JPG:
                    bytes = capture.EncodeToJPG();
                    formatSuffix = ".jpg";
                    break;
                case ImageFormat.PNG:
                    bytes = capture.EncodeToPNG();
                    formatSuffix = ".png";
                    break;
                case ImageFormat.TGA:
                    bytes = capture.EncodeToTGA();
                    formatSuffix = ".tga";
                    break;
            }
            screenshotCount = GetScreenShotCountByType(fileName, formatSuffix);

            File.WriteAllBytes(Application.dataPath + "/" + m_folderName + "/" + fileName + "_" + screenshotCount + formatSuffix, bytes);
            AssetDatabase.Refresh();
        }

        void CaptureSceneView()
        {
            CreateScreenShotFolderIfNoneExists();
            SceneView sceneView = SceneView.lastActiveSceneView;
            int width = sceneView.camera.pixelWidth;
            int height = sceneView.camera.pixelHeight;
            Texture2D capture = new Texture2D(width, height, m_textureFormat, false);
            sceneView.camera.Render();
            RenderTexture.active = sceneView.camera.targetTexture;
            capture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            capture.Apply();
            string fileName = $"(SCENE){SceneManager.GetActiveScene().name}_{width}x{height}";
            int screenshotCount = 0;
            string formatSuffix = "";
            byte[] bytes = null;
            switch (m_imageFormat)
            {
                case ImageFormat.EXR:
                    bytes = capture.EncodeToEXR();
                    formatSuffix = ".exr";
                    break;
                case ImageFormat.JPG:
                    bytes = capture.EncodeToJPG();
                    formatSuffix = ".jpg";
                    break;
                case ImageFormat.PNG:
                    bytes = capture.EncodeToPNG();
                    formatSuffix = ".png";
                    break;
                case ImageFormat.TGA:
                    bytes = capture.EncodeToTGA();
                    formatSuffix = ".tga";
                    break;
            }
            screenshotCount = GetScreenShotCountByType(fileName, formatSuffix);
            File.WriteAllBytes(Application.dataPath + "/" + m_folderName + "/" + fileName + "_" + screenshotCount + formatSuffix, bytes);
            AssetDatabase.Refresh();
        }

        void CreateScreenShotFolderIfNoneExists()
        {
            if (!Directory.Exists(Application.dataPath + "/" + m_folderName))
                AssetDatabase.CreateFolder("Assets", m_folderName);
        }

        int GetScreenShotCountByType(string fileName, string formatSuffix)
        {
            return Directory.GetFiles(Application.dataPath + "/" + m_folderName, fileName + "*" + formatSuffix, SearchOption.AllDirectories).Length;
        }

        #endregion
    }
}
#endif

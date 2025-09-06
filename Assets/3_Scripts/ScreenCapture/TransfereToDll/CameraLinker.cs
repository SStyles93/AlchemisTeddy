using UnityEditor;
using UnityEngine;

namespace PxP.Tools.ScreenCapture
{
    public class CameraLinker : EditorWindow
    {
        private Camera m_camera = null;
        [SerializeField] private bool m_linkCameraToSceneView = false;
        private SceneView m_sceneCam = null;
        [SerializeField] private Camera m_customCamera = null;
        [SerializeField] private bool m_useCustomCamera = false;

        private bool m_debugMode = false;

        public Camera CustomCamera { get => m_customCamera; set => m_customCamera = value; }
        public bool UseCustomCamera { get => m_useCustomCamera; }
        public bool DebugMode { get => m_debugMode; set => m_debugMode = value; }

        public void Init()
        {
            m_linkCameraToSceneView = false;
            m_sceneCam = null;
            m_customCamera = null;
            m_useCustomCamera = false;
        }

        public void ShowGUI()
        {
            #region Camera Linker
            GUILayout.Space(10);
            GUILayout.Label("Camera Options", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            if (m_linkCameraToSceneView) GUI.color = Color.yellow;

            var newLinkCamera = EditorGUILayout.Toggle(
                new GUIContent("Link Camera", "Links the selected camera to the scene view movements."),
                m_linkCameraToSceneView);
            GUI.color = Color.white;

            var newUseCustomCamera = EditorGUILayout.Toggle(
                new GUIContent("Custom Camera", "Use a specific camera instead of the main camera."),
                m_useCustomCamera);

            Camera newCustomCamera = m_customCamera;
            if (newUseCustomCamera)
            {
                newCustomCamera = (Camera)EditorGUILayout.ObjectField(
                    new GUIContent("Camera", "The camera to use for the screenshot. Falls back to Main Camera if none."),
                    m_customCamera, typeof(Camera), true);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, "Camera Linker Changed");
                m_linkCameraToSceneView = newLinkCamera;
                m_useCustomCamera = newUseCustomCamera;
                m_customCamera = newCustomCamera;
                EditorUtility.SetDirty(this);
                Repaint();
            }

            if (m_debugMode)
            {
                EditorGUILayout.ObjectField("Main Camera", m_camera, typeof(Camera), true);
                EditorGUILayout.ObjectField("Custom Camera", m_customCamera, typeof(Camera), true);
                EditorGUILayout.ObjectField("Scene View", m_sceneCam, typeof(SceneView), true);
            }
            #endregion
        }

        public void Tick()
        {
            if (m_linkCameraToSceneView)
            {
                if (m_camera == null) m_camera = Camera.main;

                if (focusedWindow != null && focusedWindow.titleContent.text == "Inspector")
                {
                    Camera camToAlign = m_useCustomCamera && m_customCamera != null ? m_customCamera : m_camera;
                    if (camToAlign != null)
                    {
                        SceneView.lastActiveSceneView.AlignViewToObject(camToAlign.transform);
                    }
                }
                else
                {
                    LinkCameraToSceneView(m_useCustomCamera ? m_customCamera : m_camera);
                }
            }
        }

        void LinkCameraToSceneView(Camera camera)
        {
            if (camera == null) return;
            m_sceneCam = SceneView.lastActiveSceneView;
            if (m_sceneCam != null)
            {
                camera.transform.position = m_sceneCam.camera.transform.position;
                camera.transform.rotation = m_sceneCam.camera.transform.rotation;
            }
        }
    }
}

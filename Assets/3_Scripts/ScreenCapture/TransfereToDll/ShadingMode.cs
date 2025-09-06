using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PxP.Tools.ScreenCapture
{
    public class ShadingMode : EditorWindow
    {
        [SerializeField] public ShadingStyle m_shadingMode = ShadingStyle.Shaded;
        private ShadingStyle m_currentShadingMode = ShadingStyle.Shaded;
        private Material m_wireframeMaterial = null;

        public MeshRenderer[] m_meshRenderers = null;
        public Material[][] m_savedMaterials = null;

        private bool m_debugMode = false;
        private bool m_showMaterialsFoldout = false;

        public bool DebugMode { get => m_debugMode; set => m_debugMode = value; }

        public enum ShadingStyle
        {
            Shaded,
            Wireframe,
            ShadedWireframe,
            LitWireframe
        }

        public void Init()
        {
            m_shadingMode = ShadingStyle.Shaded;
            m_currentShadingMode = ShadingStyle.Shaded;
            m_wireframeMaterial = null;
            m_meshRenderers = null;
            m_savedMaterials = null;
        }

        public void ShowGUI()
        {
            EditorGUI.BeginChangeCheck();
            var newShadingMode = (ShadingStyle)EditorGUILayout.EnumPopup(
                new GUIContent("Shading Mode", "Changes the Shading model of the Game View..."),
                m_shadingMode);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, "Shading Mode Changed");
                m_shadingMode = newShadingMode;
                EditorUtility.SetDirty(this);
                Repaint();
            }

            if (m_shadingMode != m_currentShadingMode)
            {
                SwitchShadingMode();
            }

            if (m_debugMode)
            {
                EditorGUILayout.EnumPopup("Current Shading Mode", m_currentShadingMode);
                EditorGUILayout.ObjectField("Wireframe Material", m_wireframeMaterial, typeof(Material), true);

                SerializedObject so = new SerializedObject(this);
                SerializedProperty property = so.FindProperty("m_meshRenderers");
                EditorGUILayout.PropertyField(property, true);

                if (m_savedMaterials != null)
                {
                    m_showMaterialsFoldout = EditorGUILayout.Foldout(m_showMaterialsFoldout, "Saved Materials");
                    if (m_showMaterialsFoldout)
                    {
                        EditorGUI.indentLevel++;
                        for (int i = 0; i < m_savedMaterials.Length; i++)
                        {
                            if (m_meshRenderers[i] == null) continue;
                            EditorGUILayout.LabelField($"{m_meshRenderers[i].name}:");
                            for (int j = 0; j < m_savedMaterials[i].Length; j++)
                            {
                                EditorGUILayout.ObjectField($"Material {j}", m_savedMaterials[i][j], typeof(Material), true);
                            }
                            GUILayout.Space(5);
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    GUILayout.Label("There are currently no saved materials");
                }
            }
            GUILayout.Space(10);
        }

        #region Shading
        void SwitchShadingMode()
        {
            if (m_wireframeMaterial == null)
                m_wireframeMaterial = GetMaterialByName("Wireframe");

            switch (m_shadingMode)
            {
                case ShadingStyle.Shaded:
                    if (m_currentShadingMode == ShadingStyle.ShadedWireframe || m_currentShadingMode == ShadingStyle.LitWireframe)
                        RemoveLastMaterialOnAllObjects();
                    ShadedMode();
                    break;

                case ShadingStyle.Wireframe:
                    if (m_currentShadingMode == ShadingStyle.Shaded)
                        SaveMaterials();
                    if (m_currentShadingMode == ShadingStyle.ShadedWireframe || m_currentShadingMode == ShadingStyle.LitWireframe)
                        RemoveLastMaterialOnAllObjects();
                    WireframeMode();
                    break;

                case ShadingStyle.ShadedWireframe:
                    if (m_currentShadingMode == ShadingStyle.Shaded)
                        SaveMaterials();
                    if (m_currentShadingMode == ShadingStyle.LitWireframe)
                        RemoveLastMaterialOnAllObjects();
                    ShadedWireframeMode();
                    break;

                case ShadingStyle.LitWireframe:
                    if (m_currentShadingMode == ShadingStyle.Shaded)
                        SaveMaterials();
                    if (m_currentShadingMode == ShadingStyle.ShadedWireframe)
                        RemoveLastMaterialOnAllObjects();
                    LitWireframeMode();
                    break;
            }
            m_currentShadingMode = m_shadingMode;
        }

        void ShadedMode()
        {
            if (m_savedMaterials == null) return;
            m_meshRenderers = FindRenderedObjects(true);
            for (int i = 0; i < m_meshRenderers.Length; i++)
            {
                if (i < m_savedMaterials.Length)
                {
                    m_meshRenderers[i].sharedMaterials = m_savedMaterials[i];
                    if (m_currentShadingMode == ShadingStyle.LitWireframe || m_currentShadingMode == ShadingStyle.ShadedWireframe)
                    {
                        RemoveCombinedMesh(m_meshRenderers[i].gameObject);
                    }
                }
            }
        }

        void WireframeMode()
        {
            ShadedMode();
            for (int i = 0; i < m_meshRenderers.Length; i++)
            {
                Material[] tmpMats = new Material[m_meshRenderers[i].sharedMaterials.Length];
                for (int matIndex = 0; matIndex < tmpMats.Length; matIndex++)
                {
                    tmpMats[matIndex] = m_wireframeMaterial;
                }
                m_meshRenderers[i].sharedMaterials = tmpMats;
            }
        }

        void ShadedWireframeMode()
        {
            ShadedMode();
            for (int i = 0; i < m_meshRenderers.Length; i++)
            {
                foreach (Material mat in m_meshRenderers[i].sharedMaterials)
                {
                    if (mat.name.Contains("Wireframe")) goto endloop;
                }
                Material[] matsCopy = m_meshRenderers[i].sharedMaterials;
                Material[] tmpMats = new Material[matsCopy.Length + 1];
                matsCopy.CopyTo(tmpMats, 0);
                tmpMats[matsCopy.Length] = m_wireframeMaterial;
                m_meshRenderers[i].sharedMaterials = tmpMats;
                AddCombinedMesh(m_meshRenderers[i].gameObject);
            endloop:;
            }
        }

        void LitWireframeMode()
        {
            ShadedMode();
            for (int i = 0; i < m_meshRenderers.Length; i++)
            {
                foreach (Material mat in m_meshRenderers[i].sharedMaterials)
                {
                    if (mat.name.Contains("Wireframe")) goto endloop;
                }
                Material[] matsCopy = m_meshRenderers[i].sharedMaterials;
                Material[] tmpMats = new Material[matsCopy.Length + 1];
                Material litMaterial = GetMaterialByName();
                for (int matIndex = 0; matIndex < matsCopy.Length; matIndex++)
                {
                    tmpMats[matIndex] = litMaterial;
                }
                tmpMats[matsCopy.Length] = m_wireframeMaterial;
                m_meshRenderers[i].sharedMaterials = tmpMats;
                AddCombinedMesh(m_meshRenderers[i].gameObject);
            endloop:;
            }
        }
        #endregion

        private void SaveMaterials()
        {
            m_meshRenderers = FindRenderedObjects(true);
            m_savedMaterials = new Material[m_meshRenderers.Length][];
            for (int i = 0; i < m_meshRenderers.Length; i++)
            {
                m_savedMaterials[i] = new Material[m_meshRenderers[i].sharedMaterials.Length];
                for (int j = 0; j < m_meshRenderers[i].sharedMaterials.Length; j++)
                {
                    m_savedMaterials[i][j] = m_meshRenderers[i].sharedMaterials[j];
                }
            }
        }

        private void AddCombinedMesh(GameObject gameObject)
        {
            SetMeshToReadWrite(gameObject, true);
            Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            if (mesh != null)
            {
                mesh.subMeshCount++;
                mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
            }
        }

        private MeshRenderer[] FindRenderedObjects(bool excludePlane = false)
        {
            var renderedObjectsList = new System.Collections.Generic.List<MeshRenderer>(FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID));
            if (excludePlane)
            {
                renderedObjectsList.RemoveAll(item => item.name == "Plane");
            }
            return renderedObjectsList.ToArray();
        }

        private Material GetMaterialByName(string materialName = "")
        {
            if (string.IsNullOrEmpty(materialName))
            {
                return AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Lit.mat");
            }
            string[] guids = AssetDatabase.FindAssets(materialName + " t:Material");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<Material>(path);
            }
            Debug.LogWarning("Material " + materialName + " was not found. Falling back to default Lit material.");
            return AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Lit.mat");
        }

        private ModelImporter GetModelImporter(Mesh sharedMesh)
        {
            if (sharedMesh == null) return null;
            string path = AssetDatabase.GetAssetPath(sharedMesh);
            if (string.IsNullOrEmpty(path)) return null;
            return AssetImporter.GetAtPath(path) as ModelImporter;
        }

        private void RemoveCombinedMesh(GameObject gameObject)
        {
            Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            if (mesh != null && mesh.subMeshCount > 1)
            {
                mesh.subMeshCount--;
            }
            SetMeshToReadWrite(gameObject, false);
        }

        private void RemoveLastMaterialOnAllObjects()
        {
            MeshRenderer[] renderers = FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
            foreach (var mr in renderers)
            {
                if (mr.name == "Plane") continue;
                if (mr.sharedMaterials.Length > 1)
                {
                    var materials = new System.Collections.Generic.List<Material>(mr.sharedMaterials);
                    materials.RemoveAt(materials.Count - 1);
                    mr.sharedMaterials = materials.ToArray();
                }
            }
        }

        private void SetMeshToReadWrite(GameObject gameObject, bool state)
        {
            MeshFilter mf = gameObject.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) return;
            ModelImporter importer = GetModelImporter(mf.sharedMesh);
            if (importer != null && importer.isReadable != state)
            {
                importer.isReadable = state;
                importer.SaveAndReimport();
            }
        }
    }
}

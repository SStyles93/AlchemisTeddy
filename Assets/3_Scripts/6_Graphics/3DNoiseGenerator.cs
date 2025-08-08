using UnityEditor;
using UnityEngine;

public class NoiseTextureGeneratorWindow : EditorWindow
{
    // Texture settings
    private int size = 64;
    private float noiseScale = 4.0f;
    private TextureFormat format = TextureFormat.RGBA32;
    private TextureWrapMode wrapMode = TextureWrapMode.Repeat;

    // Noise remapping settings
    private float brightness = 1.0f;
    private float contrast = 2.0f; // A value of 2.0 is a good starting point for higher contrast

    // Asset saving settings
    private string savePath = "Assets/2_Art/Materials/Fog";
    private string fileName = "3D_Colored_Noise_Texture";

    // Editor window state
    private Vector2 scrollPosition;

    [MenuItem("Tools/Noise Texture Generator")]
    public static void ShowWindow()
    {
        GetWindow<NoiseTextureGeneratorWindow>("Noise Generator");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("3D Colored Noise Texture Generator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Create a 3D noise texture with controls for brightness and contrast to avoid dark or muddy results.", MessageType.Info);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // --- Texture Configuration ---
        EditorGUILayout.LabelField("Texture Settings", EditorStyles.boldLabel);
        size = EditorGUILayout.IntSlider("Size (Resolution)", size, 16, 256);
        noiseScale = EditorGUILayout.FloatField("Noise Scale (Frequency)", noiseScale);
        format = (TextureFormat)EditorGUILayout.EnumPopup("Texture Format", format);
        EditorGUILayout.HelpBox("For colored noise, use a color format like RGBA32 or RGB24.", MessageType.None);
        wrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup("Wrap Mode", wrapMode);

        EditorGUILayout.Space(10);

        // --- Remapping Controls ---
        EditorGUILayout.LabelField("Remapping Settings", EditorStyles.boldLabel);
        brightness = EditorGUILayout.Slider("Brightness", brightness, 0.1f, 5.0f);
        EditorGUILayout.HelpBox("Multiplies the final noise value. >1 makes it brighter, <1 makes it darker.", MessageType.None);

        contrast = EditorGUILayout.Slider("Contrast (Power)", contrast, 0.1f, 5.0f);
        EditorGUILayout.HelpBox("Applies a power function to the noise. >1 increases contrast (more black & white), <1 reduces it.", MessageType.None);

        EditorGUILayout.Space(10);

        // --- File Saving ---
        EditorGUILayout.LabelField("Output Settings", EditorStyles.boldLabel);
        savePath = EditorGUILayout.TextField("Save Folder", savePath);
        fileName = EditorGUILayout.TextField("File Name", fileName);

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Generate 3D Colored Noise", GUILayout.Height(40)))
        {
            GenerateNoiseTexture();
        }

        EditorGUILayout.EndScrollView();
    }

    private void GenerateNoiseTexture()
    {
        if (string.IsNullOrEmpty(savePath) || string.IsNullOrEmpty(fileName))
        {
            EditorUtility.DisplayDialog("Error", "Save Path and File Name cannot be empty.", "OK");
            return;
        }

        if (!AssetDatabase.IsValidFolder(savePath))
        {
            string parentFolder = System.IO.Path.GetDirectoryName(savePath);
            string newFolderName = System.IO.Path.GetFileName(savePath);
            if (!string.IsNullOrEmpty(parentFolder) && !string.IsNullOrEmpty(newFolderName))
            {
                AssetDatabase.CreateFolder(parentFolder, newFolderName);
            }
        }

        Texture3D texture = new Texture3D(size, size, size, format, false);
        texture.wrapMode = wrapMode;

        Color[] colors = new Color[size * size * size];
        float inverseSize = 1.0f / size;

        for (int z = 0; z < size; z++)
        {
            int zOffset = z * size * size;
            for (int y = 0; y < size; y++)
            {
                int yOffset = y * size;
                for (int x = 0; x < size; x++)
                {
                    float xCoord = (float)x * inverseSize * noiseScale;
                    float yCoord = (float)y * inverseSize * noiseScale;
                    float zCoord = (float)z * inverseSize * noiseScale;

                    // Generate 3 independent noise values for R, G, and B channels
                    float noiseR = SampleNoise(xCoord, yCoord, zCoord);
                    float noiseG = SampleNoise(xCoord + 100f, yCoord + 100f, zCoord + 100f);
                    float noiseB = SampleNoise(xCoord - 100f, yCoord - 100f, zCoord - 100f);

                    // --- Remap the noise to adjust brightness and contrast ---
                    noiseR = Mathf.Pow(noiseR, contrast) * brightness;
                    noiseG = Mathf.Pow(noiseG, contrast) * brightness;
                    noiseB = Mathf.Pow(noiseB, contrast) * brightness;

                    // Clamp the values to ensure they are in the [0, 1] range for colors
                    colors[x + yOffset + zOffset] = new Color(
                        Mathf.Clamp01(noiseR),
                        Mathf.Clamp01(noiseG),
                        Mathf.Clamp01(noiseB),
                        1.0f
                    );
                }
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        string fullPath = $"{savePath}/{fileName}.asset";
        AssetDatabase.CreateAsset(texture, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = texture;

        Debug.Log($"3D Colored Noise Texture created and saved to {fullPath}");
        EditorUtility.DisplayDialog("Success", $"3D Colored Noise Texture saved to:\n{fullPath}", "OK");
    }

    /// <summary>
    /// Samples 3D noise by averaging 3 samples of Unity's 2D PerlinNoise function from different planes.
    /// </summary>
    private float SampleNoise(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float yz = Mathf.PerlinNoise(y, z);
        float xz = Mathf.PerlinNoise(x, z);
        return (xy + yz + xz) / 3.0f;
    }
}

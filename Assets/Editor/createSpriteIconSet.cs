using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureAtlasGenerator : EditorWindow
{
    public Texture2D[] textures;
    public int atlasWidth = 1024;
    public int atlasHeight = 1024;
    public string outputFileName = "icons-Combined";

    [MenuItem("Tools/Texture Atlas Generator")]
    public static void ShowWindow()
    {
        GetWindow<TextureAtlasGenerator>("Texture Atlas Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Combine Textures into Atlas", EditorStyles.boldLabel);

        SerializedObject so = new SerializedObject(this);
        SerializedProperty texturesProp = so.FindProperty("textures");
        EditorGUILayout.PropertyField(texturesProp, true);
        so.ApplyModifiedProperties();

        atlasWidth = EditorGUILayout.IntField("Atlas Width", atlasWidth);
        atlasHeight = EditorGUILayout.IntField("Atlas Height", atlasHeight);
        outputFileName = EditorGUILayout.TextField("Output File Name", outputFileName);

        if (GUILayout.Button("Create Atlas"))
        {
            CreateAtlas();
        }
    }

    private void CreateAtlas()
    {
        if (textures == null || textures.Length == 0)
        {
            Debug.LogError("No textures assigned.");
            return;
        }

        Texture2D atlas = new Texture2D(atlasWidth, atlasHeight);
        Color[] fillColor = new Color[atlasWidth * atlasHeight];
        for (int i = 0; i < fillColor.Length; i++) fillColor[i] = Color.clear;
        atlas.SetPixels(fillColor);

        int x = 0, y = 0, maxHeight = 0;

        foreach (var tex in textures)
        {
            if (tex == null) continue;

            if (x + tex.width > atlasWidth)
            {
                x = 0;
                y += maxHeight;
                maxHeight = 0;
            }

            if (y + tex.height > atlasHeight)
            {
                Debug.LogError("Atlas is too small to fit all textures.");
                return;
            }

            var pixels = tex.GetPixels();
            atlas.SetPixels(x, y, tex.width, tex.height, pixels);
            x += tex.width;
            maxHeight = Mathf.Max(maxHeight, tex.height);
        }

        atlas.Apply();

        byte[] pngData = atlas.EncodeToPNG();
        string path = $"Assets/{outputFileName}.png";
        File.WriteAllBytes(path, pngData);
        AssetDatabase.Refresh();

        Debug.Log($"Atlas saved to {path}");
    }
}

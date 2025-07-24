using UnityEngine;
using UnityEditor;
using System.Xml;
using System.Collections.Generic;
using System.IO;

public class SpriteSheetSlicer : EditorWindow
{
    Texture2D spriteSheet;
    TextAsset xmlFile;

    [MenuItem("Tools/Slice Sprite Sheet With XML")]
    static void Init()
    {
        SpriteSheetSlicer window = (SpriteSheetSlicer)EditorWindow.GetWindow(typeof(SpriteSheetSlicer));
        window.titleContent = new GUIContent("Sprite Slicer");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Slice Sprite Sheet Using XML", EditorStyles.boldLabel);
        spriteSheet = (Texture2D)EditorGUILayout.ObjectField("Sprite Sheet", spriteSheet, typeof(Texture2D), false);
        xmlFile = (TextAsset)EditorGUILayout.ObjectField("XML File", xmlFile, typeof(TextAsset), false);

        if (GUILayout.Button("Slice"))
        {
            if (spriteSheet && xmlFile)
            {
                Slice();
            }
            else
            {
                Debug.LogWarning("Assign both sprite sheet and XML file.");
            }
        }
    }

void Slice()
{
    string path = AssetDatabase.GetAssetPath(spriteSheet);
    TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

    ti.textureType = TextureImporterType.Sprite;
    ti.spriteImportMode = SpriteImportMode.Multiple;
    ti.isReadable = true;
    ti.maxTextureSize = 8192;
    ti.textureCompression = TextureImporterCompression.Uncompressed;

    List<SpriteMetaData> newData = new List<SpriteMetaData>();

    XmlDocument doc = new XmlDocument();
    doc.LoadXml(xmlFile.text);
    XmlNodeList nodes = doc.GetElementsByTagName("SubTexture");

    int texHeight = spriteSheet.height;

    foreach (XmlNode node in nodes)
    {
        SpriteMetaData smd = new SpriteMetaData();
        smd.name = node.Attributes["name"].Value;

        int x = int.Parse(node.Attributes["x"].Value);
        int y = int.Parse(node.Attributes["y"].Value);
        int w = int.Parse(node.Attributes["width"].Value);
        int h = int.Parse(node.Attributes["height"].Value);

        // Clamp values to prevent overshooting
        int clampedX = Mathf.Clamp(x, 0, spriteSheet.width);
        int clampedY = Mathf.Clamp(y, 0, spriteSheet.height);
        int clampedW = Mathf.Clamp(w, 0, spriteSheet.width - clampedX);
        int clampedH = Mathf.Clamp(h, 0, spriteSheet.height - clampedY);

        smd.rect = new Rect(clampedX, texHeight - clampedY - clampedH, clampedW, clampedH);
        smd.pivot = new Vector2(0.5f, 0.5f);
        smd.alignment = 9;

        newData.Add(smd);
    }

    ti.spritesheet = newData.ToArray();
    EditorUtility.SetDirty(ti);
    ti.SaveAndReimport();

    Debug.Log("✅ Sprite sheet sliced successfully!");
}

}

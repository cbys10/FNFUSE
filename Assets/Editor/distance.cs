using UnityEngine;
using UnityEditor;

public class Distance : EditorWindow
{
    Transform other;
    Transform main;

    [MenuItem("Tools/Distance Tool")]
    static void Init()
    {
        Distance window = (Distance)EditorWindow.GetWindow(typeof(Distance));
        window.titleContent = new GUIContent("Distance Tool");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Distance between two objects", EditorStyles.boldLabel);
        other = (Transform)EditorGUILayout.ObjectField("Object A", other, typeof(Transform), true);
        main = (Transform)EditorGUILayout.ObjectField("Object B", main, typeof(Transform), true);

        if (GUILayout.Button("Check"))
        {
            if (other && main)
            {
                float distance = Vector3.Distance(other.position, main.position);
                Debug.Log($"Distance between '{other.name}' and '{main.name}': {distance}");
            }
            else
            {
                Debug.LogWarning("Assign both objects to check distance.");
            }
        }
    }
}

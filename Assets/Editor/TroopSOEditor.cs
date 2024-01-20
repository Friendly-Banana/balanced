using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TroopSO))]
[CanEditMultipleObjects]
public class TroopSOEditor : Editor
{
    SerializedProperty prefab;
    SerializedProperty range;

    void OnEnable()
    {
        prefab = serializedObject.FindProperty("prefab");
        range = serializedObject.FindProperty("range");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (prefab == null)
            return;

        if (GUILayout.Button("Rename prefab"))
        {
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(prefab.objectReferenceValue), serializedObject.targetObject.name);
            EditorApplication.delayCall += AssetDatabase.Refresh;
        }

        var p = (GameObject)prefab.objectReferenceValue;
        if (p.GetComponent<Ranged>() != null)
        {
            EditorGUILayout.LabelField("Ranged");
        }
        else if (p.GetComponent<Melee>() != null)
            EditorGUILayout.LabelField("Melee");
    }
}
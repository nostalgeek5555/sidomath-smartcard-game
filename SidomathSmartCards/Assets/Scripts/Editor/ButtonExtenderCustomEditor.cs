using UnityEditor;


[CustomEditor(typeof(ButtonExtender)), CanEditMultipleObjects]
public class ButtonExtenderCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        serializedObject.ApplyModifiedProperties();
    }
}

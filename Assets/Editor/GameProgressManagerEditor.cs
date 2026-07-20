using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameProgressManager))]
public class GameProgressManagerEditor : Editor
{
    private SerializedProperty initialStateProperty;

    private void OnEnable()
    {
        initialStateProperty = serializedObject.FindProperty("initialState");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(initialStateProperty);

        using (new EditorGUI.DisabledScope(true))
        {
            string currentState = Application.isPlaying
                ? ((GameProgressManager)target).CurrentState.ToString()
                : "Play Mode에서 확인 가능";

            EditorGUILayout.TextField("Current State", currentState);
        }

        serializedObject.ApplyModifiedProperties();
    }

    public override bool RequiresConstantRepaint()
    {
        return Application.isPlaying;
    }
}

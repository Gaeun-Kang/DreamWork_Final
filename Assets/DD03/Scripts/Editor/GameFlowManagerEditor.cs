using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameFlowManager))]
public class GameFlowManagerEditor : Editor
{
    private GameState _selectedState;

    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();

        GameFlowManager manager = (GameFlowManager)target;

        if (Application.isPlaying)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

            // Show current state
            EditorGUILayout.LabelField("Current State:", manager.GameState.ToString());

            // Enum popup for selecting target state
            _selectedState = (GameState)EditorGUILayout.EnumPopup("Target State:", _selectedState);

            // Button to change
            if (GUILayout.Button($"Change State to {_selectedState}"))
            {
                manager.ChangeState(_selectedState);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to control game state.", MessageType.Info);
        }
    }
}
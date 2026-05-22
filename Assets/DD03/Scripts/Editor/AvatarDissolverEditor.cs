using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AvatarDissolver))]
public class AvatarDissolverEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();

        AvatarDissolver dissolver = (AvatarDissolver)target;

        if (Application.isPlaying)
        {
            if (GUILayout.Button($"Dissolve"))
            {
                //dissolver.Play();
            }
        }
    }
}

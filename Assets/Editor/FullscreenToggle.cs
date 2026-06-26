
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

public static class FullscreenToggle
{
    [MenuItem("Tools/Toggle Fullscreen Game View %F11")] // Ctrl+F11
    public static void ToggleFullscreen()
    {
        var gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
        var window = EditorWindow.GetWindow(gameViewType);

        // Game View를 전체화면으로 전환
        var fullscreenField = gameViewType.GetField(
            "m_Fullscreen",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        if (fullscreenField != null)
        {
            bool current = (bool)fullscreenField.GetValue(window);
            fullscreenField.SetValue(window, !current);
            window.Repaint();
        }
    }
}
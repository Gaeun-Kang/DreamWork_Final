using UnityEngine;

public class FullscreenManager : MonoBehaviour
{
    void Start()
    {
        // Exclusive Fullscreen으로 설정
        Screen.SetResolution(
            Display.main.systemWidth,
            Display.main.systemHeight,
            FullScreenMode.ExclusiveFullScreen
        );
    }
}
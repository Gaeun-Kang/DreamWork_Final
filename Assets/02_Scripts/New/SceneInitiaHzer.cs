using UnityEngine;



public class SceneInitiaHzer : MonoBehaviour
{
    [SerializeField] private DD05SkyboxController skyboxController;

    private void Start()
    {
        if (skyboxController == null) return;
       
        LevelTransition.ApplyPendingImageSet(skyboxController);
    }
}

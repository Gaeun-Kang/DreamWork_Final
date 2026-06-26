using UnityEngine;
using UnityEngine.SceneManagement;

public class SkyBoxManager : MonoBehaviour
{
    /*DD03에서 선택한 material ID 저장
      저장한 ID 기반으로 Scene을 이동시켜주는 역할 
     */

  
    public static SkyBoxManager Instance { get; private set; }
    public string CurrentSkyboxId { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(Instance);
        }
    }

    public void LoadToSkyboxScene(string objectName)
    {
        if(Instance == null)
        {
            SceneManager.LoadScene(objectName);
        }
    }
}

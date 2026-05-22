using System.Collections.Generic;
using UnityEngine;

public class DreamCanvasController : MonoBehaviour
{
    [SerializeField] private MeshRenderer m_SelfPortalMesh;
    [SerializeField] private List<Material> m_SelfPortalMaterials;

    public void OnGlobe1Selected()
    {
        m_SelfPortalMesh.material = m_SelfPortalMaterials[0];
    }
    
    public void OnGlobe2Selected()
    {
        m_SelfPortalMesh.material = m_SelfPortalMaterials[1];
    }

    public void OnGlobe3Selected()
    {
        m_SelfPortalMesh.material = m_SelfPortalMaterials[2];
    }

    public void OnGlobe4Selected()
    {
        m_SelfPortalMesh.material = m_SelfPortalMaterials[3];
    }

    public void OnGlobe5Selected()
    {
        m_SelfPortalMesh.material = m_SelfPortalMaterials[4];
    }

}

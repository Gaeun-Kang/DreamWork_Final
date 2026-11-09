using System.Collections;
using UnityEngine;

namespace InteractiveRaymarcher
{
    public class TextureCycler : MonoBehaviour
    {
        [SerializeField] private Texture2D[] _textures;

        [SerializeField] private float _changeInterval = 5f;

        private Material _material;
        private int _currentIndex = 0;

        void Start()
        {
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                _material = rend.material;
            }
            else
            {
                Debug.LogError("No Renderer found on this object!");
                return;
            }

            if (_textures != null && _textures.Length > 0)
            {
                _material.SetTexture("_MainTex", _textures[0]);
                StartCoroutine(CycleTexturesRoutine());
            }
            else
            {
                Debug.LogWarning("TextureCycler: No textures added in the Inspector!");
            }
        }

        private IEnumerator CycleTexturesRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_changeInterval);

                _currentIndex = (_currentIndex + 1) % _textures.Length;

                if (_textures[_currentIndex] != null)
                {
                    _material.SetTexture("_MainTex", _textures[_currentIndex]);
                }
            }
        }
    }
}
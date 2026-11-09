using UnityEngine;

namespace InteractiveRaymarcher
{
    public class Orb : MonoBehaviour
    {
        public Renderer Renderer { get => _renderer; }

        [SerializeField] private Renderer _renderer;
        [SerializeField] private SphereCollider _collider;
        [SerializeField] private Rigidbody _rigidBody;

        [SerializeField] private float xBounds = 0.5f;
        [SerializeField] private float yBounds = 0.5f;
        [SerializeField] private float zBounds = 0.5f;
        [SerializeField] private float _velocityMultiplier = 0.1f;
        [SerializeField] private float _minVelocity = 0.05f;
        [SerializeField] private float _minScale = 0.2f;
        [SerializeField] private float _maxScale = 1f;

        private Vector3 _startingVelocity;
        private Vector3 _origin;
        private Vector3 _baseScale;

        private void Start()
        {
            _origin = new Vector3(0, 0, 0);
            _baseScale = new Vector3(1, 1, 1);
            _rigidBody.linearVelocity = HelperMethods.GetRandomVec3() * _velocityMultiplier;
        }

        private void Update()
        {
            UpdateRigidbodies();
        }

        private void UpdateRigidbodies()
        {
            var p = _rigidBody.transform.position;
            var v = _rigidBody.linearVelocity;

            if (v.magnitude < _minVelocity)
            {
                _rigidBody.linearVelocity *= 1.1f;
            }

            if (_rigidBody.transform.position.x > xBounds)
            {
                _rigidBody.transform.position = new Vector3(xBounds, p.y, p.z);
                _rigidBody.linearVelocity = new Vector3(-v.x, v.y, v.z);
            }
            else if (_rigidBody.transform.position.x < -xBounds)
            {
                _rigidBody.transform.position = new Vector3(-xBounds, p.y, p.z);
                _rigidBody.linearVelocity = new Vector3(-v.x, v.y, v.z);
            }

            if (_rigidBody.transform.position.y > yBounds)
            {
                _rigidBody.transform.position = new Vector3(p.x, yBounds, p.z);
                _rigidBody.linearVelocity = new Vector3(v.x, -v.y, v.z);
            }
            else if (_rigidBody.transform.position.y < -yBounds)
            {
                _rigidBody.transform.position = new Vector3(p.x, -yBounds, p.z);
                _rigidBody.linearVelocity = new Vector3(v.x, -v.y, v.z);
            }

            if (_rigidBody.transform.position.z > zBounds)
            {
                _rigidBody.transform.position = new Vector3(p.x, p.y, zBounds);
                _rigidBody.linearVelocity = new Vector3(v.x, v.y, -v.z);
            }
            else if (_rigidBody.transform.position.z < -zBounds)
            {
                _rigidBody.transform.position = new Vector3(p.x, p.y, -zBounds);
                _rigidBody.linearVelocity = new Vector3(v.x, v.y, -v.z);
            }
        }
    }
}
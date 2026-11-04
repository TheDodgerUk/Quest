using UnityEngine;

namespace ArcCanvas
{
    /// <summary>Minimal WASD + right-click-drag look camera for exploring the demo scene without a VR headset. Demo convenience only.</summary>
    [AddComponentMenu("")]
    public class ArcCanvasFlyCamDemo : MonoBehaviour
    {
        public float moveSpeed = 2.5f;
        public float lookSpeed = 120f;

        float _yaw;
        float _pitch;

        void Start()
        {
            var e = transform.eulerAngles;
            _yaw = e.y;
            _pitch = e.x;
        }

        void Update()
        {
            if (Input.GetMouseButton(1))
            {
                _yaw += Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime;
                _pitch -= Input.GetAxis("Mouse Y") * lookSpeed * Time.deltaTime;
                _pitch = Mathf.Clamp(_pitch, -80f, 80f);
                transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }

            Vector3 move = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) move += transform.forward;
            if (Input.GetKey(KeyCode.S)) move -= transform.forward;
            if (Input.GetKey(KeyCode.A)) move -= transform.right;
            if (Input.GetKey(KeyCode.D)) move += transform.right;
            if (move.sqrMagnitude > 0f)
                transform.position += move.normalized * moveSpeed * Time.deltaTime;
        }
    }
}

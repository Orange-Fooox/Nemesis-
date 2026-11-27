using UnityEngine;

namespace Nemesis.CameraSystem
{
    public sealed class IsometricCameraRig : MonoBehaviour
    {
        [SerializeField] private float followSharpness = 8f;
        [SerializeField] private float distance = 14f;
        [SerializeField] private float pitch = 35.264f;
        [SerializeField] private float yaw = 45f;
        private Transform target;
        private Quaternion viewRotation;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            viewRotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.rotation = viewRotation;
            if (target != null)
                transform.position = target.position + viewRotation * (Vector3.back * distance);
        }

        private void LateUpdate()
        {
            if (target == null) return;
            viewRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desired = target.position + viewRotation * (Vector3.back * distance);
            float blend = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, blend);
            transform.rotation = viewRotation;
        }
    }
}

#nullable enable

using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Managers
{
    public class CameraController : MonoBehaviour
    {
        private readonly List<CinemachineCamera> _cameras = new();
        public static CameraController Instance { get; private set; } = null!;
        public CinemachineCamera? ActiveCamera { get; private set; }

        private void Awake()
        {
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public bool IsActive(CinemachineCamera cam)
        {
            return ActiveCamera == cam;
        }

        public void Register(CinemachineCamera cam)
        {
            if (!_cameras.Contains(cam))
                _cameras.Add(cam);

            if (!ActiveCamera)
                SetActiveCamera(cam);
        }

        public void Unregister(CinemachineCamera cam)
        {
            _cameras.Remove(cam);
            if (ActiveCamera == cam)
                SetActiveCamera(_cameras.Count > 0 ? _cameras[0] : null);
        }

        public void SetActiveCamera(CinemachineCamera? cam)
        {
            ActiveCamera = cam;
            foreach (var c in _cameras) c.Priority = c == cam ? 10 : 0;
        }
    }
}
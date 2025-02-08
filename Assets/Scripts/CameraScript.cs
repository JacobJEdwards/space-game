using System;
using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static CameraController Instance { get; set; }
    private readonly List<CinemachineCamera> _cameras = new ();

    private CinemachineCamera ActiveCamera { get; set; }

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static bool IsActive(CinemachineCamera cam)
    {
        return Instance.ActiveCamera == cam;
    }

    public static void Register(CinemachineCamera cam)
    {
        if (!Instance._cameras.Contains(cam))
            Instance._cameras.Add(cam);

        if (!Instance.ActiveCamera)
            SetActiveCamera(cam);
    }

    public static void Unregister(CinemachineCamera cam)
    {
        Instance._cameras.Remove(cam);
        if (Instance.ActiveCamera == cam)
            SetActiveCamera(Instance._cameras.Count > 0 ? Instance._cameras[0] : null);
    }

    public static void SetActiveCamera(CinemachineCamera cam)
    {
        Instance.ActiveCamera = cam;
        foreach (var c in Instance._cameras)
        {
            c.Priority = c == cam ? 10 : 0;
        }
    }
}
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static readonly List<CinemachineCamera> Cameras = new();

    private static CinemachineCamera ActiveCamera { get; set; }

    public static bool IsActive(CinemachineCamera cam)
    {
        return ActiveCamera == cam;
    }

    public static void Register(CinemachineCamera cam)
    {
        if (!Cameras.Contains(cam))
            Cameras.Add(cam);

        if (!ActiveCamera)
            SetActiveCamera(cam);
    }

    public static void Unregister(CinemachineCamera cam)
    {
        Cameras.Remove(cam);
        if (ActiveCamera == cam)
            SetActiveCamera(Cameras.Count > 0 ? Cameras[0] : null);
    }

    public static void SetActiveCamera(CinemachineCamera cam)
    {
        ActiveCamera = cam;
        foreach (var c in Cameras) c.Priority = c == cam ? 10 : 0;
    }
}
using DG.Tweening;
using Unity.Assertions;
using UnityEngine;

public static class TargetInfo
{
    public static bool IsTargetInRange(Camera cam, out RaycastHit hitInfo, float range, LayerMask mask)
    {
        var x = Screen.width / 2;
        var y = Screen.height / 2;
        var ray = cam.ScreenPointToRay(new Vector3(x, y));

        return Physics.Raycast(ray, out hitInfo, range, mask);
    }
}
using UnityEngine;

public static class TargetInfo
{
    public static bool IsTargetInRange(Vector3 rayPosition, Vector3 rayDirection, out RaycastHit hitInfo, float range,
        LayerMask mask)
    {
        return Physics.Raycast(rayPosition, rayDirection, out hitInfo, range, mask);
    }
}
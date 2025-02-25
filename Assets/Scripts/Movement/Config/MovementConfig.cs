#nullable enable

using UnityEngine;

namespace Movement.Config
{
[CreateAssetMenu(fileName = "MovementConfig", menuName = "Scriptable Objects/MovementConfig")]
public class MovementConfig : ScriptableObject
{
    [Header("Movement Settings")] [SerializeField]
    private float speed = 10f;
}
}

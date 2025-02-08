using UnityEngine;

[CreateAssetMenu(fileName = "ShipMovementConfig", menuName = "Movement/Ship Movement Config")]
public class ShipMovementConfig : MovementConfig
{
    public float rollTorque = 1000f;
    public float yawTorque = 500f;
    public float pitchTorque = 1000f;
}

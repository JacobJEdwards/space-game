using UnityEngine;

[CreateAssetMenu(fileName = "MovementConfig", menuName = "Scriptable Objects/MovementConfig")]
public abstract class MovementConfig : ScriptableObject
{
    [Header("Basic Movement")]
    public float thrust = 100f;
    public float upThrust = 50f;
    public float strafeThrust = 50f;
    [Range(0.001f, 0.999f)]
    public float thrustGlideReduction = 0.99f;
    [Range(0.001f, 0.999f)]
    public float upDownGlideReduction = 0.99f;
    [Range(0.001f, 0.999f)]
    public float leftRightGlideReduction = 0.99f;

    [Header("Boost")]
    public float maxBoostAmount = 2f;
    public float boostDepreciationRate = 0.25f;
    public float boostRechargeRate = 0.1f;
    public float boostMultiplier = 5f;
}


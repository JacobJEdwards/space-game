using UnityEngine;

[CreateAssetMenu(fileName = "SpaceMovementConfig", menuName = "Movement/SpaceMovementConfig", order = 1)]
public class SpaceMovementConfig : MovementConfig
{
    [Header("Space Movement Settings")] [SerializeField]
    private float rollTorque = 500f;

    [SerializeField] private float pitchTorque = 1000f;

    [SerializeField] private float yawTorque = 1000f;

    [SerializeField] private float thrust = 100f;

    [SerializeField] private float upThrust = 50f;

    [SerializeField] private float strafeThrust = 50f;

    [SerializeField] [Range(0.001f, 0.999f)]
    private float thrustGlideReduction = 0.99f;

    [SerializeField] [Range(0.001f, 0.999f)]
    private float upDownGlideReduction = 0.99f;

    [SerializeField] [Range(0.001f, 0.999f)]
    private float leftRightGlideReduction = 0.99f;

    [Header("Boost Settings")] [SerializeField]
    private float maxBoostAmount = 2f;

    [SerializeField] private float boostDepreciationRate = 0.25f;

    [SerializeField] private float boostRechargeRate = 0.1f;

    [SerializeField] private float boostMultiplier = 5f;

    [SerializeField] private bool useCameraDirection;

    public float RollTorque => rollTorque;
    public float PitchTorque => pitchTorque;
    public float YawTorque => yawTorque;
    public float Thrust => thrust;
    public float UpThrust => upThrust;
    public float StrafeThrust => strafeThrust;
    public float ThrustGlideReduction => thrustGlideReduction;
    public float UpDownGlideReduction => upDownGlideReduction;
    public float LeftRightGlideReduction => leftRightGlideReduction;
    public float MaxBoostAmount => maxBoostAmount;
    public float BoostDepreciationRate => boostDepreciationRate;
    public float BoostRechargeRate => boostRechargeRate;
    public float BoostMultiplier => boostMultiplier;

    public bool UseCameraDirection => useCameraDirection;
}
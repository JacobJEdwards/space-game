using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyShipAI : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private SpaceMovementConfig movementConfig;

    private Rigidbody _rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!target) return;

        // use thrust to move towards target
        var direction = (target.position - transform.position).normalized;
        var thrust = direction * movementConfig.Thrust;
        _rb.AddForce(thrust);

        // use torque to rotate towards target
        var targetRotation = Quaternion.LookRotation(direction);
        var torque = Vector3.Cross(transform.forward, targetRotation * Vector3.forward);
        _rb.AddTorque(torque * movementConfig.YawTorque);
    }
}

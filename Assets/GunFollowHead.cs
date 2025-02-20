using Unity.Assertions;
using UnityEngine;

public class GunFollowHead : MonoBehaviour
{

    [SerializeField] private Transform head;
    [SerializeField] private Transform gun;

    private void Start()
    {
        Assert.IsNotNull(head);
        Assert.IsNotNull(gun);

        gun.SetParent(head);
    }
}

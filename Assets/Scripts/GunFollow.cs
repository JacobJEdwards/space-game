using Unity.Assertions;
using UnityEngine;

public class GunFollow : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private Transform gun;
    [SerializeField] private Transform aim;

    private void Start()
    {
        Assert.IsNotNull(head);
        Assert.IsNotNull(gun);

        gun.SetParent(head);
    }
}
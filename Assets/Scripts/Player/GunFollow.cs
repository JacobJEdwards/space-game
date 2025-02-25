#nullable enable

using System;
using Unity.Assertions;
using UnityEngine;

namespace Player
{

public class GunFollow : MonoBehaviour
{
    [SerializeField] private Transform head = null!;
    [SerializeField] private Transform gun = null!;
    [SerializeField] private Animator animator = null!;

    private void Start()
    {
        Assert.IsNotNull(head);
        Assert.IsNotNull(gun);

        gun.SetParent(head);
    }
}
}

using System;
using JetBrains.Annotations;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public GameObject explosionPrefab;
    public float forceThreshold = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnCollisionEnter(Collision other)
    {
        if (other.relativeVelocity.magnitude > forceThreshold)
            Explode();
    }

    private void Explode()
    {
        if (explosionPrefab)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        gameObject.SetActive(false);
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }
}

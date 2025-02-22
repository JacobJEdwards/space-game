using System;
using Managers;
using UnityEngine;

public class Objective : MonoBehaviour
{
    public Color color;
    public Sprite sprite;
    public CompassManager compassManager;

    private void Start()
    {
        compassManager.AddObjectiveForObject(gameObject, color, sprite);
    }

}
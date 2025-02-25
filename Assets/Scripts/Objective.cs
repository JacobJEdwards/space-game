#nullable enable

using Managers;
using UnityEngine;
using UnityEngine.Assertions;

public class Objective : MonoBehaviour
{
    public Color color;
    public Sprite sprite = null!;
    public CompassManager compassManager = null!;

    private void Start()
    {
        Assert.IsNotNull(compassManager, "CompassManager is not set!");
        Assert.IsNotNull(sprite, "Sprite is not set!");

        compassManager.AddObjectiveForObject(gameObject, color, sprite);
    }

}
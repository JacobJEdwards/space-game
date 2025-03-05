#nullable enable

using UnityEngine;

namespace CollectableResources
{
    [CreateAssetMenu(fileName = "ResourceObject", menuName = "Scriptable Objects/Resource")]
    public class ResourceObject : ScriptableObject
    {
        [SerializeField] public string resourceName = "New Resource";
        [SerializeField] public int resourceAmount;
        [SerializeField] public Sprite? resourceSprite;
        [SerializeField] public string resourceDescription = "Todo";
    }
}
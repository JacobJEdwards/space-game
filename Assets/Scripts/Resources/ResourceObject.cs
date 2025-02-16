using UnityEngine;

namespace CollectableResources
{
    [CreateAssetMenu(fileName = "ResourceObject", menuName = "Scriptable Objects/Resource")]
    public class ResourceObject : ScriptableObject
    {
        public string resourceName;
        public int resourceAmount;
        public Sprite resourceSprite;
    }
}
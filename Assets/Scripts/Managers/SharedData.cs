#nullable enable
using UnityEngine;

namespace Managers
{
    public class SharedData : MonoBehaviour
    {
        public string? savePath;
        public bool newGame = true;

        public static SharedData Instance { get; private set; } = null!;


        public void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
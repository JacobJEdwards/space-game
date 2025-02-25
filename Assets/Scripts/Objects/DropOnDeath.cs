#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace Objects
{
    [RequireComponent(typeof(Health))]
    public class DropOnDeath : MonoBehaviour
    {
        public Health health = null!;
        [SerializeField] public List<GameObject> possibleDrops = new ();

        private void Start()
        {
            health = GetComponent<Health>();
            health.onDeath.AddListener(OnDie);
        }

        private void OnDie()
        {
            if (possibleDrops.Count == 0) return;

            var drop = possibleDrops[Random.Range(0, possibleDrops.Count)];
            Instantiate(drop, transform.position + Random.insideUnitSphere, Random.rotation);
        }

    }
}
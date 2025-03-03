using UnityEngine;

namespace PlanetarySystem
{
    public class Stars : MonoBehaviour
    {
        [SerializeField] private ParticleSystem starsBackground = null!;
        [SerializeField] private Transform player = null!;

        private void Start()
        {
            starsBackground.Play();
        }

        private void LateUpdate()
        {
            starsBackground.transform.position = player.position;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Managers
{
    public class IntroManager : MonoBehaviour
    {
        [SerializeField] private Transform ship = null!;
        [SerializeField] private List<Transform> damagePoints = new();
        [SerializeField] private GameObject damageEffect = null!;
        [SerializeField] private GameObject explosionEffect = null!;
        [SerializeField] private Image fadePanel = null!;
        [SerializeField] private Transform cam = null!;

        [Header("Timing")] [SerializeField] private float damageStartTime = 5f;

        [SerializeField] private float explosionTime = 8f;
        [SerializeField] private float fadeToBlackTime = 9f;

        [Header("Ship Shake")] [SerializeField]
        private float shakeIntensity = 0.3f;

        [SerializeField] private float shakeFrequency = 0.5f;
        private readonly List<GameObject> _spawnedEffects = new();

        private Tweener _fadeTween;
        private bool _hasExploded;

        private Rigidbody _rb = null!;

        private Tweener _spinTween;
        private float _timer;
        private Tween _tweener;

        private void Awake()
        {
            _rb = ship.GetComponent<Rigidbody>();

            if (!fadePanel) return;

            var panelColor = fadePanel.color;
            panelColor.a = 0f;
            fadePanel.color = panelColor;
        }

        private void Start()
        {
            StartShake();

            Utils.HideLockMouse(true);
        }

        private void FixedUpdate()
        {
            _rb.AddForce(Vector3.forward * 100f, ForceMode.Force);
            _timer += Time.deltaTime;

            CheckSequenceEvents();
        }

        private void OnDestroy()
        {
            _tweener?.Kill();
        }

        private void CheckSequenceEvents()
        {
            if (_timer >= damageStartTime && _timer < explosionTime) TriggerDamageEffects();

            if (_timer >= explosionTime && !_hasExploded)
            {
                TriggerExplosion();
                _hasExploded = true;
            }

            if (_timer >= fadeToBlackTime && fadePanel && fadePanel.color.a < 1f) FadeToBlack();
        }

        private void StartShake()
        {
            _tweener?.Kill();

            _tweener = ship.DOShakePosition(1000f, shakeIntensity, 10, 90, false, false)
                .SetLoops(-1)
                .SetEase(Ease.Linear);
        }

        private void TriggerDamageEffects()
        {
            if (!(Random.value < 0.02f) || damagePoints.Count <= 0) return;

            var randomIndex = Random.Range(0, damagePoints.Count);
            var damagePoint = damagePoints[randomIndex];

            var effect = Instantiate(damageEffect, damagePoint.position, Quaternion.identity);
            _spawnedEffects.Add(effect);

            shakeIntensity += 0.05f;
            StartShake();
        }

        private void TriggerExplosion()
        {
            _tweener?.Kill();

            Instantiate(explosionEffect, ship.position, Quaternion.identity);

            _rb.linearVelocity = Vector3.zero;

            foreach (var effect in _spawnedEffects.Where(effect => effect))
                Destroy(effect);

            _spawnedEffects.Clear();

            StartSpinning();
        }

        private void StartSpinning()
        {
            _spinTween ??= ship.DORotate(new Vector3(70f, 20f, 360f), 2f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Incremental)
                .SetEase(Ease.Linear);
        }

        private void FadeToBlack()
        {
            _fadeTween ??= fadePanel.DOFade(1f, 5f).SetEase(Ease.InQuad)
                .OnComplete(LoadNextScene);
        }

        private void LoadNextScene()
        {
            // Small delay to ensure the screen is completely black before loading
            StartCoroutine(LoadAsynchronously("SpaceScene"));
        }

        private IEnumerator LoadAsynchronously(string sceneName)
        {
            // scene name is just the name of the current scene being loaded
            var operation = SceneManager.LoadSceneAsync(sceneName);

            while (operation is { isDone: false })
                yield return null;

            _tweener?.Kill();
            _spinTween?.Kill();
            _fadeTween?.Kill();
        }
    }
}
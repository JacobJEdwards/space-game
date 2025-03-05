using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private Transform ship = null!;
    [SerializeField] private List<Transform> damagePoints = new();
    [SerializeField] private GameObject damageEffect = null!;
    [SerializeField] private GameObject explosionEffect = null!;
    [SerializeField] private Image fadePanel = null!; // Add a black Image UI element for fading
    [SerializeField] private Transform cam = null!;

    [Header("Timing")] [SerializeField] private float damageStartTime = 5f;

    [SerializeField] private float explosionTime = 8f;
    [SerializeField] private float fadeToBlackTime = 9f;

    [Header("Ship Shake")] [SerializeField]
    private float shakeIntensity = 0.3f;

    [SerializeField] private float shakeFrequency = 0.5f;
    private readonly List<GameObject> _spawnedEffects = new();
    private bool _hasExploded;

    private Rigidbody _rb = null!;
    private float _timer;
    private Tween _tweener;

    private Tweener fadeTween;

    private Tweener spinTween;

    private void Awake()
    {
        _rb = ship.GetComponent<Rigidbody>();

        // Make sure fade panel is transparent at the start
        if (!fadePanel) return;

        var panelColor = fadePanel.color;
        panelColor.a = 0f;
        fadePanel.color = panelColor;
    }

    private void Start()
    {
        // Start the ship shake immediately
        StartShake();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        _rb.AddForce(Vector3.forward * 100f, ForceMode.Force);
        _timer += Time.deltaTime;

        // Check for sequence events based on timer
        CheckSequenceEvents();
    }

    private void OnDestroy()
    {
        // Clean up any tweens
        _tweener?.Kill();
    }

    private void CheckSequenceEvents()
    {
        // Start damage effects
        if (_timer >= damageStartTime && _timer < explosionTime) TriggerDamageEffects();

        // Trigger explosion
        if (_timer >= explosionTime && !_hasExploded)
        {
            TriggerExplosion();
            _hasExploded = true;
        }

        // Fade to black
        if (_timer >= fadeToBlackTime && fadePanel != null && fadePanel.color.a < 1f) FadeToBlack();
    }

    private void StartShake()
    {
        // Make sure any existing tween is killed
        _tweener?.Kill();

        // Create continuous shake effect using DOTween
        _tweener = ship.DOShakePosition(1000f, shakeIntensity, 10, 90, false, false)
            .SetLoops(-1) // Infinite looping
            .SetEase(Ease.Linear);
    }

    private void TriggerDamageEffects()
    {
        // Spawn random damage effects at random intervals
        if (Random.value < 0.02f && damagePoints.Count > 0) // 2% chance per frame to spawn a new effect
        {
            var randomIndex = Random.Range(0, damagePoints.Count);
            var damagePoint = damagePoints[randomIndex];

            // Instantiate damage effect at the damage point position
            var effect = Instantiate(damageEffect, damagePoint.position, Quaternion.identity);
            _spawnedEffects.Add(effect);

            // Increase shake intensity as damage accumulates
            shakeIntensity += 0.05f;
            StartShake(); // Restart shake with increased intensity
        }
    }

    private void TriggerExplosion()
    {
        // Kill the shake tween
        _tweener?.Kill();

        // Spawn the explosion at the ship's position
        Instantiate(explosionEffect, ship.position, Quaternion.identity);

        // Stop the ship
        _rb.linearVelocity = Vector3.zero;

        foreach (var effect in _spawnedEffects.Where(effect => effect))
            Destroy(effect);

        _spawnedEffects.Clear();

        StartSpinning();
    }

    private void StartSpinning()
    {
        // Spin the ship around its forward axis
        spinTween ??= ship.DORotate(new Vector3(70f, 20f, 360f), 2f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);
    }

    private void FadeToBlack()
    {
        // Fade the panel to black
        fadeTween ??= fadePanel.DOFade(1f, 5f).SetEase(Ease.InQuad)
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

        while (!operation.isDone)
            yield return null;

        _tweener?.Kill();
        spinTween?.Kill();
        fadeTween?.Kill();
    }
}
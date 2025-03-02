using System;
using System.Collections;
using Interfaces;
using Managers;
using Movement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class Death : MonoBehaviour, IUIPanel
    {
        public UIState AssociatedState => UIState.Death;

        [SerializeField] private Text title = null!;
        [SerializeField] private Text tooltip = null!;

        private void Start()
        {
            UiManager.Instance.RegisterPanel(this);
        }


        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            StartCoroutine(FadeInTitle());
        }

        private IEnumerator FadeInTitle()
        {
            var color = title.color;
            while (color.a < 1)
            {
                color.a += Time.deltaTime;
                title.color = color;
                yield return null;
            }

            StartCoroutine(FadeInTooltip());
        }

        private IEnumerator FadeInTooltip()
        {
            var color = tooltip.color;
            while (color.a < 1)
            {
                color.a += Time.deltaTime;
                tooltip.color = color;
                yield return null;
            }

            InputManager.Instance.SetOnInteractPressed(OnInteractPressed);
        }

        private void OnInteractPressed()
        {
            SceneManager.LoadSceneAsync("MenuScene");
        }

        private void OnDisable()
        {
            StopAllCoroutines();

            var color = title.color;
            color.a = 0;
            title.color = color;

            var tooltipColor = tooltip.color;
            tooltipColor.a = 0;
            tooltip.color = tooltipColor;
        }
    }
}
using System.Collections;
using DG.Tweening;
using Interfaces;
using Managers;
using Movement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class Final : MonoBehaviour, IUIPanel
    {
        [SerializeField] private Text title = null!;
        [SerializeField] private Text tooltip = null!;
        [SerializeField] private Image blackScreen = null!;

        private void Start()
        {
            UiManager.Instance.RegisterPanel(this);
            Hide();
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

        public UIState AssociatedState => UIState.Final;

        public void Show()
        {
            gameObject.SetActive(true);
            FadeToBlack();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void FadeToBlack()
        {
            blackScreen.gameObject.SetActive(true);
            blackScreen.color = new Color(0, 0, 0, 0);

            blackScreen.DOFade(1, 1f).OnComplete(() =>
            {
                StartCoroutine(FadeInTitle());
            });
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

        private static void OnInteractPressed()
        {
            SceneManager.LoadSceneAsync("MenuScene");
        }
    }
}
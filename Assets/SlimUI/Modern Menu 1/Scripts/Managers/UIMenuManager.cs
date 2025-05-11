using System.Collections;
using Managers;
using Movement;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SlimUI.ModernMenu
{
    public class UIMenuManager : MonoBehaviour
    {
        public enum Theme
        {
            Custom1,
            Custom2,
            Custom3
        }

        private static readonly int Animate = Animator.StringToHash("Animate");

        [Header("MENUS")] public GameObject mainMenu;

        public GameObject firstMenu;


        public GameObject playMenu;


        public GameObject exitMenu;

        [Header("THEME SETTINGS")] public Theme theme;

        public ThemedUIData themeController;

        [Header("PANELS")] public GameObject mainCanvas;


        public GameObject PanelControls;


        public GameObject PanelVideo;


        public GameObject PanelGame;

        public Texture2D cursorTexture;
        public CursorMode cursorMode = CursorMode.Auto;
        public Vector2 hotSpot = Vector2.zero;
        public float scale = 0.3f;


        // highlights in settings screen
        [Header("SETTINGS SCREEN")] public GameObject lineGame;


        public GameObject lineVideo;


        public GameObject lineControls;

        [Header("LOADING SCREEN")] public bool waitForInput = true;

        public GameObject loadingMenu;

        public Slider loadingBar;

        public TMP_Text loadPromptText;

        [Header("SFX")] public AudioSource hoverSound;

        public AudioSource sliderSound;

        public AudioSource swooshSound;

        private bool _allowSceneActivation;
        private Animator _cameraObject;
        private InputManager _inputManager;

        private void Start()
        {
            _inputManager = InputManager.Instance;

            _inputManager.SetOnInteractPressed(
                () =>
                {
                    if (waitForInput) _allowSceneActivation = true;
                }
            );

            _cameraObject = transform.GetComponent<Animator>();

            playMenu.SetActive(false);
            exitMenu.SetActive(false);
            firstMenu.SetActive(true);
            mainMenu.SetActive(true);

            Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            SetThemeColors();

            Time.timeScale = 1;
        }

        public void LoadSavedScene()
        {
            SharedData.Instance.newGame = false;
            LoadScene("SpaceScene");
        }

        private void SetThemeColors()
        {
            switch (theme)
            {
                case Theme.Custom1:
                    themeController.currentColor = themeController.custom1.graphic1;
                    themeController.textColor = themeController.custom1.text1;
                    break;
                case Theme.Custom2:
                    themeController.currentColor = themeController.custom2.graphic2;
                    themeController.textColor = themeController.custom2.text2;
                    break;
                case Theme.Custom3:
                    themeController.currentColor = themeController.custom3.graphic3;
                    themeController.textColor = themeController.custom3.text3;
                    break;
                default:
                    Debug.Log("Invalid theme selected.");
                    break;
            }
        }

        public void PlayCampaign()
        {
            exitMenu.SetActive(false);
            playMenu.SetActive(true);
        }

        public void PlayCampaignMobile()
        {
            exitMenu.SetActive(false);
            playMenu.SetActive(true);
            mainMenu.SetActive(false);
        }

        public void ReturnMenu()
        {
            playMenu.SetActive(false);
            exitMenu.SetActive(false);
            mainMenu.SetActive(true);
        }

        public void LoadScene(string scene)
        {
            if (scene != "") StartCoroutine(LoadAsynchronously(scene));
        }

        private void DisablePlayCampaign()
        {
            playMenu.SetActive(false);
        }

        public void Position2()
        {
            DisablePlayCampaign();
            _cameraObject.SetFloat(Animate, 1);
        }

        public void Position1()
        {
            _cameraObject.SetFloat(Animate, 0);
        }

        private void DisablePanels()
        {
            PanelControls.SetActive(false);
            PanelVideo.SetActive(false);
            PanelGame.SetActive(false);

            lineGame.SetActive(false);
            lineControls.SetActive(false);
            lineVideo.SetActive(false);
        }

        public void GamePanel()
        {
            DisablePanels();
            PanelGame.SetActive(true);
            lineGame.SetActive(true);
        }

        public void VideoPanel()
        {
            DisablePanels();
            PanelVideo.SetActive(true);
            lineVideo.SetActive(true);
        }

        public void ControlsPanel()
        {
            DisablePanels();
            PanelControls.SetActive(true);
            lineControls.SetActive(true);
        }

        public void KeyBindingsPanel()
        {
            DisablePanels();
            MovementPanel();
        }

        public void MovementPanel()
        {
            DisablePanels();
        }

        public void CombatPanel()
        {
            DisablePanels();
        }

        public void GeneralPanel()
        {
            DisablePanels();
        }

        public void PlayHover()
        {
            hoverSound.Play();
        }

        public void PlaySFXHover()
        {
            sliderSound.Play();
        }

        public void PlaySwoosh()
        {
            swooshSound.Play();
        }

        // Are You Sure - Quit Panel Pop Up
        public void AreYouSure()
        {
            exitMenu.SetActive(true);
            DisablePlayCampaign();
        }

        public void AreYouSureMobile()
        {
            exitMenu.SetActive(true);
            mainMenu.SetActive(false);
            DisablePlayCampaign();
        }

        public void ExtrasMenu()
        {
            playMenu.SetActive(false);
            exitMenu.SetActive(false);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
        }

        // Load Bar synching animation
        private IEnumerator LoadAsynchronously(string sceneName)
        {
            // scene name is just the name of the current scene being loaded
            var operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null)
            {
                Debug.Log("Scene not found");
                yield break;
            }

            operation.allowSceneActivation = false;
            mainCanvas.SetActive(false);
            loadingMenu.SetActive(true);

            while (!operation.isDone)
            {
                var progress = Mathf.Clamp01(operation.progress / .95f);
                loadingBar.value = progress;

                switch (operation.progress)
                {
                    case >= 0.9f when waitForInput:
                    {
                        loadPromptText.text = "Press F to continue";
                        loadingBar.value = 1;

                        if (_allowSceneActivation) operation.allowSceneActivation = true;

                        break;
                    }
                    case >= 0.9f when !waitForInput:
                        operation.allowSceneActivation = true;
                        break;
                }

                yield return null;
            }
        }
    }
}
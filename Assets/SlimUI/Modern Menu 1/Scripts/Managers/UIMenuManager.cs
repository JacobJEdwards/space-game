using UnityEngine;
using System.Collections;
using Movement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace SlimUI.ModernMenu{
	public class UIMenuManager : MonoBehaviour {
		private Animator _cameraObject;

		// campaign button sub menu
        [Header("MENUS")]
        [Tooltip("The Menu for when the MAIN menu buttons")]
        public GameObject mainMenu;
        [Tooltip("THe first list of buttons")]
        public GameObject firstMenu;
        [Tooltip("The Menu for when the PLAY button is clicked")]
        public GameObject playMenu;
        [Tooltip("The Menu for when the EXIT button is clicked")]
        public GameObject exitMenu;

        public enum Theme {Custom1, Custom2, Custom3};
        [Header("THEME SETTINGS")]
        public Theme theme;
        private int _themeIndex;
        public ThemedUIData themeController;

        [Header("PANELS")]
        [Tooltip("The UI Panel parenting all sub menus")]
        public GameObject mainCanvas;
        [Tooltip("The UI Panel that holds the CONTROLS window tab")]
        public GameObject PanelControls;
        [Tooltip("The UI Panel that holds the VIDEO window tab")]
        public GameObject PanelVideo;
        [Tooltip("The UI Panel that holds the GAME window tab")]
        public GameObject PanelGame;


        // highlights in settings screen
        [Header("SETTINGS SCREEN")]
        [Tooltip("Highlight Image for when GAME Tab is selected in Settings")]
        public GameObject lineGame;
        [Tooltip("Highlight Image for when VIDEO Tab is selected in Settings")]
        public GameObject lineVideo;
        [Tooltip("Highlight Image for when CONTROLS Tab is selected in Settings")]
        public GameObject lineControls;

        [Header("LOADING SCREEN")]
		[Tooltip("If this is true, the loaded scene won't load until receiving user input")]
		public bool waitForInput = true;
        public GameObject loadingMenu;
		[Tooltip("The loading bar Slider UI element in the Loading Screen")]
        public Slider loadingBar;
        public TMP_Text loadPromptText;

		[Header("SFX")]
        [Tooltip("The GameObject holding the Audio Source component for the HOVER SOUND")]
        public AudioSource hoverSound;
        [Tooltip("The GameObject holding the Audio Source component for the AUDIO SLIDER")]
        public AudioSource sliderSound;
        [Tooltip("The GameObject holding the Audio Source component for the SWOOSH SOUND when switching to the Settings Screen")]
        public AudioSource swooshSound;

        private bool _allowSceneActivation;
        private InputManager _inputManager;

		private void Start(){
			_inputManager = InputManager.Instance;
			_inputManager.SetOnInteractPressed(
				() =>
				{
					if (waitForInput)
					{
						_allowSceneActivation = true;
					}
				}
				);

			_cameraObject = transform.GetComponent<Animator>();

			playMenu.SetActive(false);
			exitMenu.SetActive(false);
			firstMenu.SetActive(true);
			mainMenu.SetActive(true);

			SetThemeColors();
		}

		void SetThemeColors()
		{
			switch (theme)
			{
				case Theme.Custom1:
					themeController.currentColor = themeController.custom1.graphic1;
					themeController.textColor = themeController.custom1.text1;
					_themeIndex = 0;
					break;
				case Theme.Custom2:
					themeController.currentColor = themeController.custom2.graphic2;
					themeController.textColor = themeController.custom2.text2;
					_themeIndex = 1;
					break;
				case Theme.Custom3:
					themeController.currentColor = themeController.custom3.graphic3;
					themeController.textColor = themeController.custom3.text3;
					_themeIndex = 2;
					break;
				default:
					Debug.Log("Invalid theme selected.");
					break;
			}
		}

		public void PlayCampaign(){
			exitMenu.SetActive(false);
			playMenu.SetActive(true);
		}
		
		public void PlayCampaignMobile(){
			exitMenu.SetActive(false);
			playMenu.SetActive(true);
			mainMenu.SetActive(false);
		}

		public void ReturnMenu(){
			playMenu.SetActive(false);
			exitMenu.SetActive(false);
			mainMenu.SetActive(true);
		}

		public void LoadScene(string scene){
			if(scene != ""){
				StartCoroutine(LoadAsynchronously(scene));
			}
		}

		private void  DisablePlayCampaign(){
			playMenu.SetActive(false);
		}

		public void Position2(){
			DisablePlayCampaign();
			_cameraObject.SetFloat("Animate",1);
		}

		public void Position1(){
			_cameraObject.SetFloat("Animate",0);
		}

		void DisablePanels(){
			PanelControls.SetActive(false);
			PanelVideo.SetActive(false);
			PanelGame.SetActive(false);

			lineGame.SetActive(false);
			lineControls.SetActive(false);
			lineVideo.SetActive(false);
		}

		public void GamePanel(){
			DisablePanels();
			PanelGame.SetActive(true);
			lineGame.SetActive(true);
		}

		public void VideoPanel(){
			DisablePanels();
			PanelVideo.SetActive(true);
			lineVideo.SetActive(true);
		}

		public void ControlsPanel(){
			DisablePanels();
			PanelControls.SetActive(true);
			lineControls.SetActive(true);
		}

		public void KeyBindingsPanel(){
			DisablePanels();
			MovementPanel();
		}

		public void MovementPanel(){
			DisablePanels();
		}

		public void CombatPanel(){
			DisablePanels();
		}

		public void GeneralPanel(){
			DisablePanels();
		}

		public void PlayHover(){
			hoverSound.Play();
		}

		public void PlaySFXHover(){
			sliderSound.Play();
		}

		public void PlaySwoosh(){
			swooshSound.Play();
		}

		// Are You Sure - Quit Panel Pop Up
		public void AreYouSure(){
			exitMenu.SetActive(true);
			DisablePlayCampaign();
		}

		public void AreYouSureMobile(){
			exitMenu.SetActive(true);
			mainMenu.SetActive(false);
			DisablePlayCampaign();
		}

		public void ExtrasMenu(){
			playMenu.SetActive(false);
			exitMenu.SetActive(false);
		}

		public void QuitGame(){
			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
			#else
				Application.Quit();
			#endif
		}

		// Load Bar synching animation
		IEnumerator LoadAsynchronously(string sceneName){ // scene name is just the name of the current scene being loaded
			var operation = SceneManager.LoadSceneAsync(sceneName);
			if (operation == null){
				Debug.Log("Scene not found");
				yield break;
			}

			operation.allowSceneActivation = false;
			mainCanvas.SetActive(false);
			loadingMenu.SetActive(true);

			while (!operation.isDone){
				var progress = Mathf.Clamp01(operation.progress / .95f);
				loadingBar.value = progress;

				switch (operation.progress)
				{
					case >= 0.9f when waitForInput:
					{
						loadPromptText.text = "Press F to continue";
						loadingBar.value = 1;

						if (_allowSceneActivation){
							operation.allowSceneActivation = true;
						}

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
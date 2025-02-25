using System;
using System.Collections;
using Movement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIManager : MonoBehaviour
{
    private static readonly int Animate = Animator.StringToHash("Animate");

    [Header("UI References")] [SerializeField]
    private GameObject mainCanvas;

    [SerializeField] private GameObject loadingMenu;
    [SerializeField] private UnityEngine.UI.Slider loadingBar;
    [SerializeField] private UnityEngine.UI.Text loadPromptText;
    [SerializeField] private GameObject exitMenu;
    [SerializeField] private GameObject extrasMenu;
    [SerializeField] private GameObject playMenu;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject firstMenu;

    [Header("Others")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Animator cameraObject;


    [Header("Settings")] [SerializeField] private bool waitForInput = true;

    [Header("SFX")]
    public AudioSource hoverSound;
    public AudioSource sliderSound;
    public AudioSource swooshSound;

    private bool _allowSceneActivation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        inputManager.SetOnInteractPressed(() =>
        {
            if (waitForInput)
            {
                _allowSceneActivation = true;
            }
        });

        cameraObject = transform.GetComponent<Animator>();

        playMenu.SetActive(false);
        exitMenu.SetActive(false);
        if(extrasMenu) extrasMenu.SetActive(false);
        firstMenu.SetActive(true);
        mainMenu.SetActive(true);
    }

    public void Position2(){
        DisablePlayCampaign();
        cameraObject.SetFloat(Animate,1);
    }

    public void Position1(){
        cameraObject.SetFloat(Animate,0);
    }

    public void PlayCampaign(){
        exitMenu.SetActive(false);
        if(extrasMenu) extrasMenu.SetActive(false);
        playMenu.SetActive(true);
    }

    public void ReturnMenu(){
        playMenu.SetActive(false);
        if(extrasMenu) extrasMenu.SetActive(false);
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

    public void AreYouSure()
    {
        exitMenu.SetActive(true);
        if (extrasMenu) extrasMenu.SetActive(false);
        DisablePlayCampaign();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private IEnumerator LoadAsynchronously(string sceneName)
    {
        // scene name is just the name of the current scene being loaded
        var operation = SceneManager.LoadSceneAsync(sceneName);
        if (operation == null)
        {
            throw new Exception("Scene not found");
        }

        operation.allowSceneActivation = false;
        mainCanvas.SetActive(false);
        loadingMenu.SetActive(true);

        while (!operation.isDone)
        {
            var progress = Mathf.Clamp01(operation.progress / .95f);
            loadingBar.value = progress;

            if (operation.progress >= 0.9f && waitForInput)
            {
                loadPromptText.text = "Press F to continue";
                loadingBar.value = 1;

                if (_allowSceneActivation)
                {
                    operation.allowSceneActivation = true;
                }
            }
            else if (operation.progress >= 0.9f && !waitForInput)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
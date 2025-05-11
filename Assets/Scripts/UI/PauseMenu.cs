using Interfaces;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class PauseMenu : MonoBehaviour, IUIPanel
    {
        private void Start()
        {
            UiManager.Instance.RegisterPanel(this);
            Hide();
        }

        public UIState AssociatedState => UIState.Pause;

        public void Show()
        {
            gameObject.SetActive(true);
            Time.timeScale = 0;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            Time.timeScale = 1;
        }

        public void ResumeGame()
        {
            UiManager.Instance.TogglePause();
            Hide();
        }

        public void QuitGame()
        {
            // SaveManager.Instance.SaveGame();
            SceneManager.LoadScene("Scenes/MenuScene");
        }
    }
}
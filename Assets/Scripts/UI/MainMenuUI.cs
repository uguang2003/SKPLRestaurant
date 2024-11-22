using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    public GameObject loginPanel;

    private void Awake() {
        playButton.onClick.AddListener(() => {
            //Loader.Load(Loader.Scene.GameScene);

            //´ò¿ªµÇÂ¼Ãæ°å
            loginPanel.SetActive(true);
            this.gameObject.SetActive(false);
        });
        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });
    }

}

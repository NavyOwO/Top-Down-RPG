using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class TitleMenu : MonoBehaviour {
    [Header("UI")]
    [SerializeField] private Canvas titleCanvas;
    [SerializeField] private GameObject teachPanel;
    [SerializeField] private GameObject menu;

    [Header("Cameras")]
    [SerializeField] private Camera gameCam;
    [SerializeField] private Camera titleCam;
    void Start() {
        InitTitle();
    }
    public void InitTitle() {
        Time.timeScale = 1f;

        teachPanel.SetActive(false);

        titleCanvas.enabled = true;
        titleCanvas.gameObject.SetActive(true);

        titleCam.enabled = true;
        titleCam.gameObject.SetActive(true);

        gameCam.enabled = false;
        menu.SetActive(false);

        PauseController.SetPause(true);
    }

    public async void PlayGame() {
        await ScreenFader.Instance.FadeOut();

        gameCam.enabled = true;
        gameCam.gameObject.SetActive(true);

        titleCam.enabled = false;
        titleCanvas.enabled = false;

        await ScreenFader.Instance.FadeIn();

        PauseController.SetPause(false);
    }

    public void ShowTeachPanel(bool show) {
        teachPanel.gameObject.SetActive(show);
    }
}
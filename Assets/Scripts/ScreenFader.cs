using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class ScreenFader : MonoBehaviour {
    public static ScreenFader Instance;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float fadeDuration = 0.5f;
    [SerializeField] CinemachineCamera vcam;

    CinemachinePositionComposer composer;
    Vector3 originalDamping;

    private void Awake() {
        if (Instance == null) Instance = this;
        else {
            Destroy(gameObject);
            return;
        }

        composer = vcam.GetComponent<CinemachinePositionComposer>();
        if (composer != null) originalDamping = composer.Damping;
    }

    async Task Fade(float targetTransparency) {
        float start = canvasGroup.alpha, t = 0;
        while (t < fadeDuration) {
            t += Time.unscaledDeltaTime; //weird time scaling from before with deltatime
            canvasGroup.alpha = Mathf.Lerp(start, targetTransparency, t / fadeDuration);
            await Task.Yield();
        }
        canvasGroup.alpha = targetTransparency;
    }

    public async Task FadeOut() {
        await Fade(1); // fade to black
        SetDamping(Vector3.zero);
    }

    public async Task FadeIn() {
        await Fade(0); // fade to transparent
        SetDamping(originalDamping);
    }

    void SetDamping(Vector3 d) {
        if (composer == null) return;
        composer.Damping = d;
    }
}
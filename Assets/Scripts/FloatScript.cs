using UnityEngine;

public class FloatingTitle : MonoBehaviour {
    [Header("Bob")]
    [Tooltip("Amount is in pixels")]
    public float bobAmount = 15f;
    public float bobSpeed = 1.2f;

    [Header("Sway")]
    public float swayAmount = 6f;
    public float swaySpeed = 0.7f;

    [Header("Tilt")]
    [Tooltip("TiltAmount is in degrees")]
    public float tiltAmount = 2f;
    public float tiltSpeed = 0.9f;

    RectTransform rect;
    Vector2 startPos;

    void Awake() {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
    }

    void Update() {
        float t = Time.unscaledTime;

        float y = Mathf.Sin(t * bobSpeed) * bobAmount;
        float x = Mathf.Sin(t * swaySpeed + 1.3f) * swayAmount;
        rect.anchoredPosition = startPos + new Vector2(x, y);

        float tilt = Mathf.Sin(t * tiltSpeed + 2f) * tiltAmount;
        rect.localRotation = Quaternion.Euler(0, 0, tilt);
    }
}
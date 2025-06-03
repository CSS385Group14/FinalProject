using TMPro;
using UnityEngine;

public class PopupText : MonoBehaviour
{
    public float lifetime = 1f;
    public float moveSpeed = 3f;
    public float fadeSpeed = 4f;
    public AudioClip popupSound;

    private TextMeshPro tmpText;
    private CanvasGroup canvasGroup;
    private AudioSource audioSource;
    private float timer;

    void Awake()
    {
        tmpText = GetComponent<TextMeshPro>();
        canvasGroup = GetComponent<CanvasGroup>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        timer = 0f;

        // play sound, if wanted
        if (popupSound != null && audioSource != null)
        {
           audioSource.PlayOneShot(popupSound);
        }
    }

    void Update()
    {
        // move upward
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime, Space.Self);

        // gradually fade out
        if (canvasGroup != null)
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
        }

        // despawn
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string message, Color color)
    {
        if (tmpText != null)
        {
            tmpText.text = message;
            tmpText.color = color;
            tmpText.fontSize = 16f;
        }
    }
}

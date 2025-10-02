using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Button3D : MonoBehaviour/*, IPointerDownHandler, IPointerUpHandler*/
{
    [SerializeField] private MeshRenderer buttonRenderer;
    [SerializeField] private float fadeTime = 1.0f;
    public bool buttonPressed;
    private bool isFading = false;

    public event Action<Vector3> OnPressedEvents;
    public event Action OnReleasedEvents;


    private void Awake()
    {
        buttonRenderer = GetComponent<MeshRenderer>();
    }
    private void Start()
    {
        gameObject.SetActive(false);
        buttonRenderer.material.color = new Color(1, 1, 1, 0);
    }

    private void Update()
    {
        if (isFading) return;

        if (buttonPressed)
        {
            OnPressedEvents?.Invoke(this.transform.position);
        }
    }

    private void LateUpdate()
    {
        if (isFading) return;

        if (buttonPressed)
        {
            buttonRenderer.material.color = Color.green;
        }
        else
        {
            buttonRenderer.material.color = Color.white;
        }
    }

    public void Press()
    {
        if (buttonPressed) return;
        buttonPressed = true;
    }

    public void Release()
    {
        if (!buttonPressed) return;
        buttonPressed = false;
        OnReleasedEvents?.Invoke();
    }

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    if (eventData.button != 0) return;
    //    buttonPressed = true;
    //}

    //public void OnPointerUp(PointerEventData eventData)
    //{
    //    if (eventData.button != 0) return;
    //    buttonPressed = false;
    //    OnReleasedEvents?.Invoke();
    //}

    /// <summary>
    /// Disables the button's Update and starts the Fade Out routine
    /// </summary>
    public void DisableButton()
    {
        buttonRenderer.material.color = Color.white;
        StartCoroutine(FadeButtonTo(0.0f, fadeTime, true));
    }

    /// <summary>
    /// Starts the Fade In routine
    /// </summary>
    public void EnableButton()
    {
        StartCoroutine(FadeButtonTo(1.0f, fadeTime));
    }

    private IEnumerator FadeButtonTo(float targetAlpha, float duration, bool disableButtonAfterFade = false)
    {
        isFading = true;

        Color arrowColor = buttonRenderer.material.color;
        float startAlpha = buttonRenderer.material.color.a;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            arrowColor.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            buttonRenderer.material.color = arrowColor;
            yield return null;
        }
        buttonRenderer.material.color = arrowColor;

        // Disable the button after the fade out
        isFading = false;
        if (disableButtonAfterFade) gameObject.SetActive(false);

    }
}

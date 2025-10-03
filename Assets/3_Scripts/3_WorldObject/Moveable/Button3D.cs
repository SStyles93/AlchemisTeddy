using System;
using System.Collections;
using UnityEngine;

public class Button3D : MonoBehaviour/*, IPointerDownHandler, IPointerUpHandler*/
{
    [SerializeField] private float fadeTime = 1.0f;

    private MeshRenderer buttonRenderer;
    private BoxCollider buttonCollider;

    private bool isEnabled = false;
    public bool buttonPressed;

    public event Action<Vector3> OnPressedEvents;
    public event Action OnReleasedEvents;

    private Coroutine currentFadeRoutine = null;

    private void Awake()
    {
        buttonRenderer = GetComponent<MeshRenderer>();
        buttonCollider = GetComponent<BoxCollider>();
    }
    private void Start()
    {
        buttonCollider.enabled = false;
        buttonRenderer.material.color = new Color(1, 1, 1, 0);
        buttonRenderer.enabled = false;
    }

    private void Update()
    {
        if (currentFadeRoutine != null) return;
        if (buttonPressed)
        {
            OnPressedEvents?.Invoke(this.transform.localPosition);
            //Debug.Log($"{this.gameObject} is pressed");
        }
    }

    private void LateUpdate()
    {
        if (currentFadeRoutine != null || !isEnabled) return;

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
        if (!isEnabled) return;
        buttonRenderer.material.color = Color.white;
        if (currentFadeRoutine != null)
            StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(FadeOutButton());
        isEnabled = false;
    }

    /// <summary>
    /// Starts the Fade In routine
    /// </summary>
    public void EnableButton()
    {
        if (isEnabled) return;
        if (currentFadeRoutine != null)
            StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(FadeInButton());
        isEnabled = true;
    }

    private IEnumerator FadeButtonTo(float targetAlpha, float duration)
    {
        Color buttonColor = buttonRenderer.material.color;
        float startAlpha = buttonRenderer.material.color.a;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            buttonColor.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            buttonRenderer.material.color = buttonColor;
            yield return null;
        }
        buttonRenderer.material.color = buttonColor;

        currentFadeRoutine = null;
    }

    private IEnumerator FadeInButton()
    {
        buttonRenderer.enabled = true;
        buttonCollider.enabled = true;
        yield return FadeButtonTo(1.0f, fadeTime);
    }

    private IEnumerator FadeOutButton()
    {
        buttonCollider.enabled = false;
        yield return FadeButtonTo(0.0f, fadeTime);
        buttonRenderer.enabled = false;
    }
}

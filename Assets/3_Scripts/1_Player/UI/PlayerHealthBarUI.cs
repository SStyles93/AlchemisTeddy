using UnityEngine;
using UnityEngine.UI;// Required for Slider
using System.Collections;

[RequireComponent(typeof(Slider))]
public class PlayerHealthBarUI : MonoBehaviour
{
    [Header("Health Fill Area Parameters")]
    [SerializeField] private Image healthImage;
    [SerializeField] private Gradient healthGradient = new Gradient();

    [Header("Fade Parameters")]
    [Tooltip("How long the health bar stays fully visible before starting to fade.")]
    [SerializeField] private float displayTime = 3f;
    [Tooltip("How quickly the health bar fades out after the display time is over.")]
    [SerializeField] private float fadeSpeed = 2f;

    private Slider healthSlider;
    private CanvasGroup canvasGroup; // Reference to the CanvasGroup
    private Coroutine fadeCoroutine; // Reference to the active fade coroutine

    private void Awake()
    {
        // Get the Slider component.
        healthSlider = GetComponent<Slider>();
        
        canvasGroup = GetComponent<CanvasGroup>();
        // Start with the health bar hidden.
        canvasGroup.alpha = 0;
    }

    // Subscribe to the event when this component is enabled.
    private void OnEnable()
    {
        PlayerHealth.OnHealthChanged += UpdateHealthBar;
    }

    // Unsubscribe when the component is disabled to prevent errors.
    private void OnDisable()
    {
        PlayerHealth.OnHealthChanged -= UpdateHealthBar;
    }

    /// <summary>
    /// This method is called by the OnHealthChanged event.
    /// It receives the new health values and updates the slider.
    /// </summary>
    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        // The slider's value is a normalized value (between 0 and 1).
        // We calculate this by dividing the current health by the max health.
        // We must cast one of them to a float to avoid integer division (which would result in 0 or 1).
        healthSlider.value = currentHealth / maxHealth;
        healthImage.color = healthGradient.Evaluate(healthSlider.value);

        DisplayHealthBar();
    }

    /// <summary>
    /// Makes the health bar visible and starts the fade-out timer.
    /// </summary>
    private void DisplayHealthBar()
    {
        // If a fade-out is already in progress, stop it.
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // Make the health bar instantly visible.
        canvasGroup.alpha = 1f;

        // Start a new coroutine to handle the waiting and fading.
        fadeCoroutine = StartCoroutine(FadeOutRoutine());
    }


    /// <summary>
    /// A coroutine that waits for a set time, then smoothly fades the health bar out.
    /// </summary>
    private IEnumerator FadeOutRoutine()
    {
        // 1. Wait for the specified display time.
        yield return new WaitForSeconds(displayTime);

        // 2. Smoothly fade the alpha from 1 to 0.
        float currentAlpha = 1f;
        while (currentAlpha > 0)
        {
            // Decrease alpha over time, controlled by fadeSpeed.
            currentAlpha -= Time.deltaTime * fadeSpeed;
            canvasGroup.alpha = currentAlpha;

            // Wait for the next frame before continuing the loop.
            yield return null;
        }

        // 3. Ensure alpha is exactly 0 at the end.
        canvasGroup.alpha = 0;
        fadeCoroutine = null; // Clear the coroutine reference.
    }
}
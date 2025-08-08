using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Manages a world-space health bar with a simple two-object hierarchy.
/// It controls fill amount, color, visibility, and always faces the camera.
/// This version incorporates user corrections for camera facing, anchor logic,
/// and re-implements the fade-out feature.
/// </summary>
public class WorldHealthBar : MonoBehaviour
{
    public enum FillAnchor
    {
        Left,
        Right,
        Center
    }

    [Header("Visual Components")]
    [Tooltip("The transform of the fill quad (should be a child of this object).")]
    [SerializeField] private Transform healthFillTransform;
    [Tooltip("The renderer for the fill quad.")]
    [SerializeField] private Renderer healthFillRenderer;

    [Header("Text Component")]
    [Tooltip("The TextMeshPro component used to display health values.")]
    [SerializeField] private TextMeshPro healthText;


    [Header("Visual Style")]
    [Tooltip("Determines the direction the health bar will fill from.")]
    [SerializeField] private FillAnchor fillAnchor = FillAnchor.Left;
    [Tooltip("The gradient used to color the health bar, from 0 (empty) to 1 (full).")]
    [SerializeField] private Gradient healthGradient;
    //[Tooltip("The color of the background quad.")]
    //[SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.5f);

    [Header("Animation & Timing")]
    [Tooltip("How long the health bar stays fully visible after a change.")]
    [SerializeField] private float displayDuration = 3f;
    [Tooltip("How quickly the health bar fades out after the display duration.")]
    [SerializeField] private float fadeSpeed = 2f;
    [Tooltip("How quickly the health bar's fill animates down when damage is taken.")]
    [SerializeField] private float fillAnimationSpeed = 1.5f;

    // --- Private State ---

    private MaterialPropertyBlock propertyBlock;
    private Coroutine displayCoroutine;
    private Coroutine fillCoroutine;
    private float targetFillAmount = 1f;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        // If the background renderer isn't assigned, try to get it from this component.
        if (healthText == null)
        {
            healthText = GetComponentInChildren<TextMeshPro>();
        }
        // Start with the health bar invisible.
        SetAlpha(0);
    }

    public void OnHealthChanged(float normalizedHealth)
    {
        ShowAndBeginFade();
        targetFillAmount = Mathf.Clamp01(normalizedHealth);
        if (fillCoroutine == null)
        {
            fillCoroutine = StartCoroutine(AnimateFillRoutine());
        }
    }

    private void ShowAndBeginFade()
    {
        if (displayCoroutine != null) StopCoroutine(displayCoroutine);
        displayCoroutine = StartCoroutine(DisplayAndFadeRoutine());
    }

    /// <summary>
    /// A coroutine that makes the bar visible, waits, then fades it out.
    /// </summary>
    private IEnumerator DisplayAndFadeRoutine()
    {
        SetAlpha(1f); // Make it fully visible
        yield return new WaitForSeconds(displayDuration);

        float currentAlpha = 1f;
        while (currentAlpha > 0)
        {
            currentAlpha -= Time.deltaTime * fadeSpeed;
            SetAlpha(currentAlpha);
            yield return null;
        }
        SetAlpha(0); // Ensure it's fully transparent
        displayCoroutine = null;
    }

    private IEnumerator AnimateFillRoutine()
    {
        float currentFill = healthFillTransform.localScale.x;
        while (!Mathf.Approximately(currentFill, targetFillAmount))
        {
            currentFill = Mathf.MoveTowards(currentFill, targetFillAmount, Time.deltaTime * fillAnimationSpeed);
            UpdateFill(currentFill);
            yield return null;
        }
        UpdateFill(targetFillAmount);
        fillCoroutine = null;
    }

    private void UpdateFill(float normalizedValue)
    {
        if (healthFillTransform == null) return;

        healthFillTransform.localScale = new Vector3(normalizedValue, healthFillTransform.localScale.y, 1f);

        float positionOffset = 0f;
        float shiftAmount = (1f - normalizedValue) / 2f;

        switch (fillAnchor)
        {
            case FillAnchor.Right:
                positionOffset = shiftAmount;
                break;

            case FillAnchor.Left:
                positionOffset = -shiftAmount;
                break;

            case FillAnchor.Center:
                positionOffset = 0f;
                break;
        }

        healthFillTransform.localPosition = new Vector3(positionOffset, 0, 0.01f);
        UpdateColor(healthFillTransform.localScale.x);
    }

    /// <summary>
    /// Updates the color of the fill based on the gradient.
    /// This is now separate from SetAlpha to be called during the fill animation.
    /// </summary>
    private void UpdateColor(float normalizedValue)
    {
        if (healthFillRenderer != null)
        {
            healthFillRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", healthGradient.Evaluate(normalizedValue));
            healthFillRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    /// <summary>
    /// A method to update the health text.
    /// </summary>
    public void UpdateText(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            // Format the string to show "current / max".
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
    }

    /// <summary>
    /// Sets the alpha for both the background and fill renderers.
    /// </summary>
    private void SetAlpha(float alpha)
    {
        if (healthFillRenderer != null)
        {
            //ensure the renderer is enabled
            if (alpha >= 1) healthFillRenderer.enabled = true;
            //ensure the renderer is disabled
            if (alpha <= 0) healthFillRenderer.enabled = false;

            //OR
            
            //backgroundRenderer.enabled = alpha >= 1f ? true : alpha > 0f ? true : false;

            healthFillRenderer.GetPropertyBlock(propertyBlock);
            Color fillColor = healthGradient.Evaluate(healthFillTransform.localScale.x);
            fillColor.a = alpha;
            propertyBlock.SetColor("_Color", fillColor);
            healthFillRenderer.SetPropertyBlock(propertyBlock);
        }

        if(healthText != null)
        {
            //ensure the renderer is enabled
            if (alpha >= 1) healthText.enabled = true;
            //ensure the renderer is disabled
            if (alpha <= 0) healthText.enabled = false;

            Color fillColor = Color.black;
            fillColor.a = alpha;
            healthText.color = fillColor;
        }
    }
}

using UnityEngine;

public class DamageableEntityUIConnector : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private WorldHealthBar healthBar;

    // The color field is no longer needed here, as it's controlled by the gradient
    // on the WorldHealthBar itself.

    private IDamageable damageable;

    private void Awake()
    {
        damageable = GetComponent<IDamageable>();
    }

    private void Start()
    {
        if (damageable != null && healthBar != null)
        {
            damageable.OnHealthChanged += OnEntityHealthChanged;
            // We can optionally call it once at the start to set the initial health.
            // This requires the entity to have its health value ready in Awake/Start.
        }
    }

    private void OnDestroy()
    {
        if (damageable != null)
        {
            damageable.OnHealthChanged -= OnEntityHealthChanged;
        }
    }

    /// <summary>
    /// This method is called by the event from the IDamageable entity.
    /// </summary>
    private void OnEntityHealthChanged(int currentHealth, int maxHealth)
    {
        if (healthBar == null) return;

        // Calculate the normalized health and pass it to the health bar.
        float normalizedHealth = (maxHealth > 0) ? (float)currentHealth / maxHealth : 0;
        //Update healthBar 
        healthBar.OnHealthChanged(normalizedHealth);
        //Update healthBar Text
        healthBar.UpdateText(currentHealth, maxHealth);
    }
}

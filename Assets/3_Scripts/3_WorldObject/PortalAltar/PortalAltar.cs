using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PortalAltar : Station
{
    [Header("Portal Parts")]
    [SerializeField]
    private Portal portal = null;
    private float interactionSafeguardTimer = 1.0f;


    private void Awake()
    {
        if (interactableCollider == null)
            interactableCollider = GetComponent<BoxCollider>();
        if (portal == null)
            portal = GetComponentInChildren<Portal>();
    }

    private void Update()
    {
        if (interactionSafeguardTimer > 0) interactionSafeguardTimer -= Time.deltaTime;
    }

    public override void Activate(GameObject activator)
    {
        if (interactionSafeguardTimer > 0) return;

        InventoryUI playerUI = activator.GetComponentInChildren<InventoryUI>(true);
        PlayerInventoryManager playerInventory = activator.GetComponent<PlayerInventoryManager>();

        // Check for dependencies first to avoid errors.
        if (playerUI == null || playerInventory == null)
        {
            Debug.LogError($"Activator {activator.name} is missing an InventoryUI or InventoryManager component!");
            return;
        }

        if (currentItem == null)
        {
            // If the station is EMPTY, open the inventory in selection mode.
            // We tell the UI which station is asking for an item.
            playerUI.OpenForSelection(this);
        }
        else
        {
            if (currentItem == null) return;
            // If the station is FULL, Delete the WorldItem
            Destroy(currentWorldItem);
            currentWorldItem = null;
            Debug.Log($"Removed {currentItem.itemName} on holder {gameObject.name}.");
            // Nullify the Item
            currentItem = null;

            //Close portal
            portal.Close();
        }
    }

    /// <summary>
    /// Places an item on this station.
    /// </summary>
    public override void PlaceItem(ItemData item, PlayerInventoryManager placerInventory = null)
    {
        // We should only accept items defined by the Type.
        if (item.itemType == ItemType.Orb)
        {
            currentItem = item;
            Debug.Log($"Placed {item.itemName} on holder {gameObject.name}.");

            // Update visual model.
            if (currentWorldItem != null) { Destroy(currentWorldItem); currentWorldItem = null; }
            currentWorldItem = Instantiate(currentItem.prefab, worldItemPosition.transform.position, Quaternion.identity, worldItemPosition.transform);
            currentWorldItem.GetComponent<WorldItem>().enabled = false;
            currentWorldItem.GetComponent<Rigidbody>().isKinematic = true;
            currentWorldItem.layer = 0;


            PortalOrb portalOrb = (PortalOrb)currentItem;
            
            // Open Portal and Give the OrbData to it
            portal.Open(portalOrb);
        }
        else
        {
            Debug.LogWarning($"{item.name} is not an Orb and cannot be placed here.");
        }
    }

    private void OnDrawGizmos()
    {
        if (interactableCollider == null)
            interactableCollider = GetComponent<BoxCollider>();

        Gizmos.color = (currentItem != null) ? Color.cyan : Color.gray;
        Gizmos.DrawWireCube(transform.position + interactableCollider.center, interactableCollider.size);

        if (currentItem != null)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up, currentItem.itemName);
#endif
        }
    }

}

using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

public class PlayerInteractor : MonoBehaviour
{
    public float interactionDistance = 5f;

    private List<Interactable> interactableCache = new();
    private Player player;

    void Awake()
    {
        player = GetComponent<Player>();
    }

    private void FixedUpdate()
    {
        CacheNearbyInteractables();
    }

    private void Update()
    {
        ShowInteractUI();
    }

    private bool TryInteract()
    {
        if (interactableCache.Count == 0) return false;

        Interactable closestInteractable = GetClosestInteractable();
        if (closestInteractable != null)
        {
            if (Vector3.Distance(transform.position, closestInteractable.transform.position) > interactionDistance) return false;
            closestInteractable.OnInteract(player);
            return true;
        }
        return false;
    }

    private void OnInteract(InputValue value)
    {
        if (!TryInteract())
        {
            // Drop Pollen if the player is trying to interact but no interactable is found
            if (player.HasPollen)
            {
                //player.HasPollen = false;
                //Instantiate(player.PollenPrefab, transform.position - Vector3.up * 0.5f, Quaternion.identity);
            }
        }
    }

    private Interactable GetClosestInteractable()
    {
        Interactable closestInteractable = null;
        float closestDistance = float.MaxValue;

        foreach (Interactable interactable in interactableCache)
        {
            if (interactable == null) continue;
            float distance = Vector3.Distance(transform.position, interactable.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestInteractable = interactable;
            }
        }

        return closestInteractable;
    }

    private void ShowInteractUI()
    {
        if (interactableCache.Count == 0)
        {
            UIManager.Instance.HideInteractText();
            return;
        }

        var closest = GetClosestInteractable();
        if (closest == null)
        {
            UIManager.Instance.HideInteractText();
            return;
        }

        if (Vector3.Distance(transform.position, closest.transform.position) > interactionDistance)
        {
            UIManager.Instance.HideInteractText();
            return;
        }


        if (closest.WillShowPhrase(player))
        {
            UIManager.Instance.ShowInteractText(closest.InteractablePhrase);
        }
    }

    private void CacheNearbyInteractables()
    {
        interactableCache.Clear();

        // Cache nearby interactables
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactionDistance);
        foreach (Collider collider in colliders)
        {
            if (collider is null) continue;

            Interactable interactable = collider.GetComponent<Interactable>();
            if (interactable is null) continue;
            if (interactable is not null && interactableCache.Contains(interactable)) continue;

            interactableCache.Add(interactable);
        }
    }
}

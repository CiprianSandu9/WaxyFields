using UnityEngine;

public class PollenInteractable : Interactable
{
    public override void OnInteract(Player player)
    {
        base.OnInteract(player);

        GameManager.Instance.GameStats.CarriedPollen++;
        player.HasPollen = true;

        Destroy(gameObject.transform.parent.gameObject);
    }
}

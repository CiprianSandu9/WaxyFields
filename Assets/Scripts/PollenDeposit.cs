using UnityEngine;

public class PollenDeposit : Interactable
{
    public override void OnInteract(Player player)
    {
        base.OnInteract(player);
        if (player.HasPollen)
        {
            player.HasPollen = false;
            GameManager.Instance.GameStats.DepositedPollen += GameManager.Instance.GameStats.CarriedPollen;
            GameManager.Instance.GameStats.CarriedPollen = 0;
            UIManager.Instance.ShowDepositedText();
        }
    }

    public override bool WillShowPhrase(Player player)
    {
        return player.HasPollen;
    }
}

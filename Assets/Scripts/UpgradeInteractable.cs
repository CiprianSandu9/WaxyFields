using UnityEngine;

public class UpgradeInteractable : Interactable
{
    public enum StatType
    {
        Health,
        SprintSpeed,
        Speed
    }

    public StatType statType;
    public float healthIncreaseAmount = 5f;
    public float speedIncreaseAmount = .02f;
    public float sprintIncreaseAmount = .02f;

    public override void OnInteract(Player player)
    {
        base.OnInteract(player);

        if (GameManager.Instance.GameStats.DepositedPollen <= 0)
        {
            UIManager.Instance.ShowNotEnoughPollenText();
            return;
        }

        if (statType == StatType.Speed)
        {
            GameManager.Instance.GameStats.SpeedModfier += speedIncreaseAmount;
        }

        if (statType == StatType.SprintSpeed)
        {
            GameManager.Instance.GameStats.SprintSpeedModifier += sprintIncreaseAmount;
        }

        if (statType == StatType.Health)
        {
            GameManager.Instance.GameStats.MaxHealth += (int)healthIncreaseAmount;
            player.MaxHealth = GameManager.Instance.GameStats.MaxHealth;
            player.Health = GameManager.Instance.GameStats.MaxHealth; // Reset health to max when upgrading health
        }

        GameManager.Instance.GameStats.DepositedPollen--;

    }

}

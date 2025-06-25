using UnityEngine;

public class TurnBackVolume : MonoBehaviour
{
    private Player player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>() != null)
        {
            player = other.GetComponent<Player>();
            UIManager.Instance.ShowTurnBackText();
            InvokeRepeating(nameof(HurtPlayer), 0f, 2f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player>() != null && player != null)
        {
            UIManager.Instance.HideTurnBackText();
            player = null;
        }
    }

    private void HurtPlayer()
    {
        if (player != null)
        {
            player.Damage(5); // Assuming TakeDamage is a method in Player class
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class HiveInteractable : Interactable
{
    public override void OnInteract(Player player)
    {
        if (!FlowersLeft())
        {
            UIManager.Instance.ShowYouWinObject();
            return;
        }
        GameManager.Instance.EnterBeehive();
    }

    private bool FlowersLeft()
    {
        var flowers = GameObject.FindObjectsByType<PollenInteractable>(FindObjectsSortMode.None);
        return flowers.Length > 0;
    }
}

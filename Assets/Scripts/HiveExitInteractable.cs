using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
    public class HiveExitInteractable : Interactable
    {
        public override void OnInteract(Player player)
        {
            GameManager.Instance.ExitBeehive();
        }
    }
}
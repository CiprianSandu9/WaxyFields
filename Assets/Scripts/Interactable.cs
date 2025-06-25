using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string InteractablePhrase;

    public virtual void OnInteract(Player player)
    {
        SingleShotAudioManager.Instance.PlayCollectSound();
    }

    public virtual bool WillShowPhrase(Player player) => true;
}

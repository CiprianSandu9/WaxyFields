using TMPro;
using UnityEngine;

public class DeathScreen : MonoBehaviour
{
    public TMP_Text pollenText;

    public void Update()
    {
        pollenText.text = "COLLECTED " + GameManager.Instance.GameStats.DepositedPollen.ToString() + " POLLEN";
    }
}

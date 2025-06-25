using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TMP_Text hpText;
    public TMP_Text staminaText;
    public TMP_Text outOfStaminaText;
    public TMP_Text turnBackText;
    public TMP_Text interactText;
    public TMP_Text depositConfirmationText;
    public TMP_Text depositedPollenText;
    public TMP_Text carriedPollenText;
    public TMP_Text notEnoughPollenText;
    public GameObject youDiedObject;
    public GameObject youWinObject;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowInteractText(string text)
    {
        interactText.text = "[E] " + text;
        interactText.gameObject.SetActive(true);
    }

    public void HideInteractText()
    {
        interactText.gameObject.SetActive(false);
    }

    public void ShowTurnBackText()
    {
        turnBackText.gameObject.SetActive(true);
    }

    public void HideTurnBackText()
    {
        turnBackText.gameObject.SetActive(false);
    }

    public void ShowOutOfStaminaText()
    {
        outOfStaminaText.gameObject.SetActive(true);
        Invoke(nameof(HideOutOfStaminaText), 2f); // Hide after 2 seconds
    }

    public void UpdateHPText(int hp)
    {
        hpText.text = hp.ToString() + " HP";
    }

    public void UpdateStaminaText(float stamina)
    {
        staminaText.text = ((int)stamina).ToString() + " Stamina";
    }

    public void UpdateCarriedPollenText(int pollen)
    {
        carriedPollenText.text = "POLLEN CARRIED: " + pollen.ToString();
    }

    public void UpdateDepositedPollenText(int pollen)
    {
        depositedPollenText.text = "POLLEN DEPOSITED: " + pollen.ToString();
    }

    private void HideOutOfStaminaText()
    {
        outOfStaminaText.gameObject.SetActive(false);
    }

    public void ShowDepositedText()
    {
        depositConfirmationText.gameObject.SetActive(true);
        Invoke(nameof(HideDepositedText), 1f); // Hide after 2 seconds
    }

    public void HideDepositedText()
    {
        depositConfirmationText.gameObject.SetActive(false);
    }

    public void ShowNotEnoughPollenText()
    {
        notEnoughPollenText.gameObject.SetActive(true);
        Invoke(nameof(HideNotEnoughPollenText), 1f); // Hide after 2 seconds
    }

    public void HideNotEnoughPollenText()
    {
        notEnoughPollenText.gameObject.SetActive(false);
    }

    public void ShowYouWinObject()
    {
        youWinObject.SetActive(true);
    }

    public void ShowYouDiedText()
    {
        youDiedObject.SetActive(true);
    }

    public void HideYouDiedText()
    {
        youDiedObject.SetActive(false);
    }
}

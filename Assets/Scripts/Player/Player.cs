using System;
using UnityEngine;

public class Player : Entity
{
    [Header("References")]
    public GameObject PollenModel;
    public GameObject PollenPrefab;
    public Rigidbody Rigidbody;

    private static Player _localPlayer;
    public static Player LocalPlayer
    {
        get => _localPlayer;
        set
        {
            if (_localPlayer != null)
            {
                Debug.LogWarning("Local player is already set. Overriding with new player instance.");
            }
            _localPlayer = value;
        }
    }

    public bool HasPollen
    {
        get => _hasPollen;
        set
        {
            _hasPollen = value;
            UpdateModelVisuals();
        }
    }
    private bool _hasPollen = false;

    public bool IsHiding
    {
        get => _isHiding;
        set
        {
            _isHiding = value;
        }
    }
    private bool _isHiding = false;

    private bool isInvulnerable = false;
    private bool isEvading = false;

    public new void Awake()
    {
        base.Awake();
        LocalPlayer = this;
    }

    private void UpdateModelVisuals()
    {
        PollenModel.SetActive(_hasPollen);
    }

    private void Update()
    {
        UIManager.Instance.UpdateHPText(Health);
        UIManager.Instance.UpdateStaminaText(Stamina);

        if (GameManager.Instance == null) return;
        UIManager.Instance.UpdateCarriedPollenText(GameManager.Instance.GameStats.CarriedPollen);
        UIManager.Instance.UpdateDepositedPollenText(GameManager.Instance.GameStats.DepositedPollen);

        if (GetComponent<PlayerController>()._isEvading) 
        { 
            isEvading = true;
            Invoke(nameof(StopEvading), 1f);
        }

    }

    override public void OnDeath()
    {
        base.OnDeath();

        GameManager.Instance.EndGame();
    }

    public override void Damage(int damage) 
    {
        if (IsDead || isInvulnerable || isEvading) return;
        Health -= damage;
        if (IsDead)
        {
            Health = 0;
            OnDeath();
        }
        isInvulnerable = true;
        Invoke(nameof(MakeVulnerableAgain), .5f);
    }

    private void MakeVulnerableAgain()
    {
        isInvulnerable = false;
    }

    private void StopEvading()
    {
        isEvading = false;
    }
}

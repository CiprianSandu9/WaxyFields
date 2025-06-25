using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class EnemyController : Entity
{
    [Header("Settings")]
    public float Acceleration = 100.0f;
    public float MaxSpeed = 10.0f;
    public float RotationSpeed = 5.0f;
    public float FollowDistance = 5.0f;
    public float AttackDistance = 3f;
    public int DamageAmount = 50;

    private Vector3 moveDir;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    private bool TrackingPlayer = false;
    private GameObject TrackedPlayer;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        if (TrackedPlayer)
        {
            TrackingPlayer = !TrackedPlayer.GetComponent<Player>().IsHiding;
        }

        if (TrackingPlayer)
        {
            moveDir = Player.LocalPlayer.transform.position - transform.position;
            if (moveDir.magnitude < FollowDistance) moveDir = Vector3.zero;
            if (moveDir == Vector3.zero) return;
            transform.forward = Vector3.Lerp(transform.forward, moveDir, Time.deltaTime * 5f);

            float dist = Vector3.Distance(transform.position, Player.LocalPlayer.transform.position);
            if (dist < AttackDistance)
            {
                HandleDamage();
            }
        }
        else { moveDir = Vector3.zero; }
    }

    void FixedUpdate()
    {
        // Let Physics system handle acceleration/damping
        rb.AddForce(moveDir * Acceleration);
        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, MaxSpeed);
    }

    void HandleDamage()
    {
        if (Player.LocalPlayer.IsDead) return;

        // Deal damage to the player
        Player.LocalPlayer.Damage(DamageAmount);

        var dir = Player.LocalPlayer.transform.position - transform.position;
        Player.LocalPlayer.Rigidbody.AddForce(dir * 50, ForceMode.Impulse);

        SingleShotAudioManager.Instance.PlayHurtSound();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            TrackingPlayer = true;
            TrackedPlayer = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            TrackingPlayer = false;
            TrackedPlayer = null;
        }
    }
}


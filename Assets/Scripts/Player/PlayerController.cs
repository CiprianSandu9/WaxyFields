using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform CameraTransform;
    public Transform Body; // For visual tilting/animation
    public Transform Head; // For visual tilting/animation


    [Header("Movement Settings")]
    public float ForwardAcceleration = 75.0f;
    public float SidewaysAcceleration = 50.0f; // For strafing left/right
    public float UpDownAcceleration = 60.0f; // For ascending/descending
    public float SprintMultiplier = 2.0f; // Multiplier for acceleration and max speed when sprinting

    public float MaxForwardSpeed = 20.0f;
    public float MaxSidewaysSpeed = 15.0f;
    public float MaxUpDownSpeed = 15.0f;

    [Header("Rotation Settings")]
    public float YawSpeed = 5.0f; // Left/right rotation (turning to align with camera)
    public float RollSpeed = 8.0f; // Barrel roll/tilt (visual)
    public float CameraPitchSpeed = 3.0f; // For controlling camera's vertical look (used by camera script)
    public float EvadeRotationSpeed = 0.0f; // How fast player yaws to face evade direction

    [Header("Evade Settings")]
    public float EvadeForceMagnitude = 200f; // Increased for a noticeable dodge
    public float EvadeDuration = 0.3f; // Shorter, snappier evade
    public float EvadeCooldown = 0.5f; // Prevent spamming evade

    [Header("Stamina Settings")]
    private float MaxStamina = 100.0f;
    public float EvadeStaminaCost = 30.0f;
    public float StaminaRegenRate = 10.0f; // Stamina per second

    // Internal state
    [NonSerialized]
    public Rigidbody rb;
    private SphereCollider sphereCollider;

    private float oscillationStartTime;
    private float lastEvadeTime; // For cooldown

    private Vector2 _moveInput; // Combined X and Z input
    private float _verticalInput; // Y input for up/down
    [NonSerialized]
    public Vector2 _lookInput; // X and Y for camera/creature look

    private bool _isSprinting = false;
    public bool _isEvading = false;

    // Visual tilting
    private Quaternion startTiltBody, startTiltHead;
    private float currentRollTilt = 0f;

    public float CurrentStamina { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearDamping = 2.0f; // Tune this!
            rb.angularDamping = 5.0f; // Tune this!
        }
        else
        {
            Debug.LogError("Rigidbody not found on this GameObject. Please add a Rigidbody component.", this);
            enabled = false;
        }

        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            Debug.LogWarning("SphereCollider not found on this GameObject. Consider adding one for flying creatures.", this);
        }

        Cursor.lockState = CursorLockMode.Locked;
        CurrentStamina = MaxStamina;

        if (Body != null) startTiltBody = Body.localRotation;
        if (Head != null) startTiltHead = Head.localRotation;


    }

    private void Start()
    {
        MaxStamina = gameObject.GetComponent<Player>().MaxStamina;
    }



    void Update()
    {
        if (_isSprinting)
        {
            if (CurrentStamina > 0)
                CurrentStamina -= 10f * Time.deltaTime;
            else
                CurrentStamina = 0;
        }

        HandleStaminaRegen();
        gameObject.GetComponent<Player>().Stamina = CurrentStamina;
        HandleVisualTilt(); // Visual only, independent of primary rotation

        // TODO: NEEDS DEBUGGING!!!
        if (_isEvading)
        {
            HandleEvadeRotation(); // During evade, face the movement direction
        }
        else
        {
            HandlePlayerAlignmentWithCamera(); // Otherwise, align with camera
        }
    }

    void FixedUpdate()
    {
        if (!_isEvading)
        {
            HandleMovementForces();
        }
        else
        {
            HandleEvadeLogic();
        }

        ClampVelocity();
    }

    private void HandleStaminaRegen()
    {
        if (!_isSprinting && !_isEvading && CurrentStamina < MaxStamina)
        {
            CurrentStamina += StaminaRegenRate * Time.deltaTime;
            CurrentStamina = Mathf.Min(CurrentStamina, MaxStamina);
        }
    }

    private void HandlePlayerAlignmentWithCamera()
    {
        // Align player's yaw (Y-axis) with camera's yaw
        Quaternion targetYawRotation = Quaternion.Euler(0, CameraTransform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetYawRotation, Time.deltaTime * YawSpeed);

        // Align player's pitch (X-axis) with camera's pitch
        // Get the camera's forward, but flatten it to get just its horizontal direction
        Vector3 cameraFlatForward = new Vector3(CameraTransform.forward.x, 0, CameraTransform.forward.z).normalized;
        // Then get the camera's actual forward, including vertical component
        Vector3 cameraFullForward = CameraTransform.forward;

        // Create a target rotation that combines the creature's current yaw with the camera's pitch.
        // This makes the creature point its nose up/down with the camera, while its overall
        // horizontal turning is handled by the slerp above.
        Quaternion targetPitchRotation = Quaternion.LookRotation(cameraFullForward, Vector3.up);

        // Slerp only the pitch component by directly setting transform.forward to the camera's full forward
        // but preserving the creature's current yaw (which is aligned above)
        transform.forward = Vector3.Slerp(transform.forward, cameraFullForward, Time.deltaTime * YawSpeed);
        // Note: For finer control on pitch, you might extract Pitch and apply separately,
        // but often directly interpolating transform.forward is good enough.
    }

    private void HandleEvadeRotation()
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        // Only rotate if there's significant horizontal movement
        if (horizontalVelocity.sqrMagnitude > 0.1f) // Use a small threshold to avoid erratic rotation at near-zero velocity
        {
            // Calculate the target yaw (Y-axis rotation) based on the horizontal velocity
            Quaternion targetYaw = Quaternion.LookRotation(horizontalVelocity.normalized, Vector3.up);

            // Get the current player's yaw (Y-axis rotation)
            Quaternion currentPlayerYaw = Quaternion.Euler(0, transform.eulerAngles.y, 0);

            // Slerp the player's yaw towards the target yaw
            Quaternion blendedYaw = Quaternion.Slerp(currentPlayerYaw, targetYaw, Time.deltaTime * EvadeRotationSpeed);

            // Apply the blended yaw, preserving the player's current pitch (X-axis) and roll (Z-axis)
            // This allows the creature to maintain its vertical orientation from before the evade,
            // while turning its body to face the horizontal dodge direction.
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, blendedYaw.eulerAngles.y, transform.eulerAngles.z);
        }
        // If horizontal velocity is too small, just maintain current rotation or let physics take over.
        // The current implementation maintains it, which is good.
    }

    private void HandleVisualTilt()
    {
        float targetRollTilt = -_moveInput.x * 15f; // Adjust multiplier for desired lean amount
        currentRollTilt = Mathf.Lerp(currentRollTilt, targetRollTilt, Time.deltaTime * RollSpeed);

        if (Body != null)
        {
            Quaternion roll = Quaternion.Euler(0, 0, currentRollTilt);
            Body.localRotation = startTiltBody * roll;
        }
        if (Head != null)
        {
            Head.localRotation = startTiltHead * Quaternion.Euler(0, 0, currentRollTilt / 2f); // Head tilts less
        }
    }

    private void HandleMovementForces()
    {
        float currentForwardAccel = _isSprinting ? ForwardAcceleration * SprintMultiplier * GameManager.Instance.GameStats.SprintSpeedModifier : ForwardAcceleration *
            GameManager.Instance.GameStats.SpeedModfier;
        float currentSidewaysAccel = _isSprinting ? SidewaysAcceleration * SprintMultiplier *
                                                    GameManager.Instance.GameStats.SprintSpeedModifier : SidewaysAcceleration *
            GameManager.Instance.GameStats.SpeedModfier;
        float currentUpDownAccel = _isSprinting ? UpDownAcceleration * SprintMultiplier *
                                                  GameManager.Instance.GameStats.SprintSpeedModifier : UpDownAcceleration *
            GameManager.Instance.GameStats.SpeedModfier;

        if (_moveInput.y != 0)
        {
            rb.AddForce(transform.forward * _moveInput.y * currentForwardAccel, ForceMode.Acceleration);
        }

        if (_moveInput.x != 0)
        {
            rb.AddForce(transform.right * _moveInput.x * currentSidewaysAccel, ForceMode.Acceleration);
        }

        if (_verticalInput != 0)
        {
            rb.AddForce(Vector3.up * _verticalInput * currentUpDownAccel, ForceMode.Acceleration);
        }
    }

    private void HandleEvadeLogic()
    {
        float elapsedTime = Time.time - oscillationStartTime;

        if (elapsedTime < EvadeDuration / 2)
        {
            float normalizedTime = elapsedTime / (EvadeDuration / 2);
            float sineValue = Mathf.Sin(normalizedTime * Mathf.PI * 2); // 2 * PI for one full cycle

            Vector3 oscillationDirection = transform.right * sineValue;

            rb.AddForce(oscillationDirection * EvadeForceMagnitude, ForceMode.VelocityChange);
        }
        else if (elapsedTime < EvadeDuration)
        {
            float normalizedTime = elapsedTime / (EvadeDuration / 2) - 1;
            float sineValue = Mathf.Sin(normalizedTime * Mathf.PI * 2); // 2 * PI for one full cycle

            Vector3 oscillationDirection = -transform.right * sineValue;

            rb.AddForce(oscillationDirection * EvadeForceMagnitude, ForceMode.VelocityChange);
        }
        else
        {
            _isEvading = false;

            Vector3 currentVelocity = rb.linearVelocity;

            // Project current velocity onto the creature's forward direction (now aligned with camera on exit)
            Vector3 forwardVelocity = Vector3.Project(currentVelocity, transform.forward);

            // Project current velocity onto the world's up direction
            Vector3 verticalVelocity = Vector3.Project(currentVelocity, Vector3.up);

            // Discard any lateral (right/left) velocity relative to the creature's orientation
            Vector3 lateralVelocity = Vector3.Project(currentVelocity, transform.right);

            // Reconstruct velocity without the lateral wobble from evade
            rb.linearVelocity = forwardVelocity + verticalVelocity + (currentVelocity - forwardVelocity - verticalVelocity - lateralVelocity);
        }
    }

    private void ClampVelocity()
    {
        float currentMaxForwardSpeed = _isSprinting ? MaxForwardSpeed * SprintMultiplier : MaxForwardSpeed;
        float currentMaxSidewaysSpeed = _isSprinting ? MaxSidewaysSpeed * SprintMultiplier : MaxSidewaysSpeed;
        float currentMaxUpDownSpeed = _isSprinting ? MaxUpDownSpeed * SprintMultiplier : MaxUpDownSpeed;

        float currentForwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        if (Mathf.Abs(currentForwardSpeed) > currentMaxForwardSpeed)
        {
            rb.linearVelocity = rb.linearVelocity - (currentForwardSpeed - Mathf.Sign(currentForwardSpeed) * currentMaxForwardSpeed) * transform.forward;
        }

        float currentSidewaysSpeed = Vector3.Dot(rb.linearVelocity, transform.right);
        if (Mathf.Abs(currentSidewaysSpeed) > currentMaxSidewaysSpeed)
        {
            rb.linearVelocity = rb.linearVelocity - (currentSidewaysSpeed - Mathf.Sign(currentSidewaysSpeed) * currentMaxSidewaysSpeed) * transform.right;
        }

        float currentUpDownSpeed = Vector3.Dot(rb.linearVelocity, Vector3.up);
        if (Mathf.Abs(currentUpDownSpeed) > currentMaxUpDownSpeed)
        {
            rb.linearVelocity = rb.linearVelocity - (currentUpDownSpeed - Mathf.Sign(currentUpDownSpeed) * currentMaxUpDownSpeed) * Vector3.up;
        }
    }

    // --- Input System ---
    private void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    private void OnVertical(InputValue value)
    {
        _verticalInput = value.Get<float>();
    }

    private void OnLook(InputValue value)
    {
        _lookInput = value.Get<Vector2>();
    }

    void OnSprint(InputValue value)
    {
        _isSprinting = value.isPressed;
    }

    void OnEscape(InputValue value)
    {
        GameManager.Instance.LoadMainMenu();
    }

    void OnEvade(InputValue value)
    {
        // use this to trigger game restart as well
        if (GameManager.Instance.GameEnded)
        {
            GameManager.Instance.RestartGame();
            return;
        }

        if (value.isPressed && !_isEvading && (Time.time > lastEvadeTime + EvadeCooldown))
        {
            if (CurrentStamina >= EvadeStaminaCost)
            {
                oscillationStartTime = Time.time;
                _isEvading = true;
                CurrentStamina -= EvadeStaminaCost;
                lastEvadeTime = Time.time;
            }
            else
            {
                UIManager.Instance.ShowOutOfStaminaText();
            }
        }
    }
}
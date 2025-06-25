using UnityEngine;

public class SpringPart : MonoBehaviour
{
    public Transform parent;
    public Rigidbody parentRb;
    public float springStrength = 20f;
    public float damping = 0.75f;
    public float maxOffset = 0.5f;

    private Vector3 velocity;
    private Vector3 restLocalPosition;

    void Start()
    {
        restLocalPosition = transform.localPosition;
        if (!parent) parent = transform.parent;
        if (!parentRb) parentRb = parent.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector3 parentVel = parentRb.linearVelocity;

        // Localize the velocity to the parent's local space
        Vector3 localLagDir = -parent.InverseTransformDirection(parentVel.normalized);
        float lagAmount = Mathf.Clamp(parentVel.magnitude * 0.05f, 0, maxOffset);

        // Compute the dynamic target position
        Vector3 targetLocalPos = restLocalPosition + localLagDir * lagAmount;

        Vector3 toTarget = targetLocalPos - transform.localPosition;
        velocity += toTarget * springStrength * Time.fixedDeltaTime;
        velocity *= damping;
        transform.localPosition += velocity * Time.fixedDeltaTime;
    }
}
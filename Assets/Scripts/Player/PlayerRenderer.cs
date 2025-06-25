using System.Collections.Generic;
using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    [SerializeField] private Material _lineMaterial;
    [SerializeField] private Rigidbody _playerRigidbody;
    public List<MeshRenderer> WingsRenderers = new(); // For visual tilting/animation


    public enum RendererState
    {
        Idle,
        Carrying
    }

    private List<LineRenderer> _antennaeRenderers = new();
    private List<LineRenderer> _armRenderers = new();
    private List<LineRenderer> _legRenderers = new();

    private List<List<Vector3>> _currentAntennaePositions = new();

    private bool initialisedTargets = false;

    void Start()
    {
        InitRenderers();

        if (WingsRenderers.Count > 0)
        {
            InvokeRepeating(nameof(WingsOn), 0f, 0.1f);
            InvokeRepeating(nameof(WingsOff), 0.04f, 0.1f); // Offset for strobe effect
        }
    }

    void WingsOn()
    {
        foreach (var renderer in WingsRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true; // Enable wings
            }
        }
    }

    void WingsOff()
    {
        foreach (var renderer in WingsRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = false; // Disable wings
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRenderers();
    }


    void InitRenderers()
    {
        for (int i = 0; i < 2; i++)
        {
            // Antennae
            var antennae = new GameObject("Antennae" + i);
            antennae.transform.SetParent(transform);

            var lr = antennae.AddComponent<LineRenderer>();
            lr.startWidth = .08f;
            lr.endWidth = .08f;
            lr.positionCount = 3;
            lr.useWorldSpace = true;
            lr.material = _lineMaterial;

            _antennaeRenderers.Add(lr);

            // Arms
            var arm = new GameObject("Arm" + i);
            arm.transform.SetParent(transform);

            lr = arm.AddComponent<LineRenderer>();
            lr.startWidth = .1f;
            lr.endWidth = .1f;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.material = _lineMaterial;

            _armRenderers.Add(lr);

            // Legs
            var leg = new GameObject("Leg" + i);
            leg.transform.SetParent(transform);

            lr = leg.AddComponent<LineRenderer>();
            lr.startWidth = .1f;
            lr.endWidth = .1f;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.material = _lineMaterial;

            _legRenderers.Add(lr);
        }
    }

    void UpdateRenderers()
    {
        const float lerpSpeed = 30;
        var vel = _playerRigidbody.linearVelocity;

        for (int i = 0; i < _antennaeRenderers.Count; i++)
        {
            bool dir = i == 0;
            float spacing = .2f;

            var pos = _playerRigidbody.position + transform.up * .35f;
            pos += transform.forward * 1.2f;
            pos += transform.right * (dir ? -spacing : spacing);

            var endPos = pos;
            endPos += transform.forward * .5f;
            endPos += transform.up * .3f;

            var mid = pos + endPos;
            mid /= 2;
            mid += transform.up * .1f;

            if (!initialisedTargets)
            {
                _currentAntennaePositions.Add(new List<Vector3> { pos, mid, endPos });
            }

            var antennae = _antennaeRenderers[i];
            _currentAntennaePositions[i][0] = pos;

            // Fixed delta time is a shitty fix here - cant figure it out
            _currentAntennaePositions[i][1] = mid;
            _currentAntennaePositions[i][2] = Vector3.Lerp(_currentAntennaePositions[i][2], endPos, lerpSpeed * Time.fixedDeltaTime);

            var smooth = Util.SmoothLine(_currentAntennaePositions[i]).ToArray();
            antennae.positionCount = smooth.Length;
            antennae.SetPositions(smooth);
        }

        /* for (int i = 0; i < _armRenderers.Count; i++)
         {
             bool dir = i == 0;

             var arm = _armRenderers[i];
             arm.SetPosition(0, transform.position);
             arm.SetPosition(1, transform.position + transform.right * 2);
         }

         for (int i = 0; i < _legRenderers.Count; i++)
         {
             bool dir = i == 0;

             var leg = _legRenderers[i];
             leg.SetPosition(0, transform.position);
             leg.SetPosition(1, transform.position - transform.right * 2);
         }*/

        initialisedTargets = true;
    }
}

using UnityEngine;

public class MaskManager : MonoBehaviour
{
    [Header("Mask References")]
    [SerializeField] private Transform phantomMask;
    [SerializeField] private Transform dorianMask;
    [SerializeField] private Transform opheliaMask;

    [Header("Mask Visibility")]
    public bool phantomMaskActive = true;
    public bool dorianMaskActive = true;
    public bool opheliaMaskActive = true;

    [Header("Vertical Bob")]
    [SerializeField] private float verticalBobAmplitude = 0.05f;
    [SerializeField] private float verticalBobSpeed = 1.2f;


    [Header("Orbit Settings")]
    [SerializeField] private float orbitRadius = 0.5f;
    [SerializeField] private float orbitSpeed = 2f;
    [SerializeField] private float heightOffset = 0.8f;

    [Header("Follow Delay")]
    [SerializeField] private float followSmoothTime = 0.15f;

    private Vector3 phantomVelocity;
    private Vector3 dorianVelocity;
    private Vector3 opheliaVelocity;
    private Transform cam;

    private float time;

    private const float TWO_PI = Mathf.PI * 2f;

    private void Awake()
    {
        cam = Camera.main.transform;
    }
    private void LateUpdate()
    {
        time += Time.deltaTime * orbitSpeed;

        if (phantomMask != null)
        {
            phantomMask.gameObject.SetActive(phantomMaskActive);
            if (phantomMaskActive)
                UpdateMask(phantomMask, time, ref phantomVelocity);
        }

        if (dorianMask != null)
        {
            dorianMask.gameObject.SetActive(dorianMaskActive);
            if (dorianMaskActive)
                UpdateMask(
                    dorianMask,
                    time + (TWO_PI / 3f),
                    ref dorianVelocity
                );
        }

        if (opheliaMask != null)
        {
            opheliaMask.gameObject.SetActive(opheliaMaskActive);
            if (opheliaMaskActive)
                UpdateMask(
                    opheliaMask,
                    time + (TWO_PI * 2f / 3f),
                    ref opheliaVelocity
                );
        }
    }

    private void UpdateMask(
        Transform mask,
        float phase,
        ref Vector3 velocity
    )
    {
        float verticalBob = Mathf.Sin(
    (time + phase) * verticalBobSpeed
) * verticalBobAmplitude;

        Vector3 orbitOffset = new Vector3(
            Mathf.Sin(phase) * orbitRadius,
            heightOffset + verticalBob,
            Mathf.Cos(phase) * orbitRadius
        );


        Vector3 targetWorldPos = transform.position + orbitOffset;

        mask.position = Vector3.SmoothDamp(
            mask.position,
            targetWorldPos,
            ref velocity,
            followSmoothTime
        );

        Vector3 lookDir = mask.position - cam.position;
        lookDir.y = 0f; // evita tilt strani se vuoi solo yaw

        if (lookDir.sqrMagnitude > 0.001f)
            mask.rotation = Quaternion.LookRotation(lookDir);

    }
}

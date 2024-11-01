using System.Collections.Generic;
using UnityEditor;  // Required for Handles in the editor
using UnityEngine;
[RequireComponent(typeof(AnimationEventReceiver))]
public class FootEffectHandler : MonoBehaviour
{
    [Header("Speed Controller")]
    [SerializeField,Tooltip("Used to determine what type of controller you want to use for speed that will be checked against the run threshold for running effects")]
    ControllerType controllerType = ControllerType.CharacterController;

    [Header("Footstep Settings")]
    [SerializeField] float distanceToCheckUnderFeet = 2.0f; // Adjusted distance as needed
    [SerializeField] float runThreshold;

    [Header("Foot Transforms")]
    [SerializeField] Transform leftFoot;
    [SerializeField] Transform rightFoot;

    [Header("Libraries")]
    [SerializeField] List<PhysicalSoundLibrary> footStepSoundLibraries = new List<PhysicalSoundLibrary>();
    [SerializeField] List<PhysicalParticleLibrary> footStepParticleLibraries = new List<PhysicalParticleLibrary>();
    Dictionary<PhysicMaterial, PhysicalSoundLibrary> footstepMaterialToSound = new();
    Dictionary<PhysicMaterial, PhysicalParticleLibrary> footstepMaterialToParticle = new();

    [Header("FallbackEffects")]
    [SerializeField] SoundData fallBackFootstepSound;
    [SerializeField] ParticleData fallBackFootstepParticle;

    [Header("Ignored Physic Materials")]
    [Tooltip("List of PhysicMaterials to ignore when detecting footstep sounds.")]
    [SerializeField] List<PhysicMaterial> ignoredMaterials;

    [Header("Debugging")]
    [Tooltip("Enable or disable debug visualization in the editor.")]
    [SerializeField] bool enableDebugVisualization = true;
    [Tooltip("Color of the debug rays.")]
    [SerializeField] Color debugRayColor = Color.red;

    // Array of colors for highlighting different objects in the editor
    Color[] highlightColors = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan };
    Animator anim;
    CharacterController characterController;
    Rigidbody rb;

    // For Default controller type
    private Vector3 lastPosition;
    private float currentSpeed;

    public void Awake()
    {
        switch (controllerType)
        {
            case ControllerType.CharacterController:
                characterController = GetComponent<CharacterController>();
                break;
            case ControllerType.Animator:
                anim = GetComponent<Animator>();
                break;
            case ControllerType.Rigidbody:
                rb = GetComponent<Rigidbody>();
                break;
        }
        
        foreach (var library in footStepSoundLibraries)
        {
            footstepMaterialToSound.Add(library.MaterialToMatch, library);
        }
        foreach (var library in footStepParticleLibraries)
        {
            footstepMaterialToParticle.Add(library.MaterialToMatch, library);
        }
    }
 
    void Update()
    {
        // Track speed for Default controller type
        if (controllerType == ControllerType.Default)
        {
            Vector3 delta = transform.position - lastPosition;
            currentSpeed = delta.magnitude / Time.deltaTime;
            lastPosition = transform.position;
        }
    }

    // Call these methods via Animation Events
    public void HandleLeftFoot()
    {
        HandleFootstep(leftFoot);
    }

    public void HandleRightFoot()
    {
        HandleFootstep(rightFoot);
    }

    private void HandleFootstep(Transform foot)
    {
        float radius = 0.1f; // Adjust as needed to match foot size
        RaycastHit[] hits = Physics.SphereCastAll(foot.position, radius, Vector3.down, distanceToCheckUnderFeet);

        if (hits.Length > 0)
        {
            // Sort the hits by distance (closest first)
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            PhysicMaterial physMat = null;

            // Iterate through the hits to find the first collider with a PhysicMaterial
            foreach (RaycastHit hit in hits)
            {
                physMat = hit.collider.sharedMaterial;
                if (physMat != null && (ignoredMaterials == null || !ignoredMaterials.Contains(physMat)))
                {
                    // Found a valid PhysicMaterial
                    Debug.Log($"{physMat.name} Foot Material");
                    HandleStepping(physMat, foot);
                    return;
                }
            }

            Debug.LogWarning($"Foot spherecast did not hit any collider with a PhysicMaterial on {foot.name}.");
        }
        else
        {
            Debug.LogWarning($"Foot spherecast did not hit any collider on {foot.name}.");
        }
    }

    private void HandleStepping(PhysicMaterial physMat, Transform objPosition)
    {
        bool isRunning = false;

        // Determine running state based on ControllerType
        switch (controllerType)
        {
            case ControllerType.CharacterController:
                if (characterController != null)
                {
                    Vector3 velocity = characterController.velocity;
                    float speed = velocity.magnitude;
                    isRunning = speed >= runThreshold;
                }
                break;

            case ControllerType.Rigidbody:
                if (rb != null)
                {
                    Vector3 velocity = rb.velocity;
                    float speed = velocity.magnitude;
                    isRunning = speed >= runThreshold;
                }
                break;

            case ControllerType.Animator:
                if (anim != null)
                {
                    float horizontal = anim.GetFloat("Horizontal");
                    float vertical = anim.GetFloat("Vertical");
                    float speed = Mathf.Sqrt(horizontal * horizontal + vertical * vertical);
                    isRunning = speed >= runThreshold;
                }
                break;

            case ControllerType.Default:
                isRunning = currentSpeed >= runThreshold;
                break;
        }

        // Choose the appropriate sound and particle based on running state
        if (physMat != null)
        {
            // Get sound library
            PhysicalSoundLibrary sLibrary = null;
            if (footstepMaterialToSound.TryGetValue(physMat, out sLibrary))
            {
                if (sLibrary != null)
                {
                    int soundIndex = isRunning && sLibrary.Data.Count > 1 ? 1 : 0;
                    SoundManager.Instance.CreateSound()
                        .WithSoundData(sLibrary.Data[soundIndex])
                        .WithRandomPitch()
                        .WithPosition(objPosition.position)
                        .Play();
                }
            }
            else
            {
                // Use fallback sound
                if (fallBackFootstepSound != null)
                {
                    SoundManager.Instance.CreateSound()
                        .WithSoundData(fallBackFootstepSound)
                        .WithRandomPitch()
                        .WithPosition(objPosition.position)
                        .Play();
                }
            }

            // Get particle library
            PhysicalParticleLibrary pLibrary = null;
            if (footstepMaterialToParticle.TryGetValue(physMat, out pLibrary))
            {
                if (pLibrary != null)
                {
                    int particleIndex = isRunning && pLibrary.Data.Count > 1 ? 1 : 0;
                    ParticleManager.Instance.CreateParticle()
                        .WithParticleData(pLibrary.Data[particleIndex])
                        .WithPosition(objPosition.position)
                        .WithParent(ParticleManager.Instance.transform)
                        .Play();
                }
            }
            else
            {
                // Use fallback particle
                if (fallBackFootstepParticle != null)
                {
                    ParticleManager.Instance.CreateParticle()
                        .WithParticleData(fallBackFootstepParticle)
                        .WithPosition(objPosition.position)
                        .WithParent(ParticleManager.Instance.transform)
                        .Play();
                }
            }
        }
        else
        {
            // Use fallback effects if physMat is null
            if (fallBackFootstepSound != null)
            {
                SoundManager.Instance.CreateSound()
                    .WithSoundData(fallBackFootstepSound)
                    .WithRandomPitch()
                    .WithPosition(objPosition.position)
                    .Play();
            }

            if (fallBackFootstepParticle != null)
            {
                ParticleManager.Instance.CreateParticle()
                    .WithParticleData(fallBackFootstepParticle)
                    .WithPosition(objPosition.position)
                    .WithParent(ParticleManager.Instance.transform)
                    .Play();
            }
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enableDebugVisualization) return;

        // List to keep track of objects hit to assign different colors
        List<GameObject> hitObjects = new List<GameObject>();

        // Left Foot SphereCast Visualization
        if (leftFoot != null)
        {

            float radius = 0.1f; // Same as in SphereCast
            Gizmos.color = debugRayColor;
            Gizmos.DrawLine(leftFoot.position, leftFoot.position + Vector3.down * distanceToCheckUnderFeet);
            Gizmos.DrawWireSphere(leftFoot.position, radius);

            RaycastHit[] hits = Physics.SphereCastAll(leftFoot.position, radius, Vector3.down, distanceToCheckUnderFeet);
            if (hits.Length > 0)
            {
                // Sort hits by distance
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                // Collect hit objects
                foreach (RaycastHit hit in hits)
                {
                    if (!hitObjects.Contains(hit.collider.gameObject))
                    {
                        hitObjects.Add(hit.collider.gameObject);
                    }
                }
            }
        }

        // Right Foot SphereCast Visualization
        if (rightFoot != null)
        {

            float radius = 0.1f; // Same as in SphereCast
            Gizmos.color = debugRayColor;
            Gizmos.DrawLine(rightFoot.position, rightFoot.position + Vector3.down * distanceToCheckUnderFeet);
            Gizmos.DrawWireSphere(rightFoot.position, radius);

            RaycastHit[] hits = Physics.SphereCastAll(rightFoot.position, radius, Vector3.down, distanceToCheckUnderFeet);
            if (hits.Length > 0)
            {
                // Sort hits by distance
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                // Collect hit objects
                foreach (RaycastHit hit in hits)
                {
                    if (!hitObjects.Contains(hit.collider.gameObject))
                    {
                        hitObjects.Add(hit.collider.gameObject);
                    }
                }
            }
        }

        // Highlight hit objects with wireframe outlines
        for (int i = 0; i < hitObjects.Count; i++)
        {
            GameObject hitObject = hitObjects[i];
            Renderer renderer = hitObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds bounds = renderer.bounds;
                Handles.color = highlightColors[i % highlightColors.Length];
                Handles.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }
#endif
}
public enum ControllerType
{
    CharacterController,
    Rigidbody,
    Default,
    Animator,
}

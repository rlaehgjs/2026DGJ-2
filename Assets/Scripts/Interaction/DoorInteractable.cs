using System.Collections;
using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    private enum RotationAxis
    {
        Y,
        X,
        Z
    }

    [SerializeField, Range(1f, 180f)] private float openAngle = 90f;
    [SerializeField, Min(0.01f)] private float openDuration = 0.3f;
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y;
    [SerializeField] private bool openTowardPlayer;
    [SerializeField] private FrontDoorLock frontDoorLock;
    [SerializeField] private Collider doorCollider;

    private Quaternion closedLocalRotation;
    private Coroutine rotationRoutine;
    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        closedLocalRotation = transform.localRotation;

        if (frontDoorLock == null)
        {
            frontDoorLock = GetComponentInParent<FrontDoorLock>();
        }

        if (doorCollider == null)
        {
            doorCollider = GetComponent<Collider>();

            if (doorCollider == null)
            {
                doorCollider = GetComponentInChildren<Collider>();
            }

            if (doorCollider == null)
            {
                doorCollider = CreateBoxColliderFromMesh();
            }
        }
    }

    public bool CanInteract(PlayerInventory inventory)
    {
        if (rotationRoutine != null)
        {
            return false;
        }

        if (isOpen || frontDoorLock == null || frontDoorLock.IsUnlocked)
        {
            return true;
        }

        return frontDoorLock.CanInteract(inventory);
    }

    public void Interact(PlayerInventory inventory)
    {
        if (!CanInteract(inventory))
        {
            return;
        }

        if (isOpen)
        {
            StartRotation(closedLocalRotation, false);
            return;
        }

        if (frontDoorLock != null && !frontDoorLock.IsUnlocked)
        {
            frontDoorLock.Interact(inventory);
        }

        Vector3 localRotationAxis = GetLocalRotationAxis(rotationAxis);
        Vector3 worldRotationAxis = transform.TransformDirection(localRotationAxis);
        float signedAngle = GetSignedOpenAngle(
            transform.position,
            worldRotationAxis,
            GetDoorCenter(),
            inventory.transform.position,
            openAngle,
            openTowardPlayer);
        Quaternion targetRotation = closedLocalRotation
            * Quaternion.AngleAxis(signedAngle, localRotationAxis);

        StartRotation(targetRotation, true);
    }

    public static float CalculateOpenAngle(
        Vector3 hingePosition,
        Vector3 rotationAxis,
        Vector3 doorCenter,
        Vector3 playerPosition,
        float angle)
    {
        Vector3 centerOffset = doorCenter - hingePosition;

        if (centerOffset.sqrMagnitude <= Mathf.Epsilon)
        {
            return angle;
        }

        Vector3 positiveAngleCenter = hingePosition
            + Quaternion.AngleAxis(angle, rotationAxis) * centerOffset;
        Vector3 negativeAngleCenter = hingePosition
            + Quaternion.AngleAxis(-angle, rotationAxis) * centerOffset;

        float positiveDistance = (positiveAngleCenter - playerPosition).sqrMagnitude;
        float negativeDistance = (negativeAngleCenter - playerPosition).sqrMagnitude;

        return positiveDistance >= negativeDistance ? angle : -angle;
    }

    private static float GetSignedOpenAngle(
        Vector3 hingePosition,
        Vector3 rotationAxis,
        Vector3 doorCenter,
        Vector3 playerPosition,
        float angle,
        bool openTowardPlayer)
    {
        float awayFromPlayerAngle = CalculateOpenAngle(
            hingePosition,
            rotationAxis,
            doorCenter,
            playerPosition,
            angle);

        return openTowardPlayer ? -awayFromPlayerAngle : awayFromPlayerAngle;
    }

    private Vector3 GetDoorCenter()
    {
        return doorCollider != null ? doorCollider.bounds.center : transform.position + transform.forward;
    }

    private static Vector3 GetLocalRotationAxis(RotationAxis axis)
    {
        switch (axis)
        {
            case RotationAxis.X:
                return Vector3.right;

            case RotationAxis.Z:
                return Vector3.forward;

            default:
                return Vector3.up;
        }
    }

    private BoxCollider CreateBoxColliderFromMesh()
    {
        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>(true);

        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            return CreateBoxCollider(meshFilter.gameObject, meshFilter.sharedMesh.bounds);
        }

        Renderer renderer = GetComponentInChildren<Renderer>(true);
        if (renderer == null)
        {
            return null;
        }

        return CreateBoxCollider(renderer.gameObject, renderer.localBounds);
    }

    private static BoxCollider CreateBoxCollider(GameObject target, Bounds bounds)
    {
        if (target.TryGetComponent(out BoxCollider existingBoxCollider))
        {
            return existingBoxCollider;
        }

        if (target.TryGetComponent(out Collider _))
        {
            return null;
        }

        BoxCollider boxCollider = target.AddComponent<BoxCollider>();
        boxCollider.center = bounds.center;
        boxCollider.size = bounds.size;
        return boxCollider;
    }

    private void StartRotation(Quaternion targetRotation, bool opening)
    {
        isOpen = opening;
        rotationRoutine = StartCoroutine(RotateTo(targetRotation));
    }

    private IEnumerator RotateTo(Quaternion targetRotation)
    {
        Quaternion startRotation = transform.localRotation;
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                Mathf.Clamp01(elapsed / openDuration));
            yield return null;
        }

        transform.localRotation = targetRotation;
        rotationRoutine = null;
    }
}

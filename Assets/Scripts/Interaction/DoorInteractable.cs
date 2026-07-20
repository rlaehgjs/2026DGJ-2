using System.Collections;
using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField, Range(1f, 180f)] private float openAngle = 90f;
    [SerializeField, Min(0.01f)] private float openDuration = 0.3f;
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

        float signedAngle = CalculateOpenAngle(
            transform.position,
            transform.up,
            GetDoorCenter(),
            inventory.transform.position,
            openAngle);
        Quaternion targetRotation = closedLocalRotation * Quaternion.Euler(0f, signedAngle, 0f);

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

    private Vector3 GetDoorCenter()
    {
        return doorCollider != null ? doorCollider.bounds.center : transform.position + transform.forward;
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

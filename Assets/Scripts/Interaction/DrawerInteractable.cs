using System.Collections;
using UnityEngine;

public class DrawerInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private float openLocalX;
    [SerializeField, Min(0.01f)] private float openDuration = 0.3f;

    private Vector3 closedLocalPosition;
    private Coroutine movementRoutine;
    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        closedLocalPosition = transform.localPosition;
        EnsureInteractionCollider();
    }

    public bool CanInteract(PlayerInventory inventory)
    {
        return movementRoutine == null;
    }

    public void Interact(PlayerInventory inventory)
    {
        if (!CanInteract(inventory))
        {
            return;
        }

        Vector3 targetPosition = isOpen
            ? closedLocalPosition
            : CalculateOpenLocalPosition(closedLocalPosition, openLocalX);

        StartMovement(targetPosition, !isOpen);
    }

    public static Vector3 CalculateOpenLocalPosition(
        Vector3 closedLocalPosition,
        float openLocalX)
    {
        closedLocalPosition.x = openLocalX;
        return closedLocalPosition;
    }

    private void EnsureInteractionCollider()
    {
        if (GetComponent<Collider>() != null || GetComponentInChildren<Collider>() != null)
        {
            return;
        }

        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>(true);

        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            CreateBoxCollider(meshFilter.gameObject, meshFilter.sharedMesh.bounds);
            return;
        }

        Renderer renderer = GetComponentInChildren<Renderer>(true);
        if (renderer != null)
        {
            CreateBoxCollider(renderer.gameObject, renderer.localBounds);
        }
    }

    private static void CreateBoxCollider(GameObject target, Bounds bounds)
    {
        if (target.GetComponent<Collider>() != null)
        {
            return;
        }

        BoxCollider boxCollider = target.AddComponent<BoxCollider>();
        boxCollider.center = bounds.center;
        boxCollider.size = bounds.size;
    }

    private void StartMovement(Vector3 targetPosition, bool opening)
    {
        isOpen = opening;
        movementRoutine = StartCoroutine(MoveTo(targetPosition));
    }

    private IEnumerator MoveTo(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(
                startPosition,
                targetPosition,
                Mathf.Clamp01(elapsed / openDuration));
            yield return null;
        }

        transform.localPosition = targetPosition;
        movementRoutine = null;
    }
}

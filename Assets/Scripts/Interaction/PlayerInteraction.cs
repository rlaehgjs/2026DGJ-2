using System;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Camera interactionCamera;
    [Min(0.1f)][SerializeField] private float interactionDistance = 3f; //상호작용 거리

    private GameInputReader gameInputReader;
    private PlayerInventory playerInventory;

    private void Awake()
    {
        gameInputReader = GetComponent<GameInputReader>();
        playerInventory = GetComponent<PlayerInventory>();
    }

    private void OnEnable()
    {
        if (gameInputReader != null)
        {
            gameInputReader.InteractPressed += TryInteract;
        }
    }

    private void OnDisable()
    {
        if (gameInputReader != null)
        {
            gameInputReader.InteractPressed -= TryInteract;
        }
    }

    private void TryInteract() //상호작용의 시작점
    {
        if (interactionCamera == null || playerInventory == null)
        {
            return;
        }

        //Ray로 화면 정중앙의 물체 탐색
        Ray interactionRay = interactionCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (!Physics.Raycast(interactionRay, out RaycastHit hit, interactionDistance))
        {
            LogInteractionTarget(null, null, false);
            return;
        }

        IInteractable interactable = FindInteractable(hit.collider); //Ray에 맞은게 상호작용 가능한 물체인지 확인

        if (interactable is FrontDoorLock frontDoorLock
            && TryFindDoorInteractableBehindFrontDoorLock(
                interactionRay,
                interactionDistance,
                frontDoorLock,
                out RaycastHit doorHit,
                out DoorInteractable doorInteractable))
        {
            hit = doorHit;
            interactable = doorInteractable;
        }

        bool canInteract = interactable != null && interactable.CanInteract(playerInventory);
        LogInteractionTarget(hit.collider, interactable, canInteract);

        if (!canInteract)
        {
            return;
        }

        interactable.Interact(playerInventory);
    }

    private static int CompareHitDistance(RaycastHit firstHit, RaycastHit secondHit)
    {
        return firstHit.distance.CompareTo(secondHit.distance);
    }

    private static bool TryFindDoorInteractableBehindFrontDoorLock(
        Ray interactionRay,
        float interactionDistance,
        FrontDoorLock frontDoorLock,
        out RaycastHit doorHit,
        out DoorInteractable doorInteractable)
    {
        RaycastHit[] hits = Physics.RaycastAll(interactionRay, interactionDistance);
        Array.Sort(hits, CompareHitDistance);

        foreach (RaycastHit candidateHit in hits)
        {
            IInteractable candidateInteractable = FindInteractable(candidateHit.collider);

            if (!(candidateInteractable is DoorInteractable candidateDoor)
                || candidateDoor.GetComponentInParent<FrontDoorLock>() != frontDoorLock)
            {
                continue;
            }

            doorHit = candidateHit;
            doorInteractable = candidateDoor;
            return true;
        }

        doorHit = default;
        doorInteractable = null;
        return false;
    }

    private static IInteractable FindInteractable(Collider hitCollider)
    {
        MonoBehaviour[] behaviours = hitCollider.GetComponentsInParent<MonoBehaviour>(); //Ray가 오브젝트의 자식을 맞힐 수 있어, Parent도 확인하게 진행 -> 부모가 가진 IInteractable을 찾아 실행

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IInteractable interactable)
            {
                return interactable;
            }
        }

        return null;
    }

    private static void LogInteractionTarget(
        Collider hitCollider,
        IInteractable interactable,
        bool canInteract)
    {
        string targetName = hitCollider == null ? "None" : hitCollider.gameObject.name;
        string interactableName = interactable == null ? "None" : interactable.GetType().Name;

        Debug.Log(
            $"[Interaction] Target={targetName}; "
            + $"Interactable={interactableName}; CanInteract={canInteract}");
    }

    private void OnDrawGizmos()
    {
        if (interactionCamera == null)
        {
            return;
        }

        Ray interactionRay = interactionCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 endPoint = interactionRay.origin + interactionRay.direction * interactionDistance;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(interactionRay.origin, endPoint);
        Gizmos.DrawWireSphere(endPoint, 0.05f);
    }
}

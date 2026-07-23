using System.Collections;
using UnityEngine;

/// <summary>
/// Plays a one-shot jumpscare after a specific pickup is collected.
/// This controller stays active in the scene, so the effect can continue after the pickup deactivates itself.
/// </summary>
public class PickupJumpscareController : MonoBehaviour
{
    [SerializeField] private PickupInteractable pickupInteractable;
    [SerializeField] private GameObject ghostCanvasImage;
    [Min(0f)][SerializeField] private float delayTime = 1f;
    [Min(0f)][SerializeField] private float ghostDuration = 1.2f;

    private bool hasPlayed;
    private Coroutine jumpscareRoutine;

    private void Awake()
    {
        if (ghostCanvasImage != null)
        {
            ghostCanvasImage.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (pickupInteractable != null)
        {
            pickupInteractable.ItemCollected += HandleItemCollected;
        }
    }

    private void OnDisable()
    {
        if (pickupInteractable != null)
        {
            pickupInteractable.ItemCollected -= HandleItemCollected;
        }

        if (jumpscareRoutine != null)
        {
            StopCoroutine(jumpscareRoutine);
            jumpscareRoutine = null;
        }

        if (ghostCanvasImage != null)
        {
            ghostCanvasImage.SetActive(false);
        }
    }

    private void HandleItemCollected(ItemData _, int __)
    {
        if (hasPlayed)
        {
            return;
        }

        hasPlayed = true;
        jumpscareRoutine = StartCoroutine(PlayJumpscare());
    }

    private IEnumerator PlayJumpscare()
    {
        yield return new WaitForSeconds(delayTime);

        if (ghostCanvasImage != null)
        {
            ghostCanvasImage.SetActive(true);
        }

        yield return new WaitForSeconds(ghostDuration);

        if (ghostCanvasImage != null)
        {
            ghostCanvasImage.SetActive(false);
        }

        jumpscareRoutine = null;
    }
}

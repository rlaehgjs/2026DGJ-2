using System.Collections;
using UnityEngine;

public class EndingAutoReturn : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private float delaySeconds = 10f;

    private void OnEnable()
    {
        StartCoroutine(ReturnAfterDelay());
    }

    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSecondsRealtime(delaySeconds);
        gameManager?.ReturnToMainMenu();
    }
}

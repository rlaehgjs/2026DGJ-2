using UnityEngine;

public class InGameUIController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject endingPanel;
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class AntKill : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("💀 개미에게 잡혔습니다! 게임 오버!");
            // 씬 처음부터 재시작
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
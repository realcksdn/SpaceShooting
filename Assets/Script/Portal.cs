using UnityEngine;

/// <summary>
/// 보스 처치 후 스폰되는 포탈.
/// 플레이어가 닿으면 다음 스테이지 해금 + 메인 메뉴로 이동.
/// 프리팹에 Collider(IsTrigger) 필수.
/// </summary>
public class Portal : MonoBehaviour
{
    [Header("회전 연출")]
    public float rotateSpeed = 90f;

    private bool used = false;

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!other.CompareTag("Player") && other.transform.root.CompareTag("Player") == false) return;

        used = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnlockNextStage();

            Debug.Log($"[Portal] currentStage = {GameManager.Instance.currentStage}  rankingScene = {GameManager.Instance.rankingSceneName}");

            // Stage3(인덱스 2) 클리어 → end 씬으로
            if (GameManager.Instance.currentStage == 2)
            {
                if (FadeManager.Instance != null)
                    FadeManager.Instance.CallScene("End");
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene("End");
            }
            else
            {
                GameManager.Instance.GoToMenu();
            }
        }
    }
}

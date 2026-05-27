using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Collider의 IsTrigger를 켜두면
/// 지정한 태그가 닿을 때 씬을 이동합니다.
/// </summary>
public class SceneTrigger : MonoBehaviour
{
    public string targetScene;

    void OnTriggerEnter(Collider other)
    {
       SceneManager.LoadScene(targetScene);
    }
}

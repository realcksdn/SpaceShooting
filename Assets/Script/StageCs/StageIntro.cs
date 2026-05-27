using UnityEngine;

/// <summary>
/// 스테이지 씬의 아무 오브젝트에 추가.
/// 씬이 시작될 때 자동으로 페이드인(검정 → 화면 등장) 실행.
/// </summary>
public class StageIntro : MonoBehaviour
{
    void Start()
    {
        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeIn();

       
    }
}

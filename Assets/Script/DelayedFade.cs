using System.Collections;
using UnityEngine;

public class DelayedFade : MonoBehaviour
{
    public float delay = 5f;
    public string targetScene = "";

    IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);
        FadeManager.Instance.CallScene(targetScene);
    }
}

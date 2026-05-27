using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ImageFade : MonoBehaviour
{
    public Image fadeImage;
    public float delay    = 5f;
    public float duration = 1f;

    IEnumerator Start()
    {
        SetAlpha(0f);
        yield return new WaitForSeconds(delay);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(0f, 1f, t / duration));
            yield return null;
        }
        SetAlpha(1f);
        SceneManager.LoadScene("Shop");
    }

    void SetAlpha(float a)
    {
        if (fadeImage == null) return;
        var c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}

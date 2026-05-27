using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [SerializeField] private CanvasGroup fadeImage;
    [SerializeField] private float duration = 2.0f;

    private bool isFading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (fadeImage == null)
                CreateFadeCanvas();

            fadeImage.alpha = 0f;
            fadeImage.blocksRaycasts = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CreateFadeCanvas()
    {
        // Canvas
        var canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // 검은 이미지 (Image 먼저 추가해야 RectTransform 생김)
        var panelGO = new GameObject("FadePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);

        var image = panelGO.AddComponent<Image>();
        image.color = Color.black;

        var rect = panelGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        fadeImage = panelGO.AddComponent<CanvasGroup>();
    }

    // FadeOut / FadeIn 통합
    private IEnumerator Fade(float from, float to)
    {
        fadeImage.blocksRaycasts = true;
        fadeImage.alpha = from;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // timeScale=0(게임오버)에서도 작동
            fadeImage.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        fadeImage.alpha = to;

        if (to <= 0f)
            fadeImage.blocksRaycasts = false;
    }

    private IEnumerator SceneChangeRoutine(int sceneIndex)
    {
        isFading = true;

        yield return StartCoroutine(Fade(0f, 1f)); // alpha 0→1 : 화면이 검게 (암전)

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneIndex);
        loadOp.allowSceneActivation = false;

        while (loadOp.progress < 0.9f)
            yield return null;

        loadOp.allowSceneActivation = true;
        yield return null;                         // 씬 활성화 대기

        yield return StartCoroutine(Fade(1f, 0f)); // alpha 1→0 : 새 씬 서서히 등장

        isFading = false;
    }

    private IEnumerator SceneChangeRoutine(string sceneName)
    {
        isFading = true;

        yield return StartCoroutine(Fade(0f, 1f)); // alpha 0→1 : 화면이 검게 (암전)

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName);
        loadOp.allowSceneActivation = false;

        while (loadOp.progress < 0.9f)
            yield return null;

        loadOp.allowSceneActivation = true;
        yield return null;                         // 씬 활성화 대기

        yield return StartCoroutine(Fade(1f, 0f)); // alpha 1→0 : 새 씬 서서히 등장

        isFading = false;
    }

    /// <summary>현재 씬에서 페이드인만 실행 (씬 시작 시 호출)</summary>
    public void FadeIn()
    {
        if (isFading) return;
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        isFading = true;
        yield return StartCoroutine(Fade(1f, 0f));
        isFading = false;
    }

    /// <summary>게임오버 등 외부에서 강제로 레이캐스트 차단 해제</summary>
    public void UnblockRaycasts()
    {
        if (fadeImage != null)
        {
            fadeImage.blocksRaycasts = false;
            fadeImage.alpha = 0f;
        }
        isFading = false;
    }

    /// <summary>씬 인덱스로 이동</summary>
    public void CallScene(int sceneIndex)
    {
        if (isFading) return;
        StartCoroutine(SceneChangeRoutine(sceneIndex));
    }

    /// <summary>씬 이름으로 이동 (Build Settings에 등록된 씬만 가능)</summary>
    public void CallScene(string sceneName)
    {
        if (isFading) return;
        StartCoroutine(SceneChangeRoutine(sceneName));
    }
}

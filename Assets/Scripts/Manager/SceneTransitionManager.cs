using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float fadeOutDuration = 0.4f;

    [Header("Loading Screen Colors")]
    [SerializeField] private Color backgroundColor = new Color(0.07f, 0.07f, 0.07f, 1f);
    [SerializeField] private Color barBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color barFillColor = new Color(0.95f, 0.95f, 0.95f, 1f);

    [Header("Progress Bar")]
    [SerializeField] private readonly float barWidthFraction = 0.55f;
    [SerializeField] private readonly float barHeight = 18f;

    public const string MAIN_MENU_SCENE = "MainMenu";
    public const string GAME_SCENE = "Game";

    private CanvasGroup _canvasGroup;
    private Slider _progressSlider;
    private bool _isTransitioning;

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildLoadingScreen();
    }

    private void Start()
    {
        // Fade in on the very first scene
        StartCoroutine(FadeCanvas(1f, 0f, fadeInDuration));
    }

    #endregion

    #region Public API

    public void TransitionToScene(string sceneName)
    {
        if (_isTransitioning) return;
        StartCoroutine(DoTransition(sceneName));
    }

    public void GoToMainMenu() => TransitionToScene(MAIN_MENU_SCENE);
    public void GoToGame() => TransitionToScene(GAME_SCENE);

    #endregion

    #region Transition Coroutine

    private IEnumerator DoTransition(string sceneName)
    {
        _isTransitioning = true;

        _progressSlider.value = 0f;

        yield return StartCoroutine(FadeCanvas(0f, 1f, fadeOutDuration));

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        load.allowSceneActivation = false;

        while (load.progress < 0.9f)
        {
            _progressSlider.value = Mathf.Clamp01(load.progress / 0.9f);
            yield return null;
        }

        yield return StartCoroutine(AnimateSlider(_progressSlider.value, 1f, 0.15f));

        yield return new WaitForSeconds(0.2f);

        load.allowSceneActivation = true;
        yield return null; 

        yield return StartCoroutine(FadeCanvas(1f, 0f, fadeInDuration));

        _isTransitioning = false;
    }

    #endregion

    #region Animation Helpers

    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        float elapsed = 0f;
        _canvasGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        _canvasGroup.alpha = to;
    }

    private IEnumerator AnimateSlider(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _progressSlider.value = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        _progressSlider.value = to;
    }

    #endregion

    #region Runtime UI Construction
    private void BuildLoadingScreen()
    {
        var canvasGO = new GameObject("LoadingCanvas");
        canvasGO.transform.SetParent(transform);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        _canvasGroup = canvasGO.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 1f; 
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = backgroundColor;
        StretchToFill(bgGO.GetComponent<RectTransform>());

        var sliderGO = new GameObject("ProgressSlider");
        sliderGO.transform.SetParent(canvasGO.transform, false);

        _progressSlider = sliderGO.AddComponent<Slider>();
        _progressSlider.minValue = 0f;
        _progressSlider.maxValue = 1f;
        _progressSlider.value = 0f;
        _progressSlider.interactable = false;
        _progressSlider.wholeNumbers = false;
        _progressSlider.direction = Slider.Direction.LeftToRight;

        var sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        float barWidth = 1920f * barWidthFraction;
        sliderRect.sizeDelta = new Vector2(barWidth, barHeight);
        sliderRect.anchoredPosition = new Vector2(0f, -60f); 

        // Background track
        var trackGO = new GameObject("Background");
        trackGO.transform.SetParent(sliderGO.transform, false);
        var trackImg = trackGO.AddComponent<Image>();
        trackImg.color = barBackgroundColor;
        StretchToFill(trackGO.GetComponent<RectTransform>());

        // Fill Area
        var fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        var fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = barFillColor;
        var fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0f, 1f); 
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fillRect.pivot = new Vector2(0f, 0.5f);

        _progressSlider.fillRect = fillRect;

        _progressSlider.handleRect = null;
    }

    private static void StretchToFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    #endregion
}
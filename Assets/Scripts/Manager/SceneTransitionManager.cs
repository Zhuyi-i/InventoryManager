using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Transition Settings")]
    [Tooltip("How long the fade-out (to black) takes in seconds.")]
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Tooltip("How long the screen stays fully black before the new scene appears.")]
    [SerializeField] private float holdDuration = 0.1f;

    [Tooltip("How long the fade-in (from black) takes in seconds.")]
    [SerializeField] private float fadeInDuration = 0.5f;

    [Tooltip("Color of the transition overlay (default: black).")]
    [SerializeField] private Color fadeColor = Color.black;

    private CanvasGroup _canvasGroup;
    private bool _isTransitioning;

    public const string MAIN_MENU_SCENE = "MainMenu";
    public const string GAME_SCENE      = "TestScene";

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildOverlayCanvas();
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
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

    #region Private — Transition Coroutines

    private IEnumerator DoTransition(string sceneName)
    {
        _isTransitioning = true;

        yield return StartCoroutine(Fade(0f, 1f, fadeOutDuration));

        yield return new WaitForSeconds(holdDuration);

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        load.allowSceneActivation = false;

        while (load.progress < 0.9f)
            yield return null;

        load.allowSceneActivation = true;

        yield return null;

        yield return StartCoroutine(Fade(1f, 0f, fadeInDuration));

        _isTransitioning = false;
    }

    private IEnumerator FadeIn()
    {
        _canvasGroup.alpha = 1f;
        yield return StartCoroutine(Fade(1f, 0f, fadeInDuration));
    }

    /// <summary>Lerps the overlay alpha between <paramref name="from"/> and <paramref name="to"/>.</summary>
    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        _canvasGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        _canvasGroup.alpha = to;
    }

    #endregion

    #region Private — Overlay Canvas Construction
    private void BuildOverlayCanvas()
    {
        var canvasGO = new GameObject("TransitionCanvas");
        canvasGO.transform.SetParent(transform);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;          

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        _canvasGroup                  = canvasGO.AddComponent<CanvasGroup>();
        _canvasGroup.alpha            = 1f;   
        _canvasGroup.interactable     = false;
        _canvasGroup.blocksRaycasts   = false;

        var imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);

        var image  = imageGO.AddComponent<Image>();
        image.color = fadeColor;

        var rect = imageGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    #endregion
}

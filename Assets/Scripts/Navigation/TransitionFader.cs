using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class TransitionFader : MonoBehaviour
{
    [Tooltip("Seconds for a full fade out or fade in.")]
    public float fadeDuration = 0.35f;

    private Image _image;
    private Coroutine _current;

    private void Awake()
    {
        _image = GetComponent<Image>();
        SetAlpha(0f);
    }

    public Coroutine FadeOut(System.Action onComplete = null)
    {
        if (_current != null) StopCoroutine(_current);
        _current = StartCoroutine(DoFade(0f, 1f, onComplete));
        return _current;
    }

    public Coroutine FadeIn(System.Action onComplete = null)
    {
        if (_current != null) StopCoroutine(_current);
        _current = StartCoroutine(DoFade(1f, 0f, onComplete));
        return _current;
    }

    private IEnumerator DoFade(float from, float to, System.Action onComplete)
    {
        float elapsed = 0f;
        SetAlpha(from);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;  
            SetAlpha(Mathf.Lerp(from, to, elapsed / fadeDuration));
            yield return null;
        }

        SetAlpha(to);
        onComplete?.Invoke();
    }

    private void SetAlpha(float a)
    {
        if (_image == null) return;
        var c = _image.color;
        c.a = a;
        _image.color = c;
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class Hint : MonoBehaviour
{
    public GameObject HintObject;
    private TextMeshProUGUI hint;
    public bool hintIsActive = false;
    // private float fadeDuration = 2f;

    void Awake()
    {
        hint = HintObject.GetComponent<TextMeshProUGUI>();
        HintObject.SetActive(false);
    }

    public void ShowHint(string text, float delay)
    {
        StartCoroutine(hintCoroutine(text, delay));
    }

    public IEnumerator hintCoroutine(string text, float delay)
    {
        yield return new WaitForSeconds(delay);
        hintIsActive = true;
        HintObject.SetActive(true);
        hint.text = text;
        yield return StartCoroutine(FadeTextAlpha(0, 1, 1f)); // Плавное появление

        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(FadeTextAlpha(1, 0, 1f)); // Плавное исчезновение
        hintIsActive = false;
        HintObject.SetActive(false);
    }

    IEnumerator FadeTextAlpha(float start, float end, float duration)
    {
        float elapsed = 0;
        Color color = hint.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(start, end, elapsed / duration);
            hint.color = color;
            yield return null;
        }

        color.a = end;
        hint.color = color;
    }
}
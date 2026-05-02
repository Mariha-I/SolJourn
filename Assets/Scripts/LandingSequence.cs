using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LandingSequence : MonoBehaviour
{
    public CanvasGroup landingGroup;
    public CanvasGroup fadePanel;
    public CanvasGroup subtitleGroup;

    void Start()
    {
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        // Starting values
        landingGroup.alpha = 1f;
        subtitleGroup.alpha = 0f;

        // Wait half second
        yield return new WaitForSeconds(0.5f);

        // Fade subtitle in
        yield return StartCoroutine(Fade(subtitleGroup, 0f, 1f, 1f));

        // Hold for 2 seconds
        yield return new WaitForSeconds(2f);

        // Fade whole landing page out
        yield return StartCoroutine(Fade(landingGroup, 1f, 0f, 1.5f));

        fadePanel.alpha = 1f;
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(Fade(fadePanel, 1f, 0f, 1.5f));
        fadePanel.gameObject.SetActive(false);
    }

    IEnumerator Fade(CanvasGroup group, float start, float end, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, end, time / duration);
            yield return null;
        }

        group.alpha = end;
    }
}
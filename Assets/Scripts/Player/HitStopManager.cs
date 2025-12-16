using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager Instance;

    private IEnumerator currentHitStop = null;

    [SerializeField] private float defaultHitStopDuration = .05f;
    [SerializeField] private AnimationCurve defaultTimeScaleEasing;

    private void Awake()
    {
        // Singleton boilerplate
        if (Instance == null) Instance = this;
        else DestroyImmediate(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown("k")) InitDefaultEasedHitStop();
    }

    public void InitDefaultHitStop() => InitHitStop(defaultHitStopDuration);

    public void InitDefaultEasedHitStop() => InitEasedHitStop(defaultHitStopDuration, defaultTimeScaleEasing);

    public void InitHitStop(float duration, float scale = 0)
    {
        if (currentHitStop != null)
            StopCoroutine(currentHitStop);

        currentHitStop = HitStop(duration, scale);

        StartCoroutine(currentHitStop);
    }

    public void InitEasedHitStop(float duration, AnimationCurve easingFunc)
    {

        if (currentHitStop != null)
            StopCoroutine(currentHitStop);

        currentHitStop = HitStopEased(duration, easingFunc);

        StartCoroutine(currentHitStop);
    }

    private IEnumerator HitStop(float duration, float scale)
    {
        Time.timeScale = scale;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1;
        currentHitStop = null;
    }

    private IEnumerator HitStopEased(float duration, AnimationCurve easingFunc)
    {
        float startTime = Time.unscaledTime;

        float elapsedTimeAsPercent = 0;

        while (elapsedTimeAsPercent <= 1)
        {
            elapsedTimeAsPercent = (Time.unscaledTime - startTime) / duration;
            Time.timeScale = easingFunc.Evaluate(elapsedTimeAsPercent);

            yield return null;
        }

        Time.timeScale = 1;
        currentHitStop = null;
    }
}

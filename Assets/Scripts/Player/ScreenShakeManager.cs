using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShakeManager : MonoBehaviour
{
    public static ScreenShakeManager Instance;

    [Header("Settings")]
    public float zToIntesityMultiplier = -1f;

    [Space]

    public float defaultPerlinShakeDuration = .2f;
    public float defaultPerlinShakeIntensity = .4f;
    public float defaultPerlinShakeFrequency = .3f;

    [Space]

    public float defaultSinShakeDuration = .05f;
    public float defaultSinShakeIntensity = .15f;
    public float defaultSinShakeFrequency = 40f;

    public List<IEnumerator> activeShakes = new List<IEnumerator>();

    // Just leaving this, its deprecated but so what
    public IEnumerator currentShake;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            DestroyImmediate(this.gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown("p")) InitiateDefaultPerlinShake();
        if (Input.GetKeyDown("l")) InitiateDefaultSinShake();
    }

    private void DeleteShakeFromActive(IEnumerator shake)
    {
        activeShakes.Remove(shake);

        // Make sure the screen returns to center
        if (activeShakes.Count == 0) transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Runs perlin shake with default values
    /// </summary>
    public void InitiateDefaultPerlinShake() => InitiatePerlinShake(defaultPerlinShakeDuration, defaultPerlinShakeIntensity, defaultPerlinShakeFrequency);

    /// <summary>
    /// Fire and forget method for running a perlin noise shake
    /// </summary>
    public void InitiatePerlinShake(float duration, float intensity, float sampleFrequency, float? seedX = null, float? seedY = null)
    {
        if (currentShake != null) StopCoroutine(currentShake);

        currentShake = PerlinShake(duration, intensity, sampleFrequency, Time.time, seedX, seedY);

        StartCoroutine(currentShake);
    }

    /// <summary>
    /// Coroutine for screen shake using perlin noise. Use for more disorienting shake.
    /// </summary>
    private IEnumerator PerlinShake(float duration, float intensity, float sampleFrequency, float startTimestamp, float? seedX = null, float? seedY = null)
    {
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime = Time.time - startTimestamp;

            // Sample noise based on given parameters and return it in the range of [-1 , 1]
            float xOffset = Mathf.PerlinNoise(seedX ?? Random.Range(0, 10000f) + sampleFrequency * elapsedTime, 0) * 2 - 1;
            float yOffset = Mathf.PerlinNoise(0, seedY ?? Random.Range(0, 10000f) + sampleFrequency * elapsedTime) * 2 - 1;

            // The intensity of a shake is relative to the z position (pretty much the zoom) of the camera.
            transform.localPosition = new Vector3(xOffset, yOffset, 0) * intensity * transform.parent.position.z * zToIntesityMultiplier;

            yield return null;
        }

        currentShake = null;
        transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Runs a sin shake with the default settings.
    /// </summary>
    public void InitiateDefaultSinShake() => InitiateSinShake(defaultSinShakeDuration, defaultSinShakeIntensity, defaultSinShakeFrequency);

    /// <summary>
    /// Fire and forget function for starting a sin shake coroutine.
    /// </summary>
    public void InitiateSinShake(float duration, float intensity, float frequency)
    {
        // Save this shake instance
        IEnumerator shakeInstance = null;
        shakeInstance = SinShake(duration, intensity, frequency, () => DeleteShakeFromActive(shakeInstance));

        // Add it to the active shakes
        activeShakes.Add(shakeInstance);
        StartCoroutine(shakeInstance);
    }

    /// <summary>
    /// Coroutine for screen shake using layered sin waves.
    /// </summary>
    private IEnumerator SinShake(float duration, float intensity, float frequency, System.Action onComplete = null, float octaves = 2)
    {
        float elapsedTime = 0;
        float startTimestamp = Time.time;

        int shakeYDir =  Random.Range(0, 2) == 0 ? -1 : 1;

        try
        {
            // Shake
            while (elapsedTime < duration || Mathf.Abs(transform.localPosition.x) > .1f)
            {
                elapsedTime = Time.time - startTimestamp;

                float xOffset = 0;
                float yOffset = 0;

                for (int i = 1; i <= octaves; i++)
                {
                    xOffset += Mathf.Sin(elapsedTime * frequency * i) * intensity / i;
                    yOffset += Mathf.Sin(elapsedTime * frequency * i) * intensity / i * shakeYDir;
                }

                transform.localPosition = new Vector3(xOffset, yOffset, 0) * transform.parent.position.z * zToIntesityMultiplier;

                yield return null;
            }
        }
        finally
        {
            onComplete?.Invoke();
        }
    }

    public void InitShakeByType(ScreenShakeType type)
    {
        switch (type)
        {
            case ScreenShakeType.Default:
                InitiateDefaultSinShake();
                break;

            case ScreenShakeType.Hit:
                InitiateSinShake(.05f, .1f, 40);
                break;

            case ScreenShakeType.Explosion:
                InitiateSinShake(.08f, .15f, 30);
                break;
        }
    }
}

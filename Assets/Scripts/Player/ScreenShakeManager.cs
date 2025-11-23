using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShakeManager : MonoBehaviour
{
    public static ScreenShakeManager Instance;

    [Header("Settings")]
    public float zToIntesityMultiplier = -1f;

    public float defaultPerlinShakeDuration = .2f;
    public float defaultPerlinShakeIntensity = 2f;
    public float defaultPerlinShakeFrequency = .3f;

    [Space]

    public float defaultSinShakeDuration = .2f;
    public float defaultSinShakeIntensity = 2f;
    public float defaultSinShakeFrequency = .03f;

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

    public void InitiateDefaultPerlinShake() => InitiatePerlinShake(defaultPerlinShakeDuration, defaultPerlinShakeIntensity, defaultPerlinShakeFrequency);

    public void InitiatePerlinShake(float duration, float intensity, float sampleFrequency, float? seedX = null, float? seedY = null)
    {
        if (currentShake != null) StopCoroutine(currentShake);

        currentShake = PerlinShake(duration, intensity, sampleFrequency, Time.time, seedX, seedY);

        StartCoroutine(currentShake);
    }

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


    public void InitiateDefaultSinShake() => InitiateSinShake(defaultSinShakeDuration, defaultSinShakeIntensity, defaultSinShakeFrequency);

    public void InitiateSinShake(float duration, float intensity, float frequency)
    {
        if (currentShake != null) StopCoroutine(currentShake);

        currentShake = SinShake(duration, intensity, frequency);

        StartCoroutine(currentShake);
    }

    private IEnumerator SinShake(float duration, float intensity, float frequency, float octaves = 2)
    {
        float elapsedTime = 0;
        float startTimestamp = Time.time;

        int shakeYDir =  Random.Range(0, 2) == 0 ? -1 : 1;

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

        currentShake = null;
        transform.localPosition = Vector3.zero;

        Debug.Log("Sin shake has concluded");
    }
}

using System.Collections;
using TMPro;
using UnityEngine;

public class HeartBeatAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float beatScale = 1.2f;        // How much the heart grows during beat
    [SerializeField] private float beatDuration = 0.5f;     // Duration of one complete beat
    [SerializeField] private AnimationCurve beatCurve;      // Animation curve for smooth beating

    [Header("Loading Text Settings")]
    [SerializeField] private TMP_Text loadingText;             // Reference to the loading text UI element
    [SerializeField] private float dotAnimationSpeed = 0.5f; // Speed of the dot animation

    private Vector3 originalScale;
    private float currentTime = 0f;

    void Start()
    {
        // Store the original scale of the heart
        originalScale = transform.localScale;

        // Initialize a default animation curve if none is set in inspector
        if (beatCurve.length == 0)
        {
            Keyframe[] keys = new Keyframe[4];
            keys[0] = new Keyframe(0f, 1f);     // Start at normal size
            keys[1] = new Keyframe(0.2f, beatScale);  // Quick expansion
            keys[2] = new Keyframe(0.35f, 0.95f);    // Slight compression
            keys[3] = new Keyframe(1f, 1f);     // Return to normal

            beatCurve = new AnimationCurve(keys);
        }

        // Start the loading text animation
        if (loadingText != null)
        {
            StartCoroutine(AnimateLoadingText());
        }
    }

    void Update()
    {
        // Update the timer
        currentTime += Time.deltaTime;

        // Reset timer when we complete one beat cycle
        if (currentTime > beatDuration)
        {
            currentTime = 0f;
        }

        // Calculate the current scale factor based on the animation curve
        float currentBeatScale = beatCurve.Evaluate(currentTime / beatDuration);

        // Apply the scale
        transform.localScale = originalScale * currentBeatScale;
    }

    // Coroutine to animate the loading text
    private IEnumerator AnimateLoadingText()
    {
        int dotCount = 0;

        while (true)
        {
            /*if (GameManager._gM._Is_loading_questions)
            {
                loadingText.text = "Loading questions" + new string('.', dotCount);
            }
            else
            {
                loadingText.text = "Connecting to server" + new string('.', dotCount);
            }*/
            // Increment the dot count and reset after 3 dots
            dotCount = (dotCount + 1) % 4;

            // Wait for the specified time before updating the text again
            yield return new WaitForSeconds(dotAnimationSpeed);
        }
    }

    // Optional: Method to change beat speed at runtime
    public void SetBeatSpeed(float beatsPerSecond)
    {
        beatDuration = 1f / beatsPerSecond;
    }

    // Optional: Method to change beat intensity at runtime
    public void SetBeatIntensity(float intensity)
    {
        beatScale = 1f + intensity;
    }
}
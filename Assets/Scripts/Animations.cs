using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Added for Image component

public class Animations : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FadeOutAndDisable(GameObject targetObject)
    {
        StartCoroutine(FadeOutCoroutine(targetObject));
    }

    private IEnumerator FadeOutCoroutine(GameObject targetObject)
    {
        // Ensure object is active and get Image component
        targetObject.SetActive(true);
        Image image = targetObject.GetComponent<Image>();
        
        if (image == null)
        {
            Debug.LogError("No Image component found on target object");
            yield break;
        }

        // Store initial color and ensure full opacity at start
        Color startColor = image.color;
        startColor.a = 1f;
        image.color = startColor;
        
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        float elapsedTime = 0f;
        float duration = 2f;

        // Fade out over duration
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration;
            image.color = Color.Lerp(startColor, endColor, normalizedTime);
            yield return null;
        }

        // Ensure we reach full transparency
        image.color = endColor;
        targetObject.SetActive(false);
    }

    private Dictionary<GameObject, Coroutine> activeCoroutines = new Dictionary<GameObject, Coroutine>();

    public void PopupWarning(GameObject targetObject)
    {
        // If there's an existing coroutine for this object, stop it and reset the object
        if (activeCoroutines.ContainsKey(targetObject))
        {
            StopCoroutine(activeCoroutines[targetObject]);
            activeCoroutines.Remove(targetObject);
            
            // Reset the object's properties immediately
            Image image = targetObject.GetComponent<Image>();
            if (image != null)
            {
                Color color = image.color;
                color.a = 1f;
                image.color = color;
            }
        }
        
        // Start new coroutine and store it
        Coroutine newCoroutine = StartCoroutine(PopupWarningCoroutine(targetObject));
        activeCoroutines[targetObject] = newCoroutine;
    }

    private IEnumerator PopupWarningCoroutine(GameObject targetObject)
    {
        // Ensure object is active and get components
        targetObject.SetActive(true);
        Image image = targetObject.GetComponent<Image>();
        RectTransform rectTransform = targetObject.GetComponent<RectTransform>();
        
        if (image == null)
        {
            Debug.LogError("No Image component found on target object");
            yield break;
        }

        // Store initial values
        Color startColor = image.color;
        Vector3 originalScale = targetObject.transform.localScale;
        Vector2 originalSizeDelta = rectTransform != null ? rectTransform.sizeDelta : Vector2.zero;
        
        // Set initial state
        startColor.a = 1f;
        image.color = startColor;
        
        // Animation parameters
        float popupDuration = 0.3f;
        float displayDuration = 1.0f;
        float fadeOutDuration = 0.7f;
        float maxScaleMultiplier = 1.2f;
        
        // Popup animation
        float elapsedTime = 0f;
        while (elapsedTime < popupDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / popupDuration;
            
            // Use an ease-out curve for smooth animation
            float curveValue = 1f - Mathf.Pow(1f - normalizedTime, 2f);
            float scaleMultiplier = 1f + (maxScaleMultiplier - 1f) * (1f - curveValue);
            
            if (rectTransform != null)
            {
                // For UI elements with RectTransform, scale the sizeDelta
                rectTransform.sizeDelta = originalSizeDelta * scaleMultiplier;
            }
            else
            {
                // For regular GameObjects, scale the transform
                targetObject.transform.localScale = originalScale * scaleMultiplier;
            }
            
            yield return null;
        }
        
        // Reset scale to original
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = originalSizeDelta;
        }
        else
        {
            targetObject.transform.localScale = originalScale;
        }
        
        // Display duration
        yield return new WaitForSeconds(displayDuration);
        
        // Fade out
        elapsedTime = 0f;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeOutDuration;
            image.color = Color.Lerp(startColor, endColor, normalizedTime);
            yield return null;
        }
        
        // Reset all properties to original state
        image.color = startColor;
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = originalSizeDelta;
        }
        else
        {
            targetObject.transform.localScale = originalScale;
        }
        
        targetObject.SetActive(false);
        
        // At the end of the coroutine, remove it from the dictionary
        activeCoroutines.Remove(targetObject);
    }
}

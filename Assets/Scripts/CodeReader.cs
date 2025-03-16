using UnityEngine;
using TMPro;

public class CodeReader : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI codeText;

    void Start()
    {
        // Check for deep link
        Application.deepLinkActivated += OnDeepLinkActivated;
        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            OnDeepLinkActivated(Application.absoluteURL);
        }
    }

    private void OnDeepLinkActivated(string url)
    {
        // Parse the URL to get the code
        // URL format: rijschoolapp://code/TEST123
        string[] parts = url.Split(new[] { "code/" }, System.StringSplitOptions.None);
        if (parts.Length > 1)
        {
            string code = parts[1];
            Debug.Log($"Deep link code received: {code}");

            if (UnityAnalyticsManager.Instance != null)
            {
                UnityAnalyticsManager.Instance.InstructeurcodeQRCode(code);
            }
            else
            {
                Debug.LogError("UnityAnalyticsManager instance not found!");
            }
        }
    }
}
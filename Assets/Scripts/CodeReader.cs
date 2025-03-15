using UnityEngine;
using TMPro;

public class CodeReader : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI codeText;
    private string receivedCode = "";

    void Start()
    {
        if (codeText != null)
        {
            codeText.text = "Waiting for code...";
        }

        Application.deepLinkActivated += OnDeepLinkActivated;

        // Check if we were launched with a deep link
        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            OnDeepLinkActivated(Application.absoluteURL);
        }
    }

    private void OnDeepLinkActivated(string url)
    {
        // Update UI with received URL
        if (codeText != null)
        {
            codeText.text = "Received URL: " + url;
        }

        // Parse the URL to get the code
        if (url.Contains("code/"))
        {
            string[] parts = url.Split(new[] { "code/" }, System.StringSplitOptions.None);
            if (parts.Length > 1)
            {
                receivedCode = parts[1];
                if (codeText != null)
                {
                    codeText.text = "Code ontvangen: " + receivedCode;
                }

                if (UnityAnalyticsManager.Instance != null)
                {
                    UnityAnalyticsManager.Instance.InstructeurcodeQRCode(receivedCode);
                    codeText.text += "\nCode verstuurd naar Analytics";
                }
            }
        }
    }
}
using UnityEngine;
using TMPro;

public class CodeReader : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI codeText; // Referentie naar UI text element

    void Start()
    {
        if (codeText != null)
        {
            codeText.text = "Waiting for code...";
        }

        Application.deepLinkActivated += OnDeepLinkActivated;
        
        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            OnDeepLinkActivated(Application.absoluteURL);
        }
    }

    private void OnDeepLinkActivated(string url)
    {
        if (codeText != null)
        {
            codeText.text = "URL ontvangen: " + url;
        }

        // Parse the URL to get the code
        if (url.Contains("code/"))
        {
            string[] parts = url.Split(new[] { "code/" }, System.StringSplitOptions.None);
            if (parts.Length > 1)
            {
                string code = parts[1];
                if (codeText != null)
                {
                    codeText.text = "Code ontvangen: " + code;
                }

                if (UnityAnalyticsManager.Instance != null)
                {
                    UnityAnalyticsManager.Instance.InstructeurcodeQRCode(code);
                    codeText.text += "\nCode verstuurd naar Analytics";
                }
            }
        }
    }
}
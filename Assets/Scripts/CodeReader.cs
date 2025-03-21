using UnityEngine;
using TMPro;

public class CodeReader : MonoBehaviour
{
    void Start()
    {
        // Registreer voor deeplink events
        Application.deepLinkActivated += OnDeepLinkActivated;

        // Check of de app is geopend via een deeplink
        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            OnDeepLinkActivated(Application.absoluteURL);
        }
    }

    private void OnDeepLinkActivated(string url)
    {
        // Parse de URL om de code te krijgen
        // URL format: your-app-scheme://code/UNIQUE_CODE
        string code = url.Split('/').Length > 2 ? url.Split('/')[2] : "";
        if (!string.IsNullOrEmpty(code))
        {
            // Gebruik de code in je app
            Debug.Log("Ontvangen code: " + code);
            UnityAnalyticsManager.Instance.InstructeurcodeQRCode(code);
            //PlayerPrefs.SetString("QRCode", code);
            PlayerPrefs.Save();
        }
    }
}
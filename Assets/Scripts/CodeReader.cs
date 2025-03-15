using UnityEngine;
using TMPro;

public class CodeReader : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI codeText; // Referentie naar UI text element

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL specifieke code
            GetCodeFromLocalStorage();
#else
        // Android/iOS specifieke code
        GetCodeFromLocalStorage();
#endif
    }

    private void GetCodeFromLocalStorage()
    {
        // JavaScript functie aanroepen
        Application.ExternalEval(@"
            try {
                var code = localStorage.getItem('rijschoolAppCode');
                if (code) {
                    gameObject.SendMessage('OnCodeReceived', code);
                } else {
                    gameObject.SendMessage('OnCodeError', 'Geen code gevonden');
                }
            } catch (error) {
                gameObject.SendMessage('OnCodeError', error.message);
            }
        ");
    }

    // Deze functie wordt aangeroepen door JavaScript
    void OnCodeReceived(string code)
    {
        if (codeText != null)
        {
            codeText.text = "Code: " + code;
        }
        Debug.Log("Ontvangen code: " + code);
    }

    void OnCodeError(string error)
    {
        if (codeText != null)
        {
            codeText.text = "Error: " + error;
        }
        Debug.LogError("Error bij ophalen code: " + error);
    }
}
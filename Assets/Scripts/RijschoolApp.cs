using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;

public class RijschoolApp : MonoBehaviour
{
    public static RijschoolApp instance;
    public List<Color> leerlingKleuren = new List<Color>
    {
        new Color(0.95f, 0.23f, 0.23f),    // Bright Red
        new Color(0.13f, 0.35f, 0.13f),    // Dark Forest Green (darkened)
        new Color(0.20f, 0.30f, 0.95f),    // Royal Blue
        new Color(1.00f, 0.75f, 0.00f),    // Golden Yellow
        new Color(0.60f, 0.00f, 0.60f),    // Purple
        new Color(1.00f, 0.45f, 0.00f),    // Orange
        new Color(0.00f, 0.50f, 0.70f),    // Changed from Teal to Blue-leaning Teal
        new Color(0.85f, 0.44f, 0.84f),    // Orchid
        new Color(0.54f, 0.27f, 0.07f),    // Brown
        new Color(0.00f, 0.30f, 0.15f),    // Darker Sea Green (darkened)
        new Color(0.85f, 0.22f, 0.45f),    // Crimson
        new Color(0.42f, 0.35f, 0.80f),    // Slate Blue
        new Color(0.70f, 0.70f, 0.00f),    // Olive
        new Color(0.25f, 0.58f, 0.82f),    // Changed from Turquoise to Sky Blue
        new Color(0.93f, 0.51f, 0.93f),    // Violet
        new Color(1.00f, 0.55f, 0.41f),    // Coral
        new Color(0.18f, 0.31f, 0.31f),    // Dark Slate Gray
        new Color(0.94f, 0.90f, 0.55f),    // Khaki
        new Color(0.58f, 0.00f, 0.83f),    // Purple Blue
        new Color(0.80f, 0.36f, 0.36f),    // Indian Red
        new Color(0.15f, 0.40f, 0.25f),    // Darker Medium Sea Green (darkened)
        new Color(0.93f, 0.66f, 0.93f),    // Plum
        new Color(0.60f, 0.40f, 0.12f),    // Saddle Brown
        new Color(0.00f, 0.75f, 1.00f),    // Deep Sky Blue
        new Color(1.00f, 0.65f, 0.79f),    // Light Pink
        new Color(0.54f, 0.17f, 0.89f),    // Blue Violet
        new Color(0.80f, 0.52f, 0.25f),    // Peru
        new Color(0.29f, 0.69f, 0.00f),    // Changed from Lime Green to Darker Lime
        new Color(0.96f, 0.87f, 0.70f),    // Wheat
        new Color(0.25f, 0.41f, 0.88f)     // Royal Blue
    };
    [SerializeField] private List<GameObject> schermen; // StartScherm, LeraarScherm, LeerlingScherm, Rooster
    [SerializeField] private List<TMP_InputField> maakRijschoolInputfields;
    [SerializeField] private List<GameObject> rijscholenUI;
    [SerializeField] private TMP_InputField voegLeerlingToeNaam;
    [SerializeField] private TextMeshProUGUI voegLeerlingToeFrequentie;
    [SerializeField] private TMP_InputField voegLeerlingToeMinutesPerLes;
    [SerializeField] private Transform rijscholenUIParent;
    [SerializeField] private List<GameObject> leerlingenPool;
    [SerializeField] private GameObject nogGeenRijschoolWaarschuwing;
    [SerializeField] private GameObject rooster;
    [SerializeField] private GameObject warningMessage;
    //[SerializeField] private GameObject leerlingWaarschuwing;
    [SerializeField] private GameObject correctPasswordButton;
    [SerializeField] private GameObject wrongPasswordButton;
    [SerializeField] private GameObject maakRijschoolGameobject;
    [SerializeField] private GameObject mijnRijschoolButton;

    private List<Rijschool> alleRijscholen;
    private List<Rijschool> filteredRijscholen;

    public Rijschool selectedRijschool;
    public Leerling selectedLeerling;
    private string apiUrl = "https://rijschoolapp.onrender.com/api/rijscholen";

    

    private async void Start()
    {
        await LoadRijscholen();
        instance = this;
        SetSchermActive(true, false, false, false);
        selectedRijschool = new Rijschool();
        selectedRijschool.leerlingen = new List<Leerling>();
        
        // Initialize lists to prevent null reference
        alleRijscholen = new List<Rijschool>();
        filteredRijscholen = new List<Rijschool>();

        // Load rijscholen at start
        await LoadRijscholen();

        LoadLeerlingen();
    }

    public void SetSchermActive(bool start, bool leraar, bool leerling, bool rooster)
    {
        schermen[0].SetActive(start);
        schermen[1].SetActive(leraar);
        schermen[2].SetActive(leerling);
        schermen[3].SetActive(rooster);
    }

    private async Task LoadRijscholen()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(apiUrl))
        {
            try
            {
                await www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = www.downloadHandler.text;
                    ListRijschool rijscholen = JsonUtility.FromJson<ListRijschool>("{\"rijschoolList\":" + jsonResponse + "}");
                    alleRijscholen = rijscholen.rijschoolList;
                    filteredRijscholen = alleRijscholen;
                    UpdateRijscholenUI(alleRijscholen);
                }
                else
                {
                    Debug.LogError($"Error loading rijscholen: {www.error}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception loading rijscholen: {e.Message}");
            }
        }
    }

    public void LoadMijnRijschool()
    {
        if(!PlayerPrefs.HasKey("MijnRijschool"))
        {
            nogGeenRijschoolWaarschuwing.SetActive(true);
        }
        else
        {
            string rijschoolNaam = PlayerPrefs.GetString("MijnRijschool");
            // Find the rijschool in alleRijscholen
            selectedRijschool = alleRijscholen.FirstOrDefault(r => 
                r.naam.Equals(rijschoolNaam, System.StringComparison.OrdinalIgnoreCase));
            
            if (selectedRijschool != null)
            {
                rooster.SetActive(true);
                Rooster.instance.RoosterForInstructors(true);
                Rooster.instance.LoadLessen();
                SetSchermActive(false, false, false, true);

            }
            else
            {
                Debug.Log($"Could not find rijschool with name: {rijschoolNaam}");
                nogGeenRijschoolWaarschuwing.SetActive(true);
            }
        }
    }

    public void LeerlingFrequentiePlus(int leerling)
    {
        voegLeerlingToeFrequentie.text = Mathf.Clamp(int.Parse(voegLeerlingToeFrequentie.text) + 1,0,20).ToString();
    }
    public void LeerlingFrequentieMin(int leerling)
    {
        voegLeerlingToeFrequentie.text = Mathf.Clamp(int.Parse(voegLeerlingToeFrequentie.text) - 1, 0, 20).ToString();
    }

    public async void SaveLeerling()
    {
        string naam = voegLeerlingToeNaam.text;
        int frequentie = int.Parse(voegLeerlingToeFrequentie.text);
        
        if (selectedRijschool != null)
        {
            // Parse the minutes per les, with a default of 60 if parsing fails
            int minutesPerLes = 60;
            if (!string.IsNullOrEmpty(voegLeerlingToeMinutesPerLes.text))
            {
                int.TryParse(voegLeerlingToeMinutesPerLes.text, out minutesPerLes);
            }

            Leerling nieuweLeerling = new Leerling
            {
                naam = naam,
                frequentie = frequentie,
                colorIndex = GetNextAvailableColorIndex(),
                minutesPerLes = minutesPerLes  // Set the new value
            };
            
            if (selectedRijschool.leerlingen == null)
            {
                selectedRijschool.leerlingen = new List<Leerling>();
            }
            
            selectedRijschool.leerlingen.Add(nieuweLeerling);
            Debug.Log($"Leerling {naam} toegevoegd aan {selectedRijschool.naam}");

            // Clear input fields after successful save
            voegLeerlingToeNaam.text = "";
            voegLeerlingToeFrequentie.text = "0";
            voegLeerlingToeMinutesPerLes.text = "60";  // Reset to default value
            
            // Only update the server if the rijschool already exists (has an ID/name)
            if (alleRijscholen.Any(r => r.naam == selectedRijschool.naam))
            {
                await UpdateRijschool(selectedRijschool);
            }
            
            LoadLeerlingen();
        }
        else
        {
            Debug.LogWarning("Geen rijschool geselecteerd!");
        }
    }

    private int GetNextAvailableColorIndex()
    {
        int colorIndex = 0;
        if (selectedRijschool.leerlingen != null && selectedRijschool.leerlingen.Count > 0)
        {
            var usedIndices = selectedRijschool.leerlingen.Select(l => l.colorIndex).ToList();
            while (usedIndices.Contains(colorIndex))
            {
                colorIndex = (colorIndex + 1) % leerlingKleuren.Count;
            }
        }
        return colorIndex;
    }

    public void VerwijderLeerling()
    {
        if (selectedRijschool != null && selectedLeerling != null)
        {
            selectedRijschool.leerlingen.Remove(selectedLeerling);
            selectedLeerling = null;
            Debug.Log("Leerling verwijderd");
        }
        else
        {
            Debug.LogWarning("Geen rijschool of leerling geselecteerd!");
        }
    }

    public async void MaakRijschool()
    {
        string newName = maakRijschoolInputfields[0].text;
        string password = maakRijschoolInputfields[2].text;

        TextMeshProUGUI warningtext = warningMessage.GetComponentInChildren<TextMeshProUGUI>();

        // Check if password is empty
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(newName))
        {
            warningMessage.SetActive(true); 
            warningtext.text = "Vul alle verplichte velden in";
            return;
        }
        
        // Check if a rijschool with this name already exists (case insensitive)
        bool nameExists = alleRijscholen.Any(r => r.naam.Equals(newName, System.StringComparison.OrdinalIgnoreCase));

        if (nameExists)
        {
            warningMessage.SetActive(true);
            warningtext.text = "Rijschool bestaat al. Kies een andere naam";
            return;
        }
        
        warningMessage.SetActive(false);
        selectedRijschool = new Rijschool();
        selectedRijschool.leerlingen = new List<Leerling>();
        selectedRijschool.naam = newName;
        selectedRijschool.beschrijving = maakRijschoolInputfields[1].text;
        selectedRijschool.wachtwoord = password;

        string jsonData = JsonUtility.ToJson(selectedRijschool);

        using (UnityWebRequest www = new UnityWebRequest(apiUrl, "POST"))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                await www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Rijschool succesvol aangemaakt!");
                    // Save the rijschool name in PlayerPrefs
                    PlayerPrefs.SetString("MijnRijschool", newName);
                    PlayerPrefs.Save();

                    // Reload rijscholen to get the newly created one
                    await LoadRijscholen();

                    // Find and set the newly created rijschool as selected
                    selectedRijschool = alleRijscholen.FirstOrDefault(r => 
                        r.naam.Equals(newName, System.StringComparison.OrdinalIgnoreCase));

                    // Clear input fields
                    foreach (var inputField in maakRijschoolInputfields)
                    {
                        inputField.text = "";
                    }

                    // Now you can proceed with adding students
                    LoadLeerlingen();
                    LoadMijnRijschool();
                    maakRijschoolGameobject.SetActive(false);
                    mijnRijschoolButton.SetActive(true);
                }
                else
                {
                    Debug.LogError($"Error: {www.error}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception: {e.Message}");
            }
        }
    }

    private void UpdateRijscholenUI(List<Rijschool> rijscholenList)
    {
        foreach(GameObject obj in rijscholenUI)
        {
            obj.SetActive(false);
        }
        for (int i = 0; i < rijscholenList.Count && i < rijscholenUI.Count; i++)
        {
            GameObject uiElement = rijscholenUI[i];
            uiElement.SetActive(true);
            
            TextMeshProUGUI[] texts = uiElement.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = rijscholenList[i].naam;
            texts[1].text = rijscholenList[i].beschrijving;
        }
    }

    public void ZoekRijschool(string zoekTerm)
    {
        if (string.IsNullOrEmpty(zoekTerm))
        {
            filteredRijscholen = alleRijscholen;
        }
        else
        {
            print((filteredRijscholen == null) + " " + (alleRijscholen == null));
            filteredRijscholen = alleRijscholen
                .Where(r => r.naam.ToLower().Contains(zoekTerm.ToLower()))
                .ToList();
        }
        
        // Reset all UI elements
        foreach(GameObject obj in rijscholenUI)
        {
            obj.SetActive(false);
        }
        
        UpdateRijscholenUI(filteredRijscholen);
    }

    public void SelectRijschool(int index)
    {
        selectedRijschool = alleRijscholen[index];
    }

    public void CheckPassword(string password)
    {
        if (selectedRijschool != null && password == selectedRijschool.wachtwoord)
        {
            correctPasswordButton.SetActive(true);
            wrongPasswordButton.SetActive(false);
        }
        else
        {
            correctPasswordButton.SetActive(false);
            wrongPasswordButton.SetActive(true);
        }
    }

    public void LoadLeerlingen()
    {
        foreach(GameObject obj in leerlingenPool)
        {
            obj.SetActive(false);
        }
        foreach(Leerling leerling in selectedRijschool.leerlingen)
        {
            int index = selectedRijschool.leerlingen.IndexOf(leerling);
            GameObject UIelement = leerlingenPool[index];
            UIelement.SetActive(true);

            UnityEngine.UI.Image image = UIelement.GetComponent<UnityEngine.UI.Image>();
            image.color = leerlingKleuren[leerling.colorIndex];

            TextMeshProUGUI naamtext = UIelement.GetComponentsInChildren<TextMeshProUGUI>()[0];
            naamtext.text = leerling.naam;

            TextMeshProUGUI frequentietext = UIelement.GetComponentsInChildren<TextMeshProUGUI>()[1];
            frequentietext.text = leerling.frequentie.ToString();
        }
    }

    public async Task UpdateRijschool(Rijschool rijschool)
    {
        string jsonData = JsonUtility.ToJson(rijschool);
        Debug.Log($"Sending update to server: {jsonData}");
        
        // Log specific availability data
        if (rijschool.instructeurBeschikbaarheid != null)
        {
            Debug.Log($"Instructor availability count: {rijschool.instructeurBeschikbaarheid.Count}");
            foreach (var beschikbaarheid in rijschool.instructeurBeschikbaarheid)
            {
                Debug.Log($"Day: {beschikbaarheid.dag}, Slots: {beschikbaarheid.tijdslots?.Count ?? 0}");
            }
        }

        using (UnityWebRequest www = UnityWebRequest.Put($"{apiUrl}/{rijschool.naam}", jsonData))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            try
            {
                await www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Rijschool updated successfully");
                    Debug.Log($"Server response: {www.downloadHandler.text}");
                }
                else
                {
                    Debug.LogError($"Error updating rijschool: {www.error}");
                    Debug.LogError($"Response: {www.downloadHandler.text}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception during rijschool update: {e.Message}");
            }
        }
    }

    public Color GetLeerlingColor(int index)
    {
        if (index < 0 || index >= leerlingKleuren.Count)
            return Color.white;
        return leerlingKleuren[index];
    }

    public void SelectLeerling(int leerlingIndex)
    {
        if (selectedRijschool != null && leerlingIndex < selectedRijschool.leerlingen.Count)
        {
            selectedLeerling = selectedRijschool.leerlingen[leerlingIndex];
            Debug.Log($"Selected leerling: {selectedLeerling.naam}");
        }
    }

    public async void DecreaseLeerlingFrequentie(int leerlingIndex)
    {
        if (selectedRijschool?.leerlingen == null || leerlingIndex >= selectedRijschool.leerlingen.Count)
        {
            Debug.LogWarning("Invalid rijschool or leerling index");
            return;
        }

        // Decrease and clamp frequentie between 0 and 10
        selectedRijschool.leerlingen[leerlingIndex].frequentie = 
            Mathf.Clamp(selectedRijschool.leerlingen[leerlingIndex].frequentie - 1, 0, 10);

        // Update UI
        LoadLeerlingen();

        // Save changes to server if rijschool exists in database
        if (alleRijscholen.Any(r => r.naam == selectedRijschool.naam))
        {
            await UpdateRijschool(selectedRijschool);
        }
    }

    public async void IncreaseLeerlingFrequentie(int leerlingIndex)
    {
        if (selectedRijschool?.leerlingen == null || leerlingIndex >= selectedRijschool.leerlingen.Count)
        {
            Debug.LogWarning("Invalid rijschool or leerling index");
            return;
        }

        // Increase and clamp frequentie between 0 and 10
        selectedRijschool.leerlingen[leerlingIndex].frequentie = 
            Mathf.Clamp(selectedRijschool.leerlingen[leerlingIndex].frequentie + 1, 0, 10);

        // Update UI
        LoadLeerlingen();

        // Save changes to server if rijschool exists in database
        if (alleRijscholen.Any(r => r.naam == selectedRijschool.naam))
        {
            await UpdateRijschool(selectedRijschool);
        }
    }

    public async void RemoveLeerling(int leerlingIndex)
    {
        if (selectedRijschool?.leerlingen == null || leerlingIndex >= selectedRijschool.leerlingen.Count)
        {
            Debug.LogWarning("Invalid rijschool or leerling index");
            return;
        }

        // Remove the leerling from the list
        selectedRijschool.leerlingen.RemoveAt(leerlingIndex);

        // Update UI
        LoadLeerlingen();

        // Save changes to server if rijschool exists in database
        if (alleRijscholen.Any(r => r.naam == selectedRijschool.naam))
        {
            await UpdateRijschool(selectedRijschool);
        }
    }
}

[System.Serializable]
public class ListRijschool
{
    public List<Rijschool> rijschoolList;
}

[System.Serializable]
public class Leerling
{
    public string naam;
    public int frequentie;
    public int colorIndex;
    public int minutesPerLes = 60;
    public List<Beschikbaarheid> beschikbaarheid;

    public Leerling()
    {
        beschikbaarheid = new List<Beschikbaarheid>();
    }
}

[System.Serializable]
public class Beschikbaarheid
{
    public string dag;
    public int weekNummer;
    public int jaar;
    public List<TimeSlot> tijdslots;

    public Beschikbaarheid()
    {
        tijdslots = new List<TimeSlot>();
    }
}

[System.Serializable]
public class TimeSlot
{
    public string startTijd;
    public string eindTijd;
}

[System.Serializable]
public class Les
{
    public string begintijd;
    public string eindtijd;
    public string notities;
    public string datum;        // Format: "dd-MM-yyyy"
    public int weekNummer;      // 1-52
    public string leerlingId;   
    public string leerlingNaam;
    public bool isAutomatischGepland;
    public List<Leerling> gereserveerdDoorLeerling; // New property for student reservations
    
    public Les()
    {
        System.DateTime now = System.DateTime.Now;
        datum = now.ToString("dd-MM-yyyy");
        weekNummer = System.Globalization.ISOWeek.GetWeekOfYear(now);
        gereserveerdDoorLeerling = new List<Leerling>();
    }
}

[System.Serializable]
public class Week
{
    public List<Les> lessen;
    public int weekNummer;
    public int jaar;
    
    public Week()
    {
        lessen = new List<Les>();
    }
}

[System.Serializable]
public class LesRooster
{
    public List<Week> weken;
    
    public LesRooster()
    {
        InitializeRooster();
    }

    private void InitializeRooster()
    {
        weken = new List<Week>();
        
        // Get current week and year
        System.DateTime now = System.DateTime.Now;
        int currentWeek = System.Globalization.ISOWeek.GetWeekOfYear(now);
        int currentYear = now.Year;

        // Initialize 26 weeks
        for (int i = 0; i < 26; i++)
        {
            int weekNum = (currentWeek + i) % 52;
            if (weekNum == 0) weekNum = 52;
            
            int year = currentYear;
            if (currentWeek + i > 52) year++;

            Week week = new Week { weekNummer = weekNum, jaar = year };
            weken.Add(week);
        }
    }

    public List<Les> GetLessenForWeek(int weekNum)
    {
        Week week = weken.Find(w => w.weekNummer == weekNum);
        return week?.lessen ?? new List<Les>();
    }
}

[System.Serializable]
public class Rijschool
{
    public string naam;
    public string beschrijving;
    public string wachtwoord;
    public List<Leerling> leerlingen;
    public LesRooster rooster;
    public List<Beschikbaarheid> instructeurBeschikbaarheid;

    public Rijschool()
    {
        leerlingen = new List<Leerling>();
        rooster = new LesRooster();
        instructeurBeschikbaarheid = new List<Beschikbaarheid>();
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using System;
using UnityEngine.UI;

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
    [SerializeField] private TMP_InputField voegLeerlingToeWoonplaats;
    [SerializeField] private TMP_InputField voegLeerlingToeAdres;
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
    [SerializeField] private GameObject eigenRijschoolIndicator; // This will be shown when viewing own driving school
    [SerializeField] private TextMeshProUGUI ingelogdAlsText;
    [SerializeField] private GameObject maakrijschoolbevestigen;
    [SerializeField] private List<TMP_InputField> editRijschoolInputFields; // Name, Description, Password, Woonplaats
    [SerializeField] private GameObject nameExistsWarning;
    [SerializeField] private GameObject klikMijnRijschool;
    [SerializeField] private GameObject klikMijnRijschoolButton; // do not delete
    [SerializeField] private GameObject leerlingNaamWaarschuwing;
    [SerializeField] private TextMeshProUGUI weekOffsetText;
    [SerializeField] private GameObject drivingSchoolPassword;
    [SerializeField] private GameObject studentPassword;
    [SerializeField] private List<GameObject> woonplaatsSettings; // Add this field
    [SerializeField] private TMP_InputField startWoonplaatsen;
    [SerializeField] private TMP_InputField eindWoonplaatsen;
    [SerializeField] private GameObject RoosterLeerlingenButton;
    private int pendingLeerlingRemoval = -1;  // Store the index of student pending removal

    private List<Rijschool> alleRijscholen;
    private List<Rijschool> filteredRijscholen;

    public Rijschool selectedRijschool;
    public Leerling selectedLeerling;
    private string apiUrl = "https://rijschoolapp.onrender.com/api/rijscholen";

    

    private async void Start()
    {
        await LoadRijscholen();
        instance = this;
        
        // Check if user is an instructor (has their own driving school)
        if (PlayerPrefs.HasKey("MijnRijschool"))
        {
            string rijschoolNaam = PlayerPrefs.GetString("MijnRijschool");
            selectedRijschool = alleRijscholen.FirstOrDefault(r => 
                r.naam.Equals(rijschoolNaam, System.StringComparison.OrdinalIgnoreCase));
            
            if (selectedRijschool != null)
            {
                SetSchermActive(false, false, false, true);
                ingelogdAlsText.gameObject.SetActive(false);
                Rooster.instance.RoosterForInstructors(true);
                Rooster.instance.LoadLessen();
            }
            else
            {
                SetSchermActive(true, false, false, false);
            }
        }
        // Check if user is a previously logged-in student
        else if (PlayerPrefs.HasKey("LastStudentName") && 
                 PlayerPrefs.HasKey("LastStudentSchool") && 
                 PlayerPrefs.HasKey("LastStudentPassword"))
        {
            string schoolName = PlayerPrefs.GetString("LastStudentSchool");
            string studentName = PlayerPrefs.GetString("LastStudentName");
            string studentPass = PlayerPrefs.GetString("LastStudentPassword");

            selectedRijschool = alleRijscholen.FirstOrDefault(r => 
                r.naam.Equals(schoolName, System.StringComparison.OrdinalIgnoreCase));

            if (selectedRijschool?.leerlingen != null)
            {
                var student = selectedRijschool.leerlingen.FirstOrDefault(l => 
                    l.naam.Equals(studentName, StringComparison.OrdinalIgnoreCase) && 
                    l.wachtwoord.Equals(studentPass, StringComparison.OrdinalIgnoreCase));

                if (student != null)
                {
                    selectedLeerling = student;
                    SetSchermActive(false, false, false, true);
                    Rooster.instance.RoosterForInstructors(false);
                    Rooster.instance.LoadKiesLeerlingButtons(student);
                    if (ingelogdAlsText != null)
                    {
                        ingelogdAlsText.text = "Ingelogd als: " + student.naam;
                    }
                }
                else
                {
                    SetSchermActive(true, false, false, false);
                }
            }
            else
            {
                SetSchermActive(true, false, false, false);
            }
        }
        else
        {
            SetSchermActive(true, false, false, false);
        }

        // Initialize lists to prevent null reference
        alleRijscholen = new List<Rijschool>();
        filteredRijscholen = new List<Rijschool>();

        // Set button interactable state based on whether user has a driving school
        if (maakrijschoolbevestigen != null)
        {
            UnityEngine.UI.Button button = maakrijschoolbevestigen.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                klikMijnRijschoolButton.SetActive(PlayerPrefs.HasKey("MijnRijschool"));
                button.interactable = !PlayerPrefs.HasKey("MijnRijschool");
            }
        }

        // Load rijscholen at start
        await LoadRijscholen();

        Rooster.instance.UpdateOverzichtLeerlingen();

        // Initialize edit input fields if they exist
        if (editRijschoolInputFields != null && editRijschoolInputFields.Count >= 4)
        {
            UpdateEditRijschoolFields();
        }

        // Initialize woonplaats settings if they don't exist
        if (!PlayerPrefs.HasKey("StartInWoonplaats"))
        {
            PlayerPrefs.SetInt("StartInWoonplaats", 0);
        }
        if (!PlayerPrefs.HasKey("EindInWoonplaats"))
        {
            PlayerPrefs.SetInt("EindInWoonplaats", 0);
        }
        if (!PlayerPrefs.HasKey("StartWoonplaatsen"))
        {
            PlayerPrefs.SetString("StartWoonplaatsen", "");
        }
        if (!PlayerPrefs.HasKey("EindWoonplaatsen"))
        {
            PlayerPrefs.SetString("EindWoonplaatsen", "");
        }

        // Set input field values from PlayerPrefs
        if (startWoonplaatsen != null)
        {
            startWoonplaatsen.text = PlayerPrefs.GetString("StartWoonplaatsen");
        }
        if (eindWoonplaatsen != null)
        {
            eindWoonplaatsen.text = PlayerPrefs.GetString("EindWoonplaatsen");
        }

        // Update UI based on settings
        UpdateWoonplaatsSettingsUI();

        UpdateRoosterLeerlingenButtonVisibility();
    }

    public void SetSchermActive(bool start, bool leraar, bool leerling, bool rooster)
    {
        // Track screen exit for current screen
        if (schermen[0].activeSelf) UnityAnalyticsManager.Instance.TrackScreenExit("StartScherm");
        if (schermen[1].activeSelf) UnityAnalyticsManager.Instance.TrackScreenExit("LeraarScherm");
        if (schermen[2].activeSelf) UnityAnalyticsManager.Instance.TrackScreenExit("LeerlingScherm");
        if (schermen[3].activeSelf) UnityAnalyticsManager.Instance.TrackScreenExit("RoosterScherm");

        // Set new screen states
        schermen[0].SetActive(start);
        schermen[1].SetActive(leraar);
        schermen[2].SetActive(leerling);
        schermen[3].SetActive(rooster);

        // Track screen view for new active screen
        if (start) UnityAnalyticsManager.Instance.TrackScreenView("StartScherm");
        if (leraar) UnityAnalyticsManager.Instance.TrackScreenView("LeraarScherm");
        if (leerling) UnityAnalyticsManager.Instance.TrackScreenView("LeerlingScherm");
        if (rooster) UnityAnalyticsManager.Instance.TrackScreenView("RoosterScherm");
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

                    // Check if user has their own driving school
                    if (PlayerPrefs.HasKey("MijnRijschool"))
                    {
                        string eigenRijschoolNaam = PlayerPrefs.GetString("MijnRijschool");
                        
                        // Find the instructor's driving school
                        Rijschool eigenRijschool = alleRijscholen.FirstOrDefault(r => 
                            r.naam.Equals(eigenRijschoolNaam, System.StringComparison.OrdinalIgnoreCase));

                        if (eigenRijschool != null)
                        {
                            // Remove the driving school from its current position
                            alleRijscholen.Remove(eigenRijschool);
                            // Add it to the beginning of the list
                            alleRijscholen.Insert(0, eigenRijschool);
                            filteredRijscholen = alleRijscholen;

                            // Show the indicator if it exists
                            if (eigenRijschoolIndicator != null)
                            {
                                eigenRijschoolIndicator.SetActive(true);
                            }
                        }
                    }
                    else if (eigenRijschoolIndicator != null)
                    {
                        eigenRijschoolIndicator.SetActive(false);
                    }

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
            selectedRijschool = alleRijscholen.FirstOrDefault(r => 
                r.naam.Equals(rijschoolNaam, System.StringComparison.OrdinalIgnoreCase));
            
            if (selectedRijschool != null)
            {
                UnityAnalyticsManager.Instance.TrackDrivingSchoolAccess(rijschoolNaam);
                UnityAnalyticsManager.Instance.TrackScreenView("RoosterScherm");
                rooster.SetActive(true);
                Rooster.instance.RoosterForInstructors(true);
                Rooster.instance.LoadLessen();
                SetSchermActive(false, false, false, true);
                UpdateEditRijschoolFields();
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

    private string GenerateUniquePassword(string studentName, List<Leerling> existingStudents)
    {
        System.Random random = new System.Random();
        string password;
        bool isUnique;
        
        do
        {
            // Generate a 6-digit number
            int randomNumber = random.Next(1000, 10000);
            password = studentName.Substring(0,1).ToUpper() + randomNumber.ToString();
            
            // Check if this password is unique
            isUnique = !existingStudents.Any(s => s.wachtwoord == password);
        } while (!isUnique);
        
        return password;
    }

    public async void SaveLeerling()
    {
        string naam = voegLeerlingToeNaam.text;
        int frequentie = int.Parse(voegLeerlingToeFrequentie.text);
        
        // Get the warning text component
        TextMeshProUGUI warningText = leerlingNaamWaarschuwing.GetComponentInChildren<TextMeshProUGUI>();
        
        // Check for empty name
        if (string.IsNullOrWhiteSpace(naam))
        {
            leerlingNaamWaarschuwing.SetActive(true);
            warningText.text = "Vul een geldige naam in";
            return;
        }

        if (selectedRijschool != null)
        {
            // Check if name already exists (case insensitive)
            bool nameExists = selectedRijschool.leerlingen?.Any(l => 
                l.naam.Equals(naam, System.StringComparison.OrdinalIgnoreCase)) ?? false;

            if (nameExists)
            {
                leerlingNaamWaarschuwing.SetActive(true);
                warningText.text = "Naam wordt al gebruikt";
                return;
            }

            // Hide warning if we got this far
            leerlingNaamWaarschuwing.SetActive(false);

            // Parse the minutes per les, with a default of 60 if parsing fails
            int minutesPerLes = 60;
            if (!string.IsNullOrEmpty(voegLeerlingToeMinutesPerLes.text))
            {
                int.TryParse(voegLeerlingToeMinutesPerLes.text, out minutesPerLes);
            }

            // Generate unique password for the new student
            string uniquePassword = GenerateUniquePassword(naam, selectedRijschool.leerlingen ?? new List<Leerling>());

            Leerling nieuweLeerling = new Leerling
            {
                naam = naam,
                frequentie = frequentie,
                colorIndex = GetNextAvailableColorIndex(),
                minutesPerLes = minutesPerLes,
                wachtwoord = uniquePassword,
                woonPlaats = voegLeerlingToeWoonplaats.text, // Set the woonplaats from the input field
                adres = voegLeerlingToeAdres.text
            };
            
            if (selectedRijschool.leerlingen == null)
            {
                selectedRijschool.leerlingen = new List<Leerling>();
            }
            
            selectedRijschool.leerlingen.Add(nieuweLeerling);
            UnityAnalyticsManager.Instance.TrackStudentCreation(naam, frequentie, minutesPerLes);
            UnityAnalyticsManager.Instance.TrackStudentAdded(selectedRijschool.naam, naam);
            Debug.Log($"Leerling {naam} toegevoegd aan {selectedRijschool.naam} met wachtwoord {uniquePassword}");

            // Only reset the name input field, keep frequency and minutesPerLes
            voegLeerlingToeNaam.text = "";
            
            // Only update the server if the rijschool already exists (has an ID/name)
            if (alleRijscholen.Any(r => r.naam == selectedRijschool.naam))
            {
                await UpdateRijschool(selectedRijschool);
            }
            
            Rooster.instance.UpdateOverzichtLeerlingen();
        }
        else
        {
            Debug.LogWarning("Geen rijschool geselecteerd!");
        }

        UpdateRoosterLeerlingenButtonVisibility();
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
            UnityAnalyticsManager.Instance.TrackStudentRemoved(selectedRijschool.naam, selectedLeerling.naam);
            selectedRijschool.leerlingen.Remove(selectedLeerling);
            selectedLeerling = null;
            Debug.Log("Leerling verwijderd");
        }
        else
        {
            Debug.LogWarning("Geen rijschool of leerling geselecteerd!");
        }
    }

    public void KlikMijnRijschoolActive()
    {
        klikMijnRijschool.SetActive(PlayerPrefs.HasKey("MijnRijschool"));
    }

    public async void MaakRijschool()
    {
        string newName = maakRijschoolInputfields[0].text;
        string description = maakRijschoolInputfields[1].text;
        string woonplaats = maakRijschoolInputfields[2].text;
        string password = maakRijschoolInputfields[3].text;

        TextMeshProUGUI warningtext = warningMessage.GetComponentInChildren<TextMeshProUGUI>();

        // Check if password is empty
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(newName))
        {
            warningMessage.SetActive(true); 
            warningtext.text = "Vul alle verplichte velden in";
            nameExistsWarning.SetActive(false);
            return;
        }
        
        // Check if a rijschool with this name already exists (case insensitive)
        bool nameExists = alleRijscholen.Any(r => r.naam.Equals(newName, System.StringComparison.OrdinalIgnoreCase));

        if (nameExists)
        {
            warningMessage.SetActive(true);
            //nameExistsWarning.SetActive(true);
            return;
        }
        
        warningMessage.SetActive(false);
        nameExistsWarning.SetActive(false);
        selectedRijschool = new Rijschool();
        selectedRijschool.leerlingen = new List<Leerling>();
        selectedRijschool.naam = newName;
        selectedRijschool.beschrijving = description;
        selectedRijschool.woonPlaats = woonplaats;
        selectedRijschool.wachtwoord = password;
        selectedRijschool.LLzienLessen = false;  // Default value

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
                    UnityAnalyticsManager.Instance.TrackDrivingSchoolCreation(newName);
                    Debug.Log("Rijschool succesvol aangemaakt!");
                    // Save the rijschool name in PlayerPrefs
                    PlayerPrefs.SetString("MijnRijschool", newName);
                    PlayerPrefs.Save();

                    // Set the confirmation button to non-interactable
                    if (maakrijschoolbevestigen != null)
                    {
                        UnityEngine.UI.Button button = maakrijschoolbevestigen.GetComponent<UnityEngine.UI.Button>();
                        if (button != null)
                        {
                            klikMijnRijschoolButton.SetActive(PlayerPrefs.HasKey("MijnRijschool"));
                            button.interactable = false;
                        }
                    }

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
                    Rooster.instance.UpdateOverzichtLeerlingen();
                    LoadMijnRijschool();
                    maakRijschoolGameobject.SetActive(false);
                    mijnRijschoolButton.SetActive(true);
                }
                else
                {
                    UnityAnalyticsManager.Instance.TrackAPIFailure("create_driving_school", www.error);
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
            // Show indicator if user has their own driving school and it's in the list
            if (PlayerPrefs.HasKey("MijnRijschool") && eigenRijschoolIndicator != null)
            {
                string eigenRijschoolNaam = PlayerPrefs.GetString("MijnRijschool");
                eigenRijschoolIndicator.SetActive(filteredRijscholen.Any(r => 
                    r.naam.Equals(eigenRijschoolNaam, System.StringComparison.OrdinalIgnoreCase)));
            }
        }
        else
        {
            // Filter the list based on search term
            filteredRijscholen = alleRijscholen
                .Where(r => r.naam.ToLower().Contains(zoekTerm.ToLower()))
                .ToList();

            // Hide indicator since we're showing filtered results
            if (eigenRijschoolIndicator != null)
            {
                eigenRijschoolIndicator.SetActive(false);
            }
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
        // Use the filtered list instead of alleRijscholen
        if (index >= 0 && index < filteredRijscholen.Count)
        {
            selectedRijschool = filteredRijscholen[index];
            UpdateEditRijschoolFields();
        }
    }

    public void CheckBothPasswords()
    {
        if (selectedRijschool == null) return;

        // Get passwords from input fields
        string schoolPass = drivingSchoolPassword.GetComponent<TMP_InputField>().text;
        string studentPass = studentPassword.GetComponent<TMP_InputField>().text;

        // Only proceed with password check if both fields are filled in
        if (string.IsNullOrEmpty(schoolPass) || string.IsNullOrEmpty(studentPass))
        {
            correctPasswordButton.SetActive(false);
            wrongPasswordButton.SetActive(false);
            return;
        }

        // Check if driving school password matches (case insensitive)
        if (schoolPass.Equals(selectedRijschool.wachtwoord, StringComparison.OrdinalIgnoreCase) && 
            selectedRijschool.leerlingen != null)
        {
            // Find student with matching password (case insensitive)
            var matchingStudent = selectedRijschool.leerlingen
                .FirstOrDefault(l => l.wachtwoord.Equals(studentPass, StringComparison.OrdinalIgnoreCase));

            if (matchingStudent != null)
            {
                // Both passwords are correct
                correctPasswordButton.SetActive(true);
                wrongPasswordButton.SetActive(false);

                // Set the selected student
                selectedLeerling = matchingStudent;
                
                // Store student login information in PlayerPrefs
                PlayerPrefs.SetString("LastStudentName", matchingStudent.naam);
                PlayerPrefs.SetString("LastStudentSchool", selectedRijschool.naam);
                PlayerPrefs.SetString("LastStudentPassword", studentPass);
                PlayerPrefs.Save();

                // Update the logged-in text
                if (ingelogdAlsText != null)
                {
                    ingelogdAlsText.text = "Ingelogd als: " + matchingStudent.naam;
                }

                // Load only this student's information
                Rooster.instance.LoadKiesLeerlingButtons(matchingStudent);
            }
            else
            {
                // School password correct, but student password wrong
                UnityAnalyticsManager.Instance.TrackLoginFailure("student", "invalid_password");
                correctPasswordButton.SetActive(false);
                wrongPasswordButton.SetActive(true);
            }
        }
        else
        {
            // School password incorrect
            UnityAnalyticsManager.Instance.TrackLoginFailure("school", "invalid_password");
            correctPasswordButton.SetActive(false);
            wrongPasswordButton.SetActive(true);
        }
    }

    public async Task UpdateRijschool(Rijschool rijschool)
    {
        string jsonData = JsonUtility.ToJson(rijschool);
        //Debug.Log($"Sending update to server: {jsonData}");
        
        // Log specific availability data
        if (rijschool.instructeurBeschikbaarheid != null)
        {
            //Debug.Log($"Instructor availability count: {rijschool.instructeurBeschikbaarheid.Count}");
            foreach (var beschikbaarheid in rijschool.instructeurBeschikbaarheid)
            {
                //Debug.Log($"Day: {beschikbaarheid.dag}, Slots: {beschikbaarheid.tijdslots?.Count ?? 0}");
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
                    //Debug.Log("Rijschool updated successfully");
                    //Debug.Log($"Server response: {www.downloadHandler.text}");
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
        if(leerlingIndex < 0)
        {
            ingelogdAlsText.text = "";
            return;
        }

        // Only change selectedLeerling if user is an instructor
        if (PlayerPrefs.HasKey("MijnRijschool"))
        {
            if (selectedRijschool != null && leerlingIndex < selectedRijschool.leerlingen.Count)
            {
                selectedLeerling = selectedRijschool.leerlingen[leerlingIndex];
                Debug.Log($"Selected leerling: {selectedLeerling.naam}");
                
                // Update the ingelogd als text
                if (ingelogdAlsText != null)
                {
                    ingelogdAlsText.text = "Ingelogd als: " + selectedLeerling.naam;
                }
            }
        }
        else
        {
            // For non-instructors, just update the UI text if needed
            if (ingelogdAlsText != null && selectedLeerling != null)
            {
                ingelogdAlsText.text = "Ingelogd als: " + selectedLeerling.naam;
            }
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
        Rooster.instance.UpdateOverzichtLeerlingen();

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
        Rooster.instance.UpdateOverzichtLeerlingen();

        // Save changes to server if rijschool exists in database
        if (alleRijscholen.Any(r => r.naam == selectedRijschool.naam))
        {
            await UpdateRijschool(selectedRijschool);
        }
    }

    public void RemoveLeerling(int leerling)
    {
        // Store the student index for later confirmation
        pendingLeerlingRemoval = leerling;
        // You can trigger your confirmation UI here
    }

    public async void ConfirmRemoveLeerling()
    {
        if (pendingLeerlingRemoval < 0)
        {
            print("kan niet"); return;
        }

        if (selectedRijschool != null && selectedRijschool.leerlingen != null && 
            pendingLeerlingRemoval < selectedRijschool.leerlingen.Count)
        {
            string leerlingNaam = selectedRijschool.leerlingen[pendingLeerlingRemoval].naam;
            UnityAnalyticsManager.Instance.TrackStudentRemoved(selectedRijschool.naam, leerlingNaam);

            // Remove all lessons associated with this student
            if (selectedRijschool.rooster?.weken != null)
            {
                foreach (Week week in selectedRijschool.rooster.weken)
                {
                    if (week.lessen != null)
                    {
                        // Remove lessons where this student is the main student
                        week.lessen.RemoveAll(les => les.leerlingNaam == leerlingNaam);

                        // Remove this student from any group lessons they're part of
                        foreach (Les les in week.lessen)
                        {
                            if (les.gereserveerdDoorLeerling != null)
                            {
                                les.gereserveerdDoorLeerling.RemoveAll(l => l.naam == leerlingNaam);
                            }
                        }
                    }
                }
            }

            // Remove the student from the driving school
            selectedRijschool.leerlingen.RemoveAt(pendingLeerlingRemoval);
            selectedLeerling = null;
            Debug.Log($"Leerling {leerlingNaam} en alle bijbehorende lessen verwijderd");

            // Save changes to server
            await UpdateRijschool(selectedRijschool);
            Rooster.instance.UpdateOverzichtLeerlingen();
            Rooster.instance.LoadLessen(); // Refresh the schedule display
        }
        else
        {
            Debug.LogWarning("Geen rijschool of leerling geselecteerd!");
        }

        // Reset the pending removal
        pendingLeerlingRemoval = -1;
    }

    public void CancelRemoveLeerling()
    {
        // Reset the pending removal without taking action
        pendingLeerlingRemoval = -1;
    }

    private void UpdateEditRijschoolFields()
    {
        if (selectedRijschool != null && editRijschoolInputFields != null && editRijschoolInputFields.Count >= 4)
        {
            editRijschoolInputFields[0].text = selectedRijschool.naam;
            editRijschoolInputFields[1].text = selectedRijschool.beschrijving;
            editRijschoolInputFields[2].text = selectedRijschool.woonPlaats;
            editRijschoolInputFields[3].text = selectedRijschool.wachtwoord;
        }
    }

    public async void OnEditRijschoolName(string newName)
    {
        if (selectedRijschool != null && !string.IsNullOrEmpty(newName))
        {
            // Check if a rijschool with this name already exists (case insensitive)
            bool nameExists = alleRijscholen.Any(r => 
                r != selectedRijschool && 
                r.naam.Equals(newName, System.StringComparison.OrdinalIgnoreCase));

            if (nameExists)
            {
                nameExistsWarning.SetActive(true);
                UpdateEditRijschoolFields(); // Reset to original name
                return;
            }

            nameExistsWarning.SetActive(false);
            string oldName = selectedRijschool.naam;
            selectedRijschool.naam = newName;
            
            // Update PlayerPrefs if this is the user's driving school
            if (PlayerPrefs.HasKey("MijnRijschool") && 
                PlayerPrefs.GetString("MijnRijschool").Equals(oldName, System.StringComparison.OrdinalIgnoreCase))
            {
                PlayerPrefs.SetString("MijnRijschool", newName);
                PlayerPrefs.Save();
            }

            using (UnityWebRequest www = UnityWebRequest.Put($"{apiUrl}/{oldName}", JsonUtility.ToJson(selectedRijschool)))
            {
                www.SetRequestHeader("Content-Type", "application/json");
                try
                {
                    await www.SendWebRequest();
                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        await LoadRijscholen();
                    }
                    else
                    {
                        Debug.LogError($"Error updating rijschool: {www.error}");
                        Debug.LogError($"Response: {www.downloadHandler.text}");
                        
                        // Revert changes if update failed
                        selectedRijschool.naam = oldName;
                        if (PlayerPrefs.HasKey("MijnRijschool"))
                        {
                            PlayerPrefs.SetString("MijnRijschool", oldName);
                            PlayerPrefs.Save();
                        }
                        UpdateEditRijschoolFields();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Exception during rijschool update: {e.Message}");
                }
            }
        }
    }

    public async void OnEditRijschoolDescription(string newDescription)
    {
        if (selectedRijschool != null)
        {
            selectedRijschool.beschrijving = newDescription;
            await UpdateRijschool(selectedRijschool);
            await LoadRijscholen();
        }
    }

    public async void OnEditRijschoolPassword(string newPassword)
    {
        if (selectedRijschool != null && !string.IsNullOrEmpty(newPassword))
        {
            selectedRijschool.wachtwoord = newPassword;
            await UpdateRijschool(selectedRijschool);
        }
    }

    public async void OnEditRijschoolWoonplaats(string newWoonplaats)
    {
        if (selectedRijschool != null)
        {
            selectedRijschool.woonPlaats = newWoonplaats;
            await UpdateRijschool(selectedRijschool);
        }
    }

    private void UpdateWoonplaatsSettingsUI()
    {
        if (woonplaatsSettings == null || woonplaatsSettings.Count < 4) return;

        // First element: StartInWoonplaats = 0
        woonplaatsSettings[0].SetActive(PlayerPrefs.GetInt("StartInWoonplaats") == 0);
        
        // Second element: StartInWoonplaats = 1
        woonplaatsSettings[1].SetActive(PlayerPrefs.GetInt("StartInWoonplaats") == 1);
        
        // Third element: EindInWoonplaats = 0
        woonplaatsSettings[2].SetActive(PlayerPrefs.GetInt("EindInWoonplaats") == 0);
        
        // Fourth element: EindInWoonplaats = 1
        woonplaatsSettings[3].SetActive(PlayerPrefs.GetInt("EindInWoonplaats") == 1);
    }

    public void SetWoonplaatsSetting(int setting)
    {
        switch (setting)
        {
            case 0:
                PlayerPrefs.SetInt("StartInWoonplaats", 1);
                break;
            case 1:
                PlayerPrefs.SetInt("StartInWoonplaats", 0);
                break;
            case 2:
                PlayerPrefs.SetInt("EindInWoonplaats", 1);
                break;
            case 3:
                PlayerPrefs.SetInt("EindInWoonplaats", 0);
                break;
        }
        print(PlayerPrefs.GetInt("StartInWoonplaats") + " , " + PlayerPrefs.GetInt("EindInWoonplaats"));
        PlayerPrefs.Save();
        UpdateWoonplaatsSettingsUI();
    }

    public void SetStartWoonplaatsen(string woonplaatsen)
    {
        PlayerPrefs.SetString("StartWoonplaatsen", woonplaatsen);
        PlayerPrefs.Save();
    }

    public void SetEindWoonplaatsen(string woonplaatsen)
    {
        PlayerPrefs.SetString("EindWoonplaatsen", woonplaatsen);
        PlayerPrefs.Save();
    }

    private void UpdateRoosterLeerlingenButtonVisibility()
    {
        if (RoosterLeerlingenButton != null)
        {
            bool hasStudents = selectedRijschool?.leerlingen != null && selectedRijschool.leerlingen.Count > 0;
            RoosterLeerlingenButton.SetActive(hasStudents);
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
    public string woonPlaats;
    public string adres;  // New field
    public string wachtwoord;

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
    public string notities = "";  // Modified to have default value
    public string datum;
    public int weekNummer;
    public string leerlingId;
    public string leerlingNaam;
    public bool isAutomatischGepland;
    public List<Leerling> gereserveerdDoorLeerling;

    public Les()
    {
        System.DateTime now = System.DateTime.Now;
        datum = now.ToString("dd-MM-yyyy");
        weekNummer = System.Globalization.ISOWeek.GetWeekOfYear(now);
        gereserveerdDoorLeerling = new List<Leerling>();
        notities = "";  // Initialize empty notes
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
    public string woonPlaats;
    public bool LLzienLessen;  // New field
    public List<Leerling> leerlingen;
    public LesRooster rooster;
    public List<Beschikbaarheid> instructeurBeschikbaarheid;

    public Rijschool()
    {
        leerlingen = new List<Leerling>();
        rooster = new LesRooster();
        instructeurBeschikbaarheid = new List<Beschikbaarheid>();
        LLzienLessen = false;  // Default value
    }
}

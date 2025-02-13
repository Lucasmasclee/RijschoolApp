using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Rooster : MonoBehaviour
{
    public static Rooster instance;
    private bool roosterInstructor = false; // Of het rooster voor de instructeur of leerlingen is


    [SerializeField] private GameObject wekenScrollView;
    [SerializeField] private List<Transform> dagenScrollview;
    private List<Image> wekenScrollViewButtons;

    [SerializeField] private TextMeshProUGUI rijschoolnaam;


    [SerializeField] private List<GameObject> onlyForInstructors;

    [SerializeField] private GameObject lesPoolParent;
    private List<GameObject> lesPool; // 60 les GameObjects
    [SerializeField] private List<GameObject> LeraarLesLeerlingLes; // 1 les for instructor + 1 les for student

    private Les selectedLes;
    [SerializeField] private TMP_InputField lesBeginTijd;
    [SerializeField] private TMP_InputField lesEindTijd;
    [SerializeField] private TMP_InputField lesNotities;

    private int selectedWeek = 0;
    private int selectedDay = 0;

    [SerializeField] private List<GameObject> weekButtons;
    private int currentWeek;

    [SerializeField] private LesManager lesManager;

    [SerializeField] private GameObject reserveerButton;
    [SerializeField] private GameObject verwijderReserving;
    [SerializeField] private GameObject instructeurSelecteertLes;
    [SerializeField] private GameObject leerlingSelecteertLes;
    [SerializeField] private List<GameObject> leerlingenToewijzen;
    [SerializeField] private List<GameObject> kiesleerlingButtons;
    [SerializeField] private List<GameObject> leerlingoverzicht;

    //[SerializeField] private GameObject verwijderLesButton;
    //[SerializeField] private GameObject leerlingenGereserveerd;
    //[SerializeField] private GameObject lesToewijzen;

    private void Start()
    {
        instance = this;
        // Get current week number
        currentWeek = System.Globalization.ISOWeek.GetWeekOfYear(System.DateTime.Now);
        
        // Initialize week button texts
        for (int i = 0; i < weekButtons.Count; i++)
        {
            int weekNumber = (currentWeek + i) % 52;
            if (weekNumber == 0) weekNumber = 52;

            TextMeshProUGUI weekText = weekButtons[i].GetComponentsInChildren<TextMeshProUGUI>()[0];
            weekText.text = $"Week {weekNumber}";
        }

        wekenScrollViewButtons = Enumerable.Range(0, wekenScrollView.transform.childCount)
            .Select(i => wekenScrollView.transform.GetChild(i).gameObject.GetComponent<Image>())
            .ToList();

        lesPool = Enumerable.Range(0, lesPoolParent.transform.childCount)
            .Select(i => lesPoolParent.transform.GetChild(i).gameObject)
            .ToList();

        //SelectWeek(0);
    }

    public void LoadKiesLeerlingButtons()
    {
        foreach (GameObject obj in kiesleerlingButtons)
        {
            obj.SetActive(false);
        }
        // Set Leerlingen Toewijzen GameObjects
        List<Leerling> listleerling = RijschoolApp.instance.selectedRijschool.leerlingen;
        List<Color> colors = RijschoolApp.instance.leerlingKleuren;
        for (int i = 0; i < listleerling.Count; i++)
        {
            Image image = kiesleerlingButtons[i].GetComponent<Image>();
            image.color = colors[listleerling[i].colorIndex];
            TextMeshProUGUI naamtext = kiesleerlingButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            naamtext.text = listleerling[i].naam;
            kiesleerlingButtons[i].SetActive(true);
        }
    }

    public void RoosterForInstructors(bool instructors)
    {
        roosterInstructor = instructors;
        foreach(GameObject obj in onlyForInstructors)
        {
            obj.SetActive(roosterInstructor);
        }
    }

    public void LoadLessen()
    {
        // Deactivate all lesson objects first
        foreach (GameObject obj in lesPool)
        {
            var lesButton = obj.GetComponent<Button>();
            lesButton.onClick.RemoveAllListeners();
            obj.SetActive(false);
        }
        // Only proceed if we have a selected rijschool with a roster
        if (RijschoolApp.instance.selectedRijschool?.rooster == null)
        {
            
            Debug.LogWarning("No rijschool selected or roster is null");
            return;
        }
        rijschoolnaam.text = RijschoolApp.instance.selectedRijschool.naam;
        // Calculate actual week number
        int actualWeekNumber = (currentWeek + selectedWeek) % 52;
        if (actualWeekNumber == 0) actualWeekNumber = 52;

        // Get lessons for the selected week
        var weekLessen = RijschoolApp.instance.selectedRijschool.rooster
            .GetLessenForWeek(actualWeekNumber);

        int poolIndex = 0;
        
        // Group lessons by day
        var lessenPerDag = weekLessen
            .GroupBy(les => System.DateTime.ParseExact(les.datum, "dd-MM-yyyy", null).DayOfWeek)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Display lessons for each day
        for (int dagIndex = 0; dagIndex < 7; dagIndex++)
        {
            var dayOfWeek = (System.DayOfWeek)dagIndex;
            if (lessenPerDag.ContainsKey(dayOfWeek))
            {
                foreach (Les les in lessenPerDag[dayOfWeek])
                {
                    if (poolIndex >= lesPool.Count) break;
                    
                    GameObject UIelement = lesPool[poolIndex];
                    poolIndex++;

                    UIelement.transform.SetParent(dagenScrollview[dagIndex]);
                    UIelement.SetActive(true);

                    // Set color based on assigned student
                    Image lesImage = UIelement.GetComponent<Image>();
                    lesImage.color = lesManager.GetLeerlingColor(les.leerlingNaam);

                    TextMeshProUGUI[] texts = UIelement.GetComponentsInChildren<TextMeshProUGUI>();
                    texts[0].text = les.begintijd + " - " + les.eindtijd;
                    texts[1].text = les.notities;
                    
                    // Add click handler
                    Button lesButton = UIelement.GetComponent<Button>();
                    Les currentLes = les; // Capture for lambda
                    lesButton.onClick.AddListener(() => {
                        OnLesSelected(currentLes);
                        // Update leerlingenToewijzen visibility after selecting a lesson
                        UpdateLeerlingenToewijzenVisibility();
                    });
                }
            }
        }
    }

    private void OnLesSelected(Les les)
    {
        selectedLes = les;

        // Reset visibility of UI elements
        reserveerButton.SetActive(false);
        verwijderReserving.SetActive(false);
        instructeurSelecteertLes.SetActive(false);
        leerlingSelecteertLes.SetActive(false);

        // Get the instructor and student detail GameObjects
        GameObject instructorLes = LeraarLesLeerlingLes[0];
        GameObject studentLes = LeraarLesLeerlingLes[1];
        
        // Update the lesson details for both views
        if (selectedLes != null)
        {
            // Update instructor and student lesson details
            foreach (GameObject detailLes in new[] { instructorLes, studentLes })
            {
                // Set the lesson details
                TextMeshProUGUI[] texts = detailLes.GetComponentsInChildren<TextMeshProUGUI>();
                texts[0].text = selectedLes.begintijd + " - " + selectedLes.eindtijd;
                texts[1].text = selectedLes.notities;

                // Set the color based on assigned student
                Image lesImage = detailLes.GetComponent<Image>();
                lesImage.color = lesManager.GetLeerlingColor(selectedLes.leerlingNaam);
            }

            // Show the appropriate view based on user type
            if (roosterInstructor)
            {
                instructeurSelecteertLes.SetActive(true);
                instructorLes.SetActive(true);
                studentLes.SetActive(false);
            }
            else
            {
                // Student view
                leerlingSelecteertLes.SetActive(true);
                instructorLes.SetActive(false);
                studentLes.SetActive(true);

                if (RijschoolApp.instance.selectedLeerling != null)
                {
                    bool isReserved = selectedLes.gereserveerdDoorLeerling != null && 
                        selectedLes.gereserveerdDoorLeerling.Any(l => 
                            l.naam == RijschoolApp.instance.selectedLeerling.naam);

                    reserveerButton.SetActive(!isReserved);
                    verwijderReserving.SetActive(isReserved);
                }
            }
        }
    }

    public async void SaveLes(int dagIndex)
    {
        if (RijschoolApp.instance.selectedRijschool != null)
        {
            // Calculate actual week number
            int actualWeekNumber = (currentWeek + selectedWeek) % 52;
            if (actualWeekNumber == 0) actualWeekNumber = 52;

            // Calculate the date for this lesson
            System.DateTime now = System.DateTime.Now;
            // Get the start of the current week (Monday)
            System.DateTime startOfWeek = now.AddDays(-(int)now.DayOfWeek + 1);
            // Move to the selected week
            System.DateTime firstDayOfSelectedWeek = startOfWeek.AddDays(selectedWeek * 7);
            // Add the selected day offset (0 = Monday, 6 = Sunday)
            System.DateTime lesDate = firstDayOfSelectedWeek.AddDays(selectedDay-1);

            Les nieuweLes = new Les
            {
                begintijd = lesBeginTijd.text,
                eindtijd = lesEindTijd.text,
                notities = lesNotities.text,
                datum = lesDate.ToString("dd-MM-yyyy"),
                weekNummer = actualWeekNumber
            };

            if (RijschoolApp.instance.selectedRijschool.rooster == null)
            {
                RijschoolApp.instance.selectedRijschool.rooster = new LesRooster();
            }

            // Find or create the week
            Week targetWeek = RijschoolApp.instance.selectedRijschool.rooster.weken
                .FirstOrDefault(w => w.weekNummer == actualWeekNumber);
                
            if (targetWeek == null)
            {
                targetWeek = new Week { weekNummer = actualWeekNumber, jaar = lesDate.Year };
                RijschoolApp.instance.selectedRijschool.rooster.weken.Add(targetWeek);
            }

            targetWeek.lessen.Add(nieuweLes);
            
            Debug.Log($"Les toegevoegd op {nieuweLes.datum} (Week {nieuweLes.weekNummer}) van {lesBeginTijd.text} tot {lesEindTijd.text}");

            // Save to server
            await RijschoolApp.instance.UpdateRijschool(RijschoolApp.instance.selectedRijschool);

            // Clear input fields
            lesBeginTijd.text = "";
            lesEindTijd.text = "";
            lesNotities.text = "";
            
            // Refresh the display
            LoadLessen();
        }
        else
        {
            Debug.LogWarning("Geen rijschool geselecteerd!");
        }
    }

    public async void VerwijderLes(int dagIndex)
    {
        if (RijschoolApp.instance.selectedRijschool != null && selectedLes != null)
        {
            // Calculate actual week number
            int actualWeekNumber = (currentWeek + selectedWeek) % 52;
            if (actualWeekNumber == 0) actualWeekNumber = 52;

            Week targetWeek = RijschoolApp.instance.selectedRijschool.rooster.weken
                .FirstOrDefault(w => w.weekNummer == actualWeekNumber);

            if (targetWeek != null && targetWeek.lessen.Remove(selectedLes))
            {
                Debug.Log("Les verwijderd");
                selectedLes = null;
                
                // Save to server
                await RijschoolApp.instance.UpdateRijschool(RijschoolApp.instance.selectedRijschool);
                
                // Refresh the display
                LoadLessen();
            }
            else
            {
                Debug.LogWarning("Les niet gevonden!");
            }
        }
        else
        {
            Debug.LogWarning("Geen rijschool of les geselecteerd!");
        }
    }

    public void SelectWeek(int weekIndex)
    {
        selectedWeek = weekIndex;
        print(selectedWeek + weekButtons[weekIndex].gameObject.name);
        
        // Reset all button colors
        foreach (GameObject obj in weekButtons)
        {
            Image image = obj.GetComponent<Image>();
            image.color = Color.white;
        }
        Image i = weekButtons[selectedWeek].gameObject.GetComponent<Image>();
        i.color = Color.cyan;
        
        LoadLessen();
    }

    public void SelectDay(int day)
    {
        selectedDay = day;
    }

    public async void AddLeerlingToLes(int leerling)
    {
        if(leerling < 0) // Als de leerling zelf een les reserveert
        {
            if (selectedLes != null && RijschoolApp.instance.selectedLeerling != null)
            {
                if (selectedLes.gereserveerdDoorLeerling == null)
                {
                    selectedLes.gereserveerdDoorLeerling = new List<Leerling>();
                }

                // Check if student isn't already in the list
                if (!selectedLes.gereserveerdDoorLeerling.Any(l =>
                    l.naam == RijschoolApp.instance.selectedLeerling.naam))
                {
                    // Create a new Leerling object with the same properties
                    Leerling leerlingCopy = new Leerling
                    {
                        naam = RijschoolApp.instance.selectedLeerling.naam,
                        frequentie = RijschoolApp.instance.selectedLeerling.frequentie,
                        colorIndex = RijschoolApp.instance.selectedLeerling.colorIndex
                    };
                    
                    selectedLes.gereserveerdDoorLeerling.Add(leerlingCopy);

                    // Update UI
                    reserveerButton.SetActive(false);
                    verwijderReserving.SetActive(true);

                    // Save changes to server
                    await RijschoolApp.instance.UpdateRijschool(RijschoolApp.instance.selectedRijschool);
                    LoadLessen();
                }
            }
        }
        else // Als de instructeur een les reserveert voor een leerling
        {
            if (selectedLes != null && RijschoolApp.instance.selectedRijschool != null)
            {
                // Get the selected student from the rijschool's student list
                if (leerling < RijschoolApp.instance.selectedRijschool.leerlingen.Count)
                {
                    // Initialize the list if it's null
                    if (selectedLes.gereserveerdDoorLeerling == null)
                    {
                        selectedLes.gereserveerdDoorLeerling = new List<Leerling>();
                    }

                    // Check if student isn't already in the list
                    if (!selectedLes.gereserveerdDoorLeerling.Any(l => 
                        l.naam == RijschoolApp.instance.selectedRijschool.leerlingen[leerling].naam))
                    {
                        // Create a new Leerling object with the same properties
                        Leerling leerlingCopy = new Leerling
                        {
                            naam = RijschoolApp.instance.selectedRijschool.leerlingen[leerling].naam,
                            frequentie = RijschoolApp.instance.selectedRijschool.leerlingen[leerling].frequentie,
                            colorIndex = RijschoolApp.instance.selectedRijschool.leerlingen[leerling].colorIndex
                        };

                        // Add the new student without clearing existing ones
                        selectedLes.gereserveerdDoorLeerling.Add(leerlingCopy);

                        // Save changes to server
                        await RijschoolApp.instance.UpdateRijschool(RijschoolApp.instance.selectedRijschool);
                        LoadLessen();
                    }
                }
            }
        }
    }

    public async void RemoveLeerlingFromLes(int leerling)
    {
        if (selectedLes?.gereserveerdDoorLeerling == null) return;

        if (leerling < 0) // Leerling removing their own reservation
        {
            if (RijschoolApp.instance.selectedLeerling != null)
            {
                selectedLes.gereserveerdDoorLeerling.RemoveAll(l => 
                    l.naam == RijschoolApp.instance.selectedLeerling.naam);
                
                // Update UI
                reserveerButton.SetActive(true);
                verwijderReserving.SetActive(false);
            }
        }
        else // Instructor removing a student's reservation
        {
            if (leerling < RijschoolApp.instance.selectedRijschool.leerlingen.Count)
            {
                string leerlingNaam = RijschoolApp.instance.selectedRijschool.leerlingen[leerling].naam;
                selectedLes.gereserveerdDoorLeerling.RemoveAll(l => l.naam == leerlingNaam);
            }
        }

        // Save changes to server
        await RijschoolApp.instance.UpdateRijschool(RijschoolApp.instance.selectedRijschool);
        LoadLessen();
    }

    // Add this new method to handle the visibility update
    public void UpdateLeerlingenToewijzenVisibility()
    {
        if (selectedLes == null) return;

        foreach(GameObject obj in leerlingenToewijzen)
        {
            obj.SetActive(false);
        }
        List<Color> colors = RijschoolApp.instance.leerlingKleuren;

        List<Leerling> listleerling = RijschoolApp.instance.selectedRijschool.leerlingen;
        for (int i = 0; i < listleerling.Count; i++)
        {
            if (i >= leerlingenToewijzen.Count) break;

            GameObject leerlingObj = leerlingenToewijzen[i];
            leerlingObj.SetActive(true);

            TextMeshProUGUI naamtext = leerlingObj.GetComponentInChildren<TextMeshProUGUI>();
            naamtext.text = listleerling[i].naam;

            //Image image
            Image image = leerlingenToewijzen[i].GetComponent<Image>();
            image.color = colors[listleerling[i].colorIndex];

            // Get child GameObjects
            Transform leerlingTransform = leerlingObj.transform;
            GameObject assignButton = leerlingTransform.GetChild(1).gameObject;
            GameObject removeButton = leerlingTransform.GetChild(2).gameObject;

            // Check if this student is in the selectedLes reservations
            bool isReserved = selectedLes.gereserveerdDoorLeerling?.Any(l => l.naam == listleerling[i].naam) ?? false;

            // Set visibility based on reservation status
            assignButton.SetActive(!isReserved);
            removeButton.SetActive(isReserved);
        }
    }

    public void UpdateOverzichtLeerlingen()
    {
        if (RijschoolApp.instance.selectedRijschool == null) return;

        foreach (GameObject obj in leerlingoverzicht)
        {
            obj.SetActive(false);
        }

        List<Color> colors = RijschoolApp.instance.leerlingKleuren;
        List<Leerling> listleerling = RijschoolApp.instance.selectedRijschool.leerlingen;
        for (int i = 0; i < listleerling.Count; i++)
        {
            if (i >= leerlingoverzicht.Count) break;

            GameObject leerlingObj = leerlingoverzicht[i];
            leerlingObj.SetActive(true);

            TextMeshProUGUI naamtext = leerlingObj.GetComponentInChildren<TextMeshProUGUI>();
            naamtext.text = listleerling[i].naam;

            TextMeshProUGUI frequentietext = leerlingObj.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            frequentietext.text = listleerling[i].frequentie.ToString();

            //Image image
            Image image = leerlingoverzicht[i].GetComponent<Image>();
            image.color = colors[listleerling[i].colorIndex];

            // Get child GameObjects
            Transform leerlingTransform = leerlingObj.transform;
            GameObject plus = leerlingTransform.GetChild(1).gameObject;
            GameObject min = leerlingTransform.GetChild(2).gameObject;
        }
    }

    public async void GenerateWeeklySchedule()
    {
        // Calculate actual week number
        int actualWeekNumber = (currentWeek + selectedWeek) % 52;
        if (actualWeekNumber == 0) actualWeekNumber = 52;

        // Get all lessons for the selected week
        var weekLessen = RijschoolApp.instance.selectedRijschool.rooster
            .GetLessenForWeek(actualWeekNumber);

        // Get all students and their preferences
        var leerlingen = RijschoolApp.instance.selectedRijschool.leerlingen;
        
        // Create a dictionary to track how many lessons each student has been assigned
        Dictionary<string, int> assignedLessonsCount = new Dictionary<string, int>();
        foreach (var leerling in leerlingen)
        {
            assignedLessonsCount[leerling.naam] = 0;
        }

        // Create a list of available lessons (lessons with reservations)
        var availableLessons = weekLessen
            .Where(les => les.gereserveerdDoorLeerling != null && les.gereserveerdDoorLeerling.Any())
            .ToList();

        // Sort lessons by number of students who reserved it (ascending)
        availableLessons.Sort((a, b) => 
            a.gereserveerdDoorLeerling.Count.CompareTo(b.gereserveerdDoorLeerling.Count));

        // First pass: Try to give each student their preferred number of lessons
        foreach (var les in availableLessons)
        {
            // Get students who reserved this lesson
            var reservedStudents = les.gereserveerdDoorLeerling
                .Where(l => assignedLessonsCount[l.naam] < l.frequentie)
                .OrderBy(l => assignedLessonsCount[l.naam]) // Prioritize students with fewer assigned lessons
                .ToList();

            if (reservedStudents.Any())
            {
                // Select the student with the highest priority
                var selectedStudent = reservedStudents.First();
                les.leerlingNaam = selectedStudent.naam;
                assignedLessonsCount[selectedStudent.naam]++;
            }
        }

        // Second pass: Fill remaining lessons if any students still need lessons
        foreach (var les in availableLessons.Where(l => string.IsNullOrEmpty(l.leerlingNaam)))
        {
            var availableStudents = les.gereserveerdDoorLeerling
                .Where(l => assignedLessonsCount[l.naam] < l.frequentie)
                .OrderBy(l => assignedLessonsCount[l.naam])
                .ToList();

            if (availableStudents.Any())
            {
                var selectedStudent = availableStudents.First();
                les.leerlingNaam = selectedStudent.naam;
                assignedLessonsCount[selectedStudent.naam]++;
            }
        }

        // Save changes to server
        await RijschoolApp.instance.UpdateRijschool(RijschoolApp.instance.selectedRijschool);
        
        // Refresh the display
        LoadLessen();

        // Debug output to check results
        foreach (var kvp in assignedLessonsCount)
        {
            Debug.Log($"Student {kvp.Key}: {kvp.Value} lessons assigned");
        }
    }

    public async void GenerateMinimalChangesSchedule()
    {
        // Calculate actual week number
        int actualWeekNumber = (currentWeek + selectedWeek) % 52;
        if (actualWeekNumber == 0) actualWeekNumber = 52;

        // Get all lessons for the selected week
        var weekLessen = RijschoolApp.instance.selectedRijschool.rooster
            .GetLessenForWeek(actualWeekNumber);

        // Check if there are any existing assignments
        bool hasExistingAssignments = weekLessen.Any(l => !string.IsNullOrEmpty(l.leerlingNaam));

        // If no existing assignments, fall back to regular schedule generation
        if (!hasExistingAssignments)
        {
            GenerateWeeklySchedule();
            return;
        }

        // Rest of the existing minimal changes logic...
        Dictionary<string, string> originalAssignments = weekLessen
            .Where(l => !string.IsNullOrEmpty(l.leerlingNaam))
            .ToDictionary(l => $"{l.datum}_{l.begintijd}", l => l.leerlingNaam);

        // Get all students and their preferences
        var leerlingen = RijschoolApp.instance.selectedRijschool.leerlingen;
        
        Dictionary<string, int> assignedLessonsCount = new Dictionary<string, int>();
        foreach (var leerling in leerlingen)
        {
            assignedLessonsCount[leerling.naam] = 0;
        }

        var availableLessons = weekLessen
            .Where(les => les.gereserveerdDoorLeerling != null && les.gereserveerdDoorLeerling.Any())
            .ToList();

        // First pass: Keep existing valid assignments
        foreach (var les in availableLessons)
        {
            string lesKey = $"{les.datum}_{les.begintijd}";
            
            // If the current student is still available for this lesson, keep the assignment
            if (!string.IsNullOrEmpty(les.leerlingNaam) && 
                les.gereserveerdDoorLeerling.Any(l => l.naam == les.leerlingNaam))
            {
                assignedLessonsCount[les.leerlingNaam]++;
                continue;
            }

            // Clear the assignment if the student is no longer available
            les.leerlingNaam = null;
        }

        // Second pass: Fill empty slots with minimal changes
        foreach (var les in availableLessons.Where(l => string.IsNullOrEmpty(l.leerlingNaam)))
        {
            var availableStudents = les.gereserveerdDoorLeerling
                .Where(l => assignedLessonsCount[l.naam] < l.frequentie)
                .OrderBy(l => assignedLessonsCount[l.naam])
                .ToList();

            if (availableStudents.Any())
            {
                var selectedStudent = availableStudents.First();
                les.leerlingNaam = selectedStudent.naam;
                assignedLessonsCount[selectedStudent.naam]++;
            }
        }

        // Save changes and refresh
        await RijschoolApp.instance.UpdateRijschool(RijschoolApp.instance.selectedRijschool);
        LoadLessen();
    }

    public async void GenerateMaximalChangesSchedule()
    {
        // Calculate actual week number
        int actualWeekNumber = (currentWeek + selectedWeek) % 52;
        if (actualWeekNumber == 0) actualWeekNumber = 52;

        // Get all lessons for the selected week
        var weekLessen = RijschoolApp.instance.selectedRijschool.rooster
            .GetLessenForWeek(actualWeekNumber);

        // Check if there are any existing assignments
        bool hasExistingAssignments = weekLessen.Any(l => !string.IsNullOrEmpty(l.leerlingNaam));

        // If no existing assignments, fall back to regular schedule generation
        if (!hasExistingAssignments)
        {
            GenerateWeeklySchedule();
            return;
        }

        // Rest of the existing maximal changes logic...
        Dictionary<string, string> originalAssignments = weekLessen
            .Where(l => !string.IsNullOrEmpty(l.leerlingNaam))
            .ToDictionary(l => $"{l.datum}_{l.begintijd}", l => l.leerlingNaam);

        // Get all students and their preferences
        var leerlingen = RijschoolApp.instance.selectedRijschool.leerlingen;
        
        Dictionary<string, int> assignedLessonsCount = new Dictionary<string, int>();
        foreach (var leerling in leerlingen)
        {
            assignedLessonsCount[leerling.naam] = 0;
        }

        var availableLessons = weekLessen
            .Where(les => les.gereserveerdDoorLeerling != null && les.gereserveerdDoorLeerling.Any())
            .ToList();

        // Clear all existing assignments
        foreach (var les in availableLessons)
        {
            les.leerlingNaam = null;
        }

        // Assign lessons trying to maximize changes
        foreach (var les in availableLessons)
        {
            string lesKey = $"{les.datum}_{les.begintijd}";
            
            var availableStudents = les.gereserveerdDoorLeerling
                .Where(l => assignedLessonsCount[l.naam] < l.frequentie)
                .OrderBy(l => assignedLessonsCount[l.naam])
                .ToList();

            if (availableStudents.Any())
            {
                // If there was an original assignment, try to pick a different student
                if (originalAssignments.ContainsKey(lesKey))
                {
                    string originalStudent = originalAssignments[lesKey];
                    var differentStudent = availableStudents
                        .FirstOrDefault(s => s.naam != originalStudent) ?? availableStudents.First();
                    
                    les.leerlingNaam = differentStudent.naam;
                    assignedLessonsCount[differentStudent.naam]++;
                }
                else
                {
                    // If there was no original assignment, just pick the first available student
                    var selectedStudent = availableStudents.First();
                    les.leerlingNaam = selectedStudent.naam;
                    assignedLessonsCount[selectedStudent.naam]++;
                }
            }
        }

        // Save changes and refresh
        await RijschoolApp.instance.UpdateRijschool(RijschoolApp.instance.selectedRijschool);
        LoadLessen();
    }
}

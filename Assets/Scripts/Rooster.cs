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

    [SerializeField] private Button previousWeekButton;
    [SerializeField] private Button nextWeekButton;
    [SerializeField] private TextMeshProUGUI weekDateText;
    
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

    [SerializeField] private GameObject timeFormatWarning; // Reference to warning UI element

    [SerializeField] private TMP_InputField startTijdInput;
    [SerializeField] private TMP_InputField eindTijdInput;
    [SerializeField] private GameObject invalidTimeFormatWarning;
    [SerializeField] private GameObject createLes;

    // Add these constants at the top of the Rooster class
    private const float HOUR_HEIGHT = 100f; // Height per hour in the schedule
    private const float START_HOUR = 6f; // Schedule starts at 6:00
    private const float LESSON_WIDTH = 130f;
    private const float LESSON_X_POSITION = 72f;

    private void Start()
    {
        instance = this;
        
        // Validate required references
        if (lesPoolParent == null)
        {
            Debug.LogError("lesPoolParent is not assigned in the inspector!");
            return;
        }

        if (dagenScrollview == null || dagenScrollview.Count == 0)
        {
            Debug.LogError("dagenScrollview list is not set up correctly!");
            return;
        }

        if (weekDateText == null)
        {
            Debug.LogError("weekDateText is not assigned in the inspector!");
            return;
        }

        if (rijschoolnaam == null)
        {
            Debug.LogError("rijschoolnaam is not assigned in the inspector!");
            return;
        }
        
        // Initialize lesPool from lesPoolParent
        lesPool = new List<GameObject>();
        foreach (Transform child in lesPoolParent.transform)
        {
            lesPool.Add(child.gameObject);
        }

        // Get current week number
        currentWeek = System.Globalization.ISOWeek.GetWeekOfYear(System.DateTime.Now);
        
        // Initialize the week display
        UpdateWeekDisplay();
        //LoadLessen();
    }

    private void UpdateWeekDisplay()
    {
        // Get the date of Monday (first day) of the selected week
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + 1); // Get Monday of current week
        monday = monday.AddDays(7 * selectedWeek); // Adjust for selected week offset

        // Get Sunday (last day) of the selected week
        System.DateTime sunday = monday.AddDays(6);

        // Format the date range text
        string monthFormat = monday.Month == sunday.Month ? "" : " MMM";
        string startDate = monday.ToString("dd" + monthFormat);
        string endDate = sunday.ToString("dd MMM");

        weekDateText.text = $"{startDate} - {endDate}";
    }

    public void GoToPreviousWeek()
    {
        selectedWeek--;
        UpdateWeekDisplay();
        LoadLessen(); // This already handles both normal lessons and availability timeslots
    }

    public void GoToNextWeek()
    {
        selectedWeek++;
        UpdateWeekDisplay();
        LoadLessen(); // This already handles both normal lessons and availability timeslots
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
        Debug.Log($"Setting roosterInstructor to: {instructors}");
        foreach(GameObject obj in onlyForInstructors)
        {
            obj.SetActive(roosterInstructor);
        }
    }

    public void LoadLessen()
    {
        // First, deactivate all lesson objects
        print(lesPool.Count);
        foreach (GameObject obj in lesPool)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        // Check if we should display availability instead of lessons
        if (roosterInstructor)
        {
            DisplayAvailabilityTimeSlots();
            return;
        }
        else if (RijschoolApp.instance.selectedLeerling != null)
        {
            DisplayStudentAvailabilityTimeSlots();
            return;
        }

        // Validate lesPool is initialized
        if (lesPool == null || lesPool.Count == 0)
        {
            Debug.LogError("lesPool is not initialized or empty!");
            return;
        }

        // Only proceed if we have a selected rijschool with a roster
        if (RijschoolApp.instance?.selectedRijschool?.rooster == null)
        {
            Debug.LogWarning("No rijschool selected or roster is null");
            return;
        }

        rijschoolnaam.text = RijschoolApp.instance.selectedRijschool.naam;

        // Calculate actual week number
        int actualWeekNumber = (currentWeek + selectedWeek) % 52;
        if (actualWeekNumber <= 0) actualWeekNumber += 52;

        // Get lessons for the selected week
        var weekLessen = RijschoolApp.instance.selectedRijschool.rooster
            .GetLessenForWeek(actualWeekNumber);

        int poolIndex = 0;
        
        // Group lessons by day
        var lessenPerDag = weekLessen
            .GroupBy(les => System.DateTime.ParseExact(les.datum, "dd-MM-yyyy", null).DayOfWeek)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Display lessons for each day
        for (int dagIndex = 0; dagIndex < dagenScrollview.Count; dagIndex++)
        {
            if (dagenScrollview[dagIndex] == null)
            {
                Debug.LogError($"dagenScrollview[{dagIndex}] is null!");
                continue;
            }

            var dayOfWeek = (System.DayOfWeek)dagIndex;
            if (lessenPerDag.ContainsKey(dayOfWeek))
            {
                foreach (Les les in lessenPerDag[dayOfWeek])
                {
                    if (poolIndex >= lesPool.Count)
                    {
                        Debug.LogWarning("Not enough lesson objects in pool!");
                        break;
                    }

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
        if (!ValidateTimeFormat(lesBeginTijd.text) || !ValidateTimeFormat(lesEindTijd.text))
        {
            timeFormatWarning.SetActive(true);
            return;
        }

        timeFormatWarning.SetActive(false);
        
        // Format times consistently
        string formattedBeginTijd = FormatTime(lesBeginTijd.text);
        string formattedEindTijd = FormatTime(lesEindTijd.text);

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
                begintijd = formattedBeginTijd,
                eindtijd = formattedEindTijd,
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

    public async void GenerateScheduleFromAvailability()
    {
        if (RijschoolApp.instance.selectedRijschool == null) return;

        var rijschool = RijschoolApp.instance.selectedRijschool;
        var leerlingen = rijschool.leerlingen;
        
        // Clear existing automatically planned lessons
        var weekLessen = rijschool.rooster.weken
            .SelectMany(w => w.lessen)
            .Where(l => l.isAutomatischGepland)
            .ToList();
        
        foreach (var les in weekLessen)
        {
            les.leerlingNaam = null;
            les.leerlingId = null;
        }

        // For each day of the week
        string[] dagen = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        foreach (string dag in dagen)
        {
            // Get instructor availability for this day
            var instructeurSlots = rijschool.instructeurBeschikbaarheid
                .FirstOrDefault(b => b.dag == dag)?.tijdslots ?? new List<TimeSlot>();

            foreach (var instructeurSlot in instructeurSlots)
            {
                // Convert times to minutes for easier calculation
                int startMinutes = ConvertTimeToMinutes(instructeurSlot.startTijd);
                int endMinutes = ConvertTimeToMinutes(instructeurSlot.eindTijd);

                // For each student
                foreach (var leerling in leerlingen)
                {
                    // Get student availability for this day
                    var leerlingSlots = leerling.beschikbaarheid
                        .FirstOrDefault(b => b.dag == dag)?.tijdslots ?? new List<TimeSlot>();

                    foreach (var leerlingSlot in leerlingSlots)
                    {
                        int leerlingStart = ConvertTimeToMinutes(leerlingSlot.startTijd);
                        int leerlingEnd = ConvertTimeToMinutes(leerlingSlot.eindTijd);

                        // Find overlapping time
                        int overlapStart = Mathf.Max(startMinutes, leerlingStart);
                        int overlapEnd = Mathf.Min(endMinutes, leerlingEnd);

                        // If there's enough time for a lesson
                        if (overlapEnd - overlapStart >= leerling.minutesPerLes)
                        {
                            // Create a new lesson
                            Les nieuweLes = new Les
                            {
                                begintijd = ConvertMinutesToTime(overlapStart),
                                eindtijd = ConvertMinutesToTime(overlapStart + leerling.minutesPerLes),
                                leerlingNaam = leerling.naam,
                                leerlingId = leerling.naam,
                                isAutomatischGepland = true
                                // Set other properties as needed
                            };

                            // Add lesson to the roster
                            AddLessonToRoster(nieuweLes);
                        }
                    }
                }
            }
        }

        // Save changes to server
        await RijschoolApp.instance.UpdateRijschool(rijschool);
        LoadLessen();
    }

    private int ConvertTimeToMinutes(string time)
    {
        time = time.Replace('.', ':');
        var parts = time.Split(':');
        return int.Parse(parts[0]) * 60 + int.Parse(parts[1]);
    }

    private string ConvertMinutesToTime(int minutes)
    {
        int hours = minutes / 60;
        int mins = minutes % 60;
        return $"{hours:D2}:{mins:D2}";
    }

    private bool ValidateTimeFormat(string time)
    {
        if (string.IsNullOrEmpty(time)) return false;

        // Replace . with : for consistency
        time = time.Replace('.', ':');

        // Split the time string
        string[] parts = time.Split(':');
        if (parts.Length != 2) return false;

        // Parse hours and minutes
        if (!int.TryParse(parts[0], out int hours) || !int.TryParse(parts[1], out int minutes))
            return false;

        // Validate ranges
        if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59)
            return false;

        return true;
    }

    private string FormatTime(string time)
    {
        // Replace . with : and ensure proper formatting
        time = time.Replace('.', ':');
        string[] parts = time.Split(':');
        int hours = int.Parse(parts[0]);
        int minutes = int.Parse(parts[1]);
        return $"{hours:D2}:{minutes:D2}"; // Ensures 2-digit format
    }

    public void OnTimeInputEndEdit(TMP_InputField timeInput)
    {
        if (!ValidateTimeFormat(timeInput.text))
        {
            invalidTimeFormatWarning.SetActive(true);
            timeInput.text = ""; // Clear invalid input
            return;
        }
        
        invalidTimeFormatWarning.SetActive(false);
        timeInput.text = FormatTime(timeInput.text); // Format to consistent HH:mm
    }

    private void AddLessonToRoster(Les nieuweLes)
    {
        if (RijschoolApp.instance.selectedRijschool?.rooster == null) return;

        var rijschool = RijschoolApp.instance.selectedRijschool;
        
        // Parse the date to get week number
        System.DateTime lesDate = System.DateTime.ParseExact(nieuweLes.datum, "dd-MM-yyyy", null);
        int weekNum = System.Globalization.ISOWeek.GetWeekOfYear(lesDate);
        int year = lesDate.Year;

        // Find or create the week in the roster
        Week targetWeek = rijschool.rooster.weken.FirstOrDefault(w => 
            w.weekNummer == weekNum && w.jaar == year);

        if (targetWeek == null)
        {
            targetWeek = new Week { weekNummer = weekNum, jaar = year };
            rijschool.rooster.weken.Add(targetWeek);
        }

        // Add the lesson to the week
        if (targetWeek.lessen == null)
        {
            targetWeek.lessen = new List<Les>();
        }
        
        targetWeek.lessen.Add(nieuweLes);
    }

    public async void SaveTimeSlot()
    {
        Debug.Log($"[SaveTimeSlot] Current roosterInstructor value: {roosterInstructor}");

        if (!ValidateTimeFormat(startTijdInput.text) || !ValidateTimeFormat(eindTijdInput.text))
        {
            invalidTimeFormatWarning.SetActive(true);
            return;
        }

        invalidTimeFormatWarning.SetActive(false);

        string formattedStartTijd = FormatTime(startTijdInput.text);
        string formattedEindTijd = FormatTime(eindTijdInput.text);
        string selectedDag = GetDayName(selectedDay);

        // Get current week info from the selected week
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + 1); // Get Monday of current week
        monday = monday.AddDays(7 * selectedWeek); // Adjust for selected week offset
        int weekNum = System.Globalization.ISOWeek.GetWeekOfYear(monday);
        int year = monday.Year;

        Debug.Log($"Saving time slot: {selectedDag} {formattedStartTijd} - {formattedEindTijd} for week {weekNum}, year {year}");

        if (RijschoolApp.instance.selectedRijschool != null)
        {
            try 
            {
                var rijschool = RijschoolApp.instance.selectedRijschool;
                
                if (roosterInstructor)
                {
                    Debug.Log("Saving instructor availability");
                    if (rijschool.instructeurBeschikbaarheid == null)
                    {
                        rijschool.instructeurBeschikbaarheid = new List<Beschikbaarheid>();
                    }

                    // Find or create the day's availability for the specific week
                    var dagBeschikbaarheid = rijschool.instructeurBeschikbaarheid
                        .FirstOrDefault(b => b.dag == selectedDag && 
                                           b.weekNummer == weekNum && 
                                           b.jaar == year);

                    if (dagBeschikbaarheid == null)
                    {
                        dagBeschikbaarheid = new Beschikbaarheid 
                        { 
                            dag = selectedDag,
                            weekNummer = weekNum,
                            jaar = year,
                            tijdslots = new List<TimeSlot>()
                        };
                        rijschool.instructeurBeschikbaarheid.Add(dagBeschikbaarheid);
                    }

                    var newTimeSlot = new TimeSlot 
                    { 
                        startTijd = formattedStartTijd,
                        eindTijd = formattedEindTijd
                    };
                    dagBeschikbaarheid.tijdslots.Add(newTimeSlot);
                }
                else if (RijschoolApp.instance.selectedLeerling != null)
                {
                    Debug.Log("[SaveTimeSlot] Saving student availability");
                    var leerling = RijschoolApp.instance.selectedLeerling;
                    
                    if (leerling.beschikbaarheid == null)
                    {
                        leerling.beschikbaarheid = new List<Beschikbaarheid>();
                    }

                    // Find or create the day's availability for the specific week
                    var dagBeschikbaarheid = leerling.beschikbaarheid
                        .FirstOrDefault(b => b.dag == selectedDag && 
                                           b.weekNummer == weekNum && 
                                           b.jaar == year);

                    if (dagBeschikbaarheid == null)
                    {
                        dagBeschikbaarheid = new Beschikbaarheid 
                        { 
                            dag = selectedDag,
                            weekNummer = weekNum,
                            jaar = year,
                            tijdslots = new List<TimeSlot>()
                        };
                        leerling.beschikbaarheid.Add(dagBeschikbaarheid);
                    }

                    var newTimeSlot = new TimeSlot 
                    { 
                        startTijd = formattedStartTijd,
                        eindTijd = formattedEindTijd
                    };
                    dagBeschikbaarheid.tijdslots.Add(newTimeSlot);
                }

                // Save to server
                Debug.Log("[SaveTimeSlot] Saving to server...");
                await RijschoolApp.instance.UpdateRijschool(rijschool);

                startTijdInput.text = "";
                eindTijdInput.text = "";
                createLes.SetActive(false);
                LoadLessen();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveTimeSlot] Error: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    private string GetDayName(int dayIndex)
    {
        switch (dayIndex)
        {
            case 0: return "Monday";
            case 1: return "Tuesday";
            case 2: return "Wednesday";
            case 3: return "Thursday";
            case 4: return "Friday";
            case 5: return "Saturday";
            case 6: return "Sunday";
            default: return "Monday";
        }
    }

    // Add this helper method to calculate position and size
    private (float yPosition, float height) CalculateTimeSlotTransform(string startTime, string endTime)
    {
        // Convert times to hours (as float)
        float startHour = ConvertTimeToHours(startTime);
        float endHour = ConvertTimeToHours(endTime);
        
        // Calculate height (in pixels)
        float height = (endHour - startHour) * HOUR_HEIGHT;
        
        // Calculate Y position
        // Note: We subtract from 0 because Unity's Y axis goes up, but we want to go down
        float yPosition = -((startHour - START_HOUR) * HOUR_HEIGHT + height / 2f);
        
        return (yPosition, height);
    }

    private float ConvertTimeToHours(string time)
    {
        // Convert "HH:mm" to float hours
        string[] parts = time.Split(':');
        float hours = float.Parse(parts[0]);
        float minutes = float.Parse(parts[1]);
        return hours + (minutes / 60f);
    }

    // Add this method to display availability timeslots
    private void DisplayAvailabilityTimeSlots()
    {
        if (RijschoolApp.instance?.selectedRijschool?.instructeurBeschikbaarheid == null)
        {
            print("rijschool instance null");return;
        }
        int poolIndex = 0;
        
        // For each day
        for (int dagIndex = 0; dagIndex < dagenScrollview.Count; dagIndex++)
        {
            string dagNaam = GetDayName(dagIndex);
            
            // Find availability for this day
            var dagBeschikbaarheid = RijschoolApp.instance.selectedRijschool.instructeurBeschikbaarheid
                .FirstOrDefault(b => b.dag == dagNaam);

            if (dagBeschikbaarheid?.tijdslots != null)
            {
                foreach (var tijdslot in dagBeschikbaarheid.tijdslots)
                {
                    if (poolIndex >= lesPool.Count)
                    {
                        Debug.LogWarning("Not enough lesson objects in pool!");
                        break;
                    }

                    GameObject lesObject = lesPool[poolIndex];
                    poolIndex++;

                    // Set parent to correct day column
                    lesObject.transform.SetParent(dagenScrollview[dagIndex]);
                    
                    // Calculate position and size
                    var (yPos, height) = CalculateTimeSlotTransform(tijdslot.startTijd, tijdslot.eindTijd);
                    
                    // Set local position and size
                    RectTransform rectTransform = lesObject.GetComponent<RectTransform>();
                    rectTransform.localPosition = new Vector3(LESSON_X_POSITION, yPos, 0);
                    rectTransform.sizeDelta = new Vector2(LESSON_WIDTH, height);

                    // Update the text components
                    TextMeshProUGUI[] texts = lesObject.GetComponentsInChildren<TextMeshProUGUI>();
                    texts[0].text = $"{tijdslot.startTijd} - {tijdslot.eindTijd}";
                    texts[1].text = "Available";

                    // Set the color
                    Image lesImage = lesObject.GetComponent<Image>();
                    lesImage.color = Color.green;

                    lesObject.SetActive(true);
                }
            }
        }
    }

    // Add this method to display student availability timeslots
    private void DisplayStudentAvailabilityTimeSlots()
    {
        if (RijschoolApp.instance?.selectedLeerling?.beschikbaarheid == null)
            return;

        int poolIndex = 0;

        // For each day
        for (int dagIndex = 0; dagIndex < dagenScrollview.Count; dagIndex++)
        {
            string dagNaam = GetDayName(dagIndex);
            
            // Find availability for this day
            var dagBeschikbaarheid = RijschoolApp.instance.selectedLeerling.beschikbaarheid
                .FirstOrDefault(b => b.dag == dagNaam);

            if (dagBeschikbaarheid?.tijdslots != null)
            {
                foreach (var tijdslot in dagBeschikbaarheid.tijdslots)
                {
                    if (poolIndex >= lesPool.Count)
                    {
                        Debug.LogWarning("Not enough lesson objects in pool!");
                        break;
                    }

                    GameObject lesObject = lesPool[poolIndex];
                    poolIndex++;

                    // Set parent to correct day column
                    lesObject.transform.SetParent(dagenScrollview[dagIndex]);
                    
                    // Calculate position and size
                    var (yPos, height) = CalculateTimeSlotTransform(tijdslot.startTijd, tijdslot.eindTijd);
                    
                    // Set local position and size
                    RectTransform rectTransform = lesObject.GetComponent<RectTransform>();
                    rectTransform.localPosition = new Vector3(LESSON_X_POSITION, yPos, 0);
                    rectTransform.sizeDelta = new Vector2(LESSON_WIDTH, height);

                    // Update the text components
                    TextMeshProUGUI[] texts = lesObject.GetComponentsInChildren<TextMeshProUGUI>();
                    texts[0].text = $"{tijdslot.startTijd} - {tijdslot.eindTijd}";
                    texts[1].text = "Available";

                    // Set the color
                    Image lesImage = lesObject.GetComponent<Image>();
                    lesImage.color = lesManager.GetLeerlingColor(RijschoolApp.instance.selectedLeerling.naam);

                    lesObject.SetActive(true);
                }
            }
        }
    }
}

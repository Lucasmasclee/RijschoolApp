using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;

public class Rooster : MonoBehaviour
{
    public static Rooster instance;
    private bool roosterInstructor = false; // Of het rooster voor de instructeur of leerlingen is


    [SerializeField] private GameObject wekenScrollView;
    [SerializeField] private List<Transform> dagenScrollview;  // Parent for availability slots
    [SerializeField] private List<Transform> lessenParents;   // New parent list for actual lessons
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
    [SerializeField] private List<TMP_InputField> leerlingoverzichtMinutesPerLes;

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
    private const float LESSON_MARGIN = 10f;

    // Add these color constants at the top of the class
    private readonly Color AVAILABLE_SLOT_COLOR = new Color(0.4f, 0.8f, 0.4f, 0.8f);    // Soft green with transparency
    private readonly Color LESSON_BASE_COLOR = new Color(0.2f, 0.2f, 0.3f);      // Dark blue-grey that contrasts with white background and green slots
    private readonly Color LESSON_TEXT_COLOR = Color.white;                       // White text
    private readonly Color AVAILABLE_SLOT_TEXT_COLOR = new Color(0.1f, 0.4f, 0.1f); // Dark green text

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

        if (lessenParents == null || lessenParents.Count != 7)
        {
            Debug.LogError("lessenParents list is not set up correctly! Need exactly 7 parent objects.");
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
        RefreshDisplay();
    }

    public void GoToNextWeek()
    {
        selectedWeek++;
        UpdateWeekDisplay();
        RefreshDisplay();
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

    public void RoosterForInstructors(bool isInstructor)
    {
        // Clear any existing displays first
        foreach (GameObject obj in lesPool)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        // Reset UI states
        instructeurSelecteertLes.SetActive(false);
        leerlingSelecteertLes.SetActive(false);

        // Update the rooster type BEFORE loading lessons
        roosterInstructor = isInstructor;
        
        // Clear any previously selected timeslot
        selectedTimeSlot = null;

        Debug.Log($"Switching to {(isInstructor ? "Instructor" : "Student")} view");
        
        if (!isInstructor && RijschoolApp.instance.selectedLeerling != null)
        {
            Debug.Log($"Selected student: {RijschoolApp.instance.selectedLeerling.naam}");
        }

        // First clear everything
        foreach (GameObject obj in lesPool)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        // Load appropriate view based on user type
        if (isInstructor)
        {
            RefreshDisplay(); // This will load both lessons and availability for instructors
        }
        else
        {
            DisplayAvailabilityTimeSlots(false); // Only load availability for students
        }

        Debug.Log($"Finished loading {(isInstructor ? "instructor" : "student")} schedule");
    }

    public void LoadLessen(bool loadAvailability = true)
    {
        // Update rijschoolnaam text with selected rijschool's name
        if (RijschoolApp.instance?.selectedRijschool != null)
        {
            rijschoolnaam.text = RijschoolApp.instance.selectedRijschool.naam;
        }

        // First reset all input fields to empty
        foreach (var inputField in leerlingoverzichtMinutesPerLes)
        {
            inputField.text = "";
        }

        // Update MinutesPerLes fields
        if (RijschoolApp.instance.selectedRijschool?.leerlingen != null)
        {
            for (int i = 0; i < RijschoolApp.instance.selectedRijschool.leerlingen.Count; i++)
            {
                if (i < leerlingoverzichtMinutesPerLes.Count)
                {
                    int minutes = RijschoolApp.instance.selectedRijschool.leerlingen[i].minutesPerLes;
                    leerlingoverzichtMinutesPerLes[i].text = minutes.ToString();
                }
            }
        }

        // Clear all existing lesson objects
        foreach (GameObject obj in lesPool)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        // For instructors, first display availability slots
        if (roosterInstructor && loadAvailability)
        {
            DisplayAvailabilityTimeSlots(false);
        }

        // Load actual lessons
        var rijschool = RijschoolApp.instance?.selectedRijschool;
        if (rijschool?.rooster?.weken == null) return;

        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + 1);
        monday = monday.AddDays(7 * selectedWeek);
        int weekNum = System.Globalization.ISOWeek.GetWeekOfYear(monday);

        // Find the week in the rooster
        Week targetWeek = rijschool.rooster.weken.FirstOrDefault(w => w.weekNummer == weekNum);
        if (targetWeek?.lessen == null) return;

        // Get the next available pool index
        int poolIndex = GetNextAvailablePoolIndex();

        foreach (var les in targetWeek.lessen)
        {
            if (poolIndex >= lesPool.Count)
            {
                Debug.LogWarning("Not enough lesson objects in pool!");
                break;
            }

            // Convert date to day name
            System.DateTime lesDate = System.DateTime.ParseExact(les.datum, "dd-MM-yyyy", null);
            string dayName = lesDate.ToString("dddd");
            int dayIndex = GetDayIndex(dayName);

            if (dayIndex >= 0 && dayIndex < lessenParents.Count)  // Changed from dagenScrollview to lessenParents
            {
                GameObject lesObject = lesPool[poolIndex];
                poolIndex++;

                // Set parent to correct lessons parent for this day
                lesObject.transform.SetParent(lessenParents[dayIndex]);  // Using new parent

                // Calculate position and size
                var (yPos, height) = CalculateTimeSlotTransform(les.begintijd, les.eindtijd);
                
                // Set local position and size
                RectTransform rectTransform = lesObject.GetComponent<RectTransform>();
                rectTransform.localPosition = new Vector3(LESSON_X_POSITION, yPos, 0);
                rectTransform.sizeDelta = new Vector2(LESSON_WIDTH - (LESSON_MARGIN * 1), height);

                // Set the color for all lessons to the same base color
                Image lesImage = lesObject.GetComponent<Image>();
                lesImage.color = LESSON_BASE_COLOR;

                // Enhanced text visibility with white text
                TextMeshProUGUI[] texts = lesObject.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var text in texts)
                {
                    if (text != null)
                    {
                        text.color = LESSON_TEXT_COLOR;
                        text.fontStyle = FontStyles.Bold;
                    }
                }

                // Update the text components for actual lessons
                texts[0].text = $"{les.begintijd} - {les.eindtijd}";
                texts[1].text = les.leerlingNaam ?? "";

                // Add click event listener
                Button button = lesObject.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    Les currentLes = les;
                    button.onClick.AddListener(() => OnLesSelected(currentLes));
                }

                lesObject.SetActive(true);
                
                Debug.Log($"Displayed lesson: {les.begintijd} - {les.eindtijd} for {les.leerlingNaam} on day {dayIndex}");
            }
        }
    }

    // Helper method to find the next available pool index
    private int GetNextAvailablePoolIndex()
    {
        int index = 0;
        while (index < lesPool.Count && lesPool[index].activeSelf)
        {
            index++;
        }
        return index;
    }

    // Helper method to convert day name to index
    private int GetDayIndex(string dayName)
    {
        return dayName.ToLower() switch
        {
            "monday" or "maandag" => 0,
            "tuesday" or "dinsdag" => 1,
            "wednesday" or "woensdag" => 2,
            "thursday" or "donderdag" => 3,
            "friday" or "vrijdag" => 4,
            "saturday" or "zaterdag" => 5,
            "sunday" or "zondag" => 6,
            _ => -1
        };
    }

    public void DisplayAvailabilityTimeSlots(bool loadLessons = true)
    {
        var rijschool = RijschoolApp.instance?.selectedRijschool;
        if (rijschool == null) return;

        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + 1);
        monday = monday.AddDays(7 * selectedWeek);
        int weekNum = System.Globalization.ISOWeek.GetWeekOfYear(monday);
        int year = monday.Year;

        // Only clear the pool if we're not also loading lessons
        if (!loadLessons)
        {
            foreach (GameObject obj in lesPool)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        // Initialize poolIndex
        int poolIndex = 0;

        // Determine which availability list to use
        List<Beschikbaarheid> availabilityList;
        string userType;
        string userName;

        if (roosterInstructor)
        {
            availabilityList = rijschool.instructeurBeschikbaarheid ?? new List<Beschikbaarheid>();
            userType = "Instructor";
            userName = "Instructor";
            Debug.Log("Using instructor availability list");
        }
        else if (RijschoolApp.instance.selectedLeerling != null)
        {
            availabilityList = RijschoolApp.instance.selectedLeerling.beschikbaarheid ?? new List<Beschikbaarheid>();
            userType = "Student";
            userName = RijschoolApp.instance.selectedLeerling.naam;
            Debug.Log($"Using student availability list for {userName}");
        }
        else
        {
            Debug.LogWarning("No valid user type selected");
            return;
        }

        // Debug log the available slots
        foreach (var availability in availabilityList)
        {
            Debug.Log($"Available slots for {userType} {userName} on {availability.dag}: " +
                     $"Week {availability.weekNummer}, Year {availability.jaar}");
            foreach (var slot in availability.tijdslots)
            {
                Debug.Log($"- Slot: {slot.startTijd} - {slot.eindTijd}");
            }
        }

        // For each day
        for (int dagIndex = 0; dagIndex < dagenScrollview.Count; dagIndex++)
        {
            string dagNaam = GetDayName(dagIndex);
            
            // Find availability for this specific day AND week AND year
            var dagBeschikbaarheid = availabilityList
                .FirstOrDefault(b => b.dag == dagNaam && 
                                   b.weekNummer == weekNum && 
                                   b.jaar == year);

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

                    // Set the color and text properties
                    Image lesImage = lesObject.GetComponent<Image>();
                    lesImage.color = AVAILABLE_SLOT_COLOR;

                    // Update text components with enhanced visibility and clear text
                    TextMeshProUGUI[] texts = lesObject.GetComponentsInChildren<TextMeshProUGUI>();
                    foreach (var text in texts)
                    {
                        if (text != null)
                        {
                            text.text = ""; // Clear the text for availability slots
                            text.color = AVAILABLE_SLOT_TEXT_COLOR;
                            text.fontStyle = FontStyles.Bold;
                        }
                    }

                    // Add click event listener
                    Button button = lesObject.GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.RemoveAllListeners();
                        int currentDayIndex = dagIndex;
                        string currentStartTime = tijdslot.startTijd;
                        string currentEndTime = tijdslot.eindTijd;
                        button.onClick.AddListener(() => OnAvailabilitySlotClicked(currentDayIndex, currentStartTime, currentEndTime));
                    }

                    lesObject.SetActive(true);
                    Debug.Log($"Created availability slot for {userType} {userName} on {dagNaam}: {tijdslot.startTijd} - {tijdslot.eindTijd}");
                }
            }
        }

        // After displaying availability slots, load lessons on top if needed
        if (roosterInstructor && loadLessons)
        {
            LoadLessen(false); // Pass false to prevent recursive call
        }
    }

    private void OnAvailabilitySlotClicked(int dayIndex, string startTime, string endTime)
    {
        selectedDay = dayIndex;
        string selectedDag = GetDayName(dayIndex);

        Debug.Log($"OnAvailabilitySlotClicked - User type: {(roosterInstructor ? "Instructor" : "Student")}");

        // Reset selection panels
        instructeurSelecteertLes.SetActive(false);
        leerlingSelecteertLes.SetActive(false);

        // Store the selected timeslot information
        selectedTimeSlot = new TimeSlotInfo
        {
            startTime = startTime,
            endTime = endTime,
            day = selectedDag
        };

        // Get the instructor and student detail GameObjects
        GameObject instructorLes = LeraarLesLeerlingLes[0];
        GameObject studentLes = LeraarLesLeerlingLes[1];

        // Update UI based on user type
        if (roosterInstructor)
        {
            Debug.Log($"Showing instructor selection for slot: {selectedDag} {startTime} - {endTime}");
            instructeurSelecteertLes.SetActive(true);
            instructorLes.SetActive(true);
            studentLes.SetActive(false);

            // Update text to show it's an availability slot
            TextMeshProUGUI[] instructorTexts = instructorLes.GetComponentsInChildren<TextMeshProUGUI>();
            instructorTexts[0].text = $"{startTime} - {endTime}";
            instructorTexts[1].text = "Instructeur beschikbaar";
        }
        else if (RijschoolApp.instance.selectedLeerling != null)
        {
            Debug.Log($"Showing student selection for slot: {selectedDag} {startTime} - {endTime}");
            leerlingSelecteertLes.SetActive(true);
            instructorLes.SetActive(false);
            studentLes.SetActive(true);

            // Update text to show it's an availability slot
            TextMeshProUGUI[] studentTexts = studentLes.GetComponentsInChildren<TextMeshProUGUI>();
            studentTexts[0].text = $"{startTime} - {endTime}";
            studentTexts[1].text = $"{RijschoolApp.instance.selectedLeerling.naam}'s Availability";
        }

        Debug.Log($"Selected availability slot: {selectedDag} {startTime} - {endTime} for {(roosterInstructor ? "Instructor" : RijschoolApp.instance.selectedLeerling?.naam)}");
    }

    public void OnLesSelected(Les les)
    {
        selectedLes = les;

        // Reset visibility of UI elements
        instructeurSelecteertLes.SetActive(false);
        leerlingSelecteertLes.SetActive(false);

        // Get the instructor and student detail GameObjects
        GameObject instructorLes = LeraarLesLeerlingLes[0];
        GameObject studentLes = LeraarLesLeerlingLes[1];

        // If this is an actual lesson
        if (selectedLes != null)
        {
            // Update instructor and student lesson details
            foreach (GameObject detailLes in new[] { instructorLes, studentLes })
            {
                TextMeshProUGUI[] texts = detailLes.GetComponentsInChildren<TextMeshProUGUI>();
                texts[0].text = selectedLes.begintijd + " - " + selectedLes.eindtijd;
                texts[1].text = selectedLes.leerlingNaam ?? ""; // Display student name or empty if null
            }

            // Show the appropriate view based on user type
            if (roosterInstructor)
            {
                instructeurSelecteertLes.SetActive(true);
                instructorLes.SetActive(true);
                studentLes.SetActive(false);

                // Set up delete button to call VerwijderLes
                verwijderReserving.GetComponent<Button>().onClick.RemoveAllListeners();
                verwijderReserving.GetComponent<Button>().onClick.AddListener(() => VerwijderLes(selectedDay));
            }
            else
            {
                leerlingSelecteertLes.SetActive(true);
                instructorLes.SetActive(false);
                studentLes.SetActive(true);
            }
        }
        // If this is an availability timeslot
        else
        {
            // Get the clicked GameObject
            GameObject clickedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            if (clickedObject != null)
            {
                TextMeshProUGUI[] texts = clickedObject.GetComponentsInChildren<TextMeshProUGUI>();
                string[] times = texts[0].text.Split('-');
                string startTime = times[0].Trim();
                string endTime = times[1].Trim();
                string selectedDag = GetDayName(selectedDay);

                // Show the appropriate view based on user type
                if (roosterInstructor)
                {
                    instructeurSelecteertLes.SetActive(true);
                    instructorLes.SetActive(true);
                    studentLes.SetActive(false);

                    // Set up delete button to call DeleteAvailabilityTimeSlot
                    verwijderReserving.GetComponent<Button>().onClick.RemoveAllListeners();
                    verwijderReserving.GetComponent<Button>().onClick.AddListener(DeleteAvailabilityTimeSlot);
                }
                else if (RijschoolApp.instance.selectedLeerling != null)
                {
                    leerlingSelecteertLes.SetActive(true);
                    instructorLes.SetActive(false);
                    studentLes.SetActive(true);
                }

                // Store the selected timeslot information for deletion
                selectedTimeSlot = new TimeSlotInfo
                {
                    startTime = startTime,
                    endTime = endTime,
                    day = selectedDag
                };
            }
        }
    }

    // Add this class to store selected timeslot information
    private class TimeSlotInfo
    {
        public string startTime;
        public string endTime;
        public string day;
    }

    private TimeSlotInfo selectedTimeSlot;

    public async void DeleteAvailabilityTimeSlot()
    {
        // If this is an actual lesson
        if (selectedLes != null)
        {
            await VerwijderLes(selectedDay);
            return;
        }

        // If this is an availability slot
        if (selectedTimeSlot == null) return;

        var rijschool = RijschoolApp.instance.selectedRijschool;
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + 1);
        monday = monday.AddDays(7 * selectedWeek);
        int weekNum = System.Globalization.ISOWeek.GetWeekOfYear(monday);
        int year = monday.Year;

        if (roosterInstructor)
        {
            // Delete instructor availability
            var availability = rijschool.instructeurBeschikbaarheid
                .FirstOrDefault(b => b.dag == selectedTimeSlot.day && 
                                   b.weekNummer == weekNum && 
                                   b.jaar == year);

            if (availability != null)
            {
                availability.tijdslots.RemoveAll(t => 
                    t.startTijd == selectedTimeSlot.startTime && 
                    t.eindTijd == selectedTimeSlot.endTime);
            }
        }
        else if (RijschoolApp.instance.selectedLeerling != null)
        {
            // Delete student availability
            var leerling = RijschoolApp.instance.selectedLeerling;
            var availability = leerling.beschikbaarheid
                .FirstOrDefault(b => b.dag == selectedTimeSlot.day && 
                                   b.weekNummer == weekNum && 
                                   b.jaar == year);

            if (availability != null)
            {
                availability.tijdslots.RemoveAll(t => 
                    t.startTijd == selectedTimeSlot.startTime && 
                    t.eindTijd == selectedTimeSlot.endTime);
            }
        }

        // Save changes and refresh display appropriately
        await RijschoolApp.instance.UpdateRijschool(rijschool);
        RefreshDisplay();
        
        // Reset selection
        selectedTimeSlot = null;
        instructeurSelecteertLes.SetActive(false);
        leerlingSelecteertLes.SetActive(false);
    }

    public async Task VerwijderLes(int dagIndex)
    {
        if (RijschoolApp.instance.selectedRijschool != null && selectedLes != null)
        {
            // Find the week that contains this lesson
            Week targetWeek = RijschoolApp.instance.selectedRijschool.rooster.weken
                .FirstOrDefault(w => w.weekNummer == selectedLes.weekNummer);

            if (targetWeek != null && targetWeek.lessen.Remove(selectedLes))
            {
                Debug.Log($"Les verwijderd uit week {selectedLes.weekNummer}");
                selectedLes = null;
                
                // Save to server
                await RijschoolApp.instance.UpdateRijschool(RijschoolApp.instance.selectedRijschool);
                
                // Refresh the display
                RefreshDisplay();
            }
            else
            {
                Debug.LogWarning($"Les niet gevonden in week {selectedLes.weekNummer}!");
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
                    //reserveerButton.SetActive(false);
                    //verwijderReserving.SetActive(true);

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
                //reserveerButton.SetActive(true);
                //verwijderReserving.SetActive(false);
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

    private static int TimeStringToMinutes(string time)
    {
        var parts = time.Split(':');
        return int.Parse(parts[0]) * 60 + int.Parse(parts[1]);
    }

    private class ScheduleSlot
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public float DurationMinutes { get; set; }
        public string Day { get; set; }
        public bool IsExistingLesson { get; set; }
        public string ExistingLeerlingNaam { get; set; }

        public ScheduleSlot(string start, string end, string day, bool isExisting = false, string leerlingNaam = null)
        {
            StartTime = start;
            EndTime = end;
            Day = day;
            IsExistingLesson = isExisting;
            ExistingLeerlingNaam = leerlingNaam;
            DurationMinutes = CalculateDurationMinutes(start, end);
        }

        private float CalculateDurationMinutes(string start, string end)
        {
            return TimeStringToMinutes(end) - TimeStringToMinutes(start);
        }

        public bool OverlapsWith(ScheduleSlot other)
        {
            if (Day != other.Day) return false;
            
            var thisStart = TimeStringToMinutes(StartTime);
            var thisEnd = TimeStringToMinutes(EndTime);
            var otherStart = TimeStringToMinutes(other.StartTime);
            var otherEnd = TimeStringToMinutes(other.EndTime);

            return !(thisEnd <= otherStart || thisStart >= otherEnd);
        }
    }

    public async Task<bool> GenerateSchedule(bool minimizeChanges = true)
    {
        Debug.Log("Starting schedule generation...");
        
        DateTime now = DateTime.Now;
        DateTime monday = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        monday = monday.AddDays(7 * selectedWeek);
        int weekNum = System.Globalization.ISOWeek.GetWeekOfYear(monday);
        int year = monday.Year;

        var rijschool = RijschoolApp.instance.selectedRijschool;
        if (rijschool == null) return false;

        // Clear existing schedule for this week
        var targetWeek = rijschool.rooster.weken.Find(w => w.weekNummer == weekNum && w.jaar == year);
        if (targetWeek == null)
        {
            targetWeek = new Week { weekNummer = weekNum, jaar = year };
            rijschool.rooster.weken.Add(targetWeek);
        }
        targetWeek.lessen = new List<Les>();

        // Track lessons per student per day
        var lessonsPerStudentPerDay = new Dictionary<string, HashSet<string>>();
        foreach (var leerling in rijschool.leerlingen)
        {
            lessonsPerStudentPerDay[leerling.naam] = new HashSet<string>();
        }

        // Get instructor availability and convert to ScheduleSlots
        var instructorSlots = new List<ScheduleSlot>();
        foreach (var beschikbaarheid in rijschool.instructeurBeschikbaarheid
            .Where(b => b.weekNummer == weekNum && b.jaar == year))
        {
            foreach (var slot in beschikbaarheid.tijdslots)
            {
                instructorSlots.Add(new ScheduleSlot(
                    slot.startTijd,
                    slot.eindTijd,
                    beschikbaarheid.dag,
                    false
                ));
                Debug.Log($"Added instructor slot: {beschikbaarheid.dag} {slot.startTijd}-{slot.eindTijd}");
            }
        }

        // Sort instructor slots by day and time
        instructorSlots.Sort((a, b) => {
            var dayCompare = GetDayValue(a.Day).CompareTo(GetDayValue(b.Day));
            if (dayCompare != 0) return dayCompare;
            return TimeStringToMinutes(a.StartTime).CompareTo(TimeStringToMinutes(b.StartTime));
        });

        // Initialize student data
        var studentSlots = new Dictionary<string, List<ScheduleSlot>>();
        var studentRequirements = new Dictionary<string, (int frequency, int minutes)>();
        var assignedLessonsPerStudent = new Dictionary<string, int>();

        foreach (var leerling in rijschool.leerlingen)
        {
            studentRequirements[leerling.naam] = (leerling.frequentie, leerling.minutesPerLes);
            assignedLessonsPerStudent[leerling.naam] = 0;
            studentSlots[leerling.naam] = new List<ScheduleSlot>();

            var studentAvailability = leerling.beschikbaarheid?
                .Where(b => b.weekNummer == weekNum && b.jaar == year)
                .ToList() ?? new List<Beschikbaarheid>();

            foreach (var beschikbaarheid in studentAvailability)
            {
                foreach (var slot in beschikbaarheid.tijdslots)
                {
                    studentSlots[leerling.naam].Add(new ScheduleSlot(
                        slot.startTijd,
                        slot.eindTijd,
                        beschikbaarheid.dag,
                        false
                    ));
                }
            }
        }

        // Add debug logging for student requirements and availability
        foreach (var student in rijschool.leerlingen)
        {
            Debug.Log($"Student {student.naam}: Needs {student.frequentie} lessons, {student.minutesPerLes} minutes each");
            var availability = studentSlots[student.naam];
            foreach (var slot in availability)
            {
                Debug.Log($"- Available: {slot.Day} {slot.StartTime}-{slot.EndTime}");
            }
        }

        // Process each instructor slot
        foreach (var instructorSlot in instructorSlots)
        {
            Debug.Log($"\nProcessing instructor slot: {instructorSlot.Day} {instructorSlot.StartTime}-{instructorSlot.EndTime}");
            
            var slotStartMinutes = TimeStringToMinutes(instructorSlot.StartTime);
            var slotEndMinutes = TimeStringToMinutes(instructorSlot.EndTime);
            var currentTimeInSlot = slotStartMinutes;

            while (currentTimeInSlot < slotEndMinutes)
            {
                // Find eligible students for this time
                var eligibleStudents = studentSlots
                    .Where(kvp => {
                        var student = kvp.Key;
                        var slots = kvp.Value;
                        
                        // Debug logging for eligibility checks
                        Debug.Log($"\nChecking eligibility for {student}:");
                        Debug.Log($"- Lessons needed: {studentRequirements[student].frequency}, " +
                                $"assigned: {assignedLessonsPerStudent[student]}");
                        Debug.Log($"- Has lesson today: {lessonsPerStudentPerDay[student].Contains(instructorSlot.Day)}");
                        
                        // Check if student still needs lessons
                        if (assignedLessonsPerStudent[student] >= studentRequirements[student].frequency)
                        {
                            Debug.Log($"- Rejected: Already has enough lessons");
                            return false;
                        }

                        // Check if student already has a lesson this day
                        if (lessonsPerStudentPerDay[student].Contains(instructorSlot.Day))
                        {
                            Debug.Log($"- Rejected: Already has lesson today");
                            return false;
                        }

                        var lessonDuration = studentRequirements[student].minutes;
                        if (currentTimeInSlot + lessonDuration > slotEndMinutes)
                        {
                            Debug.Log($"- Rejected: Lesson wouldn't fit in remaining time");
                            return false;
                        }

                        // Check if student is available at this time
                        var isAvailable = slots.Any(studentSlot => 
                            studentSlot.Day == instructorSlot.Day &&
                            IsTimeSlotAvailable(
                                MinutesToTimeString(currentTimeInSlot),
                                MinutesToTimeString(currentTimeInSlot + lessonDuration),
                                studentSlot.StartTime,
                                studentSlot.EndTime
                            ));

                        Debug.Log($"- Available at this time: {isAvailable}");
                        return isAvailable;
                    })
                    .OrderByDescending(s => {
                        var remaining = studentRequirements[s.Key].frequency - assignedLessonsPerStudent[s.Key];
                        Debug.Log($"Student {s.Key} has {remaining} lessons remaining");
                        return remaining;
                    })
                    .ToList();

                if (eligibleStudents.Any())
                {
                    var selectedStudent = eligibleStudents.First().Key;
                    var lessonDuration = studentRequirements[selectedStudent].minutes;

                    var lesson = new Les
                    {
                        begintijd = MinutesToTimeString(currentTimeInSlot),
                        eindtijd = MinutesToTimeString(currentTimeInSlot + lessonDuration),
                        leerlingNaam = selectedStudent,
                        datum = GetDateForDayInWeek(instructorSlot.Day, selectedWeek),
                        weekNummer = weekNum,
                        isAutomatischGepland = true
                    };

                    targetWeek.lessen.Add(lesson);
                    assignedLessonsPerStudent[selectedStudent]++;
                    lessonsPerStudentPerDay[selectedStudent].Add(instructorSlot.Day);
                    
                    Debug.Log($"Created lesson for {selectedStudent}: {lesson.begintijd}-{lesson.eindtijd} on {instructorSlot.Day}");
                    
                    currentTimeInSlot += lessonDuration;
                }
                else
                {
                    // If no eligible students found, move time forward
                    currentTimeInSlot += 15; // Move in 15-minute increments
                    Debug.Log($"No eligible students found for {instructorSlot.Day} at {MinutesToTimeString(currentTimeInSlot)}");
                }
            }
        }

        // After scheduling, log summary
        Debug.Log("\nScheduling Summary:");
        foreach (var student in rijschool.leerlingen)
        {
            Debug.Log($"{student.naam}: {assignedLessonsPerStudent[student.naam]} of {student.frequentie} lessons scheduled");
        }

        // Save the updated schedule
        await RijschoolApp.instance.UpdateRijschool(rijschool);
        LoadLessen();
        return true;
    }

    private bool IsTimeSlotAvailable(string lessonStart, string lessonEnd, 
        string availabilityStart, string availabilityEnd)
    {
        int lessonStartMins = TimeStringToMinutes(lessonStart);
        int lessonEndMins = TimeStringToMinutes(lessonEnd);
        int availStartMins = TimeStringToMinutes(availabilityStart);
        int availEndMins = TimeStringToMinutes(availabilityEnd);

        return lessonStartMins >= availStartMins && lessonEndMins <= availEndMins;
    }

    private string MinutesToTimeString(int minutes)
    {
        int hours = minutes / 60;
        int mins = minutes % 60;
        return $"{hours:00}:{mins:00}";
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

    private bool ValidateTimeOrder(string startTime, string endTime)
    {
        if (!ValidateTimeFormat(startTime) || !ValidateTimeFormat(endTime))
            return false;

        string[] startParts = startTime.Split(':');
        string[] endParts = endTime.Split(':');
        
        int startHours = int.Parse(startParts[0]);
        int startMinutes = int.Parse(startParts[1]);
        int endHours = int.Parse(endParts[0]);
        int endMinutes = int.Parse(endParts[1]);

        if (endHours < startHours || (endHours == startHours && endMinutes <= startMinutes))
            return false;

        return true;
    }

    public async void SaveTimeSlot()
    {
        // Check both format and time order
        if (!ValidateTimeOrder(startTijdInput.text, eindTijdInput.text))
        {
            invalidTimeFormatWarning.SetActive(true);
            return;
        }

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
                    Debug.Log($"Saving availability for student {RijschoolApp.instance.selectedLeerling.naam}");
                    var leerling = RijschoolApp.instance.selectedLeerling;
                    
                    // Initialize beschikbaarheid if null
                    if (leerling.beschikbaarheid == null)
                    {
                        leerling.beschikbaarheid = new List<Beschikbaarheid>();
                    }

                    // Find or create availability for this specific day and week
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

                    // Add the new timeslot
                    var newTimeSlot = new TimeSlot 
                    { 
                        startTijd = formattedStartTijd,
                        eindTijd = formattedEindTijd
                    };
                    dagBeschikbaarheid.tijdslots.Add(newTimeSlot);

                    Debug.Log($"Added timeslot for student: {formattedStartTijd} - {formattedEindTijd}");
                }

                // Save to server
                Debug.Log("[SaveTimeSlot] Saving to server...");
                await RijschoolApp.instance.UpdateRijschool(rijschool);

                startTijdInput.text = "";
                eindTijdInput.text = "";
                createLes.SetActive(false);
                DisplayAvailabilityTimeSlots();
                if(roosterInstructor)
                {
                    LoadLessen();
                }
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

    // Add this helper method to verify the current state
    private void VerifyCurrentState()
    {
        Debug.Log($"Current State:");
        Debug.Log($"- Is Instructor View: {roosterInstructor}");
        Debug.Log($"- Selected Student: {RijschoolApp.instance.selectedLeerling?.naam ?? "None"}");
        if (selectedTimeSlot != null)
        {
            Debug.Log($"- Selected Timeslot: {selectedTimeSlot.day} {selectedTimeSlot.startTime}-{selectedTimeSlot.endTime}");
        }
        else
        {
            Debug.Log("- No timeslot selected");
        }
    }

    private int GetDayValue(string day)
    {
        return day.ToLower() switch
        {
            "monday" => 0,
            "tuesday" => 1,
            "wednesday" => 2,
            "thursday" => 3,
            "friday" => 4,
            "saturday" => 5,
            "sunday" => 6,
            _ => 0
        };
    }

    private string GetDateForDayInWeek(string dayName, int weekOffset)
    {
        // Get the current date
        DateTime now = DateTime.Now;
        
        // Find Monday of the current week
        DateTime monday = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        
        // Adjust for selected week
        monday = monday.AddDays(7 * weekOffset);
        
        // Get the target day
        int dayOffset = GetDayValue(dayName);
        DateTime targetDate = monday.AddDays(dayOffset);
        
        return targetDate.ToString("dd-MM-yyyy");
    }

    public async void ResetWeekAvailability()
    {
        Debug.Log("Resetting availability for the selected week");
        
        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + 1);
        monday = monday.AddDays(7 * selectedWeek);
        int weekNum = System.Globalization.ISOWeek.GetWeekOfYear(monday);
        int year = monday.Year;

        var rijschool = RijschoolApp.instance.selectedRijschool;
        if (rijschool == null) return;

        if (roosterInstructor)
        {
            // Reset instructor availability
            rijschool.instructeurBeschikbaarheid.RemoveAll(b => 
                b.weekNummer == weekNum && 
                b.jaar == year);
            
            Debug.Log($"Reset instructor availability for week {weekNum}");
        }
        else if (RijschoolApp.instance.selectedLeerling != null)
        {
            // Reset student availability
            var student = RijschoolApp.instance.selectedLeerling;
            student.beschikbaarheid.RemoveAll(b => 
                b.weekNummer == weekNum && 
                b.jaar == year);
            
            Debug.Log($"Reset availability for student {student.naam} in week {weekNum}");
        }

        // Save changes and refresh display
        await RijschoolApp.instance.UpdateRijschool(rijschool);
        LoadLessen();
    }

    public async void ResetWeekLessons()
    {
        Debug.Log("Resetting lessons for the selected week");
        
        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + 1);
        monday = monday.AddDays(7 * selectedWeek);
        int weekNum = System.Globalization.ISOWeek.GetWeekOfYear(monday);
        int year = monday.Year;

        var rijschool = RijschoolApp.instance.selectedRijschool;
        if (rijschool?.rooster?.weken == null) return;

        // Find the week in the rooster
        Week targetWeek = rijschool.rooster.weken.FirstOrDefault(w => 
            w.weekNummer == weekNum && 
            w.jaar == year);

        if (targetWeek != null)
        {
            // Clear all lessons for this week
            targetWeek.lessen.Clear();
            Debug.Log($"Reset all lessons for week {weekNum}");

            // Save changes and refresh display
            await RijschoolApp.instance.UpdateRijschool(rijschool);
            LoadLessen();
        }
    }

    public async void OnMinutesPerLesChanged(int studentIndex)
    {
        // Validate input
        if (studentIndex < 0 || 
            studentIndex >= leerlingoverzichtMinutesPerLes.Count || 
            RijschoolApp.instance.selectedRijschool?.leerlingen == null || 
            studentIndex >= RijschoolApp.instance.selectedRijschool.leerlingen.Count)
        {
            Debug.LogWarning($"Invalid student index: {studentIndex}");
            return;
        }

        // Get the input field value
        string inputValue = leerlingoverzichtMinutesPerLes[studentIndex].text;
        
        // Try to parse the input value
        if (int.TryParse(inputValue, out int minutes))
        {
            // Clamp the value to a reasonable range (e.g., 15-180 minutes)
            minutes = Mathf.Clamp(minutes, 15, 180);
            
            // Update the student's MinutesPerLes
            RijschoolApp.instance.selectedRijschool.leerlingen[studentIndex].minutesPerLes = minutes;
            
            // Update the input field to show the clamped value if it was changed
            leerlingoverzichtMinutesPerLes[studentIndex].text = minutes.ToString();
            
            // Save changes to server
            await RijschoolApp.instance.UpdateRijschool(RijschoolApp.instance.selectedRijschool);
            
            Debug.Log($"Updated MinutesPerLes for student {studentIndex} to {minutes} minutes");
        }
        else
        {
            // If parsing failed, reset to default value (60)
            leerlingoverzichtMinutesPerLes[studentIndex].text = "60";
            RijschoolApp.instance.selectedRijschool.leerlingen[studentIndex].minutesPerLes = 60;
            Debug.LogWarning($"Invalid input for MinutesPerLes. Reset to default value for student {studentIndex}");
        }
    }

    // Add these two public methods
    public void GenerateScheduleMinChanges()
    {
        _ = GenerateSchedule(minimizeChanges: true);
    }

    public void GenerateScheduleMaxChanges()
    {
        _ = GenerateSchedule(minimizeChanges: false);
    }

    // Helper method to refresh the display based on user type
    public void RefreshDisplay()
    {
        if (roosterInstructor)
        {
            LoadLessen(); // This loads both lessons and availability for instructors
        }
        else
        {
            DisplayAvailabilityTimeSlots(); // This only loads availability for students
        }
    }
}


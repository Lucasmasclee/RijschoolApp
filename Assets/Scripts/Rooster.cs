using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;
using Unity.Services.Analytics;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using System.Globalization;

public class Rooster : MonoBehaviour
{
    public static Rooster instance;
    private bool roosterInstructor = false;

    [SerializeField] private GameObject wekenScrollView;
    [SerializeField] private List<Transform> dagenScrollview;
    [SerializeField] private List<Transform> lessenParents;
    private List<Image> wekenScrollViewButtons;

    [SerializeField] private TextMeshProUGUI rijschoolnaam;
    [SerializeField] private List<GameObject> onlyForInstructors;
    [SerializeField] private GameObject lesPoolParent;
    private List<GameObject> lesPool;
    [SerializeField] private List<GameObject> LeraarLesLeerlingLes;

    private Les selectedLes;
    [SerializeField] private TMP_InputField lesBeginTijd;
    [SerializeField] private TMP_InputField lesEindTijd;
    [SerializeField] private TMP_InputField lesNotities;

    public int selectedWeek { get; private set; }
    private int selectedDay = 0;

    [SerializeField] private Button previousWeekButton;
    [SerializeField] private Button nextWeekButton;
    [SerializeField] private TextMeshProUGUI weekDateText;
    
    private int currentWeek;
    private int manualLessonLeerling;

    [SerializeField] private LesManager lesManager;

    [SerializeField] private GameObject reserveerButton;
    [SerializeField] private GameObject verwijderReserving;
    [SerializeField] private GameObject instructeurSelecteertLes;
    [SerializeField] private GameObject leerlingSelecteertLes;
    [SerializeField] private List<GameObject> leerlingenToewijzen;
    [SerializeField] private List<GameObject> kiesleerlingButtons; // Has only 1 element
    [SerializeField] private List<GameObject> kiesleerlingButtons2; // Has 30 elements
    [SerializeField] private List<GameObject> leerlingoverzicht;
    [SerializeField] private List<TMP_InputField> leerlingoverzichtMinutesPerLes;

    [SerializeField] private GameObject timeFormatWarning;
    [SerializeField] private TMP_InputField startTijdInput;
    [SerializeField] private TMP_InputField eindTijdInput;
    [SerializeField] private GameObject invalidTimeFormatWarning;
    [SerializeField] private GameObject createLes;

    [SerializeField] private List<GameObject> nextLeerlingRoosterButtons;
    [SerializeField] private List<GameObject> dagOverzichtLessen;

    [SerializeField] private List<TMP_InputField> leerlingoverzichtWoonplaats;
    [SerializeField] private List<TMP_InputField> leerlingoverzichtAdres;

    [SerializeField] private List<TMP_InputField> leerlingoverzichtnaam;

    private const float HOUR_HEIGHT = 100f;
    private const float START_HOUR = 6f;
    private const float LESSON_WIDTH = 130f;
    private const float LESSON_X_POSITION = 72f;
    private const float LESSON_MARGIN = 10f;

    private readonly Color AVAILABLE_SLOT_COLOR = new Color(0.4f, 0.8f, 0.4f, 0.8f);
    private readonly Color LESSON_BASE_COLOR = new Color(0.2f, 0.2f, 0.3f);
    private readonly Color LESSON_TEXT_COLOR = Color.white;
    private readonly Color AVAILABLE_SLOT_TEXT_COLOR = new Color(0.1f, 0.4f, 0.1f);

    [SerializeField] private List<TextMeshProUGUI> dagDatumTexts;
    [SerializeField] private TextMeshProUGUI weekOffsetText;

    [SerializeField] private TextMeshProUGUI ingelogdAlsText;

    // Add this field near the top with other SerializeFields
    [SerializeField] private GameObject roosterStatistics;

    [SerializeField] private GameObject leerlingNaamWaarschuwing;

    // Add this near the top with other SerializeField declarations
    [SerializeField] private GameObject noStudentsMessage;

    [SerializeField] private GameObject Buttons;

    // Add near other SerializeField declarations
    //[SerializeField] private TextMeshProUGUI copyForXWeeks;

    // Add this field with the other SerializeField declarations
    [SerializeField] private TextMeshProUGUI selectedDayText;
    [SerializeField] private TextMeshProUGUI selectedDayText2;

    [SerializeField] private TextMeshProUGUI lesGeselecteerdText;  // Add this at the top with other SerializeFields

    // Add this near the top of the class with other private variables
    private int copyForXWeeks = 4; // Default to 4 weeks

    // Add near the top with other SerializeField declarations
    [SerializeField] private Button MapsExtensie;

    // Add this with other SerializeField declarations at the top
    [SerializeField] private GameObject LLKanLessenZienTrue;

    // Add this with other SerializeField declarations at the top
    [SerializeField] private List<TMP_InputField> studentLesInputFields;
    [SerializeField] private GameObject Leerlingbekijktles;

    [SerializeField] private GameObject KanNietKopieren;
    [SerializeField] private GameObject KanNietRoostermaken;

    [SerializeField] private GameObject dagOverzicht;  // Add this line near other SerializeField declarations

    // Add this field near the top of the class with other SerializeFields
    [SerializeField] private GameObject timeIndicator;
    [SerializeField] private GameObject timeIndicator2;

    // Add this near the top with other SerializeField declarations
    [SerializeField] private List<GameObject> currentDayIndicator = new List<GameObject>();

    private void Start()
    {
        selectedWeek = 0;
        UnityServices.InitializeAsync();
        AnalyticsService.Instance.StartDataCollection();
        string currentSceneName = "teststartscene";
        CustomEvent myEvent = new CustomEvent("teststartscene")
        {
            { "param_test", currentSceneName }
        };
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();

        instance = this;
        
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
        
        lesPool = new List<GameObject>();
        foreach (Transform child in lesPoolParent.transform)
        {
            lesPool.Add(child.gameObject);
        }

        currentWeek = System.Globalization.ISOWeek.GetWeekOfYear(System.DateTime.Now);
        
        UpdateWeekDisplay();

        // Add this to initialize the GameObject's visibility
        if (LLKanLessenZienTrue != null)
        {
            LLKanLessenZienTrue.SetActive(PlayerPrefs.GetInt("LLKanLessenZien") == 1);
        }
        //LoadLessen(true);

        // Check if dagOverzicht should be active
        //UpdateDagOverzichtVisibility();
    }

    private void UpdateWeekDisplay()
    {
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);

        //System.DateTime now = System.DateTime.Now;
        //System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + 1);
        //monday = monday.AddDays(7 * selectedWeek);

        System.DateTime sunday = monday.AddDays(6);

        string monthFormat = monday.Month == sunday.Month ? "" : " MMM";
        string startDate = monday.ToString("dd" + monthFormat);
        string endDate = sunday.ToString("dd MMM");

        weekDateText.text = $"{startDate} - {endDate}";
    }

    public void NextWeek()
    {
        selectedWeek++;
        UpdateWeekDisplay();

        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);
        int weekNum = ISOWeek.GetWeekOfYear(monday);
        weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;


        Debug.Log($"Moving to next week. SelectedWeek offset: {selectedWeek}, Target week number: {weekNum}");
        LoadLessen();
    }

    public void PreviousWeek()
    {
        selectedWeek--;
        UpdateWeekDisplay();


        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);
        int weekNum = ISOWeek.GetWeekOfYear(monday);
        weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;


        Debug.Log($"Moving to previous week. SelectedWeek offset: {selectedWeek}, Target week number: {weekNum}");
        LoadLessen();
    }

    public void LoadKiesLeerlingButtons(Leerling student)
    {
        // First hide all buttons
        foreach (GameObject obj in kiesleerlingButtons)
        {
            obj.SetActive(false);
        }

        if (RijschoolApp.instance?.selectedRijschool?.leerlingen == null || 
            RijschoolApp.instance.selectedRijschool.leerlingen.Count == 0)
        {
            //noStudentsMessage.SetActive(true);
            return;
        }
        if (kiesleerlingButtons.Count > 0)
        {
            GameObject button = kiesleerlingButtons[0];
            button.SetActive(true);

            Image image = button.GetComponent<Image>();
            image.color = RijschoolApp.instance.leerlingKleuren[student.colorIndex];

            TextMeshProUGUI naamtext = button.GetComponentInChildren<TextMeshProUGUI>();
            naamtext.text = student.naam;
        }
    }

    public void LoadAllKiesLeerlingButtons()
    {
        // First hide all buttons
        foreach (GameObject obj in kiesleerlingButtons)
        {
            obj.SetActive(false);
        }
        foreach (GameObject obj in kiesleerlingButtons2)
        {
            obj.SetActive(false);
        }

        // Get the list of students from the selected driving school
        var rijschool = RijschoolApp.instance?.selectedRijschool;
        if (rijschool?.leerlingen == null || rijschool.leerlingen.Count == 0)
        {
            //noStudentsMessage.SetActive(true);
            return;
        }
        for (int i = 0; i < rijschool.leerlingen.Count && i < kiesleerlingButtons.Count; i++)
        {
            var student = rijschool.leerlingen[i];
            GameObject button = kiesleerlingButtons[i];
            button.SetActive(true);

            // Set button color based on student's color index
            Image image = button.GetComponent<Image>();
            image.color = RijschoolApp.instance.leerlingKleuren[student.colorIndex];

            // Set student name
            TextMeshProUGUI naamtext = button.GetComponentInChildren<TextMeshProUGUI>();
            naamtext.text = student.naam;
        }
        for (int i = 0; i < rijschool.leerlingen.Count && i < kiesleerlingButtons2.Count; i++)
        {
            var student = rijschool.leerlingen[i];
            GameObject button = kiesleerlingButtons2[i];
            button.SetActive(true);

            // Set button color based on student's color index
            Image image = button.GetComponent<Image>();
            image.color = RijschoolApp.instance.leerlingKleuren[student.colorIndex];

            // Set student name
            TextMeshProUGUI naamtext = button.GetComponentInChildren<TextMeshProUGUI>();
            naamtext.text = student.naam;
        }
    }

    public void SetLeerlingForManualLesson(int leerling)
    {
        var rijschool = RijschoolApp.instance?.selectedRijschool;
        if (rijschool?.leerlingen == null || rijschool.leerlingen.Count == 0)
        {
            //noStudentsMessage.SetActive(true);
            return;
        }
        manualLessonLeerling = leerling;
        for (int i = 0; i < rijschool.leerlingen.Count && i < kiesleerlingButtons2.Count; i++)
        {
            var student = rijschool.leerlingen[i];
            GameObject button = kiesleerlingButtons2[i];
            button.SetActive(true);
            button.transform.GetChild(1).gameObject.SetActive(leerling != i);
            button.transform.GetChild(2).gameObject.SetActive(leerling == i);
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

        //Debug.Log($"Switching to {(isInstructor ? "Instructor" : "Student")} view");
        
        if (!isInstructor && RijschoolApp.instance.selectedLeerling != null)
        {
            //Debug.Log($"Selected student: {RijschoolApp.instance.selectedLeerling.naam}");
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
    }

    public void LoadLessen(bool loadAvailability = true)
    {
        //print("0");

        // Get current week's Monday
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);
        int year = monday.Year;  // Add this line to define the year variable

        // Update the date texts for each day
        if (dagDatumTexts != null && dagDatumTexts.Count == 7)
        {
            dagDatumTexts[0].text = "Ma\n" + monday.AddDays(0).ToString("dd MMM").TrimEnd('.'); // Format: "24 Jan"
            dagDatumTexts[1].text = monday.AddDays(1).ToString("Di\ndd MMM").TrimEnd('.'); // Format: "24 Jan"
            dagDatumTexts[2].text = monday.AddDays(2).ToString("Woe\ndd MMM").TrimEnd('.'); // Format: "24 Jan"
            dagDatumTexts[3].text = monday.AddDays(3).ToString("Do\ndd MMM").TrimEnd('.'); // Format: "24 Jan"
            dagDatumTexts[4].text = monday.AddDays(4).ToString("Vrij\ndd MMM").TrimEnd('.'); // Format: "24 Jan"
            dagDatumTexts[5].text = monday.AddDays(5).ToString("Za\ndd MMM").TrimEnd('.'); // Format: "24 Jan"
            dagDatumTexts[6].text = monday.AddDays(6).ToString("Zo\ndd MMM").TrimEnd('.'); // Format: "24 Jan"

            // Add the week offset text
            if (weekOffsetText != null)
            {
                if (selectedWeek == 0)
                {
                    weekOffsetText.text = "Deze week";
                }
                else
                {
                    int absWeeks = Math.Abs(selectedWeek);
                    weekOffsetText.text = $"{absWeeks} {(absWeeks == 1 ? "week" : "weken")} {(selectedWeek > 0 ? "van nu" : "geleden")}";
                }
            }
        }
        else
        {
            Debug.LogWarning("Date texts not properly set up. Ensure there are exactly 7 TextMeshProUGUI components assigned.");
        }

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
        if (rijschool?.rooster?.weken == null)
        {
            print("returning"); return;
        }

        // Get current week info
        //System.DateTime now = System.DateTime.Now;
        //System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + 1);
        //monday = monday.AddDays(7 * selectedWeek);
        int weekNum = System.Globalization.ISOWeek.GetWeekOfYear(monday);
        weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;

        //print("Weeknum: " + weekNum);

        // Find the week in the rooster
        Week targetWeek = rijschool.rooster.weken.FirstOrDefault(w => w.weekNummer == weekNum);
        bool hasAvailableTimeslots = false;
        var availabilityList = roosterInstructor ?
            rijschool?.instructeurBeschikbaarheid :
            RijschoolApp.instance.selectedLeerling?.beschikbaarheid;

        if (availabilityList != null)
        {
            hasAvailableTimeslots = availabilityList.Any(b =>
                b.weekNummer == weekNum &&
                b.jaar == year &&
                b.tijdslots != null &&
                b.tijdslots.Count > 0);
        }

        // Check conditions for KanNietKopieren and KanNietRoostermaken
        if (KanNietKopieren != null)
        {
            bool hasNoLessons = targetWeek?.lessen == null || targetWeek.lessen.Count == 0;
            KanNietKopieren.SetActive(hasNoLessons && !hasAvailableTimeslots);
        }

        if (KanNietRoostermaken != null)
        {
            KanNietRoostermaken.SetActive(!hasAvailableTimeslots);
        }




        if (targetWeek?.lessen == null)
        {
            print("returning"); return;
        }

        // Get the next available pool index
        int poolIndex = GetNextAvailablePoolIndex();

        //print("1");
        foreach (var les in targetWeek.lessen)
        {
            //print("2");
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
                //print("3");

                GameObject lesObject = lesPool[poolIndex];
                poolIndex++;

                lesObject.transform.SetParent(lessenParents[dayIndex]);  // Using new parent

                var (yPos, height) = CalculateTimeSlotTransform(les.begintijd, les.eindtijd);
                
                RectTransform rectTransform = lesObject.GetComponent<RectTransform>();

                float newLessonWidth = lessenParents[dayIndex].parent.parent.GetComponent<RectTransform>().rect.width;
                rectTransform.sizeDelta = new Vector2(newLessonWidth, height);
                rectTransform.localPosition = new Vector3((newLessonWidth/2f), yPos, 0);

                //RectTransform text1 = lesObject.transform.GetChild(0).GetComponent<RectTransform>();
                //RectTransform text2 = lesObject.transform.GetChild(1).GetComponent<RectTransform>();
                //text1.sizeDelta = new Vector2(text1.sizeDelta.x, rectTransform.sizeDelta.y / 2);
                //text2.sizeDelta = new Vector2(text2.sizeDelta.x, rectTransform.sizeDelta.y / 2);


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
                //print("Setting " + lesObject.name + " active");
                lesObject.SetActive(true);
                
                //Debug.Log($"Displayed lesson: {les.begintijd} - {les.eindtijd} for {les.leerlingNaam} on day {dayIndex}");
            }
        }

        UpdateNextLeerlingRoosterButtonsVisibility();

        // Update week offset text at the end of the method
        UpdateWeekOffsetText();
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
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);

        int weekNum = System.Globalization.ISOWeek.GetWeekOfYear(monday);
        weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;

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
            //Debug.Log("Using instructor availability list");
        }
        else if (RijschoolApp.instance.selectedLeerling != null)
        {
            availabilityList = RijschoolApp.instance.selectedLeerling.beschikbaarheid ?? new List<Beschikbaarheid>();
            userType = "Student";
            userName = RijschoolApp.instance.selectedLeerling.naam;
            //Debug.Log($"Using student availability list for {userName}");
        }
        else
        {
            //Debug.LogWarning("No valid user type selected");
            return;
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
                        //Debug.LogWarning("Not enough lesson objects in pool!");
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
                    
                    float xwidth = lesObject.transform.parent.parent.parent.GetComponent<RectTransform>().rect.width;
                    rectTransform.sizeDelta = new Vector2(xwidth, height);
                    rectTransform.localPosition = new Vector3((xwidth/2), yPos, 0);
                    //RectTransform text1 = lesObject.transform.GetChild(0).GetComponent<RectTransform>();
                    //RectTransform text2 = lesObject.transform.GetChild(1).GetComponent<RectTransform>();
                    //text1.sizeDelta = new Vector2(text1.sizeDelta.x, rectTransform.sizeDelta.y / 2);
                    //text2.sizeDelta = new Vector2(text2.sizeDelta.x, rectTransform.sizeDelta.y / 2);

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
                    //Debug.Log($"Created availability slot for {userType} {userName} on {dagNaam}: {tijdslot.startTijd} - {tijdslot.eindTijd}");
                }
            }
        }

        // After displaying availability slots, load lessons on top if needed
        if (roosterInstructor && loadLessons)
        {
            LoadLessen(false); // Pass false to prevent recursive call
        }

        UpdateNextLeerlingRoosterButtonsVisibility();

        // Update week offset text at the end of the method
        UpdateWeekOffsetText();

        // After displaying availability slots, check if we should show lessons for students
        if (!roosterInstructor && 
            RijschoolApp.instance?.selectedRijschool?.LLzienLessen == true && 
            RijschoolApp.instance.selectedLeerling != null)
        {
            var targetWeek = RijschoolApp.instance.selectedRijschool.rooster?.weken?
                .FirstOrDefault(w => w.weekNummer == weekNum && w.jaar == year);

            if (targetWeek?.lessen != null)
            {
                string currentStudentName = RijschoolApp.instance.selectedLeerling.naam;

                // Filter lessons for the current student
                var studentLessons = targetWeek.lessen
                    .Where(l => l.leerlingNaam == currentStudentName ||
                               (l.gereserveerdDoorLeerling?.Any(gl => gl.naam == currentStudentName) ?? false))
                    .ToList();

                // Get the next available pool index
                poolIndex = GetNextAvailablePoolIndex();

                foreach (var les in studentLessons)
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

                    if (dayIndex >= 0 && dayIndex < lessenParents.Count)
                    {
                        GameObject lesObject = lesPool[poolIndex];
                        poolIndex++;

                        // Set parent to correct lessons parent for this day
                        lesObject.transform.SetParent(lessenParents[dayIndex]);

                        // Calculate position and size
                        var (yPos, height) = CalculateTimeSlotTransform(les.begintijd, les.eindtijd);

                        // Set local position and size
                        RectTransform rectTransform = lesObject.GetComponent<RectTransform>();
                        

                        // Calculate the new width based on the grandparent's width
                        float newLessonWidth = lessenParents[dayIndex].parent.parent.parent.GetComponent<RectTransform>().rect.width;
                        rectTransform.sizeDelta = new Vector2(newLessonWidth, height);
                        rectTransform.localPosition = new Vector3((newLessonWidth/2), yPos, 0);
                        //RectTransform text1 = lesObject.transform.GetChild(0).GetComponent<RectTransform>();
                        //RectTransform text2 = lesObject.transform.GetChild(1).GetComponent<RectTransform>();
                        //text1.sizeDelta = new Vector2(text1.sizeDelta.x, rectTransform.sizeDelta.y / 2);
                        //text2.sizeDelta = new Vector2(text2.sizeDelta.x, rectTransform.sizeDelta.y / 2);


                        // Set a different color for student-visible lessons
                        Image lesImage = lesObject.GetComponent<Image>();
                        lesImage.color = new Color(0.4f, 0.6f, 0.8f, 0.8f); // Light blue, semi-transparent

                        // Update text components
                        TextMeshProUGUI[] texts = lesObject.GetComponentsInChildren<TextMeshProUGUI>();
                        foreach (var text in texts)
                        {
                            if (text != null)
                            {
                                text.color = LESSON_TEXT_COLOR;
                                text.fontStyle = FontStyles.Bold;
                            }
                        }

                        texts[0].text = $"{les.begintijd} - {les.eindtijd}";
                        texts[1].text = les.leerlingNaam ?? "";
                        //texts[1].text = les.notities ?? "";

                        // Add click event listener
                        Button button = lesObject.GetComponent<Button>();
                        if (button != null)
                        {
                            button.onClick.RemoveAllListeners();
                            Les currentLes = les;
                            button.onClick.AddListener(() => OnLesSelected(currentLes));
                        }

                        lesObject.SetActive(true);
                    }
                }
            }
        }

        UpdateNextLeerlingRoosterButtonsVisibility();

        // Update week offset text at the end of the method
        UpdateWeekOffsetText();
    }

    private void OnAvailabilitySlotClicked(int dayIndex, string startTime, string endTime)
    {
        lesGeselecteerdText.text = "Verwijder Beschikbaarheid";

        selectedDay = dayIndex;
        string selectedDag = GetDayName(dayIndex);

        //Debug.Log($"OnAvailabilitySlotClicked - User type: {(roosterInstructor ? "Instructor" : "Student")}");

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
            //Debug.Log($"Showing instructor selection for slot: {selectedDag} {startTime} - {endTime}");
            instructeurSelecteertLes.SetActive(true);
            instructorLes.SetActive(true);
            studentLes.SetActive(false);

            // Update text to show it's an availability slot
            TextMeshProUGUI[] instructorTexts = instructorLes.GetComponentsInChildren<TextMeshProUGUI>();
            // instructorTexts[0].text = $"{startTime} - {endTime}";  // Commented out as requested
            instructorTexts[1].text = "Instructeur beschikbaar";

            // Set input field values
            //TextMeshProUGUI NotitiesInput = instructorLes.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            TMP_InputField startTimeInput = instructorLes.transform.GetChild(3).GetComponent<TMP_InputField>();
            TMP_InputField endTimeInput = instructorLes.transform.GetChild(4).GetComponent<TMP_InputField>();
            //NotitiesInput.text = "";
            startTimeInput.text = startTime;
            endTimeInput.text = endTime;
        }
        else if (RijschoolApp.instance.selectedLeerling != null)
        {
            //Debug.Log($"Showing student selection for slot: {selectedDag} {startTime} - {endTime}");
            leerlingSelecteertLes.SetActive(true);
            instructorLes.SetActive(false);
            studentLes.SetActive(true);

            // Update text to show it's an availability slot
            TextMeshProUGUI[] studentTexts = studentLes.GetComponentsInChildren<TextMeshProUGUI>();
            // studentTexts[0].text = $"{startTime} - {endTime}";  // Commented out as requested
            studentTexts[1].text = $"{RijschoolApp.instance.selectedLeerling.naam} beschikbaar";

            // Set input field values
            TMP_InputField startTimeInput = studentLes.transform.GetChild(3).GetComponent<TMP_InputField>();
            TMP_InputField endTimeInput = studentLes.transform.GetChild(4).GetComponent<TMP_InputField>();
            startTimeInput.text = startTime;
            endTimeInput.text = endTime;

            // Make all student input fields interactable when viewing an available timeslot
            foreach (var inputField in studentLesInputFields)
            {
                if (inputField != null)
                {
                    inputField.interactable = true;
                }
            }
            Leerlingbekijktles.SetActive(false);
        }

        //Debug.Log($"Selected availability slot: {selectedDag} {startTime} - {endTime} for {(roosterInstructor ? "Instructor" : RijschoolApp.instance.selectedLeerling?.naam)}");
    }

    public void OnLesSelected(Les les)
    {
        selectedLes = les;
        selectedTimeSlot = null;  // Clear any selected time slot

        lesGeselecteerdText.text = "Verwijder Les";

        instructeurSelecteertLes.SetActive(false);
        leerlingSelecteertLes.SetActive(false);

        GameObject instructorLes = LeraarLesLeerlingLes[0];
        GameObject studentLes = LeraarLesLeerlingLes[1];

        if (selectedLes != null)
        {
            foreach (GameObject detailLes in new[] { instructorLes, studentLes })
            {
                TextMeshProUGUI[] texts = detailLes.GetComponentsInChildren<TextMeshProUGUI>();
                texts[1].text = selectedLes.leerlingNaam ?? "";

                // Get the input fields
                TMP_InputField NotitiesInput = detailLes.transform.GetChild(2).GetComponent<TMP_InputField>();
                TMP_InputField startTimeInput = detailLes.transform.GetChild(3).GetComponent<TMP_InputField>();
                TMP_InputField endTimeInput = detailLes.transform.GetChild(4).GetComponent<TMP_InputField>();

                // Set the input field values
                NotitiesInput.text = selectedLes.notities ?? "";
                startTimeInput.text = selectedLes.begintijd;
                endTimeInput.text = selectedLes.eindtijd;
            }

            if (roosterInstructor)
            {
                instructeurSelecteertLes.SetActive(true);
                instructorLes.SetActive(true);
                studentLes.SetActive(false);

                verwijderReserving.GetComponent<Button>().onClick.RemoveAllListeners();
                verwijderReserving.GetComponent<Button>().onClick.AddListener(() => VerwijderLes(selectedDay));
            }
            else
            {
                leerlingSelecteertLes.SetActive(true);
                instructorLes.SetActive(false);
                studentLes.SetActive(true);

                // Make all student input fields not interactable when viewing a lesson
                foreach (var inputField in studentLesInputFields)
                {
                    if (inputField != null)
                    {
                        inputField.interactable = false;
                    }
                }
                Leerlingbekijktles.SetActive(true);

            }
        }
        else
        {
            GameObject clickedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            if (clickedObject != null)
            {
                TextMeshProUGUI[] texts = clickedObject.GetComponentsInChildren<TextMeshProUGUI>();
                string[] times = texts[0].text.Split('-');
                string startTime = times[0].Trim();
                string endTime = times[1].Trim();
                string selectedDag = GetDayName(selectedDay);

                if (roosterInstructor)
                {
                    instructeurSelecteertLes.SetActive(true);
                    instructorLes.SetActive(true);
                    studentLes.SetActive(false);

                    verwijderReserving.GetComponent<Button>().onClick.RemoveAllListeners();
                    verwijderReserving.GetComponent<Button>().onClick.AddListener(DeleteAvailabilityTimeSlot);
                }
                else if (RijschoolApp.instance.selectedLeerling != null)
                {
                    leerlingSelecteertLes.SetActive(true);
                    instructorLes.SetActive(false);
                    studentLes.SetActive(true);

                    // Make all student input fields interactable when viewing an available timeslot
                    foreach (var inputField in studentLesInputFields)
                    {
                        if (inputField != null)
                        {
                            inputField.interactable = true;
                        }
                    }
                    Leerlingbekijktles.SetActive(false);
                }

                selectedTimeSlot = new TimeSlotInfo
                {
                    startTime = startTime,
                    endTime = endTime,
                    day = selectedDag
                };
            }
        }

        UpdateMapsButton();
    }

    private class TimeSlotInfo
    {
        public string startTime;
        public string endTime;
        public string day;
    }

    private TimeSlotInfo selectedTimeSlot;

    public async void DeleteAvailabilityTimeSlot()
    {
        if (selectedLes != null)
        {
            await VerwijderLes(selectedDay);
            return;
        }

        if (selectedTimeSlot == null) return;

        var rijschool = RijschoolApp.instance.selectedRijschool;
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);

        int weekNum = ISOWeek.GetWeekOfYear(monday);
        weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;

        int year = monday.Year;

        if (roosterInstructor)
        {
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

        await RijschoolApp.instance.UpdateRijschool(rijschool);
        RefreshDisplay();
        
        selectedTimeSlot = null;
        instructeurSelecteertLes.SetActive(false);
        leerlingSelecteertLes.SetActive(false);

        UpdateMapsButton(); // Add this line
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
                //Debug.Log($"Les verwijderd uit week {selectedLes.weekNummer}");
                selectedLes = null;
                
                // Save to server
                await RijschoolApp.instance.UpdateRijschool(RijschoolApp.instance.selectedRijschool);
                
                // Refresh the display
                RefreshDisplay();
            }
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

        string leerlingNaam;
        if (leerling < 0) // Student removing their own reservation
        {
            if (RijschoolApp.instance.selectedLeerling != null)
            {
                leerlingNaam = RijschoolApp.instance.selectedLeerling.naam;
                selectedLes.gereserveerdDoorLeerling.RemoveAll(l => 
                    l.naam == RijschoolApp.instance.selectedLeerling.naam);
            }
        }
        else // Instructor removing a student's reservation
        {
            if (leerling < RijschoolApp.instance.selectedRijschool.leerlingen.Count)
            {
                leerlingNaam = RijschoolApp.instance.selectedRijschool.leerlingen[leerling].naam;
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

            //TextMeshProUGUI naamtext = leerlingObj.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            //naamtext.text = listleerling[i].naam;

            TextMeshProUGUI frequentietext = (i == 0) ? leerlingObj.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>() : leerlingObj.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();
            frequentietext.text = listleerling[i].frequentie.ToString();

            // Update woonplaats input field
            if (i < leerlingoverzichtWoonplaats.Count)
            {
                leerlingoverzichtWoonplaats[i].text = listleerling[i].woonPlaats ?? "";
            }
            if (i < leerlingoverzichtAdres.Count)
            {
                leerlingoverzichtAdres[i].text = listleerling[i].adres ?? "";
            }

            //Image image
            Image image = leerlingoverzicht[i].GetComponent<Image>();
            image.color = colors[listleerling[i].colorIndex];

            // Get child GameObjects
            Transform leerlingTransform = leerlingObj.transform;
            //GameObject plus = leerlingTransform.GetChild(1).gameObject;
            //GameObject min = leerlingTransform.GetChild(2).gameObject;

            // Update password text
            TextMeshProUGUI passwordText = (i == 0) ? leerlingObj.transform.GetChild(8).GetComponent<TextMeshProUGUI>() : leerlingTransform.GetChild(7).GetComponent<TextMeshProUGUI>();
            passwordText.text = "ww: " + listleerling[i].wachtwoord ?? "";

            // Update name input field
            if (i < leerlingoverzichtnaam.Count)
            {
                leerlingoverzichtnaam[i].text = listleerling[i].naam;
            }
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
        //Debug.Log("Starting schedule generation...");

        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);

        int weekNum = ISOWeek.GetWeekOfYear(monday);
        weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;

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
                //Debug.Log($"Added instructor slot: {beschikbaarheid.dag} {slot.startTijd}-{slot.eindTijd}");
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
            //Debug.Log($"Student {student.naam}: Needs {student.frequentie} lessons, {student.minutesPerLes} minutes each");
            var availability = studentSlots[student.naam];
            foreach (var slot in availability)
            {
                //Debug.Log($"- Available: {slot.Day} {slot.StartTime}-{slot.EndTime}");
            }
        }

        // Get preferences for woonplaats scheduling
        bool startInWoonplaats = PlayerPrefs.GetInt("StartInWoonplaats") == 1;
        bool endInWoonplaats = PlayerPrefs.GetInt("EindInWoonplaats") == 1;

        // Get the pause time between lessons
        int pauzeTussenLessen = PlayerPrefs.GetInt("PauzeTussenLessen", 0);

        // Process each instructor slot
        foreach (var instructorSlot in instructorSlots)
        {
            var slotStartMinutes = TimeStringToMinutes(instructorSlot.StartTime);
            var slotEndMinutes = TimeStringToMinutes(instructorSlot.EndTime);
            var currentTimeInSlot = slotStartMinutes;
            bool isFirstLessonOfDay = true;

            while (currentTimeInSlot < slotEndMinutes)
            {
                // Find eligible students for this time
                var eligibleStudents = studentSlots
                    .Where(kvp => {
                        var student = kvp.Key;
                        var slots = kvp.Value;
                        // Check if student still needs lessons
                        if (assignedLessonsPerStudent[student] >= studentRequirements[student].frequency)
                        {
                            //Debug.Log($"- Rejected: Already has enough lessons");
                            return false;
                        }

                        // Check if student already has a lesson this day
                        if (lessonsPerStudentPerDay[student].Contains(instructorSlot.Day))
                        {
                            //Debug.Log($"- Rejected: Already has lesson today");
                            return false;
                        }

                        var lessonDuration = studentRequirements[student].minutes;
                        if (currentTimeInSlot + lessonDuration > slotEndMinutes)
                        {
                            //Debug.Log($"- Rejected: Lesson wouldn't fit in remaining time");
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

                        //Debug.Log($"- Available at this time: {isAvailable}");
                        return isAvailable;
                    })
                    .OrderByDescending(s => {
                        var remaining = studentRequirements[s.Key].frequency - assignedLessonsPerStudent[s.Key];
                        //Debug.Log($"Student {s.Key} has {remaining} lessons remaining");
                        return remaining;
                    })
                    .ToList();

                // Apply woonplaats preferences for first and last lessons
                if (eligibleStudents.Any())
                {
                    var orderedStudents = eligibleStudents.ToList();

                    // For first lesson of the day
                    if (isFirstLessonOfDay && startInWoonplaats)
                    {
                        // Check if we're within first 30 minutes of instructor's availability
                        if (currentTimeInSlot <= slotStartMinutes + 30)
                        {
                            // Get the list of preferred start locations from PlayerPrefs
                            string startLocations = PlayerPrefs.GetString("StartWoonplaatsen", "");
                            string[] preferredStartLocations = !string.IsNullOrEmpty(startLocations) 
                                ? startLocations.Split(',').Select(s => s.Trim()).ToArray() 
                                : new string[0];

                            // Prioritize students from preferred start locations
                            orderedStudents = orderedStudents
                                .OrderByDescending(s => {
                                    var student = s.Key;
                                    var studentWoonplaats = rijschool.leerlingen
                                        .First(l => l.naam == student).woonPlaats;
                                    
                                    // Check if student's woonplaats is in the preferred locations list
                                    var matchesPreferredLocation = !string.IsNullOrEmpty(studentWoonplaats) && 
                                        preferredStartLocations.Any(loc => 
                                            studentWoonplaats.Equals(loc, StringComparison.OrdinalIgnoreCase));
                                    
                                    var remaining = studentRequirements[student].frequency - assignedLessonsPerStudent[student];
                                    return (matchesPreferredLocation ? 1000 : 0) + remaining;
                                })
                                .ToList();
                        }
                    }
                    // For last possible lesson of the day
                    else if (endInWoonplaats)
                    {
                        // Check if this could be the last lesson (no room for another full lesson after)
                        var nextStudentMinDuration = orderedStudents
                            .Min(s => studentRequirements[s.Key].minutes);
                        bool isLastPossibleLesson = currentTimeInSlot + nextStudentMinDuration >= slotEndMinutes - 40;

                        if (isLastPossibleLesson)
                        {
                            // Get the list of preferred end locations from PlayerPrefs
                            string endLocations = PlayerPrefs.GetString("EindWoonplaatsen", "");
                            string[] preferredEndLocations = !string.IsNullOrEmpty(endLocations) 
                                ? endLocations.Split(',').Select(s => s.Trim()).ToArray() 
                                : new string[0];

                            // Prioritize students from preferred end locations
                            orderedStudents = orderedStudents
                                .OrderByDescending(s => {
                                    var student = s.Key;
                                    var studentWoonplaats = rijschool.leerlingen
                                        .First(l => l.naam == student).woonPlaats;
                                    
                                    // Check if student's woonplaats is in the preferred locations list
                                    var matchesPreferredLocation = !string.IsNullOrEmpty(studentWoonplaats) && 
                                        preferredEndLocations.Any(loc => 
                                            studentWoonplaats.Equals(loc, StringComparison.OrdinalIgnoreCase));
                                    
                                    var remaining = studentRequirements[student].frequency - assignedLessonsPerStudent[student];
                                    return (matchesPreferredLocation ? 1000 : 0) + remaining;
                                })
                                .ToList();
                        }
                    }

                    // Select the first eligible student after ordering
                    var selectedStudent = orderedStudents.First().Key;
                    var lessonDuration = studentRequirements[selectedStudent].minutes;

                    // Create and add the lesson
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
                    
                    currentTimeInSlot += lessonDuration + pauzeTussenLessen;
                    isFirstLessonOfDay = false;
                }
                else
                {
                    // If no eligible students found, move time forward
                    currentTimeInSlot += 15; // Move in 15-minute increments
                    isFirstLessonOfDay = false;
                }
            }
        }

        // After scheduling, log summary
        //Debug.Log("\nScheduling Summary:");
        foreach (var student in rijschool.leerlingen)
        {
            //Debug.Log($"{student.naam}: {assignedLessonsPerStudent[student.naam]} of {student.frequentie} lessons scheduled");
        }

        // Save the updated schedule
        await RijschoolApp.instance.UpdateRijschool(rijschool);
        print("loading lessons");
        LoadLessen();
        UnityAnalyticsManager.Instance.TrackScheduleGeneration(minimizeChanges, targetWeek?.lessen?.Count ?? 0);

        // After all lessons are scheduled, calculate and display statistics
        if (roosterStatistics != null)
        {
            // Get the text components
            var lessonsText = roosterStatistics.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var fillRateText = roosterStatistics.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            var unscheduledStudentsText = roosterStatistics.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

            // 1. Total lessons generated
            int totalLessons = targetWeek?.lessen?.Count ?? 0;
            lessonsText.text = $"Aantal lessen: {totalLessons}";

            // 2. Calculate instructor availability fill rate
            float totalAvailableMinutes = 0;
            float scheduledMinutes = 0;

            // Calculate total available minutes
            var instructorAvailability = rijschool.instructeurBeschikbaarheid
                .Where(b => b.weekNummer == weekNum && b.jaar == year);

            foreach (var dayAvail in instructorAvailability)
            {
                foreach (var slot in dayAvail.tijdslots)
                {
                    int startMinutes = TimeStringToMinutes(slot.startTijd);
                    int endMinutes = TimeStringToMinutes(slot.eindTijd);
                    totalAvailableMinutes += endMinutes - startMinutes;
                }
            }

            // Calculate scheduled minutes
            if (targetWeek?.lessen != null)
            {
                foreach (var les in targetWeek.lessen)
                {
                    int startMinutes = TimeStringToMinutes(les.begintijd);
                    int endMinutes = TimeStringToMinutes(les.eindtijd);
                    scheduledMinutes += endMinutes - startMinutes;
                }
            }

            float fillRate = totalAvailableMinutes > 0 ? 
                (scheduledMinutes / totalAvailableMinutes) * 100 : 0;
            fillRateText.text = $"Bezettingsgraad: {fillRate:F1}%";

            // 3. Calculate unscheduled eligible students
            int unscheduledStudents = 0;
            foreach (var student in rijschool.leerlingen)
            {
                // Check if student needs lessons and has availability this week
                if (student.frequentie > 0)
                {
                    bool hasAvailability = student.beschikbaarheid?
                        .Any(b => b.weekNummer == weekNum && 
                                 b.jaar == year && 
                                 b.tijdslots.Any()) ?? false;

                    if (hasAvailability)
                    {
                        // Check if student got any lessons
                        bool hasLesson = targetWeek?.lessen?
                            .Any(l => l.leerlingNaam == student.naam || 
                                     (l.gereserveerdDoorLeerling != null &&
                                      l.gereserveerdDoorLeerling
                                        .Any(gl => gl.naam == student.naam))) ?? false;

                        if (!hasLesson)
                        {
                            unscheduledStudents++;
                        }
                    }
                }
            }

            unscheduledStudentsText.text = $"Niet ingeplande studenten: {unscheduledStudents}";
            roosterStatistics.SetActive(true);
        }

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
            timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Ongeldige tijd, Gebruik hh-mm.";
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

        // Handle single number input (e.g., "8" or "9")
        if (int.TryParse(time, out int singleHour))
        {
            return singleHour >= 0 && singleHour <= 23;
        }

        // Handle inputs ending with ":" or "." (e.g., "8:" or "8.")
        if (time.EndsWith(":") || time.EndsWith("."))
        {
            string hourPart = time.Substring(0, time.Length - 1);
            if (int.TryParse(hourPart, out int hour))
            {
                return hour >= 0 && hour <= 23;
            }
            return false;
        }

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
        // Replace . with : for consistency
        time = time.Replace('.', ':');

        // Handle single number input (e.g., "8" or "9")
        if (int.TryParse(time, out int singleHour))
        {
            return $"{singleHour:D2}:00";
        }

        // Handle inputs ending with ":" or "." (e.g., "8:" or "8.")
        if (time.EndsWith(":") || time.EndsWith("."))
        {
            string hourPart = time.Substring(0, time.Length - 1);
            if (int.TryParse(hourPart, out int hour))
            {
                return $"{hour:D2}:00";
            }
        }

        // Handle regular time format
        string[] parts = time.Split(':');
        int hours = int.Parse(parts[0]);
        int minutes = parts.Length > 1 ? int.Parse(parts[1]) : 0;
        return $"{hours:D2}:{minutes:D2}";
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
            timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Ongeldige tijd, Gebruik hh-mm.";
            return;
        }

        //Debug.Log($"[SaveTimeSlot] Current roosterInstructor value: {roosterInstructor}");

        if (!ValidateTimeFormat(startTijdInput.text) || !ValidateTimeFormat(eindTijdInput.text))
        {
            invalidTimeFormatWarning.SetActive(true);
            timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Ongeldige tijd, Gebruik hh-mm.";
            return;
        }

        invalidTimeFormatWarning.SetActive(false);

        string formattedStartTijd = FormatTime(startTijdInput.text);
        string formattedEindTijd = FormatTime(eindTijdInput.text);
        string selectedDag = GetDayName(selectedDay);

        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);

        int weekNum = System.Globalization.ISOWeek.GetWeekOfYear(monday);
        weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;

        int year = monday.Year;

        var rijschool = RijschoolApp.instance.selectedRijschool;
        if (rijschool == null) return;

        // Check for overlapping timeslots
        List<Beschikbaarheid> availabilityList;
        if (roosterInstructor)
        {
            availabilityList = rijschool.instructeurBeschikbaarheid ?? new List<Beschikbaarheid>();
        }
        else if (RijschoolApp.instance.selectedLeerling != null)
        {
            availabilityList = RijschoolApp.instance.selectedLeerling.beschikbaarheid ?? new List<Beschikbaarheid>();
        }
        else
        {
            return;
        }

        // Find availability for this specific day AND week AND year
        var existingDayAvailability = availabilityList
            .FirstOrDefault(b => b.dag == selectedDag && 
                               b.weekNummer == weekNum && 
                               b.jaar == year);

        if (existingDayAvailability?.tijdslots != null)
        {
            // Convert new timeslot to minutes for comparison
            int newStartMinutes = TimeStringToMinutes(formattedStartTijd);
            int newEndMinutes = TimeStringToMinutes(formattedEindTijd);

            // Check for overlap with existing slots
            foreach (var existingSlot in existingDayAvailability.tijdslots)
            {
                int existingStartMinutes = TimeStringToMinutes(existingSlot.startTijd);
                int existingEndMinutes = TimeStringToMinutes(existingSlot.eindTijd);

                // Check if slots overlap
                if (!(newEndMinutes <= existingStartMinutes || newStartMinutes >= existingEndMinutes))
                {
                    timeFormatWarning.SetActive(true);
                    timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Tijdsloten overlappen";
                    return;
                }
            }
        }

        // Original save logic continues here...
        try 
        {
            if (roosterInstructor)
            {
                //Debug.Log("Saving instructor availability");
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
                UnityAnalyticsManager.Instance.TrackAvailabilityUpdate("instructor", dagBeschikbaarheid.tijdslots.Count);
            }
            else if (RijschoolApp.instance.selectedLeerling != null)
            {
                //Debug.Log($"Saving availability for student {RijschoolApp.instance.selectedLeerling.naam}");
                var leerling = RijschoolApp.instance.selectedLeerling;
                
                if (leerling.beschikbaarheid == null)
                {
                    leerling.beschikbaarheid = new List<Beschikbaarheid>();
                }

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
                UnityAnalyticsManager.Instance.TrackAvailabilityUpdate("student", dagBeschikbaarheid.tijdslots.Count);

                //Debug.Log($"Added timeslot for student: {formattedStartTijd} - {formattedEindTijd}");
            }

            // Save to server
            //Debug.Log("[SaveTimeSlot] Saving to server...");
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
        // Retrieve start and end times from PlayerPrefs
        string defaultStartTime = PlayerPrefs.GetString("RoosterStartTime", "06:00");
        string defaultEndTime = PlayerPrefs.GetString("RoosterEndTime", "22:00");

        // Convert times to minutes
        int startMinutes = TimeStringToMinutes(startTime);
        int endMinutes = TimeStringToMinutes(endTime);
        int defaultStartMinutes = TimeStringToMinutes(defaultStartTime);
        int defaultEndMinutes = TimeStringToMinutes(defaultEndTime);

        // Calculate total available minutes based on PlayerPrefs times
        int totalAvailableMinutes = defaultEndMinutes - defaultStartMinutes;

        // Calculate height (in pixels) based on the duration of the time slot
        float height = (endMinutes - startMinutes) / (float)totalAvailableMinutes * 1580f;

        // Calculate Y position based on PlayerPrefs start time
        float yPosition = -((startMinutes - defaultStartMinutes) / (float)totalAvailableMinutes * 1580f + height / 2f);

        // Debug statements to trace values
        //Debug.Log($"Start Time: {startTime} ({startMinutes} minutes)");
        //Debug.Log($"End Time: {endTime} ({endMinutes} minutes)");
        //Debug.Log($"Default Start Time: {defaultStartTime} ({defaultStartMinutes} minutes)");
        //Debug.Log($"Default End Time: {defaultEndTime} ({defaultEndMinutes} minutes)");
        //Debug.Log($"Total Available Minutes: {totalAvailableMinutes}");
        //Debug.Log($"Calculated Height: {height}");
        //Debug.Log($"Calculated Y Position: {yPosition}");

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
        //DateTime monday = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));

        // Adjust for selected week
        monday = monday.AddDays(7 * weekOffset);
        
        // Get the target day
        int dayOffset = GetDayValue(dayName);
        DateTime targetDate = monday.AddDays(dayOffset);
        
        return targetDate.ToString("dd-MM-yyyy");
    }

    public async void ResetWeekAvailability()
    {
        //Debug.Log("Resetting availability for the selected week");

        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);

        int weekNum = System.Globalization.ISOWeek.GetWeekOfYear(monday);
        weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;

        int year = monday.Year;

        var rijschool = RijschoolApp.instance.selectedRijschool;
        if (rijschool == null) return;

        if (roosterInstructor)
        {
            // Reset instructor availability
            rijschool.instructeurBeschikbaarheid.RemoveAll(b => 
                b.weekNummer == weekNum && 
                b.jaar == year);
            
            //Debug.Log($"Reset instructor availability for week {weekNum}");
        }
        else if (RijschoolApp.instance.selectedLeerling != null)
        {
            // Reset student availability
            var student = RijschoolApp.instance.selectedLeerling;
            student.beschikbaarheid.RemoveAll(b => 
                b.weekNummer == weekNum && 
                b.jaar == year);
            
            //Debug.Log($"Reset availability for student {student.naam} in week {weekNum}");
        }

        // Save changes and refresh display
        await RijschoolApp.instance.UpdateRijschool(rijschool);
        LoadLessen();
    }

    public async void ResetWeekLessons()
    {
        Debug.Log("Starting ResetWeekLessons...");

        print("Selectedweek: " + selectedWeek);
        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);

        int weekNum = ISOWeek.GetWeekOfYear(monday);
        weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;

        int year = monday.Year;

        Debug.Log($"Starting week reset - Current selectedWeek offset: {selectedWeek}");
        Debug.Log($"Calculated date: {monday:dd-MM-yyyy}");
        Debug.Log($"Target Week Number: {weekNum}, Year: {year}");

        var rijschool = RijschoolApp.instance?.selectedRijschool;
        if (rijschool?.rooster?.weken == null)
        {
            Debug.LogError("Rijschool, rooster, or weken is null");
            return;
        }

        // Create a new sorted list with all weeks
        List<Week> updatedWeeks = rijschool.rooster.weken
            .OrderBy(w => w.jaar)
            .ThenBy(w => w.weekNummer)
            .ToList();

        Debug.Log("All weeks in rooster before reset (sorted):");
        foreach (var week in updatedWeeks)
        {
            Debug.Log($"Week {week.weekNummer}, Year {week.jaar}: {week.lessen?.Count ?? 0} lessons");
        }

        // Find and update the target week
        var targetWeek = updatedWeeks
            .FirstOrDefault(w => w.weekNummer == weekNum && w.jaar == year);

        if (targetWeek == null)
        {
            Debug.Log($"No week found to reset for Week {weekNum}, Year {year}");
            return;
        }

        Debug.Log($"Found target week {weekNum} with {targetWeek.lessen?.Count ?? 0} lessons");

        // Remove the existing week
        updatedWeeks.Remove(targetWeek);

        // Create a new week with the same details but no lessons
        var newWeek = new Week
        {
            weekNummer = weekNum,
            jaar = year,
            lessen = new List<Les>()
        };

        // Add the updated week back to the list and sort again
        updatedWeeks.Add(newWeek);
        updatedWeeks = updatedWeeks
            .OrderBy(w => w.jaar)
            .ThenBy(w => w.weekNummer)
            .ToList();

        Debug.Log("All weeks in rooster after reset (sorted):");
        foreach (var week in updatedWeeks)
        {
            Debug.Log($"Week {week.weekNummer}, Year {week.jaar}: {week.lessen?.Count ?? 0} lessons");
        }

        try
        {
            // Update the rijschool's weeks list with the sorted list
            rijschool.rooster.weken = updatedWeeks;
            
            Debug.Log("Saving changes to server...");
            await RijschoolApp.instance.UpdateRijschool(rijschool);
            Debug.Log("Successfully saved changes to server");
            
            // Refresh the display
            LoadLessen();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving to server: {e.Message}\n{e.StackTrace}");
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
            //Debug.LogWarning($"Invalid student index: {studentIndex}");
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
            
            //Debug.Log($"Updated MinutesPerLes for student {studentIndex} to {minutes} minutes");
        }
        else
        {
            // If parsing failed, reset to default value (60)
            leerlingoverzichtMinutesPerLes[studentIndex].text = "60";
            RijschoolApp.instance.selectedRijschool.leerlingen[studentIndex].minutesPerLes = 60;
            //Debug.LogWarning($"Invalid input for MinutesPerLes. Reset to default value for student {studentIndex}");
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

    public async Task CopyInstructorAvailabilityToNextWeeks()
    {
        int weeksToGenerate = copyForXWeeks;
        
        var rijschool = RijschoolApp.instance?.selectedRijschool;
        if (rijschool?.instructeurBeschikbaarheid == null) return;

        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);
        int sourceWeekNum = ISOWeek.GetWeekOfYear(monday);
        int sourceYear = monday.Year;

        // Get source week's availability
        var sourceAvailability = rijschool.instructeurBeschikbaarheid
            .Where(b => b.weekNummer == sourceWeekNum && b.jaar == sourceYear)
            .ToList();

        if (!sourceAvailability.Any()) return;

        Debug.Log($"Source week {sourceWeekNum} has {sourceAvailability.Count} availability entries");

        for (int i = 1; i <= weeksToGenerate; i++)
        {
            monday = monday.AddDays(7);
            int targetWeekNum = ISOWeek.GetWeekOfYear(monday);
            int targetYear = monday.Year;

            // Remove any existing empty availability entries for the target week
            rijschool.instructeurBeschikbaarheid.RemoveAll(b => 
                b.weekNummer == targetWeekNum && 
                b.jaar == targetYear && 
                (b.tijdslots == null || b.tijdslots.Count == 0));

            // Check if target week has any non-empty availability
            bool weekHasAvailability = rijschool.instructeurBeschikbaarheid
                .Any(b => b.weekNummer == targetWeekNum && 
                         b.jaar == targetYear && 
                         b.tijdslots != null && 
                         b.tijdslots.Count > 0);

            Debug.Log($"Target week {targetWeekNum}: Has availability = {weekHasAvailability}");

            if (!weekHasAvailability)
            {
                foreach (var availability in sourceAvailability)
                {
                    if (availability.tijdslots != null && availability.tijdslots.Count > 0)
                    {
                        var newAvailability = new Beschikbaarheid
                        {
                            dag = availability.dag,
                            weekNummer = targetWeekNum,
                            jaar = targetYear,
                            tijdslots = availability.tijdslots.Select(t => new TimeSlot
                            {
                                startTijd = t.startTijd,
                                eindTijd = t.eindTijd
                            }).ToList()
                        };
                        rijschool.instructeurBeschikbaarheid.Add(newAvailability);
                        Debug.Log($"Added availability for day {availability.dag} in week {targetWeekNum}");
                    }
                }
            }
        }

        await RijschoolApp.instance.UpdateRijschool(rijschool);
        RefreshDisplay();
    }

    public async Task CopyStudentAvailabilityToNextWeeks()
    {
        int weeksToGenerate = copyForXWeeks;
        
        var student = RijschoolApp.instance?.selectedLeerling;
        if (student?.beschikbaarheid == null) return;

        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);
        int sourceWeekNum = ISOWeek.GetWeekOfYear(monday);
        int sourceYear = monday.Year;

        // Get source week's availability
        var sourceAvailability = student.beschikbaarheid
            .Where(b => b.weekNummer == sourceWeekNum && b.jaar == sourceYear)
            .ToList();

        if (!sourceAvailability.Any()) return;

        Debug.Log($"Source week {sourceWeekNum} has {sourceAvailability.Count} availability entries");

        for (int i = 1; i <= weeksToGenerate; i++)
        {
            monday = monday.AddDays(7);
            int targetWeekNum = ISOWeek.GetWeekOfYear(monday);
            int targetYear = monday.Year;

            // Remove any existing empty availability entries for the target week
            student.beschikbaarheid.RemoveAll(b => 
                b.weekNummer == targetWeekNum && 
                b.jaar == targetYear && 
                (b.tijdslots == null || b.tijdslots.Count == 0));

            // Check if target week has any non-empty availability
            bool weekHasAvailability = student.beschikbaarheid
                .Any(b => b.weekNummer == targetWeekNum && 
                         b.jaar == targetYear && 
                         b.tijdslots != null && 
                         b.tijdslots.Count > 0);

            Debug.Log($"Target week {targetWeekNum}: Has availability = {weekHasAvailability}");

            if (!weekHasAvailability)
            {
                foreach (var availability in sourceAvailability)
                {
                    if (availability.tijdslots != null && availability.tijdslots.Count > 0)
                    {
                        var newAvailability = new Beschikbaarheid
                        {
                            dag = availability.dag,
                            weekNummer = targetWeekNum,
                            jaar = targetYear,
                            tijdslots = availability.tijdslots.Select(t => new TimeSlot
                            {
                                startTijd = t.startTijd,
                                eindTijd = t.eindTijd
                            }).ToList()
                        };
                        student.beschikbaarheid.Add(newAvailability);
                        Debug.Log($"Added availability for day {availability.dag} in week {targetWeekNum}");
                    }
                }
            }
        }

        await RijschoolApp.instance.UpdateRijschool(RijschoolApp.instance.selectedRijschool);
        RefreshDisplay();
    }

    public async Task CopyLessonsToNextWeeks()
    {
        int weeksToGenerate = copyForXWeeks;
        
        var rijschool = RijschoolApp.instance?.selectedRijschool;
        if (rijschool == null) return;

        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);
        int sourceWeekNum = ISOWeek.GetWeekOfYear(monday);
        int sourceYear = monday.Year;

        // Initialize rooster if needed
        if (rijschool.rooster == null)
        {
            rijschool.rooster = new LesRooster();
        }

        // Get source week's lessons
        var sourceWeek = rijschool.rooster.weken
            .FirstOrDefault(w => w.weekNummer == sourceWeekNum && w.jaar == sourceYear);

        if (sourceWeek?.lessen == null || !sourceWeek.lessen.Any()) return;

        for (int i = 1; i <= weeksToGenerate; i++)
        {
            monday = monday.AddDays(7);
            int targetWeekNum = ISOWeek.GetWeekOfYear(monday);
            int targetYear = monday.Year;

            // Check if target week exists and has lessons
            var targetWeek = rijschool.rooster.weken
                .FirstOrDefault(w => w.weekNummer == targetWeekNum && w.jaar == targetYear);

            bool weekHasLessons = targetWeek?.lessen != null && targetWeek.lessen.Count > 0;

            if (!weekHasLessons)
            {
                // If week doesn't exist or is empty, create it
                if (targetWeek == null)
                {
                    targetWeek = new Week { weekNummer = targetWeekNum, jaar = targetYear };
                    rijschool.rooster.weken.Add(targetWeek);
                }

                targetWeek.lessen = sourceWeek.lessen.Select(l => new Les
                {
                    begintijd = l.begintijd,
                    eindtijd = l.eindtijd,
                    notities = l.notities,
                    leerlingId = l.leerlingId,
                    leerlingNaam = l.leerlingNaam,
                    isAutomatischGepland = l.isAutomatischGepland,
                    datum = monday.AddDays(GetDayOfWeekFromDate(l.datum)).ToString("dd-MM-yyyy"),
                    weekNummer = targetWeekNum,
                    gereserveerdDoorLeerling = l.gereserveerdDoorLeerling?.Select(ll => new Leerling
                    {
                        naam = ll.naam,
                        frequentie = ll.frequentie,
                        colorIndex = ll.colorIndex
                    }).ToList() ?? new List<Leerling>()
                }).ToList();
            }
        }

        await RijschoolApp.instance.UpdateRijschool(rijschool);
        LoadLessen();
    }

    // Helper method to get day of week from date string
    private int GetDayOfWeekFromDate(string dateString)
    {
        if (DateTime.TryParseExact(dateString, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime date))
        {
            return ((int)date.DayOfWeek + 6) % 7; // Convert to Monday = 0, Sunday = 6
        }
        return 0;
    }

    // Example button click handlers
    public void OnCopyInstructorAvailabilityClick()
    {
        _ = CopyInstructorAvailabilityToNextWeeks();
    }

    public void OnCopyStudentAvailabilityClick()
    {
        _ = CopyStudentAvailabilityToNextWeeks();
    }

    public void OnCopyLessonsClick()
    {
        _ = CopyLessonsToNextWeeks();
    }

    // First, let's create a helper method to check for overlaps
    private bool HasOverlappingTimeSlot(List<TimeSlot> existingSlots, string newStartTime, string newEndTime)
    {
        // Convert new timeslot to minutes for comparison
        int newStartMinutes = TimeStringToMinutes(newStartTime);
        int newEndMinutes = TimeStringToMinutes(newEndTime);

        // Check for overlap with existing slots
        foreach (var existingSlot in existingSlots)
        {
            int existingStartMinutes = TimeStringToMinutes(existingSlot.startTijd);
            int existingEndMinutes = TimeStringToMinutes(existingSlot.eindTijd);

            // Check if slots overlap
            if (!(newEndMinutes <= existingStartMinutes || newStartMinutes >= existingEndMinutes))
            {
                return true;
            }
        }
        return false;
    }

    public async void AddMorningAvailability()
    {
        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);

        int weekNum = ISOWeek.GetWeekOfYear(monday);
        weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;

        int year = monday.Year;
        string selectedDag = GetDayName(selectedDay);

        var rijschool = RijschoolApp.instance.selectedRijschool;
        if (rijschool == null) return;

        // Check for overlaps before adding
        List<TimeSlot> existingSlots;
        if (roosterInstructor)
        {
            var existingAvailability = rijschool.instructeurBeschikbaarheid?
                .FirstOrDefault(b => b.dag == selectedDag && 
                                   b.weekNummer == weekNum && 
                                   b.jaar == year);
            existingSlots = existingAvailability?.tijdslots ?? new List<TimeSlot>();
        }
        else if (RijschoolApp.instance.selectedLeerling != null)
        {
            var existingAvailability = RijschoolApp.instance.selectedLeerling.beschikbaarheid?
                .FirstOrDefault(b => b.dag == selectedDag && 
                                   b.weekNummer == weekNum && 
                                   b.jaar == year);
            existingSlots = existingAvailability?.tijdslots ?? new List<TimeSlot>();
        }
        else
        {
            return;
        }

        // Check for overlaps
        if (HasOverlappingTimeSlot(existingSlots, "08:00", "12:00"))
        {
            timeFormatWarning.SetActive(true);
            timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Tijdsloten overlappen";
            return;
        }

        // Original code continues here...
        if (roosterInstructor)
        {
            // Add instructor availability
            if (rijschool.instructeurBeschikbaarheid == null)
            {
                rijschool.instructeurBeschikbaarheid = new List<Beschikbaarheid>();
            }

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

            dagBeschikbaarheid.tijdslots.Add(new TimeSlot 
            { 
                startTijd = "08:00",
                eindTijd = "12:00"
            });
        }
        else if (RijschoolApp.instance.selectedLeerling != null)
        {
            // Add student availability
            var leerling = RijschoolApp.instance.selectedLeerling;
            if (leerling.beschikbaarheid == null)
            {
                leerling.beschikbaarheid = new List<Beschikbaarheid>();
            }

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

            dagBeschikbaarheid.tijdslots.Add(new TimeSlot 
            { 
                startTijd = "08:00",
                eindTijd = "12:00"
            });
        }

        // Save changes and refresh display
        await RijschoolApp.instance.UpdateRijschool(rijschool);
        DisplayAvailabilityTimeSlots();
        if (roosterInstructor)
        {
            LoadLessen();
        }
    }

    public async void AddAfternoonAvailability()
    {
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);

        int weekNum = ISOWeek.GetWeekOfYear(monday);
        weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;

        int year = monday.Year;
        string selectedDag = GetDayName(selectedDay);

        var rijschool = RijschoolApp.instance.selectedRijschool;
        if (rijschool == null) return;

        // Check for overlaps before adding
        List<TimeSlot> existingSlots;
        if (roosterInstructor)
        {
            var existingAvailability = rijschool.instructeurBeschikbaarheid?
                .FirstOrDefault(b => b.dag == selectedDag && 
                                   b.weekNummer == weekNum && 
                                   b.jaar == year);
            existingSlots = existingAvailability?.tijdslots ?? new List<TimeSlot>();
        }
        else if (RijschoolApp.instance.selectedLeerling != null)
        {
            var existingAvailability = RijschoolApp.instance.selectedLeerling.beschikbaarheid?
                .FirstOrDefault(b => b.dag == selectedDag && 
                                   b.weekNummer == weekNum && 
                                   b.jaar == year);
            existingSlots = existingAvailability?.tijdslots ?? new List<TimeSlot>();
        }
        else
        {
            return;
        }

        // Check for overlaps
        if (HasOverlappingTimeSlot(existingSlots, "12:00", "16:00"))
        {
            timeFormatWarning.SetActive(true);
            timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Tijdsloten overlappen";
            return;
        }

        // Original code continues here...
        if (roosterInstructor)
        {
            // Add instructor availability
            if (rijschool.instructeurBeschikbaarheid == null)
            {
                rijschool.instructeurBeschikbaarheid = new List<Beschikbaarheid>();
            }

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

            dagBeschikbaarheid.tijdslots.Add(new TimeSlot 
            { 
                startTijd = "12:00",
                eindTijd = "16:00"
            });
        }
        else if (RijschoolApp.instance.selectedLeerling != null)
        {
            // Add student availability
            var leerling = RijschoolApp.instance.selectedLeerling;
            if (leerling.beschikbaarheid == null)
            {
                leerling.beschikbaarheid = new List<Beschikbaarheid>();
            }

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

            dagBeschikbaarheid.tijdslots.Add(new TimeSlot 
            { 
                startTijd = "12:00",
                eindTijd = "16:00"
            });
        }

        // Save changes and refresh display
        await RijschoolApp.instance.UpdateRijschool(rijschool);
        DisplayAvailabilityTimeSlots();
        if (roosterInstructor)
        {
            LoadLessen();
        }
    }

    public async void AddFullDayAvailability()
    {
        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);

        int weekNum = ISOWeek.GetWeekOfYear(monday);
        weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;

        int year = monday.Year;
        string selectedDag = GetDayName(selectedDay);

        var rijschool = RijschoolApp.instance.selectedRijschool;
        if (rijschool == null) return;

        // Check for overlaps before adding
        List<TimeSlot> existingSlots;
        if (roosterInstructor)
        {
            var existingAvailability = rijschool.instructeurBeschikbaarheid?
                .FirstOrDefault(b => b.dag == selectedDag && 
                                   b.weekNummer == weekNum && 
                                   b.jaar == year);
            existingSlots = existingAvailability?.tijdslots ?? new List<TimeSlot>();
        }
        else if (RijschoolApp.instance.selectedLeerling != null)
        {
            var existingAvailability = RijschoolApp.instance.selectedLeerling.beschikbaarheid?
                .FirstOrDefault(b => b.dag == selectedDag && 
                                   b.weekNummer == weekNum && 
                                   b.jaar == year);
            existingSlots = existingAvailability?.tijdslots ?? new List<TimeSlot>();
        }
        else
        {
            return;
        }

        // Check for overlaps
        if (HasOverlappingTimeSlot(existingSlots, PlayerPrefs.GetString("RoosterStartTime", "06:00"), PlayerPrefs.GetString("RoosterEndTime", "22:00")))
        {
            timeFormatWarning.SetActive(true);
            timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Tijdsloten overlappen";
            return;
        }

        // Original code continues here...
        if (roosterInstructor)
        {
            // Add instructor availability
            if (rijschool.instructeurBeschikbaarheid == null)
            {
                rijschool.instructeurBeschikbaarheid = new List<Beschikbaarheid>();
            }

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

            dagBeschikbaarheid.tijdslots.Add(new TimeSlot 
            { 
                startTijd = PlayerPrefs.GetString("RoosterStartTime", "06:00"),
                eindTijd = PlayerPrefs.GetString("RoosterEndTime", "22:00")
        });
        }
        else if (RijschoolApp.instance.selectedLeerling != null)
        {
            // Add student availability
            var leerling = RijschoolApp.instance.selectedLeerling;
            if (leerling.beschikbaarheid == null)
            {
                leerling.beschikbaarheid = new List<Beschikbaarheid>();
            }

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

            dagBeschikbaarheid.tijdslots.Add(new TimeSlot 
            { 
                startTijd = PlayerPrefs.GetString("RoosterStartTime", "06:00"),
                eindTijd = PlayerPrefs.GetString("RoosterEndTime", "22:00")
            });
        }

        // Save changes and refresh display
        await RijschoolApp.instance.UpdateRijschool(rijschool);
        DisplayAvailabilityTimeSlots();
        if (roosterInstructor)
        {
            LoadLessen();
        }
    }

    // Add this method to handle the visibility logic
    private void UpdateNextLeerlingRoosterButtonsVisibility()
    {
        bool shouldShowButtons = false;

        // Check if we're in student view (not instructor view)
        if (!roosterInstructor)
        {
            // Check if we have a selected student and MijnRijschool preference exists
            if (RijschoolApp.instance.selectedLeerling != null && 
                PlayerPrefs.HasKey("MijnRijschool"))
            {
                shouldShowButtons = true;
            }
        }

        var rijschool = RijschoolApp.instance?.selectedRijschool;
        if (!shouldShowButtons || rijschool?.leerlingen == null || rijschool.leerlingen.Count == 0)
        {
            // Hide all buttons if we shouldn't show them or if there are no students
            foreach (var button in nextLeerlingRoosterButtons)
            {
                if (button != null)
                {
                    button.SetActive(false);
                }
            }
            return;
        }

        // Handle button visibility based on student count
        if (nextLeerlingRoosterButtons.Count >= 2)
        {
            switch (rijschool.leerlingen.Count)
            {
                case 1:
                    // If only one student, hide both buttons
                    nextLeerlingRoosterButtons[0].SetActive(false);
                    nextLeerlingRoosterButtons[1].SetActive(false);
                    return;
                case 2:
                    // If two students, show only next button
                    nextLeerlingRoosterButtons[0].SetActive(true);
                    nextLeerlingRoosterButtons[1].SetActive(false);
                    break;
                default:
                    // Three or more students, show both buttons
                    nextLeerlingRoosterButtons[0].SetActive(true);
                    nextLeerlingRoosterButtons[1].SetActive(true);
                    break;
            }

            // Find current student index
            int currentIndex = rijschool.leerlingen.FindIndex(l => 
                l.naam == RijschoolApp.instance.selectedLeerling?.naam);

            // Calculate next and previous indices with wrapping
            int nextIndex = (currentIndex + 1) % rijschool.leerlingen.Count;
            int previousIndex = currentIndex - 1;
            if (previousIndex < 0) previousIndex = rijschool.leerlingen.Count - 1;

            // Update next student button text
            TextMeshProUGUI nextStudentText = nextLeerlingRoosterButtons[0].GetComponentInChildren<TextMeshProUGUI>();
            if (nextStudentText != null)
            {
                nextStudentText.text = rijschool.leerlingen[nextIndex].naam;
            }

            // Update previous student button text if we have more than 2 students
            if (rijschool.leerlingen.Count > 2)
            {
                TextMeshProUGUI prevStudentText = nextLeerlingRoosterButtons[1].GetComponentInChildren<TextMeshProUGUI>();
                if (prevStudentText != null)
                {
                    prevStudentText.text = rijschool.leerlingen[previousIndex].naam;
                }
            }
        }
    }

    public void LoadNextStudentSchedule()
    {
        var rijschool = RijschoolApp.instance?.selectedRijschool;
        if (rijschool?.leerlingen == null || rijschool.leerlingen.Count == 0) return;

        // Find current student index
        int currentIndex = rijschool.leerlingen.FindIndex(l => 
            l.naam == RijschoolApp.instance.selectedLeerling?.naam);

        // Calculate next index (wrap around to 0 if at end)
        int nextIndex = (currentIndex + 1) % rijschool.leerlingen.Count;

        // Select next student
        RijschoolApp.instance.selectedLeerling = rijschool.leerlingen[nextIndex];
        
        Debug.Log($"Switching to next student: {RijschoolApp.instance.selectedLeerling.naam}");

        // Update ingelogd als text
        ingelogdAlsText.text = $"Rooster van: {RijschoolApp.instance.selectedLeerling.naam}";

        // Refresh the display
        DisplayAvailabilityTimeSlots();
    }

    public void LoadPreviousStudentSchedule()
    {
        var rijschool = RijschoolApp.instance?.selectedRijschool;
        if (rijschool?.leerlingen == null || rijschool.leerlingen.Count == 0) return;

        // Find current student index
        int currentIndex = rijschool.leerlingen.FindIndex(l => 
            l.naam == RijschoolApp.instance.selectedLeerling?.naam);

        // Calculate previous index (wrap around to last if at beginning)
        int previousIndex = currentIndex - 1;
        if (previousIndex < 0) previousIndex = rijschool.leerlingen.Count - 1;

        // Select previous student
        RijschoolApp.instance.selectedLeerling = rijschool.leerlingen[previousIndex];
        
        Debug.Log($"Switching to previous student: {RijschoolApp.instance.selectedLeerling.naam}");

        // Update ingelogd als text
        ingelogdAlsText.text = $"Rooster van: {RijschoolApp.instance.selectedLeerling.naam}";

        // Refresh the display
        DisplayAvailabilityTimeSlots();
    }

    // Add this helper method to update the week text
    private void UpdateWeekOffsetText()
    {
        if (weekOffsetText != null)
        {
            if (selectedWeek == 0)
            {
                weekOffsetText.text = "Deze week";
            }
            else
            {
                int absWeeks = Math.Abs(selectedWeek);
                weekOffsetText.text = $"{absWeeks} {(absWeeks == 1 ? "week" : "weken")} {(selectedWeek > 0 ? "van nu" : "geleden")}";
            }
        }
    }

    // Update these methods to include week text updates
    //public void NextWeek()
    //{
    //    DateTime now = DateTime.Now;
    //    DateTime currentMonday = now.AddDays(-(int)now.DayOfWeek + 1);
    //    DateTime targetMonday = currentMonday.AddDays(7 * (selectedWeek + 1));
        
    //    // Calculate the actual week difference from now
    //    TimeSpan weekDiff = targetMonday - currentMonday;
    //    selectedWeek = (int)(weekDiff.Days / 7);
        
    //    Debug.Log($"Moving to next week. New selectedWeek: {selectedWeek}, " +
    //              $"Actual week number: {ISOWeek.GetWeekOfYear(targetMonday)}");
        
    //    LoadLessen();
    //}

    //public void PreviousWeek()
    //{
    //    DateTime now = DateTime.Now;
    //    DateTime currentMonday = now.AddDays(-(int)now.DayOfWeek + 1);
    //    DateTime targetMonday = currentMonday.AddDays(7 * (selectedWeek - 1));
        
    //    // Calculate the actual week difference from now
    //    TimeSpan weekDiff = targetMonday - currentMonday;
    //    selectedWeek = (int)(weekDiff.Days / 7);
        
    //    Debug.Log($"Moving to previous week. New selectedWeek: {selectedWeek}, " +
    //              $"Actual week number: {ISOWeek.GetWeekOfYear(targetMonday)}");
        
    //    LoadLessen();
    //}

    public async void ModifyLesStartTime(TMP_InputField startTimeInput)
    {
        if (selectedLes == null) return;

        // Validate time format
        if (!ValidateTimeFormat(startTimeInput.text))
        {
            invalidTimeFormatWarning.SetActive(true);
            timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Ongeldige tijd, Gebruik hh-mm.";
            startTimeInput.text = selectedLes.begintijd; // Reset to original time
            return;
        }

        string formattedStartTime = FormatTime(startTimeInput.text);
        
        // Validate time order
        if (!ValidateTimeOrder(formattedStartTime, selectedLes.eindtijd))
        {
            invalidTimeFormatWarning.SetActive(true);
            timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Starttijd moet voor eindtijd liggen";
            startTimeInput.text = selectedLes.begintijd; // Reset to original time
            return;
        }

        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);

        int weekNum = ISOWeek.GetWeekOfYear(monday);
        weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;

        int year = monday.Year;

        // Check for overlapping lessons
        var rijschool = RijschoolApp.instance.selectedRijschool;
        if (rijschool?.rooster?.weken == null) return;

        var targetWeek = rijschool.rooster.weken
            .FirstOrDefault(w => w.weekNummer == weekNum && w.jaar == year);
        
        if (targetWeek?.lessen != null)
        {
            foreach (var les in targetWeek.lessen)
            {
                // Skip checking against the same lesson
                if (les == selectedLes) continue;

                // Check if lesson is on the same day
                if (les.datum == selectedLes.datum)
                {
                    // Convert times to minutes for comparison
                    int newStartMinutes = TimeStringToMinutes(formattedStartTime);
                    int newEndMinutes = TimeStringToMinutes(selectedLes.eindtijd);
                    int existingStartMinutes = TimeStringToMinutes(les.begintijd);
                    int existingEndMinutes = TimeStringToMinutes(les.eindtijd);

                    // Check for overlap
                    if (!(newEndMinutes <= existingStartMinutes || newStartMinutes >= existingEndMinutes))
                    {
                        invalidTimeFormatWarning.SetActive(true);
                        timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Tijdsloten overlappen";
                        startTimeInput.text = selectedLes.begintijd; // Reset to original time
                        return;
                    }
                }
            }
        }

        // If all validations pass, update the lesson
        selectedLes.begintijd = formattedStartTime;
        invalidTimeFormatWarning.SetActive(false);

        // Save changes to server
        await RijschoolApp.instance.UpdateRijschool(rijschool);
        LoadLessen();
    }

    public async void ModifyLesEndTime(TMP_InputField endTimeInput)
    {
        if (selectedLes == null) return;

        // Validate time format
        if (!ValidateTimeFormat(endTimeInput.text))
        {
            invalidTimeFormatWarning.SetActive(true);
            timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Ongeldige tijd, Gebruik hh-mm.";
            endTimeInput.text = selectedLes.eindtijd; // Reset to original time
            return;
        }

        string formattedEndTime = FormatTime(endTimeInput.text);
        
        // Validate time order
        if (!ValidateTimeOrder(selectedLes.begintijd, formattedEndTime))
        {
            invalidTimeFormatWarning.SetActive(true);
            timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Eindtijd moet na starttijd liggen";
            endTimeInput.text = selectedLes.eindtijd; // Reset to original time
            return;
        }

        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);

        int weekNum = ISOWeek.GetWeekOfYear(monday);
        weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;

        int year = monday.Year;

        // Check for overlapping lessons
        var rijschool = RijschoolApp.instance.selectedRijschool;
        if (rijschool?.rooster?.weken == null) return;

        var targetWeek = rijschool.rooster.weken
            .FirstOrDefault(w => w.weekNummer == weekNum && w.jaar == year);
        
        if (targetWeek?.lessen != null)
        {
            foreach (var les in targetWeek.lessen)
            {
                // Skip checking against the same lesson
                if (les == selectedLes) continue;

                // Check if lesson is on the same day
                if (les.datum == selectedLes.datum)
                {
                    // Convert times to minutes for comparison
                    int newStartMinutes = TimeStringToMinutes(selectedLes.begintijd);
                    int newEndMinutes = TimeStringToMinutes(formattedEndTime);
                    int existingStartMinutes = TimeStringToMinutes(les.begintijd);
                    int existingEndMinutes = TimeStringToMinutes(les.eindtijd);

                    // Check for overlap
                    if (!(newEndMinutes <= existingStartMinutes || newStartMinutes >= existingEndMinutes))
                    {
                        invalidTimeFormatWarning.SetActive(true);
                        timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Tijdsloten overlappen";
                        endTimeInput.text = selectedLes.eindtijd; // Reset to original time
                        return;
                    }
                }
            }
        }

        // If all validations pass, update the lesson
        selectedLes.eindtijd = formattedEndTime;
        invalidTimeFormatWarning.SetActive(false);

        // Save changes to server
        await RijschoolApp.instance.UpdateRijschool(rijschool);
        LoadLessen();
    }

    public async void ModifyLesNotities(TMP_InputField notitiesInput)
    {
        if (selectedLes == null) return;

        string formattedEndTime = notitiesInput.text;

        var rijschool = RijschoolApp.instance.selectedRijschool;

        // If all validations pass, update the lesson
        selectedLes.notities = formattedEndTime;
        invalidTimeFormatWarning.SetActive(false);

        // Save changes to server
        await RijschoolApp.instance.UpdateRijschool(rijschool);
        LoadLessen();
    }

    public async void ConfirmLesTimeModification()
    {
        GameObject activePanel = roosterInstructor ? LeraarLesLeerlingLes[0] : LeraarLesLeerlingLes[1];
        
        // Get input fields
        TMP_InputField NotitiesInput = activePanel.transform.GetChild(2).GetComponent<TMP_InputField>();
        TMP_InputField startTimeInput = activePanel.transform.GetChild(3).GetComponent<TMP_InputField>();
        TMP_InputField endTimeInput = activePanel.transform.GetChild(4).GetComponent<TMP_InputField>();

        // Validate time formats
        if (!ValidateTimeFormat(startTimeInput.text) || !ValidateTimeFormat(endTimeInput.text))
        {
            Debug.LogWarning("Invalid time format detected");
            invalidTimeFormatWarning.SetActive(true);
            timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Ongeldige tijd. Gebruik formaat: uu:mm";
            return;
        }

        string formattedStartTime = FormatTime(startTimeInput.text);
        string formattedEndTime = FormatTime(endTimeInput.text);

        // Validate time order
        if (!ValidateTimeOrder(formattedStartTime, formattedEndTime))
        {
            Debug.LogWarning("End time is before or equal to start time");
            invalidTimeFormatWarning.SetActive(true);
            timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Ongeldige tijden";
            return;
        }

        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);

        int weekNum = ISOWeek.GetWeekOfYear(monday);
        weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;

        int year = monday.Year;
        Debug.Log($"Checking for week {weekNum} of year {year}");

        var rijschool = RijschoolApp.instance.selectedRijschool;
        if (rijschool == null) return;

        // Check if we're modifying a lesson or an availability slot
        if (selectedLes != null)
        {
            Debug.Log("Modifying an existing lesson");
            // Check for overlapping lessons
            if (rijschool.rooster?.weken != null)
            {
                var targetWeek = rijschool.rooster.weken
                    .FirstOrDefault(w => w.weekNummer == weekNum && w.jaar == year);
                
                if (targetWeek?.lessen != null)
                {
                    Debug.Log($"Found {targetWeek.lessen.Count} lessons in target week");
                    foreach (var les in targetWeek.lessen)
                    {
                        if (les == selectedLes) continue;

                        if (les.datum == selectedLes.datum)
                        {
                            Debug.Log($"Checking overlap with lesson on same day: {les.begintijd}-{les.eindtijd}");
                            int newStartMinutes = TimeStringToMinutes(formattedStartTime);
                            int newEndMinutes = TimeStringToMinutes(formattedEndTime);
                            int existingStartMinutes = TimeStringToMinutes(les.begintijd);
                            int existingEndMinutes = TimeStringToMinutes(les.eindtijd);

                            if (!(newEndMinutes <= existingStartMinutes || newStartMinutes >= existingEndMinutes))
                            {
                                Debug.LogWarning("Overlap detected with existing lesson");
                                invalidTimeFormatWarning.SetActive(true);
                                timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Tijdsloten overlappen";
                                GameObject detailPanel = roosterInstructor ? LeraarLesLeerlingLes[0] : LeraarLesLeerlingLes[1];
                                detailPanel.transform.parent.gameObject.SetActive(true);
                                return;
                            }
                        }
                    }
                }
            }

            // Update the lesson times
            selectedLes.begintijd = formattedStartTime;
            selectedLes.eindtijd = formattedEndTime;
            selectedLes.notities = NotitiesInput.text.ToString();
        }
        else if (selectedTimeSlot != null)
        {
            Debug.Log("Modifying an availability slot");
            List<Beschikbaarheid> availabilityList;
            
            if (roosterInstructor)
            {
                availabilityList = rijschool.instructeurBeschikbaarheid ?? new List<Beschikbaarheid>();
            }
            else if (RijschoolApp.instance.selectedLeerling != null)
            {
                availabilityList = RijschoolApp.instance.selectedLeerling.beschikbaarheid ?? new List<Beschikbaarheid>();
            }
            else
            {
                Debug.LogError("No valid user type for availability modification");
                return;
            }

            // Find the specific day's availability
            var dayAvailability = availabilityList
                .FirstOrDefault(b => b.dag == selectedTimeSlot.day && 
                                   b.weekNummer == weekNum && 
                                   b.jaar == year);

            if (dayAvailability != null)
            {
                // Find and update the specific timeslot
                var slot = dayAvailability.tijdslots
                    .FirstOrDefault(t => t.startTijd == selectedTimeSlot.startTime && 
                                       t.eindTijd == selectedTimeSlot.endTime);

                if (slot != null)
                {
                    // Check for overlaps with other slots
                    bool hasOverlap = dayAvailability.tijdslots
                        .Where(t => t != slot)
                        .Any(t => {
                            int newStartMinutes = TimeStringToMinutes(formattedStartTime);
                            int newEndMinutes = TimeStringToMinutes(formattedEndTime);
                            int existingStartMinutes = TimeStringToMinutes(t.startTijd);
                            int existingEndMinutes = TimeStringToMinutes(t.eindTijd);
                            return !(newEndMinutes <= existingStartMinutes || newStartMinutes >= existingEndMinutes);
                        });

                    if (hasOverlap)
                    {
                        Debug.LogWarning("Overlap detected with existing availability slot");
                        invalidTimeFormatWarning.SetActive(true);
                        timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Tijdsloten overlappen";
                        GameObject detailPanel = roosterInstructor ? LeraarLesLeerlingLes[0] : LeraarLesLeerlingLes[1];
                        detailPanel.transform.parent.gameObject.SetActive(true);
                        return;
                    }

                    // Update the timeslot
                    slot.startTijd = formattedStartTime;
                    slot.eindTijd = formattedEndTime;
                }
            }
        }
        else
        {
            Debug.LogWarning("No lesson or timeslot selected");
            return;
        }

        invalidTimeFormatWarning.SetActive(false);

        // Save changes to server
        //Debug.Log("Saving changes to server");
        await RijschoolApp.instance.UpdateRijschool(rijschool);
        //Debug.Log("Changes saved, refreshing display");
        GameObject activePanelnew = roosterInstructor ? LeraarLesLeerlingLes[0] : LeraarLesLeerlingLes[1];
        activePanelnew.transform.parent.gameObject.SetActive(false);
        RefreshDisplay();

        // Show the buttons after successful modification
        Buttons.SetActive(true);
    }

    public void FormatTimeInputField(TMP_InputField inputField)
    {
        if (string.IsNullOrEmpty(inputField.text)) return;

        if (ValidateTimeFormat(inputField.text))
        {
            inputField.text = FormatTime(inputField.text);
        }
        else
        {
            // If it's a single number (like "8"), assume it's hours and add ":00"
            if (int.TryParse(inputField.text, out int hours) && hours >= 0 && hours <= 23)
            {
                inputField.text = $"{hours:D2}:00";
            }
            else
            {
                // Invalid format, show warning
                invalidTimeFormatWarning.SetActive(true);
                timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Ongeldige tijd. Gebruik formaat: uu:mm";
            }
        }
    }

    public async void OnWoonplaatsChanged(int studentIndex)
    {
        // Validate input
        if (studentIndex < 0 || 
            studentIndex >= leerlingoverzichtWoonplaats.Count || 
            RijschoolApp.instance.selectedRijschool?.leerlingen == null || 
            studentIndex >= RijschoolApp.instance.selectedRijschool.leerlingen.Count)
        {
            //Debug.LogWarning($"Invalid student index: {studentIndex}");
            return;
        }

        // Get the input field value
        string woonplaats = leerlingoverzichtWoonplaats[studentIndex].text;
        
        // Update the student's woonplaats
        RijschoolApp.instance.selectedRijschool.leerlingen[studentIndex].woonPlaats = woonplaats;
        
        // Save changes to server
        await RijschoolApp.instance.UpdateRijschool(RijschoolApp.instance.selectedRijschool);
        
        //Debug.Log($"Updated woonplaats for student {studentIndex} to {woonplaats}");
    }

    public async void OnAdresChanged(int studentIndex)
    {
        // Validate input
        if (studentIndex < 0 ||
            studentIndex >= leerlingoverzichtAdres.Count ||
            RijschoolApp.instance.selectedRijschool?.leerlingen == null ||
            studentIndex >= RijschoolApp.instance.selectedRijschool.leerlingen.Count)
        {
            //Debug.LogWarning($"Invalid student index: {studentIndex}");
            return;
        }

        // Get the input field value
        string adres = leerlingoverzichtAdres[studentIndex].text;

        // Update the student's woonplaats
        RijschoolApp.instance.selectedRijschool.leerlingen[studentIndex].adres = adres;

        // Save changes to server
        await RijschoolApp.instance.UpdateRijschool(RijschoolApp.instance.selectedRijschool);

        //Debug.Log($"Updated woonplaats for student {studentIndex} to {woonplaats}");
    }

    public async void OnLeerlingNameChanged(int studentIndex)
    {
        if (RijschoolApp.instance.selectedRijschool == null || 
            studentIndex >= RijschoolApp.instance.selectedRijschool.leerlingen.Count ||
            studentIndex >= leerlingoverzichtnaam.Count) return;

        string newName = leerlingoverzichtnaam[studentIndex].text;
        
        // Don't allow empty names
        if (string.IsNullOrWhiteSpace(newName))
        {
            leerlingNaamWaarschuwing.SetActive(true);
            leerlingNaamWaarschuwing.GetComponentInChildren<TextMeshProUGUI>().text = "Vul een geldige naam in";
            // Reset to original name
            leerlingoverzichtnaam[studentIndex].text = RijschoolApp.instance.selectedRijschool.leerlingen[studentIndex].naam;
            return;
        }

        // Check if name already exists (case insensitive)
        bool nameExists = RijschoolApp.instance.selectedRijschool.leerlingen
            .Where((l, i) => i != studentIndex) // Exclude current student
            .Any(l => l.naam.Equals(newName, StringComparison.OrdinalIgnoreCase));

        if (nameExists)
        {
            leerlingNaamWaarschuwing.SetActive(true);
            leerlingNaamWaarschuwing.GetComponentInChildren<TextMeshProUGUI>().text = "Naam wordt al gebruikt";
            // Reset to original name
            leerlingoverzichtnaam[studentIndex].text = RijschoolApp.instance.selectedRijschool.leerlingen[studentIndex].naam;
            return;
        }

        // Hide warning if we got this far
        leerlingNaamWaarschuwing.SetActive(false);

        // Update the student's name
        string oldName = RijschoolApp.instance.selectedRijschool.leerlingen[studentIndex].naam;
        RijschoolApp.instance.selectedRijschool.leerlingen[studentIndex].naam = newName;

        // Update any lessons that reference this student
        if (RijschoolApp.instance.selectedRijschool.rooster?.weken != null)
        {
            foreach (Week week in RijschoolApp.instance.selectedRijschool.rooster.weken)
            {
                if (week.lessen != null)
                {
                    // Update main student name in lessons
                    foreach (Les les in week.lessen.Where(l => l.leerlingNaam == oldName))
                    {
                        les.leerlingNaam = newName;
                    }

                    // Update name in group reservations
                    foreach (Les les in week.lessen)
                    {
                        if (les.gereserveerdDoorLeerling != null)
                        {
                            var student = les.gereserveerdDoorLeerling.FirstOrDefault(l => l.naam == oldName);
                            if (student != null)
                            {
                                student.naam = newName;
                            }
                        }
                    }
                }
            }
        }
            
            // Save changes to server
            await RijschoolApp.instance.UpdateRijschool(RijschoolApp.instance.selectedRijschool);
        LoadLessen();
    }

    //public void ModifyCopyWeeks(bool increase)
    //{
    //    if (copyForXWeeks == null) return;
        
    //    // Parse current value
    //    if (int.TryParse(copyForXWeeks.text, out int currentWeeks))
    //    {
    //        // Increase or decrease, ensuring value stays positive
    //        int newWeeks = increase ? currentWeeks + 1 : Math.Max(1, currentWeeks - 1);
    //        copyForXWeeks.text = newWeeks.ToString();
    //    }
    //    else
    //    {
    //        // If parsing fails, reset to default value
    //        copyForXWeeks.text = "4";
    //    }
    //}

    // Add this function
    public void UpdateSelectedDayText(int dayIndex)
    {
        // Get current date info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);

        // Get the target date
        DateTime targetDate = monday.AddDays(dayIndex);
        
        // Dutch day names
        string[] dutchDayNames = {
            "Maandag", "Dinsdag", "Woensdag", "Donderdag", 
            "Vrijdag", "Zaterdag", "Zondag"
        };
        
        // Dutch month names
        string[] dutchMonthNames = {
            "Januari", "Februari", "Maart", "April", "Mei", "Juni",
            "Juli", "Augustus", "September", "Oktober", "November", "December"
        };
        
        // Format the date string
        string formattedDate = $"{dutchDayNames[dayIndex]} {targetDate.Day} {dutchMonthNames[targetDate.Month - 1]}";
        
        // Update the UI text
        if (selectedDayText != null)
        {
            selectedDayText.text = formattedDate;
            // Convert DayOfWeek to our array index (Monday = 0, Sunday = 6)
            int currentDayIndex = ((int)now.DayOfWeek + 6) % 7;
            selectedDayText2.text = $"{dutchDayNames[currentDayIndex]} {now.Day} {dutchMonthNames[now.Month - 1]}";
        }
    }

    // Add this new method
    public void SetCopyWeeks(string weeks)
    {
        if (int.TryParse(weeks, out int value))
        {
            copyForXWeeks = Math.Max(1, value); // Ensure at least 1 week
        }
    }

    // Find the method that handles lesson selection (likely SelectLes or similar)
    // and add this code to update the Maps button:
    private void UpdateMapsButton()
    {
        if (MapsExtensie != null)
        {
            bool shouldBeInteractable = selectedLes != null && 
                !string.IsNullOrWhiteSpace(RijschoolApp.instance?.selectedRijschool?.leerlingen
                    .FirstOrDefault(l => l.naam == selectedLes.leerlingNaam)?.adres);
            
            MapsExtensie.interactable = shouldBeInteractable;
        }
    }

    // Add this new method to handle opening Google Maps
    public void LoadAdresInMaps()
    {
        if (selectedLes == null) return;

        var student = RijschoolApp.instance?.selectedRijschool?.leerlingen
            .FirstOrDefault(l => l.naam == selectedLes.leerlingNaam);

        if (student == null || string.IsNullOrWhiteSpace(student.adres)) return;

        // Create search term
        string searchTerm = student.adres;
        
        // If woonplaats exists and isn't already part of the address, append it
        if (!string.IsNullOrWhiteSpace(student.woonPlaats) && 
            !searchTerm.Contains(student.woonPlaats, StringComparison.OrdinalIgnoreCase))
        {
            searchTerm += $", {student.woonPlaats}";
        }

        // Encode the address for URL
        string encodedAddress = UnityEngine.Networking.UnityWebRequest.EscapeURL(searchTerm);
        
        // Create Google Maps URL
        string url = $"https://www.google.com/maps/search/?api=1&query={encodedAddress}";
        
        // Open in default browser
        Application.OpenURL(url);
    }

    // Find the method that handles lesson selection (likely SelectLes)
    // and add the call to UpdateMapsButton():
    public void SelectLes(Les les)
    {
        selectedLes = les;
        selectedTimeSlot = null;
        
        // ... existing selection code ...

        UpdateMapsButton(); // Add this line
        
        // ... rest of existing code ...
    }

    // Also update the method that clears selection:
    public void ClearSelection()
    {
        selectedLes = null;
        selectedTimeSlot = null;
        
        // ... existing clear code ...

        UpdateMapsButton(); // Add this line
        
        // ... rest of existing code ...
    }

    // Add this new function
    public async void SetLLKanLessenZien(bool canSee)
    {
        PlayerPrefs.SetInt("LLKanLessenZien", canSee ? 1 : 0);
        PlayerPrefs.Save();
        
        if (LLKanLessenZienTrue != null)
        {
            LLKanLessenZienTrue.SetActive(canSee);
        }

        // Update the rijschool's llZienLessen property and sync with server
        var rijschool = RijschoolApp.instance?.selectedRijschool;
        if (rijschool != null)
        {
            rijschool.LLzienLessen = canSee;
            await RijschoolApp.instance.UpdateRijschool(rijschool);
        }
    }

    public async void SaveManualLes()
    {
        // Check if a student is selected
        if (manualLessonLeerling == -1)
        {
            invalidTimeFormatWarning.SetActive(true);
            timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Geen leerling geselecteerd";
            return;
        }

        // Check both format and time order
        if (!ValidateTimeOrder(startTijdInput.text, eindTijdInput.text))
        {
            invalidTimeFormatWarning.SetActive(true);
            timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Ongeldige tijd, Gebruik hh-mm.";
            return;
        }

        if (!ValidateTimeFormat(startTijdInput.text) || !ValidateTimeFormat(eindTijdInput.text))
        {
            invalidTimeFormatWarning.SetActive(true);
            timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Ongeldige tijd, Gebruik hh-mm.";
            return;
        }

        invalidTimeFormatWarning.SetActive(false);

        string formattedStartTijd = FormatTime(startTijdInput.text);
        string formattedEindTijd = FormatTime(eindTijdInput.text);
        string selectedDag = GetDayName(selectedDay);

        // Get current week info
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        monday = monday.AddDays(7 * selectedWeek);

        int weekNum = ISOWeek.GetWeekOfYear(monday);
        weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;

        int year = monday.Year;

        var rijschool = RijschoolApp.instance.selectedRijschool;
        if (rijschool == null) return;

        // Initialize rooster if needed
        if (rijschool.rooster == null)
        {
            rijschool.rooster = new LesRooster();
        }
        if (rijschool.rooster.weken == null)
        {
            rijschool.rooster.weken = new List<Week>();
        }

        // Find or create the week
        var targetWeek = rijschool.rooster.weken
            .FirstOrDefault(w => w.weekNummer == weekNum && w.jaar == year);

        if (targetWeek == null)
        {
            targetWeek = new Week { weekNummer = weekNum, jaar = year, lessen = new List<Les>() };
            rijschool.rooster.weken.Add(targetWeek);
        }
        if (targetWeek.lessen == null)
        {
            targetWeek.lessen = new List<Les>();
        }

        // Check for overlapping lessons
        int newStartMinutes = TimeStringToMinutes(formattedStartTijd);
        int newEndMinutes = TimeStringToMinutes(formattedEindTijd);
        string lesDate = GetDateForDayInWeek(selectedDag, selectedWeek);

        foreach (var existingLes in targetWeek.lessen)
        {
            if (existingLes.datum == lesDate)
            {
                int existingStartMinutes = TimeStringToMinutes(existingLes.begintijd);
                int existingEndMinutes = TimeStringToMinutes(existingLes.eindtijd);

                if (!(newEndMinutes <= existingStartMinutes || newStartMinutes >= existingEndMinutes))
                {
                    invalidTimeFormatWarning.SetActive(true);
                    timeFormatWarning.GetComponentInChildren<TextMeshProUGUI>().text = "Les overlapt met bestaande les";
                    return;
                }
            }
        }

        // Create the new lesson
        var newLes = new Les
        {
            begintijd = formattedStartTijd,
            eindtijd = formattedEindTijd,
            datum = lesDate,
            weekNummer = weekNum,
            isAutomatischGepland = false
        };

        // Add the student to the lesson
        if (manualLessonLeerling >= 0 && manualLessonLeerling < rijschool.leerlingen.Count)
        {
            var selectedStudent = rijschool.leerlingen[manualLessonLeerling];
            newLes.leerlingNaam = selectedStudent.naam;
            newLes.gereserveerdDoorLeerling = new List<Leerling>
            {
                new Leerling
                {
                    naam = selectedStudent.naam,
                    frequentie = selectedStudent.frequentie,
                    colorIndex = selectedStudent.colorIndex
                }
            };
        }

        // Add the lesson to the week
        targetWeek.lessen.Add(newLes);

        try
        {
            // Save to server
            await RijschoolApp.instance.UpdateRijschool(rijschool);

            // Clear input fields and close creation panel
            startTijdInput.text = "";
            eindTijdInput.text = "";
            //createLes.SetActive(false);

            // Refresh the display
            LoadLessen();
            SetLeerlingForManualLesson(-1);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManualLes] Error: {e.Message}\n{e.StackTrace}");
        }
    }

    public void UpdateDagOverzichtVisibility()
    {
        if (dagOverzicht == null)
        {
            print("dagoverzicht null");
            return;
        }

        var rijschool = RijschoolApp.instance?.selectedRijschool;
        if (rijschool == null) return;

        // Initialize rooster and weken if needed
        if (rijschool.rooster == null)
        {
            rijschool.rooster = new LesRooster();
        }
        if (rijschool.rooster.weken == null)
        {
            rijschool.rooster.weken = new List<Week>();
        }

        // Get current week info - REMOVED selectedWeek offset since we want actual today's lessons
        System.DateTime now = System.DateTime.Now;
        System.DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        int weekNum = ISOWeek.GetWeekOfYear(monday);
        weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;
        int year = monday.Year;
        string selectedDag = GetDayName(selectedDay);

        // Find current week's lessons
        var currentWeek = rijschool.rooster.weken
            .FirstOrDefault(w => w.weekNummer == weekNum && w.jaar == year);

        if (currentWeek?.lessen == null)
        {
            print("lessen null");
            dagOverzicht.SetActive(false);
            return;
        }

        // Get today's date in "dd-MM-yyyy" format
        string todayDate = now.ToString("dd-MM-yyyy");
        //print(todayDate);
        //print(currentWeek.weekNummer);
        //print("Week");

        // Debug print all lessons in week
        //print($"Total lessons in week: {currentWeek.lessen.Count}");
        //foreach (var les in currentWeek.lessen)
        //{
        //    //print($"Lesson date: {les.datum}, Start: {les.begintijd}, End: {les.eindtijd}");
        //    //// Add this debug line to check exact string comparison
        //    //print($"Date comparison: '{les.datum}' == '{todayDate}' : {les.datum == todayDate}");
        //}

        // Get all lessons for today
        var todaysLessons = currentWeek.lessen
            .Where(l => l.datum == todayDate)
            .OrderBy(l => TimeStringToMinutes(l.begintijd))
            .ToList();

        if (!todaysLessons.Any())
        {
            print("todayslessons null");
            dagOverzicht.SetActive(false);
            return;
        }

        // Get the last lesson of the day
        var lastLesson = todaysLessons.Last();
        print(lastLesson.leerlingNaam);
        print(lastLesson.begintijd);
        int lastLessonEndMinutes = TimeStringToMinutes(lastLesson.eindtijd);
        int currentTimeMinutes = now.Hour * 60 + now.Minute;

        // Set active if current time is before the end of the last lesson
        if(currentTimeMinutes < lastLessonEndMinutes+120)
        {
            dagOverzicht.SetActive(true);
            LoadDagOverzichtLessen(todaysLessons);
            UpdateSelectedDayText(0);
            RijschoolApp.instance.SetSchermActive(false, false, false, false);

        }
        else
        {
            dagOverzicht.SetActive(false);
        }
    }

    private void LoadDagOverzichtLessen(List<Les> lessonstoday) 
    {
        print(lessonstoday.Count);
        foreach(GameObject obj in dagOverzichtLessen)
        {
            obj.SetActive(false);
        }
        for(int i = 0; i<Math.Min(lessonstoday.Count,20); i++)
        {
            Les les = lessonstoday[i];
            //print(i);

            GameObject lesObject = dagOverzichtLessen[i];
            //print(lesObject.name);

            //Child gameobjects:
            //Image lesimage = lesObject.transform.GetChild(1).GetComponent<Image>();
            //lesImage.color = LESSON_BASE_COLOR;

            GameObject selectedIndicator = lesObject.transform.GetChild(0).gameObject;

            TextMeshProUGUI leerling = lesObject.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            leerling.text = les.leerlingNaam;

            TextMeshProUGUI starttijd = lesObject.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
            starttijd.text = les.begintijd;

            TextMeshProUGUI eindtijd = lesObject.transform.GetChild(4).GetComponent<TextMeshProUGUI>();
            eindtijd.text = les.eindtijd;

            TMP_InputField notities = lesObject.transform.GetChild(8).GetComponent<TMP_InputField>();
            notities.text = les.notities ?? "";

            // Add click event listener for maps button
            Button button = lesObject.transform.GetChild(7).GetComponent<Button>();
            bool shouldBeInteractable = les != null &&
                !string.IsNullOrWhiteSpace(RijschoolApp.instance?.selectedRijschool?.leerlingen
                .FirstOrDefault(l => l.naam == les.leerlingNaam)?.adres);
            button.interactable = shouldBeInteractable;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                var student = RijschoolApp.instance?.selectedRijschool?.leerlingen
                    .FirstOrDefault(l => l.naam == les.leerlingNaam);
                string searchTerm = student.adres;
                if (!string.IsNullOrWhiteSpace(student.woonPlaats) &&
                    !searchTerm.Contains(student.woonPlaats, StringComparison.OrdinalIgnoreCase))
                {
                    searchTerm += $", {student.woonPlaats}";
                }
                string encodedAddress = UnityEngine.Networking.UnityWebRequest.EscapeURL(searchTerm);
                string url = $"https://www.google.com/maps/search/?api=1&query={encodedAddress}";

                button.onClick.AddListener(() => Application.OpenURL(url));
            }

            // Add notes modification listener
            if (notities != null)
            {
                notities.onEndEdit.RemoveAllListeners();
                
                // Capture the current les and notities in the closure
                Les currentLes = les;
                TMP_InputField currentNotities = notities;
                
                notities.onEndEdit.AddListener(async (value) => {
                    currentLes.notities = value;
                    // Save changes to server
                    await RijschoolApp.instance.UpdateRijschool(RijschoolApp.instance.selectedRijschool);
                });
            }
            lesObject.SetActive(true);

            // Get current time in minutes
            DateTime now = DateTime.Now;
            int currentTimeMinutes = now.Hour * 60 + now.Minute;

            // Convert lesson times to minutes
            int lesStartMinutes = TimeStringToMinutes(les.begintijd);
            int lesEndMinutes = TimeStringToMinutes(les.eindtijd);

            // Set indicator active if current time falls within lesson time
            bool isCurrentLesson = currentTimeMinutes >= lesStartMinutes && currentTimeMinutes <= lesEndMinutes;
            bool isNextLesson = currentTimeMinutes < lesStartMinutes && 
                lessonstoday.Where(l => TimeStringToMinutes(l.begintijd) < lesStartMinutes)
                    .All(l => TimeStringToMinutes(l.eindtijd) < currentTimeMinutes);

            selectedIndicator.SetActive(isCurrentLesson || isNextLesson);

            // If this is the current lesson or the next upcoming lesson, center it in the scroll view
            if (isCurrentLesson || isNextLesson)
            {

                RectTransform scrollView = lesObject.transform.parent.GetComponent<RectTransform>();
                scrollView.localPosition = new Vector3(scrollView.localPosition.x, Mathf.Clamp((i-1),1,lessonstoday.Count-2)*450,scrollView.localPosition.z);
            }
        }

        // After handling all lessons, position the time indicator
        if (timeIndicator != null)
        {
            UpdateTimeIndicatorPosition(lessonstoday);
        }
    }

    // Add this new method
    private void UpdateTimeIndicatorPosition(List<Les> lessons)
    {
        DateTime now = DateTime.Now;
        int currentTimeMinutes = now.Hour * 60 + now.Minute;

        // Sort lessons by start time
        var sortedLessons = lessons.OrderBy(l => TimeStringToMinutes(l.begintijd)).ToList();

        for (int i = 0; i < sortedLessons.Count; i++)
        {
            Les currentLesson = sortedLessons[i];
            int lessonStartMinutes = TimeStringToMinutes(currentLesson.begintijd);
            int lessonEndMinutes = TimeStringToMinutes(currentLesson.eindtijd);

            if (currentTimeMinutes >= lessonStartMinutes && currentTimeMinutes <= lessonEndMinutes)
            {
                // Calculate position within lesson
                float timeProgress = (float)(currentTimeMinutes - lessonStartMinutes) / (lessonEndMinutes - lessonStartMinutes);
                float yOffset = timeProgress * 300;
                
                // Get the lesson GameObject's position
                GameObject lessonObj = dagOverzichtLessen[i];
                float basePosition = lessonObj.transform.position.y;

                // Set time indicator position
                timeIndicator.transform.localPosition = new Vector3(
                    timeIndicator.transform.localPosition.x,
                    basePosition - yOffset,
                    timeIndicator.transform.localPosition.z
                );
                return;
            }

            // Check if current time is between this lesson and the next one
            if (i < sortedLessons.Count - 1)
            {
                Les nextLesson = sortedLessons[i + 1];
                int nextLessonStartMinutes = TimeStringToMinutes(nextLesson.begintijd);

                if (currentTimeMinutes > lessonEndMinutes && currentTimeMinutes < nextLessonStartMinutes)
                {
                    // Calculate position within break
                    float breakProgress = (float)(currentTimeMinutes - lessonEndMinutes) / (nextLessonStartMinutes - lessonEndMinutes);
                    float yOffset = breakProgress * 350;

                    // Get the current lesson GameObject's position
                    GameObject lessonObj = dagOverzichtLessen[i];
                    float basePosition = lessonObj.transform.position.y;

                    // Set time indicator position
                    timeIndicator.transform.localPosition = new Vector3(
                        timeIndicator.transform.localPosition.x,
                        basePosition - 300 - yOffset,
                        timeIndicator.transform.localPosition.z
                    );
                    return;
                }
            }
        }

        timeIndicator.SetActive(false);
    }

    // Add this to ensure the time indicator updates regularly
    private void Update()
    {
        if (dagOverzicht != null && dagOverzicht.activeSelf && timeIndicator != null)
        {
            var rijschool = RijschoolApp.instance?.selectedRijschool;
            if (rijschool?.rooster?.weken != null)
            {
                // Get today's lessons
                DateTime now = DateTime.Now;
                string todayDate = now.ToString("dd-MM-yyyy");
                int weekNum = ISOWeek.GetWeekOfYear(now);
                weekNum = now.DayOfWeek == DayOfWeek.Sunday ? weekNum - 1 : weekNum;

                var currentWeek = rijschool.rooster.weken
                    .FirstOrDefault(w => w.weekNummer == weekNum && w.jaar == now.Year);

                if (currentWeek?.lessen != null)
                {
                    var todaysLessons = currentWeek.lessen
                        .Where(l => l.datum == todayDate)
                        .OrderBy(l => TimeStringToMinutes(l.begintijd))
                        .ToList();

                    if (todaysLessons.Any())
                    {
                        UpdateTimeIndicatorPosition(todaysLessons);
                    }
                }
            }
        }

        // New timeIndicator2 code
        if (timeIndicator2 != null)
        {
            // Get current week info
            DateTime now = DateTime.Now;
            DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
            int currentWeekNum = ISOWeek.GetWeekOfYear(monday);
            //currentWeekNum = now.DayOfWeek == DayOfWeek.Sunday ? currentWeekNum - 1 : currentWeekNum;

            // Get selected week info
            DateTime selectedMonday = monday.AddDays(7 * selectedWeek);
            int selectedWeekNum = ISOWeek.GetWeekOfYear(selectedMonday);
            selectedWeekNum = selectedMonday.DayOfWeek == DayOfWeek.Sunday ? selectedWeekNum - 1 : selectedWeekNum;

            // Check if we're in the current week
            if (currentWeekNum == selectedWeekNum && now.Year == selectedMonday.Year)
            {
                // Get roster start and end times
                string startTimeStr = PlayerPrefs.GetString("RoosterStartTime", "06:00");
                string endTimeStr = PlayerPrefs.GetString("RoosterEndTime", "22:00");

                int startMinutes = TimeStringToMinutes(startTimeStr);
                int endMinutes = TimeStringToMinutes(endTimeStr);
                int currentMinutes = now.Hour * 60 + now.Minute;

                // Check if current time is within roster hours
                if (currentMinutes >= startMinutes && currentMinutes <= endMinutes)
                {
                    timeIndicator2.SetActive(true);

                    // Calculate position
                    float timeProgress = (float)(currentMinutes - startMinutes) / (endMinutes - startMinutes);
                    float yPosition = Mathf.Lerp(600f, -980f, timeProgress);

                    // Update position
                    Vector3 currentPos = timeIndicator2.transform.localPosition;
                    timeIndicator2.transform.localPosition = new Vector3(currentPos.x, yPosition, currentPos.z);
                }
                else
                {
                    timeIndicator2.SetActive(false);
                }
            }
            else
            {
                timeIndicator2.SetActive(false);
            }
        }

        // Add this line at the end of Update
        UpdateCurrentDayIndicators();
    }

    // Add this method to update the day indicators
    private void UpdateCurrentDayIndicators()
    {
        // Get current week info
        DateTime now = DateTime.Now;
        DateTime monday = now.AddDays(-(int)now.DayOfWeek + (now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        int currentWeekNum = ISOWeek.GetWeekOfYear(monday);
        
        // Get selected week info
        DateTime selectedMonday = monday.AddDays(7 * selectedWeek);
        int selectedWeekNum = ISOWeek.GetWeekOfYear(selectedMonday);

        // Check if we're in the current week
        bool isCurrentWeek = currentWeekNum == selectedWeekNum && now.Year == selectedMonday.Year;

        // Deactivate all indicators first
        foreach (var indicator in currentDayIndicator)
        {
            if (indicator != null)
            {
                indicator.SetActive(false);
            }
        }

        // If it's the current week, activate the indicator for the current day
        if (isCurrentWeek && currentDayIndicator.Count >= 7)
        {
            // Convert DayOfWeek to our index (Monday = 0, Sunday = 6)
            int currentDayIndex = ((int)now.DayOfWeek + 6) % 7;
            
            if (currentDayIndex >= 0 && currentDayIndex < currentDayIndicator.Count)
            {
                currentDayIndicator[currentDayIndex].SetActive(true);
            }
        }
    }
}


using UnityEngine;
using Unity.Services.Analytics;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Core.Analytics;

public class UnityAnalyticsManager : MonoBehaviour
{
    public static UnityAnalyticsManager Instance { get; private set; }
    private DateTime sessionStartTime;
    private Dictionary<string, DateTime> screenStartTimes = new Dictionary<string, DateTime>();
    private bool _isInitialized = false;

    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            try
            {
                await UnityServices.InitializeAsync();
                _isInitialized = true;
                //Debug.Log("[Analytics] Services initialized successfully");
                //Debug.Log("[Analytics] Analytics service ready to send events");
            }
            catch (Exception e)
            {
                //Debug.LogError($"[Analytics] Failed to initialize Unity Services: {e.Message}");
                _isInitialized = false;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        sessionStartTime = DateTime.Now;
        //TrackSessionStart();
    }

    private void OnApplicationQuit()
    {
        //TrackSessionEnd();
    }

    #region User Engagement Metrics

    //public void TrackSessionStart()
    //{
    //    if (!_isInitialized) return;
        
    //    CustomEvent myEvent = new CustomEvent("session_start");
    //    AnalyticsService.Instance.RecordEvent(myEvent);
    //    AnalyticsService.Instance.Flush();
    //    Debug.Log("[Analytics] Session start event sent");
    //}

    //public void TrackSessionEnd()
    //{
    //    if (!_isInitialized) return;

    //    TimeSpan sessionDuration = DateTime.Now - sessionStartTime;
    //    CustomEvent myEvent = new CustomEvent("session_end");
    //    myEvent.Add("duration_minutes", sessionDuration.TotalMinutes);
        
    //    AnalyticsService.Instance.RecordEvent(myEvent);
    //    AnalyticsService.Instance.Flush();
    //    Debug.Log($"[Analytics] Session end event sent. Duration: {sessionDuration.TotalMinutes:F2} minutes");
    //}

    public void TrackScreenView(string screenName)
    {
        if (!_isInitialized) return;

        screenStartTimes[screenName] = DateTime.Now;
        
        CustomEvent myEvent = new CustomEvent("screen_view");
        myEvent.Add("screen_name", screenName);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
        //Debug.Log($"[Analytics] Screen view event sent: {screenName}");
    }

    public void TrackScreenExit(string screenName)
    {
        if (!_isInitialized) return;

        if (screenStartTimes.TryGetValue(screenName, out DateTime startTime))
        {
            TimeSpan duration = DateTime.Now - startTime;
            
            CustomEvent myEvent = new CustomEvent("screen_duration");
            myEvent.Add("screen_name", screenName);
            myEvent.Add("duration_seconds", duration.TotalSeconds);
            
            AnalyticsService.Instance.RecordEvent(myEvent);
            AnalyticsService.Instance.Flush();
            Debug.Log($"[Analytics] Screen exit event sent: {screenName}, Duration: {duration.TotalSeconds:F2} seconds");
            screenStartTimes.Remove(screenName);
        }
    }

    public void TrackAppError(string errorType, string errorMessage)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("app_error");
        myEvent.Add("error_type", errorType);
        myEvent.Add("error_message", errorMessage);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
        Debug.Log($"[Analytics] Error event sent: {errorType} - {errorMessage}");
    }

    #endregion

    #region Instructor Activity

    public void TrackInstructorLogin(string instructorName)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("instructor_login");
        myEvent.Add("instructor_name", instructorName);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
        Debug.Log($"[Analytics] Instructor login event sent: {instructorName}");
    }

    public void TrackDrivingSchoolCreation(string schoolName)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("driving_school_created");
        myEvent.Add("school_name", schoolName);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    public void TrackAvailabilityUpdate(string userType, int slotsCount)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("availability_update");
        myEvent.Add("user_type", userType);
        myEvent.Add("slots_count", slotsCount);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    public void TrackLessonCreation(string instructorName, int lessonDuration)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("lesson_created");
        myEvent.Add("instructor_name", instructorName);
        myEvent.Add("duration_minutes", lessonDuration);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    public void TrackScheduleGeneration(bool minimizeChanges, int lessonsGenerated)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("schedule_generated");
        myEvent.Add("minimize_changes", minimizeChanges);
        myEvent.Add("lessons_generated", lessonsGenerated);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    #endregion

    #region Student Activity

    public void TrackStudentLogin(string studentName)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("student_login");
        myEvent.Add("student_name", studentName);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
        Debug.Log($"[Analytics] Student login event sent: {studentName}");
    }

    public void TrackStudentCreation(string studentName, int frequencyPerWeek, int minutesPerLesson)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("student_created");
        myEvent.Add("student_name", studentName);
        myEvent.Add("frequency_per_week", frequencyPerWeek);
        myEvent.Add("minutes_per_lesson", minutesPerLesson);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    public void TrackSchoolSelection(string schoolName)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("school_selected");
        myEvent.Add("school_name", schoolName);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    public void TrackLessonRequest(string studentName, string instructorName)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("lesson_requested");
        myEvent.Add("student_name", studentName);
        myEvent.Add("instructor_name", instructorName);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    #endregion

    #region Scheduling Metrics

    public void TrackLessonModification(string modificationType, string reason)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("lesson_modified");
        myEvent.Add("modification_type", modificationType);
        myEvent.Add("reason", reason);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    public void TrackInstructorUtilization(string instructorName, float utilizationRate)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("instructor_utilization");
        myEvent.Add("instructor_name", instructorName);
        myEvent.Add("utilization_rate", utilizationRate);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    public void TrackLessonCancellation(string lessonId, string reason)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("lesson_cancelled");
        myEvent.Add("lesson_id", lessonId);
        myEvent.Add("reason", reason);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    #endregion

    #region Error Tracking

    public void TrackLoginFailure(string userType, string reason)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("login_failure");
        myEvent.Add("user_type", userType);
        myEvent.Add("failure_reason", reason);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    public void TrackAPIFailure(string endpoint, string errorMessage)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("api_failure");
        myEvent.Add("endpoint", endpoint);
        myEvent.Add("error_message", errorMessage);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    #endregion

    #region Subscription Tracking

    public void TrackSubscription(string subscriptionType, decimal price)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("subscription");
        myEvent.Add("subscription_type", subscriptionType);
        myEvent.Add("price", (float)price);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    public void TrackSubscriptionFailure(string subscriptionType, string reason)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("subscription_failure");
        myEvent.Add("subscription_type", subscriptionType);
        myEvent.Add("reason", reason);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    #endregion

    #region School Management

    public void TrackDrivingSchoolAccess(string schoolName)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("driving_school_access");
        myEvent.Add("school_name", schoolName);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    public void TrackStudentAdded(string schoolName, string studentName)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("student_added");
        myEvent.Add("school_name", schoolName);
        myEvent.Add("student_name", studentName);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    public void TrackStudentRemoved(string schoolName, string studentName)
    {
        if (!_isInitialized) return;

        CustomEvent myEvent = new CustomEvent("student_removed");
        myEvent.Add("school_name", schoolName);
        myEvent.Add("student_name", studentName);
        
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
    }

    #endregion
}

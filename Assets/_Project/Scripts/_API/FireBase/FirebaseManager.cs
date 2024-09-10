using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class FirebaseManager : MonoBehaviour
{
    public static void LogEventFirebase(string eventName, Parameter[] parameters)
    {

        if (InitSuccess)
        {

            FirebaseAnalytics.LogEvent(eventName, parameters);
        }
        else
        {
            onLogEventCache.AddListener(() =>
            {
                FirebaseAnalytics.LogEvent(eventName, parameters);
            });
        }
    }
    #region SetUp
    private static UnityEvent onLogEventCache = new UnityEvent();
    public static bool initSuccess;
    public static bool InitSuccess
    {
        get => initSuccess;
        set
        {
            if (value == true)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                FirebaseAnalytics.SetUserId(SystemInfo.deviceUniqueIdentifier);
                FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));

                Crashlytics.ReportUncaughtExceptionsAsFatal = true;

                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLogin);
                if (onLogEventCache != null)
                {
                    onLogEventCache.Invoke();
                    onLogEventCache.RemoveAllListeners();
                }
            }
            initSuccess = value;
        }
    }
    public static void InitValue()
    {
        InitSuccess = false;
        Remote.FetchDone = false;
    }
    public static async Task<bool> CheckAndFixDependenciesAsync()
    {
        var check = await FirebaseApp.CheckAndFixDependenciesAsync();
        return check == DependencyStatus.Available;
    }
    private static RemoteConfig remote = null;
    public static RemoteConfig Remote
    {
        get
        {
            if (remote == null)
            {
                remote = new RemoteConfig();
            }
            return remote;
        }
    }
    private DateTime timeOpen;
    private void Awake()
    {
        timeOpen = DateTime.Now;
    }
    private void OnApplicationQuit()
    {
        SetUserProperties();
    }
    private void SetUserProperties()
    {
        TimeSpan Time_PlayIn_Session = DateTime.Now - timeOpen;
        float minutesPlayInSession = Mathf.Ceil((float)Time_PlayIn_Session.TotalMinutes);
        int Day_Played = 0;
        if (!PlayerPrefs.HasKey(nameof(Day_Played)))
        {
            Day_Played = 1;
            PlayerPrefs.SetInt(nameof(Day_Played), Day_Played);
            PlayerPrefs.SetString("TimePlayed", timeOpen.ToString());
        }
        else
        {
            DateTime timeStartGame = DateTime.Parse(PlayerPrefs.GetString("TimePlayed"));
            TimeSpan time = timeOpen - timeStartGame;
            int day = (int)Mathf.Floor((float)time.TotalDays);
            if (day > 0)
            {
                Day_Played++;
                PlayerPrefs.SetInt(nameof(Day_Played), Day_Played);
            }
        }
        if (!initSuccess) return;

        FirebaseAnalytics.SetUserProperty(nameof(Time_PlayIn_Session), minutesPlayInSession.ToString());
        FirebaseAnalytics.SetUserProperty(nameof(Day_Played), Day_Played.ToString());
        //FirebaseAnalytics.SetUserProperty(StringHelper.PAYING_TYPE, UseProfile.PayingType.ToString());
        //FirebaseAnalytics.SetUserProperty(StringHelper.LEVEL, UseProfile.CurrentLevel.ToString());
    }

    #endregion SetUp
}

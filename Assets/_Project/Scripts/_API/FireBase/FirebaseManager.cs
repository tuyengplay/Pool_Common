#if FIRE_BASE
using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
#endif
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class FirebaseManager : MonoBehaviour
{
    public static void LogClickIAP(string _skuID)
    {
#if FIRE_BASE
        Parameter[] parameters = new Parameter[1]
        {
            new Parameter(NameLog.p_SkuName, _skuID.ToString())
        };
        LogEventFirebase(NameLog.BuyIAP, parameters);
#endif
        int Pay_Count = PlayerPrefs.GetInt(nameof(Pay_Count), 0);
        Pay_Count++;
        PlayerPrefs.SetInt(nameof(Pay_Count), Pay_Count);
    }

    #region Reward
    public void LogCallShowReward(string _location)
    {
#if FIRE_BASE
        Parameter[] parameters = new Parameter[1]
       {
            new Parameter(NameLog.p_Placement, _location.ToString())
       };
        LogEventFirebase(NameLog.CallShowReward, parameters);
#endif
    }
    public static void LogWatchReward(string _location, bool _hasReward, string _level)
    {
#if FIRE_BASE
        Parameter[] parameters = new Parameter[3]
        {
        new Parameter(NameLog.p_Location,_location.ToString()),
        new Parameter(NameLog.p_HasAds,_hasReward.ToString()),
        new Parameter(NameLog.p_Level,_level.ToString()),
        };
        LogEventFirebase(NameLog.WatchReward, parameters);

        LogPlacement(_location);
#endif
    }
    public static void LogRewardLoadFail(string _region, string _errormsg)
    {
#if FIRE_BASE

        Parameter[] parameters = new Parameter[2]
        {
        new Parameter(NameLog.p_Region,_region.ToString()),
        new Parameter(NameLog.p_Errormsg,_errormsg.ToString()),
        };
        LogEventFirebase(NameLog.RewardLoadFail, parameters);
#endif
    }
    public static void LogRewardClick()
    {
        LogEventFirebase(NameLog.RewardClick);
    }
    #endregion Reward
    #region Inter
    public static void LogWatchInter(string _location, bool _hasInter, string _level)
    {
#if FIRE_BASE
        Parameter[] parameters = new Parameter[3] {
        new Parameter(NameLog.p_Location,_location.ToString()),
        new Parameter(NameLog.p_HasAds,_hasInter.ToString()),
        new Parameter(NameLog.p_Level,_level.ToString()),
        };
        LogEventFirebase(NameLog.WatchInter, parameters);
        LogPlacement(_location);
#endif
    }
    public static void LogInterLoadFail(string _region, string _errormsg)
    {
#if FIRE_BASE
        Parameter[] parameters = new Parameter[2]
        {
        new Parameter(NameLog.p_Region,_region.ToString()),
        new Parameter(NameLog.p_Errormsg,_errormsg.ToString()),
        };
        LogEventFirebase(NameLog.InterLoadFail, parameters);
#endif
    }
    public static void LogInterClick()
    {
        LogEventFirebase(NameLog.InterClick);
    }
    #endregion Inter
    public static void LogAdsRevenue(string _sub, string _adsSource, string _adsUnitName, string _adsFormat, double _value, string _currency = "USD")
    {
#if FIRE_BASE
        Parameter[] parameters =
        {
            new Parameter(FirebaseAnalytics.ParameterLevel, "LevelAAA"),
            new Parameter(NameLog.p_LevelMode, "Free"),
            new Parameter(NameLog.p_AdsSource, _adsSource??""),
            new Parameter(NameLog.p_AdsUnitName, _adsUnitName??""),
            new Parameter(NameLog.p_AdsFormat,  _adsFormat ?? ""),
            new Parameter(FirebaseAnalytics.ParameterValue, _value),
            new Parameter(FirebaseAnalytics.ParameterCurrency, _currency)
        };
        LogEventFirebase(_sub + "_" + NameLog.Impression, parameters);
        LogEventFirebase(_sub + "_" + NameLog.RevenueSdk, parameters);
        LogEventFirebase(_sub + "_" + NameLog.RealTimeADS, parameters);
        LogEventFirebase(_sub + "_" + NameLog.FixRevenueSdk, parameters);
#endif
    }
    #region Static
#if FIRE_BASE
    public static void LogPlacement(string _placement)
    {
        Parameter[] parametersShowReward = new Parameter[1]
        {
        new Parameter(NameLog.p_Placement,_placement.ToString())
       };
        LogEventFirebase(_placement.ToString(), parametersShowReward);
    }
    public static void LogEventFirebase(string eventName, Parameter[] parameters)
    {

        if (InitSuccess)
        {
            try
            {
                FirebaseAnalytics.LogEvent(eventName, parameters);
            }
            catch (Exception _e)
            {
                Debug.Log(_e);
            }
        }
        else
        {
            onLogEventCache.AddListener(() =>
            {
                try
                {
                    FirebaseAnalytics.LogEvent(eventName, parameters);
                }
                catch (Exception _e)
                {
                    Debug.Log(_e);
                }
            });
        }
    }
#endif
    public static void LogEventFirebase(string eventName)
    {

        if (InitSuccess)
        {
            try
            {
#if FIRE_BASE
                FirebaseAnalytics.LogEvent(eventName);
#endif
            }
            catch (Exception _e)
            {
                Debug.Log(_e);
            }
        }
        else
        {
            onLogEventCache.AddListener(() =>
            {
                try
                {
#if FIRE_BASE
                    FirebaseAnalytics.LogEvent(eventName);
#endif
                }
                catch (Exception _e)
                {
                    Debug.Log(_e);
                }
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
#if FIRE_BASE
                FirebaseApp app = FirebaseApp.DefaultInstance;
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                FirebaseAnalytics.SetUserId(SystemInfo.deviceUniqueIdentifier);
                FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));

                Crashlytics.ReportUncaughtExceptionsAsFatal = true;

                LogEventFirebase(FirebaseAnalytics.EventLogin);
#endif
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
#if FIRE_BASE
        var check = await FirebaseApp.CheckAndFixDependenciesAsync();
        return check == DependencyStatus.Available;
#else
        return default;
#endif
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
    #endregion SetUp
    #endregion Static
    #region Follow_Player
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
                PlayerPrefs.SetString("TimePlayed", timeOpen.ToString());
            }
        }
        int Pay_Count = PlayerPrefs.GetInt(nameof(Pay_Count), 0);
        if (!initSuccess) return;
#if FIRE_BASE
        FirebaseAnalytics.SetUserProperty(nameof(Time_PlayIn_Session), minutesPlayInSession.ToString());
        FirebaseAnalytics.SetUserProperty(nameof(Day_Played), Day_Played.ToString());
        FirebaseAnalytics.SetUserProperty(nameof(Pay_Count), Pay_Count.ToString());
        //FirebaseAnalytics.SetUserProperty(StringHelper.LEVEL, UseProfile.CurrentLevel.ToString());
#endif
    }
    #endregion Follow_Player


    public static class NameLog
    {
        //NameLog
        #region NameLog
        public static readonly string BuyIAP = "Buy_IAP";

        public static readonly string WatchReward = "Watch_Reward";
        public static readonly string CallShowReward = "Call_Show_Reward";
        public static readonly string RewardLoadFail = "Reward_Load_Fail";
        public static readonly string RewardClick = "Reward_Click";

        public static readonly string WatchInter = "Watch_Inter";
        public static readonly string InterLoadFail = "Inter_Load_Fail";
        public static readonly string InterClick = "Inter_Click";

        public static readonly string Impression = "Impression_ADS";
        public static readonly string RevenueSdk = "Revenue_Sdk_ADS";
        public static readonly string RealTimeADS = "Real_Time_ADS";
        public static readonly string FixRevenueSdk = "Fix_Revenue_Sdk_ADS";
        #endregion NameLog

        //placement
        #region Placement
        public static readonly string p_SkuName = "Sku_Name";

        public static readonly string p_Location = "Location";
        public static readonly string p_HasAds = "Has_Ads";
        public static readonly string p_Level = "Level";
        public static readonly string p_Placement = "Placement";
        public static readonly string p_Errormsg = "Errormsg";
        public static readonly string p_Region = "Region";
        public static readonly string p_LevelMode = "Level_Mode";
        public static readonly string p_AdsSource = "Ads_Source";
        public static readonly string p_AdsUnitName = "Ads_Unit_Name";
        public static readonly string p_AdsFormat = "Ads_Format";
        #endregion Placement
    }
}

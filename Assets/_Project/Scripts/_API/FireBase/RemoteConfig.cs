using Firebase.RemoteConfig;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RemoteConfig
{
    public long GetValueInt(KeyConfig _key)
    {
        try
        {
            long value = 0;
            if (remoteConfigKeys.Contains(_key.ToString()))
            {
                value = FirebaseRemoteConfig.DefaultInstance.GetValue(_key.ToString()).LongValue;
            }
            else
            {
                value = (long)configDefault[_key];
            }
            return value;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return 0;
        }
    }
    public double GetValueFloat(KeyConfig _key)
    {
        try
        {
            double value = 0;
            if (remoteConfigKeys.Contains(_key.ToString()))
            {
                value = FirebaseRemoteConfig.DefaultInstance.GetValue(_key.ToString()).DoubleValue;
            }
            else
            {
                value = (double)configDefault[_key];
            }
            return value;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return 0;
        }
    }
    public bool GetValueBool(KeyConfig _key)
    {
        try
        {
            bool value = false;
            if (remoteConfigKeys.Contains(_key.ToString()))
            {
                value = FirebaseRemoteConfig.DefaultInstance.GetValue(_key.ToString()).BooleanValue;
            }
            else
            {
                value = (bool)configDefault[_key];
            }
            return value;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return false;
        }
    }
    public string GetValueString(KeyConfig _key)
    {
        try
        {
            string value = default;
            if (remoteConfigKeys.Contains(_key.ToString()))
            {
                value = FirebaseRemoteConfig.DefaultInstance.GetValue(_key.ToString()).StringValue;
            }
            else
            {
                value = (string)configDefault[_key];
            }
            return value;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return default;
        }
    }
    public bool GetValueJson<T>(KeyConfig _key, out T result)
    {
        string value = default;
        try
        {
            if (remoteConfigKeys.Contains(_key.ToString()))
            {
                value = FirebaseRemoteConfig.DefaultInstance.GetValue(_key.ToString()).StringValue;
            }
            else
            {
                value = (string)configDefault[_key];
            }
            try
            {
                result = JsonUtility.FromJson<T>(value);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                result = default;
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            result = default;
            return false;
        }
    }
    public bool fetchDone;
    public bool FetchDone
    {
        get => fetchDone;
        set
        {
            if (fetchDone == false)
            {
                configDefault.Clear();
                configDefault.Add(KeyConfig.LevelShowInter, 2);
                configDefault.Add(KeyConfig.LevelShowAOA, 2);
                configDefault.Add(KeyConfig.TimeShowInter, 30);
                configDefault.Add(KeyConfig.SkipADS, false);
            }
            fetchDone = value;
        }
    }
    #region Fetch
    private HashSet<string> remoteConfigKeys = new HashSet<string>();
    private Dictionary<KeyConfig, object> configDefault = new Dictionary<KeyConfig, object>();

    public void FetchDataNoww()
    {
        Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.FromSeconds(0));
        fetchTask.ContinueWith(FetchComplete);
    }
    public async void FetchDataNow()
    {
        Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.FromSeconds(0));
        await fetchTask.ContinueWith(FetchComplete);
    }
    public async Task FetchDataAsync()
    {
        Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.FromHours(2));
        await fetchTask.ContinueWith(FetchComplete);
    }
    private void FetchComplete(Task _fetchTask)
    {
        try
        {
            if (_fetchTask.IsCanceled)
            {
                FetchDone = true;
            }
            else if (_fetchTask.IsFaulted)
            {
                FetchDone = true;
            }
            else if (_fetchTask.IsCompleted)
            {
            }

            ConfigInfo info = FirebaseRemoteConfig.DefaultInstance.Info;
            switch (info.LastFetchStatus)
            {
                case LastFetchStatus.Success:
                    {
                        Debug.Log("---------------- Firebase Remote Init success ---------------");
                        FetchAndActivateAsync(_fetchTask, info);
                        break;
                    }
                case LastFetchStatus.Failure:
                    {
                        switch (info.LastFetchFailureReason)
                        {
                            case FetchFailureReason.Error:
                                Debug.LogError("Fetch failed for unknown reason");
                                break;
                            case FetchFailureReason.Throttled:
                                Debug.Log("Fetch throttled until " + info.ThrottledEndTime);
                                break;
                        }
                        FetchDone = true;
                        break;
                    }
                case LastFetchStatus.Pending:
                    {
                        Debug.Log("Latest Fetch call still pending.");
                        FetchDone = true;
                        break;
                    }
            }

        }
        catch (Exception e)
        {
            Debug.LogError($"Fetch Error: {e}");
            throw;
        }
    }
    public async void FetchAndActivateAsync(Task fetchTask, ConfigInfo info)
    {
        await FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync();
        if (!fetchTask.IsFaulted && fetchTask.IsCompleted)
        {
            ReloadRemoteConfigKeys();
        }
        Debug.Log(String.Format("Remote data loaded and ready (last fetch time {0}).", info.FetchTime));
        FetchDone = true;
    }
    public void ReloadRemoteConfigKeys()
    {
        remoteConfigKeys.Clear();
        foreach (var key in FirebaseRemoteConfig.DefaultInstance.Keys)
        {
            remoteConfigKeys.Add(key);
        }
    }
    #endregion Fetch
}
public enum KeyConfig
{
    LevelShowInter,
    LevelShowAOA,
    TimeShowInter,
    SkipADS,
}

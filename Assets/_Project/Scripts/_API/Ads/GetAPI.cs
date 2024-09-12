using EranCore.UniRx;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class GetAPI
{
    public static IPInfo iPInfoAPI
    {
        get;
        private set;
    }
    private static TimeDataAPI timeAPI;
    private static void TimeAPI(Action<TimeDataAPI> _result)
    {
        if (timeAPI != null)
        {
            //bat buoc co mang moi cho fetch
            GetAPIFromUrl($"http://worldtimeapi.org/api/timezone/{iPInfoAPI.timezone}", (data) =>
            {
                try
                {
                    Debug.Log(timeAPI.ToString());
                    timeAPI = JsonConvert.DeserializeObject<TimeDataAPI>(data);
                    _result?.Invoke(timeAPI);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    _result?.Invoke(timeAPI);
                }
            });
        }
    }

    private static Texture2D textureFlag;
    public static Texture2D TextureFlag(bool _isAPI = false)
    {
        if (_isAPI || PlayerPrefs.HasKey("Flag_Save") == false)
        {
            return textureFlag;
        }
        else
        {
            try
            {
                string fullPath = Path.Combine(Application.persistentDataPath, "Flag_Save");
                byte[] bytes = File.ReadAllBytes(fullPath);
                Texture2D texture = new Texture2D(256, 192);
                return texture;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }
    }
    public static void FetchIPInfo()
    {
        RequestAPI("https://ipinfo.io/json").Subscribe(data =>
        {
            try
            {
                iPInfoAPI = JsonUtility.FromJson<IPInfo>(data);
                Debug.Log(iPInfoAPI.ToString());
                LoadTexture($"https://flagcdn.com/256x192/{iPInfoAPI.country.ToLower()}.png", (flag) =>
                {
                    textureFlag = flag;
                    if (!PlayerPrefs.HasKey("Flag_Save"))
                    {
                        try
                        {
                            byte[] flagBinary = textureFlag.EncodeToPNG();
                            string fullPath = Path.Combine(Application.persistentDataPath, "Flag_Save");
                            File.WriteAllBytes(fullPath, flagBinary);
                            PlayerPrefs.SetInt("Flag_Save", 1);
                            Debug.Log("Save Flag Success _ " + fullPath);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                });
                GetAPIFromUrl($"http://worldtimeapi.org/api/timezone/{iPInfoAPI.timezone}", (data) =>
                {
                    try
                    {
                        timeAPI = JsonConvert.DeserializeObject<TimeDataAPI>(data);
                        Debug.Log(timeAPI.ToString());
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.LogError("Error Convert to Class" + ex);
            }
        }, error =>
        {
            iPInfoAPI = null;
            Debug.Log("Error IP :" + error);
        });
    }
    public static void GetAPIFromUrl(string _url, Action<string> _result)
    {
        RequestAPI(_url).Subscribe(data =>
        {
            Debug.Log("Load_Success_API");
            _result?.Invoke(data);
        }, (error =>
        {
            _result?.Invoke(null);
            Debug.Log("Error API :" + error);
        }));
    }
    private static IObservable<string> RequestAPI(string _url)
    {
        return Observable.Create<string>(observer =>
        {
            IObservable<string> request = ObservableWWW.Get(_url);
            return request.Subscribe(
                data =>
                {
                    observer.OnNext(data);
                    observer.OnCompleted();
                },
                error =>
                {
                    observer.OnError(error);
                }
            );
        });
    }
    //Load Texture
    public static void LoadTexture(string _url, Action<Texture2D> _result)
    {
        Observable.FromCoroutine<Texture2D>((observer) => IE_LoadTexture(_url, observer))
            .Subscribe(
                texture =>
                {
                    Debug.Log("Load_Success_Texture");
                    _result?.Invoke(texture);
                },
                error => Debug.LogError($"Error Texture: {error.Message}")
            );
    }
    private static IEnumerator IE_LoadTexture(string url, IObserver<Texture2D> observer)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                observer.OnError(new Exception(webRequest.error));
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                observer.OnNext(texture);
                observer.OnCompleted();
            }
        }
    }
    public class IPInfo
    {
        public string ip;
        public string city;
        public string region;
        public string country;
        public string loc;
        public string org;
        public string postal;
        public string timezone;
        public override string ToString()
        {
            return $"IP: {ip}\nCity: {city}\nRegion: {region}\nCountry: {country}\nLocation: {loc}\nOrganization: {org}\nPostal: {postal}\nTimezone: {timezone}";
        }
    }
    public class TimeDataAPI
    {
        [JsonProperty("day_of_week")]
        public int dayOfWeek;

        [JsonProperty("day_of_year")]
        public int dayOfYear;

        [JsonProperty("datetime")]
        public DateTime dateTime;

        [JsonProperty("week_number")]
        public int weekNumber;
        public override string ToString()
        {
            return $"Day Of Week: {dayOfWeek}\nDay Of Year: {dayOfYear}\nDatetime: {dateTime.ToString("yyyy-MM-dd HH:mm:ss")}\nWeek Number: {weekNumber}";
        }
        public TimeDataAPI()
        {
            dayOfWeek = 0;
            dayOfYear = 0;
            dateTime = DateTime.Now;
            weekNumber = 0;
        }
    }
}

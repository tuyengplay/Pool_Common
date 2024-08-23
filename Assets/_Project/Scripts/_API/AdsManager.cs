using System;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    [SerializeField]
    string bannerAdUnitId = "Android_banner";
    [SerializeField]
    string interstitialAdUnitId = "Android_banner";
    [SerializeField]
    string rewardAdUnitId = "Android_banner";
    [SerializeField]
    private string keyMax = "Key MAX of Game";
    #region Setup
    //Interstitial
    private int retryAttemptInterstitial;
    private Action<bool> actionShowInterstitialDone;
    private float timeShowinterstitial = 0;
    private bool isInterstitialShowing = false;
    private bool isActiveInterstitial = false;
    //Reward
    private int retryAttemptReward;
    private Action<bool> actionShowRewardDone;
    private bool isRewardShowing = false;
    private bool isClaimReward = false;
    private int totalChangeRewardToInterstitial;
    #endregion
    void Start()
    {
        InitAds();
    }
    public void InitAds()
    {
#if ADS
        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
        {
            InitializeBanner_MAX();
            InitializeInterstitial_MAX();
            InitializeReward_MAX();
        };
        MaxSdk.SetVerboseLogging(false);
        MaxSdk.SetSdkKey(keyMax);
        MaxSdk.InitializeSdk();
#endif
    }
    private void ResetValue()
    {
        //Interstitial 
        retryAttemptInterstitial = 0;
        actionShowInterstitialDone = null;
        timeShowinterstitial = 0;
        isInterstitialShowing = false;
        isActiveInterstitial = false;
        //Reward
        retryAttemptReward = 0;
        actionShowRewardDone = null;
        isRewardShowing = false;
        isClaimReward = false;
        totalChangeRewardToInterstitial = 0;
    }
    #region API
    public void ShowBanner_MAX()
    {
#if ADS
        MaxSdk.ShowBanner(bannerAdUnitId);
#endif
    }
    public void HideBanner_MAX()
    {
#if ADS
        MaxSdk.HideBanner(bannerAdUnitId);
#endif
    }
    public void DestroyBanner_MAX()
    {
#if ADS
        MaxSdk.DestroyBanner(bannerAdUnitId);
#endif
    }
    public void ShowInterstitial_MAX(Action<bool> _done, AdLocation _location, bool _isShowImmediately = false, string _locationSub = "")
    {
        isActiveInterstitial = false;
        isInterstitialShowing = true;
#if  !ADS
        isInterstitialShowing = false;
        _done?.Invoke(true);
        return;
#else
        if (_isShowImmediately == false)
        {
            if (isBlockSpam)
            {
                isInterstitialShowing = false;
                _done?.Invoke(false);
                return;
            }
        }
        if (IsInterstitialReady_MAX)
        {
            actionShowInterstitialDone = _done;
            MaxSdk.ShowInterstitial(interstitialAdUnitId, $"{_location.ToString()}___{_locationSub}");
        }
        else
        {
            isInterstitialShowing = false;
            _done?.Invoke(false);
        }
#endif
    }
    public void ShowReward_MAX(Action<bool> _done, AdLocation _location)
    {
        isRewardShowing = true;
        isClaimReward = false;
#if!ADS
        isRewardShowing = false;
        _done?.Invoke(true);
        return;
#else
        if (IsRewardedReady_MAX)
        {
            totalChangeRewardToInterstitial = 0;
            actionShowRewardDone = _done;
            MaxSdk.ShowRewardedAd(rewardAdUnitId, _location.ToString());
        }
        else
        {
            if (totalChangeRewardToInterstitial <= 3)
            {
                totalChangeRewardToInterstitial++;
                isRewardShowing = false;
                ShowInterstitial_MAX(_done, _location, true, "FromReward");
            }
            else
            {
                isRewardShowing = false;
                _done?.Invoke(true);
            }
        }
#endif
    }
    #endregion
#if ADS
    #region BANNER
    private void InitializeBanner_MAX()
    {
        MaxSdk.CreateBanner(bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);

        MaxSdk.SetBannerBackgroundColor(bannerAdUnitId, Color.clear);
        MaxSdk.SetBannerExtraParameter(bannerAdUnitId, "adaptive_banner", "true");

        MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent_MAX;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent_MAX;
        MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent_MAX;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent_MAX;
        MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent_MAX;
        MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent_MAX;
    }
    private void OnBannerAdLoadedEvent_MAX(string adUnitId, MaxSdkBase.AdInfo adInfo) { }
    private void OnBannerAdLoadFailedEvent_MAX(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) { }
    private void OnBannerAdClickedEvent_MAX(string adUnitId, MaxSdkBase.AdInfo adInfo) { }
    private void OnBannerAdRevenuePaidEvent_MAX(string adUnitId, MaxSdkBase.AdInfo adInfo) { }
    private void OnBannerAdExpandedEvent_MAX(string adUnitId, MaxSdkBase.AdInfo adInfo) { }
    private void OnBannerAdCollapsedEvent_MAX(string adUnitId, MaxSdkBase.AdInfo adInfo) { }
    #endregion
    #region Interstitial
    
    private void InitializeInterstitial_MAX()
    {
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent_MAX;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent_MAX;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent_MAX;
        MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent_MAX;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent_MAX;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent_MAX;

    }
    private void LoadInterstitial_MAX()
    {
        MaxSdk.LoadInterstitial(interstitialAdUnitId);
    }
    private bool IsInterstitialReady_MAX
    {
        get => MaxSdk.IsInterstitialReady(interstitialAdUnitId);
    }
    private bool isBlockSpam
    {
        get => Time.time - timeShowinterstitial > 30f;
    }
    private void OnInterstitialLoadedEvent_MAX(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        retryAttemptInterstitial = 0;
    }
    private void OnInterstitialLoadFailedEvent_MAX(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        retryAttemptInterstitial++;
        double retryDelay = Math.Pow(1.8f, Math.Min(6, retryAttemptInterstitial));
        Invoke(nameof(LoadInterstitial_MAX), (float)retryDelay);
    }
    private void OnInterstitialDisplayedEvent_MAX(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        isActiveInterstitial = true;
    }
    private void OnInterstitialAdFailedToDisplayEvent_MAX(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        isInterstitialShowing = false;
        actionShowInterstitialDone?.Invoke(isActiveInterstitial);
        actionShowInterstitialDone = null;
        LoadInterstitial_MAX();
    }
    private void OnInterstitialClickedEvent_MAX(string adUnitId, MaxSdkBase.AdInfo adInfo) { }
    private void OnInterstitialHiddenEvent_MAX(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        isInterstitialShowing = false;
        actionShowInterstitialDone?.Invoke(isActiveInterstitial);
        actionShowInterstitialDone = null;
        timeShowinterstitial = Time.time;
        LoadInterstitial_MAX();
    }
    #endregion
    #region Reward
    
    private void InitializeReward_MAX()
    {
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent_MAX;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent_MAX;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent_MAX;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent_MAX;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent_MAX;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent_MAX;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent_MAX;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent_MAX;

        LoadRewarded_MAX();
    }
    private void LoadRewarded_MAX()
    {
        MaxSdk.LoadRewardedAd(rewardAdUnitId);
    }
    private bool IsRewardedReady_MAX
    {
        get => MaxSdk.IsRewardedAdReady(rewardAdUnitId);
    }
    private void OnRewardedAdLoadedEvent_MAX(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        retryAttemptReward = 0;
    }
    private void OnRewardedAdLoadFailedEvent_MAX(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        retryAttemptReward++;
        double retryDelay = Math.Pow(1.8f, Math.Min(6, retryAttemptReward));
        Invoke(nameof(LoadRewarded_MAX), (float)retryDelay);
    }
    private void OnRewardedAdDisplayedEvent_MAX(string adUnitId, MaxSdkBase.AdInfo adInfo) { }
    private void OnRewardedAdFailedToDisplayEvent_MAX(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        isRewardShowing = false;
        actionShowRewardDone?.Invoke(isClaimReward);
        actionShowRewardDone = null;
        LoadRewarded_MAX();
    }
    private void OnRewardedAdClickedEvent_MAX(string adUnitId, MaxSdkBase.AdInfo adInfo) { }
    private void OnRewardedAdHiddenEvent_MAX(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        isRewardShowing = false;
        actionShowRewardDone?.Invoke(isClaimReward);
        actionShowRewardDone = null;
        LoadRewarded_MAX();
    }
    private void OnRewardedAdReceivedRewardEvent_MAX(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        isClaimReward = true;
    }
    private void OnRewardedAdRevenuePaidEvent_MAX(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }
    #endregion
#endif

}
public enum AdLocation
{
    EndGame,
}

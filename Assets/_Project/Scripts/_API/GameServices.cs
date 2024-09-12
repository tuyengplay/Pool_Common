using EranCore.UniRx;
using System;
using UnityEngine;

public class GameServices : SingletonClass<GameServices>, IService
{
    public static Subject<bool> InternetSubject = new Subject<bool>();
    public static AsyncSubject<bool> FetchFirebaseSubject = new AsyncSubject<bool>();
    public void Init()
    {
        FirebaseManager.InitValue();
        IObservable<bool> internetObservable = Observable.Interval(TimeSpan.FromSeconds(1), Scheduler.MainThreadIgnoreTimeScale).Select(value => IntenetAvaiable);
        internetObservable.DistinctUntilChanged().Subscribe(value =>
        {
            Debug.Log("DistinctUntilChanged().Subscribe" + value);
            InternetSubject.OnNext(value);
        });
        internetObservable.Where(haveInternet =>
        {
            Debug.Log("internetObservable.Where" + haveInternet);
            return haveInternet;
        }).Take(1)
        .SelectMany(value =>
        {
            GetAPI.FetchIPInfo();
            return FirebaseManager.CheckAndFixDependenciesAsync().ToObservable();
        })
        .ObserveOnMainThread()
        .SelectMany(isAvailable =>
        {
            if (isAvailable)
            {
                FirebaseManager.InitSuccess = true;
                return FirebaseManager.Remote.FetchDataAsync().ToObservable();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: ");
                return Observable.Throw<Unit>(new Exception("Could not resolve all Firebase dependencies: "));
            }
        })
        .Timeout(TimeSpan.FromSeconds(80))
        .Subscribe(value =>
        {
            FetchFirebaseSubject.OnNext(true);
            FetchFirebaseSubject.OnCompleted();

        }, ex =>
        {
            Debug.LogError("Check Firebase dependencies: " + ex.Message);
        });
    }
    public bool IntenetAvaiable
    {
        get { return Application.internetReachability != NetworkReachability.NotReachable; }
    }
}

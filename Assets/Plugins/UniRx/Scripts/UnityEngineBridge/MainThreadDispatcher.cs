#if !(UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
#define SupportCustomYieldInstruction
#endif

using System;
using System.Collections;
using EranCore.UniRx.InternalUtil;
using UnityEngine;

namespace EranCore.UniRx
{
    public sealed class MainThreadDispatcher : MonoBehaviour
    {
        public enum CullingMode
        {
            /// <summary>
            /// Won't remove any MainThreadDispatchers.
            /// </summary>
            Disabled,

            /// <summary>
            /// Checks if there is an existing MainThreadDispatcher on Awake(). If so, the new dispatcher removes itself.
            /// </summary>
            Self,

            /// <summary>
            /// Search for excess MainThreadDispatchers and removes them all on Awake().
            /// </summary>
            All
        }

        public static CullingMode cullingMode = CullingMode.Self;

        /// <summary>Dispatch Asyncrhonous action.</summary>
        public static void Post(Action<object> action, object state)
        {
            var dispatcher = Instance;
            if (!isQuitting && !object.ReferenceEquals(dispatcher, null))
            {
                dispatcher.queueWorker.Enqueue(action, state);
            }
        }

        /// <summary>Dispatch Synchronous action if possible.</summary>
        public static void Send(Action<object> action, object state)
        {
            if (mainThreadToken != null)
            {
                try
                {
                    action(state);
                }
                catch (Exception ex)
                {
                    var dispatcher = MainThreadDispatcher.Instance;
                    if (dispatcher != null)
                    {
                        dispatcher.unhandledExceptionCallback(ex);
                    }
                }
            }
            else
            {
                Post(action, state);
            }
        }

        /// <summary>Run Synchronous action.</summary>
        public static void UnsafeSend(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                var dispatcher = MainThreadDispatcher.Instance;
                if (dispatcher != null)
                {
                    dispatcher.unhandledExceptionCallback(ex);
                }
            }
        }

        /// <summary>Run Synchronous action.</summary>
        public static void UnsafeSend<T>(Action<T> action, T state)
        {
            try
            {
                action(state);
            }
            catch (Exception ex)
            {
                var dispatcher = MainThreadDispatcher.Instance;
                if (dispatcher != null)
                {
                    dispatcher.unhandledExceptionCallback(ex);
                }
            }
        }

        /// <summary>ThreadSafe StartCoroutine.</summary>
        public static void SendStartCoroutine(IEnumerator routine)
        {
            if (mainThreadToken != null)
            {
                StartCoroutine(routine);
            }
            else
            {
                var dispatcher = Instance;
                if (!isQuitting && !object.ReferenceEquals(dispatcher, null))
                {
                    dispatcher.queueWorker.Enqueue(_ =>
                    {
                        var dispacher2 = Instance;
                        if (dispacher2 != null)
                        {
                            (dispacher2 as MonoBehaviour).StartCoroutine(routine);
                        }
                    }, null);
                }
            }
        }

        public static void StartUpdateMicroCoroutine(IEnumerator routine)
        {
            var dispatcher = Instance;
            if (dispatcher != null)
            {
                dispatcher.updateMicroCoroutine.AddCoroutine(routine);
            }
        }

        public static void StartFixedUpdateMicroCoroutine(IEnumerator routine)
        {
            var dispatcher = Instance;
            if (dispatcher != null)
            {
                dispatcher.fixedUpdateMicroCoroutine.AddCoroutine(routine);
            }
        }

        public static void StartEndOfFrameMicroCoroutine(IEnumerator routine)
        {
            var dispatcher = Instance;
            if (dispatcher != null)
            {
                dispatcher.endOfFrameMicroCoroutine.AddCoroutine(routine);
            }
        }

        new public static Coroutine StartCoroutine(IEnumerator routine)
        {
            var dispatcher = Instance;
            if (dispatcher != null)
            {
                return (dispatcher as MonoBehaviour).StartCoroutine(routine);
            }
            else
            {
                return null;
            }
        }

        public static void RegisterUnhandledExceptionCallback(Action<Exception> exceptionCallback)
        {
            if (exceptionCallback == null)
            {
                // do nothing
                Instance.unhandledExceptionCallback = Stubs<Exception>.Ignore;
            }
            else
            {
                Instance.unhandledExceptionCallback = exceptionCallback;
            }
        }

        ThreadSafeQueueWorker queueWorker = new ThreadSafeQueueWorker();
        Action<Exception> unhandledExceptionCallback = ex => Debug.LogException(ex); // default

        MicroCoroutine updateMicroCoroutine = null;
        MicroCoroutine fixedUpdateMicroCoroutine = null;
        MicroCoroutine endOfFrameMicroCoroutine = null;

        static MainThreadDispatcher instance;
        static bool initialized;
        static bool isQuitting = false;

        public static string InstanceName
        {
            get
            {
                if (instance == null)
                {
                    throw new NullReferenceException("MainThreadDispatcher is not initialized.");
                }
                return instance.name;
            }
        }

        public static bool IsInitialized
        {
            get { return initialized && instance != null; }
        }

        [ThreadStatic]
        static object mainThreadToken;

        static MainThreadDispatcher Instance
        {
            get
            {
                Initialize();
                return instance;
            }
        }

        public static void Initialize()
        {
            if (!initialized)
            {
                MainThreadDispatcher dispatcher = null;

                try
                {
                    dispatcher = GameObject.FindObjectOfType<MainThreadDispatcher>();
                }
                catch
                {
                    // Throw exception when calling from a worker thread.
                    var ex = new Exception("UniRx requires a MainThreadDispatcher component created on the main thread. Make sure it is added to the scene before calling UniRx from a worker thread.");
                    UnityEngine.Debug.LogException(ex);
                    throw ex;
                }

                if (isQuitting)
                {
                    // don't create new instance after quitting
                    // avoid "Some objects were not cleaned up when closing the scene find target" error.
                    return;
                }

                if (dispatcher == null)
                {
                    // awake call immediately from UnityEngine
                    new GameObject("MainThreadDispatcher").AddComponent<MainThreadDispatcher>();
                }
                else
                {
                    dispatcher.Awake(); // force awake
                }
            }
        }

        public static bool IsInMainThread
        {
            get
            {
                return (mainThreadToken != null);
            }
        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                mainThreadToken = new object();
                initialized = true;

                updateMicroCoroutine = new MicroCoroutine(ex => unhandledExceptionCallback(ex));
                fixedUpdateMicroCoroutine = new MicroCoroutine(ex => unhandledExceptionCallback(ex));
                endOfFrameMicroCoroutine = new MicroCoroutine(ex => unhandledExceptionCallback(ex));

                StartCoroutine(RunUpdateMicroCoroutine());
                StartCoroutine(RunFixedUpdateMicroCoroutine());
                StartCoroutine(RunEndOfFrameMicroCoroutine());

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (this != instance)
                {
                    if (cullingMode == CullingMode.Self)
                    {
                        // Try to destroy this dispatcher if there's already one in the scene.
                        Debug.LogWarning("There is already a MainThreadDispatcher in the scene. Removing myself...");
                        DestroyDispatcher(this);
                    }
                    else if (cullingMode == CullingMode.All)
                    {
                        Debug.LogWarning("There is already a MainThreadDispatcher in the scene. Cleaning up all excess dispatchers...");
                        CullAllExcessDispatchers();
                    }
                    else
                    {
                        Debug.LogWarning("There is already a MainThreadDispatcher in the scene.");
                    }
                }
            }
        }

        IEnumerator RunUpdateMicroCoroutine()
        {
            while (true)
            {
                yield return null;
                updateMicroCoroutine.Run();
            }
        }

        IEnumerator RunFixedUpdateMicroCoroutine()
        {
            while (true)
            {
                yield return YieldInstructionCache.WaitForFixedUpdate;
                fixedUpdateMicroCoroutine.Run();
            }
        }

        IEnumerator RunEndOfFrameMicroCoroutine()
        {
            while (true)
            {
                yield return YieldInstructionCache.WaitForEndOfFrame;
                endOfFrameMicroCoroutine.Run();
            }
        }

        static void DestroyDispatcher(MainThreadDispatcher aDispatcher)
        {
            if (aDispatcher != instance)
            {
                // Try to remove game object if it's empty
                var components = aDispatcher.gameObject.GetComponents<Component>();
                if (aDispatcher.gameObject.transform.childCount == 0 && components.Length == 2)
                {
                    if (components[0] is Transform && components[1] is MainThreadDispatcher)
                    {
                        Destroy(aDispatcher.gameObject);
                    }
                }
                else
                {
                    // Remove component
                    MonoBehaviour.Destroy(aDispatcher);
                }
            }
        }

        public static void CullAllExcessDispatchers()
        {
            var dispatchers = GameObject.FindObjectsOfType<MainThreadDispatcher>();
            for (int i = 0; i < dispatchers.Length; i++)
            {
                DestroyDispatcher(dispatchers[i]);
            }
        }

        void OnDestroy()
        {
            if (instance == this)
            {
                instance = GameObject.FindObjectOfType<MainThreadDispatcher>();
                initialized = instance != null;

                /*
                // Although `this` still refers to a gameObject, it won't be found.
                var foundDispatcher = GameObject.FindObjectOfType<MainThreadDispatcher>();

                if (foundDispatcher != null)
                {
                    // select another game object
                    Debug.Log("new instance: " + foundDispatcher.name);
                    instance = foundDispatcher;
                    initialized = true;
                }
                */
            }
        }

        void Update()
        {
            if (update != null)
            {
                try
                {
                    update.OnNext(Unit.Default);
                }
                catch (Exception ex)
                {
                    unhandledExceptionCallback(ex);
                }
            }
            queueWorker.ExecuteAll(unhandledExceptionCallback);
        }

        // for Lifecycle Management

        Subject<Unit> update;

        public static IObservable<Unit> UpdateAsObservable()
        {
            return Instance.update ?? (Instance.update = new Subject<Unit>());
        }

        Subject<Unit> lateUpdate;

        void LateUpdate()
        {
            if (lateUpdate != null) lateUpdate.OnNext(Unit.Default);
        }

        public static IObservable<Unit> LateUpdateAsObservable()
        {
            return Instance.lateUpdate ?? (Instance.lateUpdate = new Subject<Unit>());
        }

        Subject<bool> onApplicationFocus;

        void OnApplicationFocus(bool focus)
        {
            if (onApplicationFocus != null) onApplicationFocus.OnNext(focus);
        }

        public static IObservable<bool> OnApplicationFocusAsObservable()
        {
            return Instance.onApplicationFocus ?? (Instance.onApplicationFocus = new Subject<bool>());
        }

        Subject<bool> onApplicationPause;

        void OnApplicationPause(bool pause)
        {
            if (onApplicationPause != null) onApplicationPause.OnNext(pause);
        }

        public static IObservable<bool> OnApplicationPauseAsObservable()
        {
            return Instance.onApplicationPause ?? (Instance.onApplicationPause = new Subject<bool>());
        }

        Subject<Unit> onApplicationQuit;

        void OnApplicationQuit()
        {
            isQuitting = true;
            if (onApplicationQuit != null) onApplicationQuit.OnNext(Unit.Default);
        }

        public static IObservable<Unit> OnApplicationQuitAsObservable()
        {
            return Instance.onApplicationQuit ?? (Instance.onApplicationQuit = new Subject<Unit>());
        }
    }
}
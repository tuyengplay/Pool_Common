using UnityEngine;

public class ApplicationStart
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnStartBeforeSceneLoad()
    {
        RegisterServices();
    }
    private static void RegisterServices()
    {
        RegisterService<GameServices>();
    }
    private static T RegisterService<T>() where T : class, IService, new()
    {
        SingletonClass<T>.Instance.Init();
        return SingletonClass<T>.Instance;
    }
}

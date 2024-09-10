public class SingletonClass<T> where T : class, new()
{
    public static T Instance { get; } = new T();
}
public interface IService
{
    void Init();
}

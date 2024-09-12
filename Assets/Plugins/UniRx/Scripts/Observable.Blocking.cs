using System;

namespace EranCore.UniRx
{
    public static partial class Observable
    {
        public static T Wait<T>(this IObservable<T> source)
        {
            return new EranCore.UniRx.Operators.Wait<T>(source, InfiniteTimeSpan).Run();
        }

        public static T Wait<T>(this IObservable<T> source, TimeSpan timeout)
        {
            return new EranCore.UniRx.Operators.Wait<T>(source, timeout).Run();
        }
    }
}

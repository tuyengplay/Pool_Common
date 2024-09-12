using System;

namespace EranCore.UniRx
{
    public interface ICancelable : IDisposable
    {
        bool IsDisposed { get; }
    }
}

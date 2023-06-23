using System;

namespace LibSirius.Utils;

public class SetBusy : IDisposable
{
    private readonly Action<bool> SetIsBusy;

    public SetBusy(Action<bool> setIsBusy)
    {
        SetIsBusy = setIsBusy;

        SetIsBusy(true);
    }

    public void Dispose()
    {
        SetIsBusy(false);
    }
}

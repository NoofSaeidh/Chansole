namespace Chansole.Common;

public static class Disposable
{
    public static IDisposable Join(params IDisposable[] disposables)
    {
        return new ActionDisposable(() =>
        {
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        });
    }

    private class ActionDisposable : IDisposable
    {
        private readonly Action _dispose;

        public ActionDisposable(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            _dispose();
        }
    }
}
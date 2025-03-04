using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Threading;

namespace Vet_System.Lib
{
    internal class Bootstrap
    {
        private static bool isInitialized;

        public static bool TryInitialize()
        {
            if (isInitialized)
                return true;

            try
            {
               Application.Start((p) =>
                {
                    var context = new DispatcherQueueSynchronizationContext(
                    DispatcherQueue.GetForCurrentThread());
                    SynchronizationContext.SetSynchronizationContext(context);
                });
                isInitialized = true;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

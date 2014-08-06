using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace ExampleApplication
{
    public static class DispatcherHelper
    {
        public static Task RunOnUIThreadAsync(Action action)
        {
            return RunOnUIThreadAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, action);
        }

        public static async Task RunOnUIThreadAsync(Windows.UI.Core.CoreDispatcherPriority priority, Action action)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(priority, () =>
            {
                action();
            });
        }
    }
}

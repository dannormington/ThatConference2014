using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ExampleApplication
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterMaintenanceBackgroundTask();
            RegisterTimerBackgroundTaskAsync();
        }

        #region Background Task Registration

        private const string BackgroundTaskEndpoint = "ExampleBackgroundTask.DownloadFilesTask";

        private const string DownloadTimerTaskName = "DownloadTimerTask";

        private async Task RegisterTimerBackgroundTaskAsync()
        {
            IBackgroundTaskRegistration downloadTimerTask = BackgroundTaskRegistration.AllTasks.SingleOrDefault(x => x.Value.Name == DownloadTimerTaskName).Value;

            if (downloadTimerTask == null)
            {
                //request access
                var currentStatus = await BackgroundExecutionManager.RequestAccessAsync();

                if (currentStatus == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity ||
                currentStatus == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity)
                {

                    var builder = new BackgroundTaskBuilder();

                    builder.Name = DownloadTimerTaskName;
                    builder.TaskEntryPoint = BackgroundTaskEndpoint;
                    builder.SetTrigger(new TimeTrigger(15, false));
                    builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                    builder.CancelOnConditionLoss = true;

                    downloadTimerTask = builder.Register();

                    downloadTimerTask.Completed += BackgroundTask_Completed;
                }
            }
            else
            {
                downloadTimerTask.Completed += BackgroundTask_Completed;
            }            
        }


        private const string DownloadMaintenanceTaskName = "DownloadMaintenanceTask";

        private void RegisterMaintenanceBackgroundTask()
        {
            IBackgroundTaskRegistration downloadMaintenanceTask = BackgroundTaskRegistration.AllTasks.SingleOrDefault(x => x.Value.Name == DownloadMaintenanceTaskName).Value;

            if (downloadMaintenanceTask == null)
            {
                var builder = new BackgroundTaskBuilder();

                builder.Name = DownloadMaintenanceTaskName;
                builder.TaskEntryPoint = BackgroundTaskEndpoint;
                builder.SetTrigger(new MaintenanceTrigger(15, false));
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                builder.CancelOnConditionLoss = true;

                downloadMaintenanceTask = builder.Register();
            }

            downloadMaintenanceTask.Completed += BackgroundTask_Completed;
        }

        void BackgroundTask_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            //notify user
        }

        #endregion
    }


}

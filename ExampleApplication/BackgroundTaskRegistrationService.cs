using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace ExampleApplication
{
    public class BackgroundTaskRegistrationService
    {
        public const string DownloadFilesTaskEndpoint = "ExampleBackgroundTask.DownloadFilesTask";
        public const string DownloadTimerTaskName = "DownloadTimerTask";
        public const string DownloadMaintenanceTaskName = "DownloadMaintenanceTask";

        #region Background Task Registration

        public async Task RegisterTimerBackgroundTaskAsync()
        {
            IBackgroundTaskRegistration downloadTimerTask = GetBackgroundTask(DownloadTimerTaskName);

            if (downloadTimerTask == null)
            {
                //request access
                var currentStatus = await BackgroundExecutionManager.RequestAccessAsync();

                if (currentStatus == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity ||
                currentStatus == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity)
                {

                    var builder = new BackgroundTaskBuilder();

                    builder.Name = DownloadTimerTaskName;
                    builder.TaskEntryPoint = DownloadFilesTaskEndpoint;
                    builder.SetTrigger(new TimeTrigger(15, false));
                    builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                    builder.CancelOnConditionLoss = true;

                    downloadTimerTask = builder.Register();
                }
            }
        }

        public void RegisterMaintenanceBackgroundTask()
        {
            IBackgroundTaskRegistration downloadMaintenanceTask = GetBackgroundTask(DownloadMaintenanceTaskName);

            if (downloadMaintenanceTask == null)
            {
                var builder = new BackgroundTaskBuilder();

                builder.Name = DownloadMaintenanceTaskName;
                builder.TaskEntryPoint = DownloadFilesTaskEndpoint;
                builder.SetTrigger(new MaintenanceTrigger(15, false));
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                builder.CancelOnConditionLoss = true;

                downloadMaintenanceTask = builder.Register();
            }
        }

        public IBackgroundTaskRegistration GetBackgroundTask(string taskName) 
        {
            return BackgroundTaskRegistration.AllTasks.SingleOrDefault(x => x.Value.Name == taskName).Value;
        }

        #endregion

    }
}

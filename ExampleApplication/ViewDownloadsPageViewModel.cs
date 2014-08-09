using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;

namespace ExampleApplication
{
    public class ViewDownloadsPageViewModel
    {
        public ViewDownloadsPageViewModel() 
        {
            this.Downloads = new ObservableCollection<DownloadProgressViewModel>();
        }

        public ObservableCollection<DownloadProgressViewModel> Downloads { get; set; }

        public async Task LoadDataAsync() 
        {
            //listen for progress in case task starts after user navigates to page
            var backgroundTaskService = new BackgroundTaskRegistrationService();
            var task = backgroundTaskService.GetBackgroundTask(BackgroundTaskRegistrationService.DownloadMaintenanceTaskName);
            task.Progress += task_Progress;

            await LoadActiveDowloads();
        }


        private async Task LoadActiveDowloads() 
        {
            await DispatcherHelper.RunOnUIThreadAsync(() => Downloads.Clear());

            var downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();

            await DispatcherHelper.RunOnUIThreadAsync(() =>
            {
                foreach (var download in downloads)
                {
                    this.Downloads.Add(new DownloadProgressViewModel(download));
                }
            });
        }

        void task_Progress(Windows.ApplicationModel.Background.BackgroundTaskRegistration sender, Windows.ApplicationModel.Background.BackgroundTaskProgressEventArgs args)
        {
            LoadActiveDowloads();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace ExampleBackgroundTask
{
    public sealed class DownloadFilesTask : IBackgroundTask 
    {
        BackgroundTaskDeferral _deferral;

        async void IBackgroundTask.Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += taskInstance_Canceled;
            _deferral = taskInstance.GetDeferral();

            var destinationFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("20MB.zip", CreationCollisionOption.ReplaceExisting);

            //var authInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes("username:password"));
            //downloader.SetRequestHeader("Authorization", string.Format("Basic {0}", authInfo));

            var downloader = new BackgroundDownloader();
            var download = downloader.CreateDownload(new Uri("http://www.wswdsupport.com/testdownloadfiles/20MB.zip"), destinationFile);
            await download.StartAsync();

            _deferral.Complete();
        }

        void taskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            //handle any clean-up that should occur prior to cancellation


            //mark the task as completed
            _deferral.Complete();
        }
    }
}

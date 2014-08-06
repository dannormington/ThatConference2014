using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Notifications;

namespace ExampleBackgroundTask
{
    public sealed class DownloadFilesTask : IBackgroundTask 
    {
        BackgroundTaskDeferral _deferral;

        async void IBackgroundTask.Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += taskInstance_Canceled;
            _deferral = taskInstance.GetDeferral();

            SetBadgeToSync();

            var destinationFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("20MB.zip", CreationCollisionOption.ReplaceExisting);

            var downloader = new BackgroundDownloader();
            var download = downloader.CreateDownload(new Uri("http://www.wswdsupport.com/testdownloadfiles/20MB.zip"), destinationFile);
            await download.StartAsync();

            SetBadgeCount();

            _deferral.Complete();
        }

        void taskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            //handle any clean-up that should occur prior to cancellation


            //mark the task as completed
            _deferral.Complete();
        }

        #region Badge Updates

        private void SetBadgeToSync()
        {
            string badgeXmlString = "<badge value='activity'/>";
            Windows.Data.Xml.Dom.XmlDocument badgeDOM = new Windows.Data.Xml.Dom.XmlDocument();
            badgeDOM.LoadXml(badgeXmlString);
            BadgeNotification badge = new BadgeNotification(badgeDOM);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
        }

        private void SetBadgeCount()
        {
            string badgeXmlString = string.Format("<badge value='{0}'/>", "1");

            Windows.Data.Xml.Dom.XmlDocument badgeDOM = new Windows.Data.Xml.Dom.XmlDocument();
            badgeDOM.LoadXml(badgeXmlString);
            BadgeNotification badge = new BadgeNotification(badgeDOM);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
        }

        public void ClearBadge()
        {
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
        }

        #endregion
    }
}

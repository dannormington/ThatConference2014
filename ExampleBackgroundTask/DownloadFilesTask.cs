﻿using System;
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

            await SetBadgeCountAsync();

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

        private async Task SetBadgeCountAsync()
        {
            var files = await ApplicationData.Current.LocalFolder.GetFilesAsync();

            if (files != null && files.Count > 0)
            {
                string badgeXmlString = string.Format("<badge value='{0}'/>", files.Count.ToString());

                Windows.Data.Xml.Dom.XmlDocument badgeDOM = new Windows.Data.Xml.Dom.XmlDocument();
                badgeDOM.LoadXml(badgeXmlString);
                BadgeNotification badge = new BadgeNotification(badgeDOM);
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
            }
            else 
            {
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
            }
        }

        #endregion
    }
}

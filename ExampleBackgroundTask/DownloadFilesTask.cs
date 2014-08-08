using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
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

            var isAppActive = ApplicationData.Current.LocalSettings.Values["AppIsActive"] as bool?;

            //make sure the app isn't active before sending the toast
            if (!GetIsApplicationActive())
            {
                //send toast
                SendToast();
            }

            _deferral.Complete();
        }

        void taskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            //handle any clean-up that should occur prior to cancellation


            //mark the task as completed
            _deferral.Complete();
        }

        private bool GetIsApplicationActive() 
        {
            using (Mutex mutex = new Mutex(true, "AppIsActiveMutex"))
            {
                mutex.WaitOne();
                try
                {
                    var isActive = ApplicationData.Current.LocalSettings.Values["AppIsActive"] as bool?;

                    return isActive.HasValue && isActive.Value;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        private void SendToast()
        {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode("Welcome to That Conference!"));

            XmlNodeList toastImageAttributes = toastXml.GetElementsByTagName("image");

            ((XmlElement)toastImageAttributes[0]).SetAttribute("src", "ms-appx:///assets/Logo.png");
            ((XmlElement)toastImageAttributes[0]).SetAttribute("alt", "Logo");

            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "long");

            XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", "ms-winsoundevent:Notification.IM");
            toastNode.AppendChild(audio);

            //((XmlElement)toastNode).SetAttribute("launch", "{\"type\":\"toast\",\"param1\":\"12345\",\"param2\":\"67890\"}");

            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

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
    }
}

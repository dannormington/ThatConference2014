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
        CancellationTokenSource _cancellationTokenSource;

        async void IBackgroundTask.Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            try
            {    
                taskInstance.Canceled += taskInstance_Canceled;

                _cancellationTokenSource = new CancellationTokenSource();

                SetBadgeToSync();

                var tasks = new List<Task>();

                tasks.Add(StartDownloadAsync("10MB.zip", _cancellationTokenSource.Token));
                //tasks.Add(StartDownloadAsync("20MB.zip", _cancellationTokenSource.Token));
                //tasks.Add(StartDownloadAsync("50MB.zip", _cancellationTokenSource.Token));
                //tasks.Add(StartDownloadAsync("100MB.zip", _cancellationTokenSource.Token));
                //tasks.Add(StartDownloadAsync("200MB.zip", _cancellationTokenSource.Token));

                //signal that the task has started
                taskInstance.Progress = 1;

                await Task.WhenAll(tasks);

                await SetBadgeCountAsync();

                //make sure the app isn't active before sending the toast
                //windows app store guidelines specify that toasts should be used
                //only when the app is not in the foreground
                if (!GetIsApplicationActive())
                {
                    SendToast();
                }
            }
            catch (Exception)
            {
                if(_cancellationTokenSource != null)
                    _cancellationTokenSource.Cancel();
            }
            finally 
            {
                if(_cancellationTokenSource != null)
                    _cancellationTokenSource.Dispose();

                _deferral.Complete();
            }
        }

        private async Task StartDownloadAsync(string fileName, CancellationToken cancellationToken) 
        {
            var destinationFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            var downloader = new BackgroundDownloader();
            var download = downloader.CreateDownload(new Uri(string.Format("http://www.wswdsupport.com/testdownloadfiles/{0}", fileName)), destinationFile);

            await download.StartAsync().AsTask(cancellationToken);
        }

        void taskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            //handle any clean-up that should occur prior to cancellation
            _cancellationTokenSource.Cancel();
        }

        private bool GetIsApplicationActive() 
        {
            using (Mutex mutex = new Mutex(false, "AppIsActiveMutex"))
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

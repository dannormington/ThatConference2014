using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;

namespace ExampleApplication
{
    public class DownloadProgressViewModel : ViewModel
    {
        private double _progress = 0;
        private string _status = null;
        private DownloadOperation _download = null;

        public DownloadProgressViewModel(DownloadOperation download) 
        {
            this._download = download;

            Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
            this._download.AttachAsync().AsTask(progressCallback);
        }

        private void DownloadProgress(DownloadOperation download)
        {
            this.Status = download.Progress.Status.ToString();

            if (download.Progress.TotalBytesToReceive > 0)
            {
                this.Progress = download.Progress.BytesReceived * 100 / download.Progress.TotalBytesToReceive;
            }

            if (this.Progress >= 100) 
            {
                this.Status = "Completed";
            }
        }

        public double Progress 
        {
            get { return _progress; }
            set { SetProperty(ref _progress, value); }
        }

        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }
    }
}

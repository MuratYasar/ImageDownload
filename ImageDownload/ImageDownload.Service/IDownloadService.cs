using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDownload.Service
{
    public interface IDownloadService
    {
        Task DownloadAndSave(string requestUrl, string destinationFolder, string destinationFileName, CancellationToken token);
    }
}

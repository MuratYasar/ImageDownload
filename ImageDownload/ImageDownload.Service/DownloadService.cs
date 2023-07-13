using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ImageDownload.Service
{
    public class DownloadService : IDownloadService
    {
        private readonly HttpClient _client;

        public DownloadService(HttpClient client)
        {
            _client = client;
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        private async Task<Stream> GetImageStream(string requestUrl, CancellationToken token)
        {
            try
            {
                _client.BaseAddress = new Uri(requestUrl);
                var response = await _client.GetAsync(requestUrl, token).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode) throw new ApplicationException();
                var stream = await response.Content.ReadAsStreamAsync();
                return stream;                
                
            }
            catch (Exception ex)
            {
                return Stream.Null;
            }            
        }

        private async Task SaveStream(Stream fileStream, string destinationFolder, string destinationFileName)
        {
            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);

            string path = Path.Combine(destinationFolder, destinationFileName);

            using (FileStream outputFileStream = new FileStream(path, FileMode.CreateNew))
            {
                await fileStream.CopyToAsync(outputFileStream);
            }
        }

        public async Task DownloadAndSave(string requestUrl, string destinationFolder, string destinationFileName, CancellationToken token)
        {
            Stream fileStream = await GetImageStream(requestUrl, token);

            if (fileStream != Stream.Null)
            {
                await SaveStream(fileStream, destinationFolder, destinationFileName);
            }
        }
    }
}

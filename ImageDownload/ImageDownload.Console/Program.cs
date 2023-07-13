// See https://aka.ms/new-console-template for more information
using ImageDownload.Console;
using ImageDownload.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;



var baseUrls = new BaseUrls();

var host = new HostBuilder()
    .ConfigureHostConfiguration(hostConfig =>
    {
        hostConfig.SetBasePath(Directory.GetCurrentDirectory());
        hostConfig.AddJsonFile("Input.json", true, true);
        hostConfig.AddCommandLine(args);
    })
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
        config.AddJsonFile("appsettings.json", true, true);
    })
    .ConfigureServices((hostingContext, services) =>
    {
        hostingContext.Configuration.GetSection("BaseUrls").Bind(baseUrls);
        
        services.AddHttpClient();
        services.TryAddTransient<IDownloadService, DownloadService>();

        services.AddLogging();
    }
    )
    .UseConsoleLifetime()
    .Build();

string read = string.Empty;
string countOfImageToDownload = string.Empty;
string parallelCount = string.Empty;
string imageUrl = string.Empty;
string savePath = string.Empty;

Console.WriteLine("Please Choose The Image Downloading Type");
Console.WriteLine();
do
{
    Console.WriteLine("1-Use setting file information");
    Console.WriteLine();
    Console.WriteLine("2-Specify information to download");
    Console.WriteLine();
    Console.WriteLine("Please enter 1 or 2 and press enter..");
    Console.WriteLine();
    read = Console.ReadLine() ?? string.Empty;
} while (string.IsNullOrWhiteSpace(read) || int.TryParse(read, out int res) == false);

if (read == "1")
{

}
else
{
    Console.WriteLine("Enter the URL of images (default: https://picsum.photos/200/300):");
    imageUrl = Console.ReadLine() ?? string.Empty;    

    do
    {
        Console.WriteLine("Enter the number of images to download:");
        countOfImageToDownload = Console.ReadLine() ?? string.Empty;
    } while (string.IsNullOrWhiteSpace(countOfImageToDownload) || int.TryParse(countOfImageToDownload, out int res) == false);

    do
    {
        Console.WriteLine("Enter the maximum parallel download limit:");
        parallelCount = Console.ReadLine() ?? string.Empty;
    } while (string.IsNullOrWhiteSpace(parallelCount) || int.TryParse(parallelCount, out int res) == false);    
    
    Console.WriteLine("Enter the save path (default: outputs):");
    savePath = Console.ReadLine() ?? string.Empty;

    Console.WriteLine("Starting download..");

    if (string.IsNullOrWhiteSpace(savePath))
        savePath = "outputs";

    if (string.IsNullOrWhiteSpace(imageUrl))
        imageUrl = "https://picsum.photos/200/300 ";
}


Console.WriteLine("To terminate the example, press 'CTRL+C' to cancel and exit...");
Console.WriteLine();


var cts = new CancellationTokenSource();

Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine();
    Console.WriteLine("Please wait! Canceling...");
    Console.WriteLine();
    cts.Cancel();
    e.Cancel = true;
};

var currentFolderName = DateTime.Now.ToString("yyyy.mm.dd_HH.mm.ss.fff");

try
{
    int myNumberOfConcurrentOperations = read == "1" ? baseUrls.Parallelism : Convert.ToInt32(parallelCount);
    var mySemaphore = new SemaphoreSlim(myNumberOfConcurrentOperations);
    var tasks = new List<Task>();

    Console.WriteLine("Downloading started..");
    Console.WriteLine("Downloading {0} images ({1} parallel downloads at most)", (read == "1" ? baseUrls.Count : Convert.ToInt32(countOfImageToDownload)).ToString(), read == "1" ? baseUrls.Parallelism.ToString() : parallelCount);

    ConsoleUtility.WriteProgressBar(0, 0);

    var watch = Stopwatch.StartNew();
    
    for (int i = 0; i < (read == "1" ? baseUrls.Count : Convert.ToInt32(countOfImageToDownload)); i++)
    {
        if (cts.IsCancellationRequested)
            break;

        var downloadService = host.Services.GetRequiredService<IDownloadService>();

        ConsoleUtility.WriteProgressBar(i+1, (read == "1" ? baseUrls.Count : Convert.ToInt32(countOfImageToDownload)), true);

        await mySemaphore.WaitAsync();
        var task = downloadService.DownloadAndSave(
            (read == "1" ? baseUrls.UrlName : imageUrl),
            Path.Combine(Directory.GetCurrentDirectory(), (read == "1" ? baseUrls.SavePath : savePath), currentFolderName),
            (i + 1).ToString() + ".png",
            cts.Token
            );

        tasks.Add(task);
        await task.ContinueWith(t => mySemaphore.Release());
    }

    await Task.WhenAll(tasks);

    watch.Stop();

    if (cts.IsCancellationRequested)
    {
        Console.WriteLine();
        Console.WriteLine("Downloading cancelled!!");
        Console.WriteLine();

        if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), (read == "1" ? baseUrls.SavePath : savePath), currentFolderName)))
        {
            try
            {
                Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), (read == "1" ? baseUrls.SavePath : savePath), currentFolderName), true);
                Console.WriteLine("Folder and its content has been deleted... ({0})", Path.Combine(Directory.GetCurrentDirectory(), (read == "1" ? baseUrls.SavePath : savePath), currentFolderName));
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
    else
    {
        Console.WriteLine();
        Console.WriteLine("Downloading finished..");
        Console.WriteLine("A total of {0} files were downloaded under the folder ({1}). ", (read == "1" ? baseUrls.Count : Convert.ToInt32(countOfImageToDownload)).ToString(), Path.Combine(Directory.GetCurrentDirectory(), (read == "1" ? baseUrls.SavePath : savePath), currentFolderName));
    }    
    
    Console.WriteLine("Total time in seconds: " + watch.Elapsed.TotalSeconds);
    Console.WriteLine();
}
catch (OperationCanceledException e)
{
    Console.WriteLine(e.Message);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}
finally
{
    cts.Dispose();
    cts = null;
}

Console.WriteLine("Press enter to exit..");
Console.Read();
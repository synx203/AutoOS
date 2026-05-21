using AutoOS.Core.Common;
using DevWinUI;
using Downloader;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace AutoOS.Core.Helpers.Download;

public static partial class DownloadHelper
{
	private static readonly HttpClient httpClient = new()
	{
		DefaultRequestHeaders =
		{
			UserAgent =
			{
				new ProductInfoHeaderValue("AutoOS", ProcessInfoHelper.Version)
			}
		}
	};

	public static async Task Download(string url, string path, string file = null, IStatusReporter reporter = null)
	{
		if (url.Contains("raw.githubusercontent.com", StringComparison.OrdinalIgnoreCase))
		{
			string destination = string.IsNullOrWhiteSpace(file) ? path : Path.Combine(path, file);
			await File.WriteAllTextAsync(destination, await httpClient.GetStringAsync(url), Encoding.UTF8);
			reporter?.Report(progress: 100);
			return;
		}

		DownloadConfiguration config = new()
		{
			MaxTryAgainOnFailure = 5,
			EnableAutoResumeDownload = false,
			ParallelDownload = true,
			ChunkCount = 8,
			ParallelCount = 4,
			RequestConfiguration = new RequestConfiguration
			{
				UserAgent = $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 AutoOS/{ProcessInfoHelper.Version}"
			}
		};

		if (url.Contains("www2.ati.com", StringComparison.OrdinalIgnoreCase))
		{
			config.RequestConfiguration = new RequestConfiguration
			{
				Headers = new WebHeaderCollection
				{
					{ "Referer", "http://support.amd.com" },
					{ "Accept", "*/*" },
					{ "User-Agent", "AMD Catalyst Install Manager/0.0" },
					{ "Cache-Control", "no-cache" },
					{ "Connection", "Keep-Alive" }
				}
			};
		}

		var downloadBuilder = DownloadBuilder.New()
			.WithUrl(url)
			.WithDirectory(path)
			.WithFileName(file)
			.WithConfiguration(config);

		var download = downloadBuilder.Build();

		DateTime lastLoggedTime = DateTime.MinValue;
		double lastSpeedMB = 0;
		double totalSizeMB = 0;

		download.DownloadProgressChanged += (sender, e) =>
		{
			if ((DateTime.Now - lastLoggedTime).TotalMilliseconds < 100) return;
			lastLoggedTime = DateTime.Now;

			lastSpeedMB = e.BytesPerSecondSpeed / (1024.0 * 1024.0);
			double receivedMB = e.ReceivedBytesSize / (1024.0 * 1024.0);
			totalSizeMB = e.TotalBytesToReceive / (1024.0 * 1024.0);
			double percentage = e.ProgressPercentage;

			reporter?.Report($"{lastSpeedMB:F1} MB/s - {receivedMB:F2} MB of {totalSizeMB:F2} MB", percentage, false);
		};

		download.DownloadFileCompleted += (sender, e) =>
		{
			if (e.Error == null)
			{
				reporter?.Report($"{lastSpeedMB:F1} MB/s - {totalSizeMB:F2} MB of {totalSizeMB:F2} MB", 100, false);
			}
		};

		await download.StartAsync();

		string fileName = download.Package?.FileName ?? (!string.IsNullOrEmpty(file) ? Path.Combine(path, file) : null);
		if (!File.Exists(fileName))
		{
			throw new FileNotFoundException("Downloaded file not found.");
		}

		reporter?.Report(progress: 100, isIndeterminate: true);
	}
}
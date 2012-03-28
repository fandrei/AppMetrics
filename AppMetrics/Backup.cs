using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Amazon.S3;
using Amazon.S3.Model;
using Ionic.Zip;
using Ionic.Zlib;

namespace AppMetrics
{
	public static class Backup
	{
		public static void BackupAll(ReportLogDelegate reportLog)
		{
			try
			{
				{
					var now = DateTime.UtcNow;
					var sessions = DataModel.DataSource.GetSessionsFromPath(AppSettings.DataStoragePath, TimeSpan.MaxValue);

					foreach (var session in sessions)
					{
						if (now - session.LastUpdateTime < NonArchivePeriod)
							continue;

						try
						{
							ArchiveFile(session.FileName);
						}
						catch (Exception exc)
						{
							reportLog(exc);
						}
					}
				}

				SyncAllToS3();
			}
			catch (Exception exc)
			{
				reportLog(exc);
			}
		}

		public static void ArchiveFile(string fileName)
		{
			var zipFileName = Path.ChangeExtension(fileName, ".zip");
			using (var zipFile = new ZipFile(zipFileName))
			{
				zipFile.RemoveEntries(zipFile.Entries.ToArray());

				zipFile.CompressionMethod = CompressionMethod.Deflate;
				zipFile.CompressionLevel = CompressionLevel.BestCompression;
				zipFile.AddFile(fileName, ".");

				zipFile.Save();
			}

			File.Delete(fileName);
		}

		static void SyncAllToS3()
		{
			if (string.IsNullOrEmpty(AppSettings.Instance.AmazonAccessKey) ||
					string.IsNullOrEmpty(AppSettings.Instance.AmazonSecretAccessKey))
				return;

			using (var client = CreateAmazonS3Client())
			{
				var storedFiles = client.ListObjects(new ListObjectsRequest { BucketName = AppMetricsBucketName });
				var storedFilesDic = storedFiles.S3Objects.ToDictionary(storedObject => storedObject.Key);

				foreach (var filePath in Directory.GetFiles(AppSettings.DataStoragePath, "*.zip", SearchOption.AllDirectories))
				{
					var key = Path.GetFileName(filePath);
					S3Object storedObject;
					if (storedFilesDic.TryGetValue(key, out storedObject))
					{
						var remoteLastModified = DateTime.Parse(storedObject.LastModified).ToUniversalTime();
						var localFileInfo = new FileInfo(filePath);
						if (localFileInfo.LastWriteTimeUtc <= remoteLastModified)
							continue;
					}

					SendFileToS3(client, filePath);
				}
			}
		}

		static void SendFileToS3(AmazonS3Client client, string fileName)
		{
			using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				var key = Path.GetFileName(fileName);

				client.PutObject(
					new PutObjectRequest
						{
							BucketName = AppMetricsBucketName,
							Key = key,
							InputStream = stream,
							ContentType = "application/zip"
						});
			}
		}

		static AmazonS3Client CreateAmazonS3Client()
		{
			return new AmazonS3Client(AppSettings.Instance.AmazonAccessKey, AppSettings.Instance.AmazonSecretAccessKey);
		}

		private const string AppMetricsBucketName = "CityIndex.AppMetrics";
		private static readonly TimeSpan NonArchivePeriod = TimeSpan.FromDays(7);
	}
}
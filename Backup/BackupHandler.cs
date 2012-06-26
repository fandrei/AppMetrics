using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Amazon.S3;
using Amazon.S3.Model;
using Ionic.Zip;
using Ionic.Zlib;

namespace AppMetrics.Backup
{
	public static class BackupHandler
	{
		public static void BackupAll(string dataStoragePath, ReportLogDelegate reportLog)
		{
			try
			{
				dataStoragePath = Path.GetFullPath(dataStoragePath); // normalize path

				{
					var now = DateTime.UtcNow;
					var sessions = DataReader.GetSessionsFromPath(dataStoragePath, TimePeriod.Unlimited);

					foreach (var session in sessions)
					{
						if (now - session.LastUpdateTime < NonArchivePeriod)
							continue;

						try
						{
							reportLog(string.Format("Archiving {0}", session.FileName));
							ArchiveFile(session.FileName);
						}
						catch (Exception exc)
						{
							reportLog(exc);
						}
					}
				}

				MoveOldZipFiles(dataStoragePath);
				SyncAllToS3(dataStoragePath, reportLog);

				reportLog("Finished ok");
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
				zipFile.TempFileFolder = Path.GetTempPath();

				zipFile.RemoveEntries(zipFile.Entries.ToArray());

				zipFile.CompressionMethod = CompressionMethod.Deflate;
				zipFile.CompressionLevel = CompressionLevel.BestCompression;
				zipFile.AddFile(fileName, ".");

				zipFile.Save();
			}

			File.Delete(fileName);
		}

		static void MoveOldZipFiles(string dataStoragePath)
		{
			foreach (var dirPath in Directory.GetDirectories(dataStoragePath))
			{
				foreach (var filePath in Directory.GetFiles(dirPath, "*.zip", SearchOption.TopDirectoryOnly))
				{
					var newPath = Path.Combine(dirPath, GetHierarchyName(filePath));
					var newDir = Path.GetDirectoryName(newPath);
					if (!Directory.Exists(newDir))
						Directory.CreateDirectory(newDir);
					File.Move(filePath, newPath);
				}
			}
		}

		static void SyncAllToS3(string dataStoragePath, ReportLogDelegate reportLog)
		{
			if (string.IsNullOrEmpty(AppSettings.Instance.AmazonAccessKey) ||
					string.IsNullOrEmpty(AppSettings.Instance.AmazonSecretAccessKey))
				return;

			using (var client = CreateAmazonS3Client())
			{
				var storedFiles = ListS3Objects(client, new ListObjectsRequest { BucketName = AppMetricsBucketName });
				var storedFilesDic = storedFiles.ToDictionary(storedObject => storedObject.Key);

				foreach (var filePath in Directory.GetFiles(dataStoragePath, "*.zip", SearchOption.AllDirectories))
				{
					var storageKey = GetStorageKey(dataStoragePath, filePath);

					S3Object storedObject;
					if (storedFilesDic.TryGetValue(storageKey, out storedObject))
					{
						var remoteLastModified = DateTime.Parse(storedObject.LastModified).ToUniversalTime();
						var localFileInfo = new FileInfo(filePath);
						if (localFileInfo.LastWriteTimeUtc <= remoteLastModified)
							continue;
					}

					reportLog(string.Format("Sending {0}", filePath));
					SendFileToS3(client, filePath, storageKey);
				}
			}
		}

		static void SendFileToS3(AmazonS3Client client, string fileName, string storageKey)
		{
			using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				client.PutObject(
					new PutObjectRequest
						{
							BucketName = AppMetricsBucketName,
							Key = storageKey,
							InputStream = stream,
							ContentType = "application/zip"
						});
			}
		}

		static List<S3Object> ListS3Objects(AmazonS3Client client, ListObjectsRequest request)
		{
			var res = new List<S3Object>(1000);

			while (true)
			{
				var response = client.ListObjects(request);

				res.AddRange(response.S3Objects);

				if (response.IsTruncated)
					request.Marker = response.NextMarker;
				else
					break;
			}

			return res;
		}

		private static string GetStorageKey(string dataStoragePath, string filePath)
		{
			if (!filePath.StartsWith(dataStoragePath))
				throw new ApplicationException();
			var res = filePath.Substring(dataStoragePath.Length);
			res = res.Replace('\\', '/');
			return res;
		}

		private static string GetHierarchyName(string filePath)
		{
			var fileName = Path.GetFileName(filePath);

			var nameParts = fileName.Split('.');
			var fullTime = nameParts.First();
			var timeParts = fullTime.Split(' ');
			var date = timeParts.First();
			var dateParts = date.Split('-');
			var yearMonth = dateParts[0] + "-" + dateParts[1];
			var day = dateParts.Last();

			return yearMonth + "/" + day + "/" + fileName;
		}

		static AmazonS3Client CreateAmazonS3Client()
		{
			return new AmazonS3Client(AppSettings.Instance.AmazonAccessKey, AppSettings.Instance.AmazonSecretAccessKey);
		}

		private const string AppMetricsBucketName = "cityindex.appmetrics";
		private static readonly TimeSpan NonArchivePeriod = TimeSpan.FromDays(7);
	}
}
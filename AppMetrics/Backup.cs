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
				var now = DateTime.UtcNow;
				var sessions = DataModel.DataSource.GetSessionsFromPath(AppSettings.DataStoragePath, TimeSpan.MaxValue);

				foreach (var session in sessions)
				{
					if (now - session.LastUpdateTime < NonArchivePeriod)
						continue;

					try
					{
						BackupFile(session.FileName);
					}
					catch (Exception exc)
					{
						reportLog(exc);
					}
				}
			}
			catch (Exception exc)
			{
				reportLog(exc);
			}
		}

		static void BackupFile(string fileName)
		{
			var zipFile = ArchiveFile(fileName);
			SendFileToS3(zipFile);

			//File.Delete(fileName);
		}

		public static string ArchiveFile(string fileName)
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
			return zipFileName;
		}

		public static void SendFileToS3(string fileName)
		{
			if (string.IsNullOrEmpty(AppSettings.Instance.AmazonAccessKey) || 
					string.IsNullOrEmpty(AppSettings.Instance.AmazonSecretAccessKey))
				return;

			using (var client = new AmazonS3Client(AppSettings.Instance.AmazonAccessKey, AppSettings.Instance.AmazonSecretAccessKey))
			{
				using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
				{
					var key = Path.GetFileName(fileName);

					client.PutObject(
						new PutObjectRequest
							{
								BucketName = BucketName,
								Key = key,
								InputStream = stream,
								ContentType = "application/zip"
							});
				}
			}
		}

		private const string BucketName = "CityIndex.AppMetrics";
		private static readonly TimeSpan NonArchivePeriod = TimeSpan.FromDays(7);
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Amazon.S3;
using Amazon.S3.Model;
using Ionic.Zip;
using Ionic.Zlib;

using AppMetrics.Shared;

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

		private static readonly TimeSpan NonArchivePeriod = TimeSpan.FromDays(60);
	}
}
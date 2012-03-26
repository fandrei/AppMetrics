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
			using (var client = new AmazonS3Client(AppSettings.AmazonAccessKey, AppSettings.AmazonSecretAccessKey))
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
	}
}
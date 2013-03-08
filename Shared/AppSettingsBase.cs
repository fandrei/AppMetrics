using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace AppMetrics.Shared
{
	public abstract class AppSettingsBase
	{
		protected virtual void OnAfterLoad()
		{ }

		protected virtual void OnBeforeSave()
		{ }

		private string _fileName;

		public static T Load<T>(string fileName)
			where T : AppSettingsBase, new()
		{
			T settings;

			if (File.Exists(fileName))
			{
				// http://blogs.msdn.com/b/tess/archive/2006/02/15/532804.aspx
				// beware of memory leak if you modify the next line
				var serializer = new XmlSerializer(typeof(T));
				using (var rd = new StreamReader(fileName))
				{
					settings = (T)serializer.Deserialize(rd);
				}
			}
			else
				settings = new T();

			settings._fileName = fileName;

			settings.OnAfterLoad();

			return settings;
		}

		public void Save(string fileName)
		{
			OnBeforeSave();

			var directory = Path.GetDirectoryName(fileName);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			var serializer = new XmlSerializer(GetType());
			using (var writer = new StreamWriter(fileName))
			{
				serializer.Serialize(writer, this);
			}
		}

		public void Save()
		{
			Save(_fileName);
		}
	}
}

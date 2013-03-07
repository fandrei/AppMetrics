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

		private static XmlSerializer _serializer;
		private string _fileName;

		public static T Load<T>(string fileName)
			where T : AppSettingsBase, new()
		{
			T settings;

			if (File.Exists(fileName))
			{
				var rootAttr = new XmlRootAttribute("AppSettings");
				if (_serializer == null)
				{
					_serializer = new XmlSerializer(typeof(T), null, null, rootAttr, "");
				}
				using (var rd = new StreamReader(fileName))
				{
					settings = (T)_serializer.Deserialize(rd);
				}
			}
			else
				settings = new T();

			settings.OnAfterLoad();
			settings._fileName = fileName;

			return settings;
		}

		public void Save(string fileName)
		{
			OnBeforeSave();

			var directory = Path.GetDirectoryName(fileName);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			var s = new XmlSerializer(GetType());
			using (var writer = new StreamWriter(fileName))
			{
				s.Serialize(writer, this);
			}
		}

		public void Save()
		{
			Save(_fileName);
		}
	}
}

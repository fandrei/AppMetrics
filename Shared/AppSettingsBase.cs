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

		private static readonly object Sync = new object();
		private static readonly Dictionary<Tuple<Type, string>, XmlSerializer> Serializers = new Dictionary<Tuple<Type, string>, XmlSerializer>();

		private string _fileName;

		public static T Load<T>(string fileName, string rootName = null)
			where T : AppSettingsBase, new()
		{
			T settings;

			if (File.Exists(fileName))
			{
				lock (Sync)
				{
					var serializer = GetSerializer(typeof(T), rootName);
					using (var rd = new StreamReader(fileName))
					{
						settings = (T)serializer.Deserialize(rd);
					}
				}
			}
			else
				settings = new T();

			settings._fileName = fileName;

			settings.OnAfterLoad();

			return settings;
		}

		public void Save(string fileName, string rootName = null)
		{
			OnBeforeSave();

			var directory = Path.GetDirectoryName(fileName);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			lock (Sync)
			{
				var serializer = GetSerializer(GetType(), rootName);
				using (var writer = new StreamWriter(fileName))
				{
					serializer.Serialize(writer, this);
				}
			}
		}

		// cache serializers to prevent memory leak
		// http://blogs.msdn.com/b/tess/archive/2006/02/15/532804.aspx
		static XmlSerializer GetSerializer(Type type, string rootName)
		{
			lock (Sync)
			{
				if (rootName == null)
					rootName = type.Name;
				var key = new Tuple<Type, string>(type, rootName);
				XmlSerializer res;
				if (!Serializers.TryGetValue(key, out res))
				{
					var rootAttr = new XmlRootAttribute(rootName);
					res = new XmlSerializer(type, null, null, rootAttr, "");
					Serializers.Add(key, res);
				}
				return res;
			}
		}

		public void Save()
		{
			Save(_fileName);
		}
	}
}

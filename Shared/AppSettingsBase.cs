﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace AppMetrics.Shared
{
	public class AppSettingsBase
	{
		private static XmlSerializer _serializer;

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

			return settings;
		}
	}
}
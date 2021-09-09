using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using log4net;
using log4net.Config;

namespace dtest
{
	class Logger
	{
		public static Logger inst = new Logger();
		public static Logger GetInstance()
		{
			return inst;
		}

		private static ILog logError = null;
		private static ILog logInfo = null;
		private static ILog logDebug = null;
		public Logger()
		{

		}

		public void Init()
		{
			FileInfo file = new FileInfo(".\\log4net.config");
			// LogManager에 주입한다.
			XmlConfigurator.Configure(file);

			logError = LogManager.GetLogger("Error");
			logInfo = LogManager.GetLogger("General");
			logDebug = LogManager.GetLogger("Debug");
		}
				
		public static void Error(string log) => logError.Error(log);
		public static void Info(string log) => logInfo.Info(log);
		public static void Debug(string log) => logDebug.Debug(log);
	}
}

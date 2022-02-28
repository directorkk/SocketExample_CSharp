using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LouieTool
{
	class Util
	{
		static string LogPath = "";
		static Mutex MutexWriteLog = new Mutex();

		static public void OutputDebugMessage(string Message)
		{
			if (string.IsNullOrWhiteSpace(LogPath))
			{
				string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
				string logName = ConfigurationSettings.AppSettings["LogName"];
                if (string.IsNullOrWhiteSpace(logName))
                {
                    return;
                }
				LogPath = Path.Combine(exePath, logName);
			}

			MutexWriteLog.WaitOne();
			using (StreamWriter sw = new StreamWriter(LogPath, true))
			{
				string.Format("{0}\n{1}", DateTime.Now.ToString(), Message);
				sw.WriteLine(Message);
				sw.Flush();
			}
			MutexWriteLog.ReleaseMutex();
		}
        

    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using InterfaceSample;

namespace ConsoleSelfUpdateSample
{
	class Program
	{
		private const string asm = "SerializableClassSample";
		private const string dll = asm + ".dll";
		private const string cls = "SerializableClassSample.SerializableMain";

		private const string asm2 = "MarshalByRefObjectClassSample";
		private const string dll2 = asm2 + ".dll";
		private const string cls2 = "MarshalByRefObjectClassSample.MarshalByRefObjectMain";

		static readonly object syncObj = new object();
		private static readonly List<AppDomain> old = new List<AppDomain>();
		private static AppDomain current;

		static IMain GetComponent()
		{
			lock (syncObj)
			{
				if (current == null)
					return null;
				return current.CreateInstanceAndUnwrap(asm, cls) as IMain;
			}
		}

		static void SetupAppDomain()
		{
			lock (syncObj)
			{
				if (File.Exists(dll)||File.Exists(dll2))
				{
					AppDomainSetup setup = new AppDomainSetup();
					setup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
					// DLLは自由に削除したいのでシャドーコピーを使う
					setup.ShadowCopyFiles = "true";
					// 古いAppDomainはどこかでUnloadさせる(今回は手抜き)
					if (current != null)
						old.Add(current);
					current = AppDomain.CreateDomain("AD#1", null, setup);
				}
			}
		}

		static void Main(string[] args)
		{
			var bQuit = false;
			var task = Task.Factory.StartNew(() =>
			{
				// DLLを監視
				var path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
				Console.WriteLine(path);
				FileSystemWatcher fsw = new FileSystemWatcher(path);
				fsw.Filter = "*.dll";
				fsw.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
				fsw.Changed += Changed;
				fsw.Created += Changed;
				fsw.EnableRaisingEvents = true;

				while (!bQuit)
				{
					IMain greeting = GetComponent();
					if (greeting != null)
						Console.WriteLine(greeting.Message);
					else
						Console.WriteLine("need " + dll + ".");
					Thread.Sleep(1000);
				}
				foreach (var domain in old)
				{
					AppDomain.Unload(domain);
				}
			});

			Console.WriteLine("'Q'キーで終了します。");
			while (Console.ReadKey().Key != ConsoleKey.Q)
			{
				Thread.Sleep(100);
			}
			bQuit = true;
			task.Wait();
		}

		static void Changed(object sender, FileSystemEventArgs e)
		{
			SetupAppDomain();
		}
	}
}

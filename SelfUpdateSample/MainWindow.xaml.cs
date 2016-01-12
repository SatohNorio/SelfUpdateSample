using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Xml.Serialization;
using System.Diagnostics.CodeAnalysis;

using InterfaceSample;

namespace SelfUpdateSample
{
	public class TestClass : MarshalByRefObject
	{
		public Type AssemblyType { get; set; }

		public Type GetAssemblyType()
		{
			// シャドウコピーされたアセンブリを読み込む
			var asm = Assembly.LoadFrom(MainWindow.DllName);
			foreach (var t in asm.GetExportedTypes())
			{
				if (typeof(IMain).IsAssignableFrom(t) && t.IsClass)
				{
					return t;
				}
			}
			return null;
		}

		public string GetAssemblyName()
		{
			// シャドウコピーされたアセンブリを読み込む
			var asm = Assembly.LoadFrom(MainWindow.DllName);
			foreach (var t in asm.GetExportedTypes())
			{
				if (typeof(IMain).IsAssignableFrom(t) && t.IsClass)
				{
					return t.Assembly.FullName;
				}
			}
			return null;
		}

		public string GetClassName()
		{
			// シャドウコピーされたアセンブリを読み込む
			var asm = Assembly.LoadFrom(MainWindow.DllName);
			foreach (var t in asm.GetExportedTypes())
			{
				if (typeof(IMain).IsAssignableFrom(t) && t.IsClass)
				{
					return t.FullName;
				}
			}
			return null;
		}
	}

	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		private string FAssemblyName;
		private string FClassName;
		private IMain FMain;

		public MainWindow()
		{
			InitializeComponent();

			MainWindow.SetupAppDomain();

			// Type を取得すると自動的にコピー元のアセンブリに戻ってしまうため、文字列で取得する。
			var t = this.GetType();
			var tc = current.CreateInstanceAndUnwrap(t.Assembly.FullName, "SelfUpdateSample.TestClass") as TestClass;
			if (tc != null)
			{
				//MainWindow.FType = tc.GetAssemblyType();
				this.FAssemblyName = tc.GetAssemblyName();
				this.FClassName = tc.GetClassName();
			}

			// 向こう側の AppDomain で実行された値をこちら側で受け取ることができない。
			// ※ コールバックメソッドから抜けると null に戻る。
			//current.DoCallBack(new CrossAppDomainDelegate(() =>
			//{
			//	// シャドウコピーされたアセンブリを読み込む
			//	var asm = Assembly.LoadFrom(MainWindow.FDllName);
			//	foreach (var t in asm.GetExportedTypes())
			//	{
			//		if (typeof(IMain).IsAssignableFrom(t) && t.IsClass)
			//		{
			//			MainWindow.FType = t;
			//			MainWindow.FAssemblyName = t.Assembly.FullName;
			//			break;
			//		}
			//	}
			//}));

			// アセンブリを掴んでしまい、更新できなくなるので使用できない。
			//var asm = Assembly.LoadFrom(MainWindow.FDllName);
			//foreach (var t in asm.GetExportedTypes())
			//{
			//	if (typeof(IMain).IsAssignableFrom(t) && t.IsClass)
			//	{
			//		MainWindow.FType = t;
			//		break;
			//	}
			//}

			// DLLを監視
			var path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
			Console.WriteLine(path);
			FileSystemWatcher fsw = new FileSystemWatcher(path);
			fsw.Filter = "*.dll";
			fsw.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
			fsw.Changed += Changed;
			fsw.Created += Changed;
			fsw.EnableRaisingEvents = true;

			// タイマを作成
			var tm = new DispatcherTimer();
			tm.Interval = new TimeSpan(0, 0, 1);
			tm.IsEnabled = true;
			tm.Tick += (sender, e) =>
			{
				//IMain greeting = this.GetComponent();
				IMain greeting = this.FMain;
				if (greeting != null)
				{
					this.listBox.Items.Insert(0, greeting.Message);
				}
			};
			this.FTimer = tm;
		}

		public const string DllName = "MarshalByRefObjectClassSample.dll";
		DispatcherTimer FTimer;
		static Type FType;
		static readonly object syncObj = new object();
		private static readonly List<AppDomain> old = new List<AppDomain>();
		private static AppDomain current;

		private IMain GetComponent()
		{
			lock (syncObj)
			{
				if (current == null)
					return null;
				if (!String.IsNullOrEmpty(this.FAssemblyName) && !String.IsNullOrEmpty(this.FClassName))
				{
					return current.CreateInstanceAndUnwrap(this.FAssemblyName, this.FClassName) as IMain;
				}
				else
				{
					return null;
				}
				//return current.CreateInstanceAndUnwrap("MarshalByRefObjectClassSample", "MarshalByRefObjectClassSample.MarshalByRefObjectMain") as IMain;
			}
		}

		static void SetupAppDomain()
		{
			lock (syncObj)
			{
				if (File.Exists(MainWindow.DllName))
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

		void Changed(object sender, FileSystemEventArgs e)
		{
			SetupAppDomain();
			this.FMain = current.CreateInstanceAndUnwrap(this.FAssemblyName, this.FClassName) as IMain;
			if (this.FMain != null)
			{
				this.FMain.Run();
			}
		}
	}
}

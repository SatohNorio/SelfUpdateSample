using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Globalization;
using System.Reflection;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using InterfaceSample;

namespace SelfUpdateSample
{
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application
	{
		// ------------------------------------------------------------------------------------------------------------
		#region コンストラクタ

		/// <summary>
		/// SelfUpdateSample.App クラスの新しいインスタンスを作成します。
		/// </summary>
		public App()
		{
		}

		#endregion // コンストラクタ
		// ------------------------------------------------------------------------------------------------------------
		// ------------------------------------------------------------------------------------------------------------
		#region 起動・終了処理

		/// <summary>
		/// アプリケーション起動時に実行する処理を定義します。
		/// </summary>
		/// <param name="e">スタートアップパラメータを含む、イベント引数を指定します。</param>
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			var domain = App.CreateAssemblyDomain();
			if (domain == null)
			{
				MessageBox.Show("アセンブリ情報取得用 AppDomain を作成できません。アプリケーションを終了します。");
				this.Shutdown();
				return;
			}
			App.FAssemblyDomain = domain;

			var info = App.CreateAssemblyInfo(App.DllName, App.TargetType);
			if (info == null)
			{
				MessageBox.Show("アセンブリ情報取得オブジェクトの作成に失敗しました。アプリケーションを終了します。");
				this.Shutdown();
				return;
			}
			App.FAssemblyInfo = info;

			var target = App.CreateTarget<IMain>();
			if (target == null)
			{
				MessageBox.Show("アプリケーションの初期化に失敗しました。アプリケーションを終了します。");
				this.Shutdown();
			}

			// InterfaceSample.dll の更新を監視する。
			var path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
			Trace.WriteLine(path);
			FileSystemWatcher fsw = new FileSystemWatcher(path);
			fsw.Filter = App.DllName;
			fsw.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
			fsw.Changed += this.LibraryChanged;
			fsw.Created += this.LibraryChanged;
			fsw.EnableRaisingEvents = true;

			var w = new MainWindow();
			w.Main = target;
			w.Show();
			App.FMainWIndow = w;
		}

		/// <summary>
		/// アプリケーション終了時に実行する処理を定義します。
		/// </summary>
		/// <param name="e">終了コードを含む、イベント引数を指定します。</param>
		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
			if (App.FAssemblyDomain != null)
			{
				AppDomain.Unload(App.FAssemblyDomain);
				App.FAssemblyDomain = null;
			}
			if (App.FCurrentDomain != null)
			{
				AppDomain.Unload(App.FCurrentDomain);
				App.FCurrentDomain = null;
			}
		}

		#endregion // 起動・終了処理
		// ------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// 扱う DLL 名を管理します。
		/// </summary>
		private static readonly string DllName = "MarshalByRefObjectClassSample.dll";

		/// <summary>
		/// 扱うオブジェクトの型を管理します。
		/// </summary>
		private static readonly Type TargetType = typeof(IMain);

		/// <summary>
		/// アセンブリ情報取得用の AppDomain を管理します。
		/// </summary>
		private static AppDomain FAssemblyDomain = null;

		/// <summary>
		/// 現在実行中の AppDomain を管理します。
		/// </summary>
		private static AppDomain FCurrentDomain = null;

		/// <summary>
		/// 作成した AppDomain で実行するアセンブリ情報 を管理します。
		/// </summary>
		private static AssemblyInfo FAssemblyInfo = null;

		/// <summary>
		/// メイン画面を管理します。
		/// </summary>
		private static MainWindow FMainWIndow = null;

		/// <summary>
		/// 排他制御用オブジェクトを管理します。
		/// </summary>
		private object FSyncObj = new object();

		/// <summary>
		/// アセンブリ情報取得用 AppDomain を作成します。
		/// </summary>
		/// <returns>作成した AppDomain を返します。</returns>
		private static AppDomain CreateAssemblyDomain()
		{
			var t = typeof(AssemblyInfo);
			return App.CreateAppDomain(t.ToString());
		}

		/// <summary>
		/// アセンブリ情報取得オブジェクトを作成します。
		/// </summary>
		/// <param name="dllName">取得する DLL 名を指定します。</param>
		/// <param name="targetType">取得する型を指定します。</param>
		/// <returns>作成したオブジェクトを返します。</returns>
		private static AssemblyInfo CreateAssemblyInfo(string dllName, Type targetType)
		{
			var t = typeof(AssemblyInfo);
			var domain = App.FAssemblyDomain;

			// アセンブリ情報を、作成した AppDomain から取得するためのオブジェクトを作成
			var ai = domain.CreateInstanceAndUnwrap(t.Assembly.FullName, t.ToString()) as AssemblyInfo;
			if (ai != null)
			{
				ai.MyDllName = dllName;
				ai.TargetType = targetType;
				ai.ReadAssemblyInfo();
			}
			return ai;
		}

		/// <summary>
		/// AppDomain を取得します。
		/// </summary>
		/// <param name="path">呼び出すアセンブリのファイル名を指定します。</param>
		/// <returns>取得した AppDomain を返します。</returns>
		private static AppDomain CreateAppDomain(string path)
		{
			var setup = new AppDomainSetup();
			setup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
			// DLL を自由に削除したいのでシャドウコピーを使う
			setup.ShadowCopyFiles = "true";
			return AppDomain.CreateDomain(Path.GetFileNameWithoutExtension(path), null, setup);
		}

		/// <summary>
		/// ファイルシステムの変更を通知するイベントを処理します。
		/// </summary>
		/// <param name="sender">イベントを送信したオブジェクトを指定します。</param>
		/// <param name="e">ファイルシステム情報を含む、イベント引数を指定します。</param>
		private void LibraryChanged(object sender, FileSystemEventArgs e)
		{
			var target = App.CreateTarget<IMain>();
			if (target != null)
			{
				App.FMainWIndow.Main = target;
			}
		}

		/// <summary>
		/// 追加 AppDomain で実行するオブジェクトを作成します。
		/// </summary>
		/// <returns>アプリケーションを開始できたら true を返します。</returns>
		private static T CreateTarget<T>() where T : class
		{
			T rtObject = null;

			// 目的のアセンブリ情報を取得
			var info = App.FAssemblyInfo;
			if (info != null && info.HasInfo)
			{
				var oldDomain = App.FCurrentDomain;
				var newDomain = App.CreateAppDomain(info.MyDllName);
				rtObject = newDomain.CreateInstanceAndUnwrap(info.AssemblyName, info.ClassName) as T;
				App.FCurrentDomain = newDomain;
				if (oldDomain != null)
				{
					AppDomain.Unload(oldDomain);
				}
			}
			return rtObject;
		}
	}
}

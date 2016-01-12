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
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			// タイマを作成
			var tm = new DispatcherTimer();
			tm.Interval = new TimeSpan(0, 0, 1);
			tm.IsEnabled = true;
			tm.Tick += (sender, e) =>
			{
				IMain greeting = this.Main;
				if (greeting != null)
				{
					this.listBox.Items.Insert(0, greeting.Message);
				}
			};
			this.FTimer = tm;
		}

		/// <summary>
		/// タイマを管理します。
		/// </summary>
		private DispatcherTimer FTimer;

		/// <summary>
		/// 動的にロードしたオブジェクトを管理します。
		/// </summary>
		private IMain FMain;

		/// <summary>
		/// 動的にロードしたオブジェクトを取得または設定します。
		/// </summary>
		public IMain Main
		{
			get
			{
				return this.FMain;
			}
			set
			{
				this.FMain = value;
			}
		}

		/// <summary>
		/// 排他制御用オブジェクトを管理します。
		/// </summary>
		private object FSyncObj = new object();
	}
}

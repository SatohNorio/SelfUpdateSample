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

namespace SelfUpdateSample
{
	/// <summary>
	/// 作成した AppDomain から値を取得するクラスを定義します。
	/// </summary>
	public class AssemblyInfo : MarshalByRefObject 
	{
		// ------------------------------------------------------------------------------------------------------------
		#region コンストラクタ

		/// <summary>
		/// SelfUpdateSample.AssemblyInfo クラスの新しいインスタンスを作成します。
		/// </summary>
		public AssemblyInfo()
		{
		}

		#endregion // コンストラクタ
		// ------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// アセンブリ情報を取得し、フィールドに設定します。
		/// </summary>
		public void ReadAssemblyInfo()
		{
			// シャドウコピーされたアセンブリを読み込む
			var asm = Assembly.LoadFrom(this.MyDllName);
			foreach (var t in asm.GetExportedTypes())
			{
				if (this.TargetType.IsAssignableFrom(t) && t.IsClass)
				{
					this.FAssemblyName = t.Assembly.FullName;
					this.FClassName = t.FullName;
				}
			}
		}

		// ------------------------------------------------------------------------------------------------------------
		#region プロパティ

		/// <summary>
		/// アセンブリ情報を取得する dll 名 を取得または設定します。
		/// </summary>
		public string MyDllName { get; set; }

		/// <summary>
		/// 取得するアセンブリ情報の型 を取得または設定します。
		/// </summary>
		public Type TargetType { get; set; }

		/// <summary>
		/// アセンブリ名 を管理します。
		/// </summary>
		private string FAssemblyName;

		/// <summary>
		/// アセンブリ名 を取得します。
		/// </summary>
		public string AssemblyName
		{
			get
			{
				return this.FAssemblyName;
			}
		}

		/// <summary>
		/// クラス名 を管理します。
		/// </summary>
		private string FClassName;

		/// <summary>
		/// クラス名 を取得します。
		/// </summary>
		public string ClassName
		{
			get
			{
				return this.FClassName;
			}
		}

		/// <summary>
		/// 情報の取得状態を取得します。
		/// アセンブリ名、クラス名共に取得していれば true を返します。
		/// </summary>
		public bool HasInfo
		{
			get
			{
				return (!String.IsNullOrEmpty(this.AssemblyName) && !String.IsNullOrEmpty(this.ClassName));
			}
		}

		#endregion // プロパティ
		// ------------------------------------------------------------------------------------------------------------
	}
}

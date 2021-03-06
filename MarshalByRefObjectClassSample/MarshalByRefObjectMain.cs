﻿using System;
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

namespace MarshalByRefObjectClassSample
{
	public class MarshalByRefObjectMain : MarshalByRefObject, IMain
	{
		public string Message
		{
			get
			{
				return "This is MarshalByRefObjectMain.";
			}
		}

		public void Run()
		{
			// 既定の AppDomain で Window を作成しているため、こちらでは Window の作成は不可
			//var dispatcher = Application.Current.Dispatcher;
			//if (dispatcher.CheckAccess())
			//{
			//	new Window1().Show();
			//}
			//else
			//{
			//	dispatcher.Invoke(() => new Window1().Show());
			//}
		}
	}
}

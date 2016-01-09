using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InterfaceSample;

namespace MarshalByRefObjectClassSample
{
	public class MarshalByRefObjectMain : IMain
	{
		public string Message
		{
			get
			{
				return "This is MarshalByRefObjectMain";
			}
		}
	}
}

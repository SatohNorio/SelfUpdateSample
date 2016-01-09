using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InterfaceSample;

namespace SerializableClassSample
{
	[Serializable]
	public class SerializableMain : IMain
	{
		public string Message
		{
			get
			{
				return "SerializableMain";
			}
		}
	}
}

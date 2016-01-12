using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceSample
{
	public interface IMain
	{
		void Run();

		string Message { get; }
	}
}

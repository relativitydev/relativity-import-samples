using Relativity.Import.V1;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Import.Samples.NetClientConsole.RestClientHelpers
{
	public class ResponseWrapper<T>
	{
		public ValueResponse<T> ValueResponse { get; set; }
	}
}

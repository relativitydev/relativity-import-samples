using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Import.Samples.NetClientConsole.RestClientHelpers
{
	public static class RelativityImportEndpoitns
	{

		public const string documentConfigurationUri = "/import-jobs/{importId}/documents-configurations";

		public const string createImportJobUri = "import-jobs/{@importId}";

		public const string beginImportJobUri = "/import-jobs/{importId}/begin";

		public const string endImportJobUri = "/import-jobs/{importId}/end";

		public const string importSourcesUri = "/import-jobs/{importId}/sources/{sourceId}";

		public const string importSourceDetailsUri = "/import-jobs/{importId}/sources/{sourceId}/details";

		public const string importSourceProgressUri = "/import-jobs/{importId}/sources/{sourceId}/progress";

	}
}

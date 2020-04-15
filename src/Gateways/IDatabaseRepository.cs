using System.Collections.Generic;

namespace Gateways
{
    public interface IDatabaseRepository
    {
        IEnumerable<W2BatchPrintRow> QueryBatchPrint(string query);
    }
}
using System;
using System.Collections.Generic;
using System.Data;

namespace AU.DocumentGenerationService.GeneratorIntegration.CommandQueues
{
    public interface IDatabaseSession
    {
        IDbConnection Connection();
        void CloseConnection(IDbConnection conn);

        IDataParameter CreateParam(string name, object value);
    }
}

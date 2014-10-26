using System;
using System.Data;

namespace AU.DocumentGenerationService.GeneratorIntegration.CommandQueues
{
    public class ColDef<T>
    {
        public string ColumnName;
        public Action<IDataReader, T, int> LoadValue;
        public Func<T, object> GetValue;
        public DbType DbType;
    }
}

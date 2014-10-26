using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using AU.DocumentGenerationService.GeneratorIntegration.CommandQueues;

namespace MicroRecset
{
    class Program
    {
        static void Main(string[] args)
        {
            var sess = new SqlDatabaseSession("Testo");

            // AddHeaps(sess);
            // AddRec(sess);
            Top(sess);
        }

        private static void AddHeaps(SqlDatabaseSession sess)
        {
            var cmdQueueMap = new CommandQueueMap(sess);

            for (var idx = 0; idx < 100000; idx++)
            {
                var rec = new CommandQueueRec
                {
                    Id = Guid.NewGuid(),
                    TimeQueued = DateTime.UtcNow,
                    CommandType = "Test",
                    TemplateName = "TemplateName",
                    TemplateFields = "fields" + idx.ToString("000000")
                };
                cmdQueueMap.Insert(rec);
            }
        }

        private static void Top(SqlDatabaseSession sess)
        {
            var cmdQueueMap = new CommandQueueMap(sess);

            var rec = cmdQueueMap.Load().FirstOrDefault();
            if (rec != null)
                cmdQueueMap.Delete(rec);
        }

        private static void AddRec(IDatabaseSession sess)
        {
            var rec = new CommandQueueRec
            {
                Id = Guid.NewGuid(),
                TimeQueued = DateTime.UtcNow,
                CommandType = "Test",
                TemplateName = "TemplateName",
                TemplateFields = "fields"
            };

            var cmdQueueMap = new CommandQueueMap(sess);

            cmdQueueMap.Insert(rec);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AU.DocumentGenerationService.GeneratorIntegration.CommandQueues
{
    public class CommandQueueRec
    {
        public Guid Id { get; set; }
        public DateTime TimeQueued { get; set; }
        public string CommandType { get; set; }
        public string TemplateName { get; set; }
        public string TemplateFields { get; set; }
    }

    public class CommandQueueMap : DataMap<CommandQueueRec>
    {
        public CommandQueueMap(IDatabaseSession sess) 
            : base(sess)
        {
        }

        public override string TableName { get { return "command_queue"; } }

        protected override ColDef<CommandQueueRec>[] ColDefs { get { return _colDefs; } }
        protected override ColDef<CommandQueueRec> PrimaryKey { get { return _colDefs[0]; } }

        private readonly ColDef<CommandQueueRec>[] _colDefs = new [] 
        {
            new ColDef<CommandQueueRec> { ColumnName = "command_queue_id", DbType = DbType.Guid, LoadValue = (rdr, x, idx) => x.Id = rdr.GetGuid(idx), GetValue = x => x.Id },
            new ColDef<CommandQueueRec> { ColumnName = "time_queued", DbType = DbType.DateTime, LoadValue = (rdr, x, idx) => x.TimeQueued = rdr.GetDateTime(idx), GetValue = x => x.TimeQueued },
            new ColDef<CommandQueueRec> { ColumnName = "command_type", DbType = DbType.String, LoadValue = (rdr, x, idx) => x.CommandType = rdr.GetString(idx), GetValue = x => x.CommandType },
            new ColDef<CommandQueueRec> { ColumnName = "template_name", DbType = DbType.String, LoadValue = (rdr, x, idx) => x.TemplateName = rdr.GetString(idx), GetValue = x => x.TemplateName },
            new ColDef<CommandQueueRec> { ColumnName = "template_fields", DbType = DbType.String, LoadValue = (rdr, x, idx) => x.TemplateFields = rdr.GetString(idx), GetValue = x => x.TemplateFields },
        };

        protected override string SelectModifier { get { return "top 1"; } }
        protected override string OrderModifier { get { return "command_type, time_queued"; } }
    }
}

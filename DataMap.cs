using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace AU.DocumentGenerationService.GeneratorIntegration.CommandQueues
{
    public abstract class DataMap<T> where T : new()
    {
        protected DataMap(IDatabaseSession sess)
        {
            _sess = sess;
        }

        protected readonly IDatabaseSession _sess;

        protected abstract ColDef<T>[] ColDefs { get; }
        public abstract string TableName { get; }
        protected virtual string QueryFilter { get { return string.Empty; } }
        protected virtual string Join { get { return string.Empty; } }
        protected virtual string SelectModifier { get { return string.Empty; } }
        protected virtual string OrderModifier { get { return string.Empty; } }

        protected virtual ColDef<T> PrimaryKey { get { return null; } }

        public virtual IList<T> Load()
        {
            var commandText = GenerateLoadSql();
            var retList = LoadResults(commandText);

            return retList;
        }

        protected virtual string GenerateLoadSql()
        {
            var commandText =
                "select " + SelectModifier +
                    string.Join(", ", ColDefs.Select(x => TableName + ".[" + x.ColumnName + "]").ToArray()) +
                " from "
                + TableName + " "
                + Join;

            var queryFilter = QueryFilter;
            if (!string.IsNullOrEmpty(queryFilter))
                commandText += " where " + queryFilter;

            var orderModifer = OrderModifier;
            if (!string.IsNullOrWhiteSpace(orderModifer))
                commandText += " order by " + orderModifer;

            return commandText;
        }

        protected virtual IList<T> LoadResults(string commandText)
        {
            var conn = _sess.Connection();
            var cmd = conn.CreateCommand();
            cmd.CommandText = commandText;

            var retList = new List<T>();
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read() && !rdr.IsClosed)
                {
                    var rec = LoadFromReader(rdr);
                    retList.Add(rec);
                }

                rdr.Close();
            }
            return retList;
        }

        protected virtual T LoadFromReader(IDataReader rdr)
        {
            var rec = new T();
            for (var idx = 0; idx < ColDefs.Length; idx++)
            {
                ColDefs[idx].LoadValue(rdr, rec, idx);
            }

            return rec;
        }

        public void Insert(T rec)
        {
            var conn = _sess.Connection();
            var cmd = conn.CreateCommand();

            cmd.CommandText = GenerateInsertSql();
            BindValues(cmd, rec);

            cmd.ExecuteNonQuery();
        }

        protected virtual string GenerateInsertSql()
        {
            return
                "insert into " + TableName + "(" +
                string.Join(", ", ColDefs.Select(x => x.ColumnName).ToArray()) +
                ") values (" +
                string.Join(", ", ColDefs.Select(x => "@" + x.ColumnName).ToArray()) +
                ")";
        }

        private void BindValues(IDbCommand cmd, T rec)
        {
            foreach (var colDef in ColDefs)
            {
                var param = _sess.CreateParam(colDef.ColumnName, colDef.GetValue(rec));
                cmd.Parameters.Add(param);
            }
        }

        public void Delete(T rec)
        {
           if (PrimaryKey == null)
               throw new InvalidOperationException("Can't delete if PrimaryKey is not defined.");

            var conn = _sess.Connection();
            var cmd = conn.CreateCommand();

            cmd.CommandText = "delete " + TableName + " where " + PrimaryKey.ColumnName + " = @" + PrimaryKey.ColumnName;

            var param = _sess.CreateParam(PrimaryKey.ColumnName, PrimaryKey.GetValue(rec));
            cmd.Parameters.Add(param);

            cmd.ExecuteNonQuery();
        }
    }
}

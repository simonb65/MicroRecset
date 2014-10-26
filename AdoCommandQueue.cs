using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using AU.DocumentGenerationService.Messages.GenerateCommunication;
using NServiceBus.MessageInterfaces.MessageMapper.Reflection;
using NServiceBus.Serializers.XML;

namespace AU.DocumentGenerationService.GeneratorIntegration.CommandQueues
{
    public class AdoCommandQueue<T> : IDisposable, ICommandQueue<T> where T : GenerateCommunitcation, new()
    {
        public IDbConnection Connection { get; set; }

        public AdoCommandQueue(IDbConnection connection)
        {
            if (connection == null) 
                throw new ArgumentNullException("connection");

            Connection = connection;
            Connection.Open();
        }

        public void Enqueue(T command)
        {
            var serialiseFields = SerialiseFields(command);
            InsertCommandIntoQueueTable(command, serialiseFields);
        }

        private void InsertCommandIntoQueueTable(T command, string fields)
        {
            if (command == null) 
                throw new ArgumentNullException("command");
            if (string.IsNullOrWhiteSpace(fields)) 
                throw new ArgumentNullException("fields");

            var rec = new CommandQueueRec
            {
                Id = Guid.NewGuid(),
                TimeQueued = DateTime.UtcNow,
                TemplateName = command.TemplateName,
                TemplateFields =  fields
            };

            var cmdQueueMap = new CommandQueueMap(null);

            cmdQueueMap.Insert(rec);


/*
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO command_queue(id, time_queued command_type, template_name, template_fields)  VALUES (@Id ,@TimeQueued, @CommandType, @TemplateName, @TemplateFields)";
                cmd.Parameters.Add(new SqlParameter("Id", Guid.NewGuid()));
                cmd.Parameters.Add(new SqlParameter("TimeQueued", DateTime.Now));
                cmd.Parameters.Add(new SqlParameter("CommandType", GetCommandName()));
                cmd.Parameters.Add(new SqlParameter("TemplateName", command.TemplateName));
                cmd.Parameters.Add(new SqlParameter("TemplateFields", fields));
                cmd.ExecuteNonQuery();
            }
 */ 
        }

        private static string SerialiseFields(T command)
        {
            if (command == null) 
                throw new ArgumentNullException("command");

            var serialiser = CreateFieldSerialiser();

            string fields;
            using (var memoryStream = new MemoryStream())
            using (var streamReader = new StreamReader(memoryStream))
            {
                serialiser.Serialize(command.TemplateFieldValues.ToArray(), memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                fields = streamReader.ReadToEnd();
            }

            if (string.IsNullOrWhiteSpace(fields)) throw new ApplicationException("Serialisation of template fields failed.");
            
            return fields;
        }

        private static MessageSerializer CreateFieldSerialiser()
        {
            var serialiser = new MessageSerializer
            {
                MessageMapper = new MessageMapper(),
                MessageTypes = new List<Type>(new[] {typeof (Field)})
            };
            return serialiser;
        }

        public T Dequeue()
        {
            var command = ReadAndDeleteCommandFromTable();

            return command;
        }

        private T ReadAndDeleteCommandFromTable()
        {
            var cmdQueueMap = new CommandQueueMap(null);

            var rec = cmdQueueMap.Load().FirstOrDefault();

            if (rec == null)
                return null;

            var fields = DeserialiseFields(rec.TemplateFields);
            var command = new T
            {
                                     
                TemplateName = rec.TemplateName, 
                TemplateFieldValues = fields
            };

            cmdQueueMap.Delete(rec);


/*
            T command;
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT TOP 1 Id, TemplateName]
      ,[TemplateFields]
  FROM [CommandQueues].[dbo].[CommandQueue]
  WHERE [CommandType] = @CommandType
  ORDER BY [TimeQueued]";
                cmd.Parameters.Add(new SqlParameter("CommandType", GetCommandName()));

                Guid id;
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    var templateFields = reader["TemplateFields"].ToString();
                    var templateName = reader["TemplateName"].ToString().Trim();

                    var fields = DeserialiseFields(templateFields);

                    command = new T {TemplateName = templateName, TemplateFieldValues = fields};
                    id = (Guid) reader["Id"];
                }

                DeleteCommandFromQueueTable(id);
            }
 */ 
  
            return command;
        }

        private static List<Field> DeserialiseFields(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("value");

            var serialiser = CreateFieldSerialiser();

            List<Field> fields;
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                streamWriter.Write(value);
                streamWriter.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);
                fields = serialiser.Deserialize(memoryStream).Cast<Field>().ToList();
            }

            if (fields == null) throw new ApplicationException("Desierilatisation of template fields failed.");

            return fields;
        }

        private void DeleteCommandFromQueueTable(Guid id)
        {
            if(id == Guid.Empty) throw new ArgumentNullException("id");

            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText =
                    @"DELETE FROM [CommandQueues].[dbo].[CommandQueue]
      WHERE [Id] = @Id";
                cmd.Parameters.Add(new SqlParameter("Id", id));
                cmd.ExecuteNonQuery();
            }
        }

        private static string GetCommandName()
        {
            return new List<string>(typeof (T).Name.Split('.')).Last();
        }

        public int Count
        {
            get
            {
                int count;
                using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText =
                @"SELECT COUNT([Id])
  FROM [CommandQueues].[dbo].[CommandQueue]
  WHERE [CommandType] = @CommandType";
                cmd.Parameters.Add(new SqlParameter("CommandType", GetCommandName()));
                count = (int)cmd.ExecuteScalar();
            }

                return count;
            }
        }

        public void Dispose()
        {
            Connection.Close();
            Connection.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace ORIS.week10
{
    public class MyORM
    {
        const string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ArizonaDB;Integrated Security=True";
        public IDbConnection connection = null;
        public IDbCommand command = null;

        public MyORM()
        {
            this.connection = new SqlConnection(connectionString);
            this.command = connection.CreateCommand();
        }

        public MyORM AddParameter<T>(string name, T value)
        {
            SqlParameter param = new SqlParameter();
            param.ParameterName = name;
            param.Value = value;
            command.Parameters.Add(param);
            return this;
        }

        public int ExecuteNonQuery(string query)
        {
            int noOfAffectedRows = 0;

            //this.connection = new SqlConnection(connectionString);
            //this.command = connection.CreateCommand();

            using (connection)
            {
                command.CommandText = query;
                connection.Open();
                noOfAffectedRows = command.ExecuteNonQuery();
            }

            return noOfAffectedRows;
        }
        public IEnumerable<T> Select<T>()
        {
            IList<T> list = new List<T>();
            Type t = typeof(T);

            using (connection)
            {
                string sqlExpression = $"SELECT * FROM dbo.{t.Name}s";
                command.CommandText = sqlExpression;

                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    T obj = (T)Activator.CreateInstance(t);
                    t.GetProperties().ToList().ForEach(p =>
                    {
                        p.SetValue(obj, reader[p.Name]);
                    });

                    list.Add(obj);
                }
            }
            return list;
        }

        public void Insert<T>(T model)
        {
            using (connection)
            {
                PropertyInfo[] modelFields = model.GetType().GetProperties().Where(p => !p.Name.Equals("Id")).ToArray();
                List<string> parameters = modelFields.Select(x => $"@{x.Name}").ToList();
                Console.WriteLine(string.Join(",", parameters));
                string sqlExpression = $"INSERT INTO dbo.{typeof(T).Name}s ({string.Join(",", modelFields.Select(f => f.Name))}) VALUES ({string.Join(", ", parameters)})";
                command.CommandText = sqlExpression;
                Console.WriteLine(sqlExpression);
                foreach (var field in modelFields)
                {
                    command.Parameters.Add(new SqlParameter($"@{field.Name}", field.GetValue(model)));
                }
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Delete<T>(T model)
        {
            using (connection)
            {
                PropertyInfo[] modelFields = model.GetType().GetProperties().Where(p => p.Name.Equals("Id")).ToArray();
                List<string> parameters = modelFields.Select(x => $"@{x.Name}").ToList();
                string sqlExpression = $"DELETE FROM dbo.{typeof(T).Name}s WHERE {string.Join(",", modelFields.Select(f => $"{f.Name}=@{f.Name}").ToList())}";
                command.CommandText = sqlExpression;
                Console.WriteLine(sqlExpression);
                foreach (var field in modelFields)
                {
                    command.Parameters.Add(new SqlParameter($"@{field.Name}", field.GetValue(model)));
                }

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Update<T>(int id, T model)
        {
            using(connection)
            {
                PropertyInfo[] modelFields = model.GetType().GetProperties().Where(p => !p.Name.Equals("Id")).ToArray();
                List<string> parameters = modelFields.Select(x => $"@{x.Name}").ToList();
                string sqlExpression = $"UPDATE dbo.{typeof(T).Name}s SET {string.Join(",", modelFields.Select(f => $"{f.Name}=@{f.Name}").ToList())} WHERE Id = @id";
                command.CommandText = sqlExpression;
                Console.WriteLine(sqlExpression);
                foreach (var field in modelFields)
                {
                    command.Parameters.Add(new SqlParameter($"@{field.Name}", field.GetValue(model)));
                }
                command.Parameters.Add(new SqlParameter("@id", id));

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public IEnumerable<T> ExecuteQuery<T>(string query)
        {
            IList<T> list = new List<T>();
            Type t = typeof(T);

            //using (connection)
            //{
                command.CommandText = query;

                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    T obj = (T)Activator.CreateInstance(t);
                    t.GetProperties().ToList().ForEach(p =>
                    {
                        p.SetValue(obj, reader[p.Name]);
                    });

                    list.Add(obj);
                }
            //}

            connection.Close();
            command.Parameters.Clear();
            //this.connection.ConnectionString = connectionString;

            return list;
        }
        public T ExecuteScalar<T>(string query)
        {
            T result = default(T);
            using (connection)
            {
                command.CommandText = query;
                connection.Open();
                result = (T)command.ExecuteScalar();
            }

            return result;
        }
    }
}

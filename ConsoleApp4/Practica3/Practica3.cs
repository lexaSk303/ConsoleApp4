using System;
using System.Data;
using System.Data.Common;
using Npgsql;

namespace ConsoleApp4.Practica3
{
    public class Practica3
    {
        public static void RunTask3()
        {
            string connString = "Host=wotacoma.ru;Port=5090;Username=lexask333;Password=123SLexaS321;Database=lexask333";

            DbProviderFactories.RegisterFactory("Npgsql", NpgsqlFactory.Instance);
            DbProviderFactory factory = DbProviderFactories.GetFactory("Npgsql");

            using DbConnection connection = factory.CreateConnection();
            connection.ConnectionString = connString;
            connection.Open();

            Console.WriteLine("=== Задача 3. Редактирование и сохранение ===\n");

            Console.WriteLine("Создаём таблицу students_lesson14 и копируем данные из students_lesson13...");

            using DbCommand setupCmd = factory.CreateCommand();
            setupCmd.Connection = connection;
            setupCmd.CommandText = """
                CREATE TABLE IF NOT EXISTS students_lesson14 (
                    id SERIAL PRIMARY KEY,
                    first_name TEXT NOT NULL,
                    last_name TEXT NOT NULL,
                    age INT NOT NULL DEFAULT 20
                );

                INSERT INTO students_lesson14 (first_name, last_name)
                SELECT first_name, last_name FROM students_lesson13
                WHERE NOT EXISTS (SELECT 1 FROM students_lesson14 LIMIT 1);
                """;
            setupCmd.ExecuteNonQuery();
            Console.WriteLine("Таблица students_lesson14 готова.\n");

            using DbCommand selectCmd = factory.CreateCommand();
            selectCmd.Connection = connection;
            selectCmd.CommandText = "SELECT id, first_name, last_name, age FROM students_lesson14";

            using DbDataAdapter adapter = factory.CreateDataAdapter();
            adapter.SelectCommand = selectCmd;

            DataTable table = new DataTable();
            adapter.Fill(table);

            connection.Close();
            Console.WriteLine($"Загружено студентов: {table.Rows.Count}. Соединение закрыто.\n");


            table.Rows[0]["age"] = 99;

            DataRow newRow = table.NewRow();
            newRow["first_name"] = "Lexa";
            newRow["last_name"] = "Mixailovich";
            newRow["age"] = 19;
            table.Rows.Add(newRow);

            table.Rows[1].Delete();


            Console.WriteLine("=== RowState ПЕРЕД сохранением ===");
            foreach (DataRow row in table.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    Console.WriteLine($"[DELETED] ID: {row["id", DataRowVersion.Original]}, " +
                                      $"Имя: {row["first_name", DataRowVersion.Original]} {row["last_name", DataRowVersion.Original]}");
                }
                else
                {
                    Console.WriteLine($"[{row.RowState,-9}] ID: {row["id"]}, " +
                                      $"Имя: {row["first_name"]} {row["last_name"]}, Возраст: {row["age"]}");
                }
            }
            Console.WriteLine();

            adapter.InsertCommand = factory.CreateCommand();
            adapter.InsertCommand.CommandText = "INSERT INTO students_lesson14 (first_name, last_name, age) VALUES (@first_name, @last_name, @age)";

            var ip1 = adapter.InsertCommand.CreateParameter();
            ip1.ParameterName = "@first_name"; ip1.DbType = DbType.String; ip1.SourceColumn = "first_name";
            adapter.InsertCommand.Parameters.Add(ip1);

            var ip2 = adapter.InsertCommand.CreateParameter();
            ip2.ParameterName = "@last_name"; ip2.DbType = DbType.String; ip2.SourceColumn = "last_name";
            adapter.InsertCommand.Parameters.Add(ip2);

            var ip3 = adapter.InsertCommand.CreateParameter();
            ip3.ParameterName = "@age"; ip3.DbType = DbType.Int32; ip3.SourceColumn = "age";
            adapter.InsertCommand.Parameters.Add(ip3);

            adapter.UpdateCommand = factory.CreateCommand();
            adapter.UpdateCommand.CommandText = "UPDATE students_lesson14 SET first_name = @first_name, last_name = @last_name, age = @age WHERE id = @id";

            var up1 = adapter.UpdateCommand.CreateParameter();
            up1.ParameterName = "@first_name"; up1.DbType = DbType.String; up1.SourceColumn = "first_name";
            adapter.UpdateCommand.Parameters.Add(up1);

            var up2 = adapter.UpdateCommand.CreateParameter();
            up2.ParameterName = "@last_name"; up2.DbType = DbType.String; up2.SourceColumn = "last_name";
            adapter.UpdateCommand.Parameters.Add(up2);

            var up3 = adapter.UpdateCommand.CreateParameter();
            up3.ParameterName = "@age"; up3.DbType = DbType.Int32; up3.SourceColumn = "age";
            adapter.UpdateCommand.Parameters.Add(up3);

            var upId = adapter.UpdateCommand.CreateParameter();
            upId.ParameterName = "@id"; upId.DbType = DbType.Int32; upId.SourceColumn = "id"; upId.SourceVersion = DataRowVersion.Original;
            adapter.UpdateCommand.Parameters.Add(upId);

            adapter.DeleteCommand = factory.CreateCommand();
            adapter.DeleteCommand.CommandText = "DELETE FROM students_lesson14 WHERE id = @id";

            var delId = adapter.DeleteCommand.CreateParameter();
            delId.ParameterName = "@id"; delId.DbType = DbType.Int32; delId.SourceColumn = "id"; delId.SourceVersion = DataRowVersion.Original;
            adapter.DeleteCommand.Parameters.Add(delId);

  
            connection.Open();
            using DbTransaction transaction = connection.BeginTransaction();

            adapter.InsertCommand.Connection = connection;
            adapter.InsertCommand.Transaction = transaction;

            adapter.UpdateCommand.Connection = connection;
            adapter.UpdateCommand.Transaction = transaction;

            adapter.DeleteCommand.Connection = connection;
            adapter.DeleteCommand.Transaction = transaction;

            try
            {
                Console.WriteLine("Выполняется adapter.Update() внутри транзакции...");
                int rowsAffected = adapter.Update(table);
                transaction.Commit();
                Console.WriteLine($"Успешно применено изменений к БД: {rowsAffected}\n");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Ошибка сохранения! Выполнен откат.");
                Console.WriteLine($"   Причина: {ex.Message}");
                Console.WriteLine("   Примечание: Изменения остались в DataTable (в памяти), но не попали в БД.\n");
            }


            Console.WriteLine("=== RowState ПОСЛЕ сохранения ===");
            foreach (DataRow row in table.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    Console.WriteLine($"[DELETED] ID: {row["id", DataRowVersion.Original]}");
                }
                else
                {
                    Console.WriteLine($"[{row.RowState,-9}] ID: {row["id"]}, Имя: {row["first_name"]}");
                }
            }
            Console.WriteLine("(Все строки стали Unchanged, так как адаптер автоматически вызвал AcceptChanges)\n");


            using DbCommand verifyCmd = factory.CreateCommand();
            verifyCmd.Connection = connection;
            verifyCmd.CommandText = "SELECT id, first_name, last_name, age FROM students_lesson14 ORDER BY id";

            Console.WriteLine("=== Прямая проверка данных в PostgreSQL ===");
            using DbDataReader reader = verifyCmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["id"]}, Имя: {reader["first_name"]} {reader["last_name"]}, Возраст: {reader["age"]}");
            }

            connection.Close();
        }
    }
}
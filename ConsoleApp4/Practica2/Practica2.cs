using System;
using System.Data;
using System.Data.Common;
using Npgsql;

namespace ConsoleApp4.Practica2
{
    public class Practica2
    {

        public static void RunTask2()
        {
            string connString = "Host=wotacoma.ru;Port=5090;Username=lexask333;Password=123SLexaS321;Database=lexask333";

            DbProviderFactories.RegisterFactory("Npgsql", NpgsqlFactory.Instance);
            DbProviderFactory factory = DbProviderFactories.GetFactory("Npgsql");

            using DbConnection connection = factory.CreateConnection();
            connection.ConnectionString = connString;
            connection.Open();

            using DbCommand selectCmd = factory.CreateCommand();
            selectCmd.Connection = connection;
            selectCmd.CommandText = "SELECT id, first_name, last_name FROM students_lesson13";

            using DbDataAdapter adapter = factory.CreateDataAdapter();
            adapter.SelectCommand = selectCmd;

            DataTable table = new DataTable();
            adapter.Fill(table);

            connection.Close();

            Console.WriteLine("=== Эксперимент с изменениями ===\n");

            DataRow rowToModify = table.Rows[0];
            string originalFullName = $"{rowToModify["first_name"]} {rowToModify["last_name"]}";
            rowToModify["first_name"] = "Vlad (Изменён)";

            DataRow newRow = table.NewRow();
            newRow["first_name"] = "Lexa";
            newRow["last_name"] = "GrigorunMixailovich";
            table.Rows.Add(newRow);

            DataRow rowToDelete = table.Rows[2];
            string deletedFullName = $"{rowToDelete["first_name"]} {rowToDelete["last_name"]}";
            rowToDelete.Delete();

            Console.WriteLine("=== Журнал изменений ===");
            foreach (DataRow row in table.Rows)
            {
                if (row.RowState == DataRowState.Modified)
                {
                    string origName = row["first_name", DataRowVersion.Original].ToString()!;
                    string origLast = row["last_name", DataRowVersion.Original].ToString()!;
                    string currName = row["first_name", DataRowVersion.Current].ToString()!;
                    string currLast = row["last_name", DataRowVersion.Current].ToString()!;

                    Console.WriteLine($"[MODIFIED] {origName} {origLast} → {currName} {currLast}");
                }
                else if (row.RowState == DataRowState.Added)
                {
                    string currName = row["first_name", DataRowVersion.Current].ToString()!;
                    string currLast = row["last_name", DataRowVersion.Current].ToString()!;

                    Console.WriteLine($"[ADDED]    {currName} {currLast}");
                }
                else if (row.RowState == DataRowState.Deleted)
                {
                    string origName = row["first_name", DataRowVersion.Original].ToString()!;
                    string origLast = row["last_name", DataRowVersion.Original].ToString()!;

                    Console.WriteLine($"[DELETED]  {origName} {origLast}");
                }
            }

            Console.WriteLine("\n=== Вызов RejectChanges() ===");
            table.RejectChanges();

            Console.WriteLine("Проверка восстановления данных:");
            Console.WriteLine($"Строка 0 (должна быть как была): {table.Rows[0]["first_name"]} {table.Rows[0]["last_name"]} (Состояние: {table.Rows[0].RowState})");

            bool isDeletedGone = true;
            foreach (DataRow row in table.Rows)
            {
                if ($"{row["first_name"]} {row["last_name"]}" == deletedFullName)
                {
                    isDeletedGone = false;
                }
            }
            Console.WriteLine($"Удалённый студент ({deletedFullName}) восстановлен: {(isDeletedGone ? "Нет" : "Да")}");
            Console.WriteLine($"Новый студент удалён из памяти: {(table.Rows.Count < 22 ? "Да" : "Нет")}");
        }
    }
}

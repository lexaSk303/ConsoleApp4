using System;
using System.Data;
using System.Data.Common;
using ConsoleApp4.Practica2;
using ConsoleApp4.Practica3;
using Npgsql;

public class Program 
{
    public static void Main()
    {
        Practica4.RunTask4();
        //Practica3.RunTask3();
        //Practica2.RunTask2();
        //FixAges.MainTRr();
    }
    public static void RunTask1()
    {
        string connString = "Host=wotacoma.ru;Port=5090;Username=lexask333;Password=123SLexaS321;Database=lexask333";

        DbProviderFactories.RegisterFactory("Npgsql", NpgsqlFactory.Instance);
        DbProviderFactory factory = DbProviderFactories.GetFactory("Npgsql");

        using DbConnection connection = factory.CreateConnection();
        connection.ConnectionString = connString;
        connection.Open();

        Console.WriteLine("=== Урок 14. DataRowState ===");
        Console.WriteLine("Подготовка данных в личной БД...");

        using DbCommand setupCmd = factory.CreateCommand();
        setupCmd.Connection = connection;
        setupCmd.CommandText = """
            CREATE TABLE IF NOT EXISTS students_lesson13 (
                id SERIAL PRIMARY KEY,
                first_name TEXT NOT NULL,
                last_name TEXT NOT NULL
            );

            DELETE FROM students_lesson13;

            INSERT INTO students_lesson13 (first_name, last_name) VALUES 
            ('Vlad', 'Chistalev'),
            ('Roman', 'Stepanov'),
            ('Oleg', 'Kaidalov'),
            ('Никита', 'Андрушкевич'),
            ('Alexey', 'Grigorun'),
            ('Nikita', 'Andrushkevich'),
            ('Вячеслав', 'Боталев'),
            ('Алексей', 'Иванович'),
            ('Иосиф', 'Колегов'),
            ('Карен', 'Петросян'),
            ('Талина', 'Захарова'),
            ('Artyom', 'Bulyshev'),
            ('Egor', 'Prislegin'),
            ('Michael', 'Gutorin'),
            ('Ilya', 'Tyapovkin'),
            ('Vacheslav', 'Botalov'),
            ('Александр', 'Малыгин'),
            ('Максимилиан', 'Кичигин'),
            ('Максимилиан', 'Кичигин'),
            ('Danil', 'Ponomarev');
            """;
        setupCmd.ExecuteNonQuery();
        Console.WriteLine("Таблица создана и заполнена студентами.\n");

        Console.WriteLine($"Состояние соединения до Fill(): {connection.State}");

        using DbCommand selectCmd = factory.CreateCommand();
        selectCmd.Connection = connection;
        selectCmd.CommandText = "SELECT id, first_name, last_name FROM students_lesson13";

        using DbDataAdapter adapter = factory.CreateDataAdapter();
        adapter.SelectCommand = selectCmd;

        DataTable table = new DataTable();
        adapter.Fill(table);

        Console.WriteLine($"Состояние соединения после Fill(): {connection.State}");
        Console.WriteLine($"Загружено студентов: {table.Rows.Count}");

        connection.Close();
        Console.WriteLine("После выхода из using соединение закрыто.\n");

        Console.WriteLine("=== Исходные состояния ===");
        int count = 0;
        foreach (DataRow row in table.Rows)
        {
            Console.WriteLine($"ID: {row["id"]}, Имя: {row["first_name"]}, Фамилия: {row["last_name"]}, Состояние: {row.RowState}");
            count++;
            if (count >= 21) break;
        }

        DataRow newRow = table.NewRow();
        newRow["first_name"] = "Тестовый";
        newRow["last_name"] = "студент";
        Console.WriteLine($"\nПосле NewRow(): {newRow.RowState}");

        table.Rows.Add(newRow);
        Console.WriteLine($"После Rows.Add(): {newRow.RowState}");


        DataRow rowToModify = table.Rows[0];
        string originalName = rowToModify["first_name"].ToString()!;
        rowToModify["first_name"] = "Vlad (изменён)";
        Console.WriteLine($"\nДо изменения: {rowToModify.RowState}");
        Console.WriteLine($"После изменения: {rowToModify.RowState}");

        DataRow rowToDelete = table.Rows[table.Rows.Count - 2];
        Console.WriteLine($"\nДо удаления: {rowToDelete.RowState}");
        rowToDelete.Delete();
        Console.WriteLine($"После Delete(): {rowToDelete.RowState}");


        Console.WriteLine("\n=== Состояния после всех изменений ===");
        foreach (DataRow row in table.Rows)
        {
            Console.WriteLine($"ID: {row["id"]}, Имя: {row["first_name"]}, Фамилия: {row["last_name"]}, Состояние: {row.RowState}");
        }

        Console.WriteLine("\n=== RejectChanges() ===");
        table.RejectChanges();

        foreach (DataRow row in table.Rows)
        {
            Console.WriteLine($"ID: {row["id"]}, Имя: {row["first_name"]}, Фамилия: {row["last_name"]}, Состояние: {row.RowState}");
        }
        Console.WriteLine("Изменения отменены только в памяти.");

        DataRow rowToModify2 = table.Rows[0];
        rowToModify2["first_name"] = "Vlad (изменён)";

        DataRow newRow2 = table.NewRow();
        newRow2["first_name"] = "Тестовый";
        newRow2["last_name"] = "студент";
        table.Rows.Add(newRow2);

        Console.WriteLine("\n=== AcceptChanges() ===");
        table.AcceptChanges();

        foreach (DataRow row in table.Rows)
        {
            Console.WriteLine($"ID: {row["id"]}, Имя: {row["first_name"]}, Фамилия: {row["last_name"]}, Состояние: {row.RowState}");
        }
        Console.WriteLine("AcceptChanges() не сохраняет изменения в PostgreSQL.");
    }
}
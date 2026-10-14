using System;
using System.Data;
using System.Data.Common;
using Npgsql;

namespace ConsoleApp4.Practica3
{
    public class Practica4
    {
        public static void RunTask4()
        {
            string connString = "Host=wotacoma.ru;Port=5090;Username=lexask333;Password=123SLexaS321;Database=lexask333";

            DbProviderFactories.RegisterFactory("Npgsql", NpgsqlFactory.Instance);
            DbProviderFactory factory = DbProviderFactories.GetFactory("Npgsql");

            using DbConnection connection = factory.CreateConnection();
            connection.ConnectionString = connString;
            connection.Open();

            Console.WriteLine("=== Задача 4. Фильтрация и сортировка ===\n");

            Console.WriteLine("Проверка и подготовка таблицы students_lesson14...");
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

                UPDATE students_lesson14 SET age = 20 WHERE age IS NULL;
                """;
            setupCmd.ExecuteNonQuery();
            Console.WriteLine("✓ Данные готовы, у всех студентов есть возраст.\n");

            using DbCommand selectCmd = factory.CreateCommand();
            selectCmd.Connection = connection;
            selectCmd.CommandText = "SELECT id, first_name, last_name, age FROM students_lesson14";

            using DbDataAdapter adapter = factory.CreateDataAdapter();
            adapter.SelectCommand = selectCmd;

            DataTable table = new DataTable();
            adapter.Fill(table);

            connection.Close();
            Console.WriteLine($"Загружено студентов: {table.Rows.Count}. Соединение закрыто.\n");


            DataView view = new DataView(table);

            view.RowFilter = "age > 18";

            view.Sort = "first_name ASC";

            Console.WriteLine("=== Студенты старше 18 лет, отсортированные по имени ===");
            PrintView(view);
            Console.WriteLine("\n=== Меняем фильтр: возраст = 20, сортировка по фамилии ===");
            view.RowFilter = "age = 20";
            view.Sort = "last_name ASC";
            PrintView(view);

            Console.WriteLine("\n=== Исходная таблица (все строки, без фильтра) ===");
            view.RowFilter = "";
            view.Sort = "";
            PrintView(view);
            Console.WriteLine($"(Всего строк в исходной таблице: {table.Rows.Count})");
            Console.WriteLine("DataView.RowFilter и Sort НЕ меняют данные в PostgreSQL.\n");

            Console.WriteLine("=== Меню фильтрации ===");
            Console.WriteLine("1. Все студенты (сортировка по имени)");
            Console.WriteLine("2. Только старше 18 лет");
            Console.WriteLine("3. Только с возрастом 20 лет");
            Console.WriteLine("4. Фамилия начинается на 'К' или 'С'");
            Console.WriteLine("5. Сортировка по возрасту (по убыванию)");
            Console.WriteLine("0. Выход");

            while (true)
            {
                Console.Write("\nВыберите пункт (0-5): ");
                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        view.RowFilter = "";
                        view.Sort = "first_name ASC";
                        Console.WriteLine("\n--- Все студенты ---");
                        PrintView(view);
                        break;

                    case "2":
                        view.RowFilter = "age > 18";
                        view.Sort = "first_name ASC";
                        Console.WriteLine("\n--- Старше 18 лет ---");
                        PrintView(view);
                        break;

                    case "3":
                        view.RowFilter = "age = 20";
                        view.Sort = "last_name ASC";
                        Console.WriteLine("\n--- Возраст = 20 ---");
                        PrintView(view);
                        break;

                    case "4":
                        view.RowFilter = "last_name LIKE 'К*' OR last_name LIKE 'С*'";
                        view.Sort = "last_name ASC";
                        Console.WriteLine("\n--- Фамилия на 'К' или 'С' ---");
                        PrintView(view);
                        break;

                    case "5":
                        view.RowFilter = "";
                        view.Sort = "age DESC";
                        Console.WriteLine("\n--- Все студенты (по возрасту, убывание) ---");
                        PrintView(view);
                        break;

                    case "0":
                        Console.WriteLine("Выход из меню.");
                        return;

                    default:
                        Console.WriteLine("Неверный пункт, попробуйте снова.");
                        break;
                }
            }
        }
        static void PrintView(DataView view)
        {
            if (view.Count == 0)
            {
                Console.WriteLine("  (нет записей, подходящих под фильтр)");
                return;
            }

            Console.WriteLine($"  {"ID",-4} | {"Имя",-15} | {"Фамилия",-15} | {"Возраст",-7}");
            Console.WriteLine(new string('-', 50));

            foreach (DataRowView rowView in view)
            {
                Console.WriteLine($"  {rowView["id"],-4} | {rowView["first_name"],-15} | {rowView["last_name"],-15} | {rowView["age"],-7}");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Data.Common;
using Npgsql;

namespace ConsoleApp4.Practica3
{
    public class FixAges
    {
        public static void MainTRr()
        {
            string connString = "Host=wotacoma.ru;Port=5090;Username=lexask333;Password=123SLexaS321;Database=lexask333";

            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            using var cmd = new NpgsqlCommand("""
            UPDATE students_lesson14 
            SET age = 17 + (id * 7 % 9);
            """, conn);

            int rows = cmd.ExecuteNonQuery();
            Console.WriteLine($"✓ Обновлено возрастов: {rows}");
            using var check = new NpgsqlCommand("SELECT id, first_name, last_name, age FROM students_lesson14 ORDER BY id", conn);
            using var reader = check.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["id"]}, {reader["first_name"]} {reader["last_name"]}, Возраст: {reader["age"]}");
            }
        }
    }
}

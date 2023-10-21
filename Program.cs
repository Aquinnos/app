using System;
using MySql.Data.MySqlClient;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        // Ustal połączenie z bazą danych MySQL
        string connectionString = "Server=localhost;Database=govApp;User ID=root;Password=;";
        MySqlConnection connection = new MySqlConnection(connectionString);

        try
        {
            // Otwarcie połączenia
            connection.Open();

            // Wykonuj operacje na bazie danych
            string sql = "SELECT * FROM Users";
            MySqlCommand command = new MySqlCommand(sql, connection);
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                // Odczytywanie wyników zapytania
                int userID = reader.GetInt32("UserID");
                string username = reader.GetString("Username");
                // Dodaj inne kolumny, które chcesz odczytać
                Console.WriteLine($"UżytkownikID: {userID}, Nazwa użytkownika: {username}");
            }

            reader.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił błąd: {ex.Message}");
        }
        finally
        {
            // Zamknięcie połączenia
            connection.Close();
        }
    }
}

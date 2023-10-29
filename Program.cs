using System;
using MySql.Data.MySqlClient;

internal class Program
{
    private static void Main(string[] args)
    {
        // utworzenie instancji DatabaseConnector
        DatabaseConnector dbConnector = new DatabaseConnector();

        MySqlConnection connection = dbConnector.GetConnection();

        try
        {
            // otwarcie połączenia z bazą danych
            dbConnector.OpenConnection();

            // zapytania na próbe czy działa baza
            string query = "SELECT @@version";
            MySqlCommand cmd = new MySqlCommand(query, connection);

            string version = cmd.ExecuteScalar().ToString();
            Console.WriteLine("Wersja bazy danych: " + version);


            while (true)
            {
                Console.WriteLine("Wybierz opcję:");
                Console.WriteLine("1. Dostępne Programy");
                Console.WriteLine("2. Moje Wnioski");
                Console.WriteLine("3. Profil");
                Console.WriteLine("4. Pomoc i obsługa"); 
                Console.WriteLine("5. Wyjście");

                string userInput = Console.ReadLine();

                if (userInput != null)
                {
                    switch (userInput)
                    {
                        case "1":
                            // operacje związane z Dostępnymi Programami
                            Console.WriteLine("Przechodzisz do Dostępnych Programów");
                            break;
                        case "2":
                            // operacje związane z Moimi Wnioskami
                            Console.WriteLine("Przechodzisz do Moich Wniosków");
                            break;
                        case "3":
                            // operacje związane z Profilem
                            Console.WriteLine("Przechodzisz do Profilu");
                            break;
                        case "4":
                            // operacje związane z Pomocą i Obsługą
                            Console.WriteLine("Przechodzisz do Pomocy i Obsługi");
                            break;
                        case "5":
                            // zamknięcie aplikacji
                            Console.WriteLine("Zamykanie aplikacji.");
                            return;
                        default:
                            Console.WriteLine("Niepoprawna opcja. Wybierz jeszcze raz.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Wystąpił problem z odczytem danych. Spróbuj jeszcze raz.");
                }
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine("Błąd podczas połączenia z bazą danych: " + ex.Message);
        }
        finally
        {
            // zamykanie połączenia bazy danych
            dbConnector.CloseConnection();
        }
    }
}

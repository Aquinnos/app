using System;
using Microsoft.Data.Sqlite;

internal class Program
{
    private static void Main(string[] args)
    {
        // Utwórz instancję DatabaseConnector i podaj ścieżkę do bazy danych SQLite
        DatabaseConnector dbConnector = new DatabaseConnector("./govapp.sqlite");

        // Otwórz połączenie z bazą danych
        dbConnector.OpenConnection();

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
                        Console.WriteLine("Przechodzisz do Dostępnych Programów"); // funkcja programów
                        break;
                    case "2":
                        Console.WriteLine("Przechodzisz do Moich Wniosków"); // funkcja wniosków
                        break;
                    case "3":
                        Console.WriteLine("Przechodzisz do Profilu"); // funkcja profilu użytkownika
                        break;
                    case "4":
                        Console.WriteLine("Przechodzisz do Pomocy i Obsługi"); // funkcja pomocy i obsługi (może jakieś FAQ)
                        break;
                    case "5":
                        Console.WriteLine("Zamykanie aplikacji."); // funkcja

                        // Zamknij połączenie z bazą danych przed zakończeniem programu
                        dbConnector.CloseConnection();

                        return;
                    default:
                        Console.WriteLine("Niepoprawna opcja. Wybierz jeszcze raz."); // funkcja
                        break;
                }
            }
            else
            {
                Console.WriteLine("Wystąpił problem z odczytem danych. Spróbuj jeszcze raz.");
            }
        }
    }
}

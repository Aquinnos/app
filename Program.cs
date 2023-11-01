using System;
using System.Threading;
using Microsoft.Data.Sqlite;
using System.Media;

namespace govApp
{
    public class Authentication
    {
        private SqliteConnection _connection;

        public Authentication(SqliteConnection connection)
        {
            _connection = connection;
        }

        public bool Login(string username, string password)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT UserId FROM users WHERE Username = @username AND Password = @password";
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // zwracamy true kiedy znaleziono użytkownika
                        return true;
                    }
                }

                // zwracany false kiedy nie znaleziono użytkownika
                return false;
            }
        }

        public bool Register(string username, string password, string firstName, string lastName)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO users (Username, Password, FirstName, LastName) VALUES (@username, @password, @firstName, @lastName)";
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@firstName", firstName);
                cmd.Parameters.AddWithValue("@lastName", lastName);

                int rowsAffected = cmd.ExecuteNonQuery();

                // zwracamy true jeśli użytkownik został dodany poprawnie
                return rowsAffected > 0;
            }
        }
    }



    internal class Program
    {
        private static void Main(string[] args)
        {
            DatabaseConnector dbConnector = new DatabaseConnector("./govapp.sqlite");
            dbConnector.OpenConnection();
            Authentication authentication = new Authentication(dbConnector.GetConnection());

            ConsoleKeyInfo keyInfo;
            int selectedOption = 0;
            string[] menuOptions = { "Logowanie", "Rejestracja", "Wejście bez logowania", "Dostępne Programy", "Moje Wnioski", "Profil", "Pomoc i Obsługa", "Wyjście" };
            bool isLoggedIn = false;
            string username = "";

            SoundPlayer scrollSound = new SoundPlayer("./sounds/scroll.wav");
            SoundPlayer selectSound = new SoundPlayer("./sounds/select.wav");

            string asciiArt = @"
                     ___              
   ____ _____ _   __/   |  ____  ____ 
  / __ `/ __ \ | / / /| | / __ \/ __ \
 / /_/ / /_/ / |/ / ___ |/ /_/ / /_/ /
 \__, /\____/|___/_/  |_/ .___/ .___/ 
/____/                 /_/   /_/      
        ";

            Console.WriteLine(asciiArt);
            Console.WriteLine("\n");

            Console.WriteLine("Witaj w Aplikacji Rządowej!");
            Console.WriteLine("================================");
            Thread.Sleep(1000);

            Console.WriteLine("Aplikacja Rządowa to narzędzie do dostępu do różnych programów i formularzy rządowych.");
            Thread.Sleep(1000);
            Console.WriteLine("Możesz korzystać z dostępnych programów, składać wnioski oraz zarządzać swoim profilem.");
            Thread.Sleep(1000);
            Console.WriteLine("Aby rozpocząć, wybierz jedną z dostępnych opcji z poniższego menu.");
            Thread.Sleep(3000);
            Console.WriteLine("\n");
            Console.Clear();

            while (!isLoggedIn)
            {
                Console.Clear();
                Console.WriteLine(asciiArt);
                Console.WriteLine("\n");
                Console.WriteLine("Logowanie lub Rejestracja:");
                for (int i = 0; i < 3; i++)
                {
                    if (i == selectedOption)
                    {
                        Console.Write(">> ");
                    }
                    Console.WriteLine($"{i + 1}. {menuOptions[i]}");
                }

                keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (selectedOption == 0)
                    {
                        Console.Clear();
                        Console.WriteLine("Logowanie:");
                        Console.Write("Podaj nazwę użytkownika: ");
                        username = Console.ReadLine();
                        Console.Write("Podaj hasło: ");
                        string password = Console.ReadLine();

                        if (authentication.Login(username, password))
                        {
                            isLoggedIn = true;
                            Console.WriteLine("Zalogowano pomyślnie!");
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            Console.WriteLine("Błąd logowania. Spróbuj ponownie.");
                            Thread.Sleep(1000);
                        }
                    }
                    else if (selectedOption == 1)
                    {
                        Console.Clear();
                        Console.WriteLine("Rejestracja:");
                        Console.Write("Podaj nazwę użytkownika: ");
                        username = Console.ReadLine();
                        Console.Write("Podaj hasło: ");
                        string password = Console.ReadLine();
                        Console.Write("Podaj imię: ");
                        string firstName = Console.ReadLine();
                        Console.Write("Podaj nazwisko: ");
                        string lastName = Console.ReadLine();

                        // sprawdzamy czy pola nie są puste
                        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                        {
                            Console.WriteLine("Wszystkie pola są wymagane. Spróbuj ponownie.");
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            if (authentication.Register(username, password, firstName, lastName))
                            {
                                Console.WriteLine("Zarejestrowano pomyślnie!");
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                Console.WriteLine("Błąd rejestracji. Spróbuj ponownie.");
                                Thread.Sleep(1000);
                            }
                        }
                    }
                    else if (selectedOption == 2)
                    {
                        // wejście bez logowania
                        isLoggedIn = true; // użytkownik zalogowany, nie mając konta
                        username = "Gość"; // domyślna nazwa użytkownika dla wejścia bez logowania
                    }

                    Console.WriteLine("Naciśnij dowolny klawisz, aby kontynuować...");
                    Console.ReadKey();
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    selectedOption = Math.Max(0, selectedOption - 1);
                    scrollSound.Play();
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    selectedOption = Math.Min(2, selectedOption + 1);
                    scrollSound.Play();
                }
            }

            // głowna pętla menu
            do
            {
                Console.Clear();
                Console.WriteLine(asciiArt);
                Console.WriteLine("\n");
                Console.WriteLine($"Zalogowano jako: {username}");
                for (int i = 3; i < menuOptions.Length; i++)
                {
                    if (i == selectedOption)
                    {
                        Console.Write(">> ");
                    }
                    Console.WriteLine($"{i - 3 + 1}. {menuOptions[i]}");
                }

                keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    switch (selectedOption)
                    {
                        case 3:
                            Console.WriteLine("Przechodzisz do Dostępnych Programów.");
                            selectSound.Play();
                            // funkcja dostępnych programów
                            break;
                        case 4:
                            Console.WriteLine("Przechodzisz do Moich Wniosków.");
                            selectSound.Play();
                            // funkcja wniosków
                            break;
                        case 5:
                            Console.WriteLine("Przechodzisz do Profilu.");
                            selectSound.Play();
                            // funkcja profilu
                            break;
                        case 6:
                            Console.WriteLine("Przechodzisz do Pomocy i Obsługi.");
                            selectSound.Play();
                            // funkcja pomocy
                            break;
                        case 7:
                            Console.WriteLine("Zamykanie aplikacji.");
                            return;
                    }

                    Console.WriteLine("Naciśnij dowolny klawisz, aby kontynuować...");
                    Console.ReadKey();
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    selectedOption = Math.Max(2, selectedOption - 1);
                    scrollSound.Play();
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    selectedOption = Math.Min(menuOptions.Length - 1, selectedOption + 1);
                    scrollSound.Play();
                }

            } while (keyInfo.Key != ConsoleKey.Escape);
        }
    }
}

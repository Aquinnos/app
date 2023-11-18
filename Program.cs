﻿using System;
using System.Threading;
using Microsoft.Data.Sqlite;
using System.Media;
using Spectre.Console;
using BCrypt.Net;

namespace govApp
{
    public class Project
    {
        public int Id_programu { get; set; }
        public string Nazwa_programu { get; set; }
        public string Opis_programu { get; set; }
        public string Fundusz { get; set; }
        public string Data_rozpoczecia { get; set; }
        public string Data_zakonczenia { get; set; }
        public string Osoba_odpowiedzialna { get; set; }
        public string Kategoria_programu { get; set; }
    }

    public class UserProfile
    {
        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        public string DataUrodzenia { get; set; }
        public string Pesel { get; set; }
    }

    public class Authentication
    {
        public UserProfile GetUserProfile(string username)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT Imie, Nazwisko, data_urodzenia, Pesel FROM users WHERE Username = @username";
                cmd.Parameters.AddWithValue("@username", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new UserProfile
                        {
                            Imie = reader["Imie"].ToString(),
                            Nazwisko = reader["Nazwisko"].ToString(),
                            DataUrodzenia = reader["data_urodzenia"].ToString(),
                            Pesel = reader["Pesel"].ToString()
                        };
                    }
                }
            }

            return null;
        }
        private SqliteConnection _connection;

        public Authentication(SqliteConnection connection)
        {
            _connection = connection;
        }

        public bool Login(string username, string password)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT UserId, Password FROM users WHERE Username = @username";
                cmd.Parameters.AddWithValue("@username", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string hashedPassword = reader["Password"].ToString();

                        // porównanie wprowadzonego hasła z zahaszowanym hasłem z bazy danych
                        if (BCrypt.Net.BCrypt.Verify(password, hashedPassword))
                        {
                            // true jesli hasla sie zgadzaja
                            return true;
                        }
                    }
                }
                // false, jesli haslo sie nie zgadza lub uzytkownik nie istnieje
                return false;
            }
        }


        public bool Register(string username, string password, string imie, string nazwisko, string data_urodzenia, string pesel)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO users (Username, Password, Imie, Nazwisko, data_urodzenia, Pesel) VALUES (@username, @password, @imie, @nazwisko, @data_urodzenia, @pesel)";
                cmd.Parameters.AddWithValue("@username", username);

                // haszowanie hasła przed zapisem do bazy danych
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                cmd.Parameters.AddWithValue("@password", hashedPassword);
                cmd.Parameters.AddWithValue("@imie", imie);
                cmd.Parameters.AddWithValue("@nazwisko", nazwisko);
                cmd.Parameters.AddWithValue("@data_urodzenia", data_urodzenia);
                cmd.Parameters.AddWithValue("@pesel", pesel);

                int rowsAffected = cmd.ExecuteNonQuery();

                // zwracamy true jeśli użytkownik został dodany poprawnie
                return rowsAffected > 0;
            }
        }

        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            // Pobierz hasło z bazy danych na podstawie nazwy użytkownika
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT Password FROM users WHERE Username = @username";
                cmd.Parameters.AddWithValue("@username", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string hashedPassword = reader["Password"].ToString();

                        // Sprawdź, czy stare hasło jest poprawne
                        if (BCrypt.Net.BCrypt.Verify(oldPassword, hashedPassword))
                        {
                            // Haszuj i zaktualizuj nowe hasło w bazie danych
                            string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                            using (var updateCmd = _connection.CreateCommand())
                            {
                                updateCmd.CommandText = "UPDATE users SET Password = @newPassword WHERE Username = @username";
                                updateCmd.Parameters.AddWithValue("@newPassword", newHashedPassword);
                                updateCmd.Parameters.AddWithValue("@username", username);

                                int rowsAffected = updateCmd.ExecuteNonQuery();

                                // Zwróć true, jeśli hasło zostało zaktualizowane
                                return rowsAffected > 0;
                            }
                        }
                    }
                }
            }
            return false;
        }


        public bool AddNewProject(Project project)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO programs (Nazwa_programu, Opis_programu, Fundusz, Data_rozpoczecia, Data_zakonczenia, Osoba_odpowiedzialna, Kategoria_programu) VALUES (@nazwa, @opis, @fundusz, @rozpoczecie, @zakonczenie, @odpowiedzialna, @kategoria)";
                cmd.Parameters.AddWithValue("@nazwa", project.Nazwa_programu);
                cmd.Parameters.AddWithValue("@opis", project.Opis_programu);
                cmd.Parameters.AddWithValue("@fundusz", project.Fundusz);
                cmd.Parameters.AddWithValue("@rozpoczecie", project.Data_rozpoczecia);
                cmd.Parameters.AddWithValue("@zakonczenie", project.Data_zakonczenia);
                cmd.Parameters.AddWithValue("@odpowiedzialna", project.Osoba_odpowiedzialna);
                cmd.Parameters.AddWithValue("@kategoria", project.Kategoria_programu);

                int rowsAffected = cmd.ExecuteNonQuery();

                // Zwracamy true, jeśli projekt został dodany poprawnie
                return rowsAffected > 0;
            }
        }


        public List<Project> GetAvailableProjects(SqliteConnection connection)
        {
            List<Project> projects = new List<Project>();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM programs";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Project project = new Project
                        {
                            Id_programu = Convert.ToInt32(reader["Id_programu"]),
                            Nazwa_programu = reader["Nazwa_programu"].ToString(),
                            Opis_programu = reader["Opis_programu"].ToString(),
                            Fundusz = reader["Fundusz"].ToString(),
                            Data_rozpoczecia = reader["Data_rozpoczecia"].ToString(),
                            Data_zakonczenia = reader["Data_zakonczenia"].ToString(),
                            Osoba_odpowiedzialna = reader["Osoba_odpowiedzialna"].ToString(),
                            Kategoria_programu = reader["Kategoria_programu"].ToString(),
                        };
                        projects.Add(project);
                    }
                }
            }
            return projects;
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
            string[] menuOptions = { "Dostępne Programy", "Dodaj wniosek", "Profil", "Pomoc i Obsługa", "Wyjście", "Wyloguj" };
            string[] loginOptions = { "Logowanie", "Rejestracja", "Wejście bez logowania" };
            bool isLoggedIn = false;
            string username = "";

            Console.ForegroundColor = ConsoleColor.Yellow;

            //SoundPlayer //scrollSound = new SoundPlayer("./sounds/scroll.wav");
            //SoundPlayer //selectSound = new SoundPlayer("./sounds/select.wav");

            Console.WriteLine();
            Console.WriteLine("===========================================");
            Console.WriteLine();

            Console.WriteLine("Witaj w Aplikacji Rządowej!");
            Console.WriteLine();
            Thread.Sleep(1000);

            Console.WriteLine("To narzędzie, które umożliwia Ci korzystanie z różnych programów rządowych oraz składanie wniosków w wygodny i intuicyjny sposób.");
            Thread.Sleep(1000);
            Console.WriteLine("Możesz korzystać z dostępnych programów, składać wnioski oraz zarządzać swoim profilem.");
            Thread.Sleep(1000);
            Console.WriteLine("Dodatkowo, możesz zarządzać swoim profilem, dostosowując go do swoich potrzeb.");
            Thread.Sleep(3000);
            Console.WriteLine("\n");

            AnsiConsole.Status()
                .Start("Synchronizacja bazy danych...", ctx =>
                {
                    // symulacja 
                    AnsiConsole.MarkupLine("Przygotowywanie tabel...");
                    Thread.Sleep(3000);

                    // aktualizacja statusu
                    ctx.Status("Już prawie!");
                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("green"));

                    // symulacja
                    AnsiConsole.MarkupLine("Ładuje...");
                    Thread.Sleep(3000);
                });

            while (!isLoggedIn)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine();
                Console.WriteLine("===========================================");
                Console.WriteLine();
                Console.WriteLine("Witamy w oknie logowania.");
                Console.WriteLine();
                Console.WriteLine("Aby skorzystać z naszej aplikacji prosimy o wybraniu opcji logowania.");
                Console.WriteLine("Proszę wybrać jedną opcję:");
                Console.WriteLine();
                for (int i = 0; i < loginOptions.Length; i++)
                {
                    if (i == selectedOption)
                    {
                        Console.Write(">> ");
                    }
                    Console.WriteLine($"{i + 1}. {loginOptions[i]}");
                }

                keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (selectedOption == 0)
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Logowanie:");
                        Console.Write("Podaj nazwę użytkownika: ");
                        username = Console.ReadLine().Trim(); //usuwanie bialych znakow na poczatku i koncu
                        Console.Write("Podaj hasło: ");
                        string password = "";
                        while (true)
                        {
                            ConsoleKeyInfo key = Console.ReadKey(true);

                            if (key.Key == ConsoleKey.Enter)
                            {
                                break;
                            }
                            else if (key.Key == ConsoleKey.Backspace)
                            {
                                if (password.Length > 0)
                                {
                                    password = password.Substring(0, password.Length - 1);
                                    Console.Write("\b \b"); // usuwamy ostatni znak i przesuwamy kursor w lewo
                                }
                            }
                            else
                            {
                                password += key.KeyChar;
                                Console.Write("*"); // zamiana znakow na gwiazdke
                            }
                        }

                        Console.WriteLine();
                        if (authentication.Login(username, password))
                        {
                            isLoggedIn = true;
                            Console.WriteLine();
                            Console.WriteLine("Zalogowano pomyślnie!");
                            Console.WriteLine();
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("Błąd logowania. Spróbuj ponownie.");
                            Console.WriteLine();
                            Thread.Sleep(1000);
                        }
                    }
                    else if (selectedOption == 1)
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Rejestracja:");
                        Console.Write("Podaj nazwę użytkownika: ");
                        username = Console.ReadLine().Trim(); //usuwanie bialych znakow na poczatku i koncu
                        Console.Write("Podaj hasło: ");
                        string password = "";
                        while (true)
                        {
                            ConsoleKeyInfo key = Console.ReadKey(true);

                            if (key.Key == ConsoleKey.Enter)
                            {
                                break;
                            }
                            else if (key.Key == ConsoleKey.Backspace)
                            {
                                if (password.Length > 0)
                                {
                                    password = password.Substring(0, password.Length - 1);
                                    Console.Write("\b \b"); // usuwamy ostatni znak i przesuwamy kursor w lewo
                                }
                            }
                            else
                            {
                                password += key.KeyChar;
                                Console.Write("*"); // zamiana znaków na gwiazdki
                            }
                        }
                        Console.WriteLine();
                        Console.Write("Podaj imię: ");
                        string imie = Console.ReadLine();
                        Console.Write("Podaj nazwisko: ");
                        string nazwisko = Console.ReadLine();
                        Console.Write("Podaj datę urodzenia (dd.mm.yyyy): ");
                        string data_urodzenia = Console.ReadLine();

                        string pesel = "";
                        do
                        {
                            Console.Write("Podaj pesel: ");
                            pesel = Console.ReadLine();

                            if (pesel.Length != 11)
                            {
                                Console.WriteLine("Numer PESEL musi składać się z dokładnie 11 cyfr. Spróbuj ponownie.");
                            }
                        } while (pesel.Length != 11);

                        Console.Write("Czy zgadzasz się na przetwarzanie swoich danych osobowych? (Tak/Nie): ");
                        string consent = Console.ReadLine();
                        bool agreedToProcessing = consent.Trim().Equals("Tak", StringComparison.OrdinalIgnoreCase);

                        // sprawdzamy czy pola nie są puste i czy użytkownik wyraził zgodę
                        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(imie) || string.IsNullOrWhiteSpace(nazwisko) || string.IsNullOrWhiteSpace(data_urodzenia) || string.IsNullOrWhiteSpace(pesel) || !agreedToProcessing)
                        {
                            Console.WriteLine("Wszystkie pola są wymagane, a także musisz wyrazić zgodę na przetwarzanie danych. Spróbuj ponownie.");
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            if (authentication.Register(username, password, imie, nazwisko, data_urodzenia, pesel))
                            {
                                Console.WriteLine();
                                Console.WriteLine("Zarejestrowano pomyślnie!");
                                Console.WriteLine();
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Błąd rejestracji. Spróbuj ponownie.");
                                Console.WriteLine();
                                Thread.Sleep(1000);
                            }
                        }
                    }
                    else if (selectedOption == 2)
                    {
                        // wejście bez logowania
                        isLoggedIn = true; // użytkownik zalogowany, nie mając konta
                        username = "Gość"; // domyślna nazwa użytkownika dla wejścia bez logowania
                        selectedOption = 0;
                    }

                    Console.WriteLine("Naciśnij dowolny klawisz, aby kontynuować...");
                    Console.ReadKey();
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    selectedOption = Math.Max(0, selectedOption - 1);
                    //scrollSound.Play();
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    selectedOption = Math.Min(2, selectedOption + 1);
                    //scrollSound.Play();
                }
            }

            // głowna pętla menu
            do
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine();
                Console.WriteLine("===========================================");
                Console.WriteLine();
                Console.WriteLine($"Zalogowano jako: {username}");
                Console.WriteLine();
                for (int i = 0; i < menuOptions.Length; i++)
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
                    switch (selectedOption)
                    {
                        case 0:
                            Console.WriteLine("Przechodzisz do Dostępnych Programów.");
                            //selectSound.Play();
                            // funkcja dostępnych programów

                            List<Project> availableProjects = authentication.GetAvailableProjects(dbConnector.GetConnection());

                            // wyswietlenie dostępnych programów
                            int projectOption = 0;
                            int visibleProjects = Math.Min(Console.WindowHeight - 8, availableProjects.Count);

                            while (true)
                            {
                                Console.Clear();
                                Console.WriteLine("Wybierz numer programu: ");
                                int startIndex = Math.Max(0, projectOption - (visibleProjects - 1));
                                int endIndex = Math.Min(availableProjects.Count - 1, startIndex + visibleProjects - 1);

                                for (int i = startIndex; i <= endIndex; i++)
                                {
                                    if (i == projectOption)
                                    {
                                        Console.Write(">> ");
                                    }
                                    Console.WriteLine($"{i + 1}. {availableProjects[i].Nazwa_programu}");
                                }

                                ConsoleKeyInfo projectKeyInfo = Console.ReadKey();
                                if (projectKeyInfo.Key == ConsoleKey.Enter)
                                {
                                    if (projectOption >= 0 && projectOption < availableProjects.Count)
                                    {
                                        Console.Clear();
                                        // po wybraniu projektu wyswietla jego szczegoly
                                        Project selectedProject = availableProjects[projectOption];
                                        Console.WriteLine($"Nazwa programu: {selectedProject.Nazwa_programu} \n");
                                        Console.WriteLine($"Opis programu: {selectedProject.Opis_programu} \n");
                                        Console.WriteLine($"Fundusz: {selectedProject.Fundusz} \n");
                                        Console.WriteLine($"Data rozpoczęcia programu: {selectedProject.Data_rozpoczecia} \n");
                                        Console.WriteLine($"Data zakończenia programu: {selectedProject.Data_zakonczenia} \n");
                                        Console.WriteLine($"Osoba odpowiedzialna: {selectedProject.Osoba_odpowiedzialna} \n");
                                        Console.WriteLine($"Kategoria programu: {selectedProject.Kategoria_programu} \n");
                                        break;
                                    }
                                    else if (projectOption == -1) // -1 oznacza powrót do głównego menu
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Nieprawidłowy wybór. Spróbuj ponownie.");
                                    }
                                }
                                else if (projectKeyInfo.Key == ConsoleKey.UpArrow)
                                {
                                    projectOption = Math.Max(0, projectOption - 1);
                                    //scrollSound.Play();
                                }
                                else if (projectKeyInfo.Key == ConsoleKey.DownArrow)
                                {
                                    projectOption = Math.Min(availableProjects.Count - 1, projectOption + 1);
                                    //scrollSound.Play();
                                }
                            }
                            break;


                        case 1:
                             // funkcja wniosków
                            if(username == "Gość") {
                                Console.WriteLine("Nie masz uprawnień do dodawania wniosków. Zaloguj się aby móc dodać nowy wniosek.");
                                Console.ReadKey();
                                break;
                            }
                            Console.WriteLine("Przechodzisz do Moich Wniosków.");
                            Console.Clear();

                            Console.WriteLine("Dodawanie nowego projektu:");
                            Console.Write("Podaj nazwę projektu: ");
                            string nazwaProjektu = Console.ReadLine();
                            Console.Write("Podaj opis projektu: ");
                            string opisProjektu = Console.ReadLine();
                            Console.Write("Jaki masz fundusz: ");
                            string fundusz = Console.ReadLine();
                            Console.Write("Kiedy zaczynają się zapisy na program? (dd.mm.yyyy): ");
                            string dataRozpoczecia = Console.ReadLine();
                            Console.Write("Kiedy kończą się zapisy na program? (dd.mm.yyyy): ");
                            string dataZakonczenia = Console.ReadLine();
                            Console.Write("Kto jest odpowiedzialny za program?: ");
                            string osobaOpowiedzialna = Console.ReadLine();
                            Console.Write("Podaj kategorię programu: ");
                            string kategoria = Console.ReadLine();
                            // Podobnie pobierz pozostałe dane dla nowego projektu

                            Project newProject = new Project
                            {
                                Nazwa_programu = nazwaProjektu,
                                Opis_programu = opisProjektu,
                                Fundusz = fundusz,
                                Data_rozpoczecia = dataRozpoczecia,
                                Data_zakonczenia = dataZakonczenia,
                                Osoba_odpowiedzialna = osobaOpowiedzialna,
                                Kategoria_programu = kategoria,
                                // Dodaj pozostałe właściwości nowego projektu
                            };

                            if (authentication.AddNewProject(newProject))
                            {
                                Console.WriteLine();
                                Console.WriteLine("Nowy projekt został dodany!");
                                Console.WriteLine();
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Błąd dodawania nowego projektu. Spróbuj ponownie.");
                                Console.WriteLine();
                                Thread.Sleep(1000);
                            }
                            break;
                        case 2:
                            Console.WriteLine("Przechodzisz do Profilu.");
                            //selectSound.Play();
                            // funkcja profilu
                            Console.Clear();
                            Console.WriteLine($"Nazwa użytkownika: {username}");
                            Console.WriteLine();
                            var userProfile = authentication.GetUserProfile(username);

                            if (userProfile != null)
                            {
                                Console.Clear();
                                Console.WriteLine($"Nazwa użytkownika: {username}");
                                Console.WriteLine($"Imię: {userProfile.Imie}");
                                Console.WriteLine($"Nazwisko: {userProfile.Nazwisko}");
                                Console.WriteLine($"Data urodzenia: {userProfile.DataUrodzenia}");
                                Console.WriteLine($"PESEL: {userProfile.Pesel}");
                                Console.WriteLine();
                                Console.WriteLine("Naciśnij enter aby zmienić hasło.");
                                Console.WriteLine();
                                Console.WriteLine("Naciśnij escape aby wyjść.");
                                if (Console.ReadKey().Key == ConsoleKey.Escape)
                                {
                                    break; // wyjście z funkcji profilu.
                                }
                                while (Console.ReadKey().Key != ConsoleKey.Enter)
                                {
                                    Console.Clear();
                                    Console.Write("Podaj aktualne hasło: ");
                                    string oldPassword = "";
                                    while (true)
                                    {
                                        ConsoleKeyInfo key = Console.ReadKey(true);

                                        if (key.Key == ConsoleKey.Enter)
                                        {
                                            break;
                                        }
                                        else if (key.Key == ConsoleKey.Backspace)
                                        {
                                            if (oldPassword.Length > 0)
                                            {
                                                oldPassword = oldPassword.Substring(0, oldPassword.Length - 1);
                                                Console.Write("\b \b");
                                            }
                                        }
                                        else
                                        {
                                            oldPassword += key.KeyChar;
                                            Console.Write("*");
                                        }
                                    }
                                    Console.WriteLine();
                                    Console.Write("Podaj nowe hasło: ");
                                    string newPassword = "";
                                    while (true)
                                    {
                                        ConsoleKeyInfo key = Console.ReadKey(true);

                                        if (key.Key == ConsoleKey.Enter)
                                        {
                                            break;
                                        }
                                        else if (key.Key == ConsoleKey.Backspace)
                                        {
                                            if (newPassword.Length > 0)
                                            {
                                                newPassword = newPassword.Substring(0, newPassword.Length - 1);
                                                Console.Write("\b \b");
                                            }
                                        }
                                        else
                                        {
                                            newPassword += key.KeyChar;
                                            Console.Write("*");
                                        }
                                    }
                                    if (authentication.ChangePassword(username, oldPassword, newPassword))
                                    {
                                        Console.Clear();
                                        Console.WriteLine("Hasło zostało pomyślnie zmienione.");
                                    }
                                    else
                                    {
                                        Console.Clear();
                                        Console.WriteLine("Błąd podczas zmiany hasła. Spróbuj ponownie.");
                                    }

                                }
                            }
                            break;
                        case 3:
                            Console.WriteLine("Przechodzisz do Pomocy i Obsługi.");
                            //selectSound.Play();
                            // funkcja pomocy
                            break;
                        case 4:
                            Console.WriteLine("Zamykanie aplikacji.");
                            return;
                        case 5:
                            // wylogowanie uzytkownika
                            isLoggedIn = false;
                            username = "";
                            selectedOption = 0;
                            break;
                    }

                    Console.WriteLine();
                    Console.WriteLine("Naciśnij dowolny klawisz, aby kontynuować...");
                    Console.ReadKey();
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    selectedOption = Math.Max(0, selectedOption - 1);
                    //scrollSound.Play();
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    selectedOption = Math.Min(menuOptions.Length - 1, selectedOption + 1);
                    //scrollSound.Play();
                }
            } while (keyInfo.Key != ConsoleKey.Escape);
        }
    }
}
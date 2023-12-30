﻿using System;
using System.Threading;
using Microsoft.Data.Sqlite;
using System.Media;
using System.Globalization;
using BCrypt.Net;
using System.Text;


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
        public bool UpdateUserProfile(string username, string newName, string newSurname, string newDateOfBirth, string newPesel)
        {
            var updateSet = new List<string>();
            if (!string.IsNullOrWhiteSpace(newName))
            {
                updateSet.Add("Imie = @newName");
            }
            if (!string.IsNullOrWhiteSpace(newSurname))
            {
                updateSet.Add("Nazwisko = @newSurname");
            }
            if (!string.IsNullOrWhiteSpace(newDateOfBirth))
            {
                updateSet.Add("data_urodzenia = @newDateOfBirth");
            }
            if (!string.IsNullOrWhiteSpace(newPesel) && newPesel.Length == 11)
            {
                updateSet.Add("Pesel = @newPesel");
            }
            else if (!string.IsNullOrWhiteSpace(newPesel))
            {
                Console.WriteLine("Numer PESEL musi składać się z dokładnie 11 cyfr. Spróbuj ponownie.");
                Console.ReadKey();
                return false; // Nie aktualizuj, jeśli PESEL nie ma 11 znaków
            }

            if (updateSet.Count == 0)
            {
                return false; // Nie ma żadnych zmian do wprowadzenia
            }

            using (var cmd = _connection.CreateCommand())
            {
                string updateQuery = string.Join(", ", updateSet);
                cmd.CommandText = $"UPDATE users SET {updateQuery} WHERE Username = @username";
                cmd.Parameters.AddWithValue("@username", username);
                if (updateSet.Contains("Imie = @newName"))
                    cmd.Parameters.AddWithValue("@newName", newName);
                if (updateSet.Contains("Nazwisko = @newSurname"))
                    cmd.Parameters.AddWithValue("@newSurname", newSurname);
                if (updateSet.Contains("data_urodzenia = @newDateOfBirth"))
                    cmd.Parameters.AddWithValue("@newDateOfBirth", newDateOfBirth);
                if (updateSet.Contains("Pesel = @newPesel"))
                    cmd.Parameters.AddWithValue("@newPesel", newPesel);

                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected > 0;
            }
        }

        private SqliteConnection _connection;


        public Authentication(SqliteConnection connection)
        
        {
            _connection = connection;
        }


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


        public bool Login(string username, string password, out bool isAdmin)
        {
            isAdmin = false;

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT UserId, Password, Admin FROM users WHERE Username = @username";
                cmd.Parameters.AddWithValue("@username", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string hashedPassword = reader["Password"].ToString();
                        string adminValue = reader["Admin"].ToString();

                        // sprawdzenie, czy wprowadzone hasło zgadza się z zahaszowanym hasłem z bazy danych
                        if (BCrypt.Net.BCrypt.Verify(password, hashedPassword))
                        {
                            // Ustawienie wartości isAdmin na true, jeśli użytkownik jest administratorem
                            isAdmin = adminValue.Equals("tak", StringComparison.OrdinalIgnoreCase);

                            // zwraca true, jesli hasla się zgadzają
                            return true;
                        }
                    }
                }
                // zwraca false jesli hasla sie nie zgadzaja lub uzytkownik nie istnieje
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
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT Password FROM users WHERE Username = @username";
                cmd.Parameters.AddWithValue("@username", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string hashedPassword = reader["Password"].ToString();
                        if (BCrypt.Net.BCrypt.Verify(oldPassword, hashedPassword))
                        {
                            string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                            using (var updateCmd = _connection.CreateCommand())
                            {
                                updateCmd.CommandText = "UPDATE users SET Password = @newPassword WHERE Username = @username";
                                updateCmd.Parameters.AddWithValue("@newPassword", newHashedPassword);
                                updateCmd.Parameters.AddWithValue("@username", username);

                                int rowsAffected = updateCmd.ExecuteNonQuery();
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

                // zwraca true jesli projekt jest poprawnie dodany
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


        public List<UserProfile> GetAllUsers()
        {
            List<UserProfile> usersList = new List<UserProfile>();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT Imie, Nazwisko, data_urodzenia, Pesel FROM users";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        UserProfile user = new UserProfile
                        {
                            Imie = reader["Imie"].ToString(),
                            Nazwisko = reader["Nazwisko"].ToString(),
                            DataUrodzenia = reader["data_urodzenia"].ToString(),
                            Pesel = reader["Pesel"].ToString()
                        };

                        usersList.Add(user);
                    }
                }
            }

            return usersList;
        }


        public bool DeleteUser(string usernameToDelete)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM users WHERE Username = @usernameToDelete";
                cmd.Parameters.AddWithValue("@usernameToDelete", usernameToDelete);

                int rowsAffected = cmd.ExecuteNonQuery();

                // zwraca true jesli uzytkownik zostal poprawnie usuniety
                return rowsAffected > 0;
            }
        }


        public bool MakeUserAdmin(string username)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "UPDATE users SET Admin = 'tak' WHERE Username = @username";
                cmd.Parameters.AddWithValue("@username", username);

                int rowsAffected = cmd.ExecuteNonQuery();

                // zwraca true jesli użytkownik został pomyślnie ustawiony jako administrator
                return rowsAffected > 0;
            }
        }

    }


    internal class Program
    {
        private static string ReadPassword()
        {
            var password = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b"); // usuwa ostatni znak z konsoli
                }
                else if (key.Key != ConsoleKey.Backspace)
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }
            }
            Console.WriteLine();
            return password.ToString();
        }

        private static void Main(string[] args)
        {
            DatabaseConnector dbConnector = new DatabaseConnector("./govapp.sqlite");
            dbConnector.OpenConnection();
            Authentication authentication = new Authentication(dbConnector.GetConnection());

            ConsoleKeyInfo keyInfo;
            int selectedOption = 0;
            string[] menuOptions = { "Dostępne Programy", "Dodaj wniosek", "Profil", "Pomoc i FAQ", "Wyjście", "Wyloguj" };
            string[] loginOptions = { "Logowanie", "Rejestracja", "Wejście bez logowania" };
            bool isLoggedIn = false;
            string username = "";
            bool isAdmin;

            Console.ForegroundColor = ConsoleColor.Yellow; //kolor czcionki

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

            while (!isLoggedIn)
            {
                LoginScreen();
            }

            void LoginScreen()
            {
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
                            username = Console.ReadLine().Trim().ToLower(); //usuwanie bialych znakow na poczatku i koncu
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
                            if (authentication.Login(username, password, out isAdmin))
                            {
                                if (isAdmin)
                                {
                                    menuOptions = new string[] { "Dostępne Programy", "Moje Wnioski", "Profil", "Pomoc i FAQ", "Wyjście", "Wyloguj", "Panel Admina" };
                                }
                                else
                                {
                                    menuOptions = new string[] { "Dostępne Programy", "Moje Wnioski", "Profil", "Pomoc i FAQ", "Wyjście", "Wyloguj" };
                                }
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
                            username = Console.ReadLine().Trim().ToLower(); //usuwanie bialych znakow na poczatku i koncu
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
                            bool isValidDate;
                            string data_urodzenia;
                            do
                            {
                                Console.Write("Podaj datę urodzenia (dd.mm.yyyy): ");
                                data_urodzenia = Console.ReadLine();

                                // Sprawdzenie poprawności formatu daty
                                DateTime dob;
                                isValidDate = DateTime.TryParseExact(data_urodzenia, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dob);
                                if (!isValidDate)
                                {
                                    Console.WriteLine("Nieprawidłowy format daty. Podaj datę w formacie dd.mm.yyyy. Spróbuj ponownie.");
                                }

                            } while (!isValidDate);

                            string pesel;
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
                    }
                    else if (keyInfo.Key == ConsoleKey.DownArrow)
                    {
                        selectedOption = Math.Min(2, selectedOption + 1);
                    }
                }
            }

            // głowna pętla menu
            do
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;  // zółta czcionka
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
                                        Console.WriteLine($"Fundusz: {selectedProject.Fundusz} zł\n");
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
                                }
                                else if (projectKeyInfo.Key == ConsoleKey.DownArrow)
                                {
                                    projectOption = Math.Min(availableProjects.Count - 1, projectOption + 1);
                                }
                            }
                            break;


                        case 1:
                            // funkcja wniosków
                            if (username == "Gość")
                            {
                                Console.WriteLine("Nie masz uprawnień do dodawania wniosków. Zaloguj się aby móc dodać nowy wniosek.");
                                Console.ReadKey();
                                break;
                            }
                            Console.WriteLine("Przechodzisz do okna tworzenia wniosku...");
                            Console.Clear();

                            Console.WriteLine("Witamy w kreatorze wniosków. Tutaj możesz utworzyć własny projekt. Jeśli chcesz wyjśc kliknij 'escape'");
                            if (Console.ReadKey().Key == ConsoleKey.Escape)
                            {
                                break; // wyjście z funkcji profilu.
                            }
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

                            Project newProject = new Project
                            {
                                Nazwa_programu = nazwaProjektu,
                                Opis_programu = opisProjektu,
                                Fundusz = fundusz,
                                Data_rozpoczecia = dataRozpoczecia,
                                Data_zakonczenia = dataZakonczenia,
                                Osoba_odpowiedzialna = osobaOpowiedzialna,
                                Kategoria_programu = kategoria,
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
                            if (username == "Gość")
                            {
                                Console.WriteLine("Nie możesz zobaczyć swojego profilu, ponieważ jesteś niezalogowany.");
                                Console.ReadKey();
                                break;
                            }
                            Console.WriteLine("\nPrzechodzisz do Profilu.");
                            Thread.Sleep(1000);

                            // funkcja profilu
                            var userProfile = authentication.GetUserProfile(username);
                            int profileOption = 0;
                            string[] profileOptions = { "Zaktualizuj dane profilowe", "Zmień hasło", "Wyjście" };
                            bool isProfileMenuActive = true;
                            while (isProfileMenuActive)
                            {
                                Console.Clear();
                                Console.WriteLine($"Nazwa użytkownika: {username}");
                                Console.WriteLine($"Imię: {userProfile.Imie}");
                                Console.WriteLine($"Nazwisko: {userProfile.Nazwisko}");
                                Console.WriteLine($"Data urodzenia: {userProfile.DataUrodzenia}");
                                Console.WriteLine($"PESEL: {userProfile.Pesel}");
                                Console.WriteLine();
                                for (int i = 0; i < profileOptions.Length; i++)
                                {
                                    Console.WriteLine(profileOption == i ? $">> {profileOptions[i]}" : profileOptions[i]);
                                }

                                var key = Console.ReadKey();
                                if (key.Key == ConsoleKey.UpArrow)
                                {
                                    profileOption = profileOption == 0 ? profileOptions.Length - 1 : profileOption - 1;
                                }
                                else if (key.Key == ConsoleKey.DownArrow)
                                {
                                    profileOption = profileOption == profileOptions.Length - 1 ? 0 : profileOption + 1;
                                }
                                else if (key.Key == ConsoleKey.Enter)
                                {
                                    switch (profileOption)
                                    {
                                        case 0:
                                                // Dodanie opcji aktualizacji danych profilowych
                                                    Console.Write("Podaj nowe imię (lub naciśnij Enter, aby pozostawić aktualne): ");
                                                    string newName = Console.ReadLine();
                                                    Console.Write("Podaj nowe nazwisko (lub naciśnij Enter, aby pozostawić aktualne): ");
                                                    string newSurname = Console.ReadLine();
                                                    bool validDate;
                                                    string newDateOfBirth;
                                                    do
                                                    {
                                                        Console.Write("Podaj nową datę urodzenia (dd.mm.yyyy, lub naciśnij Enter, aby pozostawić aktualną): ");
                                                        newDateOfBirth = Console.ReadLine();
                                                        if (string.IsNullOrWhiteSpace(newDateOfBirth))
                                                        {
                                                            break; // Użytkownik nie chce zmieniać daty
                                                        }
                                                        validDate = DateTime.TryParseExact(newDateOfBirth, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
                                                        if (!validDate)
                                                        {
                                                            Console.WriteLine("Nieprawidłowy format daty. Użyj formatu dd.mm.yyyy.");
                                                        }
                                                    } 
                                                    while (!validDate);

                                                    Console.Write("Podaj nowy PESEL (lub naciśnij Enter, aby pozostawić aktualny): ");
                                                    string newPesel = Console.ReadLine();

                                                    // Sprawdzenie, czy użytkownik wprowadził jakiekolwiek dane
                                                    if (!string.IsNullOrWhiteSpace(newName) || !string.IsNullOrWhiteSpace(newSurname) || !string.IsNullOrWhiteSpace(newDateOfBirth) || !string.IsNullOrWhiteSpace(newPesel))
                                                    {
                                                        if (authentication.UpdateUserProfile(username, newName, newSurname, newDateOfBirth, newPesel))
                                                        {
                                                            Console.WriteLine("Dane profilowe zostały zaktualizowane.");
                                                            Thread.Sleep(1000);
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine("Nie udało się zaktualizować danych profilowych.");
                                                            Thread.Sleep(1000);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Nie wprowadzono żadnych zmian.");
                                                        Thread.Sleep(1000);
                                                    }


                                                break;
                                        case 1:
                                        //zmiana hasła
                                        Console.Write("Podaj stare hasło: ");
                                        string oldPassword = ReadPassword();
                                        Console.Write("Podaj nowe hasło: ");
                                        string newPassword = ReadPassword();
                                            if (authentication.ChangePassword(username, oldPassword, newPassword))
                                            {
                                                Console.WriteLine("Hasło zostało zmienione.");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Błąd przy zmianie hasła. Upewnij się, że stare hasło jest poprawne.");
                                            }
                                            Thread.Sleep(2000);
                                            break;
                                        case 2:
                                            // Wyjście z menu profilu
                                            isProfileMenuActive = false;
                                            break;
                                            
                                    }
                                }
                            }
                            break;


                        case 3:
                            Console.WriteLine("\nPrzechodzisz do Pomocy i FAQ.");
                            Thread.Sleep(1000);
                            Console.Clear();

                            // intrukcja obsługi
                            Console.WriteLine("### Korzystanie z Aplikacji Rządowej ###");

                            Console.WriteLine("\nLogowanie i Rejestracja: ");
                            Console.WriteLine("- Aby korzystać z funkcji aplikacji, zaloguj się swoimi danymi lub zarejestruj nowe konto.");
                            Console.WriteLine("- W przypadku braku konta, wybierz opcję 'Rejestracja' i wypełnij wszystkie wymagane pola.");

                            Console.WriteLine("\nMenu Główne: ");
                            Console.WriteLine("- Po zalogowaniu się użytkownik ma dostęp do głównego menu, gdzie może wybierać spośród różnych opcji, takich jak 'Dostępne Programy', 'Moje Wnioski', 'Profil', 'Pomoc i Obsługa', 'Wyjście' lub 'Wyloguj'.");

                            Console.WriteLine("\nZarządzanie Profilem: ");

                            Console.WriteLine("- W sekcji 'Profil' użytkownik może przeglądać swoje dane osobowe, a także zmieniać hasło.");

                            Console.WriteLine("\n Dodawanie Nowego Projektu: ");
                            Console.WriteLine("- W przypadku posiadania konta, użytkownik ma możliwość dodania nowego projektu do bazy danych aplikacji poprzez wybranie opcji 'Moje Wnioski'.");

                            Console.WriteLine("\nWylogowanie: ");
                            Console.WriteLine("- Po zakończeniu korzystania z aplikacji, wyloguj się, aby zabezpieczyć swoje konto.\n");

                            Console.WriteLine("\n### FAQ (Najczęściej Zadawane Pytania) ###");
                            Console.WriteLine("\nPytanie: Czy aplikacja jest bezpieczna dla moich danych osobowych?");
                            Console.WriteLine("Odpowiedź: Tak, aplikacja dba o bezpieczeństwo danych. Hasła są przechowywane w formie zahaszowanej, co zabezpiecza informacje osobiste użytkowników.");

                            Console.WriteLine("\nPytanie: Czy mogę korzystać z aplikacji bez logowania?");
                            Console.WriteLine("Odpowiedź: Tak, istnieje możliwość wejścia do aplikacji bez logowania poprzez wybranie opcji 'Wejście bez logowania'. Jednakże, pewne funkcje mogą być niedostępne dla gości.");

                            Console.WriteLine("\nPytanie: Jak dodam nowy projekt do aplikacji?");
                            Console.WriteLine("Odpowiedź: Po zalogowaniu się, przejdź do sekcji 'Moje Wnioski' i wybierz opcję dodawania nowego projektu. Uzupełnij wymagane pola i zapisz informacje.");

                            Console.WriteLine();
                            Console.WriteLine("Naciśnij klawisz Enter, aby wrócić do menu głównego.");

                            // oczekiwanie na naciśnięcie Enter, aby wrócić do menu głównego
                            while (Console.ReadKey().Key != ConsoleKey.Enter)
                            {
                                // oczekiwanie na naciśnięcie Enter
                            }

                            break;

                        case 4:
                            Console.WriteLine("Zamykanie aplikacji."); // zamyka program
                            return;


                        case 5:
                            // wylogowanie uzytkownika
                            Console.WriteLine("\nWylogowywanie...");
                            Thread.Sleep(1000);
                            isLoggedIn = false;
                            username = "";
                            selectedOption = 0;
                            LoginScreen();
                            break;

                        case 6:
                            //panel admina
                            if (username == "Gość")
                            {
                                Console.WriteLine("Nie masz uprawnień do panelu admina. Zaloguj się jako admin.");
                                Console.ReadKey();
                                break;
                            }
                            Console.WriteLine("Przechodzenie do panelu admina...");
                            Thread.Sleep(1000);
                            Console.Clear();

                            int adminOption = 1;

                            do
                            {
                                Console.Clear();
                                Console.WriteLine("Witaj w panelu administratora.");
                                Console.Write("Wybierz opcję: \n");

                                for (int i = 1; i <= 4; i++)
                                {
                                    if (i == adminOption)
                                    {
                                        Console.Write(">> ");
                                    }
                                    Console.WriteLine($"{i}. {(i == 1 ? "Przeglądaj konta" : i == 2 ? "Usuń konto" : i == 3 ? "Dodaj nowego admina" : "Wyjdź z panelu")}");
                                }

                                ConsoleKeyInfo adminKey = Console.ReadKey();

                                if (adminKey.Key == ConsoleKey.UpArrow)
                                {
                                    adminOption = Math.Max(1, adminOption - 1);
                                }
                                else if (adminKey.Key == ConsoleKey.DownArrow)
                                {
                                    adminOption = Math.Min(4, adminOption + 1);
                                }
                                else if (adminKey.Key == ConsoleKey.Enter)
                                {
                                    break; // przerwij pętlę po klinknieciu enter
                                }
                            } while (true);

                            switch (adminOption)
                            {
                                case 1: // przeglądanie kont
                                    Console.Clear();
                                    Console.WriteLine("Lista użytkowników:");

                                    List<UserProfile> usersListCase1 = authentication.GetAllUsers();

                                    if (usersListCase1 != null && usersListCase1.Count > 0)
                                    {
                                        foreach (var user in usersListCase1)
                                        {
                                            Console.WriteLine($"Imię: {user.Imie}, Nazwisko: {user.Nazwisko}, Data urodzenia: {user.DataUrodzenia}, PESEL: {user.Pesel}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Brak użytkowników.");
                                    }
                                    break;

                                case 2: // usuwanie konta
                                    Console.Clear();

                                    Console.WriteLine("Lista użytkowników:");
                                    List<UserProfile> usersListCase2 = authentication.GetAllUsers();

                                    for (int i = 0; i < usersListCase2.Count; i++)
                                    {
                                        Console.WriteLine($"{i + 1}. {usersListCase2[i].Imie} {usersListCase2[i].Nazwisko}");
                                    }

                                    Console.Write("\nWybierz numer użytkownika do usunięcia: ");
                                    int userIndexToDelete;
                                    bool isIndexValid = int.TryParse(Console.ReadLine(), out userIndexToDelete);

                                    if (isIndexValid && userIndexToDelete > 0 && userIndexToDelete <= usersListCase2.Count)
                                    {
                                        string userToDelete = usersListCase2[userIndexToDelete - 1].Imie;

                                        bool isUserDeleted = authentication.DeleteUser(userToDelete);

                                        if (isUserDeleted)
                                        {
                                            Console.WriteLine($"Użytkownik {userToDelete} został pomyślnie usunięty.");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Nie udało się usunąć użytkownika {userToDelete}.");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Nieprawidłowy wybór.");
                                    }
                                    break;

                                case 3: // dodawanie nowego admina
                                    Console.Clear();
                                    Console.WriteLine("Lista użytkowników:");
                                    List<UserProfile> usersListCase3 = authentication.GetAllUsers();

                                    for (int i = 0; i < usersListCase3.Count; i++)
                                    {
                                        Console.WriteLine($"{i + 1}. {usersListCase3[i].Imie} {usersListCase3[i].Nazwisko}");
                                    }

                                    Console.Write("\nWybierz numer użytkownika do zmiany na admina: ");
                                    int userIndexToAdmin;
                                    bool isIndexValidForAdmin = int.TryParse(Console.ReadLine(), out userIndexToAdmin);

                                    if (isIndexValidForAdmin && userIndexToAdmin > 0 && userIndexToAdmin <= usersListCase3.Count)
                                    {
                                        string userToMakeAdmin = usersListCase3[userIndexToAdmin - 1].Imie;

                                        bool isUserMadeAdmin = authentication.MakeUserAdmin(userToMakeAdmin);

                                        if (isUserMadeAdmin)
                                        {
                                            Console.WriteLine($"Użytkownik {userToMakeAdmin} został pomyślnie zmieniony na administratora.");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Nie udało się zmienić użytkownika {userToMakeAdmin} na administratora.");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Nieprawidłowy wybór.");
                                    }
                                    break;

                                case 4:
                                    break;

                                default:
                                    Console.WriteLine("Nieprawidłowy wybór.");
                                    break;
                            }
                            break;
                    }

                    Console.WriteLine();
                    Console.WriteLine("Naciśnij dowolny klawisz, aby kontynuować...");
                    Console.ReadKey();
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    selectedOption = Math.Max(0, selectedOption - 1);
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    selectedOption = Math.Min(menuOptions.Length - 1, selectedOption + 1);
                }
            } while (keyInfo.Key != ConsoleKey.Escape);
        }
    }
}
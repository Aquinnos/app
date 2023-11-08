﻿using System;
using System.Threading;
using Microsoft.Data.Sqlite;
using System.Media;
using Spectre.Console;
using BCrypt.Net;

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
            string[] menuOptions = { "Dostępne Programy", "Moje Wnioski", "Profil", "Pomoc i Obsługa", "Wyjście", "Wyloguj" };
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
                            break;
                        case 1:
                            Console.WriteLine("Przechodzisz do Moich Wniosków.");
                            //selectSound.Play();
                            // funkcja wniosków
                            break;
                        case 2:
                            Console.WriteLine("Przechodzisz do Profilu.");
                            //selectSound.Play();
                            // funkcja profilu
                            break;
                        case 3:
                            Console.WriteLine("Przechodzisz do Pomocy i Obsługi.");
                            //selectSound.Play();
                            // funkcja pomocy
                            break;
                        case 4:
                            Console.WriteLine("Zamykanie aplikacji.");
                            return;
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
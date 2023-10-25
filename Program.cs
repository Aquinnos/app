using System;

internal class Program
{
    private static void Main(string[] args)
    {
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
                        Console.WriteLine("Przechodzisz do Dostępnych Programów"); //funkcja programow
                        break;
                    case "2":
                        Console.WriteLine("Przechodzisz do Moich Wniosków"); // funkcja wnioski
                        break;
                    case "3":
                        Console.WriteLine("Przechodzisz do Profilu"); //funkcja profilu uzytkownika
                        break;
                    case "4":
                        Console.WriteLine("Przechodzisz do Pomocy i Obsługi"); //funkcja  pomocy i obslugi (moze jakieś faq)
                        break;
                    case "5":
                        Console.WriteLine("Zamykanie aplikacji."); //funkcja
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

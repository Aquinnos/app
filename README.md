# app
projekt na pp

Organizacja:


Baza danych:

- panel phpmyadmin
- xampp (serwer apache i mysql)
- nazwa bazy "govapp"
- tabele przechowywać będą dane logowania i programów (projektów)


Zawartość:

- projekty:
  1. ID programu: Unikalny identyfikator programu rządowego.
  2. Nazwa programu: Nazwa programu lub inna krótka nazwa identyfikująca program.
  3. Opis programu: Szczegółowy opis programu, który może zawierać cele, zakres i informacje ogólne na jego temat.
  4. Fundusz: Informacja o dostępnym funduszu lub budżecie przeznaczonym na program.
  5. Data rozpoczęcia: Data rozpoczęcia programu. (obojetnie)
  6. Data zakończenia: Data planowanego zakończenia programu. (obojetnie)
  7. Osoba odpowiedzialna: Informacje o osobie odpowiedzialnej za program.
  8. Kategoria programu: Kategoria lub obszar, do którego należy program (np. edukacja, zdrowie, kultura).

- wnioski:
 * dane (wymagane)
 * dokumenty (pliki) ale jeszcze zobaczymy
- opcja pomocy
- profil użytkownika
- panel admina


Interfejs użytkownika:
- format tekstu
- menu użytkownika
- kolorki, znaki specjalne


Formularze:

- Formularz logowania:
 * możliwość wejścia bez logowania
 * rejestracja 
 * logowanie
 * możliwy reset hasła

- Formularz wniosku:
 * wybranie projektu
 * dane osobowe (wymagane)
 * pliki (ale jeszcze zobaczymy)
 * wysłanie w celu weryfikacji


Uprawnienia: 

- Niezalogowany (gość - najmniejsze uprawnienia):
 * wyświetlanie projektu
 * zachęcanie do założenia konta

- Zalogowany:
 * wyświetlanie projektów
 * możliwość stworzenia wniosku

- Admin:
 * wyświetlanie projektów
 * możliwość stworzenia wniosku
 * możliwość usunięcia konta
 * możliwość dodawania nowego admina

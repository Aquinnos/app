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
 * opis
 * fundusz
 * osoba finansująca
 * termin
 * środki na fundusz

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
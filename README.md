# 🍷 A&B Drinks - Premium Selection
 
> **O platformă e-commerce exclusivistă, redefinind eleganța în comerțul digital.**
 
Această aplicație oferă o experiență de cumpărături premium pentru entuziaștii de băuturi fine, combinând un design sofisticat cu funcționalități tehnice avansate. Construită pe **ASP.NET Core 9.0**, platforma asigură performanță, securitate și scalabilitate.
 
---
 
##  Funcționalități Cheie
 
###  Experiență de Cumpărături
- **Catalog Premium**: Navigare fluidă prin produse, cu filtrare avansată și detalii esențiale.
- **Coș de Cumpărături & Wishlist**: Gestionare intuitivă a produselor dorite.
- **Procesare Comenzi**: Flux complet de la plasarea comenzii până la livrare.
 
###  Sistem Avansat de Roluri (RBAC)
- **Administrator**: Control total asupra platformei (categorii, produse, utilizatori).
- **Colaborator**: Parteneri care pot propune produse noi spre aprobare.
- **Utilizator**: Acces la catalog, istoric comenzi și funcții sociale.
- **Guest**: Acces limitat pentru vizitatori.
 
###  Integrare AI & Chat
- **Asistent Virtual**: Modul de chat inteligent (`ProductChatController`) care răspunde la întrebări despre produse.
- **Istoric Chat**: Monitorizare și analiză a interacțiunilor clienților.
 
###  Design & UI
- **Estetică Dark/Gold**: O temă vizuală "Glassmorphism" cu accente aurii pentru o atmosferă de lux.
- **Responsive**: Optimizat perfect pentru desktop și mobil.
- **Interfață Modernă**: Utilizare Bootstrap 5 cu personalizări CSS avansate.
 
---
 
##  Stack Tehnologic
 
Proiectul utilizează cele mai noi tehnologii din ecosistemul .NET:
 
| Componentă | Tehnologie | Descriere |
|:---|:---|:---|
| **Backend** | ASP.NET Core 9.0 | Framework web performant și modular. |
| **ORM** | Entity Framework Core | Acces la date (SQL Server / PostgreSQL). |
| **Auth** | ASP.NET Core Identity | Autentificare, Autorizare, Roluri. |
| **Frontend** | Razor Views + Bootstrap 5 | Randare server-side cu interfață modernă. |
| **AI** | Custom AI Service | Serviciu integrat pentru răspunsuri automate. |
 
---
 
##  Instalare și Configurare
 
Urmează acești pași pentru a rula proiectul local:
 
1.  **Clonează Repository-ul**
    ```bash
    git clone https://github.com/VladAIr31/OnlineShop.git
    cd OnlineShop
    ```
 
2.  **Configurare Bază de Date**
    Asigură-te că ai SQL Server instalat și actualizează connection string-ul în `appsettings.json`.
 
3.  **Aplicare Migrări**
    ```powershell
    Update-Database
    ```
 
4.  **Pornire Aplicație**
    ```powershell
    dotnet run
    ```
    Accesează aplicația la `https://localhost:7198` (sau portul configurat).
 
---
 
##  Structură Proiect
 
- `Controllers/`: Logica de business (Products, Cart, Orders, Admin).
- `Models/`: Entități EF Core și ViewModels.
- `Views/`: Interfața utilizator (Razor).
- `wwwroot/`: Resurse statice (CSS, JS, Imagini).
- `Data/`: Contextul bazei de date și Migrări.
 
---
 
<div align="center">
  <sub>Creat cu pasiune pentru excelență. © 2025 A&B Drinks.</sub>
</div>
 

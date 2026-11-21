# 🎬 Cinema

A cinema booking web application built with **ASP.NET Core MVC (.NET 10)**, **Entity Framework Core** and **SQL Server**.
Browse movies, pick a showtime, choose your seats on a live seat map, and pay online with Stripe.

## ✨ Features

### Customers
- Browse movies with search, category / cinema / actor filters and pagination
- Movie details page with gallery, cast and related movies
- **Showtimes for the next 7 days**, grouped by cinema, with seats-left counter
- **Interactive seat map** (available / selected / taken), up to 8 seats per showtime
- **10-minute seat hold** with a live countdown in the cart
- Secure online payment with **Stripe Checkout** (payment is verified server-side)
- **My tickets** page with a booking code for every seat
- Registration, login, profile management (ASP.NET Core Identity)

### Admin panel
- Role-based access (`SuperAdmin`, `Admin`, `Customer`)
- CRUD for movies, actors, cinemas, categories and extra images
- **Halls** with auto-generated seats (rows A–Z, numbered seats)
- **Showtimes** with bulk scheduling (several days × several times) and overlap protection
- Paid bookings list
- User management (lock / unlock accounts)

## 🛠️ Tech stack

| Layer | Technology |
|-------|------------|
| Framework | ASP.NET Core MVC, Areas (`Admin`, `Client`) |
| Data | Entity Framework Core, SQL Server |
| Auth | ASP.NET Core Identity, role-based authorization |
| Payments | Stripe Checkout (`Stripe.net`) |
| UI | Bootstrap 5, custom dark theme (CSS + vanilla JS) |

## 🚀 Getting started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB or full)
- A [Stripe](https://stripe.com) account (test mode is fine)

### Setup

```bash
git clone https://github.com/<your-username>/Cinema.git
cd Cinema
```

1. **Connection string** — edit `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=CinemaDb;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

2. **Stripe keys** — store them in User Secrets (never commit them):
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
   ```
   > Use the same key name your `Program.cs` reads.

3. **Create the database**:
   ```bash
   dotnet ef database update
   ```
   (or `Update-Database` in the Package Manager Console)

4. **Run**:
   ```bash
   dotnet run
   ```

On first start the app seeds the roles and a default Super Admin account.

### Default admin account

| Email | Password |
|-------|----------|
| `superadmin@cinema.com` | `SuperAdmin@123` |

> ⚠️ Change this password (or the seeding code) before deploying anywhere public.

### Test payment

Use Stripe's test card: `4242 4242 4242 4242`, any future expiry date, any CVC.

## 🎟️ How booking works

1. An admin creates a **hall** (e.g. 8 rows × 12 seats) and adds **showtimes** for a movie.
2. A customer opens a movie, picks a showtime and selects seats.
3. Seats are **held for 10 minutes** in the cart. Other users see them as taken.
4. On checkout the seats become *pending tickets* and the customer is sent to Stripe (they stay reserved while paying).
5. After a verified payment the order becomes **Paid** and the tickets show up in **My tickets**.
6. Expired holds and unpaid orders are cleaned up automatically, and a database unique index on `(ShowtimeId, SeatId)` prevents double booking.

## 📁 Project structure

```
Cinema/
├── Areas/
│   ├── Admin/      # Controllers, ViewModels, Views for the admin panel
│   └── Client/     # Public site: movies, booking, cart, checkout, account
├── Data/           # ApplicationDbContext, migrations
├── Helpers/        # SD (roles), DbInitializer, SeatLayout, BookingService
├── Models/         # Movie, Actor, Cinemas, Hall, Seat, Showtime, CartItem, Order, Ticket ...
├── ViewModels/
├── Views/Shared/   # Layouts and shared partials
└── wwwroot/        # CSS, JS, images
```

## 🗺️ Roadmap

- [ ] Remove `Movie.CinemaId` (cinema is derived from showtimes)
- [ ] AdminLTE-style dashboard with sales charts
- [ ] Stripe webhook for payment confirmation
- [ ] Email tickets / QR codes
- [ ] Admin orders and refunds

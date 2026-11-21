# 🎬 Cinema

موقع لحجز تذاكر السينما مبني بـ **ASP.NET Core MVC (.NET 10)** و**Entity Framework Core** و**SQL Server**.
تتصفح الأفلام، تختار موعد العرض، تحجز كراسيك من خريطة القاعة، وتدفع أونلاين عن طريق Stripe.

## ✨ المميزات

### للعملاء
- تصفح الأفلام مع بحث وفلترة (تصنيف / سينما / ممثل) وتقسيم صفحات
- صفحة تفاصيل الفيلم: معرض صور، طاقم التمثيل، وأفلام مشابهة
- **مواعيد العرض للـ 7 أيام الجاية** مقسمة بالسينما، مع عدد الكراسي المتاحة
- **خريطة كراسي تفاعلية** (متاح / مختار / محجوز) وبحد أقصى 8 كراسي للموعد الواحد
- **حجز مؤقت للكراسي لمدة 10 دقايق** مع عدّاد تنازلي في السلة
- دفع آمن بـ **Stripe Checkout** (التحقق من الدفع بيتم على السيرفر)
- صفحة **My tickets** فيها كود حجز لكل كرسي
- تسجيل حساب، تسجيل دخول، وإدارة البروفايل (ASP.NET Core Identity)

### لوحة التحكم (Admin)
- صلاحيات حسب الدور (`SuperAdmin` و`Admin` و`Customer`)
- إضافة وتعديل وحذف: الأفلام، الممثلين، السينمات، التصنيفات، والصور الإضافية
- **القاعات** مع توليد الكراسي تلقائياً (صفوف A–Z وكراسي مرقمة)
- **مواعيد العرض** بإضافة جماعية (عدة أيام × عدة أوقات) مع منع تداخل المواعيد
- قائمة الحجوزات المدفوعة
- إدارة المستخدمين (قفل وفتح الحسابات)

## 🛠️ التقنيات المستخدمة

| الطبقة | التقنية |
|--------|---------|
| الإطار | ASP.NET Core MVC مع Areas (`Admin` و`Client`) |
| البيانات | Entity Framework Core، SQL Server |
| تسجيل الدخول | ASP.NET Core Identity مع صلاحيات بالأدوار |
| الدفع | Stripe Checkout (`Stripe.net`) |
| الواجهة | Bootstrap 5، تصميم داكن مخصص (CSS وJavaScript) |

## 🚀 طريقة التشغيل

### المتطلبات
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB أو نسخة كاملة)
- حساب على [Stripe](https://stripe.com) (الوضع التجريبي كفاية)

### الإعداد

```bash
git clone https://github.com/<your-username>/Cinema.git
cd Cinema
```

1. **سلسلة الاتصال:** عدّل `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=CinemaDb;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

2. **مفاتيح Stripe:** خزّنها في User Secrets (ماترفعهاش على GitHub أبداً):
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
   ```
   > استخدم نفس اسم المفتاح اللي بيقراه `Program.cs`.

3. **إنشاء قاعدة البيانات:**
   ```bash
   dotnet ef database update
   ```
   (أو `Update-Database` في Package Manager Console)

4. **تشغيل المشروع:**
   ```bash
   dotnet run
   ```

عند أول تشغيل بيتم إنشاء الأدوار وحساب Super Admin افتراضي تلقائياً.

### حساب الأدمن الافتراضي

| البريد | كلمة المرور |
|--------|-------------|
| `superadmin@cinema.com` | `SuperAdmin@123` |

> ⚠️ غيّر كلمة المرور دي (أو كود الـ seeding) قبل نشر الموقع على أي سيرفر عام.

### اختبار الدفع

استخدم كارت Stripe التجريبي: `4242 4242 4242 4242` مع أي تاريخ انتهاء في المستقبل وأي CVC.

## 🎟️ إزاي الحجز بيشتغل

1. الأدمن بيعمل **قاعة** (مثلاً 8 صفوف × 12 كرسي) وبيضيف **مواعيد عرض** للفيلم.
2. العميل بيفتح الفيلم، يختار موعد، ويحدد كراسيه.
3. الكراسي بتتحجز **10 دقايق** في السلة، وباقي المستخدمين بيشوفوها محجوزة.
4. عند الدفع الكراسي بتتحول لتذاكر معلّقة (Pending) وتفضل محجوزة، والعميل بيتحول لـ Stripe.
5. بعد التأكد من الدفع الطلب بيبقى **Paid** والتذاكر بتظهر في **My tickets**.
6. الحجوزات المنتهية والطلبات غير المدفوعة بتتنضف تلقائياً، وفهرس فريد في قاعدة البيانات على `(ShowtimeId, SeatId)` بيمنع الحجز المزدوج.

## 📁 هيكل المشروع

```
Cinema/
├── Areas/
│   ├── Admin/      # الكنترولرز والـ ViewModels والـ Views للوحة التحكم
│   └── Client/     # الموقع العام: الأفلام، الحجز، السلة، الدفع، الحساب
├── Data/           # ApplicationDbContext و migrations
├── Helpers/        # SD (الأدوار)، DbInitializer، SeatLayout، BookingService
├── Models/         # Movie, Actor, Cinemas, Hall, Seat, Showtime, CartItem, Order, Ticket ...
├── ViewModels/
├── Views/Shared/   # الـ Layouts والـ Partials المشتركة
└── wwwroot/        # CSS و JS والصور
```

## 🗺️ الخطوات الجاية

- [ ] إزالة `Movie.CinemaId` (السينما بتتحدد من المواعيد)
- [ ] داش بورد بشكل AdminLTE مع رسوم بيانية للمبيعات
- [ ] Stripe webhook لتأكيد الدفع
- [ ] إرسال التذاكر بالإيميل / QR code
- [ ] صفحة طلبات واسترجاع للأدمن

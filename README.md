# Share Food

Ứng dụng web ASP.NET Core MVC phục vụ môn Back-End với chủ đề chia sẻ công thức nấu ăn. Hệ thống tách khu vực người dùng và quản trị, dùng SQL Server, EF Core Code First, ASP.NET Identity và giao diện tiếng Việt.

## Mục đích

- Cho phép người dùng đăng ký, xác nhận email, đăng nhập và chia sẻ công thức nấu ăn.
- Cho phép quản trị viên quản lý danh mục, nguyên liệu, đơn vị đo, công thức, bình luận và người dùng.
- Đảm bảo đúng tiêu chí chấm điểm bài tập lớn Back-End theo mô hình MVC, Areas, EF Core, CRUD, validation, phân quyền, giao diện responsive và README hướng dẫn chạy.

## Công nghệ sử dụng

- ASP.NET Core MVC (.NET 9)
- Entity Framework Core + SQL Server
- ASP.NET Identity
- Bootstrap 5 + CSS tùy biến
- SMTP qua `System.Net.Mail`

## Chức năng chính

- Người dùng: đăng ký, đăng nhập, đăng xuất, xác nhận email.
- Người dùng: xem danh sách công thức, tìm kiếm, lọc danh mục, phân trang.
- Người dùng: xem chi tiết công thức.
- Người dùng: đăng công thức mới, sửa và xóa công thức của chính mình.
- Người dùng: bình luận công thức.
- Người dùng: lưu công thức yêu thích.
- Người dùng: xem hồ sơ cá nhân và danh sách công thức của tôi.
- Quản trị: dashboard tổng quan.
- Quản trị: CRUD danh mục.
- Quản trị: CRUD nguyên liệu.
- Quản trị: CRUD đơn vị đo.
- Quản trị: quản lý công thức, ẩn/hiện, xóa.
- Quản trị: quản lý bình luận.
- Quản trị: khóa / mở khóa tài khoản người dùng.

## Thiết kế CSDL 3NF

Các bảng chính:

- `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, các bảng liên quan của Identity
- `Categories`
- `Ingredients`
- `Units`
- `Recipes`
- `RecipeSteps`
- `RecipeIngredients`
- `Comments`
- `Favorites`

Điểm chuẩn hóa chính:

- Bước nấu tách thành bảng `RecipeSteps`, không lưu gộp nhiều bước vào một cột.
- Quan hệ nhiều nhiều giữa công thức và nguyên liệu tách thành `RecipeIngredients`.
- Đơn vị đo tách thành `Units`, không lặp chuỗi text tự do ở nhiều nơi.
- Bình luận và yêu thích tách bảng riêng, phụ thuộc đúng khóa của công thức và người dùng.

## Tài khoản mẫu

- Quản trị viên:
- Email: `admin@sharefood.local`
- Mật khẩu: `Admin1234`
- Thành viên:
- Email: `thanhvien@sharefood.local`
- Mật khẩu: `Thanhvien123`

Các tài khoản mẫu và dữ liệu mẫu sẽ được seed tự động khi chạy ứng dụng lần đầu.

## URL chính

- Trang chủ: `/`
- Danh sách công thức: `/cong-thuc`
- Đăng ký: `/tai-khoan/dang-ky`
- Đăng nhập: `/tai-khoan/dang-nhap`
- Hồ sơ: `/ho-so`
- Yêu thích: `/yeu-thich`
- Quản trị: `/quan-tri`
- Danh mục quản trị: `/quan-tri/danh-muc`
- Nguyên liệu quản trị: `/quan-tri/nguyen-lieu`
- Đơn vị đo quản trị: `/quan-tri/don-vi-do`

## Cấu hình cần thiết

File `src/ShareFood.Web/appsettings.json` hiện có cấu hình mẫu:

- `ConnectionStrings:DefaultConnection`
- `EmailSettings:Enabled`
- `EmailSettings:Host`
- `EmailSettings:Port`
- `EmailSettings:UserName`
- `EmailSettings:Password`
- `EmailSettings:FromName`
- `EmailSettings:FromEmail`
- `EmailSettings:EnableSsl`

Ví dụ connection string mặc định:

```json
"DefaultConnection": "Server=localhost;Database=ShareFoodDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

Nếu muốn giữ bí mật thông tin SMTP hoặc chuỗi kết nối, hãy chuyển các giá trị nhạy cảm sang User Secrets hoặc biến môi trường.

## Cấu trúc thư mục

```text
ShareFood.sln
docs/
  sql/
    init.sql
    check-data.sql
src/
  ShareFood.Web/
    Areas/
      Admin/
        Controllers/
        Views/
      Client/
        Controllers/
        Views/
    Controllers/
    Data/
    Helpers/
    Models/
      Entities/
    Services/
    Settings/
    ViewComponents/
    ViewModels/
    Views/
    wwwroot/
      css/
      js/
      images/
      uploads/
```

## Các câu lệnh liên quan

Khôi phục package:

```bash
dotnet restore
```

Build project:

```bash
dotnet build ShareFood.sln
```

Chạy ứng dụng:

```bash
dotnet run --project src/ShareFood.Web/ShareFood.Web.csproj
```

Tạo migration mới:

```bash
dotnet ef migrations add TenMigration --project src/ShareFood.Web/ShareFood.Web.csproj --startup-project src/ShareFood.Web/ShareFood.Web.csproj --output-dir Data/Migrations
```

Cập nhật database:

```bash
dotnet ef database update --project src/ShareFood.Web/ShareFood.Web.csproj --startup-project src/ShareFood.Web/ShareFood.Web.csproj
```

Sinh script SQL:

```bash
dotnet ef migrations script --project src/ShareFood.Web/ShareFood.Web.csproj --startup-project src/ShareFood.Web/ShareFood.Web.csproj --output docs/sql/init.sql
```

## SQL

- File script tạo cấu trúc DB: `docs/sql/init.sql`
- File truy vấn kiểm tra dữ liệu mẫu: `docs/sql/check-data.sql`
- Script tạo cấu trúc DB được sinh từ migration `InitialCreate`
- Dữ liệu mẫu tài khoản và công thức được seed khi ứng dụng khởi động lần đầu

## Ảnh demo và upload

- Ảnh đại diện công thức tải lên sẽ được lưu tại `src/ShareFood.Web/wwwroot/uploads/recipes`
- Trong `.gitignore` đã loại trừ ảnh upload để tránh đưa file rác vào repo

## Lưu ý bảo mật

- Không hard-code connection string trong controller hoặc service.
- Mật khẩu được hash bằng ASP.NET Identity, không lưu plain-text.
- User chỉ sửa/xóa được công thức của chính mình.
- Route quản trị được chặn bằng phân quyền role `Admin`.

## Kiểm tra nhanh trước khi nộp

- `dotnet build ShareFood.sln` phải thành công.
- `dotnet ef database update ...` phải chạy được.
- Đăng ký mới phải gửi hoặc log ra email xác nhận.
- CRUD danh mục, nguyên liệu, đơn vị đo và công thức phải hoạt động.
- Trang quản trị chỉ truy cập được bằng tài khoản admin.

# Share Food

Ứng dụng web `ASP.NET Core MVC` cho môn Back-End với chủ đề chia sẻ công thức nấu ăn. Hệ thống dùng `SQL Server`, `EF Core Code First`, `ASP.NET Identity`, chia rõ khu người dùng và khu quản trị, toàn bộ giao diện hiển thị bằng tiếng Việt.

## Mục đích

- Cho phép người dùng đăng ký, xác nhận email bằng `OTP`, đăng nhập và chia sẻ công thức nấu ăn.
- Cho phép cộng đồng bình luận, lưu yêu thích và nhận thông báo khi công thức của mình được người khác yêu thích.
- Cho phép quản trị viên quản lý danh mục, nguyên liệu, đơn vị đo, công thức, bình luận và người dùng.
- Đáp ứng tiêu chí bài tập lớn Back-End theo hướng `MVC + Areas + EF Core + SQL Server + Code First + Validation + Phân quyền`.

## Công nghệ sử dụng

- `.NET 9` + `ASP.NET Core MVC`
- `Entity Framework Core` + `SQL Server`
- `ASP.NET Identity`
- `Bootstrap 5` + `CSS tùy biến`
- `SMTP` qua `System.Net.Mail`

## Chức năng chính

### Người dùng

- Đăng ký tài khoản.
- Nhận `OTP` qua email để xác nhận tài khoản.
- Gửi lại OTP nếu mã hết hạn hoặc chưa nhận được.
- Đăng nhập, đăng xuất.
- Nhận email chào mừng sau khi xác nhận thành công.
- Nhận email gợi ý công thức mới nếu bật tùy chọn nhận bản tin.
- Xem danh sách công thức, tìm kiếm, lọc theo danh mục, phân trang.
- Xem chi tiết công thức.
- Đăng công thức mới.
- Sửa, xóa công thức của chính mình.
- Bình luận công thức.
- Lưu công thức yêu thích.
- Nhận thông báo khi có người khác thích công thức của mình.
- Xem hồ sơ cá nhân, đổi tên, cập nhật số điện thoại, đổi mật khẩu, bật/tắt nhận email cập nhật.

### Quản trị

- Dashboard tổng quan.
- CRUD danh mục.
- CRUD nguyên liệu.
- CRUD đơn vị đo.
- Quản lý công thức.
- Quản lý bình luận.
- Quản lý người dùng.
- Thêm người dùng mới trực tiếp trong khu quản trị.
- Khóa hoặc mở khóa tài khoản người dùng.

## CSDL 3NF

Các bảng chính:

- `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, các bảng Identity liên quan
- `Categories`
- `Ingredients`
- `Units`
- `Recipes`
- `RecipeSteps`
- `RecipeIngredients`
- `Comments`
- `Favorites`
- `Notifications`
- `EmailOtps`

Điểm chuẩn hóa:

- Các bước nấu được tách riêng ở `RecipeSteps`.
- Quan hệ nhiều - nhiều giữa công thức và nguyên liệu được tách thành `RecipeIngredients`.
- Đơn vị đo dùng bảng `Units`, không lặp text tự do ở nhiều nơi.
- Tương tác người dùng được tách riêng thành `Comments`, `Favorites`, `Notifications`.
- OTP email được tách riêng thành `EmailOtps`, không nhét mã xác nhận vào bảng người dùng.

## Tài khoản mẫu

- Quản trị viên
  - Email: `admin@sharefood.local`
  - Mật khẩu: `Admin1234`
- Thành viên
  - Email: `thanhvien@sharefood.local`
  - Mật khẩu: `Thanhvien123`

Ngoài ra hệ thống tự seed thêm nhiều tài khoản và công thức mẫu để demo.

## Dữ liệu mẫu đã có

- `10` công thức mẫu
- `7` người dùng
- `7` bình luận mẫu
- `8` lượt yêu thích mẫu

Ảnh món ăn mẫu được lưu cục bộ trong:

- `src/ShareFood.Web/wwwroot/uploads/seed`

Phong cách ảnh và bố cục lấy cảm hứng từ trang:

- `https://cookpad.com/vn`

Một phần ảnh mẫu được lấy từ nguồn mở như Wikimedia Commons và Foodiesfeed rồi lưu cục bộ để demo ổn định.

## URL chính

- Trang chủ: `/`
- Danh sách công thức: `/cong-thuc`
- Đăng công thức: `/cong-thuc/dang-moi`
- Đăng ký: `/tai-khoan/dang-ky`
- Xác nhận OTP: `/tai-khoan/xac-nhan-otp`
- Đăng nhập: `/tai-khoan/dang-nhap`
- Hồ sơ: `/ho-so`
- Yêu thích: `/yeu-thich`
- Thông báo: `/thong-bao`
- Quản trị: `/quan-tri`
- Quản lý người dùng: `/quan-tri/nguoi-dung`

## Cấu hình cần thiết

File:

- `src/ShareFood.Web/appsettings.json`

Các khóa quan trọng:

- `ConnectionStrings:DefaultConnection`
- `EmailSettings:Enabled`
- `EmailSettings:Host`
- `EmailSettings:Port`
- `EmailSettings:UserName`
- `EmailSettings:Password`
- `EmailSettings:FromName`
- `EmailSettings:FromEmail`
- `EmailSettings:EnableSsl`

Ví dụ:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ShareFoodDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "EmailSettings": {
    "Enabled": true,
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UserName": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromName": "Share Food",
    "FromEmail": "your-email@gmail.com",
    "EnableSsl": true
  }
}
```

Nếu `EmailSettings.Enabled = false`, hệ thống sẽ không gửi mail thật mà ghi nội dung email ra log để kiểm thử luồng OTP và email chào mừng.

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
    Data/
      Migrations/
    Helpers/
    Models/
      Entities/
    Services/
    Settings/
    ViewComponents/
    ViewModels/
    Views/
      Shared/
    wwwroot/
      css/
      js/
      images/
      uploads/
        recipes/
        seed/
```

## Các lệnh liên quan

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

- File tạo cấu trúc database: `docs/sql/init.sql`
- File truy vấn kiểm tra dữ liệu: `docs/sql/check-data.sql`
- Migration hiện có:
  - `InitialCreate`
  - `AddNotifications`
  - `AddEmailOtpAndNewsletter`

## Luồng email OTP

1. Người dùng đăng ký tài khoản.
2. Hệ thống tạo tài khoản ở trạng thái `EmailConfirmed = false`.
3. Hệ thống sinh mã OTP 6 chữ số, lưu vào bảng `EmailOtps`.
4. OTP được gửi về email người dùng.
5. Người dùng nhập OTP tại `/tai-khoan/xac-nhan-otp`.
6. Nếu OTP hợp lệ và còn hạn, hệ thống xác nhận email.
7. Sau đó hệ thống gửi email chào mừng.
8. Nếu người dùng bật nhận bản tin, hệ thống gửi thêm email gợi ý công thức mới.

## Lưu ý bảo mật

- Không hard-code thông tin SMTP trong controller hoặc service.
- Mật khẩu được băm bằng `ASP.NET Identity`, không lưu plain-text.
- OTP có thời hạn và bị đánh dấu đã dùng sau khi xác nhận thành công.
- User chỉ sửa hoặc xóa được công thức của chính mình.
- Khu quản trị được chặn bằng role `Admin`.

## Kiểm tra nhanh trước khi nộp

- `dotnet build ShareFood.sln` phải thành công.
- `dotnet ef database update ...` phải chạy được.
- Đăng ký mới phải sinh OTP.
- Xác nhận OTP xong thì `EmailConfirmed = true`.
- Bình luận khi đã đăng nhập phải hoạt động.
- Hồ sơ phải đổi tên, số điện thoại, mật khẩu được.
- Thông báo yêu thích phải xuất hiện ở `/thong-bao`.
- Admin phải thêm được người dùng mới ở `/quan-tri/nguoi-dung/them-moi`.

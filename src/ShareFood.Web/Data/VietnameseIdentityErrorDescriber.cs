using Microsoft.AspNetCore.Identity;

namespace ShareFood.Web.Data;

public class VietnameseIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() => Create("Có lỗi xảy ra, vui lòng thử lại.");
    public override IdentityError ConcurrencyFailure() => Create("Dữ liệu vừa được thay đổi. Vui lòng tải lại trang.");
    public override IdentityError PasswordMismatch() => Create("Mật khẩu không chính xác.");
    public override IdentityError InvalidToken() => Create("Mã xác thực không hợp lệ.");
    public override IdentityError LoginAlreadyAssociated() => Create("Tài khoản đăng nhập này đã được liên kết.");
    public override IdentityError InvalidUserName(string? userName) => Create($"Tên đăng nhập '{userName}' không hợp lệ.");
    public override IdentityError InvalidEmail(string? email) => Create($"Email '{email}' không hợp lệ.");
    public override IdentityError DuplicateUserName(string userName) => Create($"Tên đăng nhập '{userName}' đã tồn tại.");
    public override IdentityError DuplicateEmail(string email) => Create($"Email '{email}' đã được sử dụng.");
    public override IdentityError InvalidRoleName(string? role) => Create($"Vai trò '{role}' không hợp lệ.");
    public override IdentityError DuplicateRoleName(string role) => Create($"Vai trò '{role}' đã tồn tại.");
    public override IdentityError UserAlreadyHasPassword() => Create("Tài khoản này đã có mật khẩu.");
    public override IdentityError UserLockoutNotEnabled() => Create("Tài khoản chưa bật chế độ khóa.");
    public override IdentityError UserAlreadyInRole(string role) => Create($"Người dùng đã thuộc vai trò '{role}'.");
    public override IdentityError UserNotInRole(string role) => Create($"Người dùng không thuộc vai trò '{role}'.");
    public override IdentityError PasswordTooShort(int length) => Create($"Mật khẩu phải có ít nhất {length} ký tự.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => Create("Mật khẩu phải có ít nhất một ký tự đặc biệt.");
    public override IdentityError PasswordRequiresDigit() => Create("Mật khẩu phải có ít nhất một chữ số.");
    public override IdentityError PasswordRequiresLower() => Create("Mật khẩu phải có ít nhất một chữ thường.");
    public override IdentityError PasswordRequiresUpper() => Create("Mật khẩu phải có ít nhất một chữ in hoa.");

    private static IdentityError Create(string description)
    {
        return new IdentityError { Description = description };
    }
}

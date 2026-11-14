# Keycloak tùy chọn

Đồ án mặc định dùng đăng nhập cục bộ. Không cần chạy Keycloak để trình bày luồng chính.

Tệp realm chỉ khai báo ba vai trò và một public client, không chứa tài khoản hoặc mật khẩu. Khi thử chế độ Keycloak, cần Java 21, tự tạo tài khoản và gán vai trò trong trang quản trị, rồi cấu hình Auth.Mode, Authority và PublicUrl ở appsettings.Local.json. Script start-keycloak.ps1 hỏi mật khẩu quản trị khi chạy. Luồng đăng nhập giao diện trong bản bàn giao dùng chế độ Local; tích hợp SSO cần được kiểm tra riêng.

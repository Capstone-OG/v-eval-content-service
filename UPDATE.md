# Nhật Ký Cập Nhật (Update Log) - Content Service

## [14/09/2026] - Chuẩn Hóa Cấu Hình Production & Git Security
- **Khởi Tạo `appsettings.example.json`**:
  - Tạo file cấu hình mẫu chứa `ConnectionStrings` (PostgreSQL Supabase Connection) và `JwtSettings` (SecretKey, Issuer, Audience).
  - Sử dụng các label tham số mẫu (`YOUR_POSTGRES_HOST`, `YOUR_POSTGRES_USER`, `YOUR_POSTGRES_PASSWORD`).
- **Bảo Mật Git**:
  - Cập nhật `.gitignore` ẩn các file `appsettings.json` chứa Password thật.
  - Định tuyến thông qua Gateway tại `/api/content/{**catch-all}` (Cổng `:5249`).

## [05/09/2026] - Hỗ Trợ Lưu Trữ Hình Ảnh Câu Hỏi Gốc (Question Image Persistence)
- **Bổ sung Thuộc tính ImageUrl cho QuestionDto**:
  - Thêm `ImageUrl` vào `QuestionDto` trong `ImportMockExamCommand.cs`.
- **Tự động Nhúng Hình Minh Họa vào ContentLatex**:
  - Cập nhật `ImportMockExamCommandHandler.cs`: khi câu hỏi đơn lẻ hoặc câu hỏi thành viên trong chùm bài đọc có chứa `ImageUrl`, tự động nhúng cú pháp `![Hình minh họa](image_url)` vào nội dung `ContentLatex`.
  - Đảm bảo hiển thị hình ảnh đồ thị, sơ đồ thí nghiệm trên mọi trình duyệt KaTeX / Markdown mà không yêu cầu thay đổi cấu trúc bảng cơ sở dữ liệu PostgreSQL Supabase.

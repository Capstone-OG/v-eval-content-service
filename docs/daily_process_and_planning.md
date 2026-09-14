# Nhật ký & Kế hoạch Phát triển Content Service (Daily Process and Planning)

Tài liệu này dùng để theo dõi tiến độ phát triển thực tế hàng ngày, các cột mốc tính năng (Milestones) và kế hoạch tương lai của phân hệ **V-Eval-Content_Service**.

---

## 📅 Cập nhật ngày 14/09/2026

### 🎯 Mục tiêu hiện tại (Milestone 4 - Production Config & Security)
Khởi tạo tệp cấu hình mẫu Production (`appsettings.example.json`), ẩn các file cấu hình bí mật local chứa Connection String PostgreSQL Supabase thật qua `.gitignore`, và đồng bộ định tuyến API Gateway.

### 📋 Danh sách Task & Trạng thái

| Tên Task | Trạng thái | Ghi chú |
| :--- | :---: | :--- |
| **Khởi Tạo `appsettings.example.json`** | 🟢 Hoàn thành | Tạo file mẫu chứa đầy đủ `ConnectionStrings` (Supabase Connection) và `JwtSettings` với label mẫu. |
| **Bảo Mật Git Security** | 🟢 Hoàn thành | Cập nhật `.gitignore` ẩn tất cả file `appsettings.json` chứa thông tin nhạy cảm. |
| **Định Tuyến Gateway YARP** | 🟢 Hoàn thành | Định tuyến `/api/content/{**catch-all}` tại Gateway V-Eval trỏ về cổng `:5249`. |

---

## 📅 Cập nhật ngày 06/09/2026 - 07/09/2026

### 🎯 Mục tiêu hiện tại (Milestone 3.2)
Tối ưu hóa tiếp nhận & chuẩn hóa dữ liệu công thức Toán - Lý - Hóa, đảm bảo tương thích tuyệt đối với cấu trúc Bảng số liệu lồng 2 tầng (`colspan`/`rowspan`), đồng bộ phân tuyến hình ảnh minh họa và kiểm chứng toàn vẹn chu trình lưu - tải đề thi 2 chiều với Supabase PostgreSQL.

### 📋 Danh sách Task & Trạng thái

| Tên Task | Trạng thái | Ghi chú |
| :--- | :---: | :--- |
| **Bảo toàn Chuỗi Thoát Công thức LaTeX / Hóa - Lý** | 🟢 Hoàn thành | Đồng bộ với bộ làm sạch `SanitizeJsonForLatex` của AI Engine. |
| **Hỗ trợ Lưu trữ & Bảo toàn Bảng Số liệu HTML 2 Tầng** | 🟢 Hoàn thành | Trường `content` của `Passages` và `Questions` tiếp nhận trọn vẹn cấu trúc bảng HTML `<table>` có `rowspan="2"` và `colspan="n"`. |
| **Đồng bộ Phân Tuyến Hình ảnh Minh Họa** | 🟢 Hoàn thành | Chỉ nhúng cú pháp `![Hình minh họa](image_url)` cho các câu hỏi/chùm bài thực sự có ảnh cắt tĩnh. |
| **Kiểm Thử Toàn Diện Liên Thông 2 Chiều với Supabase** | 🟢 Hoàn thành | Xác nhận chu trình khép kín 120 câu hỏi với Supabase PostgreSQL. |

---

## 📅 Cập nhật các ngày trước (31/08/2026 - 05/09/2026)
*(Xem chi tiết trong tài liệu `docs/content_service_architecture.md`)*

# Nhật ký & Kế hoạch Phát triển Content Service (Daily Process and Planning)

Tài liệu này dùng để theo dõi tiến độ phát triển thực tế hàng ngày, các cột mốc tính năng (Milestones) và kế hoạch tương lai của phân hệ **V-Eval-Content_Service**.

---

## 📅 Cập nhật ngày 15/09/2026

### 🎯 Mục tiêu hiện tại (Milestone 5 - Docker Containerization)
Tạo Dockerfile Multi-stage cho Content Service (Cổng `:5249`) và kết nối vào `docker-compose.yml`.

### 📋 Danh sách Task & Trạng thái

| Tên Task | Trạng thái | Ghi chú |
| :--- | :---: | :--- |
| **Tạo `Dockerfile` Multi-stage** | 🟢 Hoàn thành | Build và publish .NET 9 API image trên cổng `5249`. |
| **Tích hợp Docker Compose** | 🟢 Hoàn thành | Khai báo container `v_eval_content_service` tham gia mạng `veval_network`. |

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
| **Bảo toàn Chuỗi Thoát Công thức LaTeX / Hóa - Lý** | 🟢 Hoàn thành | Đồng bộ với bộ làm sạch `SanitizeJsonForLatex` của AI Engine: Đảm bảo chuỗi JSON gửi tới `POST /api/content/exams/import` chứa các ký tự thoát LaTeX đặc biệt (`\\frac`, `\\times`, `\\Delta`, `\\text`) và số mũ âm không bị nuốt ký tự, lưu trữ nguyên trạng vào cột `content_latex`. |
| **Hỗ trợ Lưu trữ & Bảo toàn Bảng Số liệu HTML 2 Tầng** | 🟢 Hoàn thành | Trường `content` của `Passages` và `Questions` tiếp nhận trọn vẹn cấu trúc bảng HTML `<table>` có `rowspan="2"` và `colspan="n"` (như chùm câu 109–111), đảm bảo khi tải lại đề từ CSDL hiển thị đúng chuẩn lưới hình học. |
| **Đồng bộ Phân Tuyến Hình ảnh Minh Họa** | 🟢 Hoàn thành | Tương thích với bộ lọc `isChartOrTable` của AI Engine: Chỉ nhúng cú pháp `![Hình minh họa](image_url)` cho các câu hỏi/chùm bài thực sự có ảnh cắt tĩnh (đồ thị dao động điều hòa $a-x$, sơ đồ thí nghiệm). |
| **Kiểm Thử Toàn Diện Liên Thông 2 Chiều với Supabase** | 🟢 Hoàn thành | Xác nhận chu trình khép kín: AI Engine phân tích PDF 16 trang -> Web Viewer kiểm tra -> Nhấn "Lưu vào Database" (gọi Content Service) -> Dữ liệu ghi thành công vào PostgreSQL -> Tải lại từ CSDL qua `GET /api/content/exams/{id}` hiển thị sắc nét 100% cả 120 câu hỏi. |

---

## 📅 Cập nhật ngày 05/09/2026

### 🎯 Mục tiêu hiện tại (Milestone 3.1)
Mở rộng cấu trúc tiếp nhận dữ liệu từ AI Engine để lưu trữ hình ảnh câu hỏi đơn lẻ bên cạnh hình ảnh chùm bài đọc hiểu đã có.

### 📋 Danh sách Task & Trạng thái

| Tên Task | Trạng thái | Ghi chú |
| :--- | :---: | :--- |
| **Bổ sung ImageUrl vào QuestionDto** | 🟢 Hoàn thành | Mở rộng DTO `ImportMockExamCommand.cs` với trường `ImageUrl` cho từng câu hỏi. |
| **Tự động Định dạng Hình ảnh vào ContentLatex** | 🟢 Hoàn thành | Xử lý trong `ImportMockExamCommandHandler`: chèn cú pháp Markdown Image `![Hình minh họa](image_url)` vào `ContentLatex` của câu hỏi khi lưu trữ vào PostgreSQL. |

---

## 📅 Cập nhật ngày 04/09/2026

### 🎯 Mục tiêu hiện tại (Milestone 3)
Mở rộng các API Quản lý Đề thi (Listing, Details, Cascade Deletion), kích hoạt CORS phục vụ trực tiếp cho AI Engine Web Viewer.

### 📋 Danh sách Task & Trạng thái

| Tên Task | Trạng thái | Ghi chú |
| :--- | :---: | :--- |
| **API Lấy Danh sách Đề thi (`GET /api/content/exams`)** | 🟢 Hoàn thành | Triển khai `GetMockExamsQuery` với MediatR, trả về danh sách tóm tắt (ID, tiêu đề, thời lượng, tổng số câu, ngày tạo). |
| **API Lấy Chi tiết Đề thi (`GET /api/content/exams/{id}`)** | 🟢 Hoàn thành | Triển khai `GetMockExamByIdQuery`, nạp toàn bộ cấu trúc phân cấp: `MockExam` -> `ExamQuestions` -> `Questions` -> `Passages` + `Skills`. |
| **API Xóa Đề thi Cascade (`DELETE /api/content/exams/{id}`)** | 🟢 Hoàn thành | Triển khai `DeleteMockExamCommand`, xóa sạch liên kết trong `exam_questions`, các câu hỏi `questions` và chùm bài `passages` liên quan. |
| **Cấu hình CORS** | 🟢 Hoàn thành | Mở quyền truy cập cho cổng `http://localhost:5104` (AI Engine Web Viewer) và các Web/Mobile Clients. |

---

## 📅 Cập nhật ngày 31/08/2026 - 03/09/2026

### 📋 Danh sách Task & Trạng thái

| Tên Task | Trạng thái | Ghi chú |
| :--- | :---: | :--- |
| **Khởi tạo 4 Layer Clean Architecture** | 🟢 Hoàn thành | Tách bạch `Domain`, `Application`, `Infrastructure`, `API`. |
| **Xây dựng Domain Entities & EF Core Npgsql** | 🟢 Hoàn thành | Tạo `ContentDbContext`, ánh xạ Fluent API vào schema `content` trên Supabase. |
| **Thiết kế Command CQRS MediatR** | 🟢 Hoàn thành | Triển khai `CreateMockExamCommand` và `CreateMockExamCommandHandler` tự động sinh `Skill`. |

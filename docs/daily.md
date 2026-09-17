# NHẬT KÝ KIỂM TRẢ TIẾN ĐỘ VẬN HÀNH (DAILY CHECK LOG) - CONTENT SERVICE

## [18/09/2026] - Phát Hành Công Cụ Push Độc Lập `Scripts/push.bat` & Chuẩn Hóa Bộ Docs
- **Khởi Tạo `Scripts/push.bat`**: Đóng gói công cụ push độc lập hỗ trợ 3 chế độ (nhánh hiện tại, danh sách số nhánh có sẵn, tạo nhánh mới).
- **Chuẩn Hóa Bộ Docs Service**: Đồng bộ hệ thống tài liệu theo 3 file chuẩn `daily.md`, `process.md` và `architecture_acceptance.md`.

---

## [15/09/2026] - Dockerize Content Service (Milestone 5)
- **Tạo `Dockerfile` Multi-stage**: Build và publish .NET 9 API image trên cổng `5249`.
- **Tích hợp Docker Compose**: Khai báo container `v_eval_content_service` tham gia mạng `veval_network`.

---

## [14/09/2026] - Production Config & Security (Milestone 4)
- **Khởi Tạo `appsettings.example.json`**: Tạo file mẫu chứa đầy đủ `ConnectionStrings` (Supabase Connection) và `JwtSettings` với label mẫu.
- **Bảo Mật Git Security**: Cập nhật `.gitignore` ẩn tất cả file `appsettings.json` chứa thông tin nhạy cảm.
- **Định Tuyến Gateway YARP**: Định tuyến `/api/content/{**catch-all}` tại Gateway V-Eval trỏ về cổng `:5249`.

---

## [06/09/2026 - 07/09/2026] - Tối Ưu Tiếp Nhận Công Thức & Bảng Số Liệu 2 Tầng (Milestone 3.2)
- **Bảo toàn Chuỗi Thoát Công thức LaTeX / Hóa - Lý**: Đồng bộ với bộ làm sạch `SanitizeJsonForLatex` của AI Engine: Đảm bảo chuỗi JSON gửi tới `POST /api/content/exams/import` chứa các ký tự thoát LaTeX đặc biệt (`\\frac`, `\\times`, `\\Delta`, `\\text`) và số mũ âm không bị nuốt ký tự, lưu trữ nguyên trạng vào cột `content_latex`.
- **Hỗ trợ Lưu trữ & Bảo toàn Bảng Số liệu HTML 2 Tầng**: Trường `content` của `Passages` và `Questions` tiếp nhận trọn vẹn cấu trúc bảng HTML `<table>` có `rowspan="2"` và `colspan="n"` (như chùm câu 109–111), đảm bảo khi tải lại đề từ CSDL hiển thị đúng chuẩn lưới hình học.
- **Đồng bộ Phân Tuyến Hình ảnh Minh Họa**: Tương thích với bộ lọc `isChartOrTable` của AI Engine: Chỉ nhúng cú pháp `![Hình minh họa](image_url)` cho các câu hỏi/chùm bài thực sự có ảnh cắt tĩnh.
- **Kiểm Thử Toàn Diện Liên Thông 2 Chiều với Supabase**: Xác nhận chu trình khép kín: AI Engine phân tích PDF 16 trang -> Web Viewer kiểm tra -> Nhấn "Lưu vào Database" (gọi Content Service) -> Dữ liệu ghi thành công vào PostgreSQL -> Tải lại từ CSDL qua `GET /api/content/exams/{id}` hiển thị sắc nét 100% cả 120 câu hỏi.

---

## [05/09/2026] - Mở Rộng Tiếp Nhận Hình Ảnh Câu Hỏi (Milestone 3.1)
- **Bổ sung ImageUrl vào QuestionDto**: Mở rộng DTO `ImportMockExamCommand.cs` với trường `ImageUrl` cho từng câu hỏi.
- **Tự động Định dạng Hình ảnh vào ContentLatex**: Xử lý trong `ImportMockExamCommandHandler`: chèn cú pháp Markdown Image `![Hình minh họa](image_url)` vào `ContentLatex` của câu hỏi khi lưu trữ vào PostgreSQL.

---

## [04/09/2026] - API Quản Lý Đề Thi & Tích Hợp CORS (Milestone 3)
- **API Lấy Danh sách Đề thi (`GET /api/content/exams`)**: Triển khai `GetMockExamsQuery` với MediatR, trả về danh sách tóm tắt (ID, tiêu đề, thời lượng, tổng số câu, ngày tạo).
- **API Lấy Chi tiết Đề thi (`GET /api/content/exams/{id}`)**: Triển khai `GetMockExamByIdQuery`, nạp toàn bộ cấu trúc phân cấp: `MockExam` -> `ExamQuestions` -> `Questions` -> `Passages` + `Skills`.
- **API Xóa Đề thi Cascade (`DELETE /api/content/exams/{id}`)**: Triển khai `DeleteMockExamCommand`, xóa sạch liên kết trong `exam_questions`, các câu hỏi `questions` và chùm bài `passages` liên quan.
- **Cấu hình CORS**: Mở quyền truy cập cho cổng `http://localhost:5104` (AI Engine Web Viewer) và các Web/Mobile Clients.

---

## [31/08/2026 - 03/09/2026] - Khởi Tạo Clean Architecture & Npgsql EF Core
- **Khởi tạo 4 Layer Clean Architecture**: Tách bạch `Domain`, `Application`, `Infrastructure`, `API`.
- **Xây dựng Domain Entities & EF Core Npgsql**: Tạo `ContentDbContext`, ánh xạ Fluent API vào schema `content` trên Supabase.
- **Thiết kế Command CQRS MediatR**: Triển khai `CreateMockExamCommand` và `CreateMockExamCommandHandler` tự động sinh `Skill`.

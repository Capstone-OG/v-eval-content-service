# Nhật ký & Kế hoạch Phát triển Content Service (Daily Process and Planning)

Tài liệu này dùng để theo dõi tiến độ phát triển thực tế hàng ngày, các cột mốc tính năng (Milestones) và kế hoạch tương lai của phân hệ **V-Eval-Content_Service**.

---

## 📅 Cập nhật ngày 31/08/2026 - 01/09/2026

### 🎯 Mục tiêu hiện tại (Milestone 1)
Khởi tạo cấu trúc dự án **.NET 9 Web API** theo chuẩn **Clean Architecture**, thiết kế mô hình thực thể (Entities) và kết nối Cơ sở dữ liệu Supabase PostgreSQL schema `content`.

### 📋 Danh sách Task & Trạng thái

| Tên Task | Trạng thái | Ghi chú |
| :--- | :---: | :--- |
| **Khởi tạo 4 Layer Clean Architecture** | 🟢 Hoàn thành | Tách bạch `Domain`, `Application`, `Infrastructure`, `API`. Cấu hình dependency injection chuẩn mực. |
| **Xây dựng Domain Entities** | 🟢 Hoàn thành | Định nghĩa `MockExam`, `Question`, `Passage`, `Skill`, `CompetencyDomain`, `ExamQuestion`, `Material`. |
| **Cấu hình EF Core Npgsql** | 🟢 Hoàn thành | Tạo `ContentDbContext`, ánh xạ Fluent API vào schema `content` trên Supabase. |
| **Cấu hình bảo mật Secret Leak-proof** | 🟢 Hoàn thành | Chặn `appsettings.Development.json` qua `.gitignore`, sử dụng placeholder trong `appsettings.json`. |

---

## 📅 Cập nhật ngày 02/09/2026 - 03/09/2026

### 🎯 Mục tiêu hiện tại (Milestone 2)
Xây dựng luồng nhập đề thi tự động từ AI Engine (`POST /api/content/exams/import`), tự động nhận diện và phân loại kỹ năng (Dynamic Skill Auto-generation).

### 📋 Danh sách Task & Trạng thái

| Tên Task | Trạng thái | Ghi chú |
| :--- | :---: | :--- |
| **Thiết kế Command CQRS MediatR** | 🟢 Hoàn thành | Triển khai `CreateMockExamCommand` và `CreateMockExamCommandHandler`. |
| **Cơ chế Auto-Generate Skill** | 🟢 Hoàn thành | Nếu `suggestedSkillName` chưa tồn tại trong Domain tương ứng, tự động thêm mới vào bảng `skills` để tái sử dụng. |
| **Lưu trữ Chùm bài đọc hiểu (Passages)** | 🟢 Hoàn thành | Bóc tách liên kết giữa `Passage` và các câu hỏi thành viên (`startQuestion` đến `endQuestion`). |
| **Lưu trữ Câu hỏi Đơn (Single Questions)** | 🟢 Hoàn thành | Lưu các phương án A, B, C, D, đáp án đúng, giải thích, và công thức LaTeX vào bảng `questions`. |
| **Liên kết Đề thi & Thứ tự câu hỏi** | 🟢 Hoàn thành | Ghi nhận quan hệ n-n vào bảng `exam_questions` kèm số thứ tự hiển thị `order_index`. |

---

## 📅 Cập nhật ngày 04/09/2026

### 🎯 Mục tiêu hiện tại (Milestone 3)
Mở rộng các API Quản lý Đề thi (Listing, Details, Cascade Deletion), kích hoạt CORS phục vụ trực tiếp cho AI Engine Web Viewer, và kiểm chứng liên thông dữ liệu toàn diện.

### 📋 Danh sách Task & Trạng thái

| Tên Task | Trạng thái | Ghi chú |
| :--- | :---: | :--- |
| **API Lấy Danh sách Đề thi (`GET /api/content/exams`)** | 🟢 Hoàn thành | Triển khai `GetMockExamsQuery` với MediatR, trả về danh sách tóm tắt (ID, tiêu đề, thời lượng, tổng số câu, ngày tạo). |
| **API Lấy Chi tiết Đề thi (`GET /api/content/exams/{id}`)** | 🟢 Hoàn thành | Triển khai `GetMockExamByIdQuery`, nạp toàn bộ cấu trúc phân cấp: `MockExam` -> `ExamQuestions` -> `Questions` -> `Passages` + `Skills`. |
| **API Xóa Đề thi Cascade (`DELETE /api/content/exams/{id}`)** | 🟢 Hoàn thành | Triển khai `DeleteMockExamCommand`, xóa sạch liên kết trong `exam_questions`, các câu hỏi `questions` và chùm bài `passages` liên quan mà không để lại rác dữ liệu. |
| **Cấu hình CORS Cross-Origin Resource Sharing** | 🟢 Hoàn thành | Mở quyền truy cập cho cổng `http://localhost:5104` (AI Engine Web Viewer) và các Web/Mobile Clients. |
| **Kiểm thử Liên thông Toàn vẹn (End-to-End Test)** | 🟢 Hoàn thành | Kiểm thử luồng: Parse đề 120 câu từ PDF bằng AI -> Xem trên Web Viewer -> Nhấn "Lưu vào Database" -> Lưu thành công vào Supabase -> Đọc chi tiết bằng `GET` API. |
| **Khởi tạo Hệ thống Tài liệu Kỹ thuật (`docs/`)** | 🟢 Hoàn thành | Soạn thảo `docs/daily_process_and_planning.md`, `docs/content_service_architecture.md`, `docs/exam_management_api.md`. |
 
---

## 📅 Cập nhật ngày 05/09/2026

### 🎯 Mục tiêu hiện tại (Milestone 3.1)
Mở rộng cấu trúc tiếp nhận dữ liệu từ AI Engine để lưu trữ hình ảnh câu hỏi đơn lẻ (như đồ thị $a-x$, ảnh gương cầu lồi) bên cạnh hình ảnh chùm bài đọc hiểu đã có, đảm bảo hiển thị đồng bộ trên hệ thống thi trực tuyến.

### 📋 Danh sách Task & Trạng thái

| Tên Task | Trạng thái | Ghi chú |
| :--- | :---: | :--- |
| **Bổ sung ImageUrl vào QuestionDto** | 🟢 Hoàn thành | Mở rộng DTO `ImportMockExamCommand.cs` với trường `ImageUrl` cho từng câu hỏi. |
| **Tự động Định dạng Hình ảnh vào ContentLatex** | 🟢 Hoàn thành | Xử lý trong `ImportMockExamCommandHandler`: chèn cú pháp Markdown Image `![Hình minh họa](image_url)` vào `ContentLatex` của câu hỏi khi lưu trữ vào PostgreSQL. |

---

## 📅 Cập nhật ngày 06/09/2026 - 07/09/2026

### 🎯 Mục tiêu hiện tại (Milestone 3.2)
Tối ưu hóa tiếp nhận & chuẩn hóa dữ liệu công thức Toán - Lý - Hóa, đảm bảo tương thích tuyệt đối với cấu trúc Bảng số liệu lồng 2 tầng (`colspan`/`rowspan`), đồng bộ phân tuyến hình ảnh minh họa và kiểm chứng toàn vẹn chu trình lưu - tải đề thi 2 chiều với Supabase PostgreSQL.

### 📋 Danh sách Task & Trạng thái

| Tên Task | Trạng thái | Ghi chú |
| :--- | :---: | :--- |
| **Bảo toàn Chuỗi Thoát Công thức LaTeX / Hóa - Lý** | 🟢 Hoàn thành | Đồng bộ với bộ làm sạch `SanitizeJsonForLatex` của AI Engine: Đảm bảo chuỗi JSON gửi tới `POST /api/content/exams/import` chứa các ký tự thoát LaTeX đặc biệt (`\\frac`, `\\times`, `\\Delta`, `\\text`) và số mũ âm ($2,33 \cdot 10^{-3}\text{ mol}\cdot\text{l}^{-1}\cdot\text{phút}^{-1}$) không bị nuốt ký tự, lưu trữ nguyên trạng vào cột `content_latex`. |
| **Hỗ trợ Lưu trữ & Bảo toàn Bảng Số liệu HTML 2 Tầng** | 🟢 Hoàn thành | Trường `content` của `Passages` và `Questions` tiếp nhận trọn vẹn cấu trúc bảng HTML `<table>` có `rowspan="2"` và `colspan="n"` (như chùm câu 109–111), đảm bảo khi tải lại đề từ CSDL hiển thị đúng chuẩn lưới hình học. |
| **Đồng bộ Phân Tuyến Hình ảnh Minh Họa** | 🟢 Hoàn thành | Tương thích với bộ lọc `isChartOrTable` của AI Engine: Chỉ nhúng cú pháp `![Hình minh họa](image_url)` cho các câu hỏi/chùm bài thực sự có ảnh cắt tĩnh (đồ thị dao động điều hòa $a-x$, sơ đồ thí nghiệm). Các câu hỏi dạng biểu đồ hoặc bảng số liệu giữ nguyên văn bản để Frontend tự dựng Canvas Chart.js và HTML Table tương tác. |
| **Kiểm Thử Toàn Diện Liên Thông 2 Chiều với Supabase** | 🟢 Hoàn thành | Xác nhận chu trình khép kín: AI Engine phân tích PDF 16 trang -> Web Viewer kiểm tra -> Nhấn "Lưu vào Database" (gọi Content Service) -> Dữ liệu ghi thành công vào PostgreSQL -> Tải lại từ CSDL qua `GET /api/content/exams/{id}` hiển thị sắc nét 100% cả 120 câu hỏi. |

---

## 📅 Kế hoạch Tiếp theo (Upcoming Roadmap)

### 🎯 Milestone 4: Quản lý Ngân hàng Câu hỏi & Thống kê Độ khó (Question Bank & Tagging)
- [ ] API lọc ngân hàng câu hỏi theo Kỹ năng (`skillId`), Độ khó (`difficultyLevel`), hoặc Danh mục năng lực (`domainId`).
- [ ] Hỗ trợ gắn tag nâng cao (Tags/Keywords) cho từng câu hỏi phục vụ thuật toán Recommendation của AI Tutor.
- [ ] Cập nhật trạng thái duyệt câu hỏi (`is_verified`, `author_id`) trước khi đưa vào đề thi chính thức.

### 🎯 Milestone 5: Tích hợp Adaptive Testing & Exam Session Tracking
- [ ] Phối hợp cùng phân hệ Evaluation Service để sinh đề thi thích ứng (CAT - Computerized Adaptive Testing) dựa trên năng lực học sinh.
- [ ] Lưu trữ và quản lý tài liệu lý thuyết (`materials`) kèm tệp đính kèm (`file_url`) phục vụ luồng RAG của AI Engine.
- [ ] Tích hợp API Gateway (YARP) tại `V-Eval-Gateway` chuyển tiếp `/api/content/*` về cổng `5249`.

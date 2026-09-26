# KIẾN TRÚC & BẢNG THEO DÕI TIẾN ĐỘ CHỦ THỂ (PROCESS & PLANNING) - V-EVAL CONTENT SERVICE

---

## PHẦN 1: KIẾN TRÚC DỊCH VỤ & CÁC THÀNH PHẦN CẦN TRIỂN KHAI

### 1. Kiến Trúc Clean Architecture & Quản Lý Đề Thi
- **Cổng Dịch Vụ**: `5249` (HTTP) / Container `v_eval_content_service`.
- **Nhiệm Vụ Chính**:
  - Quản lý Ngân hàng đề thi (`mock_exams`), Ma trận câu hỏi (`exam_questions`), Chùm bài đọc (`passages`), và Câu hỏi chi tiết (`questions`).
  - Tiếp nhận dữ liệu đề thi trích xuất từ AI Engine (`POST /api/content/exams/import`).
  - Phục vụ API tra cứu đề thi cho Frontend và các Microservices.

### 2. Sơ Đồ CSDL PostgreSQL Schema `content`
- `mock_exams`: Thông tin tổng quan đề thi (`exam_id`, `title`, `duration_minutes`, `total_questions`).
- `passages`: Bài đọc hiểu chùm câu hỏi có chứa định dạng HTML table 2 tầng (`colspan`/`rowspan`).
- `questions`: Câu hỏi trắc nghiệm chứa công thức LaTeX (`content_latex`), đáp án lựa chọn và giải thích chi tiết.
- `skills`: Danh mục kỹ năng bài học liên kết với câu hỏi.

### 3. Chuẩn Hóa Báo Lỗi & Result Pattern (Đồng Bộ Identity Service)
- **Result Pattern**: Sử dụng `Result<T>` và `Error` (`ErrorType`: `Validation`, `NotFound`, `Conflict`, `Failure`...).
- **Base Controller**: `ApiControllerBase` xử lý tự động `HandleResult(result)` và map sang HTTP status chuẩn RFC 7807 ProblemDetails.
- **FluentValidation**: `ValidationBehavior` trong MediatR pipeline tự động bắt lỗi tham số đầu vào.
- **Bảo Mật Đề Thi Khảo Thí (Anti-cheat)**: `DiagnosticExamDto` loại bỏ đáp án đúng `CorrectOption` và lời giải `Explanation`.

---

## PHẦN 2: BẢNG THEO DÕI TIẾN ĐỘ CHI TIẾT THEO TỪNG MỤC (PROGRESS MATRIX)

| STT | Hạng Mục / Chức Năng | Vị Trí Triển Khai trong Code | Trạng Thái | Tiến Độ (%) | Ghi Chú Chi Tiết |
| :---: | :--- | :--- | :---: | :---: | :--- |
| 1 | **Clean Architecture & EF Core** | Entire Solution | 🟢 Hoàn thành | 100% | `ContentDbContext` ánh xạ schema `content` Supabase |
| 2 | **Import Đề Thi từ AI Engine** | `Features/Exams/Commands/Import/` | 🟢 Hoàn thành | 100% | API `POST /api/content/exams/import` khép kín 2 chiều |
| 3 | **Bảo Toàn LaTeX & Bảng HTML** | `Features/Exams/Commands/Import/` | 🟢 Hoàn thành | 100% | Bảo toàn `\frac`, `\Delta`, bảng 2 tầng `colspan`/`rowspan` |
| 4 | **Lưu Trữ Ảnh Minh Họa Câu Hỏi** | `Domain/Entities/` | 🟢 Hoàn thành | 100% | Chèn Markdown Image `![Hình minh họa](url)` chuẩn xác |
| 5 | **API Lấy Danh Sách Đề Thi** | `API/Controllers/MockExamsController.cs` | 🟢 Hoàn thành | 100% | API `GET /api/v1/content/exams` |
| 6 | **API Lấy Chi Tiết Đề Thi** | `API/Controllers/MockExamsController.cs` | 🟢 Hoàn thành | 100% | API `GET /api/v1/content/exams/{id}` |
| 7 | **API Xóa Đề Thi Cascade** | `API/Controllers/MockExamsController.cs` | 🟢 Hoàn thành | 100% | API `DELETE /api/v1/content/exams/{id}` |
| 8 | **Dockerfile & Compose** | `Dockerfile` | 🟢 Hoàn thành | 100% | Cổng 5249 kết nối mạng nội bộ `veval_network` |
| 9 | **Script Push Độc Lập** | `Scripts/push.bat` | 🟢 Hoàn thành | 100% | Hỗ trợ 3 chế độ push kèm kiểm tra lịch sử |
| 10 | **Result Pattern & Error Handling** | `Application/Common/Models/` & `API/Controllers/Base/` | 🟢 Hoàn thành | 100% | Triển khai `Result<T>`, `ErrorType`, `ApiControllerBase` |
| 11 | **Validation Pipeline Behavior** | `Application/Common/Behaviors/` | 🟢 Hoàn thành | 100% | `ValidationBehavior` tích hợp FluentValidation |
| 12 | **Đề Thi Chẩn Đoán (30 câu V-ACT)** | `Application/Diagnostic/` & `API/Controllers/DiagnosticController.cs` | 🟢 Hoàn thành | 100% | `GET /api/v1/content/diagnostic-test` (Anti-cheat 100%) |
| 13 | **Seeding Đề Chẩn Đoán Mẫu** | `Infrastructure/Persistence/Seeds/DiagnosticExamSeeder.cs` | 🟢 Hoàn thành | 100% | Tự động tạo đề 30 câu khi app khởi động |
| 14 | **Swagger UI Trực Quan** | `API/Program.cs` | 🟢 Hoàn thành | 100% | Giao diện Swagger phân nhóm tại `:5249/swagger` |
| 15 | **gRPC Server (Chấm Điểm)** | `API/Services/ContentGrpcService.cs` | 🟢 Hoàn thành | 100% | RPC `GetExamAnswerKey` phục vụ Practice Service |
| 16 | **Ngân Hàng Câu Hỏi Tự Sinh AI**| `API/Controllers/MockExamsController.cs` | 🟢 Hoàn thành | 100% | Nhận đề sinh từ AI Engine qua `POST /api/v1/content/exams/import` |
| 17 | **Phê Duyệt & Xuất Bản Đề Thi** | `Application/MockExams/Commands/PublishMockExam/` | 🟢 Hoàn thành | 100% | `PublishMockExamCommand`, route `PATCH /api/v1/content/exams/{id}/publish` |
| 18 | **Chuẩn Hóa Bloom 6 Cấp Độ** | `Domain/Constants/BloomTaxonomy.cs` | 🟢 Hoàn thành | 100% | Revised Bloom's Taxonomy 6 cấp độ định lượng độ khó câu hỏi |
| 19 | **Chuẩn Hóa Múi Giờ Việt Nam** | `API/Controllers/MockExamsController.cs` & DTOs | 🟢 Hoàn thành | 100% | Role timezone `Asia/Ho_Chi_Minh`, trường `createdAtVn` format chuẩn |

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
| 5 | **API Lấy Danh Sách Đề Thi** | `API/Controllers/MockExamsController.cs` | 🟢 Hoàn thành | 100% | API `GET /api/content/exams` |
| 6 | **API Lấy Chi Tiết Đề Thi** | `API/Controllers/MockExamsController.cs` | 🟢 Hoàn thành | 100% | API `GET /api/content/exams/{id}` |
| 7 | **API Xóa Đề Thi Cascade** | `API/Controllers/MockExamsController.cs` | 🟢 Hoàn thành | 100% | API `DELETE /api/content/exams/{id}` |
| 8 | **Dockerfile & Compose** | `Dockerfile` | 🟢 Hoàn thành | 100% | Cổng 5249 kết nối mạng nội bộ `veval_network` |
| 9 | **Script Push Độc Lập** | `Scripts/push.bat` | 🟢 Hoàn thành | 100% | Hỗ trợ 3 chế độ push kèm kiểm tra lịch sử |
| 10 | **Result Pattern & Error Handling** | `Application/Common/Models/` & `API/Controllers/Base/` | 🟢 Hoàn thành | 100% | Triển khai `Result<T>`, `ErrorType`, `ApiControllerBase` |
| 11 | **Validation Pipeline Behavior** | `Application/Common/Behaviors/` | 🟢 Hoàn thành | 100% | `ValidationBehavior` tích hợp FluentValidation |
| 12 | **Đề Thi Chẩn Đoán (30 câu V-ACT)** | `Application/Diagnostic/` & `API/Controllers/DiagnosticController.cs` | 🟢 Hoàn thành | 100% | `GET /api/content/diagnostic-test` (Anti-cheat 100%, hỗ trợ `excludeExamId` Unhappy Case 2) |
| 13 | **Seeding Đề Chẩn Đoán Mẫu** | `Infrastructure/Persistence/Seeds/DiagnosticExamSeeder.cs` | 🟢 Hoàn thành | 100% | Tự động tạo đề 30 câu khi app khởi động |
| 14 | **Swagger UI Trực Quan** | `API/Program.cs` | 🟢 Hoàn thành | 100% | Giao diện Swagger phân nhóm tại `:5249/swagger` |
| 15 | **gRPC Server (Chấm Điểm)** | `API/Services/ContentGrpcService.cs` | 🟢 Hoàn thành | 100% | RPC `GetExamAnswerKey` phục vụ Practice Service |
| 16 | **SkillPrerequisites & DAG Seeding** | `Domain/Entities/SkillPrerequisite.cs` & `Infrastructure/Persistence/Seeds/SkillPrerequisiteSeeder.cs` | 🟢 Hoàn thành | 100% | Entity bảng nối, cấu hình Fluent API, seed 12 kỹ năng chuẩn và 9 cung DAG |
| 17 | **gRPC RPC GetSkillsTree** | `API/Services/ContentGrpcService.cs` | 🟢 Hoàn thành | 100% | RPC `GetSkillsTree` cung cấp cấu trúc Cây khung năng lực cho Practice Service |
| 18 | **Ngân Hàng Câu Hỏi Tự Sinh AI**| `API/Controllers/MockExamsController.cs` | 🟢 Hoàn thành | 100% | Nhận đề sinh từ AI Engine qua `POST /api/content/exams/import` |
| 19 | **Phê Duyệt & Xuất Bản Đề Thi** | `Application/MockExams/Commands/PublishMockExam/` | 🟢 Hoàn thành | 100% | `PublishMockExamCommand`, route `PATCH /api/content/exams/{id}/publish` |
| 20 | **Chuẩn Hóa Bloom 6 Cấp Độ** | `Domain/Constants/BloomTaxonomy.cs` | 🟢 Hoàn thành | 100% | Revised Bloom's Taxonomy 6 cấp độ định lượng độ khó câu hỏi |
| 21 | **Chuẩn Hóa Múi Giờ Việt Nam** | `API/Controllers/MockExamsController.cs` & DTOs | 🟢 Hoàn thành | 100% | Role timezone `Asia/Ho_Chi_Minh`, trường `createdAtVn` format chuẩn |
| 22 | **Core Flow 2 - API 16: Thêm Câu Hỏi Ngân Hàng Gốc** | `Application/Questions/Commands/CreateQuestion/` | 🟢 Hoàn thành | 100% | **Phụ trách: ThinhTT**. `POST /api/content/questions` thêm câu hỏi trắc nghiệm (LaTeX, Bloom 1-6, 4 options, explanation, SkillId) |
| 23 | **Core Flow 2 - API 17: Hiệu Đính Nội Dung Câu Hỏi** | `Application/Questions/Commands/UpdateQuestion/` | 🟢 Hoàn thành | 100% | **Phụ trách: ThinhTT**. `PUT /api/content/questions/{id}` sửa nội dung, đáp án, lời giải, Bloom |
| 24 | **Core Flow 2 - API 18: Xóa / Vô Hiệu Hóa Câu Hỏi** | `Application/Questions/Commands/DeleteQuestion/` | 🟢 Hoàn thành | 100% | **Phụ trách: ThinhTT**. `DELETE /api/content/questions/{id}` xóa câu hỏi và giải phóng liên kết đề thi |
| 25 | **Core Flow 2 - API 19: Đóng Gói Bộ Đề Quiz Củng Cố**| `Application/MockExams/Commands/CreateQuizExam/` | 🟢 Hoàn thành | 100% | **Phụ trách: ThinhTT**. `POST /api/content/exams/quiz` đóng gói đề Quiz 5-10 câu theo `SkillIds` / `CommonWeakSkillIds` chuẩn Bloom |
| 26 | **Core Flow 2 - API 20: Tạo Bài Giảng Video Lý Thuyết** | `MaterialsController.cs` & `CreateMaterialCommandHandler.cs` | 🟢 Hoàn thành | 100% | `POST /api/content/materials` tạo và liên kết bài giảng video lý thuyết chuẩn (`Title`, `VideoUrl`, `DurationSeconds`) |
| 27 | **Core Flow 2 - API 21: Tra Cứu Video Bài Giảng Kỹ Năng**| `MaterialsController.cs` & `GetMaterialsBySkillQueryHandler.cs` | 🟢 Hoàn thành | 100% | `GET /api/content/materials/by-skill/{skillId}` tra cứu video bài giảng lý thuyết theo kỹ năng chặng học |
| 28 | **Core Flow 2 (Thematic Cohort - Bước 2): Bổ Sung DomainCode Vào gRPC GetSkillsTree** | `Protos/content.proto` & `API/Services/ContentGrpcService.cs` | 🟢 Hoàn thành | 100% | Mở rộng message `SkillNode` bổ sung `domain_code`, ánh xạ mã miền chuẩn cho 12 kỹ năng DAG |
| 29 | **Dàn Kiến Thức & Khung Tri Thức Động (Curriculum API)** | `CurriculumController.cs` & `GetCurriculumTreeQuery.cs` | 🟢 Hoàn thành | 100% | `GET /api/content/curriculum` cung cấp cấu trúc 4 miền, 12 kỹ năng chuẩn, công thức KaTeX và GraphRAG từ CSDL |
| 30 | **Phân Cấp Tri Thức SGK 3 Tầng & 4 Khối Sư Phạm Động 100%** | `CurriculumController.cs` & `GetCurriculumTreeQuery.cs` | 🟢 Hoàn thành | 100% | Cấu trúc Chương -> Bài -> Dạng bài kèm 4 khối: Nền tảng SGK, Phương pháp riêng, Video giảng, Quiz thích ứng (truy vấn CSDL 100%, xóa bỏ toàn bộ hardcode C#) |
| 31 | **Nạp Tri Thức Động Kép (Giảng Viên & AI Ingestion)** | `ImportCurriculumCommand.cs` & `CurriculumController.cs` | 🟢 Hoàn thành | 100% | `POST /api/content/curriculum/import` tiếp nhận dữ liệu phân cấp tri thức từ Giảng viên hoặc AI OCR SGK |
| 32 | **Bộ REST APIs Hiệu Đính Sư Phạm Dành Riêng Cho Giảng Viên** | `Commands/UpdatePattern/`, `UpdateNode/`, `CreateNode/`, `DeleteNode/` | 🟢 Hoàn thành | 100% | `PUT patterns/{id}`, `PUT nodes/{id}`, `POST nodes`, `DELETE nodes/{id}` hỗ trợ giảng viên toàn quyền hiệu đính 4 khối nội dung sau khi AI nạp |
| 33 | **Bộ Chuyển Đổi Dữ Liệu Universal Ingestion (0% Hardcode)** | `FlexibleCharJsonConverter.cs` & `ImportCurriculumCommandHandler.cs` | 🟢 Hoàn thành | 100% | Tiếp nhận linh hoạt mọi định dạng đáp án đúng (`"A."`, `"Đáp án B"`, `0..3`, `null`) từ AI OCR mà không bị lỗi System.Char conversion (0% hardcode) |
| 34 | **Phòng Thủ Toàn Diện Model Validation & Null-Coalescing Import DTOs** | `CurriculumDtos.cs` & `ImportCurriculumCommandHandler.cs` | 🟢 Hoàn thành | 100% | Tái cấu trúc các trường DTO sang nullable (`string?`) kết hợp chuỗi fallback an toàn cho `TheoryRecap`, `SolutionMethod`, `PatternName`, triệt tiêu 100% lỗi HTTP 400 Bad Request |
| 35 | **Xóa Chương Tổng Hợp & Đa Hình Node ID (Synthetic Chapter Cascade)** | `CurriculumController.cs` & `DeleteNodeCommand.cs` | 🟢 Hoàn thành | 100% | Hỗ trợ xóa cả Guid thực tế lẫn synthetic container `chap-general-{domainId}`, cascade xóa toàn bộ flat skills và quan hệ DAG, triệt tiêu lỗi HTTP 404 Not Found |



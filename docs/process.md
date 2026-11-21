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
| 36 | **Nâng Giới Hạn Tải Tệp Kestrel & FormOptions (250 MB)** | `Program.cs` | 🟢 Hoàn thành | 100% | Mở rộng Kestrel `MaxRequestBodySize` và `MultipartBodyLengthLimit` lên 250 MB tiếp nhận trọn vẹn tệp đề thi và khung tri thức lớn |
| 37 | **Xóa Toàn Diện Miền Năng Lực & Bảo Vệ Danh Mục Miền Chuẩn** | `DeleteNodeCommand.cs`, `ImportCurriculumCommand.cs` | 🟢 Hoàn thành | 100% | Hỗ trợ xóa toàn bộ Miền năng lực (`domain-{id}` / Guid), cascade xóa sạch các Skill, Prerequisite, Question, Material, xóa Domain entity; giữ nguyên tên miền chuẩn khi AI import |
| 38 | **Chuyển Toàn Bộ Nhánh Làm Việc Sang Main, Cấu Hình Kestrel ListenAnyIP & Đóng Gói Docker Container Cho Hệ Thống V-Eval** | `Program.cs`, `Dockerfile`, `docker-compose.yml` | 🟢 Hoàn thành | 100% | Cấu hình `ListenAnyIP` cho cổng REST (5249) và gRPC (5250) thay thế `ListenLocalhost`, mở cổng trong Dockerfile và ánh xạ mạng Docker Compose, biên dịch và triển khai phục vụ endpoint `https://api.pickic.io.vn/api/content/curriculum` thành công 100% |
| 39 | **Chuẩn Hóa 100% CSDL Động Khung Chương Trình 4 Miền GDPT 2018 & Loại Bỏ Miền Dư Thừa** | `GetCurriculumTreeQuery.cs`, PostgreSQL CSDL | 🟢 Hoàn thành | 100% | Di chuyển toàn bộ kỹ năng Quan hệ song song về Toán, Hóa học về KHTN, Sử/Địa về KHXH; xóa bỏ miền "Tổng hợp" rác; nạp 16 Chương, 22 Bài học, 35 Dạng bài, 121 Câu hỏi Bloom L1-L4; tinh chỉnh `GetCurriculumTreeQuery` chỉ hiện cấu trúc phân cấp chuẩn, zero-mock, zero-hardcode |
| 40 | **Rà Soát Toàn Diện Mục Lục SGK GDPT 2018 (Chân Trời Sáng Tạo) & Nạp 100% CSDL Động Đầy Đủ 130 Chương Vào PostgreSQL** | `curriculum_master_seed.sql`, CSDL PostgreSQL `v_eval_content` | 🟢 Hoàn thành | 100% | Nạp trọn vẹn 9 Chương và 23 Bài học của Vật lí 10 (Chân trời sáng tạo), Vật lí 11-12, Hóa học 10-12, Sinh học 10-12, Toán học 10-12, Ngôn ngữ, Xã hội. Đạt mốc 130 Chương, 208 Bài học, 221 Dạng bài, 388 Materials và 562 Questions Bloom L1-L4. 100% Dynamic Database |
| 41 | **Rà Soát 17 Liên Kết Mục Lục SGK Chân Trời Sáng Tạo, Nạp Đầy Đủ 139 Chương & Tối Ưu Sắp Xếp Sư Phạm Khối Lớp Tự Nhiên** | `GetCurriculumTreeQuery.cs`, CSDL PostgreSQL `v_eval_content` | 🟢 Hoàn thành | 100% | Rà soát toàn bộ 17 liên kết SGK Chân trời sáng tạo (Toán 10-12, Vật lí 10-12, Hóa học 10-12, Sinh học 10-12, Ngữ văn 10-12, Tiếng Anh, Sử, Địa, GDKT&PL). Nạp 139 Chương, 337 Bài học, 337 Dạng bài, 343 Materials, 588 Questions. Sắp xếp tự nhiên theo Khối lớp (`10 -> 11 -> 12`) và Số thứ tự Chương/Bài. 100% Dynamic Database. |
| 42 | **Triệt Tiêu 100% Generic Hardcode, Xây Dựng Master Pedagogical KB & Chuẩn Hóa Tri Thức Đặc Thù Từng Bài Học** | `pedagogical_knowledge_base.py`, CSDL PostgreSQL `v_eval_content` | 🟢 Hoàn thành | 100% | Xóa bỏ 100% công thức mẫu chung (`` `\int x^n dx` ``, `` `\Delta = b^2 - 4ac` ``, `` `2\text{Fe} + 3\text{Cl}_2` ``); xây dựng Master Pedagogical Knowledge Base cho 8 phân môn; nạp lại CSDL 139 Chương, 337 Bài học, 337 Dạng bài, 343 Materials KaTeX đặc thù và 588 Questions ĐGNL V-ACT. Zero-generic, zero-hardcode. |
| 43 | **Loại Bỏ Hoàn Toàn Placeholder Chuỗi Tĩnh & Chuẩn Hóa Dữ Liệu Thiếu (Zero Hardcode)** | `GetCurriculumTreeQuery.cs` | 🟢 Hoàn thành | 100% | Loại bỏ các chuỗi mặc định ("Kiến thức nền tảng trong cơ sở dữ liệu.", "Phương pháp giải chi tiết trong cơ sở dữ liệu.") khỏi `GetCurriculumTreeQuery.cs`; trả về chuỗi rỗng khi học liệu chưa có trong CSDL giúp frontend phân định và hiển thị trạng thái "Chưa có dữ liệu nguồn". |
| 44 | **Đa Hình Route Hiệu Đính Dạng Bài & Auto-Upsert Khi Lưu Từ Studio Sư Phạm** | `CurriculumController.cs`, `UpdatePatternCommand.cs` | 🟢 Hoàn thành | 100% | Mở rộng route `PUT /api/content/curriculum/patterns/{id}` chấp nhận cả GUID và mã chuỗi SGK Studio, tự động ánh xạ theo tên hoặc khởi tạo `Skill` + `Material` + `Question` mới khi chưa tồn tại, triệt tiêu 100% lỗi HTTP 404 khi lưu trong Studio. |
| 45 | **Liên Kết Câu Hỏi Đề Thi Vào Cây Node Khung Chương Trình & Truy Xuất Phả Hệ 4 Tầng** | `ImportMockExamCommand.cs`, `ImportMockExamCommandHandler.cs`, `MockExamDetailDto.cs`, `GetMockExamByIdQueryHandler.cs` | 🟢 Hoàn thành | 100% | Hỗ trợ gắn trực tiếp `SkillId` của nút Dạng bài/Bài học/Chương khi `POST /api/content/exams/import` (chấm dứt tạo Skill mồ côi, loại bỏ N+1 query); nâng cấp `GET /api/content/exams/{id}` lần ngược `Skill.Parent` trả về đầy đủ `domain_name`, `chapter_name`, `lesson_name`, `pattern_name`. |
| 46 | **Bảo Vệ Khóa Ngoại Khi Xóa Đề Thi (`DELETE /api/content/exams/{id}`)** | `DeleteMockExamCommandHandler.cs` | 🟢 Hoàn thành | 100% | Chỉ xóa các câu hỏi độc quyền không bị tham chiếu bởi đề thi khác (`sharedQuestionIds`) và dọn chùm đọc hiểu mồ côi an toàn, chống lỗi FK PostgreSQL 23503; `dotnet build` 0 Error, `3/3` tests pass. |



| 47 | **Đồng Bộ `content.proto`, Trả Ngữ Cảnh Đọc Hiểu `Passage` Cho PLAR, 3 Bộ Đề Diagnostic 30 Câu Chuẩn 9 Môn GDPT 2018 & Chống Spam `GetMilestoneQuiz`** | `content.proto`, `ContentGrpcService.cs`, `DiagnosticExamSeeder.cs`, `GetDiagnosticTestQuery.cs`, `DiagnosticController.cs` | 🟢 Hoàn thành | 100% | Đồng bộ full trường phân cấp/môn học/học liệu và `passage_*` (`8..10`) trên `QuizQuestionItem`, nạp `.Include(q => q.Passage)` và lọc bỏ câu hỏi đọc hiểu mồ côi thiếu `Passage`, gieo 3 đề Diagnostic 30 câu cân bằng đủ 9 môn từ 337 dạng bài SGK thật, hỗ trợ `excludeExamIds` và cờ `read_only` chấm dứt spam bảng `MockExams`. |
| 48 | **Phân Lập Môn Học Tuyệt Đối (Subject Boundary Isolation), Tách `SubjectTaxonomyHelper` & Triệt Tiêu Rò Rỉ Câu Hỏi Chéo Môn PLAR** | `ContentGrpcService.cs`, `SubjectTaxonomyHelper.cs`, `ContentGrpcServiceSubjectIsolationTests.cs` | 🟢 Hoàn thành | 100% | Thiết lập cơ chế Subject Boundary Isolation trong `GetMilestoneQuiz`: xác định `targetSubject` và lọc sạch câu hỏi khác môn trong đề thi tổng hợp; đóng gói thư viện phân loại `SubjectTaxonomyHelper` hỗ trợ 9–10 môn GDPT 2018; xóa bỏ toàn bộ fallback chéo môn; unit tests đạt 16/16 Passed 100%. |

| 49 | **Siết Chặt Biên Giới Chương & Bộ Lọc Ngôn Ngữ Chống Lọt Câu Tiếng Anh Trong PLAR** | `SubjectTaxonomyHelper.cs`, `ContentGrpcService.cs`, `ContentGrpcServiceSubjectIsolationTests.cs` | 🟢 Hoàn thành | 100% | Bổ sung `IsContentLanguageMismatch` chặn câu hỏi Tiếng Anh gán nhầm môn trong DB; siết bước `1d` chỉ mở rộng toàn môn khi dạng bài & chương có 0 câu hỏi; `dotnet test` 18/18 Passed. |

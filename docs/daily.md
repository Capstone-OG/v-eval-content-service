# NHẬT KÝ KIỂM TRẢ TIẾN ĐỘ VẬN HÀNH (DAILY CHECK LOG) - CONTENT SERVICE

## [27/09/2026] - Chuẩn Hóa Thang Đo Tư Duy Bloom 6 Cấp & Bổ Sung Endpoint Phê Duyệt Xuất Bản Đề Thi (Publish MockExam)
- **Chuẩn Hóa Thang Đo Tư Duy Bloom 6 Mức Độ Cho Ngân Hàng Câu Hỏi**:
  - Khởi tạo class `BloomTaxonomy.cs` định nghĩa 6 mức độ nhận thức: 1. Nhận biết (Remembering), 2. Thông hiểu (Understanding), 3. Vận dụng (Applying), 4. Phân tích (Analyzing), 5. Đánh giá (Evaluating), 6. Sáng tạo (Creating).
  - Đồng bộ hoá toàn diện quy ước độ khó câu hỏi phục vụ các truy vấn phân tích chẩn đoán (Diagnostic Test) và trích xuất đáp án gRPC (`GetExamAnswerKey`).
- **Bổ Sung Command Phê Duyệt & Xuất Bản Đề Thi (`PublishMockExamCommand`)**:
  - Đề thi import vào CSDL Supabase PostgreSQL qua `POST /api/v1/content/exams/import` mặc định mang trạng thái **`IsPublished = false` (Chờ duyệt / Pending Approval)**.
  - Xây dựng `PublishMockExamCommand` và handler cập nhật trạng thái `IsPublished = true` (Đã duyệt / Published).
  - Cung cấp 2 route: `PATCH /api/v1/content/exams/{id}/publish` và `PUT /api/v1/content/exams/{id}/publish` trong `MockExamsController`.
- **Chuẩn Hóa Múi Giờ Việt Nam (Asia/Ho_Chi_Minh UTC+7) Cho CSDL & API**:
  - Cấu hình server role CSDL Supabase PostgreSQL: `ALTER ROLE postgres SET timezone TO 'Asia/Ho_Chi_Minh';` giúp toàn bộ truy vấn SQL trả về trực tiếp giờ Việt Nam (`+07:00`) thay vì UTC.
  - Bổ sung trường `createdAtVn` trong `MockExamSummaryDto` và `MockExamDetailDto` tự động định dạng chuẩn ngày giờ Việt Nam (`dd/MM/yyyy HH:mm:ss`), đảm bảo hiển thị đúng ngày hôm nay `27/09/2026`.
- **Kiểm Thử Biên Dịch**: `dotnet build` giải pháp `V-Eval-Content_Service.sln` đạt **0 Error(s), 0 Warning(s)**, gọi test PATCH thành công trả về `{ is_published: true }`, API trả về `createdAtVn: 27/09/2026 03:16:47`.

---

## [24/09/2026] - Bổ Sung Route Aliases `/api/content/exams` & Tối Ưu HTTPS Redirection Pipeline
- **Bổ Sung Route AliasesSong Song (`MockExamsController` & `DiagnosticController`)**:
  - Đã thêm `[Route("api/content/exams")]` song song với `[Route("api/v1/content/exams")]` cho `MockExamsController`.
  - Đã thêm `[Route("api/content")]` song song với `[Route("api/v1/content")]` cho `DiagnosticController`.
  - Khắc phục hoàn toàn lỗi `404 (Not Found)` khi Web Viewer gọi `GET http://localhost:5249/api/content/exams`.
- **Cấu Hình HTTP Pipeline (`Program.cs`)**:
  - Tối ưu hóa `app.UseHttpsRedirection()` chỉ áp dụng khi không ở môi trường `Development`, loại bỏ cảnh báo `Failed to determine the https port for redirect` khi chạy Kestrel trên HTTP port `5249` (REST/Swagger) và port `5250` (gRPC Server).
- **Kiểm Thử & Khởi Chạy**:
  - Xử lý triệt để lỗi kết lộ route 404 cho Web Client và sẵn sàng phục vụ bài thi chẩn đoán 30 câu.

---

## [22/09/2026] - Mở Rộng gRPC Answer Keys Trả Về SkillName & DomainName
- **Cập Nhật `content.proto` & Server `ContentGrpcService`**:
  - Thêm `skill_name`, `domain_id`, `domain_name` vào message `QuestionAnswerKey`.
  - Nạp eager loading quan hệ `Question -> Skill -> Domain` trong `ContentGrpcService.GetExamAnswerKey`.
- **Kiểm Thử**: Biên dịch Solution sạch 100% (**0 Error, 0 Warning**), khởi chạy dịch vụ cổng 5249/5250 sẵn sàng phục vụ Practice Service.

---

## [20/09/2026] - Chuẩn Hóa Kiến Trúc Chuyên Nghiệp (Result Pattern, ErrorType, Controllers, Swagger UI), Đề Thi Chẩn Đoán 30 Câu & gRPC Server
- **Chuẩn Hóa Kiến Trúc Giống Identity Service (`Application Layer`)**:
  - Triển khai **Result Pattern**: [`Error.cs`](file:///d:/Capstone/All%20Services/V-Eval-Content_Service/V-Eval-Content_Service.Application/Common/Models/Error.cs) (hỗ trợ `ErrorType` từ Validation, NotFound, Conflict, Failure, Unauthorized đến Forbidden) và [`Result.cs`](file:///d:/Capstone/All%20Services/V-Eval-Content_Service/V-Eval-Content_Service.Application/Common/Models/Result.cs).
  - Tích hợp **FluentValidation**: Cài đặt `FluentValidation.DependencyInjectionExtensions 12.1.1`, cấu hình [`ValidationBehavior.cs`](file:///d:/Capstone/All%20Services/V-Eval-Content_Service/V-Eval-Content_Service.Application/Common/Behaviors/ValidationBehavior.cs) tự động đóng gói lỗi xác thực vào `Result<T>.Failure(errors)`.
- **Triển Khai Tính Năng Đề Thi Chẩn Đoán 30 Câu (Core Flow 1 - Bước 2)**:
  - DTO chống gian lận: [`DiagnosticExamDto.cs`](file:///d:/Capstone/All%20Services/V-Eval-Content_Service/V-Eval-Content_Service.Application/Diagnostic/DTOs/DiagnosticExamDto.cs) loại bỏ 100% `CorrectOption` và `Explanation` để bảo mật đề khi gửi về client học sinh.
  - Query & Handler: [`GetDiagnosticTestQuery.cs`](file:///d:/Capstone/All%20Services/V-Eval-Content_Service/V-Eval-Content_Service.Application/Diagnostic/Queries/GetDiagnosticTest/GetDiagnosticTestQuery.cs) truy xuất đề chẩn đoán `DIAGNOSTIC` trong CSDL PostgreSQL Supabase.
  - Tự động Seeding đề chẩn đoán: [`DiagnosticExamSeeder.cs`](file:///d:/Capstone/All%20Services/V-Eval-Content_Service/V-Eval-Content_Service.Infrastructure/Persistence/Seeds/DiagnosticExamSeeder.cs) tự động tạo đề chẩn đoán 30 câu chuẩn mẫu nếu CSDL chưa có.
- **Tầng API & Controller Chuẩn Hóa (`API Layer`)**:
  - [`ApiControllerBase.cs`](file:///d:/Capstone/All%20Services/V-Eval-Content_Service/V-Eval-Content_Service.API/Controllers/Base/ApiControllerBase.cs): Kế thừa `ControllerBase`, tự động map `Result<T>` và `ErrorType` sang mã HTTP (400, 404, 409...) kèm format JSON báo lỗi chi tiết.
  - [`DiagnosticController.cs`](file:///d:/Capstone/All%20Services/V-Eval-Content_Service/V-Eval-Content_Service.API/Controllers/DiagnosticController.cs): Cung cấp endpoint `GET /api/v1/content/diagnostic-test`.
  - [`MockExamsController.cs`](file:///d:/Capstone/All%20Services/V-Eval-Content_Service/V-Eval-Content_Service.API/Controllers/MockExamsController.cs): Chuyển đổi toàn bộ Minimal APIs cũ sang Controller chuẩn RESTful.
  - [`GlobalExceptionHandlerMiddleware.cs`](file:///d:/Capstone/All%20Services/V-Eval-Content_Service/V-Eval-Content_Service.API/Middlewares/GlobalExceptionHandlerMiddleware.cs): Bắt ngoại lệ 500 toàn cục.
  - Tích hợp **Swagger UI**: Cài đặt `Swashbuckle.AspNetCore 7.3.1`, hiển thị Swagger UI chuyên nghiệp tại `http://localhost:5249/swagger`.
- **Liên Dịch Vụ gRPC (Chấm Điểm Bài Nộp Học Sinh)**:
  - Bổ sung RPC `GetExamAnswerKey` trong [`content.proto`](file:///d:/Capstone/grpc/content.proto).
  - Triển khai [`ContentGrpcService.cs`](file:///d:/Capstone/All%20Services/V-Eval-Content_Service/V-Eval-Content_Service.API/Services/ContentGrpcService.cs) cung cấp đáp án an toàn server-to-server cho `Practice_Service`.
- **Kiểm Thử**:
  - `dotnet build` Solution thành công: **0 Warning, 0 Error**.
  - Gọi test `GET /api/v1/content/diagnostic-test` thành công trả về 30 câu hỏi sạch, không lộ đáp án.
  - Gọi test `GET /api/v1/content/exams/{non_exist_id}` trả về 404 với JSON error chuẩn.

---

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

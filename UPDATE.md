# Nhật Ký Cập Nhật (Update Log) - Content Service

## [22/09/2026] - Bổ Sung Thông Tin Kỹ Năng & Miền Năng Lực Trực Quan Vào gRPC Answer Keys
- **Nâng Cấp Hợp Đồng `content.proto` & Server `ContentGrpcService`**:
  - Mở rộng thông điệp `QuestionAnswerKey`: bổ sung 3 trường `skill_name`, `domain_id` và `domain_name`.
  - Tối ưu hóa truy vấn EF Core trong `ContentGrpcService.GetExamAnswerKey`: nạp kèm thông tin quan hệ (`Include(eq => eq.Question).ThenInclude(q => q.Skill).ThenInclude(s => s.Domain)`) trả về tên kỹ năng và tên môn học tiếng Việt trực quan cho `Practice_Service`.
- **Kiểm Thử Vận Hành**: Biên dịch sạch 0 Error, 0 Warning.

## [20/09/2026] - Chuẩn Hóa Kiến Trúc Chuyên Nghiệp (Result Pattern, ErrorType, Controllers, Swagger UI), Đề Thi Chẩn Đoán 30 Câu & gRPC Server
- **Chuẩn Hóa Báo Lỗi & Result Pattern (Đồng Bộ Identity Service)**:
  - Triển khai `Result<T>` và `Error` (`ErrorType`: `Validation`, `NotFound`, `Conflict`, `Failure`, `Unauthorized`, `Forbidden`).
  - Triển khai `ApiControllerBase` tự động ánh xạ `Result<T>` và mã lỗi sang đúng HTTP status code chuẩn và format JSON chi tiết.
  - Tích hợp `ValidationBehavior` qua FluentValidation trong pipeline MediatR.
  - Thêm `GlobalExceptionHandlerMiddleware` xử lý lỗi 500 toàn cục.
- **Core Flow 1 (Bước 2): Đề Thi Chẩn Đoán 30 Câu Khảo Sát Ban Đầu**:
  - API `GET /api/v1/content/diagnostic-test`: Trả về bộ đề thi chẩn đoán 30 câu hỏi chuẩn định dạng V-ACT.
  - Cơ chế chống gian lận (Anti-cheat): Ẩn 100% `CorrectOption` và `Explanation` khi gửi cho học sinh làm bài.
  - Seeder tự động: `DiagnosticExamSeeder` kiểm tra và tự động khởi tạo đề chẩn đoán 30 câu trong CSDL PostgreSQL Supabase khi ứng dụng khởi động.
- **Tầng API & Swagger UI**:
  - Chuyển đổi toàn bộ Minimal APIs sang Controllers chuẩn RESTful (`DiagnosticController`, `MockExamsController`).
  - Tích hợp `Swashbuckle.AspNetCore 7.3.1`, hiển thị Swagger UI phân nhóm trực quan tại `http://localhost:5249/swagger`.
- **Liên Dịch Vụ gRPC (Chấm Điểm Bài Nộp)**:
  - Bổ sung RPC `GetExamAnswerKey` trong `grpc/content.proto`.
  - Triển khai `ContentGrpcService` cung cấp bảng đáp án an toàn server-to-server cho `Practice_Service` chấm điểm bài thi.
- **Kiểm Thử**: Solution biên dịch sạch 100% (`dotnet build` 0 Error, 0 Warning).

## [18/09/2026] - Phát Hành Công Cụ Push Độc Lập `Scripts/push.bat` Cho Content Service
- **Tích Hợp `Scripts/push.bat` Độc Lập**:
  - Khởi tạo script [`Scripts/push.bat`](file:///e:/CapStone/All%20Services/V-Eval-Content_Service/Scripts/push.bat) độc lập cho Content Service.
  - Hỗ trợ Push nhanh trên nhánh hiện tại, chọn nhánh đã có qua Menu đánh số, hoặc tạo nhánh mới tự động.
  - Tích hợp tự động kiểm tra đồng bộ lịch sử Git với Remote, tự động pull code khi bi cham (behind) và đưa ra **Cảnh báo Đỏ (Red Warning)** ngắt quy trình khi bị xung đột lịch sử (Conflict/Diverged).

## [15/09/2026] - Dockerize Content Service & Chuẩn Hóa Docker Compose
- **Dockerfile Multi-Stage .NET 9**:
  - Khởi tạo `Dockerfile` chuẩn cho Content Service (`V-Eval-Content_Service.API`) với cổng `5249`.
  - Kết nối chung mạng nội bộ `veval_network` trong `docker-compose.yml`.

## [14/09/2026] - Chuẩn Hóa Cấu Hình Production & Git Security
- **Khởi Tạo `appsettings.example.json`**:
  - Tạo file cấu hình mẫu chứa `ConnectionStrings` và `JwtSettings`.
- **Bảo Mật Git**:
  - Cập nhật `.gitignore` ẩn các file `appsettings.json` chứa Password thật.

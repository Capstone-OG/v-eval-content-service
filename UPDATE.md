# NHẬT KÝ CẬP NHẬT (UPDATE LOG) - CONTENT SERVICE

## [27/10/2026] - Khởi Tạo Bộ Unit Test Cho FluentValidation Command Đề Thi Và Tích Hợp CI Test

- **Khởi Tạo Dự Án Kiểm Thử `V-Eval-Content_Service.Tests` (3 Tests)**:
  - Thiết lập dự án kiểm thử theo chuẩn xUnit và .NET 9, liên kết với `V-Eval-Content_Service.Application`.
  - Triển khai bộ 3 unit test cho trình xác thực nghiệp vụ [`CreateQuizExamCommandValidatorTests.cs`](./V-Eval-Content_Service.Tests/Features/Exams/Commands/CreateQuizExamCommandValidatorTests.cs):
    - Kiểm thử khi dữ liệu hợp lệ (Title, Duration, MinPassingScore) -> Validator trả về `IsValid == true`.
    - Kiểm thử khi thiếu tiêu đề (Title rỗng) -> Bắt lỗi Validation đúng quy tắc.
    - Kiểm thử khi thời lượng hoặc điểm qua môn không hợp lệ (Duration <= 0, MinPassingScore < 0) -> Bắt lỗi chính xác.
  - Đồng bộ phiên bản `FluentValidation` lên `12.1.1` khớp với `Application` dependency, triệt tiêu lỗi cảnh báo downgrade package `NU1605`.
- **Tích Hợp Tự Động Hóa CI/CD Workflow**:
  - Bổ sung bước thực thi `dotnet test --no-build --configuration Release --verbosity normal` vào `.github/workflows/ci.yml`.

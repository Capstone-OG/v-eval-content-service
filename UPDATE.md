# Nhật Ký Cập Nhật (Update Log) - Content Service

## [27/09/2026] - Chuẩn Hóa Bloom 6 Cấp Độ & Triển Khai Lệnh Phê Duyệt Đề Thi (PublishMockExamCommand)

- **Chuẩn Hóa Thang Đo Tư Duy Bloom 6 Cấp Độ**:
  - Khởi tạo class [`BloomTaxonomy.cs`](./V-Eval-Content_Service.Domain/Constants/BloomTaxonomy.cs) định nghĩa 6 mức độ nhận thức:
    1. `Remembering = 1`: Nhận biết
    2. `Understanding = 2`: Thông hiểu
    3. `Applying = 3`: Vận dụng
    4. `Analyzing = 4`: Phân tích
    5. `Evaluating = 5`: Đánh giá
    6. `Creating = 6`: Sáng tạo
  - Đồng bộ hoá toàn diện quy ước độ khó câu hỏi phục vụ các truy vấn phân tích chẩn đoán (Diagnostic Test) và trích xuất đáp án gRPC (`GetExamAnswerKey`).
- **Triển Khai Command Phê Duyệt Đề Thi (`PublishMockExamCommand`)**:
  - Khi lưu đề thi từ AI Engine (`POST /api/v1/content/exams/import`), đề thi được lưu mặc định với trạng thái `IsPublished = false` (Chờ duyệt / Pending Approval).
  - Triển khai use case `PublishMockExamCommand` và `PublishMockExamCommandHandler` xác thực sự tồn tại của đề thi và cập nhật trạng thái `IsPublished = true` (Đã duyệt / Published).
- **Kiểm Thử Biên Dịch**:
  - `dotnet build` giải pháp `V-Eval-Content_Service.sln` đạt **0 Error(s), 0 Warning(s)**.
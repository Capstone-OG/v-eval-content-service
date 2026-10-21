# Nhật Ký Cập Nhật (Update Log) - Content Service

## [21/10/2026] - Khắc Phục Triệt Để Lỗi 400 Validation (TheoryRecap & System.Char) & Phòng Thủ Toàn Diện Pipeline Nạp Tri Thức CSDL

- **Khắc Phục Lỗi 400 Validation Error (`The TheoryRecap field is required`)**:
  - Tái cấu trúc các DTO nạp tri thức trong [`CurriculumDtos.cs`](./V-Eval-Content_Service.Application/Curriculum/DTOs/CurriculumDtos.cs): Chuyển đổi các trường `TheoryRecap`, `SolutionMethod`, `PatternName`, `OptionA`..`OptionD`, `ContentLatex` sang kiểu nullable (`string?`) kết hợp fallback chuỗi an toàn, ngăn chặn hoàn toàn ASP.NET Core MVC tự động gắn lỗi `[Required]` khi tiếp nhận JSON có giá trị `null` hoặc chuỗi rỗng từ AI OCR.
  - Bổ sung cơ chế phòng thủ đa tầng tại [`ImportCurriculumCommandHandler.cs`](./V-Eval-Content_Service.Application/Curriculum/Commands/ImportCurriculum/ImportCurriculumCommand.cs): Tự động gán nhãn dự phòng khi AI trích xuất thiếu dữ liệu (`"Dạng bài chưa đặt tên"`, `"(Đang cập nhật lý thuyết SGK)"`, `"(Đang cập nhật phương pháp giải)"`), duyệt an toàn danh sách câu hỏi và kỹ năng tiên quyết rỗng.
- **Khắc Phục Lỗi 400 Validation Error (`The JSON value could not be converted to System.Char`)**:
  - Xây dựng bộ converter chuẩn hóa `FlexibleCharJsonConverter` và `FlexibleNullableCharJsonConverter` trong `V_Eval_Content_Service.Application.Common.Converters`: Tự động trích xuất ký tự đáp án chuẩn `'A'..'D'` từ mọi định dạng chuỗi (`"A."`, `"Đáp án B"`, `0..3`, `null`).
  - Đăng ký toàn cục trong `Program.cs` thông qua `.AddJsonOptions(...)` và gắn thuộc tính `[JsonConverter(typeof(FlexibleCharJsonConverter))]` trên các DTO.
- **Biên Dịch & Kiểm Thử Vận Hành**:
  - `V-Eval-Content_Service.Application.csproj` biên dịch sạch 100% (**0 Warning, 0 Error** trên .NET 9 SDK).
  - Kiểm thử trực tiếp `POST /api/content/curriculum/import` với mọi định dạng khuyết thiếu đều nạp thành công 200 OK vào CSDL PostgreSQL.

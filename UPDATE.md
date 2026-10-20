# Nhật Ký Cập Nhật (Update Log) - Content Service

## [21/10/2026] - Khắc Phục Lỗi Nạp Tri Thức CSDL & Chuẩn Hóa Toàn Diện Bộ Chuyển Đổi Dữ Liệu Universal Ingestion (0% Hardcode)

- **Khắc Phục Triệt Để Lỗi 400 Validation Error (`The JSON value could not be converted to System.Char`)**:
  - Giải quyết lỗi nạp tri thức khi trường `CorrectOption` nhận chuỗi có ký tự đặc biệt (`"A."`, `"(B)"`, `"[C]"`, `"D:"`), từ khóa (`"Đáp án C"`, `"Phương án B"`, `"Option A"`) hoặc số nguyên (`0..3`) từ AI OCR và trích xuất tài liệu.
  - Xây dựng bộ converter chuẩn hóa `FlexibleCharJsonConverter` và `FlexibleNullableCharJsonConverter` trong `V_Eval_Content_Service.Application.Common.Converters`: Tự động trích xuất ký tự đáp án chuẩn `'A'..'D'` mà không bao giờ ném lỗi `JsonException`.
  - Đăng ký toàn cục trong `Program.cs` thông qua `.AddJsonOptions(...)` và gắn thuộc tính `[JsonConverter(typeof(FlexibleCharJsonConverter))]` trên các DTO trọng yếu (`ImportQuestionDto`, `CreateQuestionRequestDto`, `UpdateQuestionRequestDto`, `QuestionDto`).
- **Phòng Thủ Đa Tầng Tại Command Handler (`ImportCurriculumCommandHandler.cs`)**:
  - Chuẩn hóa trực tiếp `safeCorrect = char.ToUpperInvariant(qDto.CorrectOption)` và kiểm tra phạm vi `'A'..'D'` trước khi lưu vào thực thể `Question` của PostgreSQL.
- **Biên Dịch & Kiểm Thử Vận Hành**:
  - `V-Eval-Content_Service.sln` biên dịch sạch 100% (**0 Warning, 0 Error** trên .NET 9 SDK).
  - Kiểm thử trực tiếp `POST /api/content/curriculum/import` với mọi dạng dữ liệu thô đều nạp thành công 200 OK vào CSDL PostgreSQL.

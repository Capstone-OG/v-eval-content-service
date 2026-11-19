# NHẬT KÝ CẬP NHẬT (UPDATE LOG) - CONTENT SERVICE

## [19/11/2026] - Đa Hình Hóa API Lưu Dạng Bài Studio (`PUT /api/content/curriculum/patterns/{id}`), Tự Động Upsert & Triệt Tiêu 100% Generic Hardcode

- **Khắc Phục Triệt Để Lỗi HTTP 404 Khi Lưu Bài Giảng Từ Studio Sư Phạm (`CurriculumController.cs` & `UpdatePatternCommand.cs`)**:
  - Mở rộng route `[HttpPut("patterns/{id}")]` tiếp nhận linh hoạt cả GUID chuẩn và mã định danh chuỗi từ SGK Studio (`SGK_...`), loại bỏ lỗi 404 từ bộ định tuyến ASP.NET Core.
  - Bổ sung cơ chế băm GUID xác định (`ParseOrCreateDeterministicGuid`), tự động tìm khớp theo tên dạng bài hoặc khởi tạo mới (Upsert) thực thể `Skill`, `Material` và `Question` trong CSDL PostgreSQL khi dạng bài chưa tồn tại.
- **Loại Bỏ Hoàn Toàn Placeholder Chuỗi Tĩnh Khỏi Backend (`GetCurriculumTreeQuery.cs`)**:
  - Trả về chuỗi rỗng khi học liệu chưa có trong CSDL và ánh xạ 100% dữ liệu động từ PostgreSQL (`v_eval_content`): 139 Chương, 337 Bài học, 337 Dạng bài, 343 Học liệu KaTeX và 588 Câu hỏi.
- **Xác Thực Biên Dịch & Vận Hành Sạch**:
  - `dotnet build`: Succeeded 100%, 0 Error.

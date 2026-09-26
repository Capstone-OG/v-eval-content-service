# Nhật Ký Cập Nhật (Update Log) - Content Service

## [27/09/2026] - Phát Hành API Phê Duyệt Xuất Bản Đề Thi & Chuẩn Hóa Múi Giờ Việt Nam (UTC+7)

- **Bổ Sung Endpoint Phê Duyệt Đề Thi (`PATCH /api/v1/content/exams/{id}/publish`)**:
  - Tích hợp endpoint RESTful trong [`MockExamsController.cs`](./V-Eval-Content_Service.API/Controllers/MockExamsController.cs) tiếp nhận lệnh duyệt đề từ giáo viên.
  - Tự động chuyển đổi cờ `is_published: true` phục vụ hiển thị trên phòng thi học sinh.
- **Chuẩn Hóa Múi Giờ Việt Nam (Asia/Ho_Chi_Minh UTC+7) Cho CSDL & DTOs**:
  - Cấu hình server role CSDL Supabase PostgreSQL: `ALTER ROLE postgres SET timezone TO 'Asia/Ho_Chi_Minh';` giúp toàn bộ truy vấn SQL trả về trực tiếp giờ Việt Nam (`+07:00`) thay vì UTC (`2026-09-27 03:16:47+07`).
  - Bổ sung trường `createdAtVn` trong [`MockExamSummaryDto.cs`](./V-Eval-Content_Service.Application/MockExams/Queries/GetMockExams/MockExamSummaryDto.cs) và [`MockExamDetailDto.cs`](./V-Eval-Content_Service.Application/MockExams/Queries/GetMockExamById/MockExamDetailDto.cs) tự động định dạng chuẩn ngày giờ Việt Nam (`dd/MM/yyyy HH:mm:ss`), hiển thị đúng ngày hôm nay `27/09/2026`.
- **Cập Nhật Ma Trận Tiến Độ & Báo Cáo Nghiệm Thu**:
  - Cập nhật [`docs/process.md`](./docs/process.md) đạt 19 hạng mục 100% hoàn thành.
  - Bổ sung tiêu chuẩn nghiệm thu kiến trúc trong [`docs/architecture_acceptance.md`](./docs/architecture_acceptance.md).
- **Kiểm Thử Vận Hành**:
  - `dotnet build` đạt 0 Error(s), 0 Warning(s).
  - Kiểm thử `PATCH http://localhost:5249/api/v1/content/exams/{id}/publish` thành công HTTP 200 OK.
  - Kiểm thử `GET http://localhost:5249/api/v1/content/exams` trả về `createdAtVn: "27/09/2026 03:16:47"`.
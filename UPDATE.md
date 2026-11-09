# NHẬT KÝ CẬP NHẬT (UPDATE LOG) - CONTENT SERVICE

## [09/11/2026] - Nâng Cấp Giới Hạn Kestrel Request Body & FormOptions Lên 250 MB Cho Dữ Liệu Khảo Thí & Phân Cấp Tri Thức

- **Cấu Hình Giới Hạn Tải Tệp Kestrel & FormOptions ([`Program.cs`](./V-Eval-Content_Service.API/Program.cs))**:
  - Bổ sung cấu hình `options.Limits.MaxRequestBodySize = 262_144_000` (250 MB) và `builder.Services.Configure<FormOptions>` với `MultipartBodyLengthLimit = 262_144_000` (250 MB).
  - Loại bỏ hoàn toàn nguy cơ nghẽn kết nối hoặc từ chối dữ liệu (HTTP 413) khi nạp đề thi lớn, cây khung chương trình phân cấp đồ sộ và tài liệu học tập từ AI Engine hoặc Giảng viên.
- **Biên Dịch & Kiểm Thử**:
  - `dotnet build` đạt kết quả biên dịch sạch `0 Warning, 0 Error`.

# Nhật Ký Cập Nhật (Update Log) - Content Service

## [28/09/2026] - Bổ Sung Tham Số ExcludeExamId Cho Diagnostic Test (Hỗ Trợ Unhappy Case 2)

- **Mở Rộng Endpoint Lấy Đề Chẩn Đoán (`DiagnosticController.cs` & `GetDiagnosticTestQuery.cs`)**:
  - Hỗ trợ tham số truy vấn `excludeExamId` trong API `GET /api/v1/content/diagnostic-test?excludeExamId=...`.
  - Khi học sinh bị khóa bài kiểm tra cũ do quá hạn 24 giờ (Unhappy Case 2), Client có thể truyền ID đề cũ để hệ thống tự động loại trừ và lấy một bộ đề chẩn đoán ngẫu nhiên khác.
  - Tích hợp cơ chế fallback an toàn: nếu không còn đề nào khác ngoài đề bị loại trừ, hệ thống vẫn phục vụ đề chẩn đoán có sẵn để đảm bảo quy trình kiểm tra đầu vào không bị gián đoạn.
- **Kiểm Thử Biên Dịch**:
  - Solution `V-Eval-Content_Service.sln` biên dịch sạch 100% (**0 Warning, 0 Error**).
# Nhật Ký Cập Nhật (Update Log) - Content Service

## [24/09/2026] - Bổ Sung Route Aliases `/api/content/exams` & Xử Lý Dịch Vụ Content Service

- **Khắc Phục Lỗi 404 Route Mismatch (`MockExamsController` & `DiagnosticController`)**:
  - Bổ sung Route Alias `[Route("api/content/exams")]` song song với `[Route("api/v1/content/exams")]` cho `MockExamsController`.
  - Bổ sung Route Alias `[Route("api/content")]` song song với `[Route("api/v1/content")]` cho `DiagnosticController`.
  - Giúp Web Client / AI Engine Web Viewer gọi `GET http://localhost:5249/api/content/exams` thành công 100% (không còn bị HTTP 404).
- **Cấu Hình HTTP Kestrel & Pipeline (`Program.cs`)**: Tắt `app.UseHttpsRedirection()` ở môi trường Development để tránh kẹt middleware HTTPS redirect khi chạy HTTP/1.1 (port 5249) và HTTP/2 gRPC (port 5250).
- **Tự Động Seed Dữ Liệu**: Tự động nạp đề thi chẩn đoán 30 câu vào Supabase PostgreSQL schema `v_eval_content`.
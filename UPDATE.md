# Nhật Ký Cập Nhật (Update Log) - Content Service

## [04/09/2026] - Nâng Cấp API Quản Lý Đề Thi & Tích Hợp Supabase PostgreSQL
- **Bổ sung API Truy Vấn & Xóa Đề Thi (CQRS MediatR)**:
  - Triển khai `GetMockExamsQuery` (`GET /api/content/exams`) trả về danh sách đề thi đã lưu phục vụ cho giao diện chọn đề.
  - Triển khai `GetMockExamByIdQuery` (`GET /api/content/exams/{id}`) lấy toàn bộ chi tiết câu hỏi, chùm bài đọc hiểu, lựa chọn và công thức LaTeX để nạp vào KaTeX Viewer.
  - Triển khai `DeleteMockExamCommand` (`DELETE /api/content/exams/{id}`) hỗ trợ xóa đề thi và cascade xóa các câu hỏi, chùm bài đọc liên quan.
- **Cấu hình CORS Cross-Origin**:
  - Mở quyền kết nối API từ cổng `http://localhost:5104` (AI Engine Web Viewer) và các Web/App client.
- **Khởi tạo Thư mục Tài liệu Kỹ thuật**:
  - Tạo tài liệu `docs/exam_management_api.md` đặc tả chi tiết kiến trúc, payload JSON và các endpoint quản lý đề thi.

---

- Fix CI error in github action

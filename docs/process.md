# KIẾN TRÚC & BẢNG THEO DÕI TIẾN ĐỘ CHỦ THỂ (PROCESS & PLANNING) - V-EVAL CONTENT SERVICE

---

## PHẦN 1: KIẾN TRÚC DỊCH VỤ & CÁC THÀNH PHẦN CẦN TRIỂN KHAI

### 1. Kiến Trúc Clean Architecture & Quản Lý Đề Thi
- **Cổng Dịch Vụ**: `5249` (HTTP) / Container `v_eval_content_service`.
- **Nhiệm Vụ Chính**:
  - Quản lý Ngân hàng đề thi (`mock_exams`), Ma trận câu hỏi (`exam_questions`), Chùm bài đọc (`passages`), và Câu hỏi chi tiết (`questions`).
  - Tiếp nhận dữ liệu đề thi trích xuất từ AI Engine (`POST /api/content/exams/import`).
  - Phục vụ API tra cứu đề thi cho Frontend và các Microservices.

### 2. Sơ Đồ CSDL PostgreSQL Schema `content`
- `mock_exams`: Thông tin tổng quan đề thi (`exam_id`, `title`, `duration_minutes`, `total_questions`).
- `passages`: Bài đọc hiểu chùm câu hỏi có chứa định dạng HTML table 2 tầng (`colspan`/`rowspan`).
- `questions`: Câu hỏi trắc nghiệm chứa công thức LaTeX (`content_latex`), đáp án lựa chọn và giải thích chi tiết.
- `skills`: Danh mục kỹ năng bài học liên kết với câu hỏi.

---

## PHẦN 2: BẢNG THEO DÕI TIẾN ĐỘ CHI TIẾT THEO TỪNG MỤC (PROGRESS MATRIX)

| STT | Hạng Mục / Chức Năng | Vị Trí Triển Khai trong Code | Trạng Thái | Tiến Độ (%) | Ghi Chú Chi Tiết |
| :---: | :--- | :--- | :---: | :---: | :--- |
| 1 | **Clean Architecture & EF Core** | Entire Solution | 🟢 Hoàn thành | 100% | `ContentDbContext` ánh xạ schema `content` Supabase |
| 2 | **Import Đề Thi từ AI Engine** | `Features/Exams/Commands/Import/` | 🟢 Hoàn thành | 100% | API `POST /api/content/exams/import` khép kín 2 chiều |
| 3 | **Bảo Toàn LaTeX & Bảng HTML** | `Features/Exams/Commands/Import/` | 🟢 Hoàn thành | 100% | Bảo toàn `\frac`, `\Delta`, bảng 2 tầng `colspan`/`rowspan` |
| 4 | **Lưu Trữ Ảnh Minh Họa Câu Hỏi** | `Domain/Entities/` | 🟢 Hoàn thành | 100% | Chèn Markdown Image `![Hình minh họa](url)` chuẩn xác |
| 5 | **API Lấy Danh Sách Đề Thi** | `Features/Exams/Queries/GetExams/` | 🟢 Hoàn thành | 100% | API `GET /api/content/exams` danh sách tóm tắt |
| 6 | **API Lấy Chi Tiết Đề Thi** | `Features/Exams/Queries/GetExamById/` | 🟢 Hoàn thành | 100% | API `GET /api/content/exams/{id}` nạp đầy đủ cấu trúc câu hỏi |
| 7 | **API Xóa Đề Thi Cascade** | `Features/Exams/Commands/Delete/` | 🟢 Hoàn thành | 100% | API `DELETE /api/content/exams/{id}` xóa sạch liên kết |
| 8 | **Dockerfile & Compose** | `Dockerfile` | 🟢 Hoàn thành | 100% | Cổng 5249 kết nối mạng nội bộ `veval_network` |
| 9 | **Script Push Độc Lập** | `Scripts/push.bat` | 🟢 Hoàn thành | 100% | Hỗ trợ 3 chế độ push kèm kiểm tra lịch sử |
| 10 | **Ngân Hàng Câu Hỏi Tự Sinh AI**| `Features/Questions/` | 🟡 Đang chờ | 0% | Phát triển tích hợp với AI Engine RAG |

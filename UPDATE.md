# NHẬT KÝ CẬP NHẬT (UPDATE LOG) - CONTENT SERVICE

## [19/11/2026] - Triệt Tiêu 100% Generic Hardcode, Cung Cấp Dữ Liệu Động CSDL & Chuẩn Hóa Thông Báo Nguồn Tri Thức Khuyết Thiếu

- **Loại Bỏ Hoàn Toàn Placeholder Chuỗi Tĩnh Khỏi Backend**:
  - Cập nhật `GetCurriculumTreeQuery.cs`: Thay thế toàn bộ các chuỗi gán mặc định ("Kiến thức nền tảng trong cơ sở dữ liệu.", "Phương pháp giải chi tiết trong cơ sở dữ liệu.") thành chuỗi rỗng khi học liệu chưa có trong CSDL, giúp frontend phân định chính xác và hiển thị trạng thái "Chưa có dữ liệu nguồn".
  - Triệt tiêu triệt để tình trạng lặp lại công thức đạo hàm, tích phân hay biệt thức mẫu ở các bài học Lớp 10.
- **Ánh Xạ Trực Tiếp 100% CSDL PostgreSQL (`v_eval_content`) (Zero-Hardcode)**:
  - Cung cấp dữ liệu phân cấp 4 miền chuẩn ĐGNL qua REST API `GET /api/content/curriculum`:
    - **139 Chương** (Chapters)
    - **337 Bài học** (Lessons)
    - **337 Dạng bài** (Patterns)
    - **343 Học liệu** (Materials) chuyên sâu với lý thuyết KaTeX nguyên bản và công thức đặc thù.
    - **588 Câu hỏi** (Questions) trắc nghiệm ĐGNL có 4 phương án A-B-C-D, đáp án đúng và lời giải chi tiết.
- **Xác Thực Biên Dịch & Vận Hành Sạch**:
  - `dotnet build`: Succeeded 100%, 0 Error.

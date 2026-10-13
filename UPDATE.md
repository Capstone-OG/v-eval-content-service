# Nhật Ký Cập Nhật (Update Log) - Content Service

## [13/10/2026] - Triển Khai Hoàn Thiện Ngân Hàng Câu Hỏi Gốc (APIs 16 - 18) & Đóng Gói Bộ Đề Quiz Thích Ứng (API 19)

- **Triển Khai Bộ 3 REST APIs Quản Trị Ngân Hàng Câu Hỏi Gốc (`QuestionsController.cs`)**:
  - **API 16: `POST /api/content/questions`**: Thêm mới câu hỏi trắc nghiệm gốc với nội dung LaTeX bảo toàn, 4 lựa chọn A-D, phương án đúng `CorrectOption`, độ khó Bloom 1-6 và lời giải chi tiết `Explanation`.
  - **API 17: `PUT /api/content/questions/{id}`**: Hiệu đính nội dung, đáp án, độ khó, cấp độ nhận thức Bloom, giải thích và liên kết bài đọc hiểu.
  - **API 18: `DELETE /api/content/questions/{id}`**: Xóa câu hỏi khỏi ngân hàng đề và tự động giải phóng các liên kết trong `exam_questions`.
  - **APIs Tra Cứu**: `GET /api/content/questions/{id}` và `GET /api/content/questions/by-skill/{skillId}` (hỗ trợ lọc theo cấp độ Bloom).
- **Triển Khai API 19: Đóng Gói Bộ Đề Quiz Thích Ứng (`POST /api/content/exams/quiz`)**:
  - Nhận danh sách `SkillIds` / `CommonWeakSkillIds` (từ thuật toán phân cụm vi mô Practice Service).
  - Thuật toán bốc ngẫu nhiên câu hỏi theo dải độ khó Bloom (mặc định Bloom 1-3), tự động đóng gói thành đề `MockExam` loại `QUIZ`.
  - Trả về `ExamId` (dùng làm `WorksheetId`), `Title` (dùng làm `WorksheetTitle`) và danh sách câu hỏi LaTeX.
- **Biên Dịch & Kiểm Thử**:
  - Toàn bộ solution `V-Eval-Content_Service.sln` biên dịch sạch 100% (**0 Warning, 0 Error**).

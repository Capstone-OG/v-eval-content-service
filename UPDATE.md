# Nhật Ký Cập Nhật (Update Log) - Content Service

## [12/10/2026] - Nghiệm Thu Kiến Trúc Ngân Hàng Câu Hỏi & Đóng Gói Đề Quiz Thích Ứng (APIs 16 - 19)

- **Nghiệm Thu Kiến Trúc Liên Thông Phân Nhóm Vi Mô (Practice Service <-> Content Service)**:
  - Thấu hiểu cơ chế tiếp nhận `CommonWeakSkillIds` từ thuật toán phân cụm vi mô `ClassMicroClusterer` để đóng gói đề tự động.
  - Khẳng định Content Service là Single Source of Truth cho toàn bộ câu hỏi, công thức LaTeX, phân loại Bloom 6 cấp độ và giải thích chi tiết.
- **Đặc Tả Kỹ Thuật Chi Tiết Bộ 4 REST APIs Core Flow 2 (Assignee: ThinhTT)**:
  - **API 16: `POST /api/content/questions`**: Thêm mới câu hỏi trắc nghiệm gốc với nội dung LaTeX bảo toàn, 4 lựa chọn, độ khó IRT `[-3.0, +3.0]`, cấp độ Bloom và lời giải chi tiết.
  - **API 17: `PUT /api/content/questions/{id}`**: Hiệu đính nội dung, đáp án, độ khó, cấp độ nhận thức Bloom và giải thích.
  - **API 18: `DELETE /api/content/questions/{id}`**: Xóa mềm / vô hiệu hóa câu hỏi sai sót trong ngân hàng đề.
  - **API 19: `POST /api/content/exams/quiz`**: Đóng gói đề Quiz củng cố 5-10 câu chuẩn Bloom từ `CommonWeakSkillIds`, tự động sinh đề `MockExam` trả về `worksheetId` và `worksheetTitle` cho Practice Service.
- **Khởi Tạo Nhánh Tính Năng Mới**:
  - Khởi tạo nhánh `ThinhTT/feature-content-questions-quiz-apis` từ `develop` để triển khai mã nguồn bộ 4 APIs.
- **Kiểm Thử Biên Dịch**:
  - Solution `V-Eval-Content_Service.sln` biên dịch sạch 100% (**0 Warning, 0 Error**).

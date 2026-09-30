# Nhật Ký Cập Nhật (Update Log) - Content Service

## [30/09/2026] - Phân Công Triển Khai Giai Đoạn 4: Quản Trị Ngân Hàng Câu Hỏi & Bộ Đề Quiz Củng Cố (APIs 16, 17, 18, 19)

- **Giao Việc Phụ Trách Kỹ Thuật (Assignee: ThinhTT)**:
  - **API 16 (`POST /api/v1/content/questions`)**: Thêm mới câu hỏi trắc nghiệm vào ngân hàng câu hỏi gốc (nội dung LaTeX, 4 đáp án A/B/C/D, đáp án đúng, giải thích chi tiết, gán `SkillId` và cấp độ tư duy Bloom 1-6). *Người phụ trách: ThinhTT*.
  - **API 17 (`PUT /api/v1/content/questions/{questionId}`)**: Hiệu đính nội dung câu hỏi, cập nhật đáp án đúng hoặc lời giải thích trong ngân hàng đề gốc khi phát hiện sai sót chuyên môn. *Người phụ trách: ThinhTT*.
  - **API 18 (`DELETE /api/v1/content/questions/{questionId}`)**: Xóa mềm hoặc vô hiệu hóa câu hỏi trong ngân hàng câu hỏi gốc, bảo toàn tính toàn vẹn dữ liệu cho lịch sử bài nộp. *Người phụ trách: ThinhTT*.
  - **API 19 (`POST /api/v1/content/exams/quiz`)**: Đóng gói và phát hành bộ đề Quiz củng cố chuyên đề chuẩn hóa (5-10 câu hỏi) gắn với `SkillId` (`IsPublished = true`), cung cấp `QuizExamId` và bảng đáp án bảo mật gRPC cho Practice Service để mở khóa chặng học Core Flow 2. *Người phụ trách: ThinhTT*.
- **Tài Liệu Tiến Độ**:
  - Cập nhật chi tiết bảng phân công và phạm vi nghiệp vụ trong [`docs/daily.md`](./docs/daily.md) và [`docs/process.md`](./docs/process.md).

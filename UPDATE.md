# UPDATE - V-Eval-Content_Service

## [20/11/2026]
- **Liên Kết Trực Tiếp Câu Hỏi Đề Thi Vào Cây Node Khung Chương Trình & Truy Xuất Phả Hệ 4 Tầng (`ImportMockExamCommand.cs`, `ImportMockExamCommandHandler.cs`, `MockExamDetailDto.cs`, `GetMockExamByIdQueryHandler.cs`)**:
  - Mở rộng `ImportMockExamCommand.QuestionDto` nhận `SkillId` (`Guid?`), `DomainName`, `ChapterName`, `LessonName`; tối ưu `ImportMockExamCommandHandler` nạp sẵn `Skills`/`Domains` 1 lần (chống N+1 query cho đề 120 câu) và gắn thẳng câu hỏi vào nút thật trên cây Khung chương trình (chấm dứt tạo Skill mồ côi).
  - Nâng cấp `GetMockExamByIdQueryHandler` và `MockExamDetailDto` lần ngược `Skill.Parent -> Parent -> Domain` trả về trọn vẹn `skill_id`, `domain_name`, `chapter_name`, `lesson_name`, `pattern_name` cho mỗi câu hỏi khi tải đề từ Database.
- **Bảo Vệ Khóa Ngoại Khi Xóa Đề Thi (`DeleteMockExamCommandHandler.cs`)**:
  - Kiểm tra câu hỏi dùng chung giữa các đề (`sharedQuestionIds`) trước khi xóa `Questions` và dọn chùm đọc hiểu mồ côi, tránh lỗi khóa ngoại PostgreSQL (`23503`) khi gọi `DELETE /api/content/exams/{id}`.
- **Kiểm Thử & Biên Dịch**: `dotnet build --no-restore` thành công (`0 Error`), `dotnet test --no-build` pass `3/3`.

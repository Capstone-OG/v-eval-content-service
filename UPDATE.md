# UPDATE LOG — V-Eval Content Service

## [24/11/2026] Chặn câu hỏi lỗi đáp án, Khóa/Mở khóa câu hỏi bị báo cáo trên CSDL & Tối ưu Curriculum
- **Lọc câu hỏi lỗi đáp án**: Bổ sung `HasBrokenOrMissingOptions` trong `ContentGrpcService.cs` để tự động loại bỏ câu hỏi thiếu đáp án, chứa placeholder (`...`, `…`, `-`, `?`, `N/A`) hoặc trùng phương án.
- **Khóa câu hỏi bị báo cáo trên CSDL**:
  - Bổ sung 3 cột `is_flagged`, `flag_reason`, `flagged_at` vào `Question.cs`, `ContentDbContext.cs` và lệnh tự động `ALTER TABLE ... ADD COLUMN IF NOT EXISTS` trong `Program.cs`.
  - Bổ sung các endpoint `GET /api/content/questions/flagged`, `POST /api/content/questions/{id}/flag`, `POST /api/content/questions/{id}/unflag` trong `QuestionsController.cs`.
  - Lọc `!q.IsFlagged` tại `ContentGrpcService.cs` và `GetQuestionsBySkillQueryHandler.cs` để câu hỏi đang bị gắn cờ không tiếp tục phát cho học sinh cho tới khi được thẩm định/mở khóa.
- **Hiệu năng Curriculum & gRPC (từ `develop`)**: Bật `QuerySplittingBehavior.SplitQuery`, `EnableRetryOnFailure(3, 3s)`, `IMemoryCache` 5 phút với `SemaphoreSlim` trong `CurriculumController.cs`, và nâng giới hạn gRPC `MaxSendMessageSize`/`MaxReceiveMessageSize` lên `32 MB`.

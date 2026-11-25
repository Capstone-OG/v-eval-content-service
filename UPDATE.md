# UPDATE - V-EVAL CONTENT SERVICE

## [25/11/2026] - Tối Ưu Supabase Egress (>95%): Cache gRPC, Chặn Cartesian Explosion & Tách Truy Vấn `GetCurriculumTree`

- **Chuẩn Hóa Bộ Đệm Danh Mục Tĩnh (`IMemoryCache`)**:
  - Tạo [ContentCacheKeys.cs](./V-Eval-Content_Service.API/Caching/ContentCacheKeys.cs) quản lý tập trung khóa cache (`CurriculumTree`, `SkillsTreeAll`, `SkillsTreeLearningNodes`, `SkillsLookup`, `ExamAnswerKey`) và hàm `InvalidateCatalog`.
  - Cập nhật [ContentGrpcService.cs](./V-Eval-Content_Service.API/Services/ContentGrpcService.cs):
    - Cache `GetSkillsTree` trong **30 phút** (có `SemaphoreSlim` chống cache stampede) + `.AsSplitQuery()`.
    - Cache `GetExamAnswerKey` theo `examId` trong **15 phút** + `.AsSplitQuery()`, thay thế `SELECT * FROM Skills` bằng `GetSkillsLookupAsync` nhẹ (chỉ lấy `SkillId`, `Name`, `ParentId`, cache 30 phút).
    - Thêm `.Select()` projection cho `GetQuestionsByDifficulty` để không kéo `ContentLatex`, `OptionA-D`, `Explanation`.
- **Triệt Tiêu Quét Toàn Bộ Bảng `Questions` Trong `GetCurriculumTree`**:
  - Cập nhật [GetCurriculumTreeQuery.cs](./V-Eval-Content_Service.Application/Curriculum/Queries/GetCurriculumTree/GetCurriculumTreeQuery.cs): tách 3 truy vấn độc lập (`.AsSplitQuery()` cho `CompetencyDomains -> Skills -> Prerequisites`, riêng `Materials`, và chỉ lấy tối đa **1 câu hỏi mẫu + tổng số câu** cho mỗi kỹ năng thay vì kéo toàn bộ bảng `Questions`).
- **Tăng TTL & Xóa Cache Tự Động Khi Ghi**:
  - Cập nhật [CurriculumController.cs](./V-Eval-Content_Service.API/Controllers/CurriculumController.cs) (nâng TTL từ 5 phút lên **30 phút**, gọi `InvalidateCatalog` khi import/update/create/delete), [QuestionsController.cs](./V-Eval-Content_Service.API/Controllers/QuestionsController.cs) và [MockExamsController.cs](./V-Eval-Content_Service.API/Controllers/MockExamsController.cs).

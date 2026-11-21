# UPDATE - CONTENT SERVICE

## [22/11/2026]
- **Siết Chặt Khớp Chương / Dạng Bài & Chặn Câu Hỏi Tiếng Anh Bị Gán Nhầm Môn Trong Chu Trình PLAR (`SubjectTaxonomyHelper.cs`, `ContentGrpcService.cs`, `ContentGrpcServiceSubjectIsolationTests.cs`)**:
  - Bổ sung bộ lọc ngôn ngữ `SubjectTaxonomyHelper.IsContentLanguageMismatch`: tự động loại bỏ các câu hỏi Tiếng Anh thuần (không có dấu tiếng Việt và chứa cấu trúc câu Tiếng Anh) bị gán nhầm vào kỹ năng các môn khác trong CSDL (như Hóa học, Toán học, Vật lí).
  - Siết chặt bước mở rộng `1d` trong `ContentGrpcService.GetMilestoneQuiz`: khi dạng bài hoặc chương hiện tại đã có ít nhất 1 câu hỏi hợp lệ, **dừng ngay việc kéo câu hỏi từ các chương khác** (khắc phục triệt để lỗi học *Tổ hợp - Xác suất* bị kéo câu hỏi *Độ dài đường thẳng*).
  - Bổ sung unit test kiểm chứng bộ lọc ngôn ngữ; `dotnet test --no-restore` đạt **18/18 Passed (100%)**.

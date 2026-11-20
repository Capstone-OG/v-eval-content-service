# UPDATE - V-Eval-Content_Service

## [21/11/2026]
- **Hotfix Phân Lập Biên Giới Môn Học (Subject Boundary Isolation), Tách `SubjectTaxonomyHelper` & Xóa Bỏ Hoàn Toàn Rò Rỉ Câu Hỏi Chéo Môn Trong Chu Trình PLAR (`ContentGrpcService.cs`, `SubjectTaxonomyHelper.cs`, `ContentGrpcServiceSubjectIsolationTests.cs`)**:
  - **Khắc Phục Lỗi Rò Rỉ Chéo Môn (Cross-Subject Leakage)**: Sửa triệt để hiện tượng học sinh học chặng Toán học (ví dụ: `Toán - Lớp 10`, `Chương 9: Phương pháp toạ độ trong mặt phẳng > Bài 2: Đường thẳng trong mặt phẳng toạ độ`) nhưng lại bị cấp phát câu hỏi môn Ngữ văn/Tục ngữ ("Trăng quầng thì hạn, trăng... thì mưa").
  - **Chuẩn Hóa Bộ Phân Loại `SubjectTaxonomyHelper`**: Đưa logic nhận diện môn học và ánh xạ mã miền năng lực `ResolveDomainCode`, `InferSubjectName` vào thư viện dùng chung `V_Eval_Content_Service.Application.Common.Helpers`, hỗ trợ chuẩn 9–10 môn GDPT 2018 (Toán học, Ngữ văn, Tiếng Anh, Vật lí, Hóa học, Sinh học, Lịch sử, Địa lí, GDKT & Pháp luật, Tư duy Logic & Số liệu).
  - **Tái Cấu Trúc gRPC `GetMilestoneQuiz`**:
    + Thiết lập quy tắc **Subject Boundary Isolation**: Khi có `parsedSkillId`, hệ thống xác định chính xác môn học mục tiêu `targetSubject` và tập hợp `sameSubjectSkillIds`.
    + Nếu có `parsedExamId`, chỉ lấy những câu hỏi trong đề thi đó mà **thuộc cùng môn học** (`sameSubjectSkillIds`), loại bỏ 100% câu hỏi môn khác trong đề thi chẩn đoán/thi thử tổng hợp.
    + Tìm câu hỏi theo 3 tầng ưu tiên chuẩn: Tầng 1 (Đúng dạng bài `directAndChildIds`) -> Tầng 2 (Cùng Chương `chapterSkillIds` thuộc môn) -> Tầng 3 (Mở rộng cùng môn học `sameSubjectSkillIds`).
    + **Xóa bỏ hoàn toàn** bước fallback theo `DomainId` chung (vốn gộp chung Văn - Anh hoặc Lý - Hóa - Sinh) và bước fallback bừa bãi toàn bộ bảng `Questions`.
  - **Kiểm Thử Tự Động & Nghiệm Thu**:
    + Bổ sung bộ kiểm thử `ContentGrpcServiceSubjectIsolationTests.cs` kiểm tra tính cách ly môn học tuyệt đối.
    + `dotnet build --no-restore` (0 Warning, 0 Error), `dotnet test --no-restore` (**16/16 Passed 100%**).

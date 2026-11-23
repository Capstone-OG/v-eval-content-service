# V-Eval Content Service — Cập Nhật Tiến Độ

## [23/11/2026] - Chống Trùng Lặp Nội Dung & Xáo Trộn Câu Hỏi Trong `GetMilestoneQuiz`

- **Cải Tiến Bộ Chọn Câu Hỏi gRPC ([`ContentGrpcService.cs`](./V-Eval-Content_Service.API/Services/ContentGrpcService.cs))**:
  - Bổ sung `seenContents` chuẩn hóa khoảng trắng để lọc trùng lặp theo nội dung (`ContentLatex`), ngăn tuyệt đối 2 câu hỏi giống hệt nhau (khác `QuestionId`) xuất hiện trong cùng 1 bài Preview/Quiz.
  - Xáo trộn ngẫu nhiên (`OrderBy(_ => Random.Shared.Next())`) trước khi chọn câu hỏi trong `AppendUnique`, khắc phục tình trạng học sinh lặp lại liên tục các câu đầu tiên trong CSDL.

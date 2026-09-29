# Nhật Ký Cập Nhật (Update Log) - Content Service

## [29/09/2026] - Bổ Sung Trọng Số Đề Thi (Weight) Vào gRPC Message SkillNode & RPC GetSkillsTree

- **Mở Rộng Protocol Buffer & RPC GetSkillsTree**:
  - Bổ sung trường `double weight = 5;` vào message `SkillNode` trong `content.proto`.
  - Cập nhật [`ContentGrpcService.cs`](./V-Eval-Content_Service.API/Services/ContentGrpcService.cs) truyền trực tiếp trọng số `s.Weight ?? 0.05` cho từng kỹ năng sang Practice Service phục vụ thuật toán Path Planning.
- **Kiểm Thử Biên Dịch**:
  - Solution `V-Eval-Content_Service.sln` biên dịch sạch 100% (**0 Warning, 0 Error**).

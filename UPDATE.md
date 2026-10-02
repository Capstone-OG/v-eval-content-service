# Nhật Ký Cập Nhật (Update Log) - Content Service

## [01/10/2026] - Nâng Cấp Core Flow 2 (Bước 2): Bổ Sung DomainCode Vào gRPC Protocol Buffer & RPC GetSkillsTree

- **Hợp Đồng gRPC Protocol Buffer ([`content.proto`](./V-Eval-Content_Service.API/Protos/content.proto))**:
  - Bổ sung trường `string domain_code = 8;` vào message `SkillNode`.
- **Hiện Thực gRPC Server ([`ContentGrpcService.cs`](./V-Eval-Content_Service.API/Services/ContentGrpcService.cs))**:
  - Ánh xạ mã miền năng lực chuẩn (`DOM_LANG`, `DOM_MATH`, `DOM_NAT_SCI`, `DOM_SOC_SCI`) trực tiếp từ `DomainId` và tên miền của từng kỹ năng thuộc đồ thị DAG.
  - Cung cấp dữ liệu chuẩn mực để Practice Service phân cụm K-Means và phân nhóm chặng học lộ trình (Stages).
- **Kiểm Thử Toàn Diện & Biên Dịch Solution**:
  - Solution `V-Eval-Content_Service.sln` biên dịch sạch 100% (**0 Warning, 0 Error**).

# Nhật Ký Cập Nhật (Update Log) - Content Service

## [28/09/2026] - Khởi Tạo Thực Thể SkillPrerequisites, Seeding DAG 12 Kỹ Năng & RPC GetSkillsTree (Core Flow 2 - Giai Đoạn 1)

- **Thực Thể & CSDL SkillPrerequisites (Đồ Thị Tiên Quyết DAG)**:
  - Bổ sung entity [`SkillPrerequisite.cs`](./V-Eval-Content_Service.Domain/Entities/SkillPrerequisite.cs) với khóa chính phức hợp `(skill_id, prerequisite_id)` đại diện cho cung có hướng trong đồ thị DAG.
  - Cập nhật [`Skill.cs`](./V-Eval-Content_Service.Domain/Entities/Skill.cs) bổ sung 2 navigation collections `Prerequisites` (kỹ năng tiên quyết) và `DependentSkills` (kỹ năng phụ thuộc).
  - Cấu hình Fluent API trong [`ContentDbContext.cs`](./V-Eval-Content_Service.Infrastructure/Persistence/ContentDbContext.cs) và cập nhật [`IContentDbContext.cs`](./V-Eval-Content_Service.Application/Common/Interfaces/IContentDbContext.cs).
- **Bộ Dữ Liệu Chuẩn & Seeding Tự Động (`SkillPrerequisiteSeeder.cs`)**:
  - Triển khai seeder [`SkillPrerequisiteSeeder.cs`](./V-Eval-Content_Service.Infrastructure/Persistence/Seeds/SkillPrerequisiteSeeder.cs) tự động xác thực và khởi tạo 4 Miền Năng Lực, 12 Kỹ Năng Chuẩn ĐGNL ĐHQG-HCM kèm trọng số và 9 cặp quan hệ DAG không chu trình.
  - Kích hoạt seeder tự động trong [`Program.cs`](./V-Eval-Content_Service.API/Program.cs) khi khởi động service.
- **Triển Khai RPC GetSkillsTree (`ContentGrpcService.cs`)**:
  - Hiện thực phương thức gRPC `GetSkillsTree` trong [`ContentGrpcService.cs`](./V-Eval-Content_Service.API/Services/ContentGrpcService.cs) theo hợp đồng `content.proto`, cho phép Practice Service truy xuất toàn bộ cây kỹ năng và danh sách `prerequisite_ids` phục vụ thuật toán Path Planning.
- **Kiểm Thử Biên Dịch & Vận Hành**:
  - Solution `V-Eval-Content_Service.sln` biên dịch sạch 100% (**0 Warning, 0 Error**).
  - Đồng bộ 12 kỹ năng và 9 cung DAG trực tiếp lên CSDL PostgreSQL Supabase thành công.
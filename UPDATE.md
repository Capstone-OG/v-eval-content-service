# Nhật Ký Cập Nhật (Update Log) - Content Service

## [22/10/2026] - Khắc Phục Lỗi 404 Khi Xóa Chương Tổng Hợp (Synthetic Chapter Container) & Hỗ Trợ Đa Hình Chuỗi Node ID

- **Khắc Phục Lỗi HTTP 404 Khi Xóa Chương Tổng Hợp (`DELETE /api/content/curriculum/nodes/{id}`)**:
  - Chuyển đổi route constraint tại [`CurriculumController.cs`](./V-Eval-Content_Service.API/Controllers/CurriculumController.cs) từ `[HttpDelete("nodes/{id:guid}")]` sang `[HttpDelete("nodes/{id}")]` với tham số `[FromRoute] string id`, cho phép tiếp nhận cả Guid chuẩn và ID tổng hợp ảo (`chap-general-...`).
  - Mở rộng xử lý tại [`DeleteNodeCommand.cs`](./V-Eval-Content_Service.Application/Curriculum/Commands/DeleteNode/DeleteNodeCommand.cs):
    - Tự động nhận diện prefix `chap-general-`: Bóc tách `domainId` và cascade delete toàn bộ các kỹ năng phẳng (flat skills) thuộc miền tri thức (`s.DomainId == domainId && s.ParentId == null`), đồng thời dọn dẹp sạch tài liệu (`Materials`), ngân hàng câu hỏi (`Questions`) và quan hệ đồ thị (`SkillPrerequisites`) trong CSDL PostgreSQL.
    - Với các ID Guid hợp lệ: Thực hiện xóa đệ quy Node (Chương / Bài học / Dạng bài) và toàn bộ các nhánh con trực thuộc.
- **Biên Dịch & Kiểm Thử Vận Hành**:
  - Solution `V-Eval-Content_Service.sln` biên dịch sạch 100% (**0 Warning, 0 Error** trên .NET 9 SDK).
  - Tái khởi động dịch vụ trên cổng 5249/5250, kiểm tra lệnh xóa chương tổng hợp hoạt động trơn tru.

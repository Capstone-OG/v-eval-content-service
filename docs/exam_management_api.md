# Tài liệu Kỹ thuật Quản lý Đề thi & Nội dung (Content Service - Exam Management)

Tài liệu này đặc tả kiến trúc, các API endpoints và quy trình lưu trữ, truy xuất đề thi ĐGNL V-ACT trong phân hệ `V-Eval-Content_Service`.

---

## 1. Vai trò & Kiến trúc

`V-Eval-Content_Service` là microservice trung tâm chịu trách nhiệm quản lý toàn bộ nội dung học tập, ngân hàng câu hỏi, đề thi thử (Mock Exams), chùm bài đọc hiểu (Passages) và kỹ năng (Skills).

### Công nghệ sử dụng:
* **Framework**: .NET 9 Web API (Minimal APIs).
* **Kiến trúc**: Clean Architecture + CQRS với MediatR.
* **Cơ sở dữ liệu**: PostgreSQL (Supabase) với schema `content`.
* **ORM**: Entity Framework Core 9.

```mermaid
graph LR
    AIEngine[AI Engine (Port 5104)] -->|POST /import| CS[Content Service (Port 5249)]
    UI[Web Viewer / Client App] -->|GET /exams, GET /exams/:id, DELETE /exams/:id| CS
    CS -->|EF Core Npgsql| DB[(Supabase PostgreSQL)]
```

---

## 2. Danh sách Endpoints Quản lý Đề thi

### 2.1. Nhập Đề thi từ AI Engine (`POST /api/content/exams/import`)
* **Mục đích**: Nhận dữ liệu đề thi đã được AI bóc tách (gồm passages, single questions, options, LaTeX, suggested skill) và lưu vào cơ sở dữ liệu.
* **Request Body (JSON)**:
  ```json
  {
    "title": "Đề thi mẫu ĐGNL ĐHQG-HCM 2024",
    "durationMinutes": 150,
    "examCategory": "FULL_MOCK",
    "subjectCode": "MATH",
    "difficultyLevel": 2,
    "passages": [
      {
        "startQuestion": 61,
        "endQuestion": 63,
        "content": "Biểu đồ bên dưới thể hiện tỷ lệ phần trăm chi phí...",
        "imageUrl": null,
        "questions": [
          {
            "questionNumber": 61,
            "pageNumber": 6,
            "content": "Tổng chi của công ty gấp bao nhiêu lần so với chi cho Nghiên cứu?",
            "options": {
              "A": "27.",
              "B": "20.",
              "C": "18.",
              "D": "8."
            },
            "correctOption": "B",
            "explanation": "",
            "difficultyLevel": 2,
            "suggestedSkillName": "Phân tích số liệu"
          }
        ]
      }
    ],
    "singleQuestions": [
      {
        "questionNumber": 1,
        "pageNumber": 1,
        "content": "Tìm tập xác định của hàm số $y = \\log_2(x^2 - 4x + 3)$.",
        "options": {
          "A": "$(-\\infty; 1) \\cup (3; +\\infty)$",
          "B": "$(1; 3)$",
          "C": "$[1; 3]$",
          "D": "$(-\\infty; 1] \\cup [3; +\\infty)$"
        },
        "correctOption": "A",
        "explanation": "",
        "difficultyLevel": 2,
        "suggestedSkillName": "Hàm số mũ và logarit"
      }
    ]
  }
  ```
* **Response (200 OK)**:
  ```json
  {
    "exam_id": "0191837f-35b8-7c85-8a21-729486c0e25b",
    "total_questions": 120,
    "total_passages": 17,
    "message": "Đã nhập đề thi thành công vào hệ thống!"
  }
  ```

---

### 2.2. Lấy Danh sách Đề thi (`GET /api/content/exams`)
* **Mục đích**: Trả về danh sách tóm tắt các đề thi đã lưu trong Database phục vụ cho giao diện chọn đề.
* **Response (200 OK)**:
  ```json
  [
    {
      "examId": "0191837f-35b8-7c85-8a21-729486c0e25b",
      "title": "Đề thi mẫu ĐGNL ĐHQG-HCM 2024",
      "durationMinutes": 150,
      "subjectCode": "MATH",
      "totalQuestions": 120,
      "createdAt": "2026-09-04T15:17:49Z"
    }
  ]
  ```

---

### 2.3. Lấy Chi tiết Đề thi (`GET /api/content/exams/{id}`)
* **Mục đích**: Trả về toàn bộ cấu trúc câu hỏi, chùm bài đọc hiểu, lựa chọn và LaTeX của đề thi để nạp vào KaTeX Viewer hoặc Client làm bài thi.
* **Response (200 OK)**: Khớp với cấu trúc `ParsedExamDto` phục vụ việc hiển thị trực tiếp.

---

### 2.4. Xóa Đề thi (`DELETE /api/content/exams/{id}`)
* **Mục đích**: Xóa đề thi khỏi Database, tự động xóa liên đới (cascade) toàn bộ chùm bài đọc, câu hỏi và lựa chọn thuộc về đề thi đó.
* **Response (200 OK)**:
  ```json
  {
    "message": "Đã xóa đề thi thành công khỏi cơ sở dữ liệu."
  }
  ```

---

## 3. Cấu hình CORS
Dịch vụ được cấu hình mở CORS cho phép các origin client kết nối tự do trong môi trường Development:
* `http://localhost:5104` (AI Engine Web Viewer)
* `http://localhost:3000` / `http://localhost:5173` (Frontend Web)
* Các ứng dụng Mobile Flutter kết nối qua Gateway / LAN IP.

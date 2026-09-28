# ARCHITECTURE ACCEPTANCE REPORT - CONTENT SERVICE

## 1. SERVICE OVERVIEW
- **Service Name**: V-Eval Content Service (Assessment Bank & Exam Engine).
- **Service Port**: `5249` (HTTP) / Container `v_eval_content_service`.
- **Architectural Style**: Clean Architecture with MediatR CQRS, Result Pattern, and gRPC Server.

## 2. CORE RESPONSIBILITIES & MULTI-SCHEMA INTEGRATION
- **Assessment Schema (`content`)**: Owns `mock_exams`, `exam_questions`, `passages`, `questions`, and `skills`.
- **Core Flow 1 (Diagnostic Assessment Baseline)**:
  - Serves 30-question diagnostic baseline exam via `GET /api/v1/content/diagnostic-test`, with `excludeExamId` query parameter support for randomized retakes when previous sessions expire (Unhappy Case 2).
  - Enforces strict anti-cheating by stripping correct options (`CorrectOption`) and detailed explanations (`Explanation`) on client responses.
- **Inter-service gRPC (`content.proto`)**:
  - Implements `GetExamAnswerKey` RPC for secure server-to-server grading by `Practice_Service`.
  - Implements `GetSkillsTree` RPC delivering the full competency skill tree and prerequisite dependencies (`prerequisite_ids`) for Path Planning.
- **Core Flow 2 (Competency DAG & Skill Prerequisites)**:
  - Supports DAG-based competency modeling with `SkillPrerequisites` table.
  - Seeds the 12 standard VNU-HCM competency skills across 4 domains with test weights and 9 directed acyclic prerequisite relationships.

## 3. RELIABILITY & ERROR HANDLING STANDARDS
- **Result Pattern (`Result<T>`, `Error`, `ErrorType`)**: Replaces raw exceptions with explicit functional domain results.
- **Validation Pipeline**: FluentValidation integrated via MediatR `ValidationBehavior` to intercept invalid payloads.
- **Unified Base Controller (`ApiControllerBase`)**: Standardizes HTTP status codes and RFC 7807 ProblemDetails format.
- **Global Exception Middleware**: Catches unhandled runtime errors returning structured JSON.

## 4. ACCEPTANCE & VERIFICATION RESULTS
- **Compilation**: Clean build (`dotnet build`) with 0 warnings, 0 errors.
- **Seeding Verification**: Automated seeder generates diagnostic 30-question mock exam, seeds 12 standard skills and 9 DAG prerequisite edges on startup.
- **API & gRPC Functional Tests**: Verified `GET /api/v1/content/diagnostic-test`, `GetExamAnswerKey`, and `GetSkillsTree`.
- **Swagger Documentation**: Interactive OpenAPI / Swagger UI ready at `http://localhost:5249/swagger`.


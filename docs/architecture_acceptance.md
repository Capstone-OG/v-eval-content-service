# ARCHITECTURE ACCEPTANCE REPORT - CONTENT SERVICE

## 1. SERVICE OVERVIEW
- **Service Name**: V-Eval Content Service (Assessment Bank & Exam Engine).
- **Service Port**: `5249` (HTTP) / Container `v_eval_content_service`.
- **Architectural Style**: Clean Architecture with MediatR CQRS, Result Pattern, and gRPC Server.

## 2. CORE RESPONSIBILITIES & MULTI-SCHEMA INTEGRATION
- **Assessment Schema (`content`)**: Owns `mock_exams`, `exam_questions`, `passages`, `questions`, and `skills`.
- **Core Flow 1 (Diagnostic Assessment Baseline)**:
  - Serves 30-question diagnostic baseline exam via `GET /api/v1/content/diagnostic-test`.
  - Enforces strict anti-cheating by stripping correct options (`CorrectOption`) and detailed explanations (`Explanation`) on client responses.
- **Inter-service gRPC (`content.proto`)**:
  - Implements `GetExamAnswerKey` RPC for secure server-to-server grading by `Practice_Service`.

## 3. RELIABILITY & ERROR HANDLING STANDARDS
- **Result Pattern (`Result<T>`, `Error`, `ErrorType`)**: Replaces raw exceptions with explicit functional domain results.
- **Validation Pipeline**: FluentValidation integrated via MediatR `ValidationBehavior` to intercept invalid payloads.
- **Unified Base Controller (`ApiControllerBase`)**: Standardizes HTTP status codes and RFC 7807 ProblemDetails format.
- **Global Exception Middleware**: Catches unhandled runtime errors returning structured JSON.

## 4. ACCEPTANCE & VERIFICATION RESULTS
- **Compilation**: Clean build (`dotnet build`) with 0 warnings, 0 errors.
- **Seeding Verification**: Automated seeder generates diagnostic 30-question mock exam on startup.
- **API Functional Tests**: Verified `GET /api/v1/content/diagnostic-test` and 404 error formatting.
- **Exam Publish Endpoint**: Verified `PATCH /api/v1/content/exams/{id}/publish` updating `is_published = true`.
- **Cognitive Taxonomy Standardization**: Domain constants class `BloomTaxonomy.cs` mapping 6 Revised Bloom levels.
- **Vietnam Timezone Standardization**: Role timezone `Asia/Ho_Chi_Minh` and computed `createdAtVn` property returning formatted `dd/MM/yyyy HH:mm:ss`.
- **Swagger Documentation**: Interactive OpenAPI / Swagger UI ready at `http://localhost:5249/swagger`.


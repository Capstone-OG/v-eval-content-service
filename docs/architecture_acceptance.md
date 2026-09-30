# ARCHITECTURE ACCEPTANCE REPORT - V-EVAL CONTENT SERVICE

## 1. SERVICE OVERVIEW
- **Service Name**: V-Eval Content Service (Curriculum Graph, Question Bank & Learning Materials).
- **Service Ports**: Port `5249` (REST HTTP/1) + Port `5250` (gRPC HTTP/2).
- **Architectural Paradigm**: Clean Architecture with CQRS pattern (MediatR), FluentValidation, Result Pattern, EF Core targeting Supabase PostgreSQL schema `v_eval_content`.

---

## 2. SYSTEM ARCHITECTURE & INTER-SERVICE COMMUNICATION
- **gRPC Server ([`content.proto`](../V-Eval-Content_Service.API/Protos/content.proto))**:
  - `GetExamAnswerKey`: Server-to-server validation providing official tamper-proof answer keys for grading diagnostic submissions and milestone quizzes.
  - `GetSkillsTree`: Returns the full Directed Acyclic Graph (DAG) of 12 standard V-ACT competencies, prerequisite relationships (`prerequisite_ids`), test weights, domain mappings (`domain_id`, `domain_name`), and standardized domain identifiers (`domain_code`).
  - `GetQuestionsByDifficulty`: Supplies randomized question sets filtered by competency and difficulty level.

---

## 3. THEMATIC COHORT ARCHITECTURE SUPPORT (CORE FLOW 2 UPGRADE - STEP 2)
- **Domain Identification Contract**:
  - Added `string domain_code = 8;` to `SkillNode` message in `content.proto`.
  - Mapped canonical domain codes in `ContentGrpcService.cs`:
    - `DOM_LANG`: Vietnamese and English language arts (Domain `b581ee4c-7277-4be8-a156-c5b20a0a59f6`).
    - `DOM_MATH`: Mathematics, logical deduction, and quantitative data analysis (Domain `6f3765db-943e-4810-bf74-d6a8bdc215da`).
    - `DOM_NAT_SCI`: Natural sciences comprising Physics, Chemistry, Biology (Domain `77777777-7777-7777-7777-000000000001`).
    - `DOM_SOC_SCI`: Social sciences comprising History, Geography (Domain `77777777-7777-7777-7777-000000000002`).
- **Downstream Enabling**: Allows Practice Service to run K-Means cluster attribution and group roadmap nodes by domain cleanly without hardcoded heuristics.
- **Verification**: Clean compilation (**0 Warning, 0 Error**).

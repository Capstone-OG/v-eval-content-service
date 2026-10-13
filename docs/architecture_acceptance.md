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

---

## 4. QUESTION BANK ARCHITECTURE & ADAPTIVE QUIZ PACKAGING ACCEPTANCE (CORE FLOW 2 - APIS 16 - 19)

### 4.1 Architectural Rationale & Inter-Service Contract
- **Inter-Service Synergy with Practice Service**:
  - In `Practice Service`, the constrained homogeneous micro-clustering algorithm (`ClassMicroClusterer`) partitions offline cohorts (up to 20 students) into tables of 3 to 5 students based on:
    - **Macro Domain Features** (`double[4] DomainFeatures`): Aggregating competency across `[Lang, Math, NatSci, SocSci]` using `domain_code` exported by Content Service's `GetSkillsTree`.
    - **Micro Skill Profiles** (`Dictionary<Guid, double> SkillScores`): Identifying exact knowledge deficiencies where `` `MasteryScore < 0.60` ``.
  - The clustering engine produces a set of common weakness skill identifiers (`CommonWeakSkillIds`).
  - To support dynamic offline coaching without requiring instructors to manually build custom worksheets, **Content Service API 19 (`POST /api/content/exams/quiz`)** serves as the automated quiz packaging engine. It consumes these `CommonWeakSkillIds` and packages adaptive 5-10 question worksheets directly matching Bloom's Taxonomy.
  - The resulting `worksheetId` and `worksheetTitle` are fed directly into Practice Service's `POST /api/practice/classes/{classId}/micro-groups/{groupId}/assign-worksheet`.

### 4.2 Single Source of Truth for Assessment Items
- `Content Service` remains the authoritative owner of all question items, LaTeX formulations, Bloom taxonomies, response options, and pedagogical step-by-step solutions:
  - Database table `content.questions`: Holds `content_latex`, `difficulty` (IRT scale -3.0 to +3.0), `bloom_level` (Levels 1 to 6), `explanation`, and `skill_id`.
  - Database table `content.question_options`: Holds normalized 4 options per question with `is_correct` flag.
  - Database table `content.mock_exams`: Packages assessment units (Diagnostic exams, Milestone tests, and Micro-group Remedial Worksheets).

### 4.3 Detailed Specification of Core Flow 2 Question Bank APIs

#### 1. API 16: Create Question Bank Item (`POST /api/content/questions`)
- **Purpose**: Enables curriculum managers and instructors to register standardized V-ACT questions directly into the central question bank.
- **Input Parameters (`CreateQuestionRequestDto`)**:
  - `SkillId` (`Guid`): Target competency node verified against the 12-skill DAG.
  - `ContentLatex` (`string`): Question stem with preserved LaTeX syntax (escaped symbols such as `\frac`, `\sqrt`, chemical formulas).
  - `BloomLevel` (`int`): Revised Bloom's Taxonomy cognitive level (1: Remember, 2: Understand, 3: Apply, 4: Analyze, 5: Evaluate, 6: Create).
  - `Difficulty` (`double`): Calibrated difficulty parameter on the continuous IRT scale (default `0.0`, range `[-3.0, +3.0]`).
  - `Options` (`List<CreateQuestionOptionDto>`): Exactly 4 choice items, strictly enforcing exactly one option where `IsCorrect == true`.
  - `ExplanationLatex` (`string`): Comprehensive pedagogical derivation and solution guide.
- **Validation Constraints**: `FluentValidation` validates non-empty LaTeX strings, valid Bloom range [1, 6], single correct option, and skill existence in `content.skills`.
- **Response**: `201 Created` with `QuestionDetailDto` and `Location` header.

#### 2. API 17: Update Question Item (`PUT /api/content/questions/{id}`)
- **Purpose**: Modifies existing stems, LaTeX formulas, answer keys, Bloom levels, or solution steps when pedagogical corrections or updates are required.
- **Input Parameters (`UpdateQuestionRequestDto`)**:
  - `ContentLatex`, `BloomLevel`, `Difficulty`, `Options`, `ExplanationLatex`.
- **Validation Constraints**: Ensures target question exists (`404 Not Found`), preserves option integrity, validates single correct choice.
- **Response**: `200 OK` with updated `QuestionDetailDto`.

#### 3. API 18: Soft-Delete / Deactivate Question (`DELETE /api/content/questions/{id}`)
- **Purpose**: Safely retires flawed or outdated questions from active test generation pools while preserving referential integrity for historical student test attempts.
- **Behavior**: Marks `is_active = false` (or performs cascade removal if unreferenced by historical exam records).
- **Response**: `204 No Content`.

#### 4. API 19: Dynamic Adaptive Quiz / Worksheet Packaging (`POST /api/content/exams/quiz`)
- **Purpose**: Dynamically constructs an adaptive remedial worksheet tailored specifically to address common weakness skills identified during class micro-grouping or milestone evaluations.
- **Input Parameters (`CreateQuizExamRequestDto`)**:
  - `Title` (`string`, optional): Custom title or auto-generated default (e.g., `"Phiếu Luyện Tập Vi Mô - Củng Cố Kiến Thức [SkillName]"`).
  - `SkillIds` (`List<Guid>`): One or more targeted skill IDs (supplied from `CommonWeakSkillIds`).
  - `QuestionCount` (`int`, optional, default 5, range [3, 10]): Number of targeted items to package.
  - `MaxBloomLevel` (`int`, optional, default 3): Upper bound for cognitive complexity (predominantly Bloom 1-3: Remember, Understand, Apply for remediation).
- **Selection Strategy**:
  - Filters active questions matching `SkillIds`.
  - Prioritizes items within the targeted Bloom range and difficulty bracket.
  - Creates a new `MockExam` entry with exam type `QUIZ` / `WORKSHEET`.
  - Links selected questions via `exam_questions` preserving item order.
- **Response**: `201 Created` returning `QuizExamResponseDto`:
  - `ExamId` (`Guid`): Serves directly as `AssignedWorksheetId` for Practice Service.
  - `Title` (`string`): Serves as `AssignedWorksheetTitle`.
  - `TotalQuestions` (`int`): Count of packaged questions.
  - `Questions` (`List<QuestionItemDto>`): Anti-cheat sanitized items (omitting correct flags during active test taking, or full items with explanations for teacher review).

### 4.4 Dual Ingestion Paradigm: Batch PDF Import vs. Master Question Bank CRUD & Adaptive Quiz Packaging
To avoid architectural ambiguity, the system establishes a clear dual ingestion paradigm:
1. **Batch PDF Import Flow (`AI Engine -> POST /api/content/exams/import`)**:
   - **Nature**: High-volume, macro ingestion of standardized static assessment papers (e.g., official 120-question V-ACT exams or 30-question diagnostic tests) extracted from raw PDF documents using Gemini AI Vision OCR.
   - **Limitation**: Monolithic and static; cannot be edited atomically without granular CRUD endpoints, and cannot dynamically adapt to real-time classroom deficiency patterns.
2. **Master Question Bank CRUD Flow (`Content Service APIs 16 - 18`)**:
   - **Human-in-the-Loop Governance**:
     - *Correction & Calibration (API 17)*: Instructors review questions and correct OCR inaccuracies, formula rendering errors, or pedagogical explanations via `PUT /api/content/questions/{id}` without re-parsing the original PDF.
     - *Exemplar Item Authoring (API 16)*: Academic coordinators register isolated, high-quality assessment items or benchmark questions directly through web forms via `POST /api/content/questions`.
     - *Flaw Retirement (API 18)*: Outdated or erroneous question items are safely eliminated via `DELETE /api/content/questions/{id}` while maintaining exam integrity.
3. **Adaptive Quiz Packaging Flow (`Content Service API 19`)**:
   - **Dynamic Remediation**: Consumes deficiency identifiers (`CommonWeakSkillIds`) generated by Practice Service's K-Means clustering engine, extracts 5 to 10 calibrated Bloom Level 1-3 items, and packages on-the-fly remedial worksheets tailored for 3-5 student study tables.

---

## 5. ARCHITECTURAL ACCEPTANCE STATUS
- **Design Alignment**: 100% aligned with Clean Architecture, CQRS (MediatR), Result Pattern, and RFC 7807 ProblemDetails error handling.
- **Inter-Service Handshake**: Formalized integration contract between Content Service (APIs 16-19) and Practice Service (Micro-group Clustering & Worksheet Assignment).
- **Implementation Status**: 100% Completed on feature branch `ThinhTT/feature-question-bank-adaptive-quiz` with 0 Warnings and 0 Errors.

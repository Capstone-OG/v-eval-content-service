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

---

## 6. CURRICULUM TREE & KNOWLEDGE ONTOLOGY API (REST ENDPOINT)
- **Endpoint**: `GET /api/content/curriculum` (`CurriculumController.cs`).
- **Architectural Rationale**: Exposes the complete 4-domain competency taxonomy and 12 standard V-ACT competencies directly from Content Service database to the Web Client through YARP Gateway, completely eliminating any hardcoded curriculum structures on the frontend.
- **Data Encapsulation**: Delivers domain metadata, competency weights, Bloom cognitive levels, KaTeX formulas, Cross-Disciplinary GraphRAG models, prerequisite relationships, sample questions, and video lecture references dynamically.

---

## 7. 3-LEVEL CURRICULUM HIERARCHY & 4 PEDAGOGICAL LAYERS ACCEPTANCE
- **Architectural Motivation**: Grounding the digital learning roadmap in realistic K12 curriculum standards (Ministry of Education GDPT 2018) by moving beyond flat skill lists to a hierarchical structure: `Domain` -> `Chapter` -> `Lesson` -> `Pattern`.
- **Database Self-Referencing Topology**:
  - Leverages `Skills.ParentId` self-referencing foreign key to represent arbitrarily deep nesting:
    - Level 1: Chapters (`ParentId = null`, e.g., Chapter 1: Derivative Applications).
    - Level 2: Lessons (`ParentId = Chapter.SkillId`, e.g., Lesson 1: Monotonicity).
    - Level 3: Patterns (`ParentId = Lesson.SkillId`, e.g., Pattern 1.1: Basic Monotonicity, Pattern 1.2: Parameterized Monotonicity).
- **Four Pedagogical Layers per Pattern**:
  - *Layer 1 (Foundational Knowledge & Prerequisites)*: Extracted from `Materials.Content` storing standard textbook definitions, theorems, and linking Directed Acyclic Graph (DAG) dependencies (`SkillPrerequisites`).
  - *Layer 2 (Step-by-step Solution Methodology & KaTeX Formulas)*: Preserves algorithmic derivation steps (Step 1 -> Step 2 -> Step 3) and key mathematical formulas.
  - *Layer 3 (Video Lecture & Step-by-step Sample Solution)*: Integrates lecture metadata (`VideoUrl`, `DurationSeconds`) and calibrated bank questions with step-by-step explanations.
  - *Layer 4 (Adaptive Quiz with Automatic Adjacent Weak Skill Bundling)*: Automatically inspects the student's mastery profile. When launching a quiz on a target pattern, any adjacent prerequisite node with mastery `` `P(L_t) <= 10%` `` is bundled into the API 19 request payload to eliminate knowledge deficiencies at their root.

---

## 8. DUAL-CHANNEL DYNAMIC INGESTION & INSTRUCTOR CURATION SUITE ACCEPTANCE
- **Anti-Hardcoding Paradigm**: Completely purged static C# seeders and switch-cases. The knowledge ontology is 100% dynamically stored and retrieved from PostgreSQL.
- **Dual-Channel Ingestion Pipeline**:
  - *Channel 1 (AI Engine OCR)*: Parses PDF textbooks into structured JSON trees and ingests via `POST /api/content/curriculum/import`.
  - *Channel 2 (Academic & Instructor Authorship)*: Academic managers manually register or curate curriculum trees directly.
- **Full-Fledged Instructor Curation REST Suite**:
  - `PUT /api/content/curriculum/patterns/{id}`: Granular atomic updates across all 4 pedagogical layers (Name, Weight, TheoryRecap, SolutionMethod, VideoUrl, Sample Question).
  - `PUT /api/content/curriculum/nodes/{id}`: Updates Chapter/Lesson titles, weights, and descriptions.
  - `POST /api/content/curriculum/nodes`: Dynamically adds new Chapters, Lessons, or Patterns without server restarts.
  - `DELETE /api/content/curriculum/nodes/{id}`: Safely cascades node deletion with clean prerequisite DAG cleanup.
- **Live Pedagogical Feedback**: Web client integrates instant KaTeX math rendering previews, video embed previews, and AI method templating to maximize instructor productivity.

---

## 9. UNIVERSAL KNOWLEDGE INGESTION PIPELINE & RESILIENT DESERIALIZATION ACCEPTANCE
- **Problem Statement**:
  - AI document extraction (Gemini Vision / OCR) and arbitrary user import payloads emit diverse, non-standard representations for multiple-choice answer keys (e.g., `"A."`, `"(B)"`, `"[C]"`, `"Option D"`, `"Đáp án C"`, 0-indexed integers `0..3`, 1-indexed integers `1..4`, or empty strings).
  - Default .NET `System.Text.Json` converts strings of length != 1 into `System.Char` by throwing a `JsonException`, invalidating model binding and returning HTTP 400 with `"The dto field is required."`
- **Architectural Solution**:
  - Implemented `FlexibleCharJsonConverter` and `FlexibleNullableCharJsonConverter` in `V_Eval_Content_Service.Application.Common.Converters`:
    - Safely unrolls stripped single characters, keyword-bound prefixes (`Đáp án`, `Phương án`, `Option`, `Answer`), ordinal integers, and Unicode characters without throwing exceptions.
    - Globally registered across all controllers via `builder.Services.AddControllers().AddJsonOptions(...)` and decorated on core DTOs (`ImportQuestionDto`, `CreateQuestionRequestDto`, `UpdateQuestionRequestDto`, `QuestionDto`).
  - Added multi-layered server-side defensive sanitization in `ImportCurriculumCommandHandler.cs` (`safeCorrect = char.ToUpperInvariant(qDto.CorrectOption)` clamped to `'A'..'D'`).
## 10. SYNTHETIC CHAPTER CASCADE DELETION & POLYMORPHIC NODE ID ARCHITECTURE
- **Problem Statement**:
  - The curriculum tree hierarchy query (`GetCurriculumTreeQuery.cs`) creates virtual synthetic container chapters (`chap-general-{domainId}`) to logically house flat skills that lack an explicit parent chapter.
  - When the frontend administrator invokes chapter deletion via `DELETE /api/content/curriculum/nodes/{id}`, the endpoint route previously enforced a Guid regex constraint (`[HttpDelete("nodes/{id:guid}")]`), rejecting synthetic IDs with HTTP 404 Not Found.
- **Architectural Solution**:
  - **Polymorphic Route Signature**: Decoupled the route constraint to `[HttpDelete("nodes/{id}")]` accepting string identifiers.
  - **Command Handler Polymorphism (`DeleteNodeCommandHandler.cs`)**:
    - Dispatches prefix-based pattern matching on `chap-general-`: Parses the underlying `domainId` and executes an atomic cascade deletion across all root flat skills within the competency domain (`s.DomainId == domainId && s.ParentId == null`), purging associated materials, questions, and DAG graph dependencies.
    - Preserves standard recursive Guid deletion for standard database nodes (Chapters, Lessons, Skill Patterns).
- **Zero-Downtime Verification**:
  - Verified against PostgreSQL Supabase test instances; returned 200 OK with clean database state.

---

## 11. [09/11/2026] HIGH-THROUGHPUT PAYLOAD STREAMING (250 MB KESTREL & FORM-OPTIONS)
- **Problem Statement**:
  - Deep-learning OCR output, multi-chapter textbook curricula ingestion payloads, and comprehensive mock exam suites can easily exceed ASP.NET Core Kestrel's 30MB default limit, causing connection drops or HTTP 413.
- **Architectural Solution**:
  - Enforced `KestrelServerLimits.MaxRequestBodySize = 262_144_000` (250 MB) in `Program.cs`.
  - Configured `FormOptions.MultipartBodyLengthLimit = 262_144_000` (250 MB) and `FormOptions.ValueLengthLimit = int.MaxValue`.
- **Verification**:
  - Clean compilation (`0 Warning(s), 0 Error(s)`). High-throughput payloads flow unhindered across Content Service.

---

## 12. [10/11/2026] CASCADE COMPETENCY DOMAIN DELETION & CANONICAL DOMAIN PRESERVATION ARCHITECTURE
- **Architectural Rationale**:
  - **Unintended Domain Sprawl**: During automated AI curriculum extraction, if an LLM hallucinates an unexpected domain or the user imports exploratory content, rogue competency domains (e.g. "Quan hệ song song trong không gian") could be created in the database.
  - Previous `DeleteNodeCommand` only searched `_context.Skills`, rejecting domain-level deletion requests and leaving rogue domains permanently in the database.
  - Furthermore, `ImportCurriculumCommand` previously allowed AI imports to overwrite existing domain names with whatever title the AI generated.
- **Architectural Solution**:
  - **Polymorphic Domain Deletion (`DeleteNodeCommand.cs`)**:
    - Extends `DeleteNodeCommand` to identify domain identifiers prefixed with `domain-` (e.g. `domain-{domainId}`) or valid domain GUIDs directly.
    - Queries `_context.CompetencyDomains` and executes an atomic cascading delete:
      1. Resolves all child skills belonging to the domain.
      2. Cascades deletion to DAG prerequisites (`SkillPrerequisites`), learning materials (`Materials`), and questions (`Questions`).
      3. Removes all child skill entities (`Skills`).
      4. Removes the `CompetencyDomain` entity itself from schema `content`.
  - **Canonical Domain Protection (`ImportCurriculumCommand.cs`)**:
    - In `ImportCurriculumCommandHandler`, preserves existing domain names. If `existingDomain` is found, its title is preserved unless the domain name is currently empty or bears the temporary default placeholder `"Miền Năng Lực"`.
- **Verification**:
  - Clean compilation (`0 Warning(s), 0 Error(s)`) under .NET 9.0 SDK.

---

## 13. [11/11/2026] MULTI-NETWORK KESTREL BINDING & PRODUCTION DOCKER DEPLOYMENT ARCHITECTURE
- **Architectural Rationale**:
  - Previously, Kestrel in Content Service enforced `options.ListenLocalhost(5249)` and `options.ListenLocalhost(5250)`. While operational on the host machine, loopback binding in Docker containers rejects any incoming TCP traffic arriving from external bridges (`veval_network`) or Gateway proxies.
- **Architectural Solution**:
  - Replaced loopback listeners with `options.ListenAnyIP(5249)` (HTTP/1 REST) and `options.ListenAnyIP(5250)` (HTTP/2 gRPC).
  - Preserved 250 MB `MaxRequestBodySize` and `FormOptions` limits.
  - Aligned Dockerfile `EXPOSE 5249 5250` and `ENV ASPNETCORE_URLS=http://+:5249;http://+:5250`.
  - Configured Docker Compose service `v_eval_content_service` ports `5249:5249` and `5250:5250` connected to bridge `capstone_veval_network`.
- **Verification & Live Acceptance**:
  - Compilation: 0 Warning, 0 Error on .NET 9 SDK.
  - Docker Compose deployment: Container running in `Up` state.
  - Public Cloudflare Gateway verification: `GET https://api.pickic.io.vn/api/content/curriculum` returns 200 OK with complete multi-domain curriculum ontology.

  - 3/3 automated unit tests passing 100%.

---

## 14. [18/11/2026] FULL DYNAMIC DATABASE CURRICULUM TREE & STANDARDIZED GDPT 2018 ONTOLOGY
- **Architectural Rationale**:
  - The curriculum management interface requires a genuine 3-tier pedagogical hierarchy (`Chapter -> Lesson -> Pattern`) operating on 100% real database records without artificial hardcoding or synthetic mock containers.
  - Previous database artifacts contained an artificial domain `"Tổng hợp"` (`6080a490-b21b-41dd-9ef6-e92545c827e4`) and unattached flat skills that created clutter and confusion.
- **Architectural Implementation**:
  - **Database Migration & Domain Integrity**:
    - Reassigned all spatial geometry skills (Chapter 01: Quan Hệ Song Song) to `Toán học & Tư duy định lượng` (`6f3765db-943e-4810-bf74-d6a8bdc215da`).
    - Reassigned Chemistry skills to `Khoa học tự nhiên` and History/Geography skills to `Khoa học xã hội`.
    - Cleanly deleted the redundant domain `"Tổng hợp"` from `v_eval_content."CompetencyDomains"`.
  - **Dynamic Tree Query Refinement (`GetCurriculumTreeQuery.cs`)**:
    - Synthetic container `"Khung Năng Lực Chuẩn"` is now restricted to initialize *only* if the domain contains zero formal chapters (`chapters.Count == 0 && flatSkills.Count > 0`). When formal chapters exist, the query cleanly returns the official hierarchical structure.
  - **Standardized GDPT 2018 Knowledge Ontology**:
    - Seeded deterministic, production-grade curriculum nodes across all 4 official domains:
      - *Mathematics & Quantitative Reasoning*: 6 Chapters, 10 Lessons, 16 Patterns, 64 Questions.
      - *Natural Sciences*: 4 Chapters, 6 Lessons, 13 Patterns, 33 Questions.
      - *Language Arts (Vietnamese & English)*: 3 Chapters, 3 Lessons, 3 Patterns, 12 Questions.
      - *Social Sciences (History, Geography & Civic Law)*: 3 Chapters, 3 Lessons, 3 Patterns, 12 Questions.
    - Total: 16 Chapters, 22 Lessons, 35 Patterns, 121 Questions equipped with SGK foundations, KaTeX algorithmic solutions, embedded JSXGraph tags, and 4-tier Bloom cognitive levels.
- **Verification**:
  - Database schema consistency verified via `psql` queries on Supabase pooler.
  - Solution compiles cleanly with 0 Errors on .NET 9 SDK.

---

## 15. [19/11/2026] FULL EXPANSION & SYNCHRONIZATION OF GDPT 2018 OFFICIAL TEXTBOOK ONTOLOGY (139 CHAPTERS)
- **Architectural Rationale**:
  - To faithfully reflect the Vietnamese National Curriculum (GDPT 2018 - "Chân trời sáng tạo" textbook edition as verified against `giaokhoaonline.com`), curriculum nodes accurately cover every formal Chapter and Lesson across Grades 10, 11, and 12 across all 4 official competency domains.
- **Architectural Implementation**:
  - **Full 17-Textbook Index Ingestion**:
    - *Mathematics (Grades 10, 11, 12)*: 26 chapters (80 lessons) covering Algebra, Trigonometry, Analytic Geometry Oxy/Oxyz, Calculus, Probability, and V-ACT Formal Logic & Data Interpretation.
    - *Natural Sciences (Grades 10, 11, 12)*: 50 chapters (127 lessons) covering Physics (17 chapters, 61 lessons), Chemistry (21 chapters, 57 lessons), and Biology (12 chapters, 39 lessons).
    - *Languages (Grades 10, 11, 12)*: 33 chapters (66 lessons) covering Vietnamese Literature genre-based lessons (27 chapters, 54 lessons) and High School English (6 chapters, 18 lessons).
    - *Social Sciences (Grades 10, 11, 12)*: 30 chapters (64 lessons) covering History (9 chapters), Geography (12 chapters), and Economic & Legal Education (9 chapters).
  - **Natural Pedagogical Sorting in CQRS Query Engine ([`GetCurriculumTreeQuery.cs`](../V-Eval-Content_Service.Application/Curriculum/Queries/GetCurriculumTree/GetCurriculumTreeQuery.cs))**:
    - Added `ExtractGrade`, `ExtractChapterNumber`, and `ExtractLessonNumber` regex parsers.
    - Resolves natural ordering by Grade (`Grade 10 -> Grade 11 -> Grade 12`) and numeric chapter order (`Chapter 1 -> Chapter 10`), eliminating lexical sorting anomalies.
  - **Database Verification in Supabase PostgreSQL (`v_eval_content`)**:
    - **139 Chapters** (Root Nodes)
    - **337 Lessons** (Sub-Nodes)
    - **337 Pedagogical Patterns**
    - **343 Learning Materials** (KaTeX Formulas & Detailed Pedagogical Theory)
    - **588 Assessment Questions** mapped to Bloom Levels L1 to L4.
- **Verification**:
  - Clean .NET 9 compilation: 0 Errors, 0 Warnings.
  - Endpoint `GET /api/content/curriculum` verified supplying 100% dynamic entities.

---

## 16. [19/11/2026] ELIMINATION OF GENERIC TEMPLATES & MASTER PEDAGOGICAL KNOWLEDGE BASE ACCEPTANCE
- **Architectural Rationale**:
  - Eliminates all placeholder or generic template text (such as derivative/integral formulas `` `\int x^n dx` `` or discriminant `` `\Delta = b^2 - 4ac` `` occurring generically in early Grade 10 lessons like Propositions, Sets, Vectors, or generic chemistry equations).
  - Guarantees that every lesson across all 9 disciplines carries 100% authentic, scientifically precise KaTeX formulas, step-by-step algorithms, and contextualized V-ACT assessment questions.
- **Architectural Implementation**:
  - **Master Pedagogical Knowledge Base (`pedagogical_knowledge_base.py`)**:
    - Centralized pedagogical knowledge engine covering 9 disciplines (Mathematics, Physics, Chemistry, Biology, Literature, English, History, Geography, Civic Education).
    - Intelligent domain-aware resolver `get_pedagogical_pattern(...)` matching each lesson by grade, title, and topic summary.
    - Zero generic math templates in Grade 10 lessons; verified via automated test scripts and database queries.
  - **PostgreSQL Database Synchronization (`v_eval_content`)**:
    - Re-ingested all 139 Chapters and 337 Lessons with zero generic formulas.
    - All 343 Learning Materials and 588 Questions reflect true textbook pedagogy.
- **Verification**:
  - Tested across all 337 lessons: 0 misplaced derivative/integral formulas in Grade 10 Math.
  - Supabase PostgreSQL pooler verification: Queries confirm authentic content for Propositions, Sets, Vectors, Newton's Laws, Atomic Structures, and Le Chatelier Equilibria.
  - Solution compiles cleanly: 0 Errors, 0 Warnings.

---

## 17. [19/11/2026] REMOVAL OF HARDCODED PLACEHOLDER STRINGS & STRICT MISSING SOURCE DATA SIGNALING
- **Architectural Rationale**:
  - The curriculum delivery pipeline must enforce a strict zero-hardcode policy. When learning materials or solution methods are absent in the database, the backend must not emit artificial mock sentences (e.g. `"Kiến thức nền tảng trong cơ sở dữ liệu."` or `"Phương pháp giải chi tiết trong cơ sở dữ liệu."`).
  - Emitting empty or null fields allows the client presentation layer to deterministically display explicit, localized missing-source notices (`"Chưa có dữ liệu nguồn lý thuyết"`, `"Chưa có dữ liệu nguồn phương pháp giải"`).
- **Architectural Implementation**:
  - Refactored `BuildPatternFromEntity` in [`GetCurriculumTreeQuery.cs`](../V-Eval-Content_Service.Application/Curriculum/Queries/GetCurriculumTree/GetCurriculumTreeQuery.cs):
    - Initialized `theory` and `method` as empty strings `""` when `mat == null` or `string.IsNullOrWhiteSpace(mat.Content)`.
    - Removed fallback strings from flat skills mapping.
- **Verification**:
  - Verified clean .NET 9 compilation: 0 Errors.
  - API responses now cleanly omit placeholder sentences, enabling explicit missing-data UI fallbacks.

---

## 18. [19/11/2026] POLYMORPHIC PATTERN UPDATE ROUTE & AUTOMATIC SKILL UPSERT FOR STUDIO AUTHORING
- **Architectural Rationale**:
  - When instructors open the Pedagogical Studio from the Textbook Ingestion or Standard Textbook views (`TextbookStudioView`), patterns may carry composite textbook identifiers (e.g., `"SGK_CHEM_G10_C1_L2_P1"`) or deterministic IDs not yet persisted in `v_eval_content."Skills"`.
  - Previously, `[HttpPut("patterns/{id:guid}")]` rejected non-GUID identifiers at the ASP.NET Core routing layer with HTTP 404, and `UpdatePatternCommandHandler` returned `Error.NotFound` when the GUID did not yet exist in `Skills`.
- **Architectural Implementation**:
  - **Polymorphic Controller Route ([`CurriculumController.cs`](../V-Eval-Content_Service.API/Controllers/CurriculumController.cs))**:
    - Relaxed route constraint from `[HttpPut("patterns/{id:guid}")]` to `[HttpPut("patterns/{id}")]` accepting string identifiers.
  - **Deterministic GUID Hashing & Auto-Upsert ([`UpdatePatternCommand.cs`](../V-Eval-Content_Service.Application/Curriculum/Commands/UpdatePattern/UpdatePatternCommand.cs))**:
    - Added `ParseOrCreateDeterministicGuid` to parse standard GUIDs or compute a deterministic MD5-based GUID for textbook pattern keys.
    - When `SkillId` is not found in `_context.Skills`, the handler first checks for a case-insensitive name match (`dto.Name`) and, if still absent, automatically provisions a new `Skill` record alongside its `Material` and sample `Question`.
- **Verification**:
  - Clean .NET 9 compilation (`0 Error(s)`).

---

## 19. [20/11/2026] DIRECT CURRICULUM NODE BINDING FOR IMPORTED EXAMS & 4-LEVEL ANCESTOR LINEAGE HYDRATION
- **Architectural Rationale**:
  - Previously, importing extracted exam questions via `POST /api/content/exams/import` matched `SuggestedSkillName` using only flat name equality and created orphan root-level `Skill` entities (`ParentId = null`) outside the 4-level GDPT 2018 Curriculum Tree (`CompetencyDomain -> Chapter -> Lesson -> Pattern`).
  - Furthermore, `GET /api/content/exams/{id}` returned only `SuggestedSkillName` without resolving the question's owning Domain, Chapter, Lesson, and Pattern hierarchy.
- **Architectural Implementation**:
  - **Curriculum-Aware Import Binding ([`ImportMockExamCommand.cs`](../V-Eval-Content_Service.Application/MockExams/Commands/ImportMockExam/ImportMockExamCommand.cs), [`ImportMockExamCommandHandler.cs`](../V-Eval-Content_Service.Application/MockExams/Commands/ImportMockExam/ImportMockExamCommandHandler.cs))**:
    - Extended `QuestionDto` with nullable `SkillId`, `DomainName`, `ChapterName`, and `LessonName`.
    - Pre-loaded `Skills` and `CompetencyDomains` once per transaction (eliminating N+1 queries on 120-question V-ACT exams) and implemented 3-tier resolution (`ResolveOrCreateSkillIdAsync`): exact `SkillId` node binding first, hierarchical tree-node (`ParentId != null`) name matching second, and safe fallback creation last.
  - **4-Level Ancestor Traversal ([`MockExamDetailDto.cs`](../V-Eval-Content_Service.Application/MockExams/Queries/GetMockExamById/MockExamDetailDto.cs), [`GetMockExamByIdQueryHandler.cs`](../V-Eval-Content_Service.Application/MockExams/Queries/GetMockExamById/GetMockExamByIdQueryHandler.cs))**:
    - Included `Skill.Domain` and two levels of `Skill.Parent` to project `skill_id`, `domain_id`, `domain_name`, `chapter_id`, `chapter_name`, `lesson_id`, `lesson_name`, `pattern_id`, and `pattern_name` onto every `ExamQuestionItemDto`.
  - **Foreign-Key-Safe Exam Deletion ([`DeleteMockExamCommandHandler.cs`](../V-Eval-Content_Service.Application/MockExams/Commands/DeleteMockExam/DeleteMockExamCommandHandler.cs))**:
    - Queries `ExamQuestions` across other exams to exclude shared `QuestionId`s before removing exclusive questions and orphan passages, preventing PostgreSQL FK constraint errors (`23503`) during `DELETE /api/content/exams/{id}`.
- **Verification**:
  - `dotnet build --no-restore`: Succeeded with `0 Error(s)`.
  - `dotnet test --no-build`: `3/3` tests passed.










---

### [20/11/2026] - Full `content.proto` Hierarchy & Passage Sync, 3 Balanced GDPT 2018 Diagnostic Suites & Read-Only PLAR Question Delivery
- **Canonical `content.proto` & Passage Context Delivery**: Unified `V-Eval-Content_Service`, `V-Eval-Practice_Service`, and `System-Repo/grpc/content.proto` with hierarchical `SkillNode` fields (`parent_id`, `level`, `chapter_name`, `lesson_name`, `subject_name`, `material_id`, `video_url`, `question_count`), full `QuestionAnswerKey` fields `9..17`, domain/subject metadata on `GetQuestionDetailResponse`, `read_only` mode on `GetMilestoneQuizRequest`, and `passage_title = 8`, `passage_content = 9`, `passage_image_url = 10` on `QuizQuestionItem`. Added `.Include(q => q.Passage)` across all 5 `GetMilestoneQuiz` queries plus `IsBrokenOrphanPassageQuestion` guard to filter out orphan passage-referencing questions lacking a linked `Passage`.
- **Balanced 30-Question GDPT 2018 Diagnostic Seeder (`DiagnosticExamSeeder.cs`)**: Seeds 3 balanced, non-overlapping 30-question diagnostic suites sourced strictly from real curriculum pattern nodes (`ParentId != null`) covering all 4 domains and all 9 subjects (`Ngữ văn 5`, `Tiếng Anh 5`, `Toán học 8`, `Vật lí 2`, `Hóa học 2`, `Sinh học 2`, `Lịch sử 2`, `Địa lí 2`, `GDKT & Pháp luật 2`) across Bloom levels; safely demotes legacy `11111111-...` to `DIAGNOSTIC_LEGACY` while preserving historical submissions.
---

## 20. [21/11/2026] SUBJECT BOUNDARY ISOLATION ARCHITECTURE & PLAR CROSS-SUBJECT LEAKAGE PREVENTION
- **Architectural Rationale**:
  - In adaptive learning journeys (PLAR Phase A: Apply), a student practicing a designated mathematics milestone (e.g., `Grade 10 Mathematics > Chapter 9: Analytic Geometry > Lesson 2: Lines in the Plane`) must never receive questions from other subjects (such as Vietnamese Literature proverbs or English Grammar).
  - Previous `GetMilestoneQuiz` implementations contained two severe architectural leaks:
    1. If `parsedExamId` pointed to a multi-subject diagnostic exam, all questions from that exam (including question 1 literature proverbs) were ingested into the candidate pool without filtering by subject.
    2. Fallback step 2d queried questions simply by matching `q.Skill.DomainId == targetSkill.DomainId` (mixing English and Vietnamese Literature within `DOM_LANG`, or Physics, Chemistry, and Biology within `DOM_NAT_SCI`), and fallback step 3 indiscriminately selected any question from `_context.Questions` without subject boundary checks.
- **Architectural Solution & Implementation**:
  - **Subject Taxonomy Helper (`SubjectTaxonomyHelper.cs`)**:
    - Centralized domain code mapping (`ResolveDomainCode`) and standardized 9-10 subject taxonomy heuristics (`InferSubjectName`) into `V_Eval_Content_Service.Application.Common.Helpers.SubjectTaxonomyHelper` covering all GDPT 2018 subjects (`Toán học`, `Ngữ văn`, `Tiếng Anh`, `Vật lí`, `Hóa học`, `Sinh học`, `Lịch sử`, `Địa lí`, `GDKT & Pháp luật`, `Tư duy Logic & Số liệu`).
  - **Strict Subject Boundary Isolation in `GetMilestoneQuiz` ([`ContentGrpcService.cs`](../V-Eval-Content_Service.API/Services/ContentGrpcService.cs))**:
    - When `parsedSkillId` is specified, the service determines `targetSubject` and pre-computes `sameSubjectSkillIds` across the curriculum tree.
    - If `parsedExamId` is provided, questions from `ExamQuestions` are **strictly filtered** to admit only those belonging to `sameSubjectSkillIds`, eliminating cross-subject contamination.
    - Question retrieval strictly adheres to a 3-tier hierarchical ladder:
      - *Tier 1 (Exact Skill & Descendants)*: Target pattern and direct child/grandchild skills.
      - *Tier 2 (Same Chapter)*: Sibling patterns within the same chapter and subject.
      - *Tier 3 (Same Subject)*: Sibling chapters and patterns across the exact same subject.
    - **Completely removed** generic domain-level and table-wide fallbacks. If the question bank contains `N < count` questions for that subject, it cleanly returns `N` subject-pure questions rather than contaminating the pool with unrelated subjects.
- **Verification & Acceptance**:
  - Automated unit tests in `ContentGrpcServiceSubjectIsolationTests.cs` verify strict isolation between Mathematics, Literature, English, Chemistry, and other disciplines.
  - `dotnet build --no-restore`: 0 Warnings, 0 Errors.
  - `dotnet test --no-restore`: **16/16 Passed 100%**.


---

### [22/11/2026] - Chapter-Scoped Precision & Content Language Mismatch Guard in `GetMilestoneQuiz`
- **Target Files**: `SubjectTaxonomyHelper.cs`, `ContentGrpcService.cs`, `ContentGrpcServiceSubjectIsolationTests.cs`.
- **Key Deliverables**:
  1. Added `SubjectTaxonomyHelper.IsContentLanguageMismatch` to filter out misclassified pure-English items stored under non-English skills (e.g., Chemistry).
  2. Restricted same-subject expansion (`Step 1d`) to trigger only when `questionsList.Count == 0`, preventing cross-chapter pollution (e.g., geometry questions leaking into combinatorics milestones).
  3. **Verification**: `dotnet test --no-restore` passed **18/18** unit tests.

## [24/11/2026] Curriculum Query Optimization & gRPC Message Ceiling Upgrade
- Configured `QuerySplittingBehavior.SplitQuery` and `EnableRetryOnFailure(3, 3s)` on `ContentDbContext` to prevent Cartesian Explosion and transient pooler 500 errors on `/api/content/curriculum`.
- Introduced `IMemoryCache` with `SemaphoreSlim` lock in `CurriculumController` (5-minute TTL, auto-evicted on curriculum mutations) and raised gRPC server message limits to 32 MB.

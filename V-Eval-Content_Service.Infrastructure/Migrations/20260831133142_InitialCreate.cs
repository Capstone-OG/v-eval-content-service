using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V_Eval_Content_Service.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.CreateTable(
                name: "CompetencyDomains",
                schema: "content",
                columns: table => new
                {
                    domain_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetencyDomains", x => x.domain_id);
                });

            migrationBuilder.CreateTable(
                name: "MockExams",
                schema: "content",
                columns: table => new
                {
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "text", nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    total_questions = table.Column<int>(type: "integer", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    exam_category = table.Column<string>(type: "text", nullable: false, defaultValue: "FULL_MOCK"),
                    subject_code = table.Column<string>(type: "text", nullable: true),
                    difficulty_level = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockExams", x => x.exam_id);
                });

            migrationBuilder.CreateTable(
                name: "Passages",
                schema: "content",
                columns: table => new
                {
                    passage_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "text", nullable: true),
                    content = table.Column<string>(type: "text", nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passages", x => x.passage_id);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                schema: "content",
                columns: table => new
                {
                    skill_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    domain_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    weight = table.Column<double>(type: "double precision", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.skill_id);
                    table.ForeignKey(
                        name: "FK_Skills_CompetencyDomains_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "content",
                        principalTable: "CompetencyDomains",
                        principalColumn: "domain_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Skills_Skills_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "content",
                        principalTable: "Skills",
                        principalColumn: "skill_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Materials",
                schema: "content",
                columns: table => new
                {
                    material_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    skill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    video_url = table.Column<string>(type: "text", nullable: true),
                    file_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.material_id);
                    table.ForeignKey(
                        name: "FK_Materials_Skills_skill_id",
                        column: x => x.skill_id,
                        principalSchema: "content",
                        principalTable: "Skills",
                        principalColumn: "skill_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                schema: "content",
                columns: table => new
                {
                    question_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    skill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    passage_id = table.Column<Guid>(type: "uuid", nullable: true),
                    difficulty_level = table.Column<int>(type: "integer", nullable: false),
                    content_latex = table.Column<string>(type: "text", nullable: false),
                    option_a = table.Column<string>(type: "text", nullable: false),
                    option_b = table.Column<string>(type: "text", nullable: false),
                    option_c = table.Column<string>(type: "text", nullable: false),
                    option_d = table.Column<string>(type: "text", nullable: false),
                    correct_option = table.Column<char>(type: "character(1)", nullable: false),
                    explanation = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.question_id);
                    table.ForeignKey(
                        name: "FK_Questions_Passages_passage_id",
                        column: x => x.passage_id,
                        principalSchema: "content",
                        principalTable: "Passages",
                        principalColumn: "passage_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Questions_Skills_skill_id",
                        column: x => x.skill_id,
                        principalSchema: "content",
                        principalTable: "Skills",
                        principalColumn: "skill_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamQuestions",
                schema: "content",
                columns: table => new
                {
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamQuestions", x => new { x.exam_id, x.question_id });
                    table.ForeignKey(
                        name: "FK_ExamQuestions_MockExams_exam_id",
                        column: x => x.exam_id,
                        principalSchema: "content",
                        principalTable: "MockExams",
                        principalColumn: "exam_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamQuestions_Questions_question_id",
                        column: x => x.question_id,
                        principalSchema: "content",
                        principalTable: "Questions",
                        principalColumn: "question_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestions_question_id",
                schema: "content",
                table: "ExamQuestions",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_skill_id",
                schema: "content",
                table: "Materials",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_passage_id",
                schema: "content",
                table: "Questions",
                column: "passage_id");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_skill_id",
                schema: "content",
                table: "Questions",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_domain_id",
                schema: "content",
                table: "Skills",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_parent_id",
                schema: "content",
                table: "Skills",
                column: "parent_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamQuestions",
                schema: "content");

            migrationBuilder.DropTable(
                name: "Materials",
                schema: "content");

            migrationBuilder.DropTable(
                name: "MockExams",
                schema: "content");

            migrationBuilder.DropTable(
                name: "Questions",
                schema: "content");

            migrationBuilder.DropTable(
                name: "Passages",
                schema: "content");

            migrationBuilder.DropTable(
                name: "Skills",
                schema: "content");

            migrationBuilder.DropTable(
                name: "CompetencyDomains",
                schema: "content");
        }
    }
}

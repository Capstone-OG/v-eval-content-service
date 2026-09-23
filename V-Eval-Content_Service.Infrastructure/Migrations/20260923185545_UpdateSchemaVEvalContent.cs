using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V_Eval_Content_Service.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaVEvalContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "v_eval_content");

            migrationBuilder.RenameTable(
                name: "Skills",
                schema: "content",
                newName: "Skills",
                newSchema: "v_eval_content");

            migrationBuilder.RenameTable(
                name: "Questions",
                schema: "content",
                newName: "Questions",
                newSchema: "v_eval_content");

            migrationBuilder.RenameTable(
                name: "Passages",
                schema: "content",
                newName: "Passages",
                newSchema: "v_eval_content");

            migrationBuilder.RenameTable(
                name: "MockExams",
                schema: "content",
                newName: "MockExams",
                newSchema: "v_eval_content");

            migrationBuilder.RenameTable(
                name: "Materials",
                schema: "content",
                newName: "Materials",
                newSchema: "v_eval_content");

            migrationBuilder.RenameTable(
                name: "ExamQuestions",
                schema: "content",
                newName: "ExamQuestions",
                newSchema: "v_eval_content");

            migrationBuilder.RenameTable(
                name: "CompetencyDomains",
                schema: "content",
                newName: "CompetencyDomains",
                newSchema: "v_eval_content");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.RenameTable(
                name: "Skills",
                schema: "v_eval_content",
                newName: "Skills",
                newSchema: "content");

            migrationBuilder.RenameTable(
                name: "Questions",
                schema: "v_eval_content",
                newName: "Questions",
                newSchema: "content");

            migrationBuilder.RenameTable(
                name: "Passages",
                schema: "v_eval_content",
                newName: "Passages",
                newSchema: "content");

            migrationBuilder.RenameTable(
                name: "MockExams",
                schema: "v_eval_content",
                newName: "MockExams",
                newSchema: "content");

            migrationBuilder.RenameTable(
                name: "Materials",
                schema: "v_eval_content",
                newName: "Materials",
                newSchema: "content");

            migrationBuilder.RenameTable(
                name: "ExamQuestions",
                schema: "v_eval_content",
                newName: "ExamQuestions",
                newSchema: "content");

            migrationBuilder.RenameTable(
                name: "CompetencyDomains",
                schema: "v_eval_content",
                newName: "CompetencyDomains",
                newSchema: "content");
        }
    }
}

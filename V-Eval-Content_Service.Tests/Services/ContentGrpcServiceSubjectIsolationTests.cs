using System;
using Xunit;
using V_Eval_Content_Service.Application.Common.Helpers;

namespace V_Eval_Content_Service.Tests.Services;

public class ContentGrpcServiceSubjectIsolationTests
{
    [Theory]
    [InlineData("Chương 9: Phương pháp toạ độ trong mặt phẳng (Toán - Lớp 10)", "Bài 2: Đường thẳng trong mặt phẳng toạ độ", "DOM_MATH", "Toán học")]
    [InlineData("Chương 1: Mệnh đề và tập hợp (Toán - Lớp 10)", "Bài 1: Mệnh đề toán học", "DOM_MATH", "Toán học")]
    [InlineData("Chương 1: Tiếng Việt thực hành (Ngữ văn - Lớp 10)", "Thành ngữ và tục ngữ dân gian", "DOM_LANG", "Ngữ văn")]
    [InlineData("Chương 3: Modal Verbs, Conditionals & Subjunctive (Tiếng Anh - Lớp 10)", "Past perfect subjunctive", "DOM_LANG", "Tiếng Anh")]
    [InlineData("Chương 5: Điện phân và pin điện (Hóa học - Lớp 12)", "Bán phản ứng anot và catot", "DOM_NAT_SCI", "Hóa học")]
    [InlineData("Chương 2: Dao động cơ (Vật lí - Lớp 12)", "Con lắc đơn", "DOM_NAT_SCI", "Vật lí")]
    [InlineData("Chương 4: Phong trào Cần Vương (Lịch sử - Lớp 11)", "Khởi nghĩa Hương Khê", "DOM_SOC_SCI", "Lịch sử")]
    [InlineData("Chương 1: Bản đồ và phương pháp bản đồ (Địa lí - Lớp 10)", "Hệ toạ độ địa lý", "DOM_SOC_SCI", "Địa lí")]
    public void InferSubjectName_ShouldCorrectlyIsolateSubjects(string chapter, string skill, string domainCode, string expectedSubject)
    {
        // Act
        var subject = SubjectTaxonomyHelper.InferSubjectName(chapter, skill, domainCode);

        // Assert
        Assert.Equal(expectedSubject, subject);
    }

    [Fact]
    public void SubjectIsolation_MathSkill_MustNeverMatchLiteratureOrEnglish()
    {
        // Arrange
        var mathChapter = "Chương 9: Phương pháp toạ độ trong mặt phẳng (Toán - Lớp 10)";
        var mathSkill = "Bài 2: Đường thẳng trong mặt phẳng toạ độ";

        var litChapter = "Chương 1: Tiếng Việt thực hành (Ngữ văn - Lớp 10)";
        var litSkill = "Chọn từ đúng điền vào câu tục ngữ";

        // Act
        var mathSubject = SubjectTaxonomyHelper.InferSubjectName(mathChapter, mathSkill, "DOM_MATH");
        var litSubject = SubjectTaxonomyHelper.InferSubjectName(litChapter, litSkill, "DOM_LANG");

        // Assert
        Assert.Equal("Toán học", mathSubject);
        Assert.Equal("Ngữ văn", litSubject);
        Assert.NotEqual(mathSubject, litSubject);
    }

    [Theory]
    [InlineData("6f3765db-943e-4810-bf74-d6a8bdc215da", "Toán học", "DOM_MATH")]
    [InlineData("b581ee4c-7277-4be8-a156-c5b20a0a59f6", "Ngôn ngữ", "DOM_LANG")]
    [InlineData("77777777-7777-7777-7777-000000000001", "Khoa học tự nhiên", "DOM_NAT_SCI")]
    [InlineData("77777777-7777-7777-7777-000000000002", "Khoa học xã hội", "DOM_SOC_SCI")]
    public void ResolveDomainCode_ShouldMapCorrectly(string domainGuid, string domainName, string expectedCode)
    {
        // Arrange
        var guid = Guid.Parse(domainGuid);

        // Act
        var code = SubjectTaxonomyHelper.ResolveDomainCode(guid, domainName);

        // Assert
        Assert.Equal(expectedCode, code);
    }
}

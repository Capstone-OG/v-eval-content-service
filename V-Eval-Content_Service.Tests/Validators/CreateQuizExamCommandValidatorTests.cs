using System;
using System.Collections.Generic;
using FluentValidation.TestHelper;
using Xunit;
using V_Eval_Content_Service.Application.MockExams.Commands.CreateQuizExam;

namespace V_Eval_Content_Service.Tests.Validators;

public class CreateQuizExamCommandValidatorTests
{
    private readonly CreateQuizExamCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyErrors()
    {
        // Arrange
        var command = new CreateQuizExamCommand
        {
            SkillIds = new List<Guid> { Guid.NewGuid() },
            QuestionCount = 10,
            MinBloomLevel = 1,
            MaxBloomLevel = 4
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenSkillIdsIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuizExamCommand
        {
            SkillIds = new List<Guid>(),
            QuestionCount = 10,
            MinBloomLevel = 1,
            MaxBloomLevel = 3
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SkillIds);
    }

    [Fact]
    public void Validate_WhenMaxBloomIsLessThanMinBloom_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuizExamCommand
        {
            SkillIds = new List<Guid> { Guid.NewGuid() },
            QuestionCount = 5,
            MinBloomLevel = 4,
            MaxBloomLevel = 2 // Sai logic: Max < Min
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxBloomLevel);
    }
}

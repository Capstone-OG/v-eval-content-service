using System;
using Microsoft.Extensions.Caching.Memory;

namespace V_Eval_Content_Service.API.Caching;

/// <summary>
/// Centralized IMemoryCache keys & invalidation helpers for static/semi-static Content catalogs
/// (prevents repeated full-table Supabase egress on Skills, CompetencyDomains, Materials, and ExamAnswerKeys).
/// </summary>
public static class ContentCacheKeys
{
    public const string CurriculumTree = "content:curriculum:tree:v1";
    public const string SkillsTreeAll = "content:grpc:skills-tree:all:v1";
    public const string SkillsTreeLearningNodes = "content:grpc:skills-tree:learning:v1";
    public const string SkillsLookup = "content:grpc:skills-lookup:v1";

    public static string ExamAnswerKey(Guid examId) => $"content:grpc:exam-answer-key:{examId:N}:v1";

    public static void InvalidateCatalog(IMemoryCache cache)
    {
        cache.Remove(CurriculumTree);
        cache.Remove(SkillsTreeAll);
        cache.Remove(SkillsTreeLearningNodes);
        cache.Remove(SkillsLookup);
    }
}

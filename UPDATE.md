# [24/11/2026]

- Fixed intermittent HTTP 500 on `GET /api/content/curriculum` caused by 3-way Cartesian Explosion across 1,800+ skills: enabled `QuerySplittingBehavior.SplitQuery` and `EnableRetryOnFailure(3, 3s)` on `ContentDbContext` in `Program.cs`.
- Added thread-safe 5-minute `IMemoryCache` (`SemaphoreSlim` stampede guard) in `CurriculumController.cs` with automatic cache invalidation on `ImportCurriculum`, `UpdatePattern`, `UpdateNode`, `CreateNode`, and `DeleteNode`.
- Raised gRPC server `MaxSendMessageSize` and `MaxReceiveMessageSize` to `32 MB` so full skill-tree gRPC responses never hit the 4 MB default ceiling.
- Verified with `dotnet build` (0 errors).

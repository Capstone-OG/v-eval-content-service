# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy gRPC definitions and project files
COPY ["grpc/", "grpc/"]
COPY ["All Services/V-Eval-Content_Service/V-Eval-Content_Service.API/V-Eval-Content_Service.API.csproj", "All Services/V-Eval-Content_Service/V-Eval-Content_Service.API/"]
COPY ["All Services/V-Eval-Content_Service/V-Eval-Content_Service.Application/V-Eval-Content_Service.Application.csproj", "All Services/V-Eval-Content_Service/V-Eval-Content_Service.Application/"]
COPY ["All Services/V-Eval-Content_Service/V-Eval-Content_Service.Domain/V-Eval-Content_Service.Domain.csproj", "All Services/V-Eval-Content_Service/V-Eval-Content_Service.Domain/"]
COPY ["All Services/V-Eval-Content_Service/V-Eval-Content_Service.Infrastructure/V-Eval-Content_Service.Infrastructure.csproj", "All Services/V-Eval-Content_Service/V-Eval-Content_Service.Infrastructure/"]
RUN dotnet restore "All Services/V-Eval-Content_Service/V-Eval-Content_Service.API/V-Eval-Content_Service.API.csproj"

# Copy full source and publish Release artifact
COPY ["All Services/V-Eval-Content_Service/", "All Services/V-Eval-Content_Service/"]
WORKDIR "/src/All Services/V-Eval-Content_Service/V-Eval-Content_Service.API"
RUN dotnet publish "V-Eval-Content_Service.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: ASP.NET Core Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 5249
ENV ASPNETCORE_URLS=http://+:5249
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "V-Eval-Content_Service.API.dll"]

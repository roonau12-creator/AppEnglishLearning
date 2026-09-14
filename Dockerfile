# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AppLearningEnglish/AppLearningEnglish.csproj AppLearningEnglish/
COPY AppLearningEnglish.Business/AppLearningEnglish.Business.csproj AppLearningEnglish.Business/
COPY AppLearningEnglish.DataAccess/AppLearningEnglish.DataAccess.csproj AppLearningEnglish.DataAccess/
COPY AppLearningEnglish.Models/AppLearningEnglish.Models.csproj AppLearningEnglish.Models/

RUN dotnet restore AppLearningEnglish/AppLearningEnglish.csproj

COPY . .
RUN dotnet publish AppLearningEnglish/AppLearningEnglish.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
# Pass secrets at runtime, e.g. -e ConnectionStrings__DefaultConnection=...

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AppLearningEnglish.dll"]

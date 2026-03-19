FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY LibraryMS.Core.Domain/LibraryMS.Core.Domain.csproj                               LibraryMS.Core.Domain/
COPY LibraryMS.Core.Application/LibraryMS.Core.Application.csproj                     LibraryMS.Core.Application/
COPY LibraryMS.Infrastructure.Identity/LibraryMS.Infrastructure.Identity.csproj       LibraryMS.Infrastructure.Identity/
COPY LibraryMS.Infrastructure.Persistence/LibraryMS.Infrastructure.Persistence.csproj LibraryMS.Infrastructure.Persistence/
COPY LibraryMS.Infrastructure.Shared/LibraryMS.Infrastructure.Shared.csproj           LibraryMS.Infrastructure.Shared/
COPY LibraryMS.WebApi/LibraryMS.WebApi.csproj                                          LibraryMS.WebApi/
RUN dotnet restore LibraryMS.WebApi/LibraryMS.WebApi.csproj
COPY . .
RUN dotnet publish LibraryMS.WebApi/LibraryMS.WebApi.csproj \
    -c Release \
    --no-restore \
    -o /app/publish


FROM base AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
    CMD wget --spider -q http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "LibraryMS.WebApi.dll"]
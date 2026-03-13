FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/NsTech.Api/NsTech.Api.csproj", "src/NsTech.Api/"]
COPY ["src/NsTech.Domain/NsTech.Domain.csproj", "src/NsTech.Domain/"]
COPY ["src/NsTech.Application/NsTech.Application.csproj", "src/NsTech.Application/"]
COPY ["src/NsTech.Infrastructure/NsTech.Infrastructure.csproj", "src/NsTech.Infrastructure/"]
RUN dotnet restore "src/NsTech.Api/NsTech.Api.csproj"
COPY . .
WORKDIR "/src/src/NsTech.Api"
RUN dotnet build "NsTech.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NsTech.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NsTech.Api.dll"]

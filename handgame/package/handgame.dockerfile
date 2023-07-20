FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app
COPY . ./
RUN dotnet publish HandGame.Api/HandGame.Api.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
LABEL author="Cvetomir Todorov"

WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "HandGame.Api.dll"]

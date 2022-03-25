#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /src
COPY ["UsosFix/UsosFix.csproj", "UsosFix/"]
RUN dotnet restore "UsosFix/UsosFix.csproj"
COPY . .
WORKDIR "/src/UsosFix"
RUN dotnet build "UsosFix.csproj" -c Release -o /app/build

FROM build AS test
WORKDIR /src
RUN dotnet test -c Release --logger:xunit --results-directory "/.TestResults"
ENTRYPOINT ["/bin/bash"]

FROM build AS publish
RUN dotnet publish "UsosFix.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UsosFix.dll"]
#FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
#WORKDIR /source
#COPY . .
#RUN dotnet restore "PRN231_Group11_Assignment1_API/PRN231_Group11_Assignment1_API.csproj" 
#RUN dotnet publish "PRN231_Group11_Assignment1_API/PRN231_Group11_Assignment1_API.csproj" -c Release -o /app --no-restore
#
#FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
#WORKDIR /app
#COPY --from=build /app ./
#EXPOSE 80
#ENTRYPOINT ["dotnet", "PRN231_Group11_Assignment1_API.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development



FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
ENV ASPNETCORE_ENVIRONMENT=Development
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PRN231_Group11_Assignment1_API/PRN231_Group11_Assignment1_API.csproj", "PRN231_Group11_Assignment1_API/"]
COPY ["PRN231_Group11_Assignment1_Repo/PRN231_Group11_Assignment1_Repo.csproj", "PRN231_Group11_Assignment1_Repo/"]
RUN dotnet restore "PRN231_Group11_Assignment1_API/PRN231_Group11_Assignment1_API.csproj"
COPY . .
WORKDIR "/src/PRN231_Group11_Assignment1_API"
RUN dotnet build "PRN231_Group11_Assignment1_API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "PRN231_Group11_Assignment1_API.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PRN231_Group11_Assignment1_API.dll"]

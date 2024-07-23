# Use the appropriate base image for Windows-based containers
FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-1809 AS build
WORKDIR /src
COPY ["ReviewRestApi.csproj", "."]
RUN dotnet restore "ReviewRestApi.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ReviewRestApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReviewRestApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReviewRestApi.dll"]

#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

#FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809 AS base
#WORKDIR /app
#EXPOSE 7070
#EXPOSE 7071
#
#FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-1809 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["ReviewRestApi/ReviewRestApi.csproj", "ReviewRestApi/"]
#RUN dotnet restore "./ReviewRestApi/./ReviewRestApi.csproj"
#COPY . .
#WORKDIR "/src/ReviewRestApi"
#RUN dotnet build "./ReviewRestApi.csproj" -c %BUILD_CONFIGURATION% -o /app/build
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./ReviewRestApi.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "ReviewRestApi.dll"]
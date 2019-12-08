FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
MAINTAINER Ivan Zivanovic <izivanovic@ipan.eu>
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Api/Api.csproj", "Api/"]
COPY ["Repository.FileSystem/Repository.FileSystem.csproj", "Repository.FileSystem/"]
COPY ["Repository/Repository.csproj", "Repository/"]
COPY ["Repository.AwsS3/Repository.AwsS3.csproj", "Repository.AwsS3/"]
RUN dotnet restore "Api/Api.csproj"
COPY . .
WORKDIR "/src/Api"
RUN dotnet build "Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV "ASPNETCORE_ENVIRONMENT"="Production"
ENV "FileRepository:Provider"="FileSystem"
ENV "FileRepository:Settings:AccessKeyId"=""
ENV "FileRepository:Settings:AccessSecretKey"=""
ENV "FileRepository:Settings:RegionSystemName"=""
ENV "FileRepository:Settings:BucketName"=""
VOLUME /app/data
ENTRYPOINT ["dotnet", "Api.dll"]
# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS base
USER $APP_UID
WORKDIR /app
EXPOSE 9443

# This stage is used to build the service project
ARG BUILDPLATFORM=linux/amd64
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG BUILD_CONFIGURATION=Release
ARG VERSION=0.0.1
WORKDIR /src
COPY ["Function.SDK.CSharp.Example/Function.SDK.CSharp.Example.csproj", "Function.SDK.CSharp.Example/"]
COPY ["Function.SDK.CSharp.SourceGenerator/Function.SDK.CSharp.SourceGenerator.csproj", "Function.SDK.CSharp.SourceGenerator/"]
COPY ["Function.SDK.CSharp/Function.SDK.CSharp.csproj", "Function.SDK.CSharp/"]
RUN dotnet restore "./Function.SDK.CSharp.Example/Function.SDK.CSharp.Example.csproj"
COPY . .
WORKDIR "/src/Function.SDK.CSharp.Example"
RUN dotnet build "./Function.SDK.CSharp.Example.csproj" -c $BUILD_CONFIGURATION --use-current-runtime -o /app/build /p:Version=${VERSION}

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG VERSION=0.0.1
RUN dotnet publish "./Function.SDK.CSharp.Example.csproj" -c $BUILD_CONFIGURATION --use-current-runtime -o /app/publish /p:UseAppHost=false /p:Version=${VERSION}

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final

WORKDIR /
COPY Function.SDK.CSharp.Example/package.yaml .

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Function.SDK.CSharp.Example.dll"]

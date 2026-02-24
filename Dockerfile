# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Hanet.sln .
COPY Hanet.SDK/Hanet.SDK.csproj ./Hanet.SDK/
COPY Hanet.WebAPI/Hanet.WebAPI.csproj ./Hanet.WebAPI/

# Restore dependencies
RUN dotnet restore Hanet.WebAPI/Hanet.WebAPI.csproj

# Copy all source code
COPY . .

# Build and publish
WORKDIR /src/Hanet.WebAPI
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 5000

# Run the app
ENTRYPOINT ["dotnet", "Hanet.WebAPI.dll"]

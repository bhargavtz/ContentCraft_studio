# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["ContentCraft_studio.sln", "./"]
COPY ["ContentCraft_studio.csproj", "./"]
RUN dotnet restore "ContentCraft_studio.csproj"

# Copy the remaining source code
COPY . .
RUN dotnet build "ContentCraft_studio.csproj" -c Release -o /app/build
RUN dotnet publish "ContentCraft_studio.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create directory for data protection keys
RUN mkdir -p /app/keys && chmod 777 /app/keys

# Copy application files
COPY --from=build /app/publish ./

# Set environment variables
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port 10000 (Render's expected port)
EXPOSE 10000

# Entry point
ENTRYPOINT ["dotnet", "ContentCraft_studio.dll"]

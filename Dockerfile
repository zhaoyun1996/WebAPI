# Use the official .NET Core SDK as a base image
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

# Set the working directory to /app
WORKDIR /app

# Copy the .csproj file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the remaining application files
COPY . ./

# Build the application
RUN dotnet publish -c Release -o out

# Use the official .NET Core runtime as a base image for the final stage
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

# Set the working directory to /app
WORKDIR /app

# Copy the built application from the build stage
COPY --from=build-env /app/out .

# Specify the entry point for the application
ENTRYPOINT ["dotnet", "WebApi.dll"]
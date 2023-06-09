# Use the .NET Core SDK image as the base
FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build

# Set the working directory
WORKDIR /app

# Copy the project files
COPY . .

# Build the project
RUN dotnet publish -c Release -o out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine

# Set the working directory
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/out .

# Expose the port
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "BugSearch.Api.dll"]

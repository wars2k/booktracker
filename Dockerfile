FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY Booktracker/*.csproj ./
RUN dotnet restore
COPY Booktracker/. ./
RUN dotnet build -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Install and configure web server (e.g., nginx)
RUN apt-get update && apt-get install -y nginx
COPY nginx.conf /etc/nginx/nginx.conf
COPY frontend /app/www
COPY Booktracker/init.sql ./

COPY external ./external


# Expose ports for web server and api
EXPOSE 80
EXPOSE 5000

# Start web server and MySQL server
CMD service nginx start && dotnet Booktracker.dll

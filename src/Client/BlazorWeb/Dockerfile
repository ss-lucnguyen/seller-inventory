# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG API_BASE_URL=http://localhost:5000
WORKDIR /src
COPY ["src/Client/BlazorWeb/BlazorWeb.csproj", "src/Client/BlazorWeb/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
COPY ["Directory.Build.props", "./"]
RUN dotnet restore "src/Client/BlazorWeb/BlazorWeb.csproj"
COPY . .
WORKDIR "/src/src/Client/BlazorWeb"

# Update API URL in appsettings.json before build
RUN sed -i "s|http://localhost:5000|${API_BASE_URL}|g" wwwroot/appsettings.json

RUN dotnet publish "BlazorWeb.csproj" -c Release -o /app/publish

# Runtime stage - nginx
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Remove default nginx content
RUN rm -rf ./*

COPY --from=build /app/publish/wwwroot .
COPY src/Client/BlazorWeb/nginx.conf.template /etc/nginx/nginx.conf.template

# Security: run as non-root
RUN chown -R nginx:nginx /usr/share/nginx/html && \
    chown -R nginx:nginx /var/cache/nginx && \
    chown -R nginx:nginx /var/log/nginx && \
    chown -R nginx:nginx /etc/nginx && \
    touch /var/run/nginx.pid && \
    chown -R nginx:nginx /var/run/nginx.pid

# Default port (Cloud Run will override via PORT env var)
ENV PORT=80

EXPOSE 80

# Use envsubst to replace PORT variable at runtime, then start nginx
CMD sh -c "envsubst '\$PORT' < /etc/nginx/nginx.conf.template > /etc/nginx/nginx.conf && nginx -g 'daemon off;'"

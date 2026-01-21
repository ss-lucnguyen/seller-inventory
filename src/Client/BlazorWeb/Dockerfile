FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Client/BlazorWeb/BlazorWeb.csproj", "src/Client/BlazorWeb/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
COPY ["Directory.Build.props", "./"]
RUN dotnet restore "src/Client/BlazorWeb/BlazorWeb.csproj"
COPY . .
WORKDIR "/src/src/Client/BlazorWeb"
RUN dotnet build "BlazorWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlazorWeb.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY src/Client/BlazorWeb/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80

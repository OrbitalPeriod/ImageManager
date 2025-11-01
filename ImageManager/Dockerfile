FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# Create the app directory and /Images as root first
WORKDIR /app
RUN mkdir -p /Images && chmod 777 /Images
# (optional but safer for controlled setups)
RUN chown -R $APP_UID:$APP_UID /Images

EXPOSE 8080
EXPOSE 8081

# Switch to non-root user after setting up writable directories
USER $APP_UID


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ImageManager.csproj", "ImageManager/"]
RUN dotnet restore "ImageManager/ImageManager.csproj"

WORKDIR "/src/ImageManager"
COPY . .
RUN dotnet build "./ImageManager.csproj" -c $BUILD_CONFIGURATION -o /app/build


FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ImageManager.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ImageManager.dll"]

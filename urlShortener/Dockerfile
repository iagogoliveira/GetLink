# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# O csproj vem primeiro para que o restore fique em cache e so refaca quando
# as dependencias mudarem, nao a cada alteracao de codigo.
COPY urlShortener.csproj .
RUN dotnet restore urlShortener.csproj

COPY . .
# Projeto nomeado explicitamente: a pasta tambem contem a solution e o projeto
# de teste, e sem alvo o publish tentaria processar todos.
RUN dotnet publish urlShortener.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

# APP_UID vem definido na imagem base: roda como usuario sem privilegios.
USER $APP_UID

EXPOSE 8080

ENTRYPOINT ["dotnet", "urlShortener.dll"]

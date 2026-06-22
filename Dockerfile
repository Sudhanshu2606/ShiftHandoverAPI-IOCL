FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ShiftHandoverAPI.csproj", "."]
RUN dotnet restore "./ShiftHandoverAPI.csproj"
COPY . .
RUN dotnet publish "ShiftHandoverAPI.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "ShiftHandoverAPI.dll"]
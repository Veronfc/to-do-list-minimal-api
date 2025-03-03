FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV CONN_STRING="User Id=postgres.teilpuqrwdvayjhjnvsg;Password=hwmK55c5cyIZSnYt;Server=aws-0-eu-central-1.pooler.supabase.com;Port=5432;Database=postgres"
ENV JWT_KEY="This!Is@A#SuperSecret%Key^That&Im*Using(To)Test_Jwt+Authentication"

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["TodoList/TodoList.csproj", "TodoList/"]
RUN dotnet restore "TodoList/TodoList.csproj"
COPY . .
WORKDIR "/src/TodoList"
RUN dotnet build "TodoList.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "TodoList.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoList.dll"]

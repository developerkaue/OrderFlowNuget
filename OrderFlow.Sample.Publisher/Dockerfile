FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore OrderFlow.Sample.Publisher/OrderFlow.Sample.Publisher.csproj
RUN dotnet publish OrderFlow.Sample.Publisher/OrderFlow.Sample.Publisher.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderFlow.Sample.Publisher.dll"]
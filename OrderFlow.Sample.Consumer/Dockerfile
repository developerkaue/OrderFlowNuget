FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore OrderFlow.Sample.Consumer/OrderFlow.Sample.Consumer.csproj
RUN dotnet publish OrderFlow.Sample.Consumer/OrderFlow.Sample.Consumer.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderFlow.Sample.Consumer.dll"]
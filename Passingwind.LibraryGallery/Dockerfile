FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Passingwind.LibraryGallery/Passingwind.LibraryGallery.csproj", "Passingwind.LibraryGallery/"]
COPY ["Passingwind.LibraryGallery.Core/Passingwind.LibraryGallery.Core.csproj", "Passingwind.LibraryGallery.Core/"]
RUN dotnet restore "Passingwind.LibraryGallery/Passingwind.LibraryGallery.csproj"
COPY . .
WORKDIR "/src/Passingwind.LibraryGallery"
RUN dotnet build "Passingwind.LibraryGallery.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Passingwind.LibraryGallery.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Passingwind.LibraryGallery.dll"]

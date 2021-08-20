FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base

# COPY ./app /app

WORKDIR /app

EXPOSE 80 

ENTRYPOINT ["dotnet", "Passingwind.LibraryGallery.dll"]

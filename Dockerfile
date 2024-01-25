# Sử dụng hình ảnh cơ bản của .NET Core SDK
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

# Sao chép toàn bộ giải pháp
COPY . .

# Xây dựng ứng dụng
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Sử dụng hình ảnh cơ bản của .NET Core runtime cho bước cuối cùng
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

# Sao chép ứng dụng đã xây dựng từ bước xây dựng
COPY --from=build-env /app/out .

# Chỉ định điểm nhập cho ứng dụng
ENTRYPOINT ["dotnet", "DemoWebAPI.dll"]

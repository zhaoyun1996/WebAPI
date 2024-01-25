# Sử dụng hình ảnh cơ bản của .NET Core SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Sao chép toàn bộ giải pháp
COPY . .

# Xây dựng ứng dụng
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Sao chép ứng dụng đã xây dựng từ bước xây dựng
COPY --from=build-env /out .

# Chỉ định điểm nhập cho ứng dụng
ENTRYPOINT ["dotnet", "DemoWebAPI.dll"]

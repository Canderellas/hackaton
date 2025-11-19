# Backend/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# ������������� ������� ����������
WORKDIR /src

# �������� ���� ������� � ��������
COPY Backend_hackaton.sln .
COPY Backend_hackaton/Backend_hackaton.csproj ./Backend_hackaton/

# ��������������� �����������
RUN dotnet restore "Backend_hackaton.sln"

# �������� ���� �������� ���
COPY Backend_hackaton/ ./Backend_hackaton/

# �������� ����������
RUN dotnet build "Backend_hackaton.sln" -c Release --no-restore -o /app/build

# ��������� ����������
RUN dotnet publish "Backend_hackaton.sln" -c Release --no-restore --no-build -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# ������������� ������� ����������
WORKDIR /app

# �������� ��������� ���������� �� ������ build
COPY --from=build /app/publish .

# ��������� ����
EXPOSE 5000

# ��������� ����������
ENTRYPOINT ["dotnet", "Backend_hackaton.dll"]
# �ϥ� .NET SDK �@����¦�M���i��غc
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# �ƻs .csproj ���٭�̿ඵ
COPY *.csproj ./
RUN dotnet restore

# �ƻs�Ѿl�������ɮרëغc
COPY . ./
RUN dotnet publish -c Release -o out

# �ϥ� ASP.NET Core �B��ɬM���@����¦�M��
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# �ƻs�غc������B�������
COPY --from=build /app/out .

# �]�m�����ܼƬ��}�o�Ҧ�
ENV ASPNETCORE_ENVIRONMENT Development

# �}�����ε{�ǹB��һݪ��ݤf
EXPOSE 443
EXPOSE 80

# �]�m�ҰʩR�O
ENTRYPOINT ["dotnet", "RestaurantQueue.dll"]
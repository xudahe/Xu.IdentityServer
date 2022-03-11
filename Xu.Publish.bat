color B

del  .publishFiles\*.*   /s /q

dotnet build

cd Xu.IdentityServer

dotnet publish -o ..\Xu.IdentityServer\bin\Debug\netcoreapp3.1\

md ..\.publishFiles

xcopy ..\Xu.IdentityServer\bin\Debug\netcoreapp3.1\*.* ..\.publishFiles\ /s /e 

echo "Successfully!!!! ^ please see the file .publishFiles"

cmd 
call SetupT8Extras\version.bat
devenv T8.sln /Rebuild Release /project SetupT8Extras

pushd SetupT8Extras\Release\
"C:\md5sum.exe" T8Extras.msi >> T8Extras.md5
popd

mkdir z:\T8Extras\%T8Extras.version%
xcopy SetupT8Extras\version.bat z:\T8Extras\%T8Extras.version%\
xcopy SetupT8Extras\Release\T8Extras.msi z:\T8Extras\%T8Extras.version%\
xcopy SetupT8Extras\Release\T8Extras.md5 z:\T8Extras\%T8Extras.version%\

echo ^<?xml version="1.0" encoding="utf-8"?^>  > z:\T8Extras\version.xml
echo ^<t8extras version="%T8Extras.version%"/^> >> z:\T8Extras\version.xml

git tag T8Extras_v%T8Extras.version%
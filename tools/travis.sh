set -x
ulimit -n 4096


git submodule update --init

powershell -c "cd ..\src; .\bootstrap.ps1; .\build.ps1 -framework "netstandard1.6" Release"
powershell -c "& c:\projects\oneget\Test\run-tests.ps1"
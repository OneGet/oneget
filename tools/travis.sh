set -x
ulimit -n 4096


git submodule update --init


powershell -c "cd src; ./bootstrap.ps1; ./build.ps1 -framework "netstandard1.6" Release"
sudo powershell -c "cd Test;  ./run-tests.ps1 coreclr"

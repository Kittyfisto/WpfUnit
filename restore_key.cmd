@setlocal
@echo off

sig\crypt.exe decrypt sig\sns key.snk
sig\crypt.exe enablesigning src\WpfUnit\WpfUnit.csproj ..\..\key.snk
sig\crypt.exe enablesigning src\WpfUnit.Test\WpfUnit.Test.csproj ..\..\key.snk

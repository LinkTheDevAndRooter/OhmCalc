@echo off
cd /d "%~dp0"

set CSC64=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
set CSC32=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
set ICON_FLAG=

if exist "resistor.ico" (
    set ICON_FLAG=/win32icon:resistor.ico
)

if exist "%CSC64%" (
    "%CSC64%" /target:winexe %ICON_FLAG% /r:System.Windows.Forms.dll,System.Drawing.dll /out:OhmCalc.exe OhmCalc.cs
) else if exist "%CSC32%" (
    "%CSC32%" /target:winexe %ICON_FLAG% /r:System.Windows.Forms.dll,System.Drawing.dll /out:OhmCalc.exe OhmCalc.cs
) else (
    echo No se encontro csc.exe en .NET Framework.
    pause
    exit /b 1
)

if %errorlevel% equ 0 (
    echo Compilacion exitosa: OhmCalc.exe generado.
) else (
    echo Hubo un error durante la compilacion.
)

pause
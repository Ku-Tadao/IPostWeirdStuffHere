!define APP_NAME "Test thingy"
!define COMP_NAME "Test thingy"
!define VERSION "01.00.00.00"
!define COPYRIGHT "Kubi  © 2006"
!define DESCRIPTION "Application"

VIProductVersion  "${VERSION}"
VIAddVersionKey "ProductName"  "${APP_NAME}"
VIAddVersionKey "CompanyName"  "${COMP_NAME}"
VIAddVersionKey "LegalCopyright"  "${COPYRIGHT}"
VIAddVersionKey "FileDescription"  "${DESCRIPTION}"
VIAddVersionKey "FileVersion"  "${VERSION}"

; The name of the installer
Name "Blitz "
; Output directory
OutFile "$DESKTOP\Test thingy.exe"
; Elevation level
RequestExecutionLevel user
; Installer Type
SilentInstall silent

Section

InitPluginsDir
SetOutPath "$PLUGINSDIR"
; All necessary files needed to be added
File "C:\Users\kubil\Documents\GitHub\IPostWeirdStuffHere\Blitz Unofficial State Checker\Blitz Unofficial State Checker\bin\Release\Blitz Unofficial State Checker.exe"
File "C:\Users\kubil\Documents\GitHub\IPostWeirdStuffHere\Blitz Unofficial State Checker\Blitz Unofficial State Checker\bin\Release\img\blitz icon.ico"

; Which file to run after the execution
ExecWait '"$PLUGINSDIR/Test thingy.exe"'
SetOutPath $Temp
SectionEnd       

!define APP_NAME "BlitzTroubleshooter"
!define COMP_NAME "BlitzTroubleshooter"
!define VERSION "02.00.00.07"
!define COPYRIGHT "Kubi© 2020"
!define DESCRIPTION "Application"
icon "C:\Users\Maruf Shah\source\repos\IPostWeirdStuffHere\Blitz Troubleshooter V2.3\Blitz Icon.ico"

VIProductVersion  "${VERSION}"
VIAddVersionKey "ProductName"  "${APP_NAME}"
VIAddVersionKey "CompanyName"  "${COMP_NAME}"
VIAddVersionKey "LegalCopyright"  "${COPYRIGHT}"
VIAddVersionKey "FileDescription"  "${DESCRIPTION}"
VIAddVersionKey "FileVersion"  "${VERSION}"

; The name of the installer
Name "BlitzTroubleshooter"
; Output directoy
OutFile "C:\Users\Maruf Shah\Downloads\Output\BlitzTroubleshooter.exe"
; Elevation level
RequestExecutionLevel user
; Installer Type
SilentInstall silent

Section

InitPluginsDir
SetOutPath "$PLUGINSDIR"
; All necessary files needed to be added
File /r "C:\Users\Maruf Shah\source\repos\IPostWeirdStuffHere\Blitz Troubleshooter V2.3\bin\Debug\*"

; Which file to run after execution
ExecWait '"$PLUGINSDIR/Blitz Troubleshooter V2.3.exe"'
SetOutPath $Temp
SectionEnd


!include "MUI2.nsh"
!include "FileFunc.nsh"

!define APPNAME "Meteo-6256577"    
!define EXE_NAME "Final.exe"         
; -------------------------------

!define COMPANY "AlexMarcouiller"   
!define INSTALLDIR "$PROGRAMFILES64\${APPNAME}" 


!define PUBLISH_DIR ".\publish"

Name "${APPNAME}"
OutFile "${APPNAME}-Setup.exe"
InstallDir "${INSTALLDIR}"
RequestExecutionLevel admin
Unicode True

Icon "app.ico"
UninstallIcon "app.ico"

!define MUI_ABORTWARNING
!define MUI_ICON "app.ico"
!define MUI_UNICON "app.ico"

!insertmacro MUI_LANGUAGE "French"
!insertmacro MUI_LANGUAGE "English"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "licence.txt"
!insertmacro MUI_PAGE_DIRECTORY

Var StartMenuFolder
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuFolder

!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

Section "Install"

  IfFileExists "${PUBLISH_DIR}\${EXE_NAME}" +2 0
    Abort "Publish introuvable: ${PUBLISH_DIR}\${EXE_NAME}. Publie en Release/win-x64 puis reessaie."

  SetOutPath "$INSTDIR"

  File /r "${PUBLISH_DIR}\*.*"

  WriteUninstaller "$INSTDIR\Uninstall.exe"

  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    CreateDirectory "$SMPROGRAMS\$StartMenuFolder"
    CreateShortcut  "$SMPROGRAMS\$StartMenuFolder\${APPNAME}.lnk" "$INSTDIR\${EXE_NAME}" "" "$INSTDIR\${EXE_NAME}" 0
    CreateShortcut  "$SMPROGRAMS\$StartMenuFolder\Désinstaller ${APPNAME}.lnk" "$INSTDIR\Uninstall.exe"
  !insertmacro MUI_STARTMENU_WRITE_END

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "${APPNAME}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" "$\"$INSTDIR\Uninstall.exe$\""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayIcon" "$\"$INSTDIR\${EXE_NAME}$\""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "InstallLocation" "$\"$INSTDIR$\""
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoRepair" 1

SectionEnd

Section "Uninstall"

  !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuFolder
  Delete "$SMPROGRAMS\$StartMenuFolder\${APPNAME}.lnk"
  Delete "$SMPROGRAMS\$StartMenuFolder\Désinstaller ${APPNAME}.lnk"
  RMDir  "$SMPROGRAMS\$StartMenuFolder"

  RMDir /r "$INSTDIR"

  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"

SectionEnd

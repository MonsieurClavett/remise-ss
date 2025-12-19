!include "MUI2.nsh"
!include "FileFunc.nsh"

!define APP_NAME "Meteo-6256577"
!define COMPANY  "6256577"
!define EXE_NAME "Final.exe"
!define PUBLISH_DIR "..\bin\Release\net8.0-windows\publish\win-x64"
!define ICON_FILE "${__FILEDIR__}\Meteo.ico"

Name "${APP_NAME}"
OutFile "${APP_NAME}-Setup.exe"
InstallDir "$PROGRAMFILES64\${APP_NAME}"
InstallDirRegKey HKCU "Software\${APP_NAME}" "InstallDir"
RequestExecutionLevel admin

Icon "${ICON_FILE}"
UninstallIcon "${ICON_FILE}"

!define MUI_ABORTWARNING
!define MUI_ICON "${ICON_FILE}"
!define MUI_UNICON "${ICON_FILE}"
!define MUI_LANGDLL_ALLLANGUAGES

Page custom LangPageCreate LangPageLeave
!define MUI_PAGE_CUSTOMFUNCTION_PRE LicenseFrPre
!insertmacro MUI_PAGE_LICENSE "${__FILEDIR__}\license_fr.txt"
!define MUI_PAGE_CUSTOMFUNCTION_PRE LicenseEnPre
!insertmacro MUI_PAGE_LICENSE "${__FILEDIR__}\license_en.txt"

!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

!insertmacro MUI_LANGUAGE "French"
!insertmacro MUI_LANGUAGE "English"

Function .onInit
  !insertmacro MUI_LANGDLL_DISPLAY
FunctionEnd

Function LangPageCreate
FunctionEnd

Function LangPageLeave
FunctionEnd

Function LicenseFrPre
  StrCmp $LANGUAGE ${LANG_FRENCH} 0 +2
  Return
  Abort
FunctionEnd

Function LicenseEnPre
  StrCmp $LANGUAGE ${LANG_ENGLISH} 0 +2
  Return
  Abort
FunctionEnd

Section "Install"
  SetOutPath "$INSTDIR"
  File /oname=Meteo.ico "${__FILEDIR__}\Meteo.ico"
  File /r "${PUBLISH_DIR}\*.*"

  WriteRegStr HKCU "Software\${APP_NAME}" "InstallDir" "$INSTDIR"

  CreateDirectory "$SMPROGRAMS\${APP_NAME}"
  CreateShortcut "$SMPROGRAMS\${APP_NAME}\${APP_NAME}.lnk" "$INSTDIR\${EXE_NAME}" "" "$INSTDIR\Meteo.ico" 0

  WriteUninstaller "$INSTDIR\Uninstall.exe"

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "DisplayName" "${APP_NAME}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "UninstallString" "$\"$INSTDIR\Uninstall.exe$\""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "DisplayIcon" "$INSTDIR\Meteo.ico"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "Publisher" "${COMPANY}"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "NoRepair" 1
SectionEnd


Section "Uninstall"
  RMDir /r "$INSTDIR"
  RMDir /r "$SMPROGRAMS\${APP_NAME}"
  DeleteRegKey HKCU "Software\${APP_NAME}"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}"
SectionEnd

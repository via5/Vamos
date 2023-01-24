setlocal enabledelayedexpansion

set "root=%~dp0"
set "dll=%~1"
set "dll_name=%~n1%~x1"
set "inc=%root%/vam.inc.bat"
set "vam_paths="
set "bepinex_plugins=BepInEx/scripts"

call :canonical "%root%"
set root=!canonical_output!

if exist "%inc%" (
	call "%inc%"
)

set "this_bepinex=%root%/../%bepinex_plugins%"
call :canonical "%this_bepinex%"
set this_bepinex=!canonical_output!

if exist "!this_bepinex!" (
	set "vam_paths=!this_bepinex!"
)

for %%p in (%more_vam_paths%) do (
	set "pp=%%~p/%bepinex_plugins%"
	call :canonical "!pp!"
	set pp=!canonical_output!

	set "vam_paths="!vam_paths!" "!pp!""
)

if "!vam_paths!" == "" (
	echo no vam paths
	exit /b 1
)

for %%p in (%vam_paths%) do (
	set "v=%%~p"

	if not exist "!v!" (
		echo path "!v!" doesn't exist
		exit /b 1
	)
)

for %%p in (%vam_paths%) do (
	set "pp=%%~p"

	if not exist "!pp!" (
		mkdir "!pp!"
	)

	echo %dll_name% -^> !pp!/%dll_name%
	copy "%dll%" "!pp!/" > NUL
)

:canonical
	set canonical_output=%~dpfn1
	set "canonical_output=%canonical_output:\=/%"
goto :eof

@echo off
setlocal

set CURR_DIR=%CD%

echo "### BUILDING UsersFunction ###"
cd UsersFunction/src/UsersFunction
call build.cmd
if %ERRORLEVEL% NEQ 0 goto FAILURE
cd %CURR_DIR%

echo "### BUILDING RolesFunction ###"
cd RolesFunction/src/RolesFunction
call build.cmd
if %ERRORLEVEL% NEQ 0 goto FAILURE
cd %CURR_DIR%

goto END

:FAILURE
echo ERROR STOP: %ERRORLEVEL%
pause

:END
echo "### BUILD SUCCESSFUL ###"
endlocal

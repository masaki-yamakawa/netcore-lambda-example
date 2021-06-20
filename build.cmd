setlocal

set CURR_DIR=%CD%

echo "### BUILDING UsersFunction ###"
cd UsersFunction/src/UsersFunction
call build.cmd
cd %CURR_DIR%

echo "### BUILDING RolesFunction ###"
cd RolesFunction/src/RolesFunction
call build.cmd
cd %CURR_DIR%

endlocal

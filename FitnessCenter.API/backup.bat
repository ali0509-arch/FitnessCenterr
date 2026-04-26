@echo off
set BACKUP_DIR=C:\Backups\FitnessDB
set DATE=%date:~-4,4%%date:~-7,2%%date:~0,2%

mkdir %BACKUP_DIR% 2>nul

"C:\Program Files\MySQL\MySQL Workbench 8.0\mysqldump.exe" -h mysql99.unoeuro.com -u kunforhustlers_dk -pRmcAfptngeBaxkw6zr5E --no-tablespaces kunforhustlers_dk_db > %BACKUP_DIR%\backup_%DATE%.sql
echo Backup gemt: %BACKUP_DIR%\backup_%DATE%.sql
pause
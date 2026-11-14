@echo off
powershell.exe -NoProfile -ExecutionPolicy RemoteSigned -File "%~dp0scripts\Open-Admin.ps1"
if errorlevel 1 pause

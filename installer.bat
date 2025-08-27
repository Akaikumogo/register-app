@echo off
setlocal

:: Startup folder yo'lini olish
set STARTUP=%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup

:: monitor.ps1 yaratamiz
(
echo $API_URL = "https://controll.akaikumogo.uz/add-log"
echo $DEVICE_NAME = $env:COMPUTERNAME
echo
echo ^# Eski processlar ro'yxati
echo $oldProcs = Get-Process ^| ForEach-Object { $_.ProcessName.ToLower() }
echo
echo while ($true^) {
echo     Start-Sleep -Seconds 3
echo
echo     $newProcs = Get-Process ^| ForEach-Object { $_.ProcessName.ToLower() }
echo
echo     ^# Yangi ochilgan processlar
echo     foreach ($p in ($newProcs ^| Where-Object {$_ -notin $oldProcs}^)) {
echo         $data = @{
echo             device = $DEVICE_NAME
echo             application = $p
echo             action = "open"
echo             time = (Get-Date).ToUniversalTime().ToString("o")
echo         }
echo         try {
echo             Invoke-RestMethod -Uri $API_URL -Method POST -Body ($data ^| ConvertTo-Json -Compress) -ContentType "application/json"
echo         } catch {}
echo     }
echo
echo     ^# Yopilgan processlar
echo     foreach ($p in ($oldProcs ^| Where-Object {$_ -notin $newProcs}^)) {
echo         $data = @{
echo             device = $DEVICE_NAME
echo             application = $p
echo             action = "close"
echo             time = (Get-Date).ToUniversalTime().ToString("o")
echo         }
echo         try {
echo             Invoke-RestMethod -Uri $API_URL -Method POST -Body ($data ^| ConvertTo-Json -Compress) -ContentType "application/json"
echo         } catch {}
echo     }
echo
echo     $oldProcs = $newProcs
echo }
) > C:\monitor.ps1

:: launcher.vbs yaratamiz
(
echo Set objShell = CreateObject("Wscript.Shell")
echo objShell.Run "powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\monitor.ps1", 0, False
) > "%STARTUP%\launcher.vbs"

echo.
echo ✅ O'rnatish tugadi! Kompyuter qayta ishga tushirilganda kuzatuv avtomatik ishlaydi.
pause

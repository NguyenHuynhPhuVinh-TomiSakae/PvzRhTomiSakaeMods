@echo off
setlocal

:: Đường dẫn file nguồn
set "sourceFile=C:\Users\kotor\Documents\PvzRhTomiSakaeMods\HoaHaiDauLuaBang\obj\Debug\net6.0\HoaHaiDauLuaBang.dll"

:: Đường dẫn thư mục đích1
set "targetFolder1=C:\Users\kotor\Documents\PvzRhTomiSakaeMods\Mods"

:: Đường dẫn thư mục đích2
set "targetFolder2=C:\Users\kotor\Documents\Plants vs Zombies Fusion - 2.3.1 [Version 2][Multi-Language][PC] by the Blooms Community\Game Files\Mods"

:: Copy file và ghi đè nếu đã tồn tại
copy /Y "%sourceFile%" "%targetFolder1%"

:: Copy file và ghi đè nếu đã tồn tại
copy /Y "%sourceFile%" "%targetFolder2%"

:: Chạy file .exe từ thư mục khác
start "" "C:\Users\kotor\Documents\Plants vs Zombies Fusion - 2.3.1 [Version 2][Multi-Language][PC] by the Blooms Community\Game Files\PlantsVsZombiesRH.exe"

endlocal

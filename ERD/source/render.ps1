param([string]$Svg, [int]$W, [int]$H, [string]$Png)
$edge = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
if (Test-Path $Png) { Remove-Item $Png -Force }
$size = "--window-size=$W,$H"
$shot = "--screenshot=$Png"
$url = "file:///" + ($Svg -replace '\\','/')
& $edge --headless --disable-gpu --hide-scrollbars $size $shot $url 2>&1 | Out-Null
Add-Type -AssemblyName System.Drawing
$img = [System.Drawing.Image]::FromFile($Png); "PNG: $($img.Width) x $($img.Height)"; $img.Dispose()

$ErrorActionPreference = 'Stop'
$plink = Join-Path $env:ProgramFiles 'PuTTY/plink.exe'
if (-not (Test-Path -LiteralPath $plink)) { throw 'PuTTY is required: plink.exe was not found.' }
$server = '194.226.125.251'
$fingerprint = 'SHA256:enAOKsRL4L5lWIhpLoiUR3SbX++g795UUBHuM2CCEfI'
function Test-AdminPort {
    $client = [Net.Sockets.TcpClient]::new()
    try { $connect = $client.ConnectAsync('127.0.0.1', 8081); return ($connect.Wait(300) -and $client.Connected) }
    catch { return $false }
    finally { $client.Dispose() }
}
if (Test-AdminPort) { throw 'Port 8081 is already in use. Close the previous tunnel before starting a new one.' }
Write-Host 'Enter the server SSH password in the PuTTY window. Keep that window open.'
$tunnel = Start-Process -FilePath $plink -ArgumentList @('-ssh','-N','-no-antispoof','-hostkey',$fingerprint,'-L','127.0.0.1:8081:127.0.0.1:8081',"root@$server") -PassThru -WindowStyle Normal
$ready = $false
for ($attempt = 0; $attempt -lt 120; $attempt++) {
    if ($tunnel.HasExited) { throw 'SSH tunnel closed. Check the password or SSH connection.' }
    if (Test-AdminPort) { $ready = $true; break }
    Start-Sleep -Seconds 1
}
if (-not $ready) { throw 'The tunnel did not open. Check the SSH password window.' }
Start-Process 'http://localhost:8081/admin'
Write-Host 'Admin opened at http://localhost:8081/admin. Closing the PuTTY window disconnects access.'

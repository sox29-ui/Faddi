# Installs or updates Faddi for the current user: downloads the latest Faddi.exe from GitHub Releases into
# %LOCALAPPDATA%\Programs\Faddi, adds Start menu and Desktop shortcuts, and opens it. No admin rights needed.
#
#   irm https://raw.githubusercontent.com/sox29-ui/Faddi/main/install.ps1 | iex
#
# Run the same line again to update.
& {
    $ErrorActionPreference = 'Stop'
    $ProgressPreference = 'SilentlyContinue' # Windows PowerShell's progress bar slows the download to a crawl
    [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12

    $dir = Join-Path $env:LOCALAPPDATA 'Programs\Faddi'
    $exe = Join-Path $dir 'Faddi.exe'
    New-Item -ItemType Directory -Force $dir | Out-Null

    Write-Host 'Downloading the latest Faddi...'
    Invoke-WebRequest 'https://github.com/sox29-ui/Faddi/releases/latest/download/Faddi.exe' -OutFile "$exe.new" -UseBasicParsing

    # a copy running from here has to close before it can be replaced
    foreach ($p in Get-Process Faddi -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe }) {
        try { $p.Kill(); $p.WaitForExit(10000) | Out-Null }
        catch { throw 'Faddi is running as admin. Close it (right-click its icon next to the clock, then Exit) and run this line again.' }
    }
    Move-Item "$exe.new" $exe -Force

    $shell = New-Object -ComObject WScript.Shell
    foreach ($folder in [Environment]::GetFolderPath('Programs'), [Environment]::GetFolderPath('Desktop')) {
        $link = $shell.CreateShortcut((Join-Path $folder 'Faddi.lnk'))
        $link.TargetPath = $exe
        $link.WorkingDirectory = $dir
        $link.Description = 'Analyze your PC and free up space'
        $link.Save()
    }

    Write-Host "Faddi is installed in $dir, with shortcuts on the Start menu and the Desktop. Opening it..."
    Start-Process $exe
}

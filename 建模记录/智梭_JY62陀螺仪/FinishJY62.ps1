$ErrorActionPreference='Stop'
$dll='D:\Program Files\SOLIDWORKS Corp\SOLIDWORKS\api\redist\SolidWorks.Interop.sldworks.dll'
Add-Type -Path $dll
Add-Type -Path (Join-Path $PSScriptRoot 'FinishJY62.cs') -ReferencedAssemblies @($dll,(Join-Path $PSHOME 'ref\System.Collections.dll'))
try { [FinishJY62]::Run() } catch { $_.Exception.ToString(); if($_.Exception.InnerException){$_.Exception.InnerException.ToString()}; exit 1 }

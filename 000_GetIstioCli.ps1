if((Test-Path ./istioctl.exe -PathType Leaf) -ne $true)
{
  Invoke-WebRequest -Uri https://github.com/istio/istio/releases/download/1.6.5/istioctl-1.6.5-win.zip -OutFile ./istio.zip
  Expand-Archive -Path ./istio.zip -DestinationPath ./
  Remove-Item ./istio.zip
}
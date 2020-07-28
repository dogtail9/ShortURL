param(
  [String]$tag = "dev"
)

$OFS = "`r`n---`r`n"
$FileName = ".\deploy-$tag.yml"

if (Test-Path $FileName) 
{
  Remove-Item $FileName
}

Get-Content .\110_namespace.yml | Add-Content -Path $FileName
$OFS | Add-Content -Path $FileName

Get-Content .\120_gateway.yml | Add-Content -Path $FileName
$OFS | Add-Content -Path $FileName

Get-Content .\130_sts.yml | Add-Content -Path $FileName
$OFS | Add-Content -Path $FileName

Get-Content .\140_zipkin.yml | Add-Content -Path $FileName
$OFS | Add-Content -Path $FileName

Get-Content .\150_database.yml | Add-Content -Path $FileName
$OFS | Add-Content -Path $FileName

Get-Content .\160_redirectApi.yml | Add-Content -Path $FileName
$OFS | Add-Content -Path $FileName

Get-Content .\170_managementApi.yml | Add-Content -Path $FileName
$OFS | Add-Content -Path $FileName

Get-Content .\180_adminWeb.yml | Add-Content -Path $FileName
$OFS | Add-Content -Path $FileName

((Get-Content -path $FileName -Raw) -replace 'dev',$tag) | Set-Content -Path $FileName
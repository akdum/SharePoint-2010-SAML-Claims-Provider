Add-PSSnapin Microsoft.SharePoint.PowerShell

$sts = Get-SPSecurityTokenServiceConfig
$providerName = "KO SiteMinder"
$trustedLoginProvider = $sts.TrustedLoginProviders[$providerName]
if ($trustedLoginProvider)
{
    $trustedLoginProvider.ClaimProviderName = "SAMLClaimsProvider"
    $trustedLoginProvider.Update()
}

$webApp = Get-SPWebApplication "http://epruizhw0101"
$webApp.Properties["SAMLUserSource"] = "http://evbyminsd0922:3000/users/find/"
$webApp.Update()
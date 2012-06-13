Add-PSSnapin Microsoft.SharePoint.PowerShell

$appName = "Your web app name"
$providerName = "Your trusted login provider name"
$userSorceUrl = "http://evbyminsd0922:3000/users/find/"

$sts = Get-SPSecurityTokenServiceConfig
$trustedLoginProvider = $sts.TrustedLoginProviders[$providerName]
if ($trustedLoginProvider)
{
    $trustedLoginProvider.ClaimProviderName = "SAMLClaimsProvider"
    $trustedLoginProvider.Update()
}

$webApp = Get-SPWebApplication $appName
$webApp.Properties["SAMLUserSource"] = $userSorceUrl
$webApp.Update()
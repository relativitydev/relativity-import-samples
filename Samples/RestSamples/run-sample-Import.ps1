#import
$global:rootDir = "$PSScriptRoot"
. "$global:rootDir\Helpers\AuthClass.ps1"
. "$global:rootDir\Helpers\WebRequestClass.ps1"

$hostAddress = "https://sample-hosts/"
$userName = "sample@username"
$password = "password!"

$global:Auth = [Auth]::new($hostAddress, $userName, $password)
$global:WebRequest = [WebRequest]::new($global:Auth)

Describe "Sample import" {
    . "$global:rootDir\SamplesCollection\sample01-import-native-files.ps1"
}
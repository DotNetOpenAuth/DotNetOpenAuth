# This is a useful diagnostic tool for when users send in failed positive assertions
# and you want to read what's in the token parameter.

param (
	$token = { throw "token or urlToken parameter required" }, 
	$urlToken
	)

[void] [Reflection.Assembly]::LoadWithPartialName('System.Web')

if ($urlToken) {
	$token = [Web.HttpUtility]::UrlDecode($urlToken)
}

$tokenBytes = [Convert]::FromBase64String($token)
$siglength = 32
$decodedToken = [Text.Encoding]::UTF8.GetString($tokenBytes, $siglength, $tokenBytes.length - $siglength)

Write-Host "Decoded token:"
Write-Host $decodedToken
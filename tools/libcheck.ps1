param(
	$OldVersion,
	$NewVersion,
	$Configuration='Debug'
)

function Usage() {
	$ScriptName = Split-Path -leaf $MyInvocation.ScriptName
	Write-Host "$ScriptName -OldVersion <tag> -NewVersion <branch>"
	exit
}

if ($Args -Contains "-?" -or !$OldVersion -or !$NewVersion -or ($OldVersion -eq $NewVersion)) {
	Usage
}

function SetupVariables() {
	$ToolsDir = Split-Path $MyInvocation.ScriptName
	$RootDir = [io.path]::getfullpath((Join-Path $ToolsDir .. -resolve))
	$BinDir = "$RootDir\bin"
	$LibCheckTmpDir = Join-Path ([IO.Path]::GetTempPath()) "LibCheck"
}

function Checkout($Version) {
	git checkout $Version
}

function Build() {
	msbuild.exe "$RootDir\src\YOURLIBNAME\YOURLIBNAME.csproj" /p:Configuration=$Configuration
}

function Generate-Metadata($Version) {
	Push-Location $LibCheckTmpDir
	& ".\libcheck.exe" -store "YOURLIBNAME.dll" $Version -full "$BinDir\$Configuration"
	Pop-Location
}

function Compare-Metadata() {
	Push-Location $LibCheckTmpDir
	& ".\libcheck.exe" -compare $OldVersion $NewVersion
	Pop-Location
}

function ShadowCopy-Libcheck() {
	# This function copies LibCheck from the checked out version to a temp
	# directory so that as we git checkout other versions of YOURLIBNAME,
	# we can be sure of running one consistent version of LibCheck.
	Remove-Item -Recurse $LibCheckTmpDir
	Copy-Item -Recurse "$ToolsDir\LibCheck" (Split-Path $LibCheckTmpDir)
	# As a side benefit, this also puts the results of running LibCheck
	# outside the git repo so it can't get checked in accidentally.
}

. SetupVariables
ShadowCopy-Libcheck
Checkout -version $OldVersion
Build
Generate-Metadata -version $OldVersion
Checkout -version $NewVersion
Build
Generate-Metadata -version $NewVersion
Compare-Metadata
Pop-Location
& "$LibCheckTmpDir\$($OldVersion)to$($NewVersion)\APIChanges$($OldVersion)to$($NewVersion).html"

param(
	$Configuration='Release',
	[switch] $Rebuild,
	[switch] $Gui
)

function Usage() {
	$ScriptName = Split-Path -leaf $MyInvocation.ScriptName
	Write-Host "$ScriptName [-Configuration Debug|Release] [-rebuild] [-gui]"
	exit
}

if ($Args -Contains "-?") {
	Usage
}

function SetupVariables() {
	$ToolsDir = Split-Path $MyInvocation.ScriptName
	$RootDir = [io.path]::getfullpath((Join-Path $ToolsDir .. -resolve))
	$BinDir = "$RootDir\bin"
	$errorActionPreference = "Stop"
}

function Build() {
	if ($Rebuild) {
		$target = "Rebuild"
	} else {
		$target = "Build"
	}
	msbuild "$RootDir\src\DotNetOpenId.Test\DotNetOpenId.Test.csproj" /p:Configuration=$Configuration /t:$Target
	if ($lastexitcode -ne 0) { throw "Build failure." }
}

function Test() {
	$target = "$BinDir\$Configuration\DotNetOpenId.Test.dll"
	if ($Gui) {
		& "$ToolsDir\nunit\bin\nunit.exe" $target /run
	} else {
		& "$ToolsDir\nunit\bin\nunit-console.exe" $target
	}
}

. SetupVariables
Build
Test

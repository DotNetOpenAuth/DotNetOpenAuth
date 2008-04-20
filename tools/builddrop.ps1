param(
	$Version, 
	$Configuration='Release',
	[switch] $Signed,
	[switch] $Force=$false,
	[switch] $Rebuild
)

$ProductName = "DotNetOpenId"

function Usage() {
	$ScriptName = Split-Path -leaf $MyInvocation.ScriptName
	Write-Host "$ScriptName [-Version x.y.z] [-Configuration Debug|Release] [-force] [-signed]"
	exit
}

if ($Args -Contains "-?") {
	Usage
}

function jdate($date = [datetime]::now) {
	$yearLastDigit = $date.year % 10
	$firstOfYear = [datetime] "1/1/$($date.year)"
	$dayOfYear = ($date - $firstOfYear).days + 1
	$jdate = $yearLastDigit * 1000 + $dayOfYear
	$jdate
}

function SetupVariables() {
	$ToolsDir = Split-Path $MyInvocation.ScriptName
	$RootDir = [io.path]::getfullpath((Join-Path $ToolsDir .. -resolve))
	$BinDir = "$RootDir\bin"
	$AssemblyInfoFiles = "$RootDir\src\$ProductName\Properties\AssemblyInfo.cs","$RootDir\src\$ProductName.Test\Properties\AssemblyInfo.cs"
	if ($Version -and ($Version -notmatch "^(\d)\.(\d)\.(\d)$")) { Usage }
	if (!$Version) {
		$VersionRegEx = [regex] "\d\.\d\.\d"
		$m = $VersionRegEx.Match((Get-Content $AssemblyInfoFiles[0]))
		$Script:Version = $m.Groups[0].Value
	}
	$Version += "." + (jdate)
	$DropDir = "$RootDir\$ProductName-$Version"
	$errorActionPreference = "Stop"
}

function PerformChecks() {
	if ((Test-Path $DropDir) -and -not $Force) {
		throw "$DropDir already exists.  Use -force to overwrite."
	}
	if (@(Get-Command "msbuild.exe").Length -eq 0) {
		throw "Unable to find msbuild.exe.  Make sure your .NET SDK is in the PATH."
	}
	if (-not (Test-Path $AssemblyInfoFiles[0])) {
		throw "Unable to find AssemblyInfo.cs at $($AssemblyInfoFiles[0])."
	}
	if (-not ($Configuration -eq "Release" -or $Configuration -eq "Debug")) { Usage }
}

function SetBuildVersion() {
	Write-Host "Building version $Version..."
	foreach ($AssemblyInfoFile in $AssemblyInfoFiles) {
		# Make backup
		Copy-Item $AssemblyInfoFile "$($AssemblyInfoFile)~"
		# Now change the version attribute in the file.
		$VersionRegEx = [regex]"\d+\.\d+\.\d+\.\d+"
		(Get-Content $AssemblyInfoFile) | 
			Foreach-Object { $VersionRegEx.Replace($_, $Version) } |
			Set-Content $AssemblyInfoFile -encoding utf8
	}
}

function RevertBuildVersion() {
	foreach ($AssemblyInfoFile in $AssemblyInfoFiles) {
		# Restore backup
		Copy-Item "$($AssemblyInfoFile)~" $AssemblyInfoFile
	}
}

function Build() {
	if ($Rebuild) {
		msbuild $RootDir\src\$ProductName.sln /p:Configuration=$Configuration /p:Sign=$Signed > $nul
	} else {
		msbuild $RootDir\src\$ProductName.sln /p:Configuration=$Configuration /p:Sign=$Signed /t:rebuild > $nul
	}
	if ($lastexitcode -ne 0) { throw "Build failure." }
}

function AssembleDrop() {
	If (Test-Path $DropDir) { Remove-Item -recurse -force $DropDir }
	[void] (mkdir $DropDir\Bin)
	Copy-Item "$BinDir\$Configuration\$ProductName.???" $DropDir\Bin
	Copy-Item -recurse $RootDir\Samples $DropDir
	Copy-Item -Recurse $RootDir\Doc\*.htm* $DropDir

	# Do a little cleanup of files that we don't want to inclue in the drop
	("obj", "*.user", "*.sln.cache", "*.suo", "*.user", ".gitignore", "*.ldf", "*Trace.txt") |% {
		Get-ChildItem -force -recurse "$DropDir\Samples" "$_" |% { 
			If (Test-Path "$($_.FullName)") {
				$errorActionPreference = "SilentlyContinue"
				Remove-Item -force -recurse -path "$($_.FullName)"
			}
		}
	}
	
	# Adjust Sample projects references
	$vsns = "http://schemas.microsoft.com/developer/msbuild/2003"
	Get-ChildItem -recurse $DropDir\Samples *.csproj |% {
		Write-Debug "Adjust project references for sample $_"
		$proj = [xml] (Get-Content -path $_.fullname)
		$nsmgr = New-Object Xml.XmlNamespaceManager $proj.get_NameTable()
		$nsmgr.AddNamespace("vs", $vsns)
		$ref = $proj.SelectSingleNode("/vs:Project/vs:ItemGroup/vs:ProjectReference", $nsmgr)
		$parentNode = $ref.get_ParentNode()
		$parentNode.RemoveChild($ref)
		$newRef = $proj.CreateElement("Reference", $vsns)
		$newRef.SetAttribute("Include", "$ProductName")
		$hintPath = $proj.CreateElement("HintPath", $vsns)
		$hintPath.set_InnerText("..\..\Bin\$ProductName.dll")
		$newRef.AppendChild($hintPath)
		$parentNode.AppendChild($newRef)
		Set-Content -path $_.FullName -value ($proj.get_outerxml())
	} > $nul
}

function Finished() {
	Write-Host "Successful.  The drop can be found in the $DropDir directory."
}

. SetupVariables
PerformChecks
SetBuildVersion
Build
RevertBuildVersion
AssembleDrop
Finished

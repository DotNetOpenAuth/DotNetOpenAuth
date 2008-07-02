param([switch]$offline)

[void] [Reflection.Assembly]::LoadWithPartialName("System.Web")

function SafeXml($value) {
	[Web.HttpUtility]::HtmlEncode($value)
}

function Get-Modifications($fromDate, $toDate, $gitRepo) {
	$commitRegex = [regex]"commit (\w+)"
	$authorRegex = [regex]"Author:\s*(.+) <(.+)>"
	$dateRegex = [regex]"Date:\s*([^ ]+) ([^ ]+) ([^ ]+)"
	$commentRegex = [regex]"(?:    (.+))|(?:^\s*$)"
	$fileRegex = [regex]"^(\S.+)"

	Push-Location $gitRepo
	# this should just be git fetch, and then look at the log on the remote branch.
	# [void] (git fetch)
	if (!$offline) { [void] (git pull) }
	# reverse dates because CruiseControl gives them in reverse order
	$gitlog = git log "--since=$toDate" "--until=$fromDate" "--date=iso" "--name-only"
	@"
<?xml version="1.0" encoding="utf-8"?>
<ArrayOfModification>
"@
	
	$i = 0
	while($i -le $gitlog.length) {
		$m = $commitRegex.match($gitlog[$i++])
		if ($m.success) {
			$changeNumber = $m.groups[1].value
			$m = $authorRegex.match($gitlog[$i++])
			$username, $email = $m.groups[1].value, $m.groups[2].value
			$m = $dateRegex.match($gitlog[$i++])
			$date, $time, $timezone = $m.groups[1].value, $m.groups[2].value, $m.groups[3].value
			[void] ($comment = New-Object Text.StringBuilder)
			while(($m = $commentRegex.match($gitlog[++$i])).success) {
				[void]$comment.AppendLine($m.groups[1].value)
			}
			$comment.length -= 2
			$files = @()
			while(($m = $fileRegex.match($gitlog[++$i])).success) {
				$files += $m.groups[1].value
			}
		@"
	<Modification>
		<ChangeNumber>$(SafeXml($changeNumber))</ChangeNumber>
		<Comment>$(SafeXml($comment))</Comment>
		<EmailAddress>$(SafeXml($email))</EmailAddress>
"@
		foreach($file in $files) { @"
		<FileName>$file</FileName>
"@
		}
@"
		<ModifiedTime>$(SafeXml("$($date)T$time$timezone"))</ModifiedTime>
		<UserName>$(SafeXml($username))</UserName>
	</Modification>
"@
		}
		while(!$commitRegex.match($gitlog[$i]) -and $i -le $gitlog.length) {
			Write-Host "There: " $gitlog[$i]
			$i++
		}
	}
	
	@"
</ArrayOfModification>
"@
	Pop-Location
}

function Get-Source($workingDirectory, $date, $gitRepo, $branch) {
	Push-Location $gitRepo
	if (!$offline) { git pull }
	# we don't yet support syncing to anything other than the tip
	#git checkout "$($branch)@{$date}"
	Pop-Location
}

function Set-Label($label, $date, $gitRepo) {
	Push-Location $gitRepo
	
	Pop-Location

	throw "Not supported yet."
}


$op = $Args[0]
switch($op) {
	"GETMODS" { Get-Modifications $Args[1] $Args[2] $Args[3] }
	"GETSOURCE" { Get-Source $Args[1] $Args[2] $Args[3] $Args[4] }
	"SETLABEL" { Set-Label $Args[1] $Args[2] $Args[3] }
	default { throw "Invalid opcode" }
}
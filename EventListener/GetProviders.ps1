$output = logman query providers
$guids = @()
foreach ($line in $output) {
    if ($line -match '{.*}$') {
        $guids += $line.Substring($line.IndexOf('{'), 38)
    }
}
$guids | Out-File -FilePath .\guids.txt
#$guids
<# eg: url="http://analytics.metrics.labs.cityindex.com/GetReport.ashx?Application=CiapiLatencyCollector&amp;Type=StreamingLatencySummaries&amp;Locations=United%20Kingdom" #>
Param($url)
try
{
$url = $url.Replace("`"", "")
write-host $url "`r`n"

$client = New-Object System.Net.WebClient
$client.Credentials = New-Object System.Net.NetworkCredential($UserName, $Password)
$page = $client.DownloadString($url)

$lines = $page.Split("`r`n", [StringSplitOptions]::RemoveEmptyEntries)

if (($lines.length -lt 2) -or ($lines.length -gt 3))
{
Write-Host "Invalid lines count"
write-host $page "`r`n"
exit 1
}

$status, $lines = $lines

if ($lines.length -eq 1)
{
Write-Host "No data"
exit 0
}

$columnNames = $lines[0].Split("`t")
$columns = $lines[1].Split("`t")

<#
0-Country 1-City 2-Location 3-FunctionName 4-Count 5-ExceptionsCount 6-Average 7-Min 8-Percentile2 9-LowerQuartile 10-Median 11-UpperQuartile 12-Percentile98 13-Max
#>
if ($columnNames.length -ne $columns.length)
{
Write-Host "Lines length doesn't match"
exit 1
}


for($i=4; $i -lt $columnNames.length; $i++) {
$columnName = $columnNames[$i].Trim()
$columnValue = $columns[$i].Trim()
write-host "Statistic.$columnName : $columnValue"
write-host "Message.$columnName : $columnName"
}
}
catch [Exception] {
Write-Host $_.Exception.ToString()
}
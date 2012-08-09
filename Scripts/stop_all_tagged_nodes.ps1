# stop_all_tagged_nodes.ps1 - Stops all the instances that match a specific tag, in all AWS regions
# Eg: stop_all_tagged_nodes.ps1 -tag isLatencyCollector -accessKeyID AKI******** -secretAccessKeyID 1VVllumsz*** 
Param(
	[parameter(mandatory=$true, HelpMessage="Enter your AWS access key.")]
	[String]
	$accessKeyID,
	[parameter(mandatory=$true, HelpMessage="Enter your AWS SECRET access key.")]
	[String]
	$secretAccessKeyID,
	[parameter(mandatory=$true, HelpMessage="Enter the tag to filter on (eg, isLatencyCollector ) ")]
	[String]
	$tag
)

Add-Type -Path AWSSDK.dll

$regionsClient = [Amazon.AWSClientFactory]::CreateAmazonEC2Client($accessKeyID, $secretAccessKeyID)
$drRequest = New-Object -TypeName Amazon.EC2.Model.DescribeRegionsRequest
$regions = $regionsClient.DescribeRegions($drRequest).DescribeRegionsResult.Region

foreach($region in $regions) {

	Write-Host "================================== "
	Write-Host "Working in region:" $region.RegionName
	$ec2Config = New-Object —TypeName Amazon.EC2.AmazonEC2Config
	$ec2Config.ServiceURL = "https://" + $region.Endpoint

	$client = [Amazon.AWSClientFactory]::CreateAmazonEC2Client($accessKeyID, $secretAccessKeyID, $ec2Config)
	
	$filter = New-Object -TypeName Amazon.EC2.Model.Filter -Property @{
		WithName = 'tag:' + $tag
		WithValue = 'true'
	} 
	$request = New-Object -TypeName Amazon.EC2.Model.DescribeInstancesRequest -Property @{
		WithFilter = $filter
	}
	$response = $client.DescribeInstances($request)

	$stopInstanceRequest = New-Object —TypeName Amazon.EC2.Model.StopInstancesRequest
	$stopInstanceRequest.InstanceId = New-Object System.Collections.Generic.List``1[System.String]

	foreach($reservation in $response.DescribeInstancesResult.Reservation) {
		foreach($instance in $reservation.RunningInstance) { 
			Write-Host "Queueing instance" $instance.InstanceId "for stopping..."
			$stopInstanceRequest.InstanceId.Add($instance.InstanceId)
		}
	}

	If ($stopInstanceRequest.InstanceId.Count -gt 0) {
		Write-Host "Stopping" $stopInstanceRequest.InstanceId.Count "instances in:" $region.RegionName
		$client.StopInstances($stopInstanceRequest).ToString()
	}
}
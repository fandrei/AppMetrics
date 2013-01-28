# list_all_tagged_nodes.ps1 - Lists all the instances that match a specific tag, in all AWS regions
# Eg: list_all_tagged_nodes.ps1 -tag isLatencyCollector -accessKeyID AKI******** -secretAccessKeyID 1VVllumsz*** 
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

$drRequest = New-Object -TypeName Amazon.EC2.Model.DescribeRegionsRequest
$regions = $client.DescribeRegions($drRequest).DescribeRegionsResult.Region

foreach($region in $regions) {

	Write-Host "================================== "
	Write-Host "Working in region:" $region.RegionName
	$ec2Config = New-Object —TypeName Amazon.EC2.AmazonEC2Config
	$ec2Config.ServiceURL = "https://" + $region.Endpoint

	$client = [Amazon.AWSClientFactory]::CreateAmazonEC2Client($accessKeyID, $secretAccessKeyID, $ec2Config)
	
	$filter = New-Object -TypeName Amazon.EC2.Model.Filter -Property @{
		WithName = 'tag:' + $tag
		WithValue = '*'
	} 
	$request = New-Object -TypeName Amazon.EC2.Model.DescribeInstancesRequest -Property @{
		WithFilter = $filter
	}
	$response = $client.DescribeInstances($request)

	$startInstanceRequest = New-Object —TypeName Amazon.EC2.Model.StartInstancesRequest
	$startInstanceRequest.InstanceId = New-Object System.Collections.Generic.List``1[System.String]

	foreach($reservation in $response.DescribeInstancesResult.Reservation) {
	  foreach($instance in $reservation.RunningInstance) { 
		Write-Host "Matching instance" $instance.InstanceId
	  }
	}
}
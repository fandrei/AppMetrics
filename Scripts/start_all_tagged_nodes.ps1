# stop_all_tagged_nodes.ps1 - Starts all the instances that match a specific tag, in all AWS regions
# Eg: start_all_tagged_nodes.ps1 -tag isLatencyCollector -accessKeyID AKI******** -secretAccessKeyID 1VVllumsz*** 
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
$endpoints = "https://ec2.us-east-1.amazonaws.com","https://ec2.us-west-2.amazonaws.com","https://ec2.us-west-1.amazonaws.com","https://ec2.eu-west-1.amazonaws.com","https://ec2.ap-southeast-1.amazonaws.com","https://ec2.ap-northeast-1.amazonaws.com","https://ec2.sa-east-1.amazonaws.com"

foreach($endpoint in $endpoints) {

    Write-Host "================================== "
	Write-Host "Working in region:" $endpoint
	$ec2Config = New-Object —TypeName Amazon.EC2.AmazonEC2Config
	$ec2Config.ServiceURL = $endpoint

	$client = [Amazon.AWSClientFactory]::CreateAmazonEC2Client($accessKeyID, $secretAccessKeyID, $ec2Config)
	
	$req = New-Object —TypeName Amazon.EC2.Model.DescribeInstancesRequest
	$response = $client.DescribeInstances($req)

	$startInstanceRequest = New-Object —TypeName Amazon.EC2.Model.StartInstancesRequest
	$startInstanceRequest.InstanceId = New-Object System.Collections.Generic.List``1[System.String]

	foreach($reservation in $response.DescribeInstancesResult.Reservation) {
	  foreach($instance in $reservation.RunningInstance) { 
		 foreach($current_tag in $instance.Tag)	{
			if ($current_tag.Key -eq $tag) 
			{
				Write-Host "Queueing instance" $instance.InstanceId "for starting..."
				$startInstanceRequest.InstanceId.Add($instance.InstanceId)
			}
		}
	  }
	}

	Write-Host "Starting" startInstanceRequest.InstanceId.Count "instances in:" $endpoint
	$startInstanceResponse = $client.StartInstances($startInstanceRequest)
	$startInstanceResponse.ToString()
}
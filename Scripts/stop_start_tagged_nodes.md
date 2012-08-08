Stop/start collection of Amazon instances based on tag
======================================================

These scripts can be used to quickly stop or start a group of AWS instances based
on an instance tag.

For example, say you needed to quickly stop all the instances that were running 
the CIAPI Latency Collector service.  From a PowerShell command prompt you would run:

PS > stop_all_tagged_nodes.ps1 --access_key="AWS-access-key" --secret_access_key="AWS-secret" --tag="CIAPILatencyCollector"

Installation
============

1. Install PowerShell
1. Enable running scripts from Admin PS command prompt ```PS > Set-ExecutionPolicy -ExecutionPolicy RemoteSigned```
1. Copy all the files into a folder on your PC
1. Ensure your EC2 API user has the following permissions:

         {
           "Statement": [
             {
               "Sid": "Stmt1344443650816",
               "Action": [
                 "ec2:DescribeInstanceStatus",
                 "ec2:DescribeInstances",
                 "ec2:DescribeTags",
                 "ec2:StartInstances",
                 "ec2:StopInstances"
               ],
               "Effect": "Allow",
               "Resource": [
                 "*"
               ]
             }
           ]
         }

Running
=======

1.  To stop ```stop_all_tagged_nodes.ps1 -tag isLatencyCollector -accessKeyID AKI******** -secretAccessKeyID 1VVllumsz*** ```
1.  To start ```start_all_tagged_nodes.ps1 -tag isLatencyCollector -accessKeyID AKI******** -secretAccessKeyID 1VVllumsz*** ```

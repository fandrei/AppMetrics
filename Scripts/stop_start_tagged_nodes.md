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
1. Enable running scripts: ```PS > Set-ExecutionPolicy -ExecutionPolicy RemoteSigned```


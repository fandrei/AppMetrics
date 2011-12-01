#Authentication
Using Basic authentication at this moment.
http://www.samuelotter.com/blog/2010/07/big-gotcha-with-custom-http-basic-authentication-in-wcf-services/
http://weblogs.asp.net/srkirkland/archive/2008/02/20/wcf-bindings-needed-for-https.aspx

#Deployment
Steps:
1. run script to build binaries and deployment package
2. Jenkins deploys it to the live server, in the temporary versioned location
3. run another script to run tests and check if everything is ok
4. reconfigure AppMetrics entry point in IIS to point to the latest installed version

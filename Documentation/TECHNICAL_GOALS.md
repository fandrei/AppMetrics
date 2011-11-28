#Technical goals

##Continuous deployment
This project aims to accelerate the "Build, Measure, Learn" feedback
loop by releasing every build to production.  This enables the smallest
possible batch size, and makes testing different variations as cheap as
possible.

The aim is to be able to concieve, develop, test, release and calculate
the impact on key metrics of a change within 1 day.

A failure in the deployment system should halt all other work until the
root cause has been corrected.

High quality is maintained via the following technical innovations:

###Automated deployment triggered by checkin
Every checkin to the release branch triggers a continuous integration 
build & production deployment.  Broken builds are not deployed.

###Phased release
There are always 2 versions of the software running.  v(n-1) is used by 90%
of the users.  v(n) is used by 10% of the users.  

###Immune system
Metrics on v(n-1) & v(n) are continuously collected.  After sufficient metrics
have been gathered to make a statistically significant measurement, v2
is either promoted all users, or retired.



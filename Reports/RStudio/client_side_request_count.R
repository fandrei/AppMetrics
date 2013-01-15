library("ggplot2")
library("RCurl")
library("plyr")
library("scales")
readRenviron("~/.Renviron") # Make sure you have a APPMETRICS_CREDENTIALS=username:password entry
metrics_credentials <- Sys.getenv(c("APPMETRICS_CREDENTIALS"))

application <- "CIAPILatencyCollector" 
startTime <- "2012-09-15 12:00:00"
endTime <- "2012-10-10 23:59:59"
data_url <- URLencode(paste("http://metrics.cityindex.com/GetRecords.ashx?Application=",application,"&StartTime=",startTime,'&EndTime=',endTime, sep = ''))

webpage <- getURL(data_url,userpwd=metrics_credentials, httpauth = 1L)
logdata <- read.table(tc <- textConnection(webpage), header=F, sep="\t", fill=TRUE, as.is=c(1,4)); 
close(tc)
colnames(logdata)=c('session','timestamp','key','value')

rpc3 <- logdata[logdata$"key" %in% c("Latency CIAPI.Trade"         
                                     ,"Latency CIAPI.ListOpenPositions"), ]
rpc3$datetime <- strptime(rpc3$timestamp, "%Y-%m-%d %H:%M:%S")
rpc3$key <- sub('Latency CIAPI.', '',rpc3$key)

d <- ggplot(rpc3, aes(x=datetime, y=as.double(value)))
d <- d + ylab("Latency in seconds") + xlab("Measurement time (UTC)")
d <- d + geom_point(alpha = 0.2)
d <- d + scale_fill_gradient(low="grey", high="red") + theme_bw()
d <- d + facet_grid(key ~ ., scales="free_y")
d <- d + opts(strip.text.y = theme_text( angle=0))
print(d)

rpc4 <- logdata[logdata$"key" %in% c("Latency CIAPI.Trade"), ]
rpc4$datetime <- strptime(rpc4$timestamp, "%Y-%m-%d %H:%M:%S")
rpc4$key <- sub('Latency CIAPI.', '',rpc4$key)
ggplot(rpc4, aes(x=datetime)) + geom_histogram(binwidth = 10000, colour="black", fill="white")

#Clean up source data
logdata$datetime <- strptime(logdata$timestamp, "%Y-%m-%d %H:%M:%S")
logdata$key <- sub('Latency CIAPI.', '',logdata$key)
logdata <- join(logdata, nodes, by = "session", match = "first")
logdata <- logdata[logdata$nodeName %in% c("AWS-EU-WEST-1B-79.125.25.36"),]

#A normal distribution
x <- seq(0, 0.6, length=1000)
hx <- dnorm(x,mean=0.3,sd=0.1)
plot(x,hx,type="l",lwd=2)

# ListNewsHeadlines
listNewsHeadlines <- logdata[logdata$"key" %in% c("ListNewsHeadlinesWithSource"), ]
newsHist <- ggplot(listNewsHeadlines, aes(x=as.numeric(value)))
newsHist <- newsHist + scale_x_continuous(limits=c(0.0, 0.6)) 
newsHist <- newsHist + geom_histogram(binwidth = 0.005)
print(newsHist)

# ListSpreadMarkets
listSpreadMarkets <- logdata[logdata$"key" %in% c("ListSpreadMarkets"), ]
spreadHist <- ggplot(listSpreadMarkets, aes(x=as.numeric(value)))
spreadHist <- spreadHist + scale_x_continuous(limits=c(0.0, 0.6)) 
spreadHist <- spreadHist + geom_histogram(binwidth = 0.005)
print(spreadHist)

# All
allRpc <- logdata[logdata$"key" %in% c("LogIn"
                                     ,"ListSpreadMarkets"
                                     ,"GetClientAndTradingAccount"     
                                     ,"GetMarketInformation"
                                     ,"GetPriceBars"
                                     ,"ListOpenPositions"
                                     ,"ListNewsHeadlinesWithSource"
                                     ,"ListTradeHistory"
                                     ,"Trade" ), ]
all <- ggplot(allRpc, aes(x=as.numeric(value)))
all <- all + scale_x_continuous(limits=c(0.0, 0.6)) 
all <- all + geom_histogram(binwidth = 0.005)
all <- all + facet_grid(key ~ nodeName, scales="free_y")
print(all)



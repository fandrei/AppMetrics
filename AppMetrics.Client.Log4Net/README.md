### Configuring AppMetrics and log4net integration

1. To send all Log4Net output to AppMetrics, add the following config elements to your App.Config file:

```
<configuration>
  <log4net>
    <appender name="AppMetrics" type="AppMetrics.Client.Log4Net.Log4NetAppender">
      <Server value="[ServerName]/LogEvent.ashx"/>
      <ApplicationKey value="[Name of your application]"/>
      <AccessKey value="[AppMetrics access key from your server settings]"/>
      <layout type="log4net.Layout.PatternLayout">
        <conversionPattern value="%utcdate %-5level - %message%newline" />
      </layout>
    </appender>
  </log4net>
</configuration>
```

1. When you need to send data both to log4net and AppMetrics, use the AppMetrics.Client.Log4Net.Log4NetTracker class instead of AppMetrics.Client.Tracker; it will ensure formatting is recognized by AppMetrics Analytics correctly.
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AppMetrics.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>AppMetrics server</title>
</head>
<body>
        <h1>AppMetrics server</h1>
        <ul>
        <li><a href="CreateUser.aspx">Create User</a> (this only works when accessed from localhost)</li>
        <li>Log some test data 
        <form action="LogEvent.ashx" method="POST" target="_blank">
            <table>
                <tr><th>MessageAppKey</th><td><input name="MessageAppKey" value="test"/></td></tr>
                <tr><th>MessageSession</th><td><input name="MessageSession" value="test-session"/></td></tr>
                <tr><th>MessagesList</th><td>
                    <textarea name="MessagesList" cols="60">2012-04-02 08:56:16.0527220|Latency TestMethod|0.0992977</textarea>
                    <br /><em>( | separated)</em>
                </td></tr>
                <tr><td colspan="2"><input type="submit" value="Post"/></td></tr>
            </table>
        </form>
        </li>
        <li>Access logged data
        <ul>
            <li><a href="GetSessions.ashx">Get sessions</a></li>
            <li>Get raw data records</a>
            <form action="GetRecords.ashx" method="GET" target="_blank">
                <table>
                <tr><th>Application</th><td><input name="Application" value="test"/></td></tr>
                <tr><th>StartTime</th><td><input name="StartTime" value="2012-04-02 08:00:00"/></td></tr>
                <tr><th>EndTime</th><td><input name="EndTime" value="2013-04-02 09:00:00"/></td></tr>
                <tr><td colspan="2"><input type="submit" value="Get Records"/></td></tr>
            </table>
            </form>
            </li>
        </ul>
        </li>
        </ul>
</body>
</html>

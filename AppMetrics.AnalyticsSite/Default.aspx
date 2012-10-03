<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AppMetrics.AnalyticsSite.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>AppMetrics Analytics server</title>
</head>
<body>
    <h1>
        AppMetrics Analytics server</h1>
    <h2>
        Generate report</h2>
        <a href="https://github.com/fandrei/AppMetrics/wiki/Creating-reports-on-collected-data">Full documentation on report types</a>
    <form action="GetReport.ashx" method="GET" target="_blank">
    <table>
        <tr>
            <th>
                Application
            </th>
            <td>
                <input name="Application" value="test" />
            </td>
        </tr>
        <tr>
            <th>
                Period
            </th>
            <td>
                <input name="Period" value="0.1:0:0" />
                <br />
                <em>in .NET Timespan format</em>
            </td>
        </tr>
        <tr>
            <th>
                Type
            </th>
            <td>
                <select name="Type">
                    <option value="LatencySummaries">Latency Summaries</option>
                    <option value="LatencyDistribution">Latency Distribution</option>
                    <option value="StreamingLatencySummaries">Streaming Latency Summaries</option>
                    <option value="StreamingLatencyDistribution">Streaming Latency Distribution</option>
                    <option value="StreamingLatencyDistribution">Streaming Latency Distribution</option>
                    <option value="JitterDistribution">Streaming Jitter Distribution</option>
                    <option value="Exceptions">Exceptions</option>
                </select>
            </td>
        </tr>
        <tr>
            <th>
                Slice By Location
            </th>
            <td>
                <select name="SliceByLocation">
                    <option value="Countries">Countries</option>
                    <option value="CountriesAndCities">Countries And Cities</option>
                    <option value="None">None</option>
                </select>
            </td>
        </tr>
        <tr>
            <th>
                Slice By Function
            </th>
            <td>
                <select name="SliceByFunction">
                    <option value="No">No</option>
                    <option value="Yes">Yes</option>
                </select>
            </td>
        </tr>
        <tr>
            <th>
                Slice By Node Name
            </th>
            <td>
                <select name="SliceByNodeName">
                    <option value="No">No</option>
                    <option value="Yes">Yes</option>
                </select>
            </td>
        </tr>
        <tr>
            <th>
                Filter by locations
            </th>
            <td>
                <input name="Locations" value="" />
                <br />
                <em>Location names are formatted like: CountryName/RegionName/CityName</em>
            </td>
        </tr>
        <tr>
            <th>
                Filter by function name
            </th>
            <td>
                <input name="FunctionFilter" value="" />
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <input type="submit" value="Generate report" />
            </td>
        </tr>
    </table>
    </form>
</body>
</html>

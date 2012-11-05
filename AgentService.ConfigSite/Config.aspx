<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Config.aspx.cs" Inherits="AppMetrics.AgentService.ConfigSite.Config" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>CIAPI Latency Collector Configuration</title>
    <style type="text/css" media="all">
        #NodesList td
        {
            padding-right: 20px;
        }
    </style>
</head>
<body>
    <h1>
        CIAPI Latency Collector Configuration</h1>
    <form id="form1" runat="server">
    <div>
        <table width="50%">
            <tr>
                <td>
                    <asp:Button ID="EnableButton" runat="server" Text="Enable polling" OnClick="EnableButton_Click" />
                </td>
                <td>
                    <asp:Button ID="DisableButton" runat="server" Text="Disable polling" OnClick="DisableButton_Click" />
                </td>
            </tr>
            <tr>
                <td>
                    &nbsp;
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Hyperlink runat="server" ID="CheckDataUrl">Check data being received</asp:Hyperlink>
                </td>
            </tr>
            <tr>
                <td>
                    &nbsp;
                </td>
            </tr>
            <tr>
                <td>
                    Nodes currently online:
                </td>
                <td>
                    <asp:Table ID="NodesList" runat="server" />
                </td>
            </tr>
        </table>
    </div>
    </form>
</body>
</html>

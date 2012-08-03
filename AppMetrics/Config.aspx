<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Config.aspx.cs" Inherits="AppMetrics.Config" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>AppMetrics Config</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <table width="50%">
            <tr>
                <td>
                    <asp:CheckBox ID="RequireAccessKey" runat="server" Text="Require access key" />
                </td>
            </tr>
            <tr>
                <td>
                    Access Keys (one per line)
                </td>
            </tr>
            <tr>
                <td>
                    <asp:TextBox ID="AccessKeysList" runat="server" Width="100%" 
                        TextMode="MultiLine" Rows="10"/>
                </td>
            </tr>
            <tr>
                <td align="right">
                    <asp:Button ID="OkButton" runat="server" Text="OK" OnClick="OkButton_Click" />
                </td>
            </tr>
        </table>
    </div>
    </form>
</body>
</html>

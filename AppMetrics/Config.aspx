<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Config.aspx.cs" Inherits="AppMetrics.Config" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <table>
            <tr>
                <td>Amazon access key</td>
                <td>
                    <asp:TextBox ID="AccessKeyEdit" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>Amazon secret access key</td>
                <td>
                    <asp:TextBox TextMode="Password" ID="SecretAccessKeyEdit" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td></td>
                <td align="right">
                    <asp:Button ID="OkButton" runat="server" Text="OK" OnClick="OkButton_Click" />
                </td>
            </tr>
        </table>
    </div>
    </form>
</body>
</html>

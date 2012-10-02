<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CreateUser.aspx.cs" Inherits="AppMetrics.CreateUser" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        Create a new user
        <table>
            <tr>
                <td>User name</td>
                <td>
                    <asp:TextBox ID="UserNameEdit" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>Password</td>
                <td>
                    <asp:TextBox TextMode="Password" ID="PasswordEdit" runat="server"></asp:TextBox>
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

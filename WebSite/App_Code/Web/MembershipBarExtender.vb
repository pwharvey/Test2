Imports MyCompany.Data
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Configuration
Imports System.Data
Imports System.Text
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.HtmlControls
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebParts

Namespace MyCompany.Web
    
    Partial Public Class MembershipBarExtender
        Inherits MembershipBarExtenderBase
    End Class
    
    <TargetControlType(GetType(HtmlGenericControl)),  _
     ToolboxItem(false)>  _
    Public Class MembershipBarExtenderBase
        Inherits AquariumExtenderBase
        
        Public Sub New()
            MyBase.New("Web.Membership")
        End Sub
        
        Protected Overrides Sub ConfigureDescriptor(ByVal descriptor As ScriptBehaviorDescriptor)
            descriptor.AddProperty("displayRememberMe", Properties("DisplayRememberMe"))
            descriptor.AddProperty("rememberMeSet", Properties("RememberMeSet"))
            descriptor.AddProperty("displaySignUp", Properties("DisplaySignUp"))
            descriptor.AddProperty("displayPasswordRecovery", Properties("DisplayPasswordRecovery"))
            descriptor.AddProperty("displayMyAccount", Properties("DisplayMyAccount"))
            Dim s As String = CType(Properties("Welcome"),String)
            If Not (String.IsNullOrEmpty(s)) Then
                descriptor.AddProperty("welcome", Properties("Welcome"))
            End If
            s = CType(Properties("User"),String)
            If Not (String.IsNullOrEmpty(s)) Then
                descriptor.AddProperty("user", Properties("User"))
            End If
            descriptor.AddProperty("displayHelp", Properties("DisplayHelp"))
            descriptor.AddProperty("enableHistory", Properties("EnableHistory"))
            descriptor.AddProperty("enablePermalinks", Properties("EnablePermalinks"))
            descriptor.AddProperty("displayLogin", Properties("DisplayLogin"))
            If Properties.ContainsKey("IdleUserTimeout") Then
                descriptor.AddProperty("idleTimeout", Properties("IdleUserTimeout"))
            End If
            Dim cultures As String = CType(Properties("Cultures"),String)
            If (cultures.Split(New Char() {Global.Microsoft.VisualBasic.ChrW(59)}, StringSplitOptions.RemoveEmptyEntries).Length > 1) Then
                descriptor.AddProperty("cultures", cultures)
            End If
        End Sub
        
        Protected Overrides Sub ConfigureScripts(ByVal scripts As List(Of ScriptReference))
            If EnableCombinedScript Then
                Return
            End If
            scripts.Add(CreateScriptReference("~/Scripts/daf-resources.js"))
            scripts.Add(CreateScriptReference("~/Scripts/daf-membership.js"))
        End Sub
    End Class
End Namespace

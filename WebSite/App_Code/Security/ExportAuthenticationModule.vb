Imports MyCompany.Services
Imports System
Imports System.Collections.Generic
Imports System.Configuration
Imports System.Security.Permissions
Imports System.Security.Principal
Imports System.Text
Imports System.Web
Imports System.Web.Security

Namespace MyCompany.Security
    
    <AspNetHostingPermission(SecurityAction.LinkDemand, Level:=AspNetHostingPermissionLevel.Minimal)>  _
    Partial Public Class ExportAuthenticationModule
        Inherits ExportAuthenticationModuleBase
    End Class
    
    Public Class ExportAuthenticationModuleBase
        Inherits Object
        Implements IHttpModule
        
        Sub IHttpModule_Init(ByVal context As HttpApplication) Implements IHttpModule.Init
            AddHandler context.AuthenticateRequest, AddressOf Me.contextAuthenticateRequest
            AddHandler context.EndRequest, AddressOf Me.contextEndRequest
        End Sub
        
        Sub IHttpModule_Dispose() Implements IHttpModule.Dispose
        End Sub
        
        Private Sub contextEndRequest(ByVal sender As Object, ByVal e As EventArgs)
            Dim app As HttpApplication = CType(sender,HttpApplication)
            If (app.Response.StatusCode = 401) Then
                RequestAuthentication(app)
            End If
        End Sub
        
        Private Sub contextAuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
            Dim app As HttpApplication = CType(sender,HttpApplication)
            Dim appServices As ApplicationServices = New ApplicationServices()
            If Not (appServices.RequiresAuthentication(app.Context.Request)) Then
                Return
            End If
            If appServices.AuthenticateRequest(app.Context) Then
                Return
            End If
            Dim c As HttpCookie = app.Request.Cookies(FormsAuthentication.FormsCookieName)
            If (Not (c) Is Nothing) Then
                Dim t As FormsAuthenticationTicket = FormsAuthentication.Decrypt(c.Value)
                If Not (String.IsNullOrEmpty(t.Name)) Then
                    Return
                End If
            End If
            Dim authorization As String = app.Request.Headers("Authorization")
            If String.IsNullOrEmpty(authorization) Then
                RequestAuthentication(app)
            Else
                If authorization.StartsWith("Basic", StringComparison.CurrentCultureIgnoreCase) Then
                    ValidateUserIdentity(app, authorization)
                Else
                    RequestAuthentication(app)
                End If
            End If
        End Sub
        
        Private Sub RequestAuthentication(ByVal app As HttpApplication)
            Dim appServices As ApplicationServices = New ApplicationServices()
            app.Response.AppendHeader("WWW-Authenticate", String.Format("Basic realm=""{0}""", appServices.Realm))
            app.Response.StatusCode = 401
            app.CompleteRequest()
        End Sub
        
        Private Sub ValidateUserIdentity(ByVal app As HttpApplication, ByVal authorization As String)
            Dim login() As String = Encoding.Default.GetString(Convert.FromBase64String(authorization.Substring(6))).Split(New Char() {Global.Microsoft.VisualBasic.ChrW(58)}, 2)
            If Membership.ValidateUser(login(0), login(1)) Then
                app.Context.User = New RolePrincipal(New FormsIdentity(New FormsAuthenticationTicket(login(0), false, 10)))
            Else
                app.Response.StatusCode = 401
                app.Response.StatusDescription = "Access Denied"
                app.Response.Write("Access denied. Please enter a valid user name and password.")
                app.CompleteRequest()
            End If
        End Sub
    End Class
End Namespace

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Threading
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.HtmlControls

Namespace MyCompany.Data
    
    Public Class CultureManager
        
        Public Const AutoDetectCulture As String = "Detect,Detect"
        
        Public Shared SupportedCultures() As String = New String() {"en-NZ,en-GB"}
        
        Public Shared Sub Initialize()
            Dim ctx As HttpContext = HttpContext.Current
            If ((ctx Is Nothing) OrElse (Not (ctx.Items("CultureManager_Initialized")) Is Nothing)) Then
                Return
            End If
            ctx.Items("CultureManager_Initialized") = true
            Dim cultureCookie As HttpCookie = ctx.Request.Cookies(".COTCULTURE")
            Dim culture As String = Nothing
            If (Not (cultureCookie) Is Nothing) Then
                culture = cultureCookie.Value
            End If
            If (String.IsNullOrEmpty(culture) OrElse (culture = CultureManager.AutoDetectCulture)) Then
                If (Not (ctx.Request.UserLanguages) Is Nothing) Then
                    For Each l As String in ctx.Request.UserLanguages
                        Dim languageInfo() As String = l.Split(Global.Microsoft.VisualBasic.ChrW(59))
                        For Each c As String in SupportedCultures
                            If c.StartsWith(languageInfo(0)) Then
                                culture = c
                                Exit For
                            End If
                        Next
                        If (Not (culture) Is Nothing) Then
                            Exit For
                        End If
                    Next
                Else
                    culture = SupportedCultures(0)
                End If
            End If
            If Not (String.IsNullOrEmpty(culture)) Then
                Dim cultureIndex As Integer = Array.IndexOf(SupportedCultures, culture)
                If Not ((cultureIndex = -1)) Then
                    Dim ci() As String = culture.Split(Global.Microsoft.VisualBasic.ChrW(44))
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci(0))
                    Thread.CurrentThread.CurrentUICulture = New CultureInfo(ci(1))
                    If TypeOf ctx.Handler Is Page Then
                        Dim p As Page = CType(ctx.Handler,Page)
                        p.Culture = ci(0)
                        p.UICulture = ci(1)
                        If (Not (cultureCookie) Is Nothing) Then
                            If (cultureCookie.Value = CultureManager.AutoDetectCulture) Then
                                cultureCookie.Expires = DateTime.Now.AddDays(-14)
                            Else
                                cultureCookie.Expires = DateTime.Now.AddDays(14)
                            End If
                            ctx.Response.AppendCookie(cultureCookie)
                        End If
                    End If
                End If
            End If
        End Sub
        
        Public Overloads Shared Function ResolveEmbeddedResourceName(ByVal resourceName As String, ByVal culture As String) As String
            Return ResolveEmbeddedResourceName(GetType(CultureManager).Assembly, resourceName, culture)
        End Function
        
        Public Overloads Shared Function ResolveEmbeddedResourceName(ByVal resourceName As String) As String
            Return ResolveEmbeddedResourceName(GetType(CultureManager).Assembly, resourceName, Thread.CurrentThread.CurrentUICulture.Name)
        End Function
        
        Public Overloads Shared Function ResolveEmbeddedResourceName(ByVal a As [Assembly], ByVal resourceName As String, ByVal culture As String) As String
            Dim extension As String = Path.GetExtension(resourceName)
            Dim fileName As String = Path.GetFileNameWithoutExtension(resourceName)
            Dim localizedResourceName As String = String.Format("{0}.{1}{2}", fileName, culture.Replace("-", "_"), extension)
            Dim mri As ManifestResourceInfo = a.GetManifestResourceInfo(localizedResourceName)
            If (mri Is Nothing) Then
                If culture.Contains("-") Then
                    localizedResourceName = String.Format("{0}.{1}_{2}", fileName, culture.Substring(0, culture.LastIndexOf("-")).Replace("-", "_"), extension)
                Else
                    localizedResourceName = String.Format("{0}.{1}_{2}", fileName, culture, extension)
                End If
                mri = a.GetManifestResourceInfo(localizedResourceName)
            End If
            If (mri Is Nothing) Then
                localizedResourceName = resourceName
            End If
            Return localizedResourceName
        End Function
    End Class
    
    Public Class GenericHandlerBase
        
        Public Sub New()
            MyBase.New
            CultureManager.Initialize()
        End Sub
        
        Protected Overridable Function GenerateOutputFileName(ByVal args As ActionArgs, ByVal outputFileName As String) As String
            args.CommandArgument = args.CommandName
            args.CommandName = "FileName"
            Dim values As List(Of FieldValue) = New List(Of FieldValue)()
            values.Add(New FieldValue("FileName", outputFileName))
            args.Values = values.ToArray()
            Dim result As ActionResult = ControllerFactory.CreateDataController().Execute(args.Controller, args.View, args)
            For Each v As FieldValue in result.Values
                If (v.Name = "FileName") Then
                    outputFileName = Convert.ToString(v.Value)
                    Exit For
                End If
            Next
            Return outputFileName
        End Function
        
        Protected Overridable Sub AppendDownloadTokenCookie()
            Dim context As HttpContext = HttpContext.Current
            Dim downloadToken As String = "APPFACTORYDOWNLOADTOKEN"
            Dim tokenCookie As HttpCookie = context.Request.Cookies(downloadToken)
            If (tokenCookie Is Nothing) Then
                tokenCookie = New HttpCookie(downloadToken)
            End If
            tokenCookie.Value = String.Format("{0},{1}", tokenCookie.Value, Guid.NewGuid())
            context.Response.AppendCookie(tokenCookie)
        End Sub
    End Class
End Namespace

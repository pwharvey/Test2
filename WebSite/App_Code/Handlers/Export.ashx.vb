Imports MyCompany.Data
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Xml
Imports System.Xml.XPath

Namespace MyCompany.Handlers
    
    Public Class Export
        Inherits GenericHandlerBase
        Implements IHttpHandler, System.Web.SessionState.IRequiresSessionState
        
        ReadOnly Property IHttpHandler_IsReusable() As Boolean Implements IHttpHandler.IsReusable
            Get
                Return true
            End Get
        End Property
        
        Sub IHttpHandler_ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
            Dim fileName As String = Nothing
            Dim q As String = context.Request.Params("q")
            If Not (String.IsNullOrEmpty(q)) Then
                If q.Contains("{") Then
                    q = Convert.ToBase64String(Encoding.Default.GetBytes(q))
                    context.Response.Redirect(((context.Request.AppRelativeCurrentExecutionFilePath + "?q=")  _
                                    + HttpUtility.UrlEncode(q)))
                End If
                q = Encoding.Default.GetString(Convert.FromBase64String(q))
                '
                Dim serializer As System.Web.Script.Serialization.JavaScriptSerializer = New System.Web.Script.Serialization.JavaScriptSerializer()
                '
                Dim args As ActionArgs = serializer.Deserialize(Of ActionArgs)(q)
                'execute data export
                Dim controller As IDataController = ControllerFactory.CreateDataController()
                'create an Excel Web Query
                If ((args.CommandName = "ExportRowset") AndAlso Not (context.Request.Url.AbsoluteUri.Contains("&d"))) Then
                    Dim webQueryUrl As String = (context.Request.Url.AbsoluteUri + "&d")
                    context.Response.Write(("Web" & ControlChars.CrLf &"1" & ControlChars.CrLf  + webQueryUrl))
                    context.Response.ContentType = "text/x-ms-iqy"
                    context.Response.AddHeader("Content-Disposition", String.Format(String.Format("attachment; filename={0}", GenerateOutputFileName(args, String.Format("{0}_{1}.iqy", args.Controller, args.View)))))
                    Return
                End If
                'export data in the requested format
                Dim result As ActionResult = controller.Execute(args.Controller, args.View, args)
                fileName = CType(result.Values(0).Value,String)
                'send file to output
                If File.Exists(fileName) Then
                    If (args.CommandName = "ExportCsv") Then
                        context.Response.ContentType = "text/csv"
                        context.Response.AddHeader("Content-Disposition", String.Format(String.Format("attachment; filename={0}", GenerateOutputFileName(args, String.Format("{0}_{1}.csv", args.Controller, args.View)))))
                        context.Response.Charset = "utf-8"
                        context.Response.Write(Convert.ToChar(65279))
                    Else
                        If (args.CommandName = "ExportRowset") Then
                            context.Response.ContentType = "text/xml"
                        Else
                            context.Response.ContentType = "application/rss+xml"
                        End If
                    End If
                    Dim reader As StreamReader = File.OpenText(fileName)
                    Do While Not (reader.EndOfStream)
                        Dim s As String = reader.ReadLine()
                        context.Response.Output.WriteLine(s)
                    Loop
                    reader.Close()
                    File.Delete(fileName)
                End If
            End If
            If String.IsNullOrEmpty(fileName) Then
                Throw New HttpException(404, String.Empty)
            End If
        End Sub
    End Class
End Namespace

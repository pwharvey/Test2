Imports Microsoft.Reporting.WebForms
Imports MyCompany.Data
Imports MyCompany.Web
Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Web.Caching
Imports System.Xml
Imports System.Xml.XPath
Imports System.Xml.Xsl

Namespace MyCompany.Handlers
    
    ''' <summary>
    ''' A collection of parameters controlling the process or report generation.
    ''' </summary>
    Public Class ReportArgs
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Controller As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_View As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_TemplateName As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Format As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_FilterDetails As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_SortExpression As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Filter() As FieldFilter
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_MimeType As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_FileNameExtension As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Encoding As String
        
        Public Sub New()
            MyBase.New
            View = "grid1"
        End Sub
        
        ''' <summary>
        ''' The name of the data controller
        ''' </summary>
        Public Property Controller() As String
            Get
                Return Me.m_Controller
            End Get
            Set
                Me.m_Controller = value
            End Set
        End Property
        
        ''' <summary>
        ''' The ID of the view. Optional.
        ''' </summary>
        Public Property View() As String
            Get
                Return Me.m_View
            End Get
            Set
                Me.m_View = value
            End Set
        End Property
        
        ''' <summary>
        ''' The name of a custom RDLC template. Optional.
        ''' </summary>
        Public Property TemplateName() As String
            Get
                Return Me.m_TemplateName
            End Get
            Set
                Me.m_TemplateName = value
            End Set
        End Property
        
        ''' <summary>
        ''' Report output format. Supported values are Pdf, Word, Excel, and Tiff. The default value is Pdf. Optional.
        ''' </summary>
        Public Property Format() As String
            Get
                Return Me.m_Format
            End Get
            Set
                Me.m_Format = value
            End Set
        End Property
        
        ''' <summary>
        ''' Specifies a user-friendly description of the filter. The description is displayed on the automatically produced reports below the report header. Optional.
        ''' </summary>
        Public Property FilterDetails() As String
            Get
                Return Me.m_FilterDetails
            End Get
            Set
                Me.m_FilterDetails = value
            End Set
        End Property
        
        ''' <summary>
        ''' Sort expression that must be applied to the dataset prior to the report generation. Optional.
        ''' </summary>
        Public Property SortExpression() As String
            Get
                Return Me.m_SortExpression
            End Get
            Set
                Me.m_SortExpression = value
            End Set
        End Property
        
        ''' <summary>
        ''' A filter expression that must be applied to the dataset prior to the report generation. Optional.
        ''' </summary>
        Public Property Filter() As FieldFilter()
            Get
                Return Me.m_Filter
            End Get
            Set
                Me.m_Filter = value
            End Set
        End Property
        
        ''' <summary>
        ''' Specifies the MIME type of the report produced by Report.Execute() method.
        ''' </summary>
        Public Property MimeType() As String
            Get
                Return Me.m_MimeType
            End Get
            Set
                Me.m_MimeType = value
            End Set
        End Property
        
        ''' <summary>
        ''' Specifies the file name extension of the report produced by Report.Execute() method.
        ''' </summary>
        Public Property FileNameExtension() As String
            Get
                Return Me.m_FileNameExtension
            End Get
            Set
                Me.m_FileNameExtension = value
            End Set
        End Property
        
        ''' <summary>
        ''' Specifies the encoding of the report produced by Report.Execute() method.
        ''' </summary>
        Public Property Encoding() As String
            Get
                Return Me.m_Encoding
            End Get
            Set
                Me.m_Encoding = value
            End Set
        End Property
    End Class
    
    Partial Public Class Report
        Inherits ReportBase
        
        ''' <summary>
        ''' Generates a report using the default or custom report template with optional sort expression and filter applied to the dataset.
        ''' </summary>
        ''' <param name="args">A collection of parameters that control the report generation.</param>
        ''' <returns>A binary array representing the report data.</returns>
        Public Shared Function Execute(ByVal args As ReportArgs) As Byte()
            Dim reportHandler As Report = New Report()
            Dim output As Stream = New MemoryStream()
            reportHandler.OutputStream = output
            reportHandler.Arguments = args
            Dim request As PageRequest = New PageRequest()
            reportHandler.Request = request
            request.Controller = args.Controller
            request.View = args.View
            request.SortExpression = args.SortExpression
            If (Not (args.Filter) Is Nothing) Then
                Dim dve As DataViewExtender = New DataViewExtender()
                dve.AssignStartupFilter(args.Filter)
                request.Filter = CType(dve.Properties("StartupFilter"),List(Of String)).ToArray()
            End If
            CType(reportHandler,IHttpHandler).ProcessRequest(HttpContext.Current)
            'return report data
            output.Position = 0
            Dim data((output.Length) - 1) As Byte
            output.Read(data, 0, data.Length)
            Return data
        End Function
    End Class
    
    Public Class ReportBase
        Inherits GenericHandlerBase
        Implements IHttpHandler, System.Web.SessionState.IRequiresSessionState
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Arguments As ReportArgs
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Request As PageRequest
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_OutputStream As Stream
        
        Private Shared m_ValidationKeyRegex As Regex = New Regex("/Blob.ashx\?")
        
        Protected Property Arguments() As ReportArgs
            Get
                Return Me.m_Arguments
            End Get
            Set
                Me.m_Arguments = value
            End Set
        End Property
        
        Protected Property Request() As PageRequest
            Get
                Return Me.m_Request
            End Get
            Set
                Me.m_Request = value
            End Set
        End Property
        
        Protected Property OutputStream() As Stream
            Get
                Return Me.m_OutputStream
            End Get
            Set
                Me.m_OutputStream = value
            End Set
        End Property
        
        ReadOnly Property IHttpHandler_IsReusable() As Boolean Implements IHttpHandler.IsReusable
            Get
                Return false
            End Get
        End Property
        
        Sub IHttpHandler_ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
            Dim c As String = context.Request("c")
            Dim q As String = context.Request("q")
            Dim request As PageRequest = Me.Request
            If ((request Is Nothing) AndAlso (String.IsNullOrEmpty(c) OrElse String.IsNullOrEmpty(q))) Then
                Throw New Exception("Invalid report request.")
            End If
            '
            Dim serializer As System.Web.Script.Serialization.JavaScriptSerializer = New System.Web.Script.Serialization.JavaScriptSerializer()
            '
            'create a data table for report
            Dim templateName As String = Nothing
            Dim aa As String = Nothing
            Dim reportFormat As String = Nothing
            If (request Is Nothing) Then
                request = serializer.Deserialize(Of PageRequest)(q)
                templateName = context.Request.Form("a")
                aa = context.Request("aa")
            Else
                templateName = Me.Arguments.TemplateName
                reportFormat = Me.Arguments.Format
                request.FilterDetails = Me.Arguments.FilterDetails
            End If
            request.PageIndex = 0
            request.PageSize = Int32.MaxValue
            request.RequiresMetaData = true
            'try to generate a report via a business rule
            Dim args As ActionArgs = Nothing
            If Not (String.IsNullOrEmpty(aa)) Then
                args = serializer.Deserialize(Of ActionArgs)(aa)
                Dim controller As IDataController = ControllerFactory.CreateDataController()
                Dim result As ActionResult = controller.Execute(args.Controller, args.View, args)
                If Not (String.IsNullOrEmpty(result.NavigateUrl)) Then
                    AppendDownloadTokenCookie()
                    context.Response.Redirect(result.NavigateUrl)
                End If
                If result.Canceled Then
                    AppendDownloadTokenCookie()
                    Return
                End If
                result.RaiseExceptionIfErrors()
                'parse action data
                Dim actionData As SortedDictionary(Of String, String) = New SortedDictionary(Of String, String)()
                CType(controller,DataControllerBase).Config.ParseActionData(args.Path, actionData)
                Dim filter As List(Of String) = New List(Of String)()
                For Each name As String in actionData.Keys
                    Dim v As String = actionData(name)
                    If name.StartsWith("_") Then
                        If (name = "_controller") Then
                            request.Controller = v
                        End If
                        If (name = "_view") Then
                            request.View = v
                        End If
                        If (name = "_sortExpression") Then
                            request.SortExpression = v
                        End If
                        If (name = "_count") Then
                            request.PageSize = Convert.ToInt32(v)
                        End If
                        If (name = "_template") Then
                            templateName = v
                        End If
                    Else
                        If (v = "@Arguments_SelectedValues") Then
                            If ((Not (args.SelectedValues) Is Nothing) AndAlso (args.SelectedValues.Length > 0)) Then
                                Dim sb As StringBuilder = New StringBuilder()
                                For Each key As String in args.SelectedValues
                                    If (sb.Length > 0) Then
                                        sb.Append("$or$")
                                    End If
                                    sb.Append(key)
                                Next
                                filter.Add(String.Format("{0}:$in${1}", name, sb.ToString()))
                            Else
                                Return
                            End If
                        Else
                            If Regex.IsMatch(v, "^('|"").+('|"")$") Then
                                filter.Add(String.Format("{0}:={1}", name, v.Substring(1, (v.Length - 2))))
                            Else
                                If (Not (args.Values) Is Nothing) Then
                                    For Each fv As FieldValue in args.Values
                                        If (fv.Name = v) Then
                                            filter.Add(String.Format("{0}:={1}", name, fv.Value))
                                        End If
                                    Next
                                End If
                            End If
                        End If
                    End If
                    request.Filter = filter.ToArray()
                Next
            End If
            'load report definition
            Dim reportTemplate As String = Controller.CreateReportInstance(Nothing, templateName, request.Controller, request.View)
            Dim page As ViewPage = ControllerFactory.CreateDataController().GetPage(request.Controller, request.View, request)
            Dim table As DataTable = page.ToDataTable()
            'insert validation key
            reportTemplate = m_ValidationKeyRegex.Replace(reportTemplate, String.Format("/Blob.ashx?_validationKey={0}&amp;", BlobAdapter.ValidationKey))
            'figure report output format
            If (Me.Arguments Is Nothing) Then
                Dim m As Match = Regex.Match(c, "^(ReportAs|Report)(Pdf|Excel|Image|Word|)$")
                reportFormat = m.Groups(2).Value
            End If
            If String.IsNullOrEmpty(reportFormat) Then
                reportFormat = "Pdf"
            End If
            'render a report
            Dim mimeType As String = Nothing
            Dim encoding As String = Nothing
            Dim fileNameExtension As String = Nothing
            Dim streams() As String = Nothing
            Dim warnings() As Warning = Nothing
            Using report As LocalReport = New LocalReport()
                report.EnableHyperlinks = true
                report.EnableExternalImages = true
                report.LoadReportDefinition(New StringReader(reportTemplate))
                report.DataSources.Add(New ReportDataSource(request.Controller, table))
                report.EnableExternalImages = true
                For Each p As ReportParameterInfo in report.GetParameters()
                    If (p.Name.Equals("FilterDetails") AndAlso Not (String.IsNullOrEmpty(request.FilterDetails))) Then
                        report.SetParameters(New ReportParameter("FilterDetails", request.FilterDetails))
                    End If
                    If p.Name.Equals("BaseUrl") Then
                        Dim baseUrl As String = String.Format("{0}://{1}{2}", context.Request.Url.Scheme, context.Request.Url.Authority, context.Request.ApplicationPath.TrimEnd(Global.Microsoft.VisualBasic.ChrW(47)))
                        report.SetParameters(New ReportParameter("BaseUrl", baseUrl))
                    End If
                    If (p.Name.Equals("Query") AndAlso Not (String.IsNullOrEmpty(q))) Then
                        report.SetParameters(New ReportParameter("Query", HttpUtility.UrlEncode(q)))
                    End If
                Next
                report.SetBasePermissionsForSandboxAppDomain(New System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted))
                Dim reportData() As Byte = report.Render(reportFormat, Nothing, mimeType, encoding, fileNameExtension, streams, warnings)
                If (Not (Me.Arguments) Is Nothing) Then
                    Me.Arguments.MimeType = mimeType
                    Me.Arguments.FileNameExtension = fileNameExtension
                    Me.Arguments.Encoding = encoding
                    Me.OutputStream.Write(reportData, 0, reportData.Length)
                Else
                    'send report data to the client
                    context.Response.Clear()
                    context.Response.ContentType = mimeType
                    context.Response.AddHeader("Content-Length", reportData.Length.ToString())
                    AppendDownloadTokenCookie()
                    Dim fileName As String = FormatFileName(context, request, fileNameExtension)
                    If String.IsNullOrEmpty(fileName) Then
                        fileName = String.Format("{0}_{1}.{2}", request.Controller, request.View, fileNameExtension)
                        If (Not (args) Is Nothing) Then
                            fileName = GenerateOutputFileName(args, fileName)
                        End If
                    End If
                    context.Response.AddHeader("Content-Disposition", String.Format("attachment;filename={0}", fileName))
                    context.Response.OutputStream.Write(reportData, 0, reportData.Length)
                End If
            End Using
        End Sub
        
        Protected Overridable Function FormatFileName(ByVal context As HttpContext, ByVal request As PageRequest, ByVal extension As String) As String
            Return Nothing
        End Function
    End Class
End Namespace

Imports MyCompany.Data
Imports MyCompany.Handlers
Imports MyCompany.Web
Imports System
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Configuration
Imports System.Data
Imports System.Data.Common
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.IO.Compression
Imports System.Linq
Imports System.Net
Imports System.Security.Principal
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Web
Imports System.Web.Caching
Imports System.Web.Configuration
Imports System.Web.Routing
Imports System.Web.Security
Imports System.Web.SessionState
Imports System.Web.UI
Imports System.Web.UI.HtmlControls

Namespace MyCompany.Services
    
    Public Class ServiceRequestError
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_ExceptionType As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Message As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_StackTrace As String
        
        Public Property ExceptionType() As String
            Get
                Return Me.m_ExceptionType
            End Get
            Set
                Me.m_ExceptionType = value
            End Set
        End Property
        
        Public Property Message() As String
            Get
                Return Me.m_Message
            End Get
            Set
                Me.m_Message = value
            End Set
        End Property
        
        Public Property StackTrace() As String
            Get
                Return Me.m_StackTrace
            End Get
            Set
                Me.m_StackTrace = value
            End Set
        End Property
    End Class
    
    Public Class WorkflowResources
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_StaticResources As SortedDictionary(Of String, String)
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_DynamicResources As List(Of Regex)
        
        Public Sub New()
            MyBase.New
            m_StaticResources = New SortedDictionary(Of String, String)()
            m_DynamicResources = New List(Of Regex)()
        End Sub
        
        Public Property StaticResources() As SortedDictionary(Of String, String)
            Get
                Return Me.m_StaticResources
            End Get
            Set
                Me.m_StaticResources = value
            End Set
        End Property
        
        Public Property DynamicResources() As List(Of Regex)
            Get
                Return Me.m_DynamicResources
            End Get
            Set
                Me.m_DynamicResources = value
            End Set
        End Property
    End Class
    
    Partial Public Class WorkflowRegister
        Inherits WorkflowRegisterBase
    End Class
    
    Public Class WorkflowRegisterBase
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Resources As SortedDictionary(Of String, WorkflowResources)
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_RoleRegister As SortedDictionary(Of String, List(Of String))
        
        Public Sub New()
            MyBase.New
            'initialize system workflows
            m_Resources = New SortedDictionary(Of String, WorkflowResources)()
            RegisterBuiltinWorkflowResources()
            For Each w As SiteContentFile in ApplicationServices.Current.ReadSiteContent("sys/workflows%", "%")
                Dim text As String = w.Text
                If Not (String.IsNullOrEmpty(text)) Then
                    Dim wr As WorkflowResources = Nothing
                    If Not (Resources.TryGetValue(w.PhysicalName, wr)) Then
                        wr = New WorkflowResources()
                        Resources(w.PhysicalName) = wr
                    End If
                    For Each s As String in text.Split(New Char() {Global.Microsoft.VisualBasic.ChrW(10)}, StringSplitOptions.RemoveEmptyEntries)
                        Dim query As String = s.Trim()
                        If Not (String.IsNullOrEmpty(query)) Then
                            If s.StartsWith("regex ") Then
                                Dim regexQuery As String = s.Substring(6).Trim()
                                If Not (String.IsNullOrEmpty(regexQuery)) Then
                                    Try 
                                        wr.DynamicResources.Add(New Regex(regexQuery, RegexOptions.IgnoreCase))
                                    Catch __exception As Exception
                                    End Try
                                End If
                            Else
                                wr.StaticResources(query.ToLower()) = query
                            End If
                        End If
                    Next
                End If
            Next
            'read "role" workflows from the register
            m_RoleRegister = New SortedDictionary(Of String, List(Of String))()
            For Each rr As SiteContentFile in ApplicationServices.Current.ReadSiteContent("sys/register/roles%", "%")
                Dim text As String = rr.Text
                If Not (String.IsNullOrEmpty(text)) Then
                    Dim workflows As List(Of String) = Nothing
                    If Not (RoleRegister.TryGetValue(rr.PhysicalName, workflows)) Then
                        workflows = New List(Of String)()
                        RoleRegister(rr.PhysicalName) = workflows
                    End If
                    For Each s As String in text.Split(New Char() {Global.Microsoft.VisualBasic.ChrW(10), Global.Microsoft.VisualBasic.ChrW(44)}, StringSplitOptions.RemoveEmptyEntries)
                        Dim name As String = s.Trim()
                        If Not (String.IsNullOrEmpty(name)) Then
                            workflows.Add(name)
                        End If
                    Next
                End If
            Next
        End Sub
        
        Public Property Resources() As SortedDictionary(Of String, WorkflowResources)
            Get
                Return Me.m_Resources
            End Get
            Set
                Me.m_Resources = value
            End Set
        End Property
        
        Public Property RoleRegister() As SortedDictionary(Of String, List(Of String))
            Get
                Return Me.m_RoleRegister
            End Get
            Set
                Me.m_RoleRegister = value
            End Set
        End Property
        
        Public ReadOnly Property UserWorkflows() As List(Of String)
            Get
                Dim workflows As List(Of String) = CType(HttpContext.Current.Items("WorkflowRegister_UserWorkflows"),List(Of String))
                If (workflows Is Nothing) Then
                    workflows = New List(Of String)()
                    Dim identity As IIdentity = HttpContext.Current.User.Identity
                    If identity.IsAuthenticated Then
                        For Each urf As SiteContentFile in ApplicationServices.Current.ReadSiteContent("sys/register/users%", identity.Name)
                            Dim text As String = urf.Text
                            If Not (String.IsNullOrEmpty(text)) Then
                                For Each s As String in text.Split(New Char() {Global.Microsoft.VisualBasic.ChrW(10), Global.Microsoft.VisualBasic.ChrW(44)}, StringSplitOptions.RemoveEmptyEntries)
                                    Dim name As String = s.Trim()
                                    If (Not (String.IsNullOrEmpty(name)) AndAlso Not (workflows.Contains(name))) Then
                                        workflows.Add(name)
                                    End If
                                Next
                            End If
                        Next
                    End If
                    'enumerate role workflows
                    Dim isAuthenticated As Boolean = HttpContext.Current.User.Identity.IsAuthenticated
                    For Each role As String in RoleRegister.Keys
                        If ((((role = "?") AndAlso Not (isAuthenticated)) OrElse ((role = "*") AndAlso isAuthenticated)) OrElse DataControllerBase.UserIsInRole(role)) Then
                            For Each name As String in RoleRegister(role)
                                If Not (workflows.Contains(name)) Then
                                    workflows.Add(name)
                                End If
                            Next
                        End If
                    Next
                    HttpContext.Current.Items("WorkflowRegister_UserWorkflows") = workflows
                End If
                Return workflows
            End Get
        End Property
        
        Public ReadOnly Property Enabled() As Boolean
            Get
                Return (m_Resources.Count > 0)
            End Get
        End Property
        
        Public Shared ReadOnly Property IsEnabled() As Boolean
            Get
                If Not (ApplicationServices.IsSiteContentEnabled) Then
                    Return false
                End If
                Dim wr As WorkflowRegister = WorkflowRegister.GetCurrent()
                Return ((Not (wr) Is Nothing) AndAlso wr.Enabled)
            End Get
        End Property
        
        Public Overridable ReadOnly Property CacheDuration() As Integer
            Get
                Return 30
            End Get
        End Property
        
        Protected Overridable Sub RegisterBuiltinWorkflowResources()
        End Sub
        
        Public Shared Function Allows(ByVal fileName As String) As Boolean
            If Not (ApplicationServices.IsSiteContentEnabled) Then
                Return false
            End If
            Dim wr As WorkflowRegister = WorkflowRegister.GetCurrent(fileName)
            If ((wr Is Nothing) OrElse Not (wr.Enabled)) Then
                Return false
            End If
            Return wr.IsMatch(fileName)
        End Function
        
        Public Overloads Function IsMatch(ByVal physicalPath As String, ByVal physicalName As String) As Boolean
            Dim fileName As String = physicalPath
            If String.IsNullOrEmpty(fileName) Then
                fileName = physicalName
            Else
                fileName = ((fileName + "/")  _
                            + physicalName)
            End If
            Return IsMatch(fileName)
        End Function
        
        Public Overloads Function IsMatch(ByVal fileName As String) As Boolean
            fileName = fileName.ToLower()
            Dim activeWorkflows As List(Of String) = UserWorkflows
            For Each workflow As String in activeWorkflows
                Dim resourceList As WorkflowResources = Nothing
                If Resources.TryGetValue(workflow, resourceList) Then
                    If resourceList.StaticResources.ContainsKey(fileName) Then
                        Return true
                    End If
                    For Each re As Regex in resourceList.DynamicResources
                        If re.IsMatch(fileName) Then
                            Return true
                        End If
                    Next
                End If
            Next
            Return false
        End Function
        
        Public Overloads Shared Function GetCurrent() As WorkflowRegister
            Return GetCurrent(Nothing)
        End Function
        
        Public Overloads Shared Function GetCurrent(ByVal relativePath As String) As WorkflowRegister
            If ((Not (relativePath) Is Nothing) AndAlso (relativePath.StartsWith("sys/workflows") OrElse relativePath.StartsWith("sys/register"))) Then
                Return Nothing
            End If
            Dim key As String = "WorkflowRegister_Current"
            Dim context As HttpContext = HttpContext.Current
            Dim instance As WorkflowRegister = CType(context.Items(key),WorkflowRegister)
            If (instance Is Nothing) Then
                instance = CType(context.Cache(key),WorkflowRegister)
                If (instance Is Nothing) Then
                    instance = New WorkflowRegister()
                    context.Cache.Add(key, instance, Nothing, DateTime.Now.AddSeconds(instance.CacheDuration), Cache.NoSlidingExpiration, CacheItemPriority.AboveNormal, Nothing)
                End If
                context.Items(key) = instance
            End If
            Return instance
        End Function
    End Class
    
    Public Enum SiteContentFields
        
        SiteContentId
        
        DataFileName
        
        DataContentType
        
        Path
        
        Data
        
        Roles
        
        Users
        
        Text
        
        CacheProfile
        
        RoleExceptions
        
        UserExceptions
        
        Schedule
        
        ScheduleExceptions
    End Enum
    
    Public Class SiteContentFile
        
        Private m_Id As Object
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Name As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Path As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_ContentType As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Data() As Byte
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_PhysicalName As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Error As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Schedule As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_ScheduleExceptions As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_CacheProfile As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_CacheDuration As Integer
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_CacheLocation As HttpCacheability
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_CacheVaryByParams() As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_CacheVaryByHeaders() As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_CacheNoStore As Boolean
        
        Public Sub New()
            MyBase.New
            Me.CacheLocation = HttpCacheability.NoCache
        End Sub
        
        Public Property Id() As Object
            Get
                Return m_Id
            End Get
            Set
                If ((Not (value) Is Nothing) AndAlso TypeOf value Is Byte) Then
                    value = New Guid(CType(value,Byte()))
                End If
                m_Id = value
            End Set
        End Property
        
        Public Property Name() As String
            Get
                Return Me.m_Name
            End Get
            Set
                Me.m_Name = value
            End Set
        End Property
        
        Public Property Path() As String
            Get
                Return Me.m_Path
            End Get
            Set
                Me.m_Path = value
            End Set
        End Property
        
        Public Property ContentType() As String
            Get
                Return Me.m_ContentType
            End Get
            Set
                Me.m_ContentType = value
            End Set
        End Property
        
        Public Property Data() As Byte()
            Get
                Return Me.m_Data
            End Get
            Set
                Me.m_Data = value
            End Set
        End Property
        
        Public Property PhysicalName() As String
            Get
                Return Me.m_PhysicalName
            End Get
            Set
                Me.m_PhysicalName = value
            End Set
        End Property
        
        Public Property [Error]() As String
            Get
                Return Me.m_Error
            End Get
            Set
                Me.m_Error = value
            End Set
        End Property
        
        Public Property Schedule() As String
            Get
                Return Me.m_Schedule
            End Get
            Set
                Me.m_Schedule = value
            End Set
        End Property
        
        Public Property ScheduleExceptions() As String
            Get
                Return Me.m_ScheduleExceptions
            End Get
            Set
                Me.m_ScheduleExceptions = value
            End Set
        End Property
        
        Public Property CacheProfile() As String
            Get
                Return Me.m_CacheProfile
            End Get
            Set
                Me.m_CacheProfile = value
            End Set
        End Property
        
        Public Property CacheDuration() As Integer
            Get
                Return Me.m_CacheDuration
            End Get
            Set
                Me.m_CacheDuration = value
            End Set
        End Property
        
        Public Property CacheLocation() As HttpCacheability
            Get
                Return Me.m_CacheLocation
            End Get
            Set
                Me.m_CacheLocation = value
            End Set
        End Property
        
        Public Property CacheVaryByParams() As String()
            Get
                Return Me.m_CacheVaryByParams
            End Get
            Set
                Me.m_CacheVaryByParams = value
            End Set
        End Property
        
        Public Property CacheVaryByHeaders() As String()
            Get
                Return Me.m_CacheVaryByHeaders
            End Get
            Set
                Me.m_CacheVaryByHeaders = value
            End Set
        End Property
        
        Public Property CacheNoStore() As Boolean
            Get
                Return Me.m_CacheNoStore
            End Get
            Set
                Me.m_CacheNoStore = value
            End Set
        End Property
        
        Public Property Text() As String
            Get
                If ((Not (Me.Data) Is Nothing) AndAlso (Not (String.IsNullOrEmpty(Me.ContentType)) AndAlso Me.ContentType.StartsWith("text/"))) Then
                    Return Encoding.UTF8.GetString(Me.Data)
                End If
                Return Nothing
            End Get
            Set
                If (value Is Nothing) Then
                    m_Data = Nothing
                Else
                    m_Data = Encoding.UTF8.GetBytes(value)
                    m_ContentType = "text/plain"
                End If
            End Set
        End Property
        
        Public ReadOnly Property IsText() As Boolean
            Get
                Return ((Not (m_ContentType) Is Nothing) AndAlso (m_ContentType.StartsWith("text/") OrElse (m_ContentType = "application/javascript")))
            End Get
        End Property
        
        Public Overrides Function ToString() As String
            Return String.Format("{0}/{1}", Path, Name)
        End Function
    End Class
    
    Public Class SiteContentFileList
        Inherits List(Of SiteContentFile)
    End Class
    
    Partial Public Class ApplicationServices
        Inherits EnterpriseApplicationServices
        
        Public Shared ReadOnly Property HomePageUrl() As [String]
            Get
                Return Create().UserHomePageUrl()
            End Get
        End Property
        
        Public Shared Sub Initialize()
            Create().RegisterServices()
        End Sub
        
        Public Shared Function Login(ByVal username As String, ByVal password As String, ByVal createPersistentCookie As Boolean) As Boolean
            Return Create().UserLogin(username, password, createPersistentCookie)
        End Function
        
        Public Shared Sub Logout()
            Create().UserLogout()
        End Sub
        
        Public Shared Function Roles() As String()
            Return Create().UserRoles()
        End Function
    End Class
    
    Public Class ApplicationServicesBase
        
        Public Shared EnableMobileClient As Boolean = true
        
        Public Shared DesignerPort As String = String.Empty
        
        Private Shared m_EnableMinifiedCss As Boolean
        
        Public Shared NameValueListRegex As Regex = New Regex("^\s*(?'Name'\w+)\s*=\s*(?'Value'[\S\s]+?)\s*$", RegexOptions.Multiline)
        
        Public Shared SystemResourceRegex As Regex = New Regex("~/((sys/)|(views/)|(site\b))", RegexOptions.IgnoreCase)
        
        Private m_UserTheme As String
        
        Public Shared CssUrlRegex As Regex = New Regex("(?'Header'\burl\s*\(\s*(\&quot;|\')?)(?'Name'\w+)(?'Symbol'\S)")
        
        Public Shared ViewPageCompressRegex As Regex = New Regex("((""(DefaultValue)""\:(""[\s\S]*?""))|(""(Items|Pivots|Fields|Views|ActionGroups|Categ"& _ 
                "ories|Filter|Expressions)""\:(\[\]))|(""(Len|CategoryIndex|Rows|Columns|Search|Ite"& _ 
                "msPageSize|Aggregate|OnDemandStyle|TextMode|MaskType|AutoCompletePrefixLength|Da"& _ 
                "taViewPageSize|PageOffset)""\:(0))|(""(CausesValidation|AllowQBE|AllowSorting|Form"& _ 
                "atOnClient|HtmlEncode|RequiresMetaData|RequiresRowCount|ShowInSelector)""\:(true)"& _ 
                ")|(""(IsPrimaryKey|ReadOnly|HasDefaultValue|Hidden|AllowLEV|AllowNulls|OnDemand|I"& _ 
                "sMirror|Calculated|CausesCalculate|IsVirtual|AutoSelect|SearchOnStart|ShowInSumm"& _ 
                "ary|ItemsLetters|WhenKeySelected|RequiresSiteContentText|RequiresPivot|RequiresA"& _ 
                "ggregates|InTransaction|NewColumn|Floating|Collapsed|SupportsCaching|AllowDistin"& _ 
                "ctFieldInFilter|Flat|RequiresMetaData|RequiresRowCount|(DataView(ShowInSummary|S"& _ 
                "howActionBar|ShowDescription|ShowViewSelector|ShowModalForms|SearchByFirstLetter"& _ 
                "|SearchOnStart|ShowPager|ShowPageSize|ShowSearchBar|ShowQuickFind|ShowRowNumber|"& _ 
                "AutoHighlightFirstRow|AutoSelectFirstRow)))""\:(false))|(""(AliasName|Tag|FooterTe"& _ 
                "xt|ToolTip|Watermark|DataFormatString|Copy|HyperlinkFormatString|SourceFields|Se"& _ 
                "archOptions|ItemsDataController|ItemsTargetController|ItemsDataView|ItemsDataVal"& _ 
                "ueField|ItemsDataTextField|ItemsStyle|ItemsNewDataView|OnDemandHandler|Mask|Cont"& _ 
                "extFields|Formula|Configuration|Editor|ItemsDescription|Group|CommandName|Comman"& _ 
                "dArgument|HeaderText|Description|CssClass|Confirmation|Notify|Key|WhenLastComman"& _ 
                "dName|WhenLastCommandArgument|WhenClientScript|WhenTag|WhenHRef|WhenView|PivotDe"& _ 
                "finitions|Aggregates|PivotDefinitions|Aggregates|ViewType|LastView|StatusBar|Ico"& _ 
                "ns|LEVs|QuickFindHint|InnerJoinPrimaryKey|SystemFilter|DistinctValueFieldName|Cl"& _ 
                "ientScript|FirstLetters|SortExpression|Template|Tab|Wizard|InnerJoinForeignKey|E"& _ 
                "xpressions|ViewHeaderText|ViewLayout|GroupExpression|(DataView(FilterSource|Cont"& _ 
                "roller|Id|FilterFields|ShowActionBar|ShowActionButtons|MultiSelect)))""\:(""""|null"& _ 
                "))|(""Type"":""String"")),?")
        
        Public Shared ViewPageCompress2Regex As Regex = New Regex(",\}(,|])")
        
        Public Shared Property EnableMinifiedCss() As Boolean
            Get
                Return m_EnableMinifiedCss
            End Get
            Set
                m_EnableMinifiedCss = value
            End Set
        End Property
        
        Public Shared ReadOnly Property IsSiteContentEnabled() As Boolean
            Get
                Return Not (String.IsNullOrEmpty(SiteContentControllerName))
            End Get
        End Property
        
        Public Shared ReadOnly Property SiteContentControllerName() As String
            Get
                Return Create().GetSiteContentControllerName()
            End Get
        End Property
        
        Public Shared ReadOnly Property SiteContentEditors() As String()
            Get
                Return Create().GetSiteContentEditors()
            End Get
        End Property
        
        Public Shared ReadOnly Property SiteContentDevelopers() As String()
            Get
                Return Create().GetSiteContentDevelopers()
            End Get
        End Property
        
        Public Shared ReadOnly Property IsContentEditor() As Boolean
            Get
                Dim principal As IPrincipal = HttpContext.Current.User
                For Each r As String in Create().GetSiteContentEditors()
                    If principal.IsInRole(r) Then
                        Return true
                    End If
                Next
                Return false
            End Get
        End Property
        
        Public Shared ReadOnly Property IsDeveloper() As Boolean
            Get
                Dim principal As IPrincipal = HttpContext.Current.User
                For Each r As String in Create().GetSiteContentDevelopers()
                    If principal.IsInRole(r) Then
                        Return true
                    End If
                Next
                Return false
            End Get
        End Property
        
        Public Shared ReadOnly Property IsSafeMode() As Boolean
            Get
                Dim request As HttpRequest = HttpContext.Current.Request
                Dim test As Uri = request.UrlReferrer
                If (test Is Nothing) Then
                    test = request.Url
                End If
                Return ((test Is Nothing) AndAlso (test.ToString().Contains("_safemode=true") AndAlso DataControllerBase.UserIsInRole(SiteContentDevelopers)))
            End Get
        End Property
        
        Public Overridable ReadOnly Property ScheduleCacheDuration() As Integer
            Get
                Return 20
            End Get
        End Property
        
        Public Overridable ReadOnly Property Realm() As String
            Get
                Return Name
            End Get
        End Property
        
        Public Overridable ReadOnly Property Name() As String
            Get
                Return "Test 2"
            End Get
        End Property
        
        Public Overridable ReadOnly Property MaxPivotRowCount() As Integer
            Get
                Return 250000
            End Get
        End Property
        
        Public Shared ReadOnly Property Current() As ApplicationServices
            Get
                Return Create()
            End Get
        End Property
        
        Public Shared ReadOnly Property IsTouchClient() As Boolean
            Get
                Dim isMobile As Object = HttpContext.Current.Items("ApplicationServices_IsTouchClient")
                If (isMobile Is Nothing) Then
                    isMobile = ClientIsUsingTouchUI()
                    HttpContext.Current.Items("ApplicationServices_IsTouchClient") = isMobile
                End If
                Return CType(isMobile,Boolean)
            End Get
        End Property
        
        Public Overridable Property UserTheme() As String
            Get
                If String.IsNullOrEmpty(m_UserTheme) Then
                    If (Not (HttpContext.Current) Is Nothing) Then
                        Dim themeCookie As HttpCookie = HttpContext.Current.Request.Cookies(".COTTHEME")
                        If (Not (themeCookie) Is Nothing) Then
                            m_UserTheme = themeCookie.Value
                        End If
                    End If
                    If String.IsNullOrEmpty(m_UserTheme) Then
                        m_UserTheme = "Light"
                    End If
                    m_UserTheme = m_UserTheme.Replace(" ", String.Empty)
                End If
                Return m_UserTheme
            End Get
            Set
                m_UserTheme = value
            End Set
        End Property
        
        Public Overridable Function GetNavigateUrl() As String
            Return Nothing
        End Function
        
        Public Shared Sub VerifyUrl()
            Dim navigateUrl As String = Create().GetNavigateUrl()
            If Not (String.IsNullOrEmpty(navigateUrl)) Then
                Dim current As HttpContext = HttpContext.Current
                If Not (VirtualPathUtility.ToAbsolute(navigateUrl).Equals(current.Request.RawUrl, StringComparison.CurrentCultureIgnoreCase)) Then
                    current.Response.Redirect(navigateUrl)
                End If
            End If
        End Sub
        
        Public Overridable Sub RegisterServices()
            CreateStandardMembershipAccounts()
            Dim routes As RouteCollection = RouteTable.Routes
            RegisterIgnoredRoutes(routes)
            RegisterContentServices(RouteTable.Routes)
            'Find Designer Port
            Try 
                Dim configPath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "IISExpress\config\applicationhost.config")
                If File.Exists(configPath) Then
                    Dim content As String = File.ReadAllText(configPath)
                    Dim m As Match = Regex.Match(content, "<site name=""CodeOnTime"".*?bindingInformation=""\*:(?'Port'\d+):localhost""", RegexOptions.Singleline)
                    If m.Success Then
                        DesignerPort = m.Groups("Port").Value
                    End If
                End If
            Finally
                'release resources here
            End Try
        End Sub
        
        Public Shared Sub Start()
            Current.InstanceStart()
        End Sub
        
        Protected Overridable Sub InstanceStart()
            MyCompany.Services.ApplicationServices.Initialize()
        End Sub
        
        Public Shared Sub [Stop]()
            Current.InstanceStop()
        End Sub
        
        Protected Overridable Sub InstanceStop()
        End Sub
        
        Public Shared Sub SessionStart()
            'The line below will prevent intermittent error “Session state has created a session id,
            'but cannot save it because the response was already flushed by the application.”
            Dim sessionId As String = HttpContext.Current.Session.SessionID
            Current.UserSessionStart()
        End Sub
        
        Protected Overridable Sub UserSessionStart()
        End Sub
        
        Public Shared Sub SessionStop()
            Current.UserSessionStop()
        End Sub
        
        Protected Overridable Sub UserSessionStop()
        End Sub
        
        Public Shared Sub [Error]()
            Dim context As HttpContext = HttpContext.Current
            If (Not (context) Is Nothing) Then
                Current.HandleError(context, context.Server.GetLastError())
            End If
        End Sub
        
        Sub HandleError(ByVal context As HttpContext, ByVal [error] As Exception)
        End Sub
        
        Public Overridable Sub RegisterContentServices(ByVal routes As RouteCollection)
            GenericRoute.Map(RouteTable.Routes, New PlaceholderHandler(), "placeholder/{FileName}")
            routes.MapPageRoute("SiteContent", "{*url}", "~/Site.aspx")
            routes.MapPageRoute("DataControllerService", "{*url}", AquariumExtenderBase.DefaultServicePath)
        End Sub
        
        Public Overridable Sub RegisterIgnoredRoutes(ByVal routes As RouteCollection)
            routes.Ignore("{handler}.ashx")
            routes.Ignore("favicon.ico")
            routes.Ignore("controlhost.aspx")
            routes.Ignore("charthost.aspx")
            routes.Ignore("{resource}.axd/{*pathInfo}")
            routes.Ignore("daf/{service}/{*methodName}")
            routes.Ignore("app_themes/{themeFolder}/{file}")
            routes.Ignore("{id}/arterySignalR/{*pathInfo}")
            If Not (IsSiteContentEnabled) Then
                routes.Ignore("images/{*pathInfo}")
                routes.Ignore("documents/{*pathInfo}")
                routes.Ignore("download/{*pathInfo}")
            End If
            routes.Ignore("touch/{*pathInfo}")
            routes.Ignore("scripts/{*pathInfo}")
            routes.Ignore("services/{*pathInfo}")
        End Sub
        
        Public Overloads Shared Function LoadContent() As SortedDictionary(Of String, String)
            Dim content As SortedDictionary(Of String, String) = New SortedDictionary(Of String, String)()
            Create().LoadContent(HttpContext.Current.Request, HttpContext.Current.Response, content)
            Dim rawContent As String = Nothing
            If content.TryGetValue("File", rawContent) Then
                'find the head
                Dim headMatch As Match = Regex.Match(rawContent, "<head>([\s\S]+?)</head>")
                If headMatch.Success Then
                    Dim head As String = headMatch.Groups(1).Value
                    head = Regex.Replace(head, "\s*<meta charset="".+""\s*/?>\s*", String.Empty)
                    content("Head") = Regex.Replace(head, "\s*<title>([\S\s]*?)</title>\s*", String.Empty)
                    'find the title
                    Dim titleMatch As Match = Regex.Match(head, "<title>(?'Title'[\S\s]+?)</title>")
                    If titleMatch.Success Then
                        Dim title As String = titleMatch.Groups("Title").Value
                        content("PageTitle") = title
                        content("PageTitleContent") = title
                    End If
                    'find "about"
                    Dim aboutMatch As Match = Regex.Match(head, "<meta\s+name\s*=\s*""description""\s+content\s*=\s*""([\s\S]+?)""\s*/>")
                    If aboutMatch.Success Then
                        content("About") = HttpUtility.HtmlDecode(aboutMatch.Groups(1).Value)
                    End If
                End If
                'find the body
                Dim bodyMatch As Match = Regex.Match(rawContent, "<body(?'Attr'[\s\S]*?)>(?'Body'[\s\S]+?)</body>")
                If bodyMatch.Success Then
                    content("PageContent") = bodyMatch.Groups("Body").Value
                    content("BodyAttributes") = bodyMatch.Groups("Attr").Value
                Else
                    content("PageContent") = rawContent
                End If
            End If
            Return content
        End Function
        
        Public Overridable Function GetSiteContentControllerName() As String
            Return Nothing
        End Function
        
        Public Overridable Function GetSiteContentViewId() As String
            Return "editForm1"
        End Function
        
        Public Overridable Function GetSiteContentEditors() As String()
            Return New String() {"Administrators", "Content Editors", "Developers"}
        End Function
        
        Public Overridable Function GetSiteContentDevelopers() As String()
            Return New String() {"Administrators", "Developers"}
        End Function
        
        Public Overridable Sub AfterAction(ByVal args As ActionArgs, ByVal result As ActionResult)
        End Sub
        
        Public Overridable Sub BeforeAction(ByVal args As ActionArgs, ByVal result As ActionResult)
            If (args.Controller = SiteContentControllerName) Then
                Dim userIsDeveloper As Boolean = IsDeveloper
                If ((Not (IsContentEditor) OrElse Not (userIsDeveloper)) OrElse (args.Values Is Nothing)) Then
                    Throw New HttpException(403, "Forbidden")
                End If
                Dim id As FieldValue = args.SelectFieldValueObject(SiteContentFieldName(SiteContentFields.SiteContentId))
                Dim path As FieldValue = args.SelectFieldValueObject(SiteContentFieldName(SiteContentFields.Path))
                Dim fileName As FieldValue = args.SelectFieldValueObject(SiteContentFieldName(SiteContentFields.DataFileName))
                Dim text As FieldValue = args.SelectFieldValueObject(SiteContentFieldName(SiteContentFields.Text))
                'verify "Path" access
                If ((path Is Nothing) OrElse (fileName Is Nothing)) Then
                    Throw New HttpException(403, "Forbidden")
                End If
                If (((Not (path.Value) Is Nothing) AndAlso path.Value.ToString().StartsWith("sys/", StringComparison.CurrentCultureIgnoreCase)) AndAlso Not (userIsDeveloper)) Then
                    Throw New HttpException(403, "Forbidden")
                End If
                If (((Not (path.OldValue) Is Nothing) AndAlso path.OldValue.ToString().StartsWith("sys/", StringComparison.CurrentCultureIgnoreCase)) AndAlso Not (userIsDeveloper)) Then
                    Throw New HttpException(403, "Forbidden")
                End If
                'convert and parse "Text" as needed
                If ((Not (text) Is Nothing) AndAlso Not ((args.CommandName = "Delete"))) Then
                    Dim s As String = Convert.ToString(text.Value)
                    If (s = "$Text") Then
                        Dim fullPath As String = Convert.ToString(path.Value)
                        If Not (String.IsNullOrEmpty(fullPath)) Then
                            fullPath = (fullPath + "/")
                        End If
                        fullPath = (fullPath + Convert.ToString(fileName.Value))
                        If Not (fullPath.StartsWith("/")) Then
                            fullPath = ("/" + fullPath)
                        End If
                        If Not (fullPath.EndsWith(".html", StringComparison.CurrentCultureIgnoreCase)) Then
                            fullPath = (fullPath + ".html")
                        End If
                        Dim physicalPath As String = HttpContext.Current.Server.MapPath(("~" + fullPath))
                        If Not (File.Exists(physicalPath)) Then
                            physicalPath = HttpContext.Current.Server.MapPath(("~" + fullPath.Replace("-", String.Empty)))
                            If Not (File.Exists(physicalPath)) Then
                                physicalPath = Nothing
                            End If
                        End If
                        If Not (String.IsNullOrEmpty(physicalPath)) Then
                            text.NewValue = File.ReadAllText(physicalPath)
                        End If
                    End If
                End If
            End If
        End Sub
        
        Public Overridable Function SiteContentFieldName(ByVal field As SiteContentFields) As String
            Return field.ToString()
        End Function
        
        Public Overridable Function ReadSiteContentString(ByVal relativePath As String) As String
            Dim data() As Byte = ReadSiteContentBytes(relativePath)
            If (data Is Nothing) Then
                Return Nothing
            End If
            Return Encoding.UTF8.GetString(data)
        End Function
        
        Public Overridable Function ReadSiteContentBytes(ByVal relativePath As String) As Byte()
            Dim f As SiteContentFile = ReadSiteContent(relativePath)
            If (f Is Nothing) Then
                Return Nothing
            End If
            Return f.Data
        End Function
        
        Public Overloads Overridable Function ReadSiteContent(ByVal relativePath As String) As SiteContentFile
            Dim context As HttpContext = HttpContext.Current
            Dim f As SiteContentFile = CType(context.Items(relativePath),SiteContentFile)
            If (f Is Nothing) Then
                f = CType(context.Cache(relativePath),SiteContentFile)
            End If
            If (f Is Nothing) Then
                Dim path As String = relativePath
                Dim fileName As String = relativePath
                Dim index As Integer = relativePath.LastIndexOf("/")
                If (index >= 0) Then
                    fileName = path.Substring((index + 1))
                    path = relativePath.Substring(0, index)
                Else
                    path = Nothing
                End If
                Dim files As SiteContentFileList = ReadSiteContent(path, fileName, 1)
                If (files.Count = 1) Then
                    f = files(0)
                    context.Items(relativePath) = f
                    If (f.CacheDuration > 0) Then
                        context.Cache.Add(relativePath, f, Nothing, DateTime.Now.AddSeconds(f.CacheDuration), Cache.NoSlidingExpiration, CacheItemPriority.Normal, Nothing)
                    End If
                End If
            End If
            Return f
        End Function
        
        Public Overloads Overridable Function ReadSiteContent(ByVal relativePath As String, ByVal fileName As String) As SiteContentFileList
            Return ReadSiteContent(relativePath, fileName, Int32.MaxValue)
        End Function
        
        Public Overloads Overridable Function ReadSiteContent(ByVal relativePath As String, ByVal fileName As String, ByVal maxCount As Integer) As SiteContentFileList
            Dim result As SiteContentFileList = New SiteContentFileList()
            If IsSafeMode Then
                Return result
            End If
            'prepare a filter
            Dim dataFileNameField As String = SiteContentFieldName(SiteContentFields.DataFileName)
            Dim pathField As String = SiteContentFieldName(SiteContentFields.Path)
            Dim filter As List(Of String) = New List(Of String)()
            Dim pathFilter As String = Nothing
            If Not (String.IsNullOrEmpty(relativePath)) Then
                pathFilter = "{0}:={1}"
                Dim firstWildcardIndex As Integer = relativePath.IndexOf("%")
                If (firstWildcardIndex >= 0) Then
                    Dim lastWildcardIndex As Integer = relativePath.LastIndexOf("%")
                    pathFilter = "{0}:$contains${1}"
                    If (firstWildcardIndex = lastWildcardIndex) Then
                        If (firstWildcardIndex = 0) Then
                            pathFilter = "{0}:$endswith${1}"
                            relativePath = relativePath.Substring(1)
                        Else
                            If (lastWildcardIndex = (relativePath.Length - 1)) Then
                                pathFilter = "{0}:$beginswith${1}"
                                relativePath = relativePath.Substring(0, lastWildcardIndex)
                            End If
                        End If
                    End If
                End If
            Else
                pathFilter = "{0}:=null"
            End If
            Dim fileNameFilter As String = Nothing
            If (Not (String.IsNullOrEmpty(fileName)) AndAlso Not ((fileName = "%"))) Then
                fileNameFilter = "{0}:={1}"
                Dim firstWildcardIndex As Integer = fileName.IndexOf("%")
                If (firstWildcardIndex >= 0) Then
                    Dim lastWildcardIndex As Integer = fileName.LastIndexOf("%")
                    fileNameFilter = "{0}:$contains${1}"
                    If (firstWildcardIndex = lastWildcardIndex) Then
                        If (firstWildcardIndex = 0) Then
                            fileNameFilter = "{0}:$endswith${1}"
                            fileName = fileName.Substring(1)
                        Else
                            If (lastWildcardIndex = (fileName.Length - 1)) Then
                                fileNameFilter = "{0}:$beginswith${1}"
                                fileName = fileName.Substring(0, lastWildcardIndex)
                            End If
                        End If
                    End If
                End If
            End If
            If (Not (String.IsNullOrEmpty(pathFilter)) OrElse Not (String.IsNullOrEmpty(fileNameFilter))) Then
                filter.Add("_match_:$all$")
                If Not (String.IsNullOrEmpty(pathFilter)) Then
                    filter.Add(String.Format(pathFilter, pathField, DataControllerBase.ValueToString(relativePath)))
                End If
                If (Not ((fileName = Nothing)) AndAlso Not ((fileName = "%"))) Then
                    filter.Add(String.Format(fileNameFilter, dataFileNameField, DataControllerBase.ValueToString(fileName)))
                    If (String.IsNullOrEmpty(Path.GetExtension(fileName)) AndAlso (String.IsNullOrEmpty(relativePath) OrElse (Not (relativePath.StartsWith("sys/", StringComparison.OrdinalIgnoreCase)) OrElse relativePath.StartsWith("sys/controls", StringComparison.OrdinalIgnoreCase)))) Then
                        filter.Add("_match_:$all$")
                        If Not (String.IsNullOrEmpty(pathFilter)) Then
                            filter.Add(String.Format(pathFilter, pathField, DataControllerBase.ValueToString(relativePath)))
                        End If
                        filter.Add(String.Format(fileNameFilter, dataFileNameField, DataControllerBase.ValueToString((Path.GetFileNameWithoutExtension(fileName).Replace("-", String.Empty) + ".html"))))
                    End If
                End If
            End If
            ' determine user identity
            Dim context As HttpContext = HttpContext.Current
            Dim userName As String = String.Empty
            Dim isAuthenticated As Boolean = false
            Dim user As IPrincipal = context.User
            If (Not (user) Is Nothing) Then
                userName = user.Identity.Name.ToLower()
                isAuthenticated = user.Identity.IsAuthenticated
            End If
            'enumerate site content files
            Dim r As PageRequest = New PageRequest()
            r.Controller = GetSiteContentControllerName()
            r.View = GetSiteContentViewId()
            r.RequiresSiteContentText = true
            r.PageSize = Int32.MaxValue
            r.Filter = filter.ToArray()
            Dim engine As IDataEngine = ControllerFactory.CreateDataEngine()
            Dim controller As DataControllerBase = CType(engine,DataControllerBase)
            controller.AllowPublicAccess = true
            Dim reader As IDataReader = engine.ExecuteReader(r)
            Dim blobsToResolve As SortedDictionary(Of String, SiteContentFile) = New SortedDictionary(Of String, SiteContentFile)()
            'verify optional SiteContent fields
            Dim fieldDictionary As SortedDictionary(Of String, String) = New SortedDictionary(Of String, String)()
            Dim i As Integer = 0
            Do While (i < reader.FieldCount)
                Dim fieldName As String = reader.GetName(i)
                fieldDictionary(fieldName) = fieldName
                i = (i + 1)
            Loop
            Dim rolesField As String = Nothing
            fieldDictionary.TryGetValue(SiteContentFieldName(SiteContentFields.Roles), rolesField)
            Dim usersField As String = Nothing
            fieldDictionary.TryGetValue(SiteContentFieldName(SiteContentFields.Users), usersField)
            Dim roleExceptionsField As String = Nothing
            fieldDictionary.TryGetValue(SiteContentFieldName(SiteContentFields.RoleExceptions), roleExceptionsField)
            Dim userExceptionsField As String = Nothing
            fieldDictionary.TryGetValue(SiteContentFieldName(SiteContentFields.UserExceptions), userExceptionsField)
            Dim cacheProfileField As String = Nothing
            fieldDictionary.TryGetValue(SiteContentFieldName(SiteContentFields.CacheProfile), cacheProfileField)
            Dim scheduleField As String = Nothing
            fieldDictionary.TryGetValue(SiteContentFieldName(SiteContentFields.Schedule), scheduleField)
            Dim scheduleExceptionsField As String = Nothing
            fieldDictionary.TryGetValue(SiteContentFieldName(SiteContentFields.ScheduleExceptions), scheduleExceptionsField)
            Dim dataField As DataField = controller.CreateViewPage().FindField(SiteContentFieldName(SiteContentFields.Data))
            Dim blobHandler As String = dataField.OnDemandHandler
            Dim wr As WorkflowRegister = WorkflowRegister.GetCurrent(relativePath)
            'read SiteContent files
            Do While reader.Read()
                'verify user access rights
                Dim include As Boolean = true
                If Not (String.IsNullOrEmpty(rolesField)) Then
                    Dim roles As String = Convert.ToString(reader(rolesField))
                    If (Not (String.IsNullOrEmpty(roles)) AndAlso Not ((roles = "?"))) Then
                        If ((roles = "*") AndAlso Not (isAuthenticated)) Then
                            include = false
                        Else
                            If (Not (isAuthenticated) OrElse (Not ((roles = "*")) AndAlso Not (DataControllerBase.UserIsInRole(roles)))) Then
                                include = false
                            End If
                        End If
                    End If
                End If
                If (include AndAlso Not (String.IsNullOrEmpty(usersField))) Then
                    Dim users As String = Convert.ToString(reader(usersField))
                    If (Not (String.IsNullOrEmpty(users)) AndAlso (Array.IndexOf(users.ToLower().Split(New Char() {Global.Microsoft.VisualBasic.ChrW(44)}, StringSplitOptions.RemoveEmptyEntries), userName) = -1)) Then
                        include = false
                    End If
                End If
                If (include AndAlso Not (String.IsNullOrEmpty(roleExceptionsField))) Then
                    Dim roleExceptions As String = Convert.ToString(reader(roleExceptionsField))
                    If (Not (String.IsNullOrEmpty(roleExceptions)) AndAlso (isAuthenticated AndAlso ((roleExceptions = "*") OrElse DataControllerBase.UserIsInRole(roleExceptions)))) Then
                        include = false
                    End If
                End If
                If (include AndAlso Not (String.IsNullOrEmpty(userExceptionsField))) Then
                    Dim userExceptions As String = Convert.ToString(reader(userExceptionsField))
                    If (Not (String.IsNullOrEmpty(userExceptions)) AndAlso Not ((Array.IndexOf(userExceptions.ToLower().Split(New Char() {Global.Microsoft.VisualBasic.ChrW(44)}, StringSplitOptions.RemoveEmptyEntries), userName) = -1))) Then
                        include = false
                    End If
                End If
                Dim physicalName As String = Convert.ToString(reader(dataFileNameField))
                Dim physicalPath As String = Convert.ToString(reader(SiteContentFieldName(SiteContentFields.Path)))
                'check if the content object is a part of a workflow
                If (((Not (wr) Is Nothing) AndAlso wr.Enabled) AndAlso Not (wr.IsMatch(physicalPath, physicalName))) Then
                    include = false
                End If
                Dim schedule As String = Nothing
                Dim scheduleExceptions As String = Nothing
                'check if the content object is on schedule
                If (include AndAlso (String.IsNullOrEmpty(physicalPath) OrElse Not (physicalPath.StartsWith("sys/schedules/")))) Then
                    If Not (String.IsNullOrEmpty(scheduleField)) Then
                        schedule = Convert.ToString(reader(scheduleField))
                    End If
                    If Not (String.IsNullOrEmpty(scheduleExceptionsField)) Then
                        scheduleExceptions = Convert.ToString(reader(scheduleExceptionsField))
                    End If
                    If (Not (String.IsNullOrEmpty(schedule)) OrElse Not (String.IsNullOrEmpty(scheduleExceptions))) Then
                        Dim scheduleStatusKey As String = String.Format("ScheduleStatus|{0}|{1}", schedule, scheduleExceptions)
                        Dim status As ScheduleStatus = CType(context.Items(scheduleStatusKey),ScheduleStatus)
                        If (status Is Nothing) Then
                            status = CType(context.Cache(scheduleStatusKey),ScheduleStatus)
                        End If
                        Dim scheduleStatusChanged As Boolean = false
                        If (status Is Nothing) Then
                            If (Not (String.IsNullOrEmpty(schedule)) AndAlso Not (schedule.Contains("+"))) Then
                                schedule = ReadSiteContentString(("sys/schedules%/" + schedule))
                            End If
                            If (Not (String.IsNullOrEmpty(scheduleExceptions)) AndAlso Not (scheduleExceptions.Contains("+"))) Then
                                scheduleExceptions = ReadSiteContentString(("sys/schedules%/" + scheduleExceptions))
                            End If
                            If (Not (String.IsNullOrEmpty(schedule)) OrElse Not (String.IsNullOrEmpty(scheduleExceptions))) Then
                                status = Scheduler.Test(schedule, scheduleExceptions)
                            Else
                                status = New ScheduleStatus()
                                status.Success = true
                                status.NextTestDate = DateTime.MaxValue
                            End If
                            context.Items(scheduleStatusKey) = status
                            scheduleStatusChanged = true
                        Else
                            If (DateTime.Now > status.NextTestDate) Then
                                status = Scheduler.Test(status.Schedule, status.Exceptions)
                                context.Items(scheduleStatusKey) = status
                                scheduleStatusChanged = true
                            End If
                        End If
                        If scheduleStatusChanged Then
                            context.Cache.Add(scheduleStatusKey, status, Nothing, DateTime.Now.AddSeconds(ScheduleCacheDuration), Cache.NoSlidingExpiration, CacheItemPriority.Normal, Nothing)
                        End If
                        If Not (status.Success) Then
                            include = false
                        End If
                    End If
                End If
                'create a file instance
                If include Then
                    Dim siteContentIdField As String = SiteContentFieldName(SiteContentFields.SiteContentId)
                    Dim f As SiteContentFile = New SiteContentFile()
                    f.Id = reader(siteContentIdField)
                    f.Name = fileName
                    f.PhysicalName = physicalName
                    If (String.IsNullOrEmpty(f.Name) OrElse f.Name.Contains("%")) Then
                        f.Name = f.PhysicalName
                    End If
                    f.Path = physicalPath
                    f.ContentType = Convert.ToString(reader(SiteContentFieldName(SiteContentFields.DataContentType)))
                    f.Schedule = schedule
                    f.ScheduleExceptions = scheduleExceptions
                    If Not (String.IsNullOrEmpty(cacheProfileField)) Then
                        Dim cacheProfile As String = Convert.ToString(reader(cacheProfileField))
                        If Not (String.IsNullOrEmpty(cacheProfile)) Then
                            f.CacheProfile = cacheProfile
                            cacheProfile = ReadSiteContentString(("sys/cache-profiles/" + cacheProfile))
                            If Not (String.IsNullOrEmpty(cacheProfile)) Then
                                Dim m As Match = NameValueListRegex.Match(cacheProfile)
                                Do While m.Success
                                    Dim n As String = m.Groups("Name").Value.ToLower()
                                    Dim v As String = m.Groups("Value").Value
                                    If (n = "duration") Then
                                        Dim duration As Integer = 0
                                        If Int32.TryParse(v, duration) Then
                                            f.CacheDuration = duration
                                            f.CacheLocation = HttpCacheability.ServerAndPrivate
                                        End If
                                    Else
                                        If (n = "location") Then
                                            Try 
                                                f.CacheLocation = CType(TypeDescriptor.GetConverter(GetType(HttpCacheability)).ConvertFromString(v),HttpCacheability)
                                            Catch __exception As Exception
                                            End Try
                                        Else
                                            If (n = "varybyheaders") Then
                                                f.CacheVaryByHeaders = v.Split(New Char() {Global.Microsoft.VisualBasic.ChrW(44), Global.Microsoft.VisualBasic.ChrW(59)}, StringSplitOptions.RemoveEmptyEntries)
                                            Else
                                                If (n = "varybyparams") Then
                                                    f.CacheVaryByParams = v.Split(New Char() {Global.Microsoft.VisualBasic.ChrW(44), Global.Microsoft.VisualBasic.ChrW(59)}, StringSplitOptions.RemoveEmptyEntries)
                                                Else
                                                    If (n = "nostore") Then
                                                        f.CacheNoStore = (v.ToLower() = "true")
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                    m = m.NextMatch()
                                Loop
                            End If
                        End If
                    End If
                    Dim textString As Object = reader(SiteContentFieldName(SiteContentFields.Text))
                    If (DBNull.Value.Equals(textString) OrElse Not (f.IsText)) Then
                        Dim blobKey As String = String.Format("{0}=o|{1}", blobHandler, f.Id)
                        If (f.CacheDuration > 0) Then
                            f.Data = CType(HttpContext.Current.Cache(blobKey),Byte())
                        End If
                        If (f.Data Is Nothing) Then
                            blobsToResolve(blobKey) = f
                        End If
                    Else
                        If String.IsNullOrEmpty(f.ContentType) Then
                            If Regex.IsMatch(CType(textString,String), "</\w+\s*>") Then
                                f.ContentType = "text/xml"
                            Else
                                f.ContentType = "text/plain"
                            End If
                        End If
                        f.Data = Encoding.UTF8.GetBytes(CType(textString,String))
                    End If
                    result.Add(f)
                    If (result.Count = maxCount) Then
                        Exit Do
                    End If
                End If
            Loop
            reader.Close()
            For Each blobKey As String in blobsToResolve.Keys
                Dim f As SiteContentFile = blobsToResolve(blobKey)
                'download blob content
                Try 
                    f.Data = Blob.Read(blobKey)
                    If (f.CacheDuration > 0) Then
                        HttpContext.Current.Cache.Add(blobKey, f.Data, Nothing, DateTime.Now.AddSeconds(f.CacheDuration), Cache.NoSlidingExpiration, CacheItemPriority.Normal, Nothing)
                    End If
                Catch ex As Exception
                    f.Error = ex.Message
                End Try
            Next
            Return result
        End Function
        
        Public Overridable Function IsSystemResource(ByVal request As HttpRequest) As Boolean
            Return SystemResourceRegex.IsMatch(request.AppRelativeCurrentExecutionFilePath)
        End Function
        
        Public Overloads Overridable Sub LoadContent(ByVal request As HttpRequest, ByVal response As HttpResponse, ByVal content As SortedDictionary(Of String, String))
            If IsSystemResource(request) Then
                Return
            End If
            Dim text As String = Nothing
            Dim tryFileSystem As Boolean = true
            If IsSiteContentEnabled Then
                Dim fileName As String = HttpUtility.UrlDecode(request.Url.Segments((request.Url.Segments.Length - 1)))
                Dim path As String = request.CurrentExecutionFilePath.Substring(request.ApplicationPath.Length)
                If ((fileName = "/") AndAlso String.IsNullOrEmpty(path)) Then
                    fileName = "index"
                Else
                    path = path.Substring(0, (path.Length - fileName.Length))
                    If path.EndsWith("/") Then
                        path = path.Substring(0, (path.Length - 1))
                    End If
                End If
                If String.IsNullOrEmpty(path) Then
                    path = Nothing
                End If
                Dim files As SiteContentFileList = ReadSiteContent(path, fileName, 1)
                If (files.Count > 0) Then
                    Dim f As SiteContentFile = files(0)
                    If (f.ContentType = "text/html") Then
                        text = f.Text
                        tryFileSystem = false
                    Else
                        If (f.CacheDuration > 0) Then
                            Dim expires As DateTime = DateTime.Now.AddSeconds(f.CacheDuration)
                            response.Cache.SetExpires(expires)
                            response.Cache.SetCacheability(f.CacheLocation)
                            If (Not (f.CacheVaryByParams) Is Nothing) Then
                                For Each header As String in f.CacheVaryByParams
                                    response.Cache.VaryByParams(header) = true
                                Next
                            End If
                            If (Not (f.CacheVaryByHeaders) Is Nothing) Then
                                For Each header As String in f.CacheVaryByHeaders
                                    response.Cache.VaryByHeaders(header) = true
                                Next
                            End If
                            If f.CacheNoStore Then
                                response.Cache.SetNoStore()
                            End If
                        End If
                        response.ContentType = f.ContentType
                        response.AddHeader("Content-Disposition", ("filename=" + HttpUtility.UrlEncode(f.PhysicalName)))
                        response.OutputStream.Write(f.Data, 0, f.Data.Length)
                        Try 
                            response.Flush()
                        Catch __exception As Exception
                        End Try
                        response.End()
                    End If
                End If
            End If
            If tryFileSystem Then
                Dim fileName As String = request.PhysicalPath
                If Not (String.IsNullOrEmpty(Path.GetExtension(fileName))) Then
                    fileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName))
                End If
                fileName = (fileName + ".html")
                If File.Exists(fileName) Then
                    text = File.ReadAllText(fileName)
                Else
                    If Path.GetFileNameWithoutExtension(fileName).Contains("-") Then
                        fileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileName(fileName).Replace("-", String.Empty))
                        If File.Exists(fileName) Then
                            text = File.ReadAllText(fileName)
                        End If
                    End If
                End If
                If (Not (text) Is Nothing) Then
                    text = Localizer.Replace("Pages", fileName, text)
                End If
            End If
            If (Not (text) Is Nothing) Then
                text = Regex.Replace(text, "<!--[\s\S]+?-->\s*", String.Empty)
                content("File") = text
            End If
        End Sub
        
        Public Overridable Sub CreateStandardMembershipAccounts()
            'Create a separate code file with a definition of the partial class ApplicationServices overriding
            'this method to prevent automatic registration of 'admin' and 'user'. Do not change this file directly.
            RegisterStandardMembershipAccounts()
        End Sub
        
        Public Overridable Function RequiresAuthentication(ByVal request As HttpRequest) As Boolean
            Return request.Path.EndsWith("Export.ashx", StringComparison.CurrentCultureIgnoreCase)
        End Function
        
        Public Overridable Function AuthenticateRequest(ByVal context As HttpContext) As Boolean
            Return false
        End Function
        
        Public Overridable Function UserLogin(ByVal username As String, ByVal password As String, ByVal createPersistentCookie As Boolean) As Boolean
            If Membership.ValidateUser(username, password) Then
                FormsAuthentication.SetAuthCookie(username, createPersistentCookie)
                Return true
            Else
                Return false
            End If
        End Function
        
        Public Overridable Sub UserLogout()
            FormsAuthentication.SignOut()
        End Sub
        
        Public Overridable Function UserRoles() As String()
            Return Roles.GetRolesForUser()
        End Function
        
        Public Overridable Function UserHomePageUrl() As String
            If IsSiteContentEnabled Then
                Dim index As SiteContentFile = ReadSiteContent("index")
                If (Not (index) Is Nothing) Then
                    Return HttpContext.Current.Request.ApplicationPath
                End If
            End If
            Return "~/pages/home"
        End Function
        
        Public Shared Function Create() As ApplicationServices
            Return New ApplicationServices()
        End Function
        
        Public Shared Function UserIsAuthorizedToAccessResource(ByVal path As String, ByVal roles As String) As Boolean
            Return Not (Create().ResourceAuthorizationIsRequired(path, roles))
        End Function
        
        Public Overridable Function ResourceAuthorizationIsRequired(ByVal path As String, ByVal roles As String) As Boolean
            If (roles Is Nothing) Then
                roles = String.Empty
            Else
                roles = roles.Trim()
            End If
            Dim requiresAuthorization As Boolean = false
            Dim isAuthenticated As Boolean = HttpContext.Current.User.Identity.IsAuthenticated
            If (String.IsNullOrEmpty(roles) AndAlso Not (isAuthenticated)) Then
                requiresAuthorization = true
            End If
            If (Not (String.IsNullOrEmpty(roles)) AndAlso Not ((roles = "?"))) Then
                If (roles = "*") Then
                    If Not (isAuthenticated) Then
                        requiresAuthorization = true
                    End If
                Else
                    If (Not (isAuthenticated) OrElse Not (DataControllerBase.UserIsInRole(roles))) Then
                        requiresAuthorization = true
                    End If
                End If
            End If
            If (path = FormsAuthentication.LoginUrl) Then
                requiresAuthorization = false
            End If
            Return requiresAuthorization
        End Function
        
        Public Shared Sub RegisterStandardMembershipAccounts()
            Dim admin As MembershipUser = Membership.GetUser("admin")
            If ((Not (admin) Is Nothing) AndAlso admin.IsLockedOut) Then
                admin.UnlockUser()
            End If
            Dim user As MembershipUser = Membership.GetUser("user")
            If ((Not (user) Is Nothing) AndAlso user.IsLockedOut) Then
                user.UnlockUser()
            End If
            If (Membership.GetUser("admin") Is Nothing) Then
                Dim status As MembershipCreateStatus
                admin = Membership.CreateUser("admin", "admin123%", "admin@MyCompany.com", "ASP.NET", "Code OnTime", true, status)
                user = Membership.CreateUser("user", "user123%", "user@MyCompany.com", "ASP.NET", "Code OnTime", true, status)
                Roles.CreateRole("Administrators")
                Roles.CreateRole("Users")
                Roles.AddUserToRole(admin.UserName, "Users")
                Roles.AddUserToRole(user.UserName, "Users")
                Roles.AddUserToRole(admin.UserName, "Administrators")
            End If
        End Sub
        
        Public Shared Function ClientIsUsingTouchUI() As Boolean
            If Not (EnableMobileClient) Then
                Return false
            End If
            Dim request As HttpRequest = HttpContext.Current.Request
            Dim mobileCookie As HttpCookie = request.Cookies("appfactorytouchui")
            If (Not (mobileCookie) Is Nothing) Then
                Return (mobileCookie.Value = "true")
            End If
            Return true
        End Function
        
        Public Shared Sub RegisterCssLinks(ByVal p As Page, ByVal contentFramework As String)
            For Each c As Control in p.Header.Controls
                If TypeOf c Is HtmlLink Then
                    Dim l As HtmlLink = CType(c,HtmlLink)
                    If (l.ID = "MyCompanyTheme") Then
                        Return
                    End If
                    If l.Href.Contains("_Theme_Aquarium.css") Then
                        l.ID = "MyCompanyTheme"
                        If ApplicationServices.IsTouchClient Then
                            Dim jqmCss As String = String.Format("jquery.mobile-{0}.min.css", ApplicationServices.JqmVersion)
                            l.Href = ("~/touch/" + jqmCss)
                            Dim meta As HtmlMeta = New HtmlMeta()
                            meta.Attributes("name") = "viewport"
                            meta.Attributes("content") = "width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"
                            p.Header.Controls.AddAt(0, meta)
                            Dim allowCompression As Boolean = true
                            If (ApplicationServices.EnableMinifiedCss AndAlso allowCompression) Then
                                l.Href = (p.ResolveUrl(String.Format("~/appservices/stylesheet-{0}.min.css", ApplicationServices.Version)) + String.Format("?_t={0}&_cf={1}", ApplicationServices.Create().UserTheme, contentFramework))
                                l.Attributes("class") = "app-theme"
                            Else
                                For Each stylesheet As String in ApplicationServices.TouchUIStylesheets()
                                    If (Not (stylesheet.StartsWith("jquery.mobile")) AndAlso ((contentFramework = "bootstrap") OrElse Not (stylesheet.StartsWith("bootstrap")))) Then
                                        Dim cssLink As HtmlLink = New HtmlLink()
                                        cssLink.Href = String.Format("~/touch/{0}?{1}", stylesheet, ApplicationServices.Version)
                                        cssLink.Attributes("type") = "text/css"
                                        cssLink.Attributes("rel") = "stylesheet"
                                        If stylesheet.Contains("app-themes.") Then
                                            cssLink.Attributes("class") = "app-theme"
                                        End If
                                        p.Header.Controls.Add(cssLink)
                                    End If
                                Next
                            End If
                            Dim removeList As List(Of Control) = New List(Of Control)()
                            For Each c2 As Control in p.Header.Controls
                                If TypeOf c2 Is HtmlLink Then
                                    l = CType(c2,HtmlLink)
                                    If l.Href.Contains("App_Themes/") Then
                                        removeList.Add(l)
                                    End If
                                End If
                            Next
                            For Each c2 As Control in removeList
                                p.Header.Controls.Remove(c2)
                            Next
                            Return
                        Else
                            If Not (l.Href.Contains("?")) Then
                                l.Href = (l.Href + String.Format("?{0}", ApplicationServices.Version))
                            End If
                        End If
                        Return
                    End If
                End If
            Next
        End Sub
        
        Protected Overridable Function AllowTouchUIStylesheet(ByVal name As String) As Boolean
            Dim file As String = Path.GetFileName(name)
            If (file.StartsWith("app-themes") AndAlso Not (file.StartsWith(("app-themes."  _
                            + (UserTheme.ToLower() + "."))))) Then
                Return false
            End If
            Return true
        End Function
        
        Public Shared Function TouchUIStylesheets() As List(Of String)
            Return Create().EnumerateTouchUIStylesheets()
        End Function
        
        Public Overridable Function EnumerateTouchUIStylesheets() As List(Of String)
            Dim stylesheets As List(Of String) = New List(Of String)()
            stylesheets.Add(String.Format("jquery.mobile-{0}.min.css", ApplicationServices.JqmVersion))
            'enumerate custom css files
            Dim mobilePath As String = Path.GetDirectoryName(HttpContext.Current.Server.MapPath("~/touch/"))
            Dim cssFiles As SortedDictionary(Of String, String) = New SortedDictionary(Of String, String)()
            Dim minifiedCssFiles As List(Of String) = New List(Of String)()
            For Each css As String in Directory.GetFiles(mobilePath, "*.css")
                If Not (css.Contains("jquery.mobile")) Then
                    cssFiles(css) = css
                    If css.EndsWith(".min.css") Then
                        If EnableMinifiedCss Then
                            minifiedCssFiles.Add((css.Substring(0, (css.Length - 7)) + "css"))
                        Else
                            cssFiles.Remove(css)
                        End If
                    End If
                End If
            Next
            For Each css As String in minifiedCssFiles
                cssFiles.Remove(css)
            Next
            For Each fileName As String in cssFiles.Keys
                Dim cssFile As String = Path.GetFileName(fileName)
                If AllowTouchUIStylesheet(fileName) Then
                    stylesheets.Add(cssFile)
                End If
            Next
            Return stylesheets
        End Function
        
        Private Shared Function DoReplaceCssUrl(ByVal m As Match) As String
            Dim header As String = m.Groups("Header").Value
            Dim name As String = m.Groups("Name").Value
            Dim symbol As String = m.Groups("Symbol").Value
            If ((name = "data") AndAlso (symbol = ":")) Then
                Return m.Value
            End If
            Return (header  _
                        + ("../touch/"  _
                        + (name + symbol)))
        End Function
        
        Public Shared Function CombineTouchUIStylesheets(ByVal context As HttpContext) As String
            Dim response As HttpResponse = context.Response
            Dim cache As HttpCachePolicy = response.Cache
            cache.SetCacheability(HttpCacheability.Public)
            cache.VaryByHeaders("User-Agent") = true
            cache.SetOmitVaryStar(true)
            cache.SetExpires(DateTime.Now.AddDays(365))
            cache.SetValidUntilExpires(true)
            cache.SetLastModifiedFromFileDependencies()
            'combine scripts
            Dim contentFramework As String = context.Request.QueryString("_cf")
            Dim includeBootstrap As Boolean = (contentFramework = "bootstrap")
            Dim sb As StringBuilder = New StringBuilder()
            For Each stylesheet As String in ApplicationServices.TouchUIStylesheets()
                If (includeBootstrap OrElse Not (stylesheet.StartsWith("bootstrap"))) Then
                    Dim data As String = File.ReadAllText(HttpContext.Current.Server.MapPath(("~/touch/" + stylesheet)))
                    data = CssUrlRegex.Replace(data, AddressOf DoReplaceCssUrl)
                    sb.AppendLine(data)
                End If
            Next
            Return sb.ToString()
        End Function
        
        Public Shared Sub CompressOutput(ByVal context As HttpContext, ByVal data As String)
            Dim request As HttpRequest = context.Request
            Dim response As HttpResponse = context.Response
            Dim acceptEncoding As String = request.Headers("Accept-Encoding")
            If Not (String.IsNullOrEmpty(acceptEncoding)) Then
                If acceptEncoding.Contains("gzip") Then
                    response.Filter = New GZipStream(response.Filter, CompressionMode.Compress)
                    response.AppendHeader("Content-Encoding", "gzip")
                Else
                    If acceptEncoding.Contains("deflate") Then
                        response.Filter = New DeflateStream(response.Filter, CompressionMode.Compress)
                        response.AppendHeader("Content-Encoding", "deflate")
                    End If
                End If
            End If
            Dim output() As Byte = Encoding.UTF8.GetBytes(data)
            response.ContentEncoding = Encoding.Unicode
            response.AddHeader("Content-Length", output.Length.ToString())
            response.OutputStream.Write(output, 0, output.Length)
            Try 
                response.Flush()
            Catch __exception As Exception
            End Try
        End Sub
        
        Public Shared Sub HandleServiceRequest(ByVal context As HttpContext)
            Dim data((context.Request.InputStream.Length) - 1) As Byte
            context.Request.InputStream.Read(data, 0, data.Length)
            Dim json As String = Encoding.UTF8.GetString(data)
            Dim serializer As System.Web.Script.Serialization.JavaScriptSerializer = New System.Web.Script.Serialization.JavaScriptSerializer()
            Dim result As Object = Nothing
            Dim methodName As String = context.Request.Path.Substring((CType(context.Handler,Page).ResolveUrl(AquariumExtenderBase.DefaultServicePath).Length + 1))
            If ((String.IsNullOrEmpty(methodName) OrElse (String.IsNullOrEmpty(json) AndAlso Not (Regex.IsMatch(methodName, "^(Logout|ListAllPermalinks|Roles)$")))) OrElse Not ((context.Request.HttpMethod = "POST"))) Then
                context.Response.End()
            End If
            Dim v As Object = serializer.Deserialize(Of Object)(json)
            Dim args As Dictionary(Of String, Object) = CType(v,Dictionary(Of String, Object))
            Dim service As DataControllerService = New DataControllerService()
            Try 
                If (methodName = "GetPage") Then
                    Dim r As PageRequest = CType(serializer.Deserialize(Of PageRequest)(Regex.Match(json, """request"":([\s\S]+?)(}$)").Groups(1).Value),PageRequest)
                    result = service.GetPage(CType(args("controller"),String), CType(args("view"),String), r)
                Else
                    If (methodName = "GetPageList") Then
                        Dim list As List(Of PageRequest) = New List(Of PageRequest)()
                        Dim m As Match = Regex.Match(json, """requests"":\[([\s\S]+?)\]}$")
                        If m.Success Then
                            Dim m2 As Match = Regex.Match(m.Groups(1).Value, "({[\s\S]+?})(,|$)")
                            Do While m2.Success
                                list.Add(CType(serializer.Deserialize(Of PageRequest)(m2.Groups(1).Value),PageRequest))
                                m2 = m2.NextMatch()
                            Loop
                        End If
                        result = service.GetPageList(list.ToArray())
                    Else
                        If (methodName = "GetListOfValues") Then
                            Dim r As DistinctValueRequest = CType(serializer.Deserialize(Of DistinctValueRequest)(Regex.Match(json, """request"":([\s\S]+?)(}$)").Groups(1).Value),DistinctValueRequest)
                            result = service.GetListOfValues(CType(args("controller"),String), CType(args("view"),String), r)
                        Else
                            If (methodName = "Execute") Then
                                Dim a As ActionArgs = CType(serializer.Deserialize(Of ActionArgs)(Regex.Match(json, """args"":([\s\S]+?)(}$)").Groups(1).Value),ActionArgs)
                                result = service.Execute(CType(args("controller"),String), CType(args("view"),String), a)
                            Else
                                If (methodName = "GetCompletionList") Then
                                    result = service.GetCompletionList(CType(args("prefixText"),String), CType(args("count"),Integer), CType(args("contextKey"),String))
                                Else
                                    If (methodName = "Login") Then
                                        result = service.Login(CType(args("username"),String), CType(args("password"),String), CType(args("createPersistentCookie"),Boolean))
                                    Else
                                        If (methodName = "Logout") Then
                                            service.Logout()
                                        Else
                                            If (methodName = "Roles") Then
                                                result = service.Roles()
                                            Else
                                                If (methodName = "SavePermalink") Then
                                                    service.SavePermalink(CType(args("link"),String), CType(args("html"),String))
                                                Else
                                                    If (methodName = "EncodePermalink") Then
                                                        result = service.EncodePermalink(CType(args("link"),String), CType(args("rooted"),Boolean))
                                                    Else
                                                        If (methodName = "ListAllPermalinks") Then
                                                            result = service.ListAllPermalinks()
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                Do While (Not (ex.InnerException) Is Nothing)
                    ex = ex.InnerException
                Loop
                Dim [error] As ServiceRequestError = New ServiceRequestError()
                [error].Message = ex.Message
                [error].ExceptionType = ex.GetType().ToString()
                [error].StackTrace = ex.StackTrace
                result = [error]
            End Try
            If (Not (result) Is Nothing) Then
                context.Response.ContentType = "application/json; charset=utf-8"
                Dim output As String = String.Format("{{""d"":{0}}}", serializer.Serialize(result))
                Dim dataIndex As Integer = output.IndexOf(",""NewRow"":")
                If (dataIndex >= 0) Then
                    Dim metadata As String = ViewPageCompressRegex.Replace(output.Substring(0, dataIndex), String.Empty)
                    If metadata.EndsWith(",") Then
                        metadata = metadata.Substring(0, (metadata.Length - 1))
                    End If
                    output = (ViewPageCompress2Regex.Replace(metadata, "}$1") + output.Substring(dataIndex))
                End If
                ApplicationServices.CompressOutput(context, output)
            End If
            context.Response.End()
        End Sub
    End Class
    
    Public Class AnonymousUserIdentity
        Inherits Object
        Implements IIdentity
        
        ReadOnly Property IIdentity_AuthenticationType() As String Implements IIdentity.AuthenticationType
            Get
                Return "None"
            End Get
        End Property
        
        ReadOnly Property IIdentity_IsAuthenticated() As Boolean Implements IIdentity.IsAuthenticated
            Get
                Return false
            End Get
        End Property
        
        ReadOnly Property IIdentity_Name() As String Implements IIdentity.Name
            Get
                Return String.Empty
            End Get
        End Property
    End Class
    
    Partial Public Class ApplicationSiteMapProvider
        Inherits ApplicationSiteMapProviderBase
    End Class
    
    Public Class ApplicationSiteMapProviderBase
        Inherits System.Web.XmlSiteMapProvider
        
        Public Overrides Function IsAccessibleToUser(ByVal context As HttpContext, ByVal node As SiteMapNode) As Boolean
            Dim device As String = node("Device")
            Dim isTouchUI As Boolean = ApplicationServices.IsTouchClient
            If ((device = "touch") AndAlso Not (isTouchUI)) Then
                Return false
            End If
            If ((device = "desktop") AndAlso isTouchUI) Then
                Return false
            End If
            Return MyBase.IsAccessibleToUser(context, node)
        End Function
    End Class
    
    Partial Public Class PlaceholderHandler
        Inherits PlaceholderHandlerBase
    End Class
    
    Public Class PlaceholderHandlerBase
        Inherits Object
        Implements IHttpHandler
        
        Private Shared m_ImageSizeRegex As Regex = New Regex("((?'background'[a-zA-Z0-9]+?)-((?'textcolor'[a-zA-Z0-9]+?)-)?)?(?'width'[0-9]+?)("& _ 
                "x(?'height'[0-9]*))?\.[a-zA-Z][a-zA-Z][a-zA-Z]")
        
        Overridable ReadOnly Property IHttpHandler_IsReusable() As Boolean Implements IHttpHandler.IsReusable
            Get
                Return true
            End Get
        End Property
        
        Overridable Sub IHttpHandler_ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
            'Get file name
            Dim routeValues As RouteValueDictionary = context.Request.RequestContext.RouteData.Values
            Dim fileName As String = CType(routeValues("FileName"),String)
            'Get extension
            Dim ext As String = Path.GetExtension(fileName)
            Dim format As ImageFormat = ImageFormat.Png
            Dim contentType As String = "image/png"
            If (ext = ".jpg") Then
                format = ImageFormat.Jpeg
                contentType = "image/jpg"
            Else
                If (ext = ".gif") Then
                    format = ImageFormat.Gif
                    contentType = "image/jpg"
                End If
            End If
            'get width and height
            Dim regexMatch As Match = m_ImageSizeRegex.Matches(fileName)(0)
            Dim widthCapture As Capture = regexMatch.Groups("width")
            Dim width As Integer = 500
            If Not ((widthCapture.Length = 0)) Then
                width = Convert.ToInt32(widthCapture.Value)
            End If
            If (width = 0) Then
                width = 500
            End If
            If (width > 4096) Then
                width = 4096
            End If
            Dim heightCapture As Capture = regexMatch.Groups("height")
            Dim height As Integer = width
            If Not ((heightCapture.Length = 0)) Then
                height = Convert.ToInt32(heightCapture.Value)
            End If
            If (height = 0) Then
                height = 500
            End If
            If (height > 4096) Then
                height = 4096
            End If
            'Get background and text colors
            Dim background As Color = GetColor(regexMatch.Groups("background"), Color.LightGray)
            Dim textColor As Color = GetColor(regexMatch.Groups("textcolor"), Color.Black)
            Dim fontSize As Integer = ((width + height)  _
                        / 50)
            If (fontSize < 10) Then
                fontSize = 10
            End If
            Dim font As Font = New Font(FontFamily.GenericSansSerif, fontSize)
            'Get text
            Dim text As String = context.Request.QueryString("text")
            If String.IsNullOrEmpty(text) Then
                text = String.Format("{0} x {1}", width, height)
            End If
            'Get position for text
            Dim textSize As SizeF
            Using img As Image = New Bitmap(1, 1)
                Dim textDrawing As Graphics = Graphics.FromImage(img)
                textSize = textDrawing.MeasureString(text, font)
            End Using
            'Draw the image
            Using image As Image = New Bitmap(width, height)
                Dim drawing As Graphics = Graphics.FromImage(image)
                drawing.Clear(background)
                Using textBrush As Brush = New SolidBrush(textColor)
                    drawing.DrawString(text, font, textBrush, ((width - textSize.Width)  _
                                    / 2), ((height - textSize.Height)  _
                                    / 2))
                End Using
                drawing.Save()
                drawing.Dispose()
                'Return image
                Using stream As MemoryStream = New MemoryStream()
                    image.Save(stream, format)
                    Dim cache As HttpCachePolicy = context.Response.Cache
                    cache.SetCacheability(HttpCacheability.Public)
                    cache.SetOmitVaryStar(true)
                    cache.SetExpires(DateTime.Now.AddDays(365))
                    cache.SetValidUntilExpires(true)
                    cache.SetLastModifiedFromFileDependencies()
                    context.Response.ContentType = contentType
                    context.Response.AddHeader("Content-Length", Convert.ToString(stream.Length))
                    context.Response.AddHeader("File-Name", fileName)
                    context.Response.BinaryWrite(stream.ToArray())
                    context.Response.OutputStream.Flush()
                End Using
            End Using
        End Sub
        
        Private Shared Function GetColor(ByVal colorName As Capture, ByVal defaultColor As Color) As Color
            Try 
                If (colorName.Length > 0) Then
                    Dim s As String = colorName.Value
                    If Regex.IsMatch(s, "^[0-9abcdef]{3,6}$") Then
                        s = ("#" + s)
                    End If
                    Return ColorTranslator.FromHtml(s)
                End If
            Catch __exception As Exception
            End Try
            Return defaultColor
        End Function
    End Class
    
    Public Class GenericRoute
        Inherits Object
        Implements IRouteHandler
        
        Private m_Handler As IHttpHandler
        
        Public Sub New(ByVal handler As IHttpHandler)
            MyBase.New
            m_Handler = handler
        End Sub
        
        Function IRouteHandler_GetHttpHandler(ByVal context As RequestContext) As IHttpHandler Implements IRouteHandler.GetHttpHandler
            Return m_Handler
        End Function
        
        Public Shared Sub Map(ByVal routes As RouteCollection, ByVal handler As IHttpHandler, ByVal url As String)
            Dim r As Route = New Route(url, New GenericRoute(handler))
            r.Defaults = New RouteValueDictionary()
            r.Constraints = New RouteValueDictionary()
            routes.Add(r)
        End Sub
    End Class
End Namespace

Imports MyCompany.Data
Imports MyCompany.Services
Imports System
Imports System.Collections.Generic
Imports System.Configuration
Imports System.Data
Imports System.Globalization
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.HtmlControls
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebParts

Namespace MyCompany.Web
    
    Public Class AquariumFieldEditorAttribute
        Inherits Attribute
    End Class
    
    Public Class AquariumExtenderBase
        Inherits ExtenderControl
        
        Private m_ClientComponentName As String
        
        Public Shared DefaultServicePath As String = "~/_invoke"
        
        Private m_ServicePath As String
        
        Private m_Properties As SortedDictionary(Of String, Object)
        
        Private Shared m_EnableCombinedScript As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_IgnoreCombinedScript As Boolean
        
        Public Sub New(ByVal clientComponentName As String)
            MyBase.New
            Me.m_ClientComponentName = clientComponentName
        End Sub
        
        <System.ComponentModel.Description("A path to a data controller web service."),  _
         System.ComponentModel.DefaultValue("~/_invoke")>  _
        Public Overridable Property ServicePath() As String
            Get
                If String.IsNullOrEmpty(m_ServicePath) Then
                    Return AquariumExtenderBase.DefaultServicePath
                End If
                Return m_ServicePath
            End Get
            Set
                m_ServicePath = value
            End Set
        End Property
        
        <System.ComponentModel.Browsable(false)>  _
        Public ReadOnly Property Properties() As SortedDictionary(Of String, Object)
            Get
                If (m_Properties Is Nothing) Then
                    m_Properties = New SortedDictionary(Of String, Object)()
                End If
                Return m_Properties
            End Get
        End Property
        
        Public Shared Property EnableCombinedScript() As Boolean
            Get
                Return m_EnableCombinedScript
            End Get
            Set
                m_EnableCombinedScript = value
            End Set
        End Property
        
        Public Property IgnoreCombinedScript() As Boolean
            Get
                Return Me.m_IgnoreCombinedScript
            End Get
            Set
                Me.m_IgnoreCombinedScript = value
            End Set
        End Property
        
        Protected Overrides Function GetScriptDescriptors(ByVal targetControl As Control) As System.Collections.Generic.IEnumerable(Of ScriptDescriptor)
            If (Not (Site) Is Nothing) Then
                Return Nothing
            End If
            If ScriptManager.GetCurrent(Page).IsInAsyncPostBack Then
                Dim requireRegistration As Boolean = false
                Dim c As Control = Me
                Do While (Not (requireRegistration) AndAlso ((Not (c) Is Nothing) AndAlso Not (TypeOf c Is HtmlForm)))
                    If TypeOf c Is UpdatePanel Then
                        requireRegistration = true
                    End If
                    c = c.Parent
                Loop
                If Not (requireRegistration) Then
                    Return Nothing
                End If
            End If
            Dim descriptor As ScriptBehaviorDescriptor = New ScriptBehaviorDescriptor(m_ClientComponentName, targetControl.ClientID)
            descriptor.AddProperty("id", Me.ClientID)
            Dim baseUrl As String = ResolveClientUrl("~")
            If (baseUrl = "~") Then
                baseUrl = String.Empty
            End If
            Dim isTouchUI As Boolean = ApplicationServices.IsTouchClient
            If Not (isTouchUI) Then
                descriptor.AddProperty("baseUrl", baseUrl)
                descriptor.AddProperty("servicePath", ResolveClientUrl(ServicePath))
            End If
            ConfigureDescriptor(descriptor)
            Return New ScriptBehaviorDescriptor() {descriptor}
        End Function
        
        Protected Overridable Sub ConfigureDescriptor(ByVal descriptor As ScriptBehaviorDescriptor)
        End Sub
        
        Public Shared Function CreateScriptReference(ByVal p As String) As ScriptReference
            Dim culture As CultureInfo = Thread.CurrentThread.CurrentUICulture
            Dim scripts As List(Of String) = CType(HttpRuntime.Cache("AllApplicationScripts"),List(Of String))
            If (scripts Is Nothing) Then
                scripts = New List(Of String)()
                Dim files() As String = Directory.GetFiles(HttpContext.Current.Server.MapPath("~/Scripts"), "*.js")
                For Each script As String in files
                    Dim m As Match = Regex.Match(Path.GetFileName(script), "^(.+?)\.(\w\w(\-\w+)*)\.js$")
                    If m.Success Then
                        scripts.Add(m.Value)
                    End If
                Next
                HttpRuntime.Cache("AllApplicationScripts") = scripts
            End If
            If (scripts.Count > 0) Then
                Dim name As Match = Regex.Match(p, "^(?'Path'.+\/)(?'Name'.+?)\.js$")
                If name.Success Then
                    Dim test As String = String.Format("{0}.{1}.js", name.Groups("Name").Value, culture.Name)
                    Dim success As Boolean = scripts.Contains(test)
                    If Not (success) Then
                        test = String.Format("{0}.{1}.js", name.Groups("Name").Value, culture.Name.Substring(0, 2))
                        success = scripts.Contains(test)
                    End If
                    If success Then
                        p = (name.Groups("Path").Value + test)
                    End If
                End If
            End If
            p = (p + String.Format("?{0}", ApplicationServices.Version))
            Return New ScriptReference(p)
        End Function
        
        Protected Overrides Function GetScriptReferences() As System.Collections.Generic.IEnumerable(Of ScriptReference)
            If (Not (Site) Is Nothing) Then
                Return Nothing
            End If
            If ((Not (Page) Is Nothing) AndAlso ScriptManager.GetCurrent(Page).IsInAsyncPostBack) Then
                Return Nothing
            End If
            Dim scripts As List(Of ScriptReference) = New List(Of ScriptReference)()
            If (EnableCombinedScript AndAlso Not (IgnoreCombinedScript)) Then
                Return scripts
            End If
            Dim isMobile As Boolean = ApplicationServices.IsTouchClient
            scripts.Add(CreateScriptReference("~/Scripts/_System.js"))
            If isMobile Then
                scripts.Add(CreateScriptReference(String.Format("~/touch/jquery.mobile-{0}.min.js", ApplicationServices.JqmVersion)))
            End If
            scripts.Add(CreateScriptReference("~/Scripts/MicrosoftAjax.js"))
            If Not (isMobile) Then
                scripts.Add(CreateScriptReference("~/Scripts/AjaxControlToolkit.js"))
                scripts.Add(CreateScriptReference("~/Scripts/MicrosoftAjaxWebForms.js"))
            End If
            scripts.Add(CreateScriptReference("~/Scripts/daf-resources.js"))
            If Not (isMobile) Then
                scripts.Add(CreateScriptReference("~/Scripts/daf-menu.js"))
            End If
            scripts.Add(CreateScriptReference("~/Scripts/daf.js"))
            If (Not (isMobile) AndAlso New ControllerUtilities().SupportsScrollingInDataSheet) Then
                scripts.Add(CreateScriptReference("~/Scripts/daf-extensions.js"))
            End If
            If EnableCombinedScript Then
                scripts.Add(CreateScriptReference("~/Scripts/daf-membership.js"))
            End If
            ConfigureScripts(scripts)
            If isMobile Then
                scripts.Add(CreateScriptReference("~/Scripts/touch.js"))
                scripts.Add(CreateScriptReference("~/Scripts/touch-charts.js"))
            End If
            If (Context.Request.Url.Host.Equals("localhost") AndAlso File.Exists(Context.Server.MapPath("~/Scripts/codeontime.designer.js"))) Then
                scripts.Add(CreateScriptReference("~/Scripts/codeontime.designer.js"))
            End If
            Return scripts
        End Function
        
        Protected Overridable Sub ConfigureScripts(ByVal scripts As List(Of ScriptReference))
        End Sub
        
        Protected Overrides Sub OnLoad(ByVal e As EventArgs)
            If ScriptManager.GetCurrent(Page).IsInAsyncPostBack Then
                Return
            End If
            MyBase.OnLoad(e)
            If (Not (Site) Is Nothing) Then
                Return
            End If
            RegisterFrameworkSettings(Page)
        End Sub
        
        Public Shared Sub RegisterFrameworkSettings(ByVal p As Page)
            If Not (p.ClientScript.IsStartupScriptRegistered(GetType(AquariumExtenderBase), "TargetFramework")) Then
                Dim siteContent As String = String.Empty
                If ApplicationServices.IsContentEditor Then
                    siteContent = String.Format(",siteContent:""{0}"",siteContentPK:""{1}"",rootUrl:""{2}""", ApplicationServices.SiteContentControllerName, ApplicationServices.Create().SiteContentFieldName(SiteContentFields.SiteContentId), p.ResolveUrl("~"))
                End If
                p.ClientScript.RegisterStartupScript(GetType(AquariumExtenderBase), "TargetFramework", String.Format("var __targetFramework=""4.6"",__tf=4.0,__servicePath=""{0}"",__baseUrl=""{1}"",__design"& _ 
                            "erPort=""{2}"";", p.ResolveClientUrl(AquariumExtenderBase.DefaultServicePath), p.ResolveClientUrl("~"), ApplicationServices.DesignerPort), true)
                p.ClientScript.RegisterStartupScript(GetType(AquariumExtenderBase), "TouchUI", String.Format("var __settings={{appInfo:""Test2|{0}"",mobileDisplayDensity:""Auto"",desktopDisplayDe"& _ 
                            "nsity:""Condensed"",mapApiIdentifier:"""",labelsInList:""DisplayedBelow"",labelsInForm"& _ 
                            ":""AlignedLeft"",initialListMode:""SeeAll"",buttonShapes:true,sidebar:""Landscape"",pr"& _ 
                            "omoteActions:true,transitions:"""",theme:""Light"",maxPivotRowCount: {1},help:true,u"& _ 
                            "i:""TouchUI""{2}}};", BusinessRules.JavaScriptString(HttpContext.Current.User.Identity.Name), ApplicationServices.Create().MaxPivotRowCount, siteContent), true)
            End If
        End Sub
        
        Public Overloads Shared Function StandardScripts() As List(Of ScriptReference)
            Return StandardScripts(false)
        End Function
        
        Public Overloads Shared Function StandardScripts(ByVal ignoreCombinedScriptFlag As Boolean) As List(Of ScriptReference)
            Dim extender As AquariumExtenderBase = New AquariumExtenderBase(Nothing)
            extender.IgnoreCombinedScript = ignoreCombinedScriptFlag
            Dim list As List(Of ScriptReference) = New List(Of ScriptReference)()
            list.AddRange(extender.GetScriptReferences())
            Return list
        End Function
        
        Protected Overrides Sub OnPreRender(ByVal e As EventArgs)
            MyBase.OnPreRender(e)
        End Sub
    End Class
End Namespace

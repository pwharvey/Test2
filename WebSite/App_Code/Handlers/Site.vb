Imports MyCompany.Data
Imports MyCompany.Services
Imports MyCompany.Web
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Web.Caching
Imports System.Web.Configuration
Imports System.Web.Hosting
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.HtmlControls
Imports System.Web.UI.WebControls
Imports System.Xml.Linq

Namespace MyCompany.Handlers
    
    Partial Public Class Site
        Inherits SiteBase
        
        Shared Sub New()
            AquariumExtenderBase.EnableCombinedScript = true
            ApplicationServices.EnableMinifiedCss = true
        End Sub
    End Class
    
    Public Class SiteBase
        Inherits MyCompany.Web.PageBase
        
        Private m_IsTouchUI As Boolean
        
        Private m_BodyAttributes As AttributeDictionary
        
        Private m_BodyTag As LiteralControl
        
        Private m_PageHeaderContent As LiteralContainer
        
        Private m_PageTitleContent As LiteralContainer
        
        Private m_HeadContent As LiteralContainer
        
        Private m_PageContent As LiteralContainer
        
        Private m_PageFooterContent As LiteralContainer
        
        Private m_PageSideBarContent As LiteralContainer
        
        Private m_ContentFramework As String
        
        Public Shared MicrosoftJavaScript() As String = New String() {"MicrosoftAjax.js", "MicrosoftAjaxWebForms.js"}
        
        Public Overrides ReadOnly Property Device() As String
            Get
                Return m_BodyAttributes("data-device")
            End Get
        End Property
        
        Public Function ResolveAppUrl(ByVal html As String) As String
            Dim appPath As String = Request.ApplicationPath
            If Not (appPath.EndsWith("/")) Then
                appPath = (appPath + "/")
            End If
            Return html.Replace("=""~/", ("=""" + appPath))
        End Function
        
        Protected Overrides Sub OnInit(ByVal e As EventArgs)
            If Request.Path.StartsWith((ResolveUrl(AquariumExtenderBase.DefaultServicePath) + "/"), StringComparison.CurrentCultureIgnoreCase) Then
                ApplicationServices.HandleServiceRequest(Context)
            End If
            If (Request.Params("_page") = "_blank") Then
                Return
            End If
            Dim link As String = Request.Params("_link")
            If Not (String.IsNullOrEmpty(link)) Then
                Dim enc As StringEncryptor = New StringEncryptor()
                Dim permalink() As String = enc.Decrypt(link.Split(Global.Microsoft.VisualBasic.ChrW(44))(0)).Split(Global.Microsoft.VisualBasic.ChrW(63))
                Page.ClientScript.RegisterStartupScript([GetType](), "Redirect", String.Format("window.location.replace('0?_link=1');", permalink(0), HttpUtility.UrlEncode(link)), true)
                Return
            Else
                Dim requestUrl As String = Request.RawUrl
                If ((requestUrl.Length > 1) AndAlso requestUrl.EndsWith("/")) Then
                    requestUrl = requestUrl.Substring(0, (requestUrl.Length - 1))
                End If
                If Request.ApplicationPath.Equals(requestUrl, StringComparison.CurrentCultureIgnoreCase) Then
                    Dim homePageUrl As String = ApplicationServices.HomePageUrl
                    If Not (Request.ApplicationPath.Equals(homePageUrl)) Then
                        Response.Redirect(homePageUrl)
                    End If
                End If
            End If
            Dim contentInfo As SortedDictionary(Of String, String) = ApplicationServices.LoadContent()
            InitializeSiteMaster()
            Dim s As String = Nothing
            If Not (contentInfo.TryGetValue("PageTitle", s)) Then
                s = ApplicationServices.Current.Name
            End If
            Me.Title = s
            If (Not (m_PageTitleContent) Is Nothing) Then
                If m_IsTouchUI Then
                    m_PageTitleContent.Text = String.Empty
                Else
                    m_PageTitleContent.Text = s
                End If
            End If
            Dim appName As HtmlMeta = New HtmlMeta()
            appName.Name = "application-name"
            appName.Content = ApplicationServices.Current.Name
            Header.Controls.Add(appName)
            If (contentInfo.TryGetValue("Head", s) AndAlso (Not (m_HeadContent) Is Nothing)) Then
                m_HeadContent.Text = s
            End If
            If (contentInfo.TryGetValue("PageContent", s) AndAlso (Not (m_PageContent) Is Nothing)) Then
                If m_IsTouchUI Then
                    s = String.Format("<div id=""PageContent"" style=""display:none"">{0}</div>", s)
                End If
                Dim userControl As Match = Regex.Match(s, "<div\s+data-user-control\s*=s*""([\s\S]+?)""\s*>\s*</div>")
                If userControl.Success Then
                    Dim startPos As Integer = 0
                    Do While userControl.Success
                        m_PageContent.Controls.Add(New LiteralControl(s.Substring(startPos, (userControl.Index - startPos))))
                        startPos = (userControl.Index + userControl.Length)
                        Dim controlFileName As String = userControl.Groups(1).Value
                        Dim controlExtension As String = Path.GetExtension(controlFileName)
                        Dim siteControlText As String = Nothing
                        If Not (controlFileName.StartsWith("~")) Then
                            controlFileName = (controlFileName + "~")
                        End If
                        If String.IsNullOrEmpty(controlExtension) Then
                            Dim testFileName As String = (controlFileName + ".ascx")
                            If File.Exists(Server.MapPath(testFileName)) Then
                                controlFileName = testFileName
                                controlExtension = ".ascx"
                            Else
                                If ApplicationServices.IsSiteContentEnabled Then
                                    Dim relativeControlPath As String = controlFileName.Substring(1)
                                    If relativeControlPath.StartsWith("/") Then
                                        relativeControlPath = relativeControlPath.Substring(1)
                                    End If
                                    siteControlText = ApplicationServices.Current.ReadSiteContentString(("sys/" + relativeControlPath))
                                End If
                                If (siteControlText Is Nothing) Then
                                    testFileName = (controlFileName + ".html")
                                    If File.Exists(Server.MapPath(testFileName)) Then
                                        controlFileName = testFileName
                                        controlExtension = ".html"
                                    End If
                                End If
                            End If
                        End If
                        Try 
                            If (controlExtension = ".ascx") Then
                                m_PageContent.Controls.Add(LoadControl(controlFileName))
                            Else
                                Dim controlText As String = siteControlText
                                If (controlText Is Nothing) Then
                                    controlText = File.ReadAllText(Server.MapPath(controlFileName))
                                End If
                                Dim bodyMatch As Match = Regex.Match(controlText, "<body[\s\S]*?>([\s\S]+?)</body>")
                                If bodyMatch.Success Then
                                    controlText = bodyMatch.Groups(1).Value
                                End If
                                controlText = Localizer.Replace("Controls", Path.GetFileName(Server.MapPath(controlFileName)), controlText)
                                m_PageContent.Controls.Add(New LiteralControl(controlText))
                            End If
                        Catch ex As Exception
                            m_PageContent.Controls.Add(New LiteralControl(String.Format("Error loading '{0}': {1}", controlFileName, ex.Message)))
                        End Try
                        userControl = userControl.NextMatch()
                    Loop
                    If (startPos < s.Length) Then
                        m_PageContent.Controls.Add(New LiteralControl(s.Substring(startPos)))
                    End If
                Else
                    m_PageContent.Text = s
                End If
                If s.Contains("data-content-framework=""bootstrap""") Then
                    m_ContentFramework = "bootstrap"
                End If
            Else
                If m_IsTouchUI Then
                    m_PageContent.Text = "<div id=""PageContent"" style=""display:none""><div data-app-role=""page"">404 Not Foun"& _ 
                        "d</div></div>"
                    Me.Title = "Test 2"
                Else
                    m_PageContent.Text = "404 Not Found"
                End If
            End If
            If m_IsTouchUI Then
                If (Not (m_PageFooterContent) Is Nothing) Then
                    m_PageFooterContent.Text = "<footer style=""display:none""><small>&copy; 2016 MyCompany. ^Copyright^All rights "& _ 
                        "reserved.^Copyright^</small></footer>"
                End If
            Else
                If contentInfo.TryGetValue("About", s) Then
                    If (Not (m_PageSideBarContent) Is Nothing) Then
                        m_PageSideBarContent.Text = String.Format("<div class=""TaskBox About""><div class=""Inner""><div class=""Header"">About</div><div"& _ 
                                " class=""Value"">{0}</div></div></div>", s)
                    End If
                End If
            End If
            Dim bodyAttributes As String = Nothing
            If contentInfo.TryGetValue("BodyAttributes", bodyAttributes) Then
                m_BodyAttributes.Parse(bodyAttributes)
            End If
            If Not (ApplicationServices.UserIsAuthorizedToAccessResource(HttpContext.Current.Request.Path, m_BodyAttributes("data-authorize-roles"))) Then
                Dim requestPath As String = Request.Path.Substring(1)
                If WorkflowRegister.IsEnabled Then
                    If Not (WorkflowRegister.Allows(requestPath)) Then
                        FormsAuthentication.RedirectToLoginPage()
                    End If
                Else
                    FormsAuthentication.RedirectToLoginPage()
                End If
            End If
            m_BodyAttributes.Remove("data-authorize-roles")
            If Not (m_IsTouchUI) Then
                Dim classAttr As String = m_BodyAttributes("class")
                If String.IsNullOrEmpty(classAttr) Then
                    classAttr = String.Empty
                End If
                If Not (classAttr.Contains("Wide")) Then
                    classAttr = (classAttr + " Standard")
                End If
                classAttr = ((classAttr + " ")  _
                            + (Regex.Replace(Request.Path.ToLower(), "\W", "_").Substring(1) + "_html"))
                m_BodyAttributes("class") = classAttr.Trim()
            End If
            m_BodyTag.Text = String.Format("" & ControlChars.CrLf &"<body{0}>" & ControlChars.CrLf , m_BodyAttributes.ToString())
            MyBase.OnInit(e)
        End Sub
        
        Protected Overridable Sub InitializeSiteMaster()
            m_IsTouchUI = ApplicationServices.IsTouchClient
            Dim html As String = String.Empty
            Dim siteMasterPath As String = "~/site.desktop.html"
            If m_IsTouchUI Then
                siteMasterPath = "~/site.touch.html"
            End If
            siteMasterPath = Server.MapPath(siteMasterPath)
            If Not (File.Exists(siteMasterPath)) Then
                siteMasterPath = Server.MapPath("~/site.html")
            End If
            If File.Exists(siteMasterPath) Then
                html = File.ReadAllText(siteMasterPath)
            Else
                Throw New Exception("File site.html has not been found.")
            End If
            Dim htmlMatch As Match = Regex.Match(html, "<html(?'HtmlAttr'[\S\s]*?)>\s*<head(?'HeadAttr'[\S\s]*?)>\s*(?'Head'[\S\s]*?)\s*<"& _ 
                    "/head>\s*<body(?'BodyAttr'[\S\s]*?)>\s*(?'Body'[\S\s]*?)\s*</body>\s*</html>\s*")
            If Not (htmlMatch.Success) Then
                Throw New Exception("File site.html must contain 'head' and 'body' elements.")
            End If
            'instructions
            Controls.Add(New LiteralControl(html.Substring(0, htmlMatch.Index)))
            'html
            Controls.Add(New LiteralControl(String.Format("<html{0} xml:lang={1} lang=""{1}"">" & ControlChars.CrLf , htmlMatch.Groups("HtmlAttr").Value, CultureInfo.CurrentUICulture.IetfLanguageTag)))
            'head
            Controls.Add(New HtmlHead())
            If m_IsTouchUI Then
                Header.Controls.Add(New LiteralControl("<meta charset=""utf-8"">" & ControlChars.CrLf ))
            Else
                Header.Controls.Add(New LiteralControl("<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">" & ControlChars.CrLf ))
            End If
            Dim headHtml As String = Regex.Replace(htmlMatch.Groups("Head").Value, "\s*<title([\s\S+]*?title>)\s*", String.Empty)
            Header.Controls.Add(New LiteralControl(headHtml))
            m_HeadContent = New LiteralContainer()
            Header.Controls.Add(m_HeadContent)
            'body
            m_BodyTag = New LiteralControl()
            m_BodyAttributes = New AttributeDictionary(htmlMatch.Groups("BodyAttr").Value)
            Controls.Add(m_BodyTag)
            Dim themePath As String = Server.MapPath("~/App_Themes/MyCompany")
            If Directory.Exists(themePath) Then
                For Each stylesheetFileName As String in Directory.GetFiles(themePath, "*.css")
                    Dim fileName As String = Path.GetFileName(stylesheetFileName)
                    If Not (fileName.Equals("_Theme_Aquarium.css")) Then
                        Dim link As HtmlLink = New HtmlLink()
                        link.Href = ("~/App_Themes/MyCompany/" + fileName)
                        link.Attributes("type") = "text/css"
                        link.Attributes("rel") = "stylesheet"
                        Header.Controls.Add(link)
                    End If
                Next
            End If
            'form
            Controls.Add(New HtmlForm())
            Form.ID = "aspnetForm"
            'ScriptManager
            Dim sm As ScriptManager = New ScriptManager()
            sm.ID = "sm"
            sm.EnableScriptGlobalization = true
            If AquariumExtenderBase.EnableCombinedScript Then
                sm.EnableScriptLocalization = false
            End If
            sm.ScriptMode = ScriptMode.Release
            AddHandler sm.ResolveScriptReference, AddressOf Me.sm_ResolveScriptReference
            Form.Controls.Add(sm)
            'SiteMapDataSource
            Dim siteMapDataSource1 As SiteMapDataSource = New SiteMapDataSource()
            siteMapDataSource1.ID = "SiteMapDataSource1"
            siteMapDataSource1.ShowStartingNode = false
            Form.Controls.Add(siteMapDataSource1)
            'parse and initialize placeholders
            Dim body As String = htmlMatch.Groups("Body").Value
            Dim placeholderMatch As Match = Regex.Match(body, "<div\s+data-role\s*=\s*""placeholder""(?'Attributes'[\s\S]+?)>\s*(?'DefaultContent'"& _ 
                    "[\s\S]*?)\s*</div>")
            Dim startPos As Integer = 0
            Do While placeholderMatch.Success
                Dim attributes As AttributeDictionary = New AttributeDictionary(placeholderMatch.Groups("Attributes").Value)
                'create placeholder content
                Form.Controls.Add(New LiteralControl(body.Substring(startPos, (placeholderMatch.Index - startPos))))
                Dim placeholder As String = attributes("data-placeholder")
                Dim defaultContent As String = placeholderMatch.Groups("DefaultContent").Value
                If Not (CreatePlaceholder(Form.Controls, placeholder, defaultContent, attributes)) Then
                    Dim placeholderControl As LiteralContainer = New LiteralContainer()
                    placeholderControl.Text = defaultContent
                    Form.Controls.Add(placeholderControl)
                    If (placeholder = "page-header") Then
                        m_PageHeaderContent = placeholderControl
                    End If
                    If (placeholder = "page-title") Then
                        m_PageTitleContent = placeholderControl
                    End If
                    If (placeholder = "page-side-bar") Then
                        m_PageSideBarContent = placeholderControl
                    End If
                    If (placeholder = "page-content") Then
                        m_PageContent = placeholderControl
                    End If
                    If (placeholder = "page-footer") Then
                        m_PageFooterContent = placeholderControl
                    End If
                End If
                startPos = (placeholderMatch.Index + placeholderMatch.Length)
                placeholderMatch = placeholderMatch.NextMatch()
            Loop
            If (startPos < body.Length) Then
                Form.Controls.Add(New LiteralControl(body.Substring(startPos)))
            End If
            'end body
            Controls.Add(New LiteralControl("" & ControlChars.CrLf &"</body>" & ControlChars.CrLf ))
            'end html
            Controls.Add(New LiteralControl("" & ControlChars.CrLf &"</html>" & ControlChars.CrLf ))
        End Sub
        
        Protected Overridable Function CreatePlaceholder(ByVal container As ControlCollection, ByVal placeholder As String, ByVal defaultContent As String, ByVal attributes As AttributeDictionary) As Boolean
            If (placeholder = "membership-bar") Then
                Dim mb As MembershipBar = New MembershipBar()
                mb.ID = "mb"
                If (attributes("data-display-remember-me") = "false") Then
                    mb.DisplayRememberMe = false
                End If
                If (attributes("data-remember-me-set") = "true") Then
                    mb.RememberMeSet = true
                End If
                If (attributes("data-display-password-recovery") = "false") Then
                    mb.DisplayPasswordRecovery = false
                End If
                If (attributes("data-display-sign-up") = "false") Then
                    mb.DisplaySignUp = false
                End If
                If (attributes("data-display-my-account") = "false") Then
                    mb.DisplayMyAccount = false
                End If
                If (attributes("data-display-help") = "false") Then
                    mb.DisplayHelp = false
                End If
                If (attributes("data-display-login") = "false") Then
                    mb.DisplayLogin = false
                End If
                If Not (String.IsNullOrEmpty(attributes("data-idle-user-timeout"))) Then
                    mb.IdleUserTimeout = Convert.ToInt32(attributes("data-idle-user-timeout"))
                End If
                If (attributes("data-enable-history") = "true") Then
                    mb.EnableHistory = true
                End If
                If (attributes("data-enable-permalinks") = "true") Then
                    mb.EnablePermalinks = true
                End If
                container.Add(mb)
                Return true
            End If
            If (placeholder = "menu-bar") Then
                Dim menuDiv As HtmlGenericControl = New HtmlGenericControl()
                menuDiv.TagName = "div"
                menuDiv.ID = "PageMenuBar"
                menuDiv.Attributes("class") = "PageMenuBar"
                container.Add(menuDiv)
                Dim menu As MenuExtender = New MenuExtender()
                menu.ID = "Menu1"
                menu.DataSourceID = "SiteMapDataSource1"
                menu.TargetControlID = menuDiv.ID
                menu.HoverStyle = CType(TypeDescriptor.GetConverter(GetType(MenuHoverStyle)).ConvertFromString(attributes.ValueOf("data-hover-style", "Auto")),MenuHoverStyle)
                menu.PopupPosition = CType(TypeDescriptor.GetConverter(GetType(MenuPopupPosition)).ConvertFromString(attributes.ValueOf("data-popup-position", "Left")),MenuPopupPosition)
                menu.ShowSiteActions = (attributes("data-show-site-actions") = "true")
                menu.PresentationStyle = CType(TypeDescriptor.GetConverter(GetType(MenuPresentationStyle)).ConvertFromString(attributes.ValueOf("data-presentation-style", "MultiLevel")),MenuPresentationStyle)
                container.Add(menu)
                Return true
            End If
            If (placeholder = "site-map-path") Then
                Dim siteMapPath1 As SiteMapPath = New SiteMapPath()
                siteMapPath1.ID = "SiteMapPath1"
                siteMapPath1.CssClass = "SiteMapPath"
                siteMapPath1.PathSeparatorStyle.CssClass = "PathSeparator"
                siteMapPath1.CurrentNodeStyle.CssClass = "CurrentNode"
                siteMapPath1.NodeStyle.CssClass = "Node"
                siteMapPath1.RootNodeStyle.CssClass = "RootNode"
                container.Add(siteMapPath1)
                Return true
            End If
            Return false
        End Function
        
        Protected Sub sm_ResolveScriptReference(ByVal sender As Object, ByVal e As ScriptReferenceEventArgs)
            If (System.Array.IndexOf(MicrosoftJavaScript, e.Script.Name) >= 0) Then
                If AquariumExtenderBase.EnableCombinedScript Then
                    Dim lang As String = CultureInfo.CurrentUICulture.IetfLanguageTag.ToLower()
                    Dim scriptPath As String = ResolveUrl(String.Format("~/appservices/combined-{0}.{1}.js", ApplicationServices.Version, lang))
                    If ApplicationServices.IsTouchClient Then
                        scriptPath = String.Format("{0}?_spa", scriptPath)
                    End If
                    e.Script.Path = scriptPath
                    e.Script.ResourceUICultures = Nothing
                    Return
                End If
                e.Script.Path = String.Format("~/Scripts/{0}?{1}", e.Script.Name, ApplicationServices.Version)
            End If
        End Sub
        
        Protected Overrides Sub OnPreRender(ByVal e As EventArgs)
            ApplicationServices.RegisterCssLinks(Me, m_ContentFramework)
            If m_IsTouchUI Then
                'hide top-level literals
                For Each c As Control in Form.Controls
                    If TypeOf c Is LiteralControl Then
                        c.Visible = false
                    End If
                Next
                'look deep in children for ASP.NET controls
                HideAspNetControls(Form.Controls)
            End If
            MyBase.OnPreRender(e)
        End Sub
        
        Protected Overrides Sub Render(ByVal writer As HtmlTextWriter)
            'create page content
            Dim sb As StringBuilder = New StringBuilder()
            Dim w As HtmlTextWriter = New HtmlTextWriter(New StringWriter(sb))
            MyBase.Render(w)
            w.Flush()
            w.Close()
            Dim content As String = sb.ToString()
            If m_IsTouchUI Then
                'perform cleanup for super lightweight output
                content = Regex.Replace(content, "(<body([\s\S]*?)>\s*)<form\s+([\s\S]*?)</div>\s*", "$1")
                content = Regex.Replace(content, "\s*</form>\s*(</body>)", "" & ControlChars.CrLf &"$1")
                content = Regex.Replace(content, "<script(?'Attributes'[\s\S]*?)>(?'Script'[\s\S]*?)</script>\s*", AddressOf DoValidateScript)
                content = Regex.Replace(content, "<title>\s*([\s\S]+?)\s*</title>", "<title>$1</title>")
                content = Regex.Replace(content, "<div>\s*<input([\s\S]+?)VIEWSTATEGENERATOR([\s\S]+?)</div>", String.Empty)
                content = Regex.Replace(content, "<div.+?></div>.+?(<div.+?class=""PageMenuBar""></div>)\s*", String.Empty)
                content = Regex.Replace(content, "\$get\("".*?mb_d""\)", "null")
                content = Regex.Replace(content, "\s*(<footer[\s\S]+?</small></footer>)\s*", "$1")
                content = Regex.Replace(content, "\s*type=""text/javascript""\s*", " ")
            End If
            content = Regex.Replace(content, "(>\s+)//<\!\[CDATA\[\s*", "$1")
            content = Regex.Replace(content, "\s*//\]\]>\s*</script>", "" & ControlChars.CrLf &"</script>")
            content = Regex.Replace(content, "<div\s+data-role\s*=""placeholder""\s+(?'Attributes'[\s\S]+?)>(?'DefaultContent'[\s"& _ 
                    "\S]*?)</div>", AddressOf DoReplacePlaceholder)
            content = ResolveAppUrl(content)
            ApplicationServices.CompressOutput(Context, content)
        End Sub
        
        Private Function DoReplacePlaceholder(ByVal m As Match) As String
            Dim attributes As AttributeDictionary = New AttributeDictionary(m.Groups("Attributes").Value)
            Dim defaultContent As String = m.Groups("DefaultContent").Value
            Dim replacement As String = ReplaceStaticPlaceholder(attributes("data-placeholder"), attributes, defaultContent)
            If (replacement Is Nothing) Then
                Return m.Value
            Else
                Return replacement
            End If
        End Function
        
        Public Overridable Function ReplaceStaticPlaceholder(ByVal name As String, ByVal attributes As AttributeDictionary, ByVal defaultContent As String) As String
            Return Nothing
        End Function
        
        Private Sub HideAspNetControls(ByVal controls As ControlCollection)
            Dim i As Integer = 0
            Do While (i < controls.Count)
                Dim c As Control = controls(i)
                If (TypeOf c Is SiteMapPath OrElse (TypeOf c Is Image OrElse TypeOf c Is TreeView)) Then
                    controls.Remove(c)
                Else
                    HideAspNetControls(c.Controls)
                    i = (i + 1)
                End If
            Loop
        End Sub
        
        Private Function DoValidateScript(ByVal m As Match) As String
            Dim script As String = m.Groups("Script").Value
            If script.Contains("aspnetForm") Then
                Return String.Empty
            End If
            Dim srcMatch As Match = Regex.Match(m.Groups("Attributes").Value, "src=""(.+?)""")
            If srcMatch.Success Then
                Dim src As String = srcMatch.Groups(1).Value
                If src.Contains(".axd?") Then
                    Try 
                        Dim client As WebClient = New WebClient()
                        script = client.DownloadString(String.Format("http://{0}/{1}", Request.Url.Authority, src))
                    Catch __exception As Exception
                        Return script
                    End Try
                    If script.Contains("WebForm_PostBack") Then
                        Return String.Empty
                    End If
                End If
            End If
            script = m.Value.Replace("WebForm_InitCallback();", String.Empty)
            Return script
        End Function
    End Class
    
    Public Class LiteralContainer
        Inherits Panel
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Text As String
        
        Public Property Text() As String
            Get
                Return Me.m_Text
            End Get
            Set
                Me.m_Text = value
            End Set
        End Property
        
        Protected Overrides Sub Render(ByVal output As HtmlTextWriter)
            If (Controls.Count > 0) Then
                For Each c As Control in Controls
                    c.RenderControl(output)
                Next
            Else
                output.Write(Text)
            End If
        End Sub
    End Class
    
    Public Class AttributeDictionary
        Inherits SortedDictionary(Of String, String)
        
        Public Sub New(ByVal attributes As String)
            MyBase.New
            Parse(attributes)
        End Sub
        
        Public Shadows Default Property Item(ByVal name As String) As String
            Get
                Return Me.ValueOf(name, Nothing)
            End Get
            Set
                If (value Is Nothing) Then
                    Remove(name)
                Else
                    MyBase.Item(name) = value
                End If
            End Set
        End Property
        
        Public Function ValueOf(ByVal name As String, ByVal defaultValue As String) As String
            Dim v As String = Nothing
            If Not (TryGetValue(name, v)) Then
                v = defaultValue
            End If
            Return v
        End Function
        
        Public Sub Parse(ByVal attributes As String)
            Dim attributeMatch As Match = Regex.Match(attributes, "\s*(?'Name'[\w\-]+?)\s*=\s*""(?'Value'.+?)""")
            Do While attributeMatch.Success
                Me(attributeMatch.Groups("Name").Value) = attributeMatch.Groups("Value").Value
                attributeMatch = attributeMatch.NextMatch()
            Loop
        End Sub
        
        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()
            For Each name As String in Keys
                sb.AppendFormat(" {0}=""{1}""", name, Me(name))
            Next
            Return sb.ToString()
        End Function
    End Class
End Namespace

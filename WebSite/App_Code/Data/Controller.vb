Imports MyCompany.Services
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Configuration
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Transactions
Imports System.Web
Imports System.Web.Caching
Imports System.Web.Configuration
Imports System.Web.Security
Imports System.Xml
Imports System.Xml.XPath
Imports System.Xml.Xsl

Namespace MyCompany.Data
    
    Partial Public Class DataControllerBase
        
        Private m_View As XPathNavigator
        
        Private m_ViewId As String
        
        Private m_ParameterMarker As String
        
        Private m_LeftQuote As String
        
        Private m_RightQuote As String
        
        Private m_ViewType As String
        
        Private m_Config As ControllerConfiguration
        
        Private m_ViewPage As ViewPage
        
        Private m_ViewOverridingDisabled As Boolean
        
        Public Shared SqlSelectRegex1 As Regex = New Regex("/\*<select>\*/(?'Select'[\S\s]*)?/\*</select>\*[\S\s]*?/\*<from>\*/(?'From'[\S\s]"& _ 
                "*)?/\*</from>\*[\S\s]*?/\*(<order-by>\*/(?'OrderBy'[\S\s]*)?/\*</order-by>\*/)?", RegexOptions.IgnoreCase)
        
        Public Shared SqlSelectRegex2 As Regex = New Regex("\s*select\s*(?'Select'[\S\s]*)?\sfrom\s*(?'From'[\S\s]*)?\swhere\s*(?'Where'[\S\s"& _ 
                "]*)?\sorder\s+by\s*(?'OrderBy'[\S\s]*)?|\s*select\s*(?'Select'[\S\s]*)?\sfrom\s*"& _ 
                "(?'From'[\S\s]*)?\swhere\s*(?'Where'[\S\s]*)?|\s*select\s*(?'Select'[\S\s]*)?\sf"& _ 
                "rom\s*(?'From'[\S\s]*)?\sorder\s+by\s*(?'OrderBy'[\S\s]*)?|\s*select\s*(?'Select"& _ 
                "'[\S\s]*)?\sfrom\s*(?'From'[\S\s]*)?", RegexOptions.IgnoreCase)
        
        ''' "table name" regular expression:
        ''' ^(?'Table'((\[|"|`)([\w\s]+)?(\]|"|`)|\w+)(\s*\.\s*((\[|"|`)([\w\s]+)?(\]|"|`)|\w+))*(\s*\.\s*((\[|"|`)([\w\s]+)?(\]|"|`)|\w+))*)(\s*(as|)\s*(\[|"|`|)([\w\s]+)?(\]|"|`|))
        Public Shared TableNameRegex As Regex = New Regex("^(?'Table'((\[|""|`)([\w\s]+)?(\]|""|`)|\w+)(\s*\.\s*((\[|""|`)([\w\s]+)?(\]|""|`)|\w"& _ 
                "+))*(\s*\.\s*((\[|""|`)([\w\s]+)?(\]|""|`)|\w+))*)(\s*(as|)\s*(\[|""|`|)([\w\s]+)?("& _ 
                "\]|""|`|))", RegexOptions.IgnoreCase)
        
        Private m_Expressions As SelectClauseDictionary
        
        Public Shared ParamDetectionRegex As Regex = New Regex("(?:(\W|^))(?'Parameter'(@|:)\w+)")
        
        Public Shared SelectExpressionRegex As Regex = New Regex("\s*(?'Expression'[\S\s]*?(\([\s\S]*?\)|(\.((""|'|\[|`)(?'FieldName'[\S\s]*?)(""|'|\"& _ 
                "]|`))|(""|'|\[|`|)(?'FieldName'[\w\s]*?)(""|'|\]|)|)))((\s+as\s+|\s+)(""|'|\[|`|)(?"& _ 
                "'Alias'[\S\s]*?)|)(""|'|\]|`|)\s*(,|$)", RegexOptions.IgnoreCase)
        
        Private Shared m_TypeMap As SortedDictionary(Of String, Type)
        
        Public Overridable ReadOnly Property Config() As ControllerConfiguration
            Get
                Return m_Config
            End Get
        End Property
        
        Private ReadOnly Property Resolver() As IXmlNamespaceResolver
            Get
                Return m_Config.Resolver
            End Get
        End Property
        
        Public Property ViewOverridingDisabled() As Boolean
            Get
                Return m_ViewOverridingDisabled
            End Get
            Set
                m_ViewOverridingDisabled = value
            End Set
        End Property
        
        Public Shared ReadOnly Property TypeMap() As SortedDictionary(Of String, Type)
            Get
                Return m_TypeMap
            End Get
        End Property
        
        Protected Overridable Function YieldsSingleRow(ByVal command As DbCommand) As Boolean
            Return ((command Is Nothing) OrElse (command.CommandText.IndexOf("count(*)") = -1))
        End Function
        
        Protected Function CreateValueFromSourceFields(ByVal field As DataField, ByVal reader As DbDataReader) As String
            Dim v As String = String.Empty
            If DBNull.Value.Equals(reader(field.Name)) Then
                v = "null"
            End If
            Dim m As Match = Regex.Match(field.SourceFields, "(\w+)\s*(,|$)")
            Do While m.Success
                If (v.Length > 0) Then
                    v = (v + "|")
                End If
                Dim rawValue As Object = reader(m.Groups(1).Value)
                If DBNull.Value.Equals(rawValue) Then
                    v = (v + "null")
                Else
                    If ((Not (rawValue) Is Nothing) AndAlso TypeOf rawValue Is Byte) Then
                        rawValue = New Guid(CType(rawValue,Byte()))
                    End If
                    v = (v + Convert.ToString(rawValue))
                End If
                m = m.NextMatch()
            Loop
            Return v
        End Function
        
        Private Sub PopulatePageCategories(ByVal page As ViewPage)
            Dim categoryIterator As XPathNodeIterator = m_View.Select("c:categories/c:category", Resolver)
            Do While categoryIterator.MoveNext()
                page.Categories.Add(New Category(categoryIterator.Current, Resolver))
            Loop
            If (page.Categories.Count = 0) Then
                page.Categories.Add(New Category())
            End If
        End Sub
        
        Public Function CreateViewPage() As ViewPage
            If (m_ViewPage Is Nothing) Then
                m_ViewPage = New ViewPage()
                PopulatePageFields(m_ViewPage)
                EnsurePageFields(m_ViewPage, Nothing)
            End If
            Return m_ViewPage
        End Function
        
        Sub PopulateDynamicLookups(ByVal args As ActionArgs, ByVal result As ActionResult)
            Dim page As ViewPage = CreateViewPage()
            For Each field As DataField in page.Fields
                If (Not (String.IsNullOrEmpty(field.ContextFields)) AndAlso page.PopulateStaticItems(field, args.Values)) Then
                    result.Values.Add(New FieldValue(field.Name, field.Items.ToArray()))
                End If
            Next
        End Sub
        
        Public Shared Function UserIsInRole(ByVal ParamArray roles() as System.[String]) As Boolean
            Return New ControllerUtilities().UserIsInRole(roles)
        End Function
        
        Private Sub ExecutePostActionCommands(ByVal args As ActionArgs, ByVal result As ActionResult, ByVal connection As DbConnection)
            If Not (TransactionManager.InTransaction(args)) Then
                Dim eventName As String = String.Empty
                If args.CommandName.Equals("insert", StringComparison.OrdinalIgnoreCase) Then
                    eventName = "Inserted"
                Else
                    If args.CommandName.Equals("update", StringComparison.OrdinalIgnoreCase) Then
                        eventName = "Updated"
                    Else
                        If args.CommandName.Equals("delete", StringComparison.OrdinalIgnoreCase) Then
                            eventName = "Deleted"
                        End If
                    End If
                End If
                Dim eventCommandIterator As XPathNodeIterator = m_Config.Select("/c:dataController/c:commands/c:command[@event='{0}']", eventName)
                Do While eventCommandIterator.MoveNext()
                    ExecuteActionCommand(args, result, connection, eventCommandIterator.Current)
                Loop
            End If
            If New ControllerUtilities().SupportsLastEnteredValues(args.Controller) Then
                If ((args.SaveLEVs AndAlso (Not (HttpContext.Current.Session) Is Nothing)) AndAlso ((args.CommandName = "Insert") OrElse (args.CommandName = "Update"))) Then
                    HttpContext.Current.Session(String.Format("{0}$LEVs", args.Controller)) = args.Values
                End If
            End If
        End Sub
        
        Private Sub ExecuteActionCommand(ByVal args As ActionArgs, ByVal result As ActionResult, ByVal connection As DbConnection, ByVal commandNavigator As XPathNavigator)
            Dim command As DbCommand = SqlStatement.CreateCommand(connection)
            Dim commandType As String = commandNavigator.GetAttribute("type", String.Empty)
            If String.IsNullOrEmpty(commandType) Then
                commandType = "Text"
            End If
            command.CommandType = CType(TypeDescriptor.GetConverter(GetType(CommandType)).ConvertFromString(commandType),CommandType)
            command.CommandText = CType(commandNavigator.Evaluate("string(c:text)", Resolver),String)
            Dim reader As DbDataReader = command.ExecuteReader()
            If reader.Read() Then
                Dim outputIndex As Integer = 0
                Dim outputIterator As XPathNodeIterator = commandNavigator.Select("c:output/c:*", Resolver)
                Do While outputIterator.MoveNext()
                    If (outputIterator.Current.LocalName = "fieldOutput") Then
                        Dim name As String = outputIterator.Current.GetAttribute("name", String.Empty)
                        Dim fieldName As String = outputIterator.Current.GetAttribute("fieldName", String.Empty)
                        For Each v As FieldValue in args.Values
                            If v.Name.Equals(fieldName, StringComparison.CurrentCultureIgnoreCase) Then
                                If String.IsNullOrEmpty(name) Then
                                    v.NewValue = reader(outputIndex)
                                Else
                                    v.NewValue = reader(name)
                                End If
                                If ((Not (v.NewValue) Is Nothing) AndAlso ((v.NewValue.GetType() Is GetType(Byte())) AndAlso (CType(v.NewValue,Byte()).Length = 16))) Then
                                    v.NewValue = New Guid(CType(v.NewValue,Byte()))
                                End If
                                v.Modified = true
                                If (Not (result) Is Nothing) Then
                                    result.Values.Add(v)
                                End If
                                Exit For
                            End If
                        Next
                    End If
                    outputIndex = (outputIndex + 1)
                Loop
            End If
            reader.Close()
        End Sub
        
        Private Sub ExecutePreActionCommands(ByVal args As ActionArgs, ByVal result As ActionResult, ByVal connection As DbConnection)
            If TransactionManager.InTransaction(args) Then
                Return
            End If
            Dim eventName As String = String.Empty
            If args.CommandName.Equals("insert", StringComparison.OrdinalIgnoreCase) Then
                eventName = "Inserting"
            Else
                If args.CommandName.Equals("update", StringComparison.OrdinalIgnoreCase) Then
                    eventName = "Updating"
                Else
                    If args.CommandName.Equals("delete", StringComparison.OrdinalIgnoreCase) Then
                        eventName = "Deleting"
                    End If
                End If
            End If
            Dim eventCommandIterator As XPathNodeIterator = m_Config.Select("/c:dataController/c:commands/c:command[@event='{0}']", eventName)
            Do While eventCommandIterator.MoveNext()
                ExecuteActionCommand(args, result, connection, eventCommandIterator.Current)
            Loop
        End Sub
        
        Protected Overridable Function CreateConfiguration(ByVal controllerName As String) As ControllerConfiguration
            Return Controller.CreateConfigurationInstance([GetType](), controllerName)
        End Function
        
        Public Shared Function CreateConfigurationInstance(ByVal t As Type, ByVal controller As String) As ControllerConfiguration
            Dim configKey As String = ("DataController_" + controller)
            Dim config As ControllerConfiguration = CType(HttpContext.Current.Items(configKey),ControllerConfiguration)
            If (Not (config) Is Nothing) Then
                Return config
            End If
            config = CType(HttpRuntime.Cache(configKey),ControllerConfiguration)
            If (config Is Nothing) Then
                Dim res As Stream = ControllerFactory.GetDataControllerStream(controller)
                Dim allowCaching As Boolean = (res Is Nothing)
                If ApplicationServices.IsSiteContentEnabled Then
                    allowCaching = false
                End If
                If (res Is DefaultDataControllerStream) Then
                    res = Nothing
                End If
                If (res Is Nothing) Then
                    res = t.Assembly.GetManifestResourceStream(String.Format("MyCompany.Controllers.{0}.xml", controller))
                End If
                If (res Is Nothing) Then
                    res = t.Assembly.GetManifestResourceStream(String.Format("MyCompany.{0}.xml", controller))
                End If
                If (res Is Nothing) Then
                    Dim controllerPath As String = Path.Combine(Path.Combine(HttpRuntime.AppDomainAppPath, "Controllers"), (controller + ".xml"))
                    If Not (File.Exists(controllerPath)) Then
                        Throw New Exception(String.Format("Controller '{0}' does not exist.", controller))
                    End If
                    config = New ControllerConfiguration(controllerPath)
                    If allowCaching Then
                        HttpRuntime.Cache.Insert(configKey, config, New CacheDependency(controllerPath))
                    End If
                Else
                    config = New ControllerConfiguration(res)
                    If allowCaching Then
                        HttpRuntime.Cache.Insert(configKey, config)
                    End If
                End If
            End If
            Dim requiresLocalization As Boolean = config.RequiresLocalization
            If config.UsesVariables Then
                config = config.Clone()
            End If
            config = config.EnsureVitalElements()
            If (Not (config.PlugIn) Is Nothing) Then
                config = config.PlugIn.Create(config)
            End If
            If requiresLocalization Then
                config = config.Localize(controller)
            End If
            If config.RequiresVirtualization(controller) Then
                config = config.Virtualize(controller)
            End If
            config.Complete()
            HttpContext.Current.Items(configKey) = config
            Return config
        End Function
        
        Public Overridable Sub SelectView(ByVal controller As String, ByVal view As String)
            m_Config = CreateConfiguration(controller)
            Dim iterator As XPathNodeIterator = Nothing
            If String.IsNullOrEmpty(view) Then
                iterator = m_Config.Select("/c:dataController/c:views/c:view[1]")
            Else
                iterator = m_Config.Select("/c:dataController/c:views/c:view[@id='{0}']", view)
            End If
            If Not (iterator.MoveNext()) Then
                iterator = m_Config.Select("/c:dataController/c:views/c:view[1]")
                If Not (iterator.MoveNext()) Then
                    Throw New Exception(String.Format("The view '{0}' does not exist.", view))
                End If
            End If
            m_View = iterator.Current
            m_ViewId = iterator.Current.GetAttribute("id", String.Empty)
            If Not (ViewOverridingDisabled) Then
                Dim overrideIterator As XPathNodeIterator = m_Config.Select("/c:dataController/c:views/c:view[@virtualViewId='{0}']", m_ViewId)
                Do While overrideIterator.MoveNext()
                    Dim viewId As String = overrideIterator.Current.GetAttribute("id", String.Empty)
                    Dim rules As BusinessRules = m_Config.CreateBusinessRules()
                    If ((Not (rules) Is Nothing) AndAlso rules.IsOverrideApplicable(controller, viewId, m_ViewId)) Then
                        m_View = overrideIterator.Current
                        Exit Do
                    End If
                Loop
            End If
            m_ViewType = iterator.Current.GetAttribute("type", String.Empty)
            Dim accessType As String = iterator.Current.GetAttribute("access", String.Empty)
            If String.IsNullOrEmpty(accessType) Then
                accessType = "Private"
            End If
            If Not (ValidateViewAccess(controller, m_ViewId, accessType)) Then
                Throw New Exception(String.Format("Not authorized to access private view '{0}' in data controller '{1}'. Set 'Access"& _ 
                            "' property of the view to 'Public' or enable 'Idle User Detection' to automatica"& _ 
                            "lly logout user after a period of inactivity.", m_ViewId, controller))
            End If
        End Sub
        
        Protected Overloads Overridable Function CreateConnection() As DbConnection
            Return CreateConnection(true)
        End Function
        
        Protected Overloads Overridable Function CreateConnection(ByVal open As Boolean) As DbConnection
            Dim settings As ConnectionStringSettings = ConnectionStringSettingsFactory.Create(m_Config.ConnectionStringName)
            If (settings Is Nothing) Then
                Throw New Exception(String.Format("Connection string '{0}' is not defined in web.config of this application.", m_Config.ConnectionStringName))
            End If
            If (settings.ProviderName = "CodeOnTime.CustomDataProvider") Then
                open = false
                settings = New ConnectionStringSettings("CustomDataProvider", "", "System.Data.SqlClient")
            End If
            Dim factory As DbProviderFactory = DbProviderFactories.GetFactory(settings.ProviderName)
            Dim connection As DbConnection = factory.CreateConnection()
            Dim connectionString As String = settings.ConnectionString
            If SupportsLimitInSelect(connection) Then
                connectionString = (connectionString + "Allow User Variables=True")
            End If
            connection.ConnectionString = connectionString
            If open Then
                connection.Open()
            End If
            m_ParameterMarker = SqlStatement.ConvertTypeToParameterMarker(settings.ProviderName)
            m_LeftQuote = SqlStatement.ConvertTypeToLeftQuote(settings.ProviderName)
            m_RightQuote = SqlStatement.ConvertTypeToRightQuote(settings.ProviderName)
            Return connection
        End Function
        
        Protected Overloads Overridable Function CreateCommand(ByVal connection As DbConnection) As DbCommand
            Return CreateCommand(connection, Nothing)
        End Function
        
        Protected Overloads Overridable Function CreateCommand(ByVal connection As DbConnection, ByVal args As ActionArgs) As DbCommand
            Dim commandId As String = m_View.GetAttribute("commandId", String.Empty)
            Dim commandNav As XPathNavigator = m_Config.SelectSingleNode("/c:dataController/c:commands/c:command[@id='{0}']", commandId)
            If ((Not (args) Is Nothing) AndAlso Not (String.IsNullOrEmpty(args.CommandArgument))) Then
                Dim commandNav2 As XPathNavigator = m_Config.SelectSingleNode("/c:dataController/c:commands/c:command[@id='{0}']", args.CommandArgument)
                If (Not (commandNav2) Is Nothing) Then
                    commandNav = commandNav2
                End If
            End If
            If (commandNav Is Nothing) Then
                Return Nothing
            End If
            Dim command As DbCommand = SqlStatement.CreateCommand(connection)
            If (Not (SinglePhaseTransactionScope.Current) Is Nothing) Then
                SinglePhaseTransactionScope.Current.Enlist(command)
            End If
            Dim theCommandType As String = commandNav.GetAttribute("type", String.Empty)
            If Not (String.IsNullOrEmpty(theCommandType)) Then
                command.CommandType = CType(TypeDescriptor.GetConverter(GetType(CommandType)).ConvertFromString(theCommandType),CommandType)
            End If
            command.CommandText = CType(commandNav.Evaluate("string(c:text)", Resolver),String)
            If String.IsNullOrEmpty(command.CommandText) Then
                command.CommandText = commandNav.InnerXml
            End If
            Dim handler As IActionHandler = m_Config.CreateActionHandler()
            Dim parameterIterator As XPathNodeIterator = commandNav.Select("c:parameters/c:parameter", Resolver)
            Dim missingFields As SortedDictionary(Of String, String) = Nothing
            Do While parameterIterator.MoveNext()
                Dim parameter As DbParameter = command.CreateParameter()
                parameter.ParameterName = parameterIterator.Current.GetAttribute("name", String.Empty)
                Dim s As String = parameterIterator.Current.GetAttribute("type", String.Empty)
                If Not (String.IsNullOrEmpty(s)) Then
                    parameter.DbType = CType(TypeDescriptor.GetConverter(GetType(DbType)).ConvertFromString(s),DbType)
                End If
                s = parameterIterator.Current.GetAttribute("direction", String.Empty)
                If Not (String.IsNullOrEmpty(s)) Then
                    parameter.Direction = CType(TypeDescriptor.GetConverter(GetType(ParameterDirection)).ConvertFromString(s),ParameterDirection)
                End If
                command.Parameters.Add(parameter)
                s = parameterIterator.Current.GetAttribute("defaultValue", String.Empty)
                If Not (String.IsNullOrEmpty(s)) Then
                    parameter.Value = s
                End If
                s = parameterIterator.Current.GetAttribute("fieldName", String.Empty)
                If ((Not (args) Is Nothing) AndAlso Not (String.IsNullOrEmpty(s))) Then
                    Dim v As FieldValue = args.SelectFieldValueObject(s)
                    If (Not (v) Is Nothing) Then
                        s = parameterIterator.Current.GetAttribute("fieldValue", String.Empty)
                        If (s = "Old") Then
                            parameter.Value = v.OldValue
                        Else
                            If (s = "New") Then
                                parameter.Value = v.NewValue
                            Else
                                parameter.Value = v.Value
                            End If
                        End If
                    Else
                        If (missingFields Is Nothing) Then
                            missingFields = New SortedDictionary(Of String, String)()
                        End If
                        missingFields.Add(parameter.ParameterName, s)
                    End If
                End If
                s = parameterIterator.Current.GetAttribute("propertyName", String.Empty)
                If (Not (String.IsNullOrEmpty(s)) AndAlso (Not (handler) Is Nothing)) Then
                    Dim result As Object = handler.GetType().InvokeMember(s, (System.Reflection.BindingFlags.GetProperty Or System.Reflection.BindingFlags.GetField), Nothing, handler, New Object(-1) {})
                    parameter.Value = result
                End If
                If (parameter.Value Is Nothing) Then
                    parameter.Value = DBNull.Value
                End If
            Loop
            If (Not (missingFields) Is Nothing) Then
                Dim retrieveMissingValues As Boolean = true
                Dim filter As List(Of String) = New List(Of String)()
                Dim page As ViewPage = CreateViewPage()
                For Each field As DataField in page.Fields
                    If field.IsPrimaryKey Then
                        Dim v As FieldValue = args.SelectFieldValueObject(field.Name)
                        If (v Is Nothing) Then
                            retrieveMissingValues = false
                            Exit For
                        Else
                            filter.Add(String.Format("{0}:={1}", v.Name, v.Value))
                        End If
                    End If
                Next
                If retrieveMissingValues Then
                    Dim editView As String = CType(m_Config.Evaluate("string(//c:view[@type='Form']/@id)"),String)
                    If Not (String.IsNullOrEmpty(editView)) Then
                        Dim request As PageRequest = New PageRequest(0, 1, Nothing, filter.ToArray())
                        request.RequiresMetaData = true
                        page = ControllerFactory.CreateDataController().GetPage(args.Controller, editView, request)
                        If (page.Rows.Count > 0) Then
                            For Each parameterName As String in missingFields.Keys
                                Dim index As Integer = 0
                                Dim fieldName As String = missingFields(parameterName)
                                For Each field As DataField in page.Fields
                                    If field.Name.Equals(fieldName) Then
                                        Dim v As Object = page.Rows(0)(index)
                                        If (Not (v) Is Nothing) Then
                                            command.Parameters(parameterName).Value = v
                                        End If
                                    End If
                                    index = (index + 1)
                                Next
                            Next
                        End If
                    End If
                End If
            End If
            Return command
        End Function
        
        Protected Overridable Function ConfigureCommand(ByVal command As DbCommand, ByVal page As ViewPage, ByVal commandConfiguration As CommandConfigurationType, ByVal values() As FieldValue) As Boolean
            If (page Is Nothing) Then
                page = New ViewPage()
            End If
            PopulatePageFields(page)
            If (command Is Nothing) Then
                Return true
            End If
            If (command.CommandType = CommandType.Text) Then
                Dim statementMatch As Match = SqlSelectRegex1.Match(command.CommandText)
                If Not (statementMatch.Success) Then
                    statementMatch = SqlSelectRegex2.Match(command.CommandText)
                End If
                Dim expressions As SelectClauseDictionary = m_Expressions
                If (expressions Is Nothing) Then
                    expressions = ParseSelectExpressions(statementMatch.Groups("Select").Value)
                    m_Expressions = expressions
                End If
                EnsurePageFields(page, expressions)
                Dim commandId As String = m_View.GetAttribute("commandId", String.Empty)
                Dim commandIsCustom As Boolean = ((Not (m_Config.SelectSingleNode("/c:dataController/c:commands/c:command[@id='{0}' and @custom='true']", commandId)) Is Nothing) OrElse page.RequiresResultSet(commandConfiguration))
                AddComputedExpressions(expressions, page, commandConfiguration, commandIsCustom)
                If statementMatch.Success Then
                    Dim fromClause As String = statementMatch.Groups("From").Value
                    Dim whereClause As String = statementMatch.Groups("Where").Value
                    Dim orderByClause As String = statementMatch.Groups("OrderBy").Value
                    If commandIsCustom Then
                        Dim customCommandText As String = command.CommandText
                        If Not (String.IsNullOrEmpty(orderByClause)) Then
                            customCommandText = Regex.Replace(customCommandText, ("order\s+by\s+" + Regex.Escape(orderByClause)), String.Empty, RegexOptions.IgnoreCase)
                        End If
                        fromClause = String.Format("({0}) resultset__", customCommandText)
                        whereClause = String.Empty
                        orderByClause = String.Empty
                    End If
                    Dim tableName As String = Nothing
                    If Not (commandConfiguration.ToString().StartsWith("Select")) Then
                        tableName = CType(m_Config.Evaluate("string(/c:dataController/c:commands/c:command[@id='{0}']/@tableName)", commandId),String)
                    End If
                    If String.IsNullOrEmpty(tableName) Then
                        tableName = TableNameRegex.Match(fromClause).Groups("Table").Value
                    End If
                    If (commandConfiguration = CommandConfigurationType.Update) Then
                        Return ConfigureCommandForUpdate(command, page, expressions, tableName, values)
                    Else
                        If (commandConfiguration = CommandConfigurationType.Insert) Then
                            Return ConfigureCommandForInsert(command, page, expressions, tableName, values)
                        Else
                            If (commandConfiguration = CommandConfigurationType.Delete) Then
                                Return ConfigureCommandForDelete(command, page, expressions, tableName, values)
                            Else
                                ConfigureCommandForSelect(command, page, expressions, fromClause, whereClause, orderByClause, commandConfiguration)
                                ProcessExpressionParameters(command, expressions)
                            End If
                        End If
                    End If
                Else
                    If ((commandConfiguration = CommandConfigurationType.Select) AndAlso YieldsSingleRow(command)) Then
                        Dim sb As StringBuilder = New StringBuilder()
                        sb.Append("select ")
                        AppendSelectExpressions(sb, page, expressions, true)
                        command.CommandText = sb.ToString()
                    End If
                End If
                Return Not ((commandConfiguration = CommandConfigurationType.None))
            End If
            Return (command.CommandType = CommandType.StoredProcedure)
        End Function
        
        Private Sub ProcessExpressionParameters(ByVal command As DbCommand, ByVal expressions As SelectClauseDictionary)
            For Each fieldName As String in expressions.Keys
                Me.m_CurrentCommand = command
                Dim formula As String = expressions(fieldName)
                Dim m As Match = ParamDetectionRegex.Match(formula)
                If m.Success Then
                    AssignFilterParameterValue(m.Groups(3).Value)
                End If
            Next
        End Sub
        
        Private Sub AddComputedExpressions(ByVal expressions As SelectClauseDictionary, ByVal page As ViewPage, ByVal commandConfiguration As CommandConfigurationType, ByVal generateFormula As Boolean)
            Dim useFormulaAsIs As Boolean = ((commandConfiguration = CommandConfigurationType.Insert) OrElse (commandConfiguration = CommandConfigurationType.Update))
            For Each field As DataField in page.Fields
                If Not (String.IsNullOrEmpty(field.Formula)) Then
                    If useFormulaAsIs Then
                        expressions(field.ExpressionName()) = field.Formula
                    Else
                        expressions(field.ExpressionName()) = String.Format("({0})", field.Formula)
                    End If
                Else
                    If generateFormula Then
                        If useFormulaAsIs Then
                            expressions(field.ExpressionName()) = field.Name
                        Else
                            expressions(field.ExpressionName()) = String.Format("({0})", field.Name)
                        End If
                    End If
                End If
            Next
        End Sub
        
        Private Function ConfigureCommandForDelete(ByVal command As DbCommand, ByVal page As ViewPage, ByVal expressions As SelectClauseDictionary, ByVal tableName As String, ByVal values() As FieldValue) As Boolean
            Dim sb As StringBuilder = New StringBuilder()
            sb.AppendFormat("delete from {0}", tableName)
            AppendWhereExpressions(sb, command, page, expressions, values)
            command.CommandText = sb.ToString()
            Return true
        End Function
        
        Private Function ConfigureCommandForInsert(ByVal command As DbCommand, ByVal page As ViewPage, ByVal expressions As SelectClauseDictionary, ByVal tableName As String, ByVal values() As FieldValue) As Boolean
            Dim sb As StringBuilder = New StringBuilder()
            sb.AppendFormat("insert into {0} (", tableName)
            Dim firstField As Boolean = true
            For Each v As FieldValue in values
                Dim field As DataField = page.FindField(v.Name)
                If (IsFieldInsertable(field) AndAlso v.Modified) Then
                    sb.AppendLine()
                    If firstField Then
                        firstField = false
                    Else
                        sb.Append(",")
                    End If
                    sb.AppendFormat(RemoveTableAliasFromExpression(expressions(v.Name)))
                End If
            Next
            If firstField Then
                Return false
            End If
            sb.AppendLine(")")
            sb.AppendLine("values(")
            firstField = true
            For Each v As FieldValue in values
                Dim field As DataField = page.FindField(v.Name)
                If (IsFieldInsertable(field) AndAlso v.Modified) Then
                    sb.AppendLine()
                    If firstField Then
                        firstField = false
                    Else
                        sb.Append(",")
                    End If
                    If ((v.NewValue Is Nothing) AndAlso field.HasDefaultValue) Then
                        sb.Append(field.DefaultValue)
                    Else
                        sb.AppendFormat("{0}p{1}", m_ParameterMarker, command.Parameters.Count)
                        Dim parameter As DbParameter = command.CreateParameter()
                        parameter.ParameterName = String.Format("{0}p{1}", m_ParameterMarker, command.Parameters.Count)
                        AssignParameterValue(parameter, field.Type, v.NewValue)
                        command.Parameters.Add(parameter)
                    End If
                End If
            Next
            sb.AppendLine(")")
            command.CommandText = sb.ToString()
            Return true
        End Function
        
        Private Function RemoveTableAliasFromExpression(ByVal expression As String) As String
            'alias extraction regular expression:
            '"[\w\s]+".("[\w\s]+")
            Dim m As Match = Regex.Match(expression, """[\w\s]+"".(""[\w\s]+"")")
            If m.Success Then
                Return m.Groups(1).Value
            End If
            Return expression
        End Function
        
        Private Function ConfigureCommandForUpdate(ByVal command As DbCommand, ByVal page As ViewPage, ByVal expressions As SelectClauseDictionary, ByVal tableName As String, ByVal values() As FieldValue) As Boolean
            Dim sb As StringBuilder = New StringBuilder()
            sb.AppendFormat("update {0} set ", tableName)
            Dim firstField As Boolean = true
            For Each v As FieldValue in values
                Dim field As DataField = page.FindField(v.Name)
                If (IsFieldUpdatable(field) AndAlso v.Modified) Then
                    sb.AppendLine()
                    If firstField Then
                        firstField = false
                    Else
                        sb.Append(",")
                    End If
                    sb.AppendFormat(RemoveTableAliasFromExpression(expressions(v.Name)))
                    If ((v.NewValue Is Nothing) AndAlso field.HasDefaultValue) Then
                        sb.Append(String.Format("={0}", field.DefaultValue))
                    Else
                        sb.AppendFormat("={0}p{1}", m_ParameterMarker, command.Parameters.Count)
                        Dim parameter As DbParameter = command.CreateParameter()
                        parameter.ParameterName = String.Format("{0}p{1}", m_ParameterMarker, command.Parameters.Count)
                        AssignParameterValue(parameter, field.Type, v.NewValue)
                        command.Parameters.Add(parameter)
                    End If
                End If
            Next
            If firstField Then
                Return false
            End If
            AppendWhereExpressions(sb, command, page, expressions, values)
            command.CommandText = sb.ToString()
            Return true
        End Function
        
        Private Sub ConfigureCommandForSelect(ByVal command As DbCommand, ByVal page As ViewPage, ByVal expressions As SelectClauseDictionary, ByVal fromClause As String, ByVal whereClause As String, ByVal orderByClause As String, ByVal commandConfiguration As CommandConfigurationType)
            Dim useServerPaging As Boolean = ((Not ((commandConfiguration = CommandConfigurationType.SelectDistinct)) AndAlso Not (m_ServerRules.EnableResultSet)) AndAlso (Not ((commandConfiguration = CommandConfigurationType.SelectAggregates)) AndAlso Not ((commandConfiguration = CommandConfigurationType.SelectFirstLetters))))
            Dim useLimit As Boolean = SupportsLimitInSelect(command)
            Dim useSkip As Boolean = SupportsSkipInSelect(command)
            If useServerPaging Then
                page.AcceptAllRows()
            End If
            Dim sb As StringBuilder = New StringBuilder()
            If (useLimit OrElse useSkip) Then
                useServerPaging = false
            End If
            Dim countUsingHierarchy As Boolean = false
            If ((commandConfiguration = CommandConfigurationType.SelectCount) AndAlso (useServerPaging AndAlso RequiresHierarchy(page))) Then
                countUsingHierarchy = true
                commandConfiguration = CommandConfigurationType.Select
            End If
            If (commandConfiguration = CommandConfigurationType.SelectExisting) Then
                useServerPaging = false
            End If
            If (commandConfiguration = CommandConfigurationType.SelectCount) Then
                sb.AppendLine("select count(*)")
            Else
                If useServerPaging Then
                    sb.AppendLine("with page_cte__ as (")
                Else
                    If ((commandConfiguration = CommandConfigurationType.Sync) AndAlso useLimit) Then
                        sb.Append("select * from (select @row_num := @row_num+1 row_number__,cte__.* from (select @r"& _ 
                                "ow_num:=0) r,(")
                    End If
                End If
                sb.AppendLine("select")
                If useServerPaging Then
                    AppendRowNumberExpression(sb, page, expressions, orderByClause)
                End If
                If (commandConfiguration = CommandConfigurationType.SelectDistinct) Then
                    Dim distinctField As DataField = page.FindField(page.DistinctValueFieldName)
                    Dim distinctExpression As String = expressions(distinctField.ExpressionName())
                    If distinctField.Type.StartsWith("Date") Then
                        Dim commandType As String = command.GetType().ToString()
                        If (commandType = "System.Data.SqlClient.SqlCommand") Then
                            distinctExpression = String.Format("DATEADD(dd, 0, DATEDIFF(dd, 0, {0}))", distinctExpression)
                        End If
                        If (commandType = "MySql.Data.MySqlClient.MySqlCommand") Then
                            distinctExpression = String.Format("cast({0} as date)", distinctExpression)
                        End If
                    End If
                    sb.AppendFormat("distinct {0} ""{1}""" & ControlChars.CrLf , distinctExpression, page.DistinctValueFieldName)
                Else
                    If (commandConfiguration = CommandConfigurationType.SelectAggregates) Then
                        AppendAggregateExpressions(sb, page, expressions)
                    Else
                        If (commandConfiguration = CommandConfigurationType.SelectFirstLetters) Then
                            Dim substringFunction As String = "substring"
                            If DatabaseEngineIs(command, "Oracle", "DB2") Then
                                substringFunction = "substr"
                            End If
                            AppendFirstLetterExpressions(sb, page, expressions, substringFunction)
                        Else
                            If ((commandConfiguration = CommandConfigurationType.Select) AndAlso useSkip) Then
                                sb.AppendFormat(" first {0} skip {1}" & ControlChars.CrLf , page.PageSize, (page.PageSize * page.PageIndex))
                            End If
                            If ((commandConfiguration = CommandConfigurationType.Sync) AndAlso useSkip) Then
                                'Only select the primary key.
                                For Each field As DataField in page.Fields
                                    If field.IsPrimaryKey Then
                                        sb.Append(expressions(field.ExpressionName()))
                                        Exit For
                                    End If
                                Next
                            Else
                                If (commandConfiguration = CommandConfigurationType.SelectExisting) Then
                                    sb.AppendLine("*")
                                Else
                                    AppendSelectExpressions(sb, page, expressions, Not (useServerPaging))
                                End If
                            End If
                        End If
                    End If
                End If
            End If
            sb.AppendLine("from")
            sb.AppendLine(fromClause)
            m_HasWhere = false
            If String.IsNullOrEmpty(m_ViewFilter) Then
                m_ViewFilter = m_View.GetAttribute("filter", String.Empty)
                If (String.IsNullOrEmpty(m_ViewFilter) AndAlso ((m_ViewType = "Form") AndAlso Not (String.IsNullOrEmpty(page.LastView)))) Then
                    Dim lastView As XPathNavigator = m_Config.SelectSingleNode("/c:dataController/c:views/c:view[@id='{0}']", page.LastView)
                    If (Not (lastView) Is Nothing) Then
                        m_ViewFilter = lastView.GetAttribute("filter", String.Empty)
                    End If
                End If
            End If
            If Not (String.IsNullOrEmpty(m_ViewFilter)) Then
                m_ViewFilter = String.Format("({0})", m_ViewFilter)
            End If
            If (commandConfiguration = CommandConfigurationType.SelectExisting) Then
                EnsureWhereKeyword(sb)
                sb.Append(expressions(page.InnerJoinForeignKey.ToLower()))
                sb.Append("=")
                sb.Append(page.InnerJoinPrimaryKey)
                sb.AppendLine(" and ")
            End If
            AppendSystemFilter(command, page, expressions)
            AppendAccessControlRules(command, page, expressions)
            If (((Not (page.Filter) Is Nothing) AndAlso (page.Filter.Length > 0)) OrElse Not (String.IsNullOrEmpty(m_ViewFilter))) Then
                AppendFilterExpressionsToWhere(sb, page, command, expressions, whereClause)
            Else
                If Not (String.IsNullOrEmpty(whereClause)) Then
                    EnsureWhereKeyword(sb)
                    sb.AppendLine(whereClause)
                End If
            End If
            If (commandConfiguration = CommandConfigurationType.Select) Then
                Dim preFetch As Boolean = RequiresPreFetching(page)
                If useServerPaging Then
                    If Not (ConfigureCTE(sb, page, command, expressions, countUsingHierarchy)) Then
                        sb.Append(")" & ControlChars.CrLf &"select * from page_cte__ ")
                    End If
                    If Not (countUsingHierarchy) Then
                        sb.AppendFormat("where row_number__ > {0}PageRangeFirstRowNumber and row_number__ <= {0}PageRangeL"& _ 
                                "astRowNumber order by row_number__", m_ParameterMarker)
                        Dim p As DbParameter = command.CreateParameter()
                        p.ParameterName = (m_ParameterMarker + "PageRangeFirstRowNumber")
                        p.Value = ((page.PageSize * page.PageIndex)  _
                                    + page.PageOffset)
                        If preFetch Then
                            p.Value = (CType(p.Value,Integer) - page.PageSize)
                        End If
                        command.Parameters.Add(p)
                        Dim p2 As DbParameter = command.CreateParameter()
                        p2.ParameterName = (m_ParameterMarker + "PageRangeLastRowNumber")
                        p2.Value = ((page.PageSize  _
                                    * (page.PageIndex + 1))  _
                                    + page.PageOffset)
                        If preFetch Then
                            p2.Value = (CType(p2.Value,Integer) + page.PageSize)
                        End If
                        command.Parameters.Add(p2)
                    End If
                Else
                    AppendOrderByExpression(sb, page, expressions, orderByClause)
                    If useLimit Then
                        sb.AppendFormat("" & ControlChars.CrLf &"limit {0}Limit_PageOffset, {0}Limit_PageSize", m_ParameterMarker)
                        Dim p As DbParameter = command.CreateParameter()
                        p.ParameterName = (m_ParameterMarker + "Limit_PageOffset")
                        p.Value = ((page.PageSize * page.PageIndex)  _
                                    + page.PageOffset)
                        If (preFetch AndAlso (CType(p.Value,Integer) > page.PageSize)) Then
                            p.Value = (CType(p.Value,Integer) - page.PageSize)
                        End If
                        command.Parameters.Add(p)
                        Dim p2 As DbParameter = command.CreateParameter()
                        p2.ParameterName = (m_ParameterMarker + "Limit_PageSize")
                        p2.Value = page.PageSize
                        If preFetch Then
                            Dim pagesToFetch As Integer = 2
                            If (CType(p.Value,Integer) > page.PageSize) Then
                                pagesToFetch = 3
                            End If
                            p2.Value = (page.PageSize * pagesToFetch)
                        End If
                        command.Parameters.Add(p2)
                    End If
                End If
            Else
                If (commandConfiguration = CommandConfigurationType.Sync) Then
                    If useServerPaging Then
                        If Not (ConfigureCTE(sb, page, command, expressions, false)) Then
                            sb.Append(")" & ControlChars.CrLf &"select * from page_cte__ ")
                        End If
                        sb.Append("where ")
                    Else
                        If (useLimit OrElse useSkip) Then
                            AppendOrderByExpression(sb, page, expressions, orderByClause)
                        End If
                        If Not (useSkip) Then
                            sb.Append(") cte__)cte2__ where ")
                        End If
                    End If
                    Dim first As Boolean = true
                    If Not (useSkip) Then
                        For Each field As DataField in page.Fields
                            If field.IsPrimaryKey Then
                                If first Then
                                    first = false
                                Else
                                    sb.AppendFormat(" and ")
                                End If
                                sb.AppendFormat("{2}{1}{3}={0}PrimaryKey_{1}", m_ParameterMarker, field.Name, m_LeftQuote, m_RightQuote)
                            End If
                        Next
                    End If
                Else
                    If ((commandConfiguration = CommandConfigurationType.SelectDistinct) OrElse (commandConfiguration = CommandConfigurationType.SelectFirstLetters)) Then
                        sb.Append("order by 1")
                    End If
                End If
            End If
            command.CommandText = sb.ToString()
            If (commandConfiguration = CommandConfigurationType.Select) Then
                ApplyFieldFilter(page)
            End If
            m_ViewFilter = Nothing
        End Sub
        
        Protected Overridable Function ConfigureCTE(ByVal sb As StringBuilder, ByVal page As ViewPage, ByVal command As DbCommand, ByVal expressions As SelectClauseDictionary, ByVal performCount As Boolean) As Boolean
            If Not (RequiresHierarchy(page)) Then
                Return false
            End If
            'detect hierarchy
            Dim primaryKeyField As DataField = Nothing
            Dim parentField As DataField = Nothing
            Dim sortField As DataField = Nothing
            Dim sortOrder As String = "asc"
            Dim hierarchyOrganization As String = HierarchyOrganizationFieldName
            For Each field As DataField in page.Fields
                If field.IsPrimaryKey Then
                    primaryKeyField = field
                End If
                If field.IsTagged("hierarchy-parent") Then
                    parentField = field
                Else
                    If field.IsTagged("hierarchy-organization") Then
                        hierarchyOrganization = field.Name
                    End If
                End If
            Next
            If (parentField Is Nothing) Then
                Return false
            End If
            'select a hierarchy sort field
            If (sortField Is Nothing) Then
                If Not (String.IsNullOrEmpty(page.SortExpression)) Then
                    Dim sortExpression As Match = Regex.Match(page.SortExpression, "(?'FieldName'\w+)(\s+(?'SortOrder'asc|desc)?)", RegexOptions.IgnoreCase)
                    If sortExpression.Success Then
                        For Each field As DataField in page.Fields
                            If (field.Name = sortExpression.Groups("FieldName").Value) Then
                                sortField = field
                                sortOrder = sortExpression.Groups("SortOrder").Value
                                Exit For
                            End If
                        Next
                    End If
                End If
                If (sortField Is Nothing) Then
                    For Each field As DataField in page.Fields
                        If Not (field.Hidden) Then
                            sortField = field
                            Exit For
                        End If
                    Next
                End If
            End If
            If (sortField Is Nothing) Then
                sortField = page.Fields(0)
            End If
            'append a hierarchical CTE
            Dim isOracle As Boolean = DatabaseEngineIs(command, "Oracle")
            sb.AppendLine("),")
            sb.AppendLine("h__(")
            Dim first As Boolean = true
            For Each field As DataField in page.Fields
                If first Then
                    first = false
                Else
                    sb.Append(",")
                End If
                sb.AppendFormat("{0}{1}{2}", m_LeftQuote, field.Name, m_RightQuote)
                sb.AppendLine()
            Next
            sb.AppendFormat(",{0}{1}{2}", m_LeftQuote, hierarchyOrganization, m_RightQuote)
            sb.AppendLine(")as(")
            'top-level of self-referring CTE
            sb.AppendLine("select")
            first = true
            For Each field As DataField in page.Fields
                If first Then
                    first = false
                Else
                    sb.Append(",")
                End If
                sb.AppendFormat("h1__.{0}{1}{2}", m_LeftQuote, field.Name, m_RightQuote)
                sb.AppendLine()
            Next
            'add top-level hierarchy organization field
            If isOracle Then
                sb.AppendFormat(",lpad(cast(row_number() over (partition by h1__.{0}{1}{2} order by h1__.{0}{3}{2}"& _ 
                        " {4}) as varchar(5)), 5, '0') as {0}{5}{2}", m_LeftQuote, parentField.Name, m_RightQuote, sortField.Name, sortOrder, hierarchyOrganization)
            Else
                sb.AppendFormat(",cast(right('0000' + cast(row_number() over (partition by h1__.{0}{1}{2} order by"& _ 
                        " h1__.{0}{3}{2} {4}) as varchar), 4) as varchar) as {0}{5}{2}", m_LeftQuote, parentField.Name, m_RightQuote, sortField.Name, sortOrder, hierarchyOrganization)
            End If
            'add top-level "from" clause
            sb.AppendLine()
            sb.AppendFormat("from page_cte__ h1__ where h1__.{0}{1}{2} is null ", m_LeftQuote, parentField.Name, m_RightQuote)
            sb.AppendLine()
            sb.AppendLine("union all")
            'sublevel of self-referring CTE
            sb.AppendLine("select")
            first = true
            For Each field As DataField in page.Fields
                If first Then
                    first = false
                Else
                    sb.Append(",")
                End If
                sb.AppendFormat("h2__.{0}{1}{2}", m_LeftQuote, field.Name, m_RightQuote)
                sb.AppendLine()
            Next
            'add sublevel hierarchy organization field
            If isOracle Then
                sb.AppendFormat(",h__.{0}{5}{2} || '/' || lpad(cast(row_number() over (partition by h2__.{0}{1}{2}"& _ 
                        " order by h2__.{0}{3}{2} {4}) as varchar(5)), 5, '0') as {0}{5}{2}", m_LeftQuote, parentField.Name, m_RightQuote, sortField.Name, sortOrder, hierarchyOrganization)
            Else
                sb.AppendFormat(",convert(varchar, h__.{0}{5}{2} + '/' + cast(right('0000' + cast(row_number() ove"& _ 
                        "r (partition by h2__.{0}{1}{2} order by h2__.{0}{3}{2} {4}) as varchar), 4) as v"& _ 
                        "archar)) as {0}{5}{2}", m_LeftQuote, parentField.Name, m_RightQuote, sortField.Name, sortOrder, hierarchyOrganization)
            End If
            sb.AppendLine()
            'add sublevel "from" clause
            sb.AppendFormat("from page_cte__ h2__ inner join h__ on h2__.{0}{1}{2} = h__.{0}{3}{2}", m_LeftQuote, parentField.Name, m_RightQuote, primaryKeyField.Name)
            sb.AppendLine()
            sb.AppendLine("),")
            sb.AppendFormat("ho__ as (select row_number() over (order by ({0}{1}{2})) as row_number__, h__.* f"& _ 
                    "rom h__)", m_LeftQuote, hierarchyOrganization, m_RightQuote)
            If performCount Then
                sb.AppendLine("select count(*) from ho__")
            Else
                sb.AppendLine("select * from ho__")
            End If
            sb.AppendLine()
            Return true
        End Function
        
        Private Sub AppendFirstLetterExpressions(ByVal sb As StringBuilder, ByVal page As ViewPage, ByVal expressions As SelectClauseDictionary, ByVal substringFunction As String)
            For Each field As DataField in page.Fields
                If ((Not (field.Hidden) AndAlso field.AllowQBE) AndAlso (field.Type = "String")) Then
                    Dim fieldName As String = field.AliasName
                    If String.IsNullOrEmpty(fieldName) Then
                        fieldName = field.Name
                    End If
                    sb.AppendFormat("distinct {1}({0},1,1) first_letter__" & ControlChars.CrLf , expressions(fieldName), substringFunction)
                    page.FirstLetters = fieldName
                    page.RemoveFromFilter(fieldName)
                    Exit For
                End If
            Next
        End Sub
        
        Public Shared Sub AssignParameterDbType(ByVal parameter As DbParameter, ByVal systemType As String)
            If (systemType = "SByte") Then
                parameter.DbType = DbType.Int16
            Else
                If (systemType = "TimeSpan") Then
                    parameter.DbType = DbType.String
                Else
                    If ((systemType = "Guid") AndAlso parameter.GetType().Name.Contains("Oracle")) Then
                        parameter.DbType = DbType.Binary
                    Else
                        parameter.DbType = CType(TypeDescriptor.GetConverter(GetType(DbType)).ConvertFrom(systemType),DbType)
                    End If
                End If
            End If
        End Sub
        
        Public Shared Sub AssignParameterValue(ByVal parameter As DbParameter, ByVal systemType As String, ByVal v As Object)
            AssignParameterDbType(parameter, systemType)
            If (v Is Nothing) Then
                parameter.Value = DBNull.Value
            Else
                If (parameter.DbType = DbType.String) Then
                    parameter.Value = v.ToString()
                Else
                    parameter.Value = ConvertToType(Controller.TypeMap(systemType), v)
                End If
                If ((parameter.DbType = DbType.Binary) AndAlso TypeOf parameter.Value Is Guid) Then
                    parameter.Value = CType(parameter.Value,Guid).ToByteArray()
                End If
            End If
        End Sub
        
        Private Sub AppendSelectExpressions(ByVal sb As StringBuilder, ByVal page As ViewPage, ByVal expressions As SelectClauseDictionary, ByVal firstField As Boolean)
            For Each field As DataField in page.Fields
                If (field.IsPrimaryKey OrElse page.IncludeField(field.Name)) Then
                    If firstField Then
                        firstField = false
                    Else
                        sb.Append(",")
                    End If
                    Try 
                        If field.OnDemand Then
                            Dim onDemandExpression As String = field.ExpressionName()
                            Dim sourceField As DataField = page.FindField(field.SourceFields)
                            If ((Not (sourceField) Is Nothing) AndAlso Not (sourceField.IsPrimaryKey)) Then
                                onDemandExpression = sourceField.ExpressionName()
                            End If
                            sb.Append(String.Format("case when {0} is not null then 1 else null end as ", expressions(onDemandExpression)))
                        Else
                            sb.Append(expressions(field.ExpressionName()))
                        End If
                    Catch __exception As Exception
                        Throw New Exception(String.Format("Unknown data field '{0}'.", field.Name))
                    End Try
                    sb.Append(" """)
                    sb.Append(field.Name)
                    sb.AppendLine("""")
                End If
            Next
        End Sub
        
        Sub AppendAggregateExpressions(ByVal sb As StringBuilder, ByVal page As ViewPage, ByVal expressions As SelectClauseDictionary)
            Dim firstField As Boolean = true
            For Each field As DataField in page.Fields
                If firstField Then
                    firstField = false
                Else
                    sb.Append(",")
                End If
                If (field.Aggregate = DataFieldAggregate.None) Then
                    sb.Append("null ")
                Else
                    Dim functionName As String = field.Aggregate.ToString()
                    If (functionName = "Average") Then
                        functionName = "Avg"
                    End If
                    Dim fmt As String = "{0}({1})"
                    If (functionName = "Count") Then
                        fmt = "{0}(distinct {1})"
                    End If
                    sb.AppendFormat(fmt, functionName, expressions(field.ExpressionName()))
                End If
                sb.Append(" """)
                sb.Append(field.Name)
                sb.AppendLine("""")
            Next
        End Sub
        
        Private Sub AppendRowNumberExpression(ByVal sb As StringBuilder, ByVal page As ViewPage, ByVal expressions As SelectClauseDictionary, ByVal orderByClause As String)
            sb.Append("row_number() over (")
            AppendOrderByExpression(sb, page, expressions, orderByClause)
            sb.AppendLine(") as row_number__")
        End Sub
        
        Public Overridable Function IsEmptyString(ByVal s As String) As Boolean
            Return String.IsNullOrEmpty(s)
        End Function
        
        Public Overridable Function IsFieldUpdatable(ByVal field As DataField) As Boolean
            Return (((Not (field) Is Nothing) AndAlso String.IsNullOrEmpty(field.ItemsTargetController)) AndAlso (Not (field.IsVirtual) AndAlso Not (field.ReadOnly)))
        End Function
        
        Public Overridable Function IsFieldInsertable(ByVal field As DataField) As Boolean
            Return ((Not (field) Is Nothing) AndAlso (field.IsPrimaryKey OrElse IsFieldUpdatable(field)))
        End Function
        
        Private Sub AppendOrderByExpression(ByVal sb As StringBuilder, ByVal page As ViewPage, ByVal expressions As SelectClauseDictionary, ByVal orderByClause As String)
            If String.IsNullOrEmpty(page.SortExpression) Then
                page.SortExpression = m_View.GetAttribute("sortExpression", String.Empty)
            End If
            If String.IsNullOrEmpty(page.GroupExpression) Then
                page.GroupExpression = m_View.GetAttribute("groupExpression", String.Empty)
                If Not (String.IsNullOrEmpty(page.GroupExpression)) Then
                    If (page.SortExpression Is Nothing) Then
                        page.SortExpression = String.Empty
                    End If
                End If
                Dim groupBy As List(Of String) = New List(Of String)(BusinessRules.ListRegex.Split(page.GroupExpression.Trim()))
                Dim sortBy As List(Of String) = New List(Of String)(BusinessRules.ListRegex.Split(page.SortExpression.Trim()))
                groupBy.RemoveAll(AddressOf IsEmptyString)
                page.GroupExpression = String.Join(",", groupBy.ToArray())
                sortBy.RemoveAll(AddressOf IsEmptyString)
                Dim i As Integer = 0
                Do While (i < groupBy.Count)
                    Dim groupField As String = groupBy(i)
                    If (i < sortBy.Count) Then
                        Dim sortField() As String = Regex.Split(sortBy(i), "\s+")
                        If Not ((groupField = sortField(0))) Then
                            sortBy.Insert(i, groupField)
                        End If
                    Else
                        sortBy.Insert(i, groupField)
                    End If
                    i = (i + 1)
                Loop
                page.SortExpression = String.Join(",", sortBy.ToArray())
            End If
            Dim hasOrderBy As Boolean = false
            sb.Append("order by ")
            If String.IsNullOrEmpty(page.SortExpression) Then
                If Not (String.IsNullOrEmpty(orderByClause)) Then
                    sb.Append(orderByClause)
                    hasOrderBy = true
                End If
            Else
                Dim firstSortField As Boolean = true
                Dim orderByMatch As Match = Regex.Match(page.SortExpression, "\s*(?'Alias'[\s\w]+?)\s*(?'Order'\s(ASC|DESC))?\s*(,|$)", RegexOptions.IgnoreCase)
                Do While orderByMatch.Success
                    If firstSortField Then
                        firstSortField = false
                    Else
                        sb.Append(",")
                    End If
                    Dim fieldName As String = orderByMatch.Groups("Alias").Value
                    If fieldName.EndsWith("_Mirror") Then
                        fieldName = fieldName.Substring(0, (fieldName.Length - 7))
                    End If
                    sb.Append(expressions(fieldName))
                    sb.Append(" ")
                    sb.Append(orderByMatch.Groups("Order").Value)
                    orderByMatch = orderByMatch.NextMatch()
                    hasOrderBy = true
                Loop
            End If
            Dim firstKey As Boolean = Not (hasOrderBy)
            For Each field As DataField in page.Fields
                If field.IsPrimaryKey Then
                    If firstKey Then
                        firstKey = false
                    Else
                        sb.Append(",")
                    End If
                    sb.Append(expressions(field.ExpressionName()))
                End If
            Next
            If firstKey Then
                sb.Append(expressions(page.Fields(0).ExpressionName()))
            End If
        End Sub
        
        Private Sub EnsurePageFields(ByVal page As ViewPage, ByVal expressions As SelectClauseDictionary)
            Dim statusBar As XPathNavigator = m_Config.SelectSingleNode("/c:dataController/c:statusBar")
            If (Not (statusBar) Is Nothing) Then
                page.StatusBar = statusBar.Value
            End If
            If (page.Fields.Count = 0) Then
                Dim fieldIterator As XPathNodeIterator = m_Config.Select("/c:dataController/c:fields/c:field")
                Do While fieldIterator.MoveNext()
                    Dim fieldName As String = fieldIterator.Current.GetAttribute("name", String.Empty)
                    If expressions.ContainsKey(fieldName) Then
                        page.Fields.Add(New DataField(fieldIterator.Current, Resolver))
                    End If
                Loop
            End If
            Dim keyFieldIterator As XPathNodeIterator = m_Config.Select("/c:dataController/c:fields/c:field[@isPrimaryKey='true' or @hidden='true']")
            Do While keyFieldIterator.MoveNext()
                Dim fieldName As String = keyFieldIterator.Current.GetAttribute("name", String.Empty)
                If Not (page.ContainsField(fieldName)) Then
                    page.Fields.Add(New DataField(keyFieldIterator.Current, Resolver, true))
                End If
            Loop
            Dim aliasIterator As XPathNodeIterator = m_View.Select(".//c:dataFields/c:dataField/@aliasFieldName", Resolver)
            Do While aliasIterator.MoveNext()
                Dim aliasField As DataField = page.FindField(aliasIterator.Current.Value)
                If (aliasField Is Nothing) Then
                    Dim fieldIterator As XPathNodeIterator = m_Config.Select("/c:dataController/c:fields/c:field[@name='{0}']", aliasIterator.Current.Value)
                    If fieldIterator.MoveNext() Then
                        page.Fields.Add(New DataField(fieldIterator.Current, Resolver, true))
                    End If
                Else
                    aliasField.Hidden = true
                End If
            Loop
            Dim lookupFieldIterator As XPathNodeIterator = m_Config.Select("/c:dataController/c:fields/c:field[c:items/@dataController]")
            Do While lookupFieldIterator.MoveNext()
                Dim fieldName As String = lookupFieldIterator.Current.GetAttribute("name", String.Empty)
                If Not (page.ContainsField(fieldName)) Then
                    page.Fields.Add(New DataField(lookupFieldIterator.Current, Resolver, true))
                End If
            Loop
            Dim groupExpression As String = m_View.GetAttribute("groupExpression", String.Empty)
            If Not (String.IsNullOrEmpty(groupExpression)) Then
                For Each groupField As String in BusinessRules.ListRegex.Split(groupExpression)
                    If (Not (String.IsNullOrEmpty(groupField)) AndAlso Not (page.ContainsField(groupField))) Then
                        Dim groupFieldIterator As XPathNodeIterator = m_Config.Select("/c:dataController/c:fields/c:field[@name='{0}']", groupField)
                        If groupFieldIterator.MoveNext() Then
                            page.Fields.Add(New DataField(groupFieldIterator.Current, Resolver, true))
                        End If
                    End If
                Next
            End If
            Dim i As Integer = 0
            Do While (i < page.Fields.Count)
                Dim field As DataField = page.Fields(i)
                If ((Not (field.FormatOnClient) AndAlso Not (String.IsNullOrEmpty(field.DataFormatString))) AndAlso Not (field.IsMirror)) Then
                    page.Fields.Insert((i + 1), New DataField(field))
                    i = (i + 2)
                Else
                    i = (i + 1)
                End If
            Loop
            Dim dynamicConfigIterator As XPathNodeIterator = m_Config.Select("/c:dataController/c:fields/c:field[c:configuration!='']/c:configuration|/c:dataCo"& _ 
                    "ntroller/c:fields/c:field/c:items[@copy!='']/@copy")
            Do While dynamicConfigIterator.MoveNext()
                Dim dynamicConfig As Match = Regex.Match(dynamicConfigIterator.Current.Value, "(\w+)=(\w+)")
                Do While dynamicConfig.Success
                    Dim groupIndex As Integer = 2
                    If (dynamicConfigIterator.Current.Name = "copy") Then
                        groupIndex = 1
                    End If
                    If Not (page.ContainsField(dynamicConfig.Groups(groupIndex).Value)) Then
                        Dim nav As XPathNavigator = m_Config.SelectSingleNode("/c:dataController/c:fields/c:field[@name='{0}']", dynamicConfig.Groups(1).Value)
                        If (Not (nav) Is Nothing) Then
                            page.Fields.Add(New DataField(nav, Resolver, true))
                        End If
                    End If
                    dynamicConfig = dynamicConfig.NextMatch()
                Loop
                If page.InTransaction Then
                    Dim globalFieldIterator As XPathNodeIterator = m_Config.Select("/c:dataController/c:fields/c:field")
                    Do While globalFieldIterator.MoveNext()
                        Dim fieldName As String = globalFieldIterator.Current.GetAttribute("name", String.Empty)
                        If Not (page.ContainsField(fieldName)) Then
                            page.Fields.Add(New DataField(globalFieldIterator.Current, Resolver, true))
                        End If
                    Loop
                End If
            Loop
            For Each field As DataField in page.Fields
                ConfigureDataField(page, field)
            Next
            If (page.RequiresSiteContentText AndAlso (page.Controller = ApplicationServices.SiteContentControllerName)) Then
                Dim siteContentTextFieldName As String = ApplicationServices.Current.SiteContentFieldName(SiteContentFields.Text)
                If (page.FindField(siteContentTextFieldName) Is Nothing) Then
                    Dim field As DataField = New DataField()
                    field.Name = siteContentTextFieldName
                    field.Type = "String"
                    page.Fields.Add(field)
                End If
            End If
        End Sub
        
        Private Function ParseSelectExpressions(ByVal selectClause As String) As SelectClauseDictionary
            Dim expressions As SelectClauseDictionary = New SelectClauseDictionary()
            Dim fieldMatch As Match = SelectExpressionRegex.Match(selectClause)
            Do While fieldMatch.Success
                Dim expression As String = fieldMatch.Groups("Expression").Value
                Dim fieldName As String = fieldMatch.Groups("FieldName").Value
                Dim [alias] As String = fieldMatch.Groups("Alias").Value
                If Not (String.IsNullOrEmpty(expression)) Then
                    If String.IsNullOrEmpty([alias]) Then
                        If String.IsNullOrEmpty(fieldName) Then
                            [alias] = expression
                        Else
                            [alias] = fieldName
                        End If
                    End If
                    If Not (expressions.ContainsKey([alias])) Then
                        expressions.Add([alias], expression)
                    End If
                End If
                fieldMatch = fieldMatch.NextMatch()
            Loop
            Return expressions
        End Function
        
        Protected Sub PopulatePageFields(ByVal page As ViewPage)
            If (page.Fields.Count > 0) Then
                Return
            End If
            Dim dataFieldIterator As XPathNodeIterator = m_View.Select(".//c:dataFields/c:dataField", Resolver)
            Do While dataFieldIterator.MoveNext()
                Dim fieldIterator As XPathNodeIterator = m_Config.Select("/c:dataController/c:fields/c:field[@name='{0}']", dataFieldIterator.Current.GetAttribute("fieldName", String.Empty))
                If fieldIterator.MoveNext() Then
                    Dim field As DataField = New DataField(fieldIterator.Current, Resolver)
                    field.Hidden = (dataFieldIterator.Current.GetAttribute("hidden", String.Empty) = "true")
                    field.DataFormatString = fieldIterator.Current.GetAttribute("dataFormatString", String.Empty)
                    Dim formatOnClient As String = dataFieldIterator.Current.GetAttribute("formatOnClient", String.Empty)
                    If Not (String.IsNullOrEmpty(formatOnClient)) Then
                        field.FormatOnClient = Not ((formatOnClient = "false"))
                    End If
                    If String.IsNullOrEmpty(field.DataFormatString) Then
                        field.DataFormatString = dataFieldIterator.Current.GetAttribute("dataFormatString", String.Empty)
                    End If
                    field.HeaderText = CType(dataFieldIterator.Current.Evaluate("string(c:headerText)", Resolver),String)
                    field.FooterText = CType(dataFieldIterator.Current.Evaluate("string(c:footerText)", Resolver),String)
                    field.ToolTip = dataFieldIterator.Current.GetAttribute("toolTip", String.Empty)
                    field.Watermark = dataFieldIterator.Current.GetAttribute("watermark", String.Empty)
                    field.HyperlinkFormatString = dataFieldIterator.Current.GetAttribute("hyperlinkFormatString", String.Empty)
                    field.AliasName = dataFieldIterator.Current.GetAttribute("aliasFieldName", String.Empty)
                    field.Tag = dataFieldIterator.Current.GetAttribute("tag", String.Empty)
                    If Not (String.IsNullOrEmpty(dataFieldIterator.Current.GetAttribute("allowQBE", String.Empty))) Then
                        field.AllowQBE = (dataFieldIterator.Current.GetAttribute("allowQBE", String.Empty) = "true")
                    End If
                    If Not (String.IsNullOrEmpty(dataFieldIterator.Current.GetAttribute("allowSorting", String.Empty))) Then
                        field.AllowSorting = (dataFieldIterator.Current.GetAttribute("allowSorting", String.Empty) = "true")
                    End If
                    field.CategoryIndex = Convert.ToInt32(dataFieldIterator.Current.Evaluate("count(parent::c:dataFields/parent::c:category/preceding-sibling::c:category)", Resolver))
                    Dim columns As String = dataFieldIterator.Current.GetAttribute("columns", String.Empty)
                    If Not (String.IsNullOrEmpty(columns)) Then
                        field.Columns = Convert.ToInt32(columns)
                    End If
                    Dim rows As String = dataFieldIterator.Current.GetAttribute("rows", String.Empty)
                    If Not (String.IsNullOrEmpty(rows)) Then
                        field.Rows = Convert.ToInt32(rows)
                    End If
                    Dim textMode As String = dataFieldIterator.Current.GetAttribute("textMode", String.Empty)
                    If Not (String.IsNullOrEmpty(textMode)) Then
                        field.TextMode = CType(TypeDescriptor.GetConverter(GetType(TextInputMode)).ConvertFromString(textMode),TextInputMode)
                    End If
                    Dim maskType As String = fieldIterator.Current.GetAttribute("maskType", String.Empty)
                    If Not (String.IsNullOrEmpty(maskType)) Then
                        field.MaskType = CType(TypeDescriptor.GetConverter(GetType(DataFieldMaskType)).ConvertFromString(maskType),DataFieldMaskType)
                    End If
                    field.Mask = fieldIterator.Current.GetAttribute("mask", String.Empty)
                    Dim [readOnly] As String = dataFieldIterator.Current.GetAttribute("readOnly", String.Empty)
                    If Not (String.IsNullOrEmpty([readOnly])) Then
                        field.ReadOnly = ([readOnly] = "true")
                    End If
                    Dim aggregate As String = dataFieldIterator.Current.GetAttribute("aggregate", String.Empty)
                    If Not (String.IsNullOrEmpty(aggregate)) Then
                        field.Aggregate = CType(TypeDescriptor.GetConverter(GetType(DataFieldAggregate)).ConvertFromString(aggregate),DataFieldAggregate)
                    End If
                    Dim search As String = dataFieldIterator.Current.GetAttribute("search", String.Empty)
                    If Not (String.IsNullOrEmpty(search)) Then
                        field.Search = CType(TypeDescriptor.GetConverter(GetType(FieldSearchMode)).ConvertFromString(search),FieldSearchMode)
                    End If
                    field.SearchOptions = dataFieldIterator.Current.GetAttribute("searchOptions", String.Empty)
                    Dim prefixLength As String = dataFieldIterator.Current.GetAttribute("autoCompletePrefixLength", String.Empty)
                    If Not (String.IsNullOrEmpty(prefixLength)) Then
                        field.AutoCompletePrefixLength = Convert.ToInt32(prefixLength)
                    End If
                    Dim itemsIterator As XPathNodeIterator = dataFieldIterator.Current.Select("c:items[c:item]", Resolver)
                    If Not (itemsIterator.MoveNext()) Then
                        itemsIterator = fieldIterator.Current.Select("c:items", Resolver)
                        If Not (itemsIterator.MoveNext()) Then
                            itemsIterator = Nothing
                        End If
                    End If
                    If (Not (itemsIterator) Is Nothing) Then
                        field.ItemsDataController = itemsIterator.Current.GetAttribute("dataController", String.Empty)
                        field.ItemsDataView = itemsIterator.Current.GetAttribute("dataView", String.Empty)
                        field.ItemsDataValueField = itemsIterator.Current.GetAttribute("dataValueField", String.Empty)
                        field.ItemsDataTextField = itemsIterator.Current.GetAttribute("dataTextField", String.Empty)
                        field.ItemsStyle = itemsIterator.Current.GetAttribute("style", String.Empty)
                        field.ItemsNewDataView = itemsIterator.Current.GetAttribute("newDataView", String.Empty)
                        field.ItemsTargetController = itemsIterator.Current.GetAttribute("targetController", String.Empty)
                        field.Copy = itemsIterator.Current.GetAttribute("copy", String.Empty)
                        Dim pageSize As String = itemsIterator.Current.GetAttribute("pageSize", String.Empty)
                        If Not (String.IsNullOrEmpty(pageSize)) Then
                            field.ItemsPageSize = Convert.ToInt32(pageSize)
                        End If
                        field.ItemsLetters = (itemsIterator.Current.GetAttribute("letters", String.Empty) = "true")
                        Dim itemIterator As XPathNodeIterator = itemsIterator.Current.Select("c:item", Resolver)
                        Do While itemIterator.MoveNext()
                            Dim itemValue As String = itemIterator.Current.GetAttribute("value", String.Empty)
                            If (itemValue = "NULL") Then
                                itemValue = String.Empty
                            End If
                            Dim itemText As String = itemIterator.Current.GetAttribute("text", String.Empty)
                            field.Items.Add(New Object() {itemValue, itemText})
                        Loop
                        If (Not (String.IsNullOrEmpty(field.ItemsNewDataView)) AndAlso (((ActionArgs.Current Is Nothing) OrElse (ActionArgs.Current.Controller = field.ItemsDataController)) AndAlso ((PageRequest.Current Is Nothing) OrElse (PageRequest.Current.Controller = field.ItemsDataController)))) Then
                            Dim itemsController As Controller = CType(Me.GetType().Assembly.CreateInstance(Me.GetType().FullName),Controller)
                            itemsController.SelectView(field.ItemsDataController, field.ItemsNewDataView)
                            Dim roles As String = CType(itemsController.m_Config.Evaluate("string(//c:action[@commandName='New' and @commandArgument='{0}'][1]/@roles)", field.ItemsNewDataView),String)
                            If Not (Controller.UserIsInRole(roles)) Then
                                field.ItemsNewDataView = Nothing
                            End If
                        End If
                        field.AutoSelect = (itemsIterator.Current.GetAttribute("autoSelect", String.Empty) = "true")
                        field.SearchOnStart = (itemsIterator.Current.GetAttribute("searchOnStart", String.Empty) = "true")
                        field.ItemsDescription = itemsIterator.Current.GetAttribute("description", String.Empty)
                    End If
                    If Not (Controller.UserIsInRole(fieldIterator.Current.GetAttribute("writeRoles", String.Empty))) Then
                        field.ReadOnly = true
                    End If
                    If Not (Controller.UserIsInRole(fieldIterator.Current.GetAttribute("roles", String.Empty))) Then
                        field.ReadOnly = true
                        field.Hidden = true
                    End If
                    page.Fields.Add(field)
                    'populate data view field values
                    Dim dataViewNav As XPathNavigator = dataFieldIterator.Current.SelectSingleNode("c:dataView", Resolver)
                    If (Not (dataViewNav) Is Nothing) Then
                        field.DataViewShowInSummary = (dataViewNav.GetAttribute("showInSummary", String.Empty) = "true")
                        field.DataViewShowActionBar = (dataViewNav.GetAttribute("showActionBar", String.Empty) = "true")
                        field.DataViewShowActionButtons = dataViewNav.GetAttribute("showActionButtons", String.Empty)
                        field.DataViewShowDescription = (dataViewNav.GetAttribute("showDescription", String.Empty) = "true")
                        field.DataViewShowViewSelector = (dataViewNav.GetAttribute("showViewSelector", String.Empty) = "true")
                        field.DataViewShowModalForms = (dataViewNav.GetAttribute("showModalForms", String.Empty) = "true")
                        field.DataViewSearchByFirstLetter = (dataViewNav.GetAttribute("searchByFirstLetter", String.Empty) = "true")
                        field.DataViewSearchOnStart = (dataViewNav.GetAttribute("searchOnStart", String.Empty) = "true")
                        Dim pageSize As String = dataViewNav.GetAttribute("pageSize", String.Empty)
                        If Not (String.IsNullOrEmpty(pageSize)) Then
                            field.DataViewPageSize = Convert.ToInt32(pageSize)
                        End If
                        field.DataViewMultiSelect = dataViewNav.GetAttribute("multiSelect", String.Empty)
                        field.DataViewShowPager = (dataViewNav.GetAttribute("showPager", String.Empty) = "true")
                        field.DataViewShowPageSize = (dataViewNav.GetAttribute("showPageSize", String.Empty) = "true")
                        field.DataViewShowSearchBar = (dataViewNav.GetAttribute("showSearchBar", String.Empty) = "true")
                        field.DataViewShowQuickFind = (dataViewNav.GetAttribute("showQuickFind", String.Empty) = "true")
                        field.DataViewShowRowNumber = (dataViewNav.GetAttribute("showRowNumber", String.Empty) = "true")
                        field.DataViewAutoSelectFirstRow = (dataViewNav.GetAttribute("autoSelectFirstRow", String.Empty) = "true")
                        field.DataViewAutoHighlightFirstRow = (dataViewNav.GetAttribute("autoHighlightFirstRow", String.Empty) = "true")
                    End If
                    'populate pivot info
                    If page.RequiresPivot Then
                        If ((Not (page.PivotDefinitions) Is Nothing) AndAlso (page.PivotDefinitions.Count > 0)) Then
                            field.Tag = String.Empty
                            If page.PivotDefinitions.ContainsKey(field.Name) Then
                                field.Tag = page.PivotDefinitions(field.Name)
                            End If
                        End If
                        For Each tag As String in field.Tag.Split(Global.Microsoft.VisualBasic.ChrW(32))
                            If tag.StartsWith("pivot") Then
                                page.AddPivotField(field)
                                Exit For
                            End If
                        Next
                    End If
                End If
            Loop
        End Sub
        
        Protected Overridable Sub ConfigureDataField(ByVal page As ViewPage, ByVal field As DataField)
        End Sub
        
        Public Shared Function LookupText(ByVal controllerName As String, ByVal filterExpression As String, ByVal fieldNames As String) As String
            Dim dataTextFields() As String = fieldNames.Split(Global.Microsoft.VisualBasic.ChrW(44))
            Dim request As PageRequest = New PageRequest(-1, 1, Nothing, New String() {filterExpression})
            Dim page As ViewPage = ControllerFactory.CreateDataController().GetPage(controllerName, String.Empty, request)
            Dim result As String = String.Empty
            If (page.Rows.Count > 0) Then
                Dim i As Integer = 0
                Do While (i < page.Fields.Count)
                    Dim field As DataField = page.Fields(i)
                    If (Array.IndexOf(dataTextFields, field.Name) >= 0) Then
                        If (result.Length > 0) Then
                            result = (result + "; ")
                        End If
                        result = (result + Convert.ToString(page.Rows(0)(i)))
                    End If
                    i = (i + 1)
                Loop
            End If
            Return result
        End Function
        
        Public Shared Function ConvertSampleToQuery(ByVal sample As String) As String
            Dim m As Match = Regex.Match(sample, "^\s*(?'Operation'(<|>)={0,1}){0,1}\s*(?'Value'.+)\s*$")
            If Not (m.Success) Then
                Return Nothing
            End If
            Dim operation As String = m.Groups("Operation").Value
            sample = m.Groups("Value").Value.Trim()
            If String.IsNullOrEmpty(operation) Then
                operation = "*"
                Dim doubleTest As Double
                If [Double].TryParse(sample, doubleTest) Then
                    operation = "="
                Else
                    Dim boolTest As Boolean
                    If [Boolean].TryParse(sample, boolTest) Then
                        operation = "="
                    Else
                        Dim dateTest As DateTime
                        If DateTime.TryParse(sample, dateTest) Then
                            operation = "="
                        End If
                    End If
                End If
            End If
            Return String.Format("{0}{1}{2}", operation, sample, Convert.ToChar(0))
        End Function
        
        Public Shared Function LookupActionArgument(ByVal controllerName As String, ByVal commandName As String) As String
            Dim c As Controller = New Controller()
            c.SelectView(controllerName, Nothing)
            Dim action As XPathNavigator = c.m_Config.SelectSingleNode("//c:action[@commandName='{0}' and contains(@commandArgument, 'Form')]", commandName)
            If (action Is Nothing) Then
                Return Nothing
            End If
            If Not (UserIsInRole(action.GetAttribute("roles", String.Empty))) Then
                Return Nothing
            End If
            Return action.GetAttribute("commandArgument", String.Empty)
        End Function
        
        Public Overloads Shared Function CreateReportInstance(ByVal t As Type, ByVal name As String, ByVal controller As String, ByVal view As String) As String
            Return CreateReportInstance(t, name, controller, view, true)
        End Function
        
        Public Overloads Shared Function CreateReportInstance(ByVal t As Type, ByVal name As String, ByVal controller As String, ByVal view As String, ByVal validate As Boolean) As String
            If String.IsNullOrEmpty(name) Then
                Dim instance As String = CreateReportInstance(t, String.Format("{0}_{1}.rdlc", controller, view), controller, view, false)
                If Not (String.IsNullOrEmpty(instance)) Then
                    Return instance
                End If
                instance = CreateReportInstance(t, "CustomTemplate.xslt", controller, view, false)
                If Not (String.IsNullOrEmpty(instance)) Then
                    Return instance
                End If
                name = "Template.xslt"
            End If
            Dim isGeneric As Boolean = (Path.GetExtension(name).ToLower() = ".xslt")
            Dim reportKey As String = ("Report_" + name)
            If isGeneric Then
                reportKey = String.Format("Reports_{0}_{1}", controller, view)
            End If
            Dim report As String = Nothing
            'try loading a report as a resource or from the folder ~/Reports/
            If (t Is Nothing) Then
                t = GetType(MyCompany.Data.Controller)
            End If
            Dim res As Stream = t.Assembly.GetManifestResourceStream(String.Format("MyCompany.Reports.{0}", name))
            If (res Is Nothing) Then
                res = t.Assembly.GetManifestResourceStream(String.Format("MyCompany.{0}", name))
            End If
            If (res Is Nothing) Then
                Dim templatePath As String = Path.Combine(Path.Combine(HttpRuntime.AppDomainAppPath, "Reports"), name)
                If Not (File.Exists(templatePath)) Then
                    If validate Then
                        Throw New Exception(String.Format("Report or report template \'{0}\' does not exist.", name))
                    Else
                        Return Nothing
                    End If
                End If
                report = File.ReadAllText(templatePath)
            Else
                Dim reader As StreamReader = New StreamReader(res)
                report = reader.ReadToEnd()
                reader.Close()
            End If
            If isGeneric Then
                'transform a data controller into a report by applying the specified template
                Dim config As ControllerConfiguration = MyCompany.Data.Controller.CreateConfigurationInstance(t, controller)
                Dim arguments As XsltArgumentList = New XsltArgumentList()
                arguments.AddParam("ViewName", String.Empty, view)
                Dim transform As XslCompiledTransform = New XslCompiledTransform()
                transform.Load(New XPathDocument(New StringReader(report)))
                Dim output As MemoryStream = New MemoryStream()
                transform.Transform(config.TrimmedNavigator, arguments, output)
                output.Position = 0
                Dim sr As StreamReader = New StreamReader(output)
                report = sr.ReadToEnd()
                sr.Close()
            End If
            report = Regex.Replace(report, "(<Language>)(.+?)(</Language>)", String.Format("$1{0}$3", System.Threading.Thread.CurrentThread.CurrentUICulture.Name))
            report = Localizer.Replace("Reports", name, report)
            Return report
        End Function
        
        Public Shared Function FindSelectedValueByTag(ByVal tag As String) As Object
            Dim serializer As System.Web.Script.Serialization.JavaScriptSerializer = New System.Web.Script.Serialization.JavaScriptSerializer()
            Dim selectedValues() As Object = serializer.Deserialize(Of Object())(HttpContext.Current.Request.Form("__WEB_DATAVIEWSTATE"))
            If (Not (selectedValues) Is Nothing) Then
                Dim i As Integer = 0
                Do While (i < selectedValues.Length)
                    Dim k As String = CType(selectedValues(i),String)
                    i = (i + 1)
                    If (k = tag) Then
                        Dim v() As Object = CType(selectedValues(i),Object())
                        If ((v Is Nothing) OrElse (v.Length = 0)) Then
                            Return Nothing
                        End If
                        If (v.Length = 1) Then
                            Return v(0)
                        End If
                        Return v
                    End If
                    i = (i + 1)
                Loop
            End If
            Return Nothing
        End Function
    End Class
End Namespace

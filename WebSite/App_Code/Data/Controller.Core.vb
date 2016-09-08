Imports MyCompany.Handlers
Imports MyCompany.Services
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Configuration
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography
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
    
    Partial Public Class Controller
        Inherits DataControllerBase
    End Class
    
    Partial Public Class DataControllerBase
        Inherits Object
        Implements IDataController, IAutoCompleteManager, IDataEngine, IBusinessObject
        
        Public Const MaximumDistinctValues As Integer = 200
        
        Public Shared SpecialConversionTypes() As Type = New Type() {GetType(System.Guid), GetType(System.DateTimeOffset), GetType(System.TimeSpan)}
        
        Public Shared SpecialConverters() As SpecialConversionFunction
        
        Public Shared ISO8601DateStringMatcher As Regex = New Regex("\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}")
        
        Public Shared SpecialTypes() As String = New String() {"System.DateTimeOffset", "System.TimeSpan", "Microsoft.SqlServer.Types.SqlGeography", "Microsoft.SqlServer.Types.SqlHierarchyId"}
        
        Private m_ServerRules As BusinessRules
        
        Private m_JunctionTableMap As SortedDictionary(Of String, List(Of String))
        
        Private m_JunctionTableFieldName As String
        
        Public Shared DefaultDataControllerStream As Stream = New MemoryStream()
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_AllowPublicAccess As Boolean
        
        Private m_ResultSetParameters As DbParameterCollection
        
        Shared Sub New()
            'initialize type map
            m_TypeMap = New SortedDictionary(Of String, Type)()
            m_TypeMap.Add("AnsiString", GetType(String))
            m_TypeMap.Add("Binary", GetType(Byte()))
            m_TypeMap.Add("Byte", GetType(Byte))
            m_TypeMap.Add("Boolean", GetType(Boolean))
            m_TypeMap.Add("Currency", GetType(Decimal))
            m_TypeMap.Add("Date", GetType(DateTime))
            m_TypeMap.Add("DateTime", GetType(DateTime))
            m_TypeMap.Add("Decimal", GetType(Decimal))
            m_TypeMap.Add("Double", GetType(Double))
            m_TypeMap.Add("Guid", GetType(Guid))
            m_TypeMap.Add("Int16", GetType(Short))
            m_TypeMap.Add("Int32", GetType(Integer))
            m_TypeMap.Add("Int64", GetType(Long))
            m_TypeMap.Add("Object", GetType(Object))
            m_TypeMap.Add("SByte", GetType(SByte))
            m_TypeMap.Add("Single", GetType(Single))
            m_TypeMap.Add("String", GetType(String))
            m_TypeMap.Add("Time", GetType(TimeSpan))
            m_TypeMap.Add("TimeSpan", GetType(DateTime))
            m_TypeMap.Add("UInt16", GetType(UShort))
            m_TypeMap.Add("UInt32", GetType(UInteger))
            m_TypeMap.Add("UInt64", GetType(ULong))
            m_TypeMap.Add("VarNumeric", GetType(Object))
            m_TypeMap.Add("AnsiStringFixedLength", GetType(String))
            m_TypeMap.Add("StringFixedLength", GetType(String))
            m_TypeMap.Add("Xml", GetType(String))
            m_TypeMap.Add("DateTime2", GetType(DateTime))
            m_TypeMap.Add("DateTimeOffset", GetType(DateTimeOffset))
            'initialize rowset type map
            m_RowsetTypeMap = New SortedDictionary(Of String, String)()
            m_RowsetTypeMap.Add("AnsiString", "string")
            m_RowsetTypeMap.Add("Binary", "bin.base64")
            m_RowsetTypeMap.Add("Byte", "u1")
            m_RowsetTypeMap.Add("Boolean", "boolean")
            m_RowsetTypeMap.Add("Currency", "float")
            m_RowsetTypeMap.Add("Date", "date")
            m_RowsetTypeMap.Add("DateTime", "dateTime")
            m_RowsetTypeMap.Add("Decimal", "float")
            m_RowsetTypeMap.Add("Double", "float")
            m_RowsetTypeMap.Add("Guid", "uuid")
            m_RowsetTypeMap.Add("Int16", "i2")
            m_RowsetTypeMap.Add("Int32", "i4")
            m_RowsetTypeMap.Add("Int64", "i8")
            m_RowsetTypeMap.Add("Object", "string")
            m_RowsetTypeMap.Add("SByte", "i1")
            m_RowsetTypeMap.Add("Single", "float")
            m_RowsetTypeMap.Add("String", "string")
            m_RowsetTypeMap.Add("Time", "time")
            m_RowsetTypeMap.Add("UInt16", "u2")
            m_RowsetTypeMap.Add("UInt32", "u4")
            m_RowsetTypeMap.Add("UIn64", "u8")
            m_RowsetTypeMap.Add("VarNumeric", "float")
            m_RowsetTypeMap.Add("AnsiStringFixedLength", "string")
            m_RowsetTypeMap.Add("StringFixedLength", "string")
            m_RowsetTypeMap.Add("Xml", "string")
            m_RowsetTypeMap.Add("DateTime2", "dateTime")
            m_RowsetTypeMap.Add("DateTimeOffset", "dateTime.tz")
            m_RowsetTypeMap.Add("TimeSpan", "time")
            'initialize the special converters
            SpecialConverters = New SpecialConversionFunction((SpecialConversionTypes.Length) - 1) {}
            SpecialConverters(0) = AddressOf ConvertToGuid
            SpecialConverters(1) = AddressOf ConvertToDateTimeOffset
            SpecialConverters(2) = AddressOf ConvertToTimeSpan
        End Sub
        
        Public Sub New()
            MyBase.New
            Initialize()
        End Sub
        
        Protected Overridable ReadOnly Property HierarchyOrganizationFieldName() As String
            Get
                Return "HierarchyOrganization__"
            End Get
        End Property
        
        Public Overridable Property AllowPublicAccess() As Boolean
            Get
                Return Me.m_AllowPublicAccess
            End Get
            Set
                Me.m_AllowPublicAccess = value
            End Set
        End Property
        
        Protected Overridable Sub Initialize()
            CultureManager.Initialize()
        End Sub
        
        Public Shared Function StringIsNull(ByVal s As String) As Boolean
            Return ((s = "null") OrElse (s = "%js%null"))
        End Function
        
        Public Shared Function ConvertToGuid(ByVal o As Object) As Object
            Return New Guid(Convert.ToString(o))
        End Function
        
        Public Shared Function ConvertToDateTimeOffset(ByVal o As Object) As Object
            Return System.DateTimeOffset.Parse(Convert.ToString(o))
        End Function
        
        Public Shared Function ConvertToTimeSpan(ByVal o As Object) As Object
            Return System.TimeSpan.Parse(Convert.ToString(o))
        End Function
        
        Public Shared Function ConvertToType(ByVal targetType As Type, ByVal o As Object) As Object
            If targetType.IsGenericType Then
                targetType = targetType.GetProperty("Value").PropertyType
            End If
            If ((o Is Nothing) OrElse o.GetType().Equals(targetType)) Then
                Return o
            End If
            Dim i As Integer = 0
            Do While (i < SpecialConversionTypes.Length)
                Dim t As Type = SpecialConversionTypes(i)
                If (t Is targetType) Then
                    Return SpecialConverters(i)(o)
                End If
                i = (i + 1)
            Loop
            If TypeOf o Is IConvertible Then
                o = Convert.ChangeType(o, targetType)
                If TypeOf o Is DateTime Then
                    o = CType(o,DateTime).ToUniversalTime()
                End If
            Else
                If (targetType.Equals(GetType(String)) AndAlso (Not (o) Is Nothing)) Then
                    o = o.ToString()
                End If
            End If
            Return o
        End Function
        
        Public Shared Function ValueToString(ByVal o As Object) As String
            Dim serializer As System.Web.Script.Serialization.JavaScriptSerializer = New System.Web.Script.Serialization.JavaScriptSerializer()
            If ((Not (o) Is Nothing) AndAlso TypeOf o Is Date) Then
                o = CType(o,DateTime).ToString("yyyy-MM-ddTHH\:mm\:ss.fff")
            End If
            Return ("%js%" + serializer.Serialize(o))
        End Function
        
        Public Overloads Shared Function StringToValue(ByVal s As String) As Object
            Return StringToValue(Nothing, s)
        End Function
        
        Public Overloads Shared Function StringToValue(ByVal field As DataField, ByVal s As String) As Object
            If (Not (String.IsNullOrEmpty(s)) AndAlso s.StartsWith("%js%")) Then
                Dim serializer As System.Web.Script.Serialization.JavaScriptSerializer = New System.Web.Script.Serialization.JavaScriptSerializer()
                Dim v As Object = serializer.Deserialize(Of Object)(s.Substring(4))
                If (TypeOf v Is String AndAlso ISO8601DateStringMatcher.IsMatch(CType(v,String))) Then
                    Return Date.Parse(CType(v,String))
                End If
                If (Not (TypeOf v Is String) OrElse ((field Is Nothing) OrElse (field.Type = "String"))) Then
                    Return v
                End If
                s = CType(v,String)
            Else
                If ISO8601DateStringMatcher.IsMatch(s) Then
                    Return Date.Parse(s)
                End If
            End If
            If (Not (field) Is Nothing) Then
                Return TypeDescriptor.GetConverter(Controller.TypeMap(field.Type)).ConvertFromString(s)
            End If
            Return s
        End Function
        
        Public Shared Function ConvertObjectToValue(ByVal o As Object) As Object
            If SpecialTypes.Contains(o.GetType().FullName) Then
                Return o.ToString()
            End If
            Return o
        End Function
        
        Public Shared Function EnsureJsonCompatibility(ByVal o As Object) As Object
            If (Not (o) Is Nothing) Then
                If TypeOf o Is List(Of Object()) Then
                    For Each values() As Object in CType(o,List(Of Object()))
                        EnsureJsonCompatibility(values)
                    Next
                Else
                    If (TypeOf o Is Array AndAlso (o.GetType().GetElementType() Is GetType(Object))) Then
                        Dim row() As Object = CType(o,Object())
                        Dim i As Integer = 0
                        Do While (i < row.Length)
                            row(i) = EnsureJsonCompatibility(row(i))
                            i = (i + 1)
                        Loop
                    Else
                        If TypeOf o Is DateTime Then
                            Return CType(o,DateTime).ToString("yyyy-MM-ddTHH\:mm\:ss.fff")
                        End If
                    End If
                End If
            End If
            Return o
        End Function
        
        Protected Function CreateBusinessRules() As BusinessRules
            Return BusinessRules.Create(m_Config)
        End Function
        
        Private Sub ApplyFieldFilter(ByVal page As ViewPage)
            If ((Not (page.FieldFilter) Is Nothing) AndAlso (page.FieldFilter.Length > 0)) Then
                Dim newFields As List(Of DataField) = New List(Of DataField)()
                For Each f As DataField in page.Fields
                    If (f.IsPrimaryKey OrElse page.IncludeField(f.Name)) Then
                        newFields.Add(f)
                    End If
                Next
                page.Fields.Clear()
                page.Fields.AddRange(newFields)
                page.FieldFilter = Nothing
            End If
        End Sub
        
        Protected Overridable Function InitBusinessRules(ByVal request As PageRequest, ByVal page As ViewPage) As BusinessRules
            Dim rules As BusinessRules = m_Config.CreateBusinessRules()
            m_ServerRules = rules
            If (m_ServerRules Is Nothing) Then
                m_ServerRules = CreateBusinessRules()
            End If
            m_ServerRules.Page = page
            m_ServerRules.RequiresRowCount = (page.RequiresRowCount AndAlso Not ((request.Inserting OrElse request.DoesNotRequireData)))
            If (Not (rules) Is Nothing) Then
                rules.BeforeSelect(request)
            Else
                m_ServerRules.ExecuteServerRules(request, ActionPhase.Before)
            End If
            Return rules
        End Function
        
        Public Overridable Function GetPageList(ByVal requests() As PageRequest) As ViewPage()
            Dim result As List(Of ViewPage) = New List(Of ViewPage)()
            For Each r As PageRequest in requests
                result.Add(ControllerFactory.CreateDataController().GetPage(r.Controller, r.View, r))
            Next
            Return result.ToArray()
        End Function
        
        Function IDataController_GetPage(ByVal controller As String, ByVal view As String, ByVal request As PageRequest) As ViewPage Implements IDataController.GetPage
            SelectView(controller, view)
            request.AssignContext(controller, Me.m_ViewId, m_Config)
            Dim page As ViewPage = New ViewPage(request)
            If ((Not (page.FieldFilter) Is Nothing) AndAlso ((page.FieldFilter.Length > 0) AndAlso (Not (Config.SelectSingleNode("/c:dataController/c:businessRules/c:rule[@commandName='Select']")) Is Nothing))) Then
                page.FieldFilter = Nothing
            End If
            If (Not (m_Config.PlugIn) Is Nothing) Then
                m_Config.PlugIn.PreProcessPageRequest(request, page)
            End If
            m_Config.AssignDynamicExpressions(page)
            page.ApplyDataFilter(m_Config.CreateDataFilter(), request.Controller, request.View, request.LookupContextController, request.LookupContextView, request.LookupContextFieldName)
            Dim rules As BusinessRules = InitBusinessRules(request, page)
            Using connection As DbConnection = CreateConnection()
                If m_ServerRules.RequiresRowCount Then
                    Dim countCommand As DbCommand = CreateCommand(connection)
                    ConfigureCommand(countCommand, page, CommandConfigurationType.SelectCount, Nothing)
                    If m_ServerRules.EnableResultSet Then
                        page.TotalRowCount = m_ServerRules.ResultSetSize
                    Else
                        If YieldsSingleRow(countCommand) Then
                            page.TotalRowCount = 1
                        Else
                            page.TotalRowCount = Convert.ToInt32(countCommand.ExecuteScalar())
                        End If
                    End If
                    If (Not (request.DoesNotRequireAggregates) AndAlso page.RequiresAggregates) Then
                        Dim aggregates((page.Fields.Count) - 1) As Object
                        If m_ServerRules.EnableResultSet Then
                            Dim dt As DataTable = ExecuteResultSetTable(page)
                            Dim j As Integer = 0
                            Do While (j < aggregates.Length)
                                Dim field As DataField = page.Fields(j)
                                If Not ((field.Aggregate = DataFieldAggregate.None)) Then
                                    Dim func As String = field.Aggregate.ToString()
                                    If (func = "Count") Then
                                        Dim uniqueValues As SortedDictionary(Of String, String) = New SortedDictionary(Of String, String)()
                                        For Each r As DataRow in dt.Rows
                                            Dim v As Object = r(field.Name)
                                            If Not (DBNull.Value.Equals(v)) Then
                                                uniqueValues(v.ToString()) = Nothing
                                            End If
                                        Next
                                        aggregates(j) = uniqueValues.Keys.Count
                                    Else
                                        If (func = "Average") Then
                                            func = "avg"
                                        End If
                                        aggregates(j) = dt.Compute(String.Format("{0}([{1}])", func, field.Name), Nothing)
                                    End If
                                End If
                                j = (j + 1)
                            Loop
                        Else
                            Dim aggregateCommand As DbCommand = CreateCommand(connection)
                            ConfigureCommand(aggregateCommand, page, CommandConfigurationType.SelectAggregates, Nothing)
                            Dim reader As DbDataReader = aggregateCommand.ExecuteReader()
                            If reader.Read() Then
                                Dim j As Integer = 0
                                Do While (j < aggregates.Length)
                                    Dim field As DataField = page.Fields(j)
                                    If Not ((field.Aggregate = DataFieldAggregate.None)) Then
                                        aggregates(j) = reader(field.Name)
                                    End If
                                    j = (j + 1)
                                Loop
                            End If
                            reader.Close()
                        End If
                        Dim i As Integer = 0
                        Do While (i < aggregates.Length)
                            Dim field As DataField = page.Fields(i)
                            If Not ((field.Aggregate = DataFieldAggregate.None)) Then
                                Dim v As Object = aggregates(i)
                                If (Not (DBNull.Value.Equals(v)) AndAlso (Not (v) Is Nothing)) Then
                                    If (Not (field.FormatOnClient) AndAlso Not (String.IsNullOrEmpty(field.DataFormatString))) Then
                                        v = String.Format(field.DataFormatString, v)
                                    End If
                                    aggregates(i) = v
                                End If
                            End If
                            i = (i + 1)
                        Loop
                        page.Aggregates = aggregates
                    End If
                End If
                Dim selectCommand As DbCommand = CreateCommand(connection)
                If ((selectCommand Is Nothing) AndAlso m_ServerRules.EnableResultSet) Then
                    PopulatePageFields(page)
                    EnsurePageFields(page, Nothing)
                End If
                If (page.RequiresMetaData AndAlso page.IncludeMetadata("categories")) Then
                    PopulatePageCategories(page)
                End If
                SyncRequestedPage(request, page, connection)
                ConfigureCommand(selectCommand, page, CommandConfigurationType.Select, Nothing)
                If ((page.PageSize > 0) AndAlso Not ((request.Inserting OrElse request.DoesNotRequireData))) Then
                    EnsureSystemPageFields(request, page, selectCommand)
                    Dim reader As DbDataReader = ExecuteResultSetReader(page)
                    If (reader Is Nothing) Then
                        If (selectCommand Is Nothing) Then
                            reader = ExecuteVirtualReader(request, page)
                        Else
                            reader = TransactionManager.ExecuteReader(request, page, selectCommand)
                        End If
                    End If
                    Do While page.SkipNext()
                        reader.Read()
                    Loop
                    Do While (page.ReadNext() AndAlso reader.Read())
                        Dim values((page.Fields.Count) - 1) As Object
                        Dim i As Integer = 0
                        Do While (i < values.Length)
                            Dim field As DataField = page.Fields(i)
                            Dim v As Object = reader(field.Name)
                            If Not (DBNull.Value.Equals(v)) Then
                                If field.IsMirror Then
                                    v = String.Format(field.DataFormatString, v)
                                Else
                                    If ((field.Type = "Guid") AndAlso (v.GetType() Is GetType(Byte()))) Then
                                        v = New Guid(CType(v,Byte()))
                                    Else
                                        v = ConvertObjectToValue(v)
                                    End If
                                End If
                                values(i) = v
                            End If
                            If Not (String.IsNullOrEmpty(field.SourceFields)) Then
                                values(i) = CreateValueFromSourceFields(field, reader)
                            End If
                            i = (i + 1)
                        Loop
                        If page.RequiresPivot Then
                            page.AddPivotValues(values)
                        Else
                            page.Rows.Add(values)
                        End If
                    Loop
                    reader.Close()
                End If
                If (request.RequiresFirstLetters AndAlso Not ((Me.m_ViewType = "Form"))) Then
                    If Not (page.RequiresRowCount) Then
                        page.FirstLetters = String.Empty
                    Else
                        Dim firstLettersCommand As DbCommand = CreateCommand(connection)
                        Dim oldFilter() As String = page.Filter
                        ConfigureCommand(firstLettersCommand, page, CommandConfigurationType.SelectFirstLetters, Nothing)
                        page.Filter = oldFilter
                        If Not (String.IsNullOrEmpty(page.FirstLetters)) Then
                            Dim reader As DbDataReader = firstLettersCommand.ExecuteReader()
                            Dim firstLetters As StringBuilder = New StringBuilder(page.FirstLetters)
                            Do While reader.Read()
                                firstLetters.Append(",")
                                Dim letter As String = Convert.ToString(reader(0))
                                If Not (String.IsNullOrEmpty(letter)) Then
                                    firstLetters.Append(letter)
                                End If
                            Loop
                            reader.Close()
                            page.FirstLetters = firstLetters.ToString()
                        End If
                    End If
                End If
            End Using
            If (Not (m_Config.PlugIn) Is Nothing) Then
                m_Config.PlugIn.ProcessPageRequest(request, page)
            End If
            If request.Inserting Then
                page.NewRow = New Object((page.Fields.Count) - 1) {}
            End If
            If request.Inserting Then
                If m_ServerRules.SupportsCommand("Sql|Code", "New") Then
                    m_ServerRules.ExecuteServerRules(request, ActionPhase.Execute, "New", page.NewRow)
                End If
            Else
                If m_ServerRules.SupportsCommand("Sql|Code", "Select") Then
                    For Each row() As Object in page.Rows
                        m_ServerRules.ExecuteServerRules(request, ActionPhase.Execute, "Select", row)
                    Next
                End If
            End If
            If Not (request.Inserting) Then
                PopulateManyToManyFields(page)
            End If
            If (Not (rules) Is Nothing) Then
                Dim rowHandler As IRowHandler = rules
                If request.Inserting Then
                    If rowHandler.SupportsNewRow(request) Then
                        rowHandler.NewRow(request, page, page.NewRow)
                    End If
                Else
                    If rowHandler.SupportsPrepareRow(request) Then
                        For Each row() As Object in page.Rows
                            rowHandler.PrepareRow(request, page, row)
                        Next
                    End If
                End If
                rules.ProcessPageRequest(request, page)
                If rules.CompleteConfiguration() Then
                    ResetViewPage(page)
                End If
            End If
            If (Not (rules) Is Nothing) Then
                rules.AfterSelect(request)
            Else
                m_ServerRules.ExecuteServerRules(request, ActionPhase.After)
            End If
            m_ServerRules.Result.Merge(page)
            Return page.ToResult(m_Config, m_View)
        End Function
        
        Public Overridable Sub ResetViewPage(ByVal page As ViewPage)
            page.RequiresMetaData = true
            Dim fieldIndexes As SortedDictionary(Of String, Integer) = New SortedDictionary(Of String, Integer)()
            Dim i As Integer = 0
            Do While (i < page.Fields.Count)
                fieldIndexes(page.Fields(i).Name) = i
                i = (i + 1)
            Loop
            page.Fields.Clear()
            page.Categories.Clear()
            PopulatePageFields(page)
            EnsurePageFields(page, m_Expressions)
            PopulatePageCategories(page)
            If (Not (page.NewRow) Is Nothing) Then
                page.NewRow = ReorderRowValues(page, fieldIndexes, page.NewRow)
            End If
            If (Not (page.Rows) Is Nothing) Then
                Dim j As Integer = 0
                Do While (j < page.Rows.Count)
                    page.Rows(j) = ReorderRowValues(page, fieldIndexes, page.Rows(j))
                    j = (j + 1)
                Loop
            End If
        End Sub
        
        Private Function ReorderRowValues(ByVal page As ViewPage, ByVal indexes As SortedDictionary(Of String, Integer), ByVal row() As Object) As Object()
            Dim newRow((row.Length) - 1) As Object
            Dim i As Integer = 0
            Do While (i < page.Fields.Count)
                Dim field As DataField = page.Fields(i)
                newRow(i) = row(indexes(field.Name))
                i = (i + 1)
            Loop
            Return newRow
        End Function
        
        Function IDataController_GetListOfValues(ByVal controller As String, ByVal view As String, ByVal request As DistinctValueRequest) As Object() Implements IDataController.GetListOfValues
            SelectView(controller, view)
            Dim page As ViewPage = New ViewPage(request)
            page.ApplyDataFilter(m_Config.CreateDataFilter(), controller, view, request.LookupContextController, request.LookupContextView, request.LookupContextFieldName)
            Dim distinctValues As List(Of Object) = New List(Of Object)()
            Dim rules As BusinessRules = m_Config.CreateBusinessRules()
            m_ServerRules = rules
            If (m_ServerRules Is Nothing) Then
                m_ServerRules = CreateBusinessRules()
            End If
            m_ServerRules.Page = page
            If (Not (rules) Is Nothing) Then
                rules.BeforeSelect(request)
            Else
                m_ServerRules.ExecuteServerRules(request, ActionPhase.Before)
            End If
            If m_ServerRules.EnableResultSet Then
                Dim reader As IDataReader = ExecuteResultSetReader(page)
                Dim uniqueValues As SortedDictionary(Of Object, Object) = New SortedDictionary(Of Object, Object)()
                Dim hasNull As Boolean = false
                Do While reader.Read()
                    Dim v As Object = reader(request.FieldName)
                    If DBNull.Value.Equals(v) Then
                        hasNull = true
                    Else
                        uniqueValues(v) = v
                    End If
                Loop
                If hasNull Then
                    distinctValues.Add(Nothing)
                End If
                For Each v As Object in uniqueValues.Keys
                    If (distinctValues.Count < page.PageSize) Then
                        distinctValues.Add(ConvertObjectToValue(v))
                    Else
                        Exit For
                    End If
                Next
            Else
                Using connection As DbConnection = CreateConnection()
                    Dim command As DbCommand = CreateCommand(connection)
                    ConfigureCommand(command, page, CommandConfigurationType.SelectDistinct, Nothing)
                    Dim reader As DbDataReader = command.ExecuteReader()
                    Do While (reader.Read() AndAlso (distinctValues.Count < page.PageSize))
                        Dim v As Object = reader.GetValue(0)
                        If Not (DBNull.Value.Equals(v)) Then
                            v = ConvertObjectToValue(v)
                        End If
                        distinctValues.Add(v)
                    Loop
                    reader.Close()
                End Using
            End If
            If (Not (rules) Is Nothing) Then
                rules.AfterSelect(request)
            Else
                m_ServerRules.ExecuteServerRules(request, ActionPhase.After)
            End If
            Dim result() As Object = distinctValues.ToArray()
            EnsureJsonCompatibility(result)
            Return result
        End Function
        
        Function IDataController_Execute(ByVal controller As String, ByVal view As String, ByVal args As ActionArgs) As ActionResult Implements IDataController.Execute
            Dim result As ActionResult = New ActionResult()
            SelectView(controller, view)
            Try 
                m_ServerRules = m_Config.CreateBusinessRules()
                If (m_ServerRules Is Nothing) Then
                    m_ServerRules = CreateBusinessRules()
                End If
                Dim handler As IActionHandler = CType(m_ServerRules,IActionHandler)
                If (Not (m_Config.PlugIn) Is Nothing) Then
                    m_Config.PlugIn.PreProcessArguments(args, result, CreateViewPage())
                End If
                If Not ((args.SqlCommandType = CommandConfigurationType.None)) Then
                    Using connection As DbConnection = CreateConnection()
                        Dim command As DbCommand = CreateCommand(connection, args)
                        If ((Not (args.SelectedValues) Is Nothing) AndAlso (((args.LastCommandName = "BatchEdit") AndAlso (args.CommandName = "Update")) OrElse ((args.CommandName = "Delete") AndAlso (args.SelectedValues.Length > 1)))) Then
                            Dim page As ViewPage = CreateViewPage()
                            PopulatePageFields(page)
                            Dim originalCommandText As String = command.CommandText
                            For Each sv As String in args.SelectedValues
                                result.Canceled = false
                                m_ServerRules.ClearBlackAndWhiteLists()
                                Dim key() As String = sv.Split(Global.Microsoft.VisualBasic.ChrW(44))
                                Dim keyIndex As Integer = 0
                                For Each v As FieldValue in args.Values
                                    Dim field As DataField = page.FindField(v.Name)
                                    If (Not (field) Is Nothing) Then
                                        If Not (field.IsPrimaryKey) Then
                                            v.Modified = true
                                        Else
                                            If (v.Name = field.Name) Then
                                                v.OldValue = key(keyIndex)
                                                v.Modified = false
                                                keyIndex = (keyIndex + 1)
                                            End If
                                        End If
                                    End If
                                Next
                                ExecutePreActionCommands(args, result, connection)
                                If (Not (handler) Is Nothing) Then
                                    handler.BeforeSqlAction(args, result)
                                Else
                                    m_ServerRules.ExecuteServerRules(args, result, ActionPhase.Before)
                                End If
                                If ((result.Errors.Count = 0) AndAlso Not (result.Canceled)) Then
                                    ConfigureCommand(command, Nothing, args.SqlCommandType, args.Values)
                                    result.RowsAffected = (result.RowsAffected + TransactionManager.ExecuteNonQuery(command))
                                    If (Not (handler) Is Nothing) Then
                                        handler.AfterSqlAction(args, result)
                                    Else
                                        m_ServerRules.ExecuteServerRules(args, result, ActionPhase.After)
                                    End If
                                    command.CommandText = originalCommandText
                                    command.Parameters.Clear()
                                    If (Not (m_Config.PlugIn) Is Nothing) Then
                                        m_Config.PlugIn.ProcessArguments(args, result, page)
                                    End If
                                End If
                            Next
                        Else
                            If (args.CommandName = "Delete") Then
                                ProcessManyToManyFields(args)
                            End If
                            ExecutePreActionCommands(args, result, connection)
                            If (Not (handler) Is Nothing) Then
                                handler.BeforeSqlAction(args, result)
                            Else
                                m_ServerRules.ExecuteServerRules(args, result, ActionPhase.Before)
                            End If
                            If ((result.Errors.Count = 0) AndAlso Not (result.Canceled)) Then
                                If ConfigureCommand(command, Nothing, args.SqlCommandType, args.Values) Then
                                    result.RowsAffected = TransactionManager.ExecuteNonQuery(args, result, CreateViewPage(), command)
                                    If (result.RowsAffected = 0) Then
                                        result.RowNotFound = true
                                        result.Errors.Add(Localizer.Replace("RecordChangedByAnotherUser", "The record has been changed by another user."))
                                    Else
                                        ExecutePostActionCommands(args, result, connection)
                                    End If
                                End If
                                If (Not (handler) Is Nothing) Then
                                    handler.AfterSqlAction(args, result)
                                Else
                                    m_ServerRules.ExecuteServerRules(args, result, ActionPhase.After)
                                End If
                                If (Not (m_Config.PlugIn) Is Nothing) Then
                                    m_Config.PlugIn.ProcessArguments(args, result, CreateViewPage())
                                End If
                                If ((args.CommandName = "Insert") OrElse (args.CommandName = "Update")) Then
                                    ProcessManyToManyFields(args)
                                End If
                            End If
                        End If
                    End Using
                Else
                    If args.CommandName.StartsWith("Export") Then
                        ExecuteDataExport(args, result)
                    Else
                        If args.CommandName.Equals("PopulateDynamicLookups") Then
                            PopulateDynamicLookups(args, result)
                        Else
                            If args.CommandName.Equals("ProcessImportFile") Then
                                ImportProcessor.Execute(args)
                            Else
                                If args.CommandName.Equals("Execute") Then
                                    Using connection As DbConnection = CreateConnection()
                                        Dim command As DbCommand = CreateCommand(connection, args)
                                        TransactionManager.ExecuteNonQuery(command)
                                    End Using
                                Else
                                    m_ServerRules.ProcessSpecialActions(args, result)
                                End If
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                If TypeOf ex Is System.Reflection.TargetInvocationException Then
                    ex = ex.InnerException
                End If
                HandleException(ex, args, result)
            End Try
            result.EnsureJsonCompatibility()
            Return result
        End Function
        
        Private Function SupportsLimitInSelect(ByVal command As Object) As Boolean
            Return command.ToString().Contains("MySql")
        End Function
        
        Private Function SupportsSkipInSelect(ByVal command As Object) As Boolean
            Return command.ToString().Contains("Firebird")
        End Function
        
        Protected Overridable Sub SyncRequestedPage(ByVal request As PageRequest, ByVal page As ViewPage, ByVal connection As DbConnection)
            If (((request.SyncKey Is Nothing) OrElse (request.SyncKey.Length = 0)) OrElse (page.PageSize < 0)) Then
                Return
            End If
            Dim keyFields As List(Of DataField) = New List(Of DataField)()
            For Each field As DataField in page.Fields
                If field.IsPrimaryKey Then
                    keyFields.Add(field)
                End If
            Next
            If ((keyFields.Count > 0) AndAlso (keyFields.Count = request.SyncKey.Length)) Then
                Dim syncCommand As DbCommand = CreateCommand(connection)
                ConfigureCommand(syncCommand, page, CommandConfigurationType.Sync, Nothing)
                Dim useSkip As Boolean = (m_ServerRules.EnableResultSet OrElse SupportsSkipInSelect(syncCommand))
                If Not (useSkip) Then
                    Dim i As Integer = 0
                    Do While (i < keyFields.Count)
                        Dim field As DataField = keyFields(i)
                        Dim p As DbParameter = syncCommand.CreateParameter()
                        p.ParameterName = String.Format("{0}PrimaryKey_{1}", m_ParameterMarker, field.Name)
                        Try 
                            AssignParameterValue(p, field.Type, request.SyncKey(i))
                        Catch __exception As Exception
                            Return
                        End Try
                        syncCommand.Parameters.Add(p)
                        i = (i + 1)
                    Loop
                End If
                Dim reader As DbDataReader
                If m_ServerRules.EnableResultSet Then
                    reader = ExecuteResultSetReader(page)
                Else
                    reader = syncCommand.ExecuteReader()
                End If
                If Not (useSkip) Then
                    If reader.Read() Then
                        Dim rowIndex As Long = Convert.ToInt64(reader(0))
                        page.PageIndex = Convert.ToInt32(Math.Floor((Convert.ToDouble((rowIndex - 1)) / Convert.ToDouble(page.PageSize))))
                        page.PageOffset = 0
                    End If
                Else
                    Dim rowIndex As Long = 1
                    Dim keyFieldIndexes As List(Of Integer) = New List(Of Integer)()
                    For Each pkField As DataField in keyFields
                        keyFieldIndexes.Add(reader.GetOrdinal(pkField.Name))
                    Next
                    Do While reader.Read()
                        Dim matchCount As Integer = 0
                        For Each primaryKeyFieldIndex As Integer in keyFieldIndexes
                            If (Convert.ToString(reader(primaryKeyFieldIndex)) = Convert.ToString(request.SyncKey(matchCount))) Then
                                matchCount = (matchCount + 1)
                            Else
                                Exit For
                            End If
                        Next
                        If (matchCount = keyFieldIndexes.Count) Then
                            page.PageIndex = Convert.ToInt32(Math.Floor((Convert.ToDouble((rowIndex - 1)) / Convert.ToDouble(page.PageSize))))
                            page.PageOffset = 0
                            page.ResetSkipCount(false)
                            Exit Do
                        Else
                            rowIndex = (rowIndex + 1)
                        End If
                    Loop
                End If
                reader.Close()
            End If
        End Sub
        
        Protected Overridable Sub HandleException(ByVal ex As Exception, ByVal args As ActionArgs, ByVal result As ActionResult)
            Do While (Not (ex) Is Nothing)
                result.Errors.Add(ex.Message)
                ex = ex.InnerException
            Loop
        End Sub
        
        Function IDataEngine_ExecuteReader(ByVal request As PageRequest) As DbDataReader Implements IDataEngine.ExecuteReader
            m_ViewPage = New ViewPage(request)
            If (m_Config Is Nothing) Then
                m_Config = CreateConfiguration(request.Controller)
                SelectView(request.Controller, request.View)
            End If
            m_ViewPage.ApplyDataFilter(m_Config.CreateDataFilter(), request.Controller, request.View, Nothing, Nothing, Nothing)
            InitBusinessRules(request, m_ViewPage)
            Dim connection As DbConnection = CreateConnection()
            Dim selectCommand As DbCommand = CreateCommand(connection)
            ConfigureCommand(selectCommand, m_ViewPage, CommandConfigurationType.Select, Nothing)
            Return selectCommand.ExecuteReader(CommandBehavior.CloseConnection)
        End Function
        
        Function IAutoCompleteManager_GetCompletionList(ByVal prefixText As String, ByVal count As Integer, ByVal contextKey As String) As String() Implements IAutoCompleteManager.GetCompletionList
            If (contextKey = Nothing) Then
                Return Nothing
            End If
            Dim arguments() As String = contextKey.Split(Global.Microsoft.VisualBasic.ChrW(44))
            If Not ((arguments.Length = 3)) Then
                Return Nothing
            End If
            Dim request As DistinctValueRequest = New DistinctValueRequest()
            request.FieldName = arguments(2)
            Dim filter As String = (request.FieldName + ":")
            For Each s As String in prefixText.Split(Global.Microsoft.VisualBasic.ChrW(44), Global.Microsoft.VisualBasic.ChrW(59))
                Dim query As String = Controller.ConvertSampleToQuery(s)
                If Not (String.IsNullOrEmpty(query)) Then
                    filter = (filter + query)
                End If
            Next
            request.Filter = New String() {filter}
            request.AllowFieldInFilter = true
            request.MaximumValueCount = count
            request.Controller = arguments(0)
            request.View = arguments(1)
            Dim list() As Object = ControllerFactory.CreateDataController().GetListOfValues(arguments(0), arguments(1), request)
            Dim result As List(Of String) = New List(Of String)()
            For Each o As Object in list
                result.Add(Convert.ToString(o))
            Next
            Return result.ToArray()
        End Function
        
        Overridable Sub IBusinessObject_AssignFilter(ByVal filter As String, ByVal parameters As BusinessObjectParameters) Implements IBusinessObject.AssignFilter
            m_ViewFilter = filter
            m_Parameters = parameters
        End Sub
        
        Public Shared Function GetSelectView(ByVal controller As String) As String
            Dim c As ControllerUtilities = New ControllerUtilities()
            Return c.GetActionView(controller, "editForm1", "Select")
        End Function
        
        Public Shared Function GetUpdateView(ByVal controller As String) As String
            Dim c As ControllerUtilities = New ControllerUtilities()
            Return c.GetActionView(controller, "editForm1", "Update")
        End Function
        
        Public Shared Function GetInsertView(ByVal controller As String) As String
            Dim c As ControllerUtilities = New ControllerUtilities()
            Return c.GetActionView(controller, "createForm1", "Insert")
        End Function
        
        Public Shared Function GetDeleteView(ByVal controller As String) As String
            Dim c As ControllerUtilities = New ControllerUtilities()
            Return c.GetActionView(controller, "editForm1", "Delete")
        End Function
        
        Private Sub PopulateManyToManyFields(ByVal page As ViewPage)
            Dim primaryKeyField As String = String.Empty
            For Each field As DataField in page.Fields
                If Not (String.IsNullOrEmpty(field.ItemsTargetController)) Then
                    If String.IsNullOrEmpty(primaryKeyField) Then
                        For Each f As DataField in page.Fields
                            If f.IsPrimaryKey Then
                                primaryKeyField = f.Name
                                Exit For
                            End If
                        Next
                    End If
                    PopulateManyToManyField(page, field, primaryKeyField)
                End If
            Next
        End Sub
        
        Public Sub PopulateManyToManyField(ByVal page As ViewPage, ByVal field As DataField, ByVal primaryKeyField As String)
            If Not ((m_JunctionTableFieldName = field.Name)) Then
                m_JunctionTableFieldName = field.Name
                m_JunctionTableMap = Nothing
            End If
            If (m_JunctionTableMap Is Nothing) Then
                m_JunctionTableMap = New SortedDictionary(Of String, List(Of String))()
                If (page.Rows.Count > 0) Then
                    'read contents of junction table from the database for each row of the page
                    Dim foreignKeyIndex As Integer = page.IndexOfField(primaryKeyField)
                    Dim listOfForeignKeys As StringBuilder = New StringBuilder()
                    For Each row() As Object in page.Rows
                        If (listOfForeignKeys.Length > 0) Then
                            listOfForeignKeys.Append("$or$")
                        End If
                        listOfForeignKeys.Append(DataControllerBase.ConvertObjectToValue(row(foreignKeyIndex)))
                    Next
                    Dim targetForeignKey1 As String
                    Dim targetForeignKey2 As String
                    ViewPage.InitializeManyToManyProperties(field, page.Controller, targetForeignKey1, targetForeignKey2)
                    Dim filter As String = String.Format("{0}:$in${1}", targetForeignKey1, listOfForeignKeys.ToString())
                    Dim request As PageRequest = New PageRequest(0, Int32.MaxValue, Nothing, New String() {filter})
                    request.RequiresMetaData = true
                    Dim manyToManyPage As ViewPage = ControllerFactory.CreateDataController().GetPage(field.ItemsTargetController, Nothing, request)
                    'enumerate values in junction table
                    Dim targetForeignKey1Index As Integer = manyToManyPage.IndexOfField(targetForeignKey1)
                    Dim targetForeignKey2Index As Integer = manyToManyPage.IndexOfField(targetForeignKey2)
                    'determine text field for items
                    Dim items As Dictionary(Of Object, Object) = New Dictionary(Of Object, Object)()
                    Dim targetTextIndex As Integer = -1
                    If Not ((field.ItemsStyle = "CheckBoxList")) Then
                        For Each f As DataField in manyToManyPage.Fields
                            If (f.Name = targetForeignKey2) Then
                                If Not (String.IsNullOrEmpty(f.AliasName)) Then
                                    targetTextIndex = manyToManyPage.IndexOfField(f.AliasName)
                                Else
                                    targetTextIndex = manyToManyPage.IndexOfField(f.Name)
                                End If
                                Exit For
                            End If
                        Next
                    End If
                    For Each row() As Object in manyToManyPage.Rows
                        Dim v1 As Object = row(targetForeignKey1Index)
                        Dim v2 As Object = row(targetForeignKey2Index)
                        If (Not (v1) Is Nothing) Then
                            Dim s1 As String = Convert.ToString(v1)
                            Dim values As List(Of String) = Nothing
                            If Not (m_JunctionTableMap.TryGetValue(s1, values)) Then
                                values = New List(Of String)()
                                m_JunctionTableMap(s1) = values
                            End If
                            values.Add(Convert.ToString(v2))
                            If Not ((targetTextIndex = -1)) Then
                                Dim text As Object = row(targetTextIndex)
                                If Not (items.ContainsKey(v2)) Then
                                    items.Add(v2, text)
                                End If
                            End If
                        End If
                    Next
                    If Not ((items.Count = 0)) Then
                        For Each kvp As KeyValuePair(Of Object, Object) in items
                            field.Items.Add(New Object() {kvp.Key, kvp.Value})
                        Next
                    End If
                End If
            End If
            For Each values() As Object in page.Rows
                Dim key As String = Convert.ToString(page.SelectFieldValue(primaryKeyField, values))
                Dim keyValues As List(Of String) = Nothing
                If m_JunctionTableMap.TryGetValue(key, keyValues) Then
                    page.UpdateFieldValue(field.Name, values, String.Join(",", keyValues.ToArray()))
                End If
            Next
        End Sub
        
        Private Sub ProcessManyToManyFields(ByVal args As ActionArgs)
            Dim m2mFields As XPathNodeIterator = Config.Select("/c:dataController/c:fields/c:field[c:items/@targetController!='']")
            If (m2mFields.Count > 0) Then
                Dim primaryKeyNode As XPathNavigator = Config.SelectSingleNode("/c:dataController/c:fields/c:field[@isPrimaryKey='true']")
                Dim primaryKey As FieldValue = args.SelectFieldValueObject(primaryKeyNode.GetAttribute("name", String.Empty))
                Do While m2mFields.MoveNext()
                    Dim field As DataField = New DataField(m2mFields.Current, Config.Resolver)
                    Dim fv As FieldValue = args.SelectFieldValueObject(field.Name)
                    If (args.CommandName = "Delete") Then
                        fv.NewValue = Nothing
                    End If
                    ProcessManyToManyField(args.Controller, field, fv, primaryKey.Value)
                    fv.Modified = false
                Loop
            End If
        End Sub
        
        Public Sub ProcessManyToManyField(ByVal controllerName As String, ByVal field As DataField, ByVal fieldValue As FieldValue, ByVal primaryKey As Object)
            Dim oldValues As List(Of String) = BusinessRulesBase.ValueToList(CType(fieldValue.OldValue,String))
            Dim newValues As List(Of String) = BusinessRulesBase.ValueToList(CType(fieldValue.NewValue,String))
            If Not (BusinessRulesBase.ListsAreEqual(oldValues, newValues)) Then
                Dim targetForeignKey1 As String
                Dim targetForeignKey2 As String
                ViewPage.InitializeManyToManyProperties(field, controllerName, targetForeignKey1, targetForeignKey2)
                Dim controller As IDataController = ControllerFactory.CreateDataController()
                For Each s As String in oldValues
                    If Not (newValues.Contains(s)) Then
                        Dim deleteArgs As ActionArgs = New ActionArgs()
                        deleteArgs.Controller = field.ItemsTargetController
                        deleteArgs.CommandName = "Delete"
                        deleteArgs.LastCommandName = "Select"
                        deleteArgs.Values = New FieldValue() {New FieldValue(targetForeignKey1, primaryKey, primaryKey), New FieldValue(targetForeignKey2, s, s), New FieldValue("_IgnorePrimaryKeyInWhere", true)}
                        Dim result As ActionResult = controller.Execute(field.ItemsTargetController, Nothing, deleteArgs)
                        result.RaiseExceptionIfErrors()
                    End If
                Next
                For Each s As String in newValues
                    If Not (oldValues.Contains(s)) Then
                        Dim updateArgs As ActionArgs = New ActionArgs()
                        updateArgs.Controller = field.ItemsTargetController
                        updateArgs.CommandName = "Insert"
                        updateArgs.LastCommandName = "New"
                        updateArgs.Values = New FieldValue() {New FieldValue(targetForeignKey1, primaryKey), New FieldValue(targetForeignKey2, s)}
                        Dim result As ActionResult = controller.Execute(field.ItemsTargetController, Nothing, updateArgs)
                        result.RaiseExceptionIfErrors()
                    End If
                Next
            End If
        End Sub
        
        Public Overridable Function GetDataControllerStream(ByVal controller As String) As Stream
            If (Not (ApplicationServices.IsSiteContentEnabled) OrElse (controller = ApplicationServices.SiteContentControllerName)) Then
                Return Nothing
            End If
            Dim context As HttpContext = HttpContext.Current
            Dim requiresUser As Boolean = false
            If (context.User Is Nothing) Then
                requiresUser = true
                'Establish user identity for REST requests
                Dim authorization As String = context.Request.Headers("Authorization")
                If String.IsNullOrEmpty(authorization) Then
                    context.User = New RolePrincipal(New AnonymousUserIdentity())
                Else
                    If authorization.StartsWith("Basic", StringComparison.OrdinalIgnoreCase) Then
                        Dim login() As String = Encoding.Default.GetString(Convert.FromBase64String(authorization.Substring(6))).Split(New Char() {Global.Microsoft.VisualBasic.ChrW(58)}, 2)
                        If Membership.ValidateUser(login(0), login(1)) Then
                            context.User = New RolePrincipal(New FormsIdentity(New FormsAuthenticationTicket(login(0), false, 10)))
                        End If
                    End If
                    If (context.User Is Nothing) Then
                        context.Response.StatusCode = 401
                        context.Response.End()
                    End If
                End If
            End If
            Dim data() As Byte = ApplicationServices.Current.ReadSiteContentBytes(("sys/controllers%/"  _
                            + (controller + ".xml")))
            If requiresUser Then
                context.User = Nothing
            End If
            If (data Is Nothing) Then
                Return Nothing
            Else
                Return New MemoryStream(data)
            End If
        End Function
        
        Protected Overridable Function ExecuteVirtualReader(ByVal request As PageRequest, ByVal page As ViewPage) As DbDataReader
            Dim table As DataTable = New DataTable()
            For Each field As DataField in page.Fields
                table.Columns.Add(field.Name, GetType(Integer))
            Next
            Dim r As DataRow = table.NewRow()
            If page.ContainsField("PrimaryKey") Then
                r("PrimaryKey") = 1
            End If
            table.Rows.Add(r)
            Return New DataTableReader(table)
        End Function
        
        Protected Overridable Function GetRequestedViewType(ByVal page As ViewPage) As String
            Dim viewType As String = page.ViewType
            If String.IsNullOrEmpty(viewType) Then
                viewType = m_View.GetAttribute("type", String.Empty)
            End If
            Return viewType
        End Function
        
        Protected Overridable Sub EnsureSystemPageFields(ByVal request As PageRequest, ByVal page As ViewPage, ByVal command As DbCommand)
            If Not (RequiresHierarchy(page)) Then
                Return
            End If
            Dim requiresHierarchyOrganization As Boolean = false
            For Each field As DataField in page.Fields
                If field.IsTagged("hierarchy-parent") Then
                    requiresHierarchyOrganization = true
                Else
                    If field.IsTagged("hierarchy-organization") Then
                        requiresHierarchyOrganization = false
                        Exit For
                    End If
                End If
            Next
            If requiresHierarchyOrganization Then
                Dim field As DataField = New DataField()
                field.Name = HierarchyOrganizationFieldName
                field.Type = "String"
                field.Tag = "hierarchy-organization"
                field.Len = 255
                field.Columns = 20
                field.Hidden = true
                field.ReadOnly = true
                page.Fields.Add(field)
            End If
        End Sub
        
        Protected Overridable Function RequiresHierarchy(ByVal page As ViewPage) As Boolean
            If Not ((GetRequestedViewType(page) = "DataSheet")) Then
                Return false
            End If
            For Each field As DataField in page.Fields
                If field.IsTagged("hierarchy-parent") Then
                    If ((Not (page.Filter) Is Nothing) AndAlso (page.Filter.Length > 0)) Then
                        Return false
                    End If
                    Return true
                End If
            Next
            Return false
        End Function
        
        Protected Overloads Overridable Function DatabaseEngineIs(ByVal command As DbCommand, ByVal ParamArray flavors() as System.[String]) As Boolean
            Return DatabaseEngineIs(command.GetType().FullName, flavors)
        End Function
        
        Protected Overloads Overridable Function DatabaseEngineIs(ByVal typeName As String, ByVal ParamArray flavors() as System.[String]) As Boolean
            For Each s As String in flavors
                If typeName.Contains(s) Then
                    Return true
                End If
            Next
            Return false
        End Function
        
        Protected Overridable Function ValidateViewAccess(ByVal controller As String, ByVal view As String, ByVal access As String) As Boolean
            Dim context As HttpContext = HttpContext.Current
            If (AllowPublicAccess OrElse (context.Request.Params("_validationKey") = BlobAdapter.ValidationKey)) Then
                Return true
            End If
            If ((controller = ApplicationServices.SiteContentControllerName) AndAlso Not (UserIsInRole(ApplicationServices.SiteContentEditors))) Then
                Return false
            End If
            Dim allow As Boolean = true
            Dim executionFilePath As String = context.Request.AppRelativeCurrentExecutionFilePath
            If ((Not (executionFilePath.StartsWith("~/appservices/", StringComparison.OrdinalIgnoreCase)) AndAlso Not (executionFilePath.Equals("~/blob.ashx", StringComparison.OrdinalIgnoreCase))) AndAlso Not (executionFilePath.Equals("~/charthost.aspx", StringComparison.OrdinalIgnoreCase))) Then
                If (Not (context.User.Identity.IsAuthenticated) AndAlso Not (controller.StartsWith("aspnet_"))) Then
                    allow = (access = "Public")
                End If
            End If
            Return allow
        End Function
        
        Function ExecuteResultSetTable(ByVal page As ViewPage) As DataTable
            If (m_ServerRules.ResultSet Is Nothing) Then
                Return Nothing
            End If
            Dim expressions As SelectClauseDictionary = New SelectClauseDictionary()
            For Each c As DataColumn in m_ServerRules.ResultSet.Columns
                expressions(c.ColumnName) = c.ColumnName
            Next
            If (page.Fields.Count = 0) Then
                PopulatePageFields(page)
                EnsurePageFields(page, Nothing)
            End If
            Dim resultView As DataView = New DataView(m_ServerRules.ResultSet)
            resultView.Sort = page.SortExpression
            Using connection As DbConnection = CreateConnection(false)
                Dim command As DbCommand = connection.CreateCommand()
                Dim sb As StringBuilder = New StringBuilder()
                m_ResultSetParameters = command.Parameters
                expressions.Add("_DataView_RowFilter_", "true")
                AppendFilterExpressionsToWhere(sb, page, command, expressions, String.Empty)
                Dim filter As String = sb.ToString()
                If filter.StartsWith("where") Then
                    filter = filter.Substring(5)
                End If
                filter = Regex.Replace(filter, (Regex.Escape(m_ParameterMarker) + "\w+"), AddressOf DoReplaceResultSetParameter)
                resultView.RowFilter = filter
                If (page.PageSize > 0) Then
                    page.TotalRowCount = resultView.Count
                End If
            End Using
            If RequiresPreFetching(page) Then
                page.ResetSkipCount(true)
            End If
            Return resultView.ToTable()
        End Function
        
        Function ExecuteResultSetReader(ByVal page As ViewPage) As DbDataReader
            If (m_ServerRules.ResultSet Is Nothing) Then
                Return Nothing
            End If
            Return ExecuteResultSetTable(page).CreateDataReader()
        End Function
        
        Protected Overridable Function DoReplaceResultSetParameter(ByVal m As Match) As String
            Dim p As DbParameter = m_ResultSetParameters(m.Value)
            Return String.Format("'{0}'", p.Value.ToString().Replace("'", "''"))
        End Function
        
        Function RequiresPreFetching(ByVal page As ViewPage) As Boolean
            Dim viewType As String = page.ViewType
            If String.IsNullOrEmpty(viewType) Then
                viewType = m_View.GetAttribute("type", String.Empty)
            End If
            Return (Not ((page.PageSize = Int32.MaxValue)) AndAlso New ControllerUtilities().SupportsCaching(page, viewType))
        End Function
        
        Public Delegate Function SpecialConversionFunction(ByVal o As Object) As Object
    End Class
    
    Partial Public Class ControllerUtilities
        Inherits ControllerUtilitiesBase
    End Class
    
    Public Class ControllerUtilitiesBase
        
        Public Overridable ReadOnly Property SupportsScrollingInDataSheet() As Boolean
            Get
                Return false
            End Get
        End Property
        
        Public Overridable Function GetActionView(ByVal controller As String, ByVal view As String, ByVal action As String) As String
            Return view
        End Function
        
        Public Overridable Function UserIsInRole(ByVal ParamArray roles() as System.[String]) As Boolean
            Dim context As HttpContext = HttpContext.Current
            If (context Is Nothing) Then
                Return true
            End If
            Dim count As Integer = 0
            For Each r As String in roles
                If Not (String.IsNullOrEmpty(r)) Then
                    For Each role As String in r.Split(Global.Microsoft.VisualBasic.ChrW(44))
                        Dim testRole As String = role.Trim()
                        If Not (String.IsNullOrEmpty(testRole)) Then
                            Dim roleKey As String = ("IsInRole_" + testRole)
                            Dim isInRole As Object = context.Items(roleKey)
                            If (isInRole Is Nothing) Then
                                isInRole = context.User.IsInRole(testRole)
                                context.Items(roleKey) = isInRole
                            End If
                            If CType(isInRole,Boolean) Then
                                Return true
                            End If
                        End If
                        count = (count + 1)
                    Next
                End If
            Next
            Return (count = 0)
        End Function
        
        Public Overridable Function SupportsLastEnteredValues(ByVal controller As String) As Boolean
            Return false
        End Function
        
        Public Overridable Function SupportsCaching(ByVal page As ViewPage, ByVal viewType As String) As Boolean
            If (viewType = "DataSheet") Then
                If (Not (SupportsScrollingInDataSheet) AndAlso Not (ApplicationServices.IsTouchClient)) Then
                    page.SupportsCaching = false
                End If
            Else
                If (viewType = "Grid") Then
                    If Not (ApplicationServices.IsTouchClient) Then
                        page.SupportsCaching = false
                    End If
                Else
                    page.SupportsCaching = false
                End If
            End If
            Return page.SupportsCaching
        End Function
    End Class
    
    Public Class ControllerFactory
        
        Public Shared Function CreateDataController() As IDataController
            Return New Controller()
        End Function
        
        Public Shared Function CreateAutoCompleteManager() As IAutoCompleteManager
            Return New Controller()
        End Function
        
        Public Shared Function CreateDataEngine() As IDataEngine
            Return New Controller()
        End Function
        
        Public Shared Function GetDataControllerStream(ByVal controller As String) As Stream
            Return New Controller().GetDataControllerStream(controller)
        End Function
    End Class
    
    Partial Public Class StringEncryptor
        Inherits StringEncryptorBase
    End Class
    
    Public Class StringEncryptorBase
        
        Public Overridable ReadOnly Property Key() As Byte()
            Get
                Return New Byte() {253, 124, 8, 201, 31, 27, 89, 189, 251, 47, 198, 241, 38, 78, 198, 193, 18, 179, 209, 220, 34, 84, 178, 99, 193, 84, 64, 15, 188, 98, 101, 153}
            End Get
        End Property
        
        Public Overridable ReadOnly Property IV() As Byte()
            Get
                Return New Byte() {87, 84, 163, 98, 205, 255, 139, 173, 16, 88, 88, 254, 133, 176, 55, 112}
            End Get
        End Property
        
        Public Overridable Function Encrypt(ByVal s As String) As String
            Dim plainText() As Byte = Encoding.Default.GetBytes(String.Format("{0}$${1}", s, s.GetHashCode()))
            Dim cipherText() As Byte
            Using output As MemoryStream = New MemoryStream()
                Using cOutput As Stream = New CryptoStream(output, Aes.Create().CreateEncryptor(Key, IV), CryptoStreamMode.Write)
                    cOutput.Write(plainText, 0, plainText.Length)
                End Using
                cipherText = output.ToArray()
            End Using
            Return Convert.ToBase64String(cipherText)
        End Function
        
        Public Overridable Function Decrypt(ByVal s As String) As String
            Dim cipherText() As Byte = Convert.FromBase64String(s)
            Dim plainText() As Byte
            Using output As MemoryStream = New MemoryStream()
                Using cOutput As Stream = New CryptoStream(output, Aes.Create().CreateDecryptor(Key, IV), CryptoStreamMode.Write)
                    cOutput.Write(cipherText, 0, cipherText.Length)
                End Using
                plainText = output.ToArray()
            End Using
            Dim plain As String = Encoding.Default.GetString(plainText)
            Dim parts() As String = plain.Split(New String() {"$$"}, StringSplitOptions.None)
            If (Not ((parts.Length = 2)) OrElse Not ((parts(0).GetHashCode() = Convert.ToInt32(parts(1))))) Then
                Throw New Exception("Attempt to alter the hashed URL.")
            End If
            Return parts(0)
        End Function
    End Class
End Namespace

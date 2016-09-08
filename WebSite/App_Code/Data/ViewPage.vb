Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Web.Caching
Imports System.Web.Configuration
Imports System.Web.Security
Imports System.Xml
Imports System.Xml.XPath

Namespace MyCompany.Data
    
    Public Class ViewPage
        
        Private m_SkipCount As Integer
        
        Private m_ReadCount As Integer
        
        Private m_OriginalFilter() As String
        
        Private m_Controller As String
        
        Private m_View As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_ViewType As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_PageIndex As Integer
        
        Private m_PageSize As Integer
        
        Private m_PageOffset As Integer
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Tag As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_RequiresMetaData As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_FieldFilter() As String
        
        Private m_MetadataFilter() As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_RequiresSiteContentText As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_RequiresPivot As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_PivotDefinitions As Dictionary(Of String, String)
        
        Private m_Pivots As SortedDictionary(Of Integer, PivotTable) = New SortedDictionary(Of Integer, PivotTable)()
        
        Private Shared m_TargetDataPoints As Integer = 25
        
        Private m_RequiresRowCount As Boolean
        
        Private m_DisableJSONCompatibility As Boolean
        
        Private m_Aggregates() As Object
        
        Private m_Fields As List(Of DataField)
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_SortExpression As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_GroupExpression As String
        
        Private m_TotalRowCount As Integer
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_ClientScript As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_FirstLetters As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Filter() As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_SystemFilter() As String
        
        Private m_DistinctValueFieldName As String
        
        Private m_Views As List(Of View)
        
        Private m_Expressions() As DynamicExpression
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_SupportsCaching As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_LastView As String
        
        Private m_InTransaction As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_StatusBar As String
        
        Private m_AllowDistinctFieldInFilter As Boolean
        
        Private m_Icons() As String
        
        Private m_Levs() As FieldValue
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_QuickFindHint As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_InnerJoinPrimaryKey As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_InnerJoinForeignKey As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_ViewHeaderText As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_ViewLayout As String
        
        Private m_ActionGroups As List(Of ActionGroup)
        
        Private m_Categories As List(Of Category)
        
        Private m_NewRow() As Object
        
        Private m_Rows As List(Of Object())
        
        <ThreadStatic()>  _
        Public Shared PopulatingStaticItems As Boolean
        
        Private m_CustomFilter As SortedDictionary(Of String, Object)
        
        Private Shared m_StringNumSplitter As Regex = New Regex("(?'PropertyName'.+?)(?'PropertyValue'\d+)?$")
        
        Private Shared m_SupportedChartTypes() As String = New String() {"annotation", "area", "areastacked", "bar", "barstacked", "bubble", "candlestick", "combo", "column", "columnstacked", "diff", "gauge", "geo", "histogram", "interval", "line", "map", "org", "pie", "pie3d", "donut", "sankey", "scatter", "steppedarea", "table", "timeline", "treemap", "trendline", "wordtree"}
        
        Private Shared m_SupportedBuckets() As String = New String() {"timeofday", "second", "minute", "halfhour", "hour", "day", "dayofweek", "dayofyear", "weekofmonth", "week", "weekofyear", "twoweek", "twoweeks", "month", "quarter", "year"}
        
        Private Shared m_SupportedModes() As String = New String() {"sum", "min", "max", "avg", "count"}
        
        Private m_PivotTagRegex As Regex = New Regex("(?'tag'pivot[\w-]+(:("".+?""|'.+?'|\S))?)")
        
        Public Sub New()
            Me.New(New PageRequest(0, 0, Nothing, Nothing))
        End Sub
        
        Public Sub New(ByVal request As DistinctValueRequest)
            Me.New(New PageRequest(0, 0, Nothing, request.Filter))
            m_Tag = request.Tag
            m_DistinctValueFieldName = request.FieldName
            m_PageSize = request.MaximumValueCount
            m_AllowDistinctFieldInFilter = request.AllowFieldInFilter
            m_Controller = request.Controller
            m_View = request.View
            m_Filter = request.Filter
            m_QuickFindHint = request.QuickFindHint
        End Sub
        
        Public Sub New(ByVal request As PageRequest)
            MyBase.New
            m_Tag = request.Tag
            Me.PageOffset = request.PageOffset
            m_RequiresMetaData = ((request.PageIndex = -1) OrElse request.RequiresMetaData)
            m_RequiresRowCount = ((request.PageIndex < 0) OrElse request.RequiresRowCount)
            If (request.PageIndex = -2) Then
                request.PageIndex = 0
            End If
            m_PageSize = request.PageSize
            If request.RequiresPivot Then
                RequiresPivot = true
                m_RequiresMetaData = false
                m_RequiresRowCount = false
                m_PageSize = Int32.MaxValue
                PivotDefinitions = New Dictionary(Of String, String)()
                If Not (String.IsNullOrEmpty(request.PivotDefinitions)) Then
                    For Each definition As String in request.PivotDefinitions.Split(Global.Microsoft.VisualBasic.ChrW(59))
                        Dim def() As String = definition.Split(Global.Microsoft.VisualBasic.ChrW(61))
                        If (def.Length = 2) Then
                            PivotDefinitions.Add(def(0), def(1).Replace(Global.Microsoft.VisualBasic.ChrW(44), Global.Microsoft.VisualBasic.ChrW(32)))
                        End If
                    Next
                End If
            End If
            If (request.PageIndex > 0) Then
                m_PageIndex = request.PageIndex
            End If
            m_Rows = New List(Of Object())()
            m_Fields = New List(Of DataField)()
            ResetSkipCount(false)
            m_ReadCount = request.PageSize
            m_SortExpression = request.SortExpression
            m_GroupExpression = request.GroupExpression
            m_Filter = request.Filter
            m_SystemFilter = request.SystemFilter
            m_TotalRowCount = -1
            m_Views = New List(Of View)()
            m_ActionGroups = New List(Of ActionGroup)()
            m_Categories = New List(Of Category)()
            m_Controller = request.Controller
            m_View = request.View
            m_InTransaction = Not (String.IsNullOrEmpty(request.Transaction))
            m_LastView = request.LastView
            m_ViewType = request.ViewType
            m_SupportsCaching = request.SupportsCaching
            m_QuickFindHint = request.QuickFindHint
            m_InnerJoinPrimaryKey = request.InnerJoinPrimaryKey
            m_InnerJoinForeignKey = request.InnerJoinForeignKey
            m_RequiresSiteContentText = request.RequiresSiteContentText
            m_DisableJSONCompatibility = request.DisableJSONCompatibility
            m_FieldFilter = request.FieldFilter
            m_MetadataFilter = request.MetadataFilter
        End Sub
        
        Public ReadOnly Property Controller() As String
            Get
                Return m_Controller
            End Get
        End Property
        
        Public ReadOnly Property View() As String
            Get
                Return m_View
            End Get
        End Property
        
        Public Property ViewType() As String
            Get
                Return Me.m_ViewType
            End Get
            Set
                Me.m_ViewType = value
            End Set
        End Property
        
        Public Property PageIndex() As Integer
            Get
                Return Me.m_PageIndex
            End Get
            Set
                Me.m_PageIndex = value
            End Set
        End Property
        
        Public ReadOnly Property PageSize() As Integer
            Get
                Return m_PageSize
            End Get
        End Property
        
        Public Property PageOffset() As Integer
            Get
                Return m_PageOffset
            End Get
            Set
                m_PageOffset = value
            End Set
        End Property
        
        Public Property Tag() As String
            Get
                Return Me.m_Tag
            End Get
            Set
                Me.m_Tag = value
            End Set
        End Property
        
        Public Property RequiresMetaData() As Boolean
            Get
                Return Me.m_RequiresMetaData
            End Get
            Set
                Me.m_RequiresMetaData = value
            End Set
        End Property
        
        Public Property FieldFilter() As String()
            Get
                Return Me.m_FieldFilter
            End Get
            Set
                Me.m_FieldFilter = value
            End Set
        End Property
        
        Public Overridable Property RequiresSiteContentText() As Boolean
            Get
                Return Me.m_RequiresSiteContentText
            End Get
            Set
                Me.m_RequiresSiteContentText = value
            End Set
        End Property
        
        Public Overridable Property RequiresPivot() As Boolean
            Get
                Return Me.m_RequiresPivot
            End Get
            Set
                Me.m_RequiresPivot = value
            End Set
        End Property
        
        Public Overridable Property PivotDefinitions() As Dictionary(Of String, String)
            Get
                Return Me.m_PivotDefinitions
            End Get
            Set
                Me.m_PivotDefinitions = value
            End Set
        End Property
        
        Public Overridable ReadOnly Property Pivots() As PivotTable()
            Get
                'Eliminate auto pivots
                Dim groups As Dictionary(Of Integer, List(Of PivotTable)) = New Dictionary(Of Integer, List(Of PivotTable))()
                For Each pivot As PivotTable in m_Pivots.Values
                    If (pivot.Group <> 0) Then
                        If Not (groups.ContainsKey(pivot.Group)) Then
                            groups(pivot.Group) = New List(Of PivotTable)()
                        End If
                        groups(pivot.Group).Add(pivot)
                    End If
                Next
                For Each groupKvp As KeyValuePair(Of Integer, List(Of PivotTable)) in groups
                    Dim candidate As PivotTable = Nothing
                    For Each p As PivotTable in groupKvp.Value
                        If (candidate Is Nothing) Then
                            candidate = p
                        Else
                            Dim delta As Integer = Math.Abs((m_TargetDataPoints - p.Rows.Count))
                            Dim candidateDelta As Integer = Math.Abs((m_TargetDataPoints - candidate.Rows.Count))
                            If (delta < candidateDelta) Then
                                candidate = p
                            End If
                        End If
                        m_Pivots.Remove(p.Id)
                    Next
                    Dim original As PivotTable = m_Pivots(groupKvp.Key)
                    candidate.Id = original.Id
                    candidate.Name = original.Name
                    candidate.ChartType = original.ChartType
                    m_Pivots(groupKvp.Key) = candidate
                Next
                Return m_Pivots.Values.ToArray()
            End Get
        End Property
        
        Public ReadOnly Property RequiresRowCount() As Boolean
            Get
                Return m_RequiresRowCount
            End Get
        End Property
        
        Public ReadOnly Property RequiresAggregates() As Boolean
            Get
                For Each field As DataField in Fields
                    If Not ((field.Aggregate = DataFieldAggregate.None)) Then
                        Return true
                    End If
                Next
                Return false
            End Get
        End Property
        
        Public Property Aggregates() As Object()
            Get
                Return m_Aggregates
            End Get
            Set
                m_Aggregates = value
            End Set
        End Property
        
        Public ReadOnly Property Fields() As List(Of DataField)
            Get
                Return m_Fields
            End Get
        End Property
        
        Public Property SortExpression() As String
            Get
                Return Me.m_SortExpression
            End Get
            Set
                Me.m_SortExpression = value
            End Set
        End Property
        
        Public Property GroupExpression() As String
            Get
                Return Me.m_GroupExpression
            End Get
            Set
                Me.m_GroupExpression = value
            End Set
        End Property
        
        Public Property TotalRowCount() As Integer
            Get
                Return m_TotalRowCount
            End Get
            Set
                m_TotalRowCount = value
                Dim pageCount As Integer = (value / Me.PageSize)
                If ((value Mod Me.PageSize) > 0) Then
                    pageCount = (pageCount + 1)
                End If
                If (pageCount <= PageIndex) Then
                    Me.m_PageIndex = 0
                End If
            End Set
        End Property
        
        Public Property ClientScript() As String
            Get
                Return Me.m_ClientScript
            End Get
            Set
                Me.m_ClientScript = value
            End Set
        End Property
        
        Public Property FirstLetters() As String
            Get
                Return Me.m_FirstLetters
            End Get
            Set
                Me.m_FirstLetters = value
            End Set
        End Property
        
        Public Property Filter() As String()
            Get
                Return Me.m_Filter
            End Get
            Set
                Me.m_Filter = value
            End Set
        End Property
        
        Public Property SystemFilter() As String()
            Get
                Return Me.m_SystemFilter
            End Get
            Set
                Me.m_SystemFilter = value
            End Set
        End Property
        
        Public ReadOnly Property DistinctValueFieldName() As String
            Get
                Return m_DistinctValueFieldName
            End Get
        End Property
        
        Public ReadOnly Property Views() As List(Of View)
            Get
                Return m_Views
            End Get
        End Property
        
        Public Property Expressions() As DynamicExpression()
            Get
                Return m_Expressions
            End Get
            Set
                m_Expressions = value
            End Set
        End Property
        
        Public Property SupportsCaching() As Boolean
            Get
                Return Me.m_SupportsCaching
            End Get
            Set
                Me.m_SupportsCaching = value
            End Set
        End Property
        
        Public Property LastView() As String
            Get
                Return Me.m_LastView
            End Get
            Set
                Me.m_LastView = value
            End Set
        End Property
        
        Public ReadOnly Property InTransaction() As Boolean
            Get
                Return m_InTransaction
            End Get
        End Property
        
        Public Property StatusBar() As String
            Get
                Return Me.m_StatusBar
            End Get
            Set
                Me.m_StatusBar = value
            End Set
        End Property
        
        Public ReadOnly Property AllowDistinctFieldInFilter() As Boolean
            Get
                Return m_AllowDistinctFieldInFilter
            End Get
        End Property
        
        Public Property Icons() As String()
            Get
                Return m_Icons
            End Get
            Set
                m_Icons = value
            End Set
        End Property
        
        Public ReadOnly Property IsAuthenticated() As Boolean
            Get
                Return HttpContext.Current.User.Identity.IsAuthenticated
            End Get
        End Property
        
        Public Property LEVs() As FieldValue()
            Get
                Return m_Levs
            End Get
            Set
                m_Levs = value
            End Set
        End Property
        
        Public Property QuickFindHint() As String
            Get
                Return Me.m_QuickFindHint
            End Get
            Set
                Me.m_QuickFindHint = value
            End Set
        End Property
        
        Public Property InnerJoinPrimaryKey() As String
            Get
                Return Me.m_InnerJoinPrimaryKey
            End Get
            Set
                Me.m_InnerJoinPrimaryKey = value
            End Set
        End Property
        
        Public Property InnerJoinForeignKey() As String
            Get
                Return Me.m_InnerJoinForeignKey
            End Get
            Set
                Me.m_InnerJoinForeignKey = value
            End Set
        End Property
        
        Public Property ViewHeaderText() As String
            Get
                Return Me.m_ViewHeaderText
            End Get
            Set
                Me.m_ViewHeaderText = value
            End Set
        End Property
        
        Public Property ViewLayout() As String
            Get
                Return Me.m_ViewLayout
            End Get
            Set
                Me.m_ViewLayout = value
            End Set
        End Property
        
        Public ReadOnly Property ActionGroups() As List(Of ActionGroup)
            Get
                Return m_ActionGroups
            End Get
        End Property
        
        Public ReadOnly Property Categories() As List(Of Category)
            Get
                Return m_Categories
            End Get
        End Property
        
        Public Property NewRow() As Object()
            Get
                Return m_NewRow
            End Get
            Set
                m_NewRow = value
            End Set
        End Property
        
        Public ReadOnly Property Rows() As List(Of Object())
            Get
                Return m_Rows
            End Get
        End Property
        
        Public Function IncludeField(ByVal name As String) As Boolean
            Return ((m_FieldFilter Is Nothing) OrElse m_FieldFilter.Contains(name))
        End Function
        
        Public Function IncludeMetadata(ByVal name As String) As Boolean
            Return ((m_MetadataFilter Is Nothing) OrElse m_MetadataFilter.Contains(name))
        End Function
        
        Public Sub ChangeFilter(ByVal filter() As String)
            m_Filter = filter
            m_OriginalFilter = Nothing
        End Sub
        
        Public Function SkipNext() As Boolean
            If (m_SkipCount = 0) Then
                Return false
            End If
            m_SkipCount = (m_SkipCount - 1)
            Return true
        End Function
        
        Public Sub ResetSkipCount(ByVal preFetch As Boolean)
            If preFetch Then
                m_SkipCount = ((m_PageIndex - 1)  _
                            * m_PageSize)
                m_ReadCount = (m_ReadCount * 3)
                If (m_SkipCount < 0) Then
                    m_SkipCount = 0
                    m_ReadCount = (m_ReadCount - m_PageSize)
                End If
            Else
                m_SkipCount = (m_PageIndex * m_PageSize)
            End If
        End Sub
        
        Public Function ReadNext() As Boolean
            If (m_ReadCount = 0) Then
                Return false
            End If
            m_ReadCount = (m_ReadCount - 1)
            Return true
        End Function
        
        Public Sub AcceptAllRows()
            m_ReadCount = Int32.MaxValue
            m_SkipCount = 0
        End Sub
        
        Public Function ContainsField(ByVal name As String) As Boolean
            Return (Not (FindField(name)) Is Nothing)
        End Function
        
        Public Function FindField(ByVal name As String) As DataField
            For Each field As DataField in Fields
                If field.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) Then
                    Return field
                End If
            Next
            Return Nothing
        End Function
        
        Public Function IndexOfField(ByVal name As String) As Integer
            Dim i As Integer = 0
            Do While (i < Fields.Count)
                Dim field As DataField = Fields(i)
                If field.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) Then
                    Return i
                End If
                i = (i + 1)
            Loop
            Return -1
        End Function
        
        Public Function PopulateStaticItems(ByVal field As DataField, ByVal contextValues() As FieldValue) As Boolean
            If Not (IncludeMetadata("items")) Then
                Return false
            End If
            If ((field.ItemsStyle = "CheckBoxList") AndAlso (Not (String.IsNullOrEmpty(field.ItemsTargetController)) AndAlso String.IsNullOrEmpty(field.ItemsDataValueField))) Then
                Dim dummy1 As String
                Dim dummy2 As String
                InitializeManyToManyProperties(field, Controller, dummy1, dummy2)
            End If
            If (field.SupportsStaticItems() AndAlso (String.IsNullOrEmpty(field.ContextFields) OrElse ((Not (contextValues) Is Nothing) OrElse (field.ItemsStyle = "CheckBoxList")))) Then
                If PopulatingStaticItems Then
                    Return true
                End If
                PopulatingStaticItems = true
                Try 
                    Dim filter() As String = Nothing
                    If Not (String.IsNullOrEmpty(field.ContextFields)) Then
                        Dim contextFilter As List(Of String) = New List(Of String)()
                        Dim m As Match = Regex.Match(field.ContextFields, "(\w+)\s*=\s*(.+?)($|,)")
                        Dim staticContextValues As SortedDictionary(Of String, List(Of String)) = New SortedDictionary(Of String, List(Of String))()
                        Do While m.Success
                            Dim vm As Match = Regex.Match(m.Groups(2).Value, "^(\'(?'Value'.+?)\'|(?'Value'\d+))$")
                            If vm.Success Then
                                Dim lov As List(Of String) = Nothing
                                If Not (staticContextValues.TryGetValue(m.Groups(1).Value, lov)) Then
                                    lov = New List(Of String)()
                                    staticContextValues(m.Groups(1).Value) = lov
                                End If
                                lov.Add(vm.Groups("Value").Value)
                            Else
                                If (Not (contextValues) Is Nothing) Then
                                    For Each cv As FieldValue in contextValues
                                        If (cv.Name = m.Groups(2).Value) Then
                                            If (cv.NewValue Is Nothing) Then
                                                Return true
                                            End If
                                            contextFilter.Add(String.Format("{0}:={1}", m.Groups(1).Value, cv.NewValue))
                                            Exit For
                                        End If
                                    Next
                                End If
                            End If
                            m = m.NextMatch()
                        Loop
                        For Each fieldName As String in staticContextValues.Keys
                            Dim lov As List(Of String) = staticContextValues(fieldName)
                            If (lov.Count = 1) Then
                                contextFilter.Add(String.Format("{0}:={1}", fieldName, lov(0)))
                            Else
                                contextFilter.Add(String.Format("{0}:$in${1}", fieldName, String.Join("$or$", lov.ToArray())))
                            End If
                        Next
                        filter = contextFilter.ToArray()
                    End If
                    Dim request As PageRequest = New PageRequest(-1, 1000, field.ItemsDataTextField, filter)
                    If (Not (ActionArgs.Current) Is Nothing) Then
                        request.ExternalFilter = ActionArgs.Current.ExternalFilter
                    End If
                    request.MetadataFilter = New String() {"fields"}
                    Dim page As ViewPage = ControllerFactory.CreateDataController().GetPage(field.ItemsDataController, field.ItemsDataView, request)
                    Dim dataValueFieldIndex As Integer = page.Fields.IndexOf(page.FindField(field.ItemsDataValueField))
                    If (dataValueFieldIndex = -1) Then
                        For Each aField As DataField in page.Fields
                            If aField.IsPrimaryKey Then
                                dataValueFieldIndex = page.Fields.IndexOf(aField)
                                Exit For
                            End If
                        Next
                    End If
                    Dim dataTextFieldIndex As Integer = page.Fields.IndexOf(page.FindField(field.ItemsDataTextField))
                    If (dataTextFieldIndex = -1) Then
                        Dim i As Integer = 0
                        Do While ((dataTextFieldIndex = -1) AndAlso (i < page.Fields.Count))
                            Dim f As DataField = page.Fields(i)
                            If (Not (f.Hidden) AndAlso (f.Type = "String")) Then
                                dataTextFieldIndex = i
                            End If
                            i = (i + 1)
                        Loop
                        If (dataTextFieldIndex = -1) Then
                            dataTextFieldIndex = 0
                        End If
                    End If
                    Dim fieldIndexes As List(Of Integer) = New List(Of Integer)()
                    fieldIndexes.Add(dataValueFieldIndex)
                    fieldIndexes.Add(dataTextFieldIndex)
                    If Not (String.IsNullOrEmpty(field.Copy)) Then
                        Dim m As Match = Regex.Match(field.Copy, "(\w+)\s*=\s*(\w+)")
                        Do While m.Success
                            Dim copyFieldIndex As Integer = page.Fields.IndexOf(page.FindField(m.Groups(2).Value))
                            fieldIndexes.Add(copyFieldIndex)
                            m = m.NextMatch()
                        Loop
                    End If
                    For Each row() As Object in page.Rows
                        Dim values((fieldIndexes.Count) - 1) As Object
                        Dim i As Integer = 0
                        Do While (i < fieldIndexes.Count)
                            Dim copyFieldIndex As Integer = fieldIndexes(i)
                            If (copyFieldIndex >= 0) Then
                                values(i) = row(copyFieldIndex)
                            Else
                                values(i) = Nothing
                            End If
                            i = (i + 1)
                        Loop
                        field.Items.Add(values)
                    Next
                    Return true
                Finally
                    PopulatingStaticItems = false
                End Try
            End If
            Return false
        End Function
        
        Public Function ToResult(ByVal configuration As ControllerConfiguration, ByVal mainView As XPathNavigator) As ViewPage
            If Not (m_RequiresMetaData) Then
                Fields.Clear()
                Expressions = Nothing
            Else
                If IncludeMetadata("views") Then
                    Dim viewIterator As XPathNodeIterator = configuration.Select("/c:dataController/c:views/c:view[not(@virtualViewId!='')]")
                    Do While viewIterator.MoveNext()
                        Dim v As View = New View(viewIterator.Current, mainView, configuration.Resolver)
                        If (v.Id = Me.View) Then
                            ViewHeaderText = v.HeaderText()
                        End If
                        Views.Add(v)
                    Loop
                End If
                If IncludeMetadata("layouts") Then
                    'load the view layout
                    Dim fileName As String = String.Format("{0}.{1}.html", Me.Controller, Me.View)
                    Dim tryLoad As Boolean = true
                    Do While tryLoad
                        fileName = Path.Combine(Path.Combine(HttpRuntime.AppDomainAppPath, "Views"), fileName)
                        If File.Exists(fileName) Then
                            ViewLayout = File.ReadAllText(fileName)
                        Else
                            Dim stream As Stream = [GetType]().Assembly.GetManifestResourceStream(String.Format("MyCompany.Views.{0}.{1}.html", Me.Controller, Me.View))
                            If (Not (stream) Is Nothing) Then
                                Using sr As StreamReader = New StreamReader(stream)
                                    ViewLayout = sr.ReadToEnd()
                                End Using
                            End If
                        End If
                        If ((Not (ViewLayout) Is Nothing) AndAlso Regex.IsMatch(ViewLayout, "^\s*w+\.\w\.html\s*$", RegexOptions.IgnoreCase)) Then
                            fileName = ViewLayout
                        Else
                            tryLoad = false
                        End If
                    Loop
                End If
                If IncludeMetadata("actions") Then
                    Dim actionGroupIterator As XPathNodeIterator = configuration.Select("/c:dataController/c:actions//c:actionGroup")
                    Do While actionGroupIterator.MoveNext()
                        ActionGroups.Add(New ActionGroup(actionGroupIterator.Current, configuration.Resolver))
                    Loop
                End If
                If IncludeMetadata("items") Then
                    Dim contextValues() As FieldValue = Nothing
                    Dim row() As Object = NewRow
                    If ((row Is Nothing) AndAlso (Rows.Count >= 1)) Then
                        row = Rows(0)
                    End If
                    If (Not (row) Is Nothing) Then
                        Dim valueList As List(Of FieldValue) = New List(Of FieldValue)()
                        Dim i As Integer = 0
                        For Each field As DataField in Fields
                            valueList.Add(New FieldValue(field.Name, row(i)))
                            i = (i + 1)
                        Next
                        contextValues = valueList.ToArray()
                    End If
                    For Each field As DataField in Fields
                        PopulateStaticItems(field, contextValues)
                    Next
                End If
            End If
            If (Not (m_OriginalFilter) Is Nothing) Then
                m_Filter = m_OriginalFilter
            End If
            If (Not (m_Filter) Is Nothing) Then
                Dim i As Integer = 0
                Do While (i < m_Filter.Length)
                    Dim f As String = m_Filter(i)
                    If (f.StartsWith("_match_") OrElse f.StartsWith("_donotmatch_")) Then
                        Dim oldFilter() As String = m_Filter
                        m_Filter = New String((i) - 1) {}
                        Array.Copy(oldFilter, m_Filter, i)
                        Exit Do
                    End If
                    i = (i + 1)
                Loop
            End If
            If New ControllerUtilities().SupportsLastEnteredValues(Me.Controller) Then
                If (RequiresMetaData AndAlso ((Not (HttpContext.Current) Is Nothing) AndAlso (Not (HttpContext.Current.Session) Is Nothing))) Then
                    LEVs = CType(HttpContext.Current.Session(String.Format("{0}$LEVs", m_Controller)),FieldValue())
                End If
            End If
            If Not (m_DisableJSONCompatibility) Then
                DataControllerBase.EnsureJsonCompatibility(NewRow)
                DataControllerBase.EnsureJsonCompatibility(Rows)
            End If
            If Not (IncludeMetadata("fields")) Then
                Fields.Clear()
            Else
                If ((Not (m_FieldFilter) Is Nothing) AndAlso (m_FieldFilter.Length > 0)) Then
                    Dim newFields As List(Of DataField) = New List(Of DataField)()
                    Dim newFieldMap As List(Of Integer) = New List(Of Integer)()
                    Dim index As Integer = 0
                    For Each f As DataField in Fields
                        If (f.IsPrimaryKey OrElse IncludeField(f.Name)) Then
                            newFields.Add(f)
                            newFieldMap.Add(index)
                            index = (index + 1)
                        End If
                    Next
                    If Not ((newFields.Count = Fields.Count)) Then
                        Dim newRows As List(Of Object()) = New List(Of Object())()
                        For Each row() As Object in Rows
                            Dim r((newFields.Count) - 1) As Object
                            index = 0
                            For Each oldIndex As Integer in newFieldMap
                                r(index) = row(oldIndex)
                            Next
                            newRows.Add(r)
                        Next
                    End If
                End If
            End If
            Return Me
        End Function
        
        Public Overloads Function ToDataTable() As DataTable
            Return ToDataTable("table")
        End Function
        
        Public Overloads Function ToDataTable(ByVal tableName As String) As DataTable
            Dim table As DataTable = New DataTable(tableName)
            Dim columnTypes As List(Of Type) = New List(Of Type)()
            For Each field As DataField in Fields
                Dim t As System.Type = GetType(String)
                If Not (((field.Type = "Byte[]") OrElse (field.Type = "DataView"))) Then
                    t = DataControllerBase.TypeMap(field.Type)
                End If
                table.Columns.Add(field.Name, t)
                columnTypes.Add(t)
            Next
            For Each row() As Object in Rows
                Dim newRow As DataRow = table.NewRow()
                Dim i As Integer = 0
                Do While (i < Fields.Count)
                    Dim v As Object = row(i)
                    If (v Is Nothing) Then
                        v = DBNull.Value
                    Else
                        Dim t As Type = columnTypes(i)
                        If ((t Is GetType(DateTime)) AndAlso TypeOf v Is String) Then
                            v = DateTime.Parse(CType(v,String)).ToUniversalTime()
                        Else
                            If ((t Is GetType(DateTimeOffset)) AndAlso TypeOf v Is String) Then
                                Dim dto As DateTimeOffset
                                If DateTimeOffset.TryParse(CType(v,String), dto) Then
                                    v = dto
                                Else
                                    v = DBNull.Value
                                End If
                            End If
                        End If
                    End If
                    newRow(i) = v
                    i = (i + 1)
                Loop
                table.Rows.Add(newRow)
            Next
            table.AcceptChanges()
            Return table
        End Function
        
        Public Function ToList(Of T)() As List(Of T)
            Dim objectType As Type = GetType(T)
            Dim list As List(Of T) = New List(Of T)()
            Dim args((1) - 1) As Object
            Dim types((Fields.Count) - 1) As Type
            Dim j As Integer = 0
            Do While (j < Fields.Count)
                Dim propInfo As System.Reflection.PropertyInfo = objectType.GetProperty(Fields(j).Name)
                If (Not (propInfo) Is Nothing) Then
                    types(j) = propInfo.PropertyType
                End If
                j = (j + 1)
            Loop
            For Each row() As Object in Rows
                Dim instance As T = CType(objectType.Assembly.CreateInstance(objectType.FullName),T)
                Dim i As Integer = 0
                For Each field As DataField in Fields
                    Try 
                        Dim fieldType As Type = types(i)
                        If (Not (fieldType) Is Nothing) Then
                            args(0) = DataControllerBase.ConvertToType(fieldType, row(i))
                            objectType.InvokeMember(field.Name, System.Reflection.BindingFlags.SetProperty, Nothing, instance, args)
                        End If
                    Catch __exception As Exception
                    End Try
                    i = (i + 1)
                Next
                list.Add(instance)
            Next
            Return list
        End Function
        
        Public Function CustomFilteredBy(ByVal fieldName As String) As Boolean
            Return ((Not (m_CustomFilter) Is Nothing) AndAlso m_CustomFilter.ContainsKey(fieldName))
        End Function
        
        Public Sub ApplyDataFilter(ByVal dataFilter As IDataFilter, ByVal controller As String, ByVal view As String, ByVal lookupContextController As String, ByVal lookupContextView As String, ByVal lookupContextFieldName As String)
            If (dataFilter Is Nothing) Then
                Return
            End If
            If (m_Filter Is Nothing) Then
                m_Filter = New String((0) - 1) {}
            End If
            Dim dataFilter2 As IDataFilter2 = Nothing
            If GetType(IDataFilter2).IsInstanceOfType(dataFilter) Then
                dataFilter2 = CType(dataFilter,IDataFilter2)
                dataFilter2.AssignContext(controller, view, lookupContextController, lookupContextView, lookupContextFieldName)
            End If
            Dim newFilter As List(Of String) = New List(Of String)(m_Filter)
            m_CustomFilter = New SortedDictionary(Of String, Object)()
            If (Not (dataFilter2) Is Nothing) Then
                dataFilter2.Filter(controller, view, m_CustomFilter)
            Else
                dataFilter.Filter(m_CustomFilter)
            End If
            For Each key As String in m_CustomFilter.Keys
                Dim v As Object = m_CustomFilter(key)
                If ((v Is Nothing) OrElse Not (v.GetType().IsArray)) Then
                    v = New Object() {v}
                End If
                Dim sb As StringBuilder = New StringBuilder()
                sb.AppendFormat("{0}:", key)
                For Each item As Object in CType(v,Array)
                    If (Not (dataFilter2) Is Nothing) Then
                        sb.Append(item)
                    Else
                        sb.AppendFormat("={0}", item)
                    End If
                    sb.Append(Convert.ToChar(0))
                Next
                newFilter.Add(sb.ToString())
            Next
            m_OriginalFilter = m_Filter
            m_Filter = newFilter.ToArray()
        End Sub
        
        Public Sub UpdateFieldValue(ByVal fieldName As String, ByVal row() As Object, ByVal value As Object)
            Dim i As Integer = 0
            Do While (i < Fields.Count)
                If Fields(i).Name.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase) Then
                    row(i) = value
                End If
                i = (i + 1)
            Loop
        End Sub
        
        Public Function SelectFieldValue(ByVal fieldName As String, ByVal row() As Object) As Object
            Dim i As Integer = 0
            Do While (i < Fields.Count)
                If Fields(i).Name.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase) Then
                    Return row(i)
                End If
                i = (i + 1)
            Loop
            Return Nothing
        End Function
        
        Public Function SelectFieldValueObject(ByVal fieldName As String, ByVal row() As Object) As FieldValue
            Dim i As Integer = 0
            Do While (i < Fields.Count)
                If Fields(i).Name.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase) Then
                    Return New FieldValue(fieldName, row(i))
                End If
                i = (i + 1)
            Loop
            Return Nothing
        End Function
        
        Public Sub RemoveFromFilter(ByVal fieldName As String)
            If (m_Filter Is Nothing) Then
                Return
            End If
            Dim newFilter As List(Of String) = New List(Of String)(m_Filter)
            Dim prefix As String = (fieldName + ":")
            For Each s As String in newFilter
                If s.StartsWith(prefix) Then
                    newFilter.Remove(s)
                    Exit For
                End If
            Next
            m_Filter = newFilter.ToArray()
        End Sub
        
        Public Function RequiresResultSet(ByVal configuration As CommandConfigurationType) As Boolean
            If Not (String.IsNullOrEmpty(QuickFindHint)) Then
                Return true
            End If
            If (Not (m_Filter) Is Nothing) Then
                For Each s As String in m_Filter
                    Dim m As Match = DataControllerBase.FilterExpressionRegex.Match(s)
                    If (m.Success = m.Groups("Alias").Value.Contains(",")) Then
                        Return true
                    End If
                Next
            End If
            Return false
        End Function
        
        Public Shared Sub InitializeManyToManyProperties(ByVal field As DataField, ByVal controller As String, ByRef targetForeignKey1 As String, ByRef targetForeignKey2 As String)
            Dim target As Controller = New Controller()
            target.SelectView(field.ItemsTargetController, Nothing)
            Dim field1 As XPathNodeIterator = target.Config.Select(String.Format("/c:dataController/c:fields/c:field[c:items/@dataController='{0}']", controller))
            Dim field2 As XPathNodeIterator = target.Config.Select(String.Format("/c:dataController/c:fields/c:field[c:items/@dataController='{0}']", field.ItemsDataController))
            If Not (field1.MoveNext()) Then
                Throw New Exception(String.Format("Field with lookup controller '{0}' not found in target controller '{1}'.", controller, field.ItemsTargetController))
            End If
            If Not (field2.MoveNext()) Then
                Throw New Exception(String.Format("Field with lookup controller '{0}' not found in target controller '{1}'.", field.ItemsDataController, field.ItemsTargetController))
            End If
            targetForeignKey1 = field1.Current.GetAttribute("name", String.Empty)
            targetForeignKey2 = field2.Current.GetAttribute("name", String.Empty)
            Dim field2items As XPathNodeIterator = target.Config.Select(String.Format("/c:dataController/c:fields/c:field[@name='{0}']/c:items", targetForeignKey2))
            If field2items.MoveNext() Then
                field.ItemsDataValueField = field2items.Current.GetAttribute("dataValueField", String.Empty)
                field.ItemsDataTextField = field2items.Current.GetAttribute("dataTextField", String.Empty)
                field.ItemsDataView = field2items.Current.GetAttribute("dataView", String.Empty)
            End If
        End Sub
        
        Public Overridable Sub AddPivotField(ByVal field As DataField)
            'process tags
            Dim tags As MatchCollection = m_PivotTagRegex.Matches(field.Tag)
            For Each tagCapture As Capture in tags
                Dim tag As String = tagCapture.Value
                If tag.StartsWith("pivot") Then
                    Dim properties() As String = tag.Split(Global.Microsoft.VisualBasic.ChrW(45))
                    If (properties.Length >= 2) Then
                        'populate properties
                        Dim pivotID As Integer = 0
                        Dim chartType As String = String.Empty
                        Dim fieldType As String = String.Empty
                        Dim subtotalsEnabled As Boolean = false
                        Dim grandTotalsEnabled As Boolean = false
                        Dim fieldTypeIndex As Integer = 0
                        Dim additionalProperties As Dictionary(Of String, Object) = New Dictionary(Of String, Object)()
                        For Each [property] As String in properties
                            Dim match As Match = m_StringNumSplitter.Match([property])
                            If match.Success Then
                                Dim propertyName As String = CType(match.Groups("PropertyName").Value.ToLower().Trim(),String)
                                Dim propertyValue As Integer
                                Dim propertyValueString As String = String.Empty
                                If Not (Integer.TryParse(match.Groups("PropertyValue").Value, propertyValue)) Then
                                    propertyValue = 0
                                    If [property].Contains(Global.Microsoft.VisualBasic.ChrW(58)) Then
                                        propertyName = [property].Split(Global.Microsoft.VisualBasic.ChrW(58))(0)
                                        propertyValueString = [property].Split(Global.Microsoft.VisualBasic.ChrW(58))(1)
                                        propertyValueString = propertyValueString.Substring(1, (propertyValueString.Length - 2))
                                    End If
                                End If
                                If (propertyName = "pivot") Then
                                    pivotID = propertyValue
                                Else
                                    If (((propertyName = "row") OrElse (propertyName = "col")) OrElse ((propertyName = "val") OrElse (propertyName = "value"))) Then
                                        fieldType = propertyName
                                        fieldTypeIndex = propertyValue
                                    Else
                                        If (propertyName = "column") Then
                                            If (propertyValue = 0) Then
                                                chartType = propertyName
                                            Else
                                                fieldType = propertyName
                                                fieldTypeIndex = propertyValue
                                            End If
                                        Else
                                            If ((propertyName = "subtotal") OrElse (propertyName = "subtotals")) Then
                                                subtotalsEnabled = true
                                            Else
                                                If ((propertyName = "grandtotal") OrElse (propertyName = "grandtotals")) Then
                                                    grandTotalsEnabled = true
                                                Else
                                                    If m_SupportedChartTypes.Contains(propertyName) Then
                                                        chartType = propertyName
                                                    Else
                                                        If String.IsNullOrEmpty(propertyValueString) Then
                                                            additionalProperties.Add(propertyName.ToLower(), propertyValue)
                                                        Else
                                                            additionalProperties.Add(propertyName.ToLower(), propertyValueString)
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        Next
                        'get or add pivot
                        Dim pivot As PivotTable
                        If Not (m_Pivots.ContainsKey(pivotID)) Then
                            pivot = New PivotTable(pivotID, Me)
                            m_Pivots.Add(pivotID, pivot)
                        Else
                            pivot = m_Pivots(pivotID)
                        End If
                        If Not (String.IsNullOrEmpty(chartType)) Then
                            pivot.ChartType = chartType
                        End If
                        If subtotalsEnabled Then
                            pivot.SubtotalsEnabled = true
                        End If
                        If grandTotalsEnabled Then
                            pivot.GrandTotalsEnabled = true
                        End If
                        'add field to pivot
                        If Not (String.IsNullOrEmpty(fieldType)) Then
                            Dim fi As FieldInfo = Nothing
                            If (fieldType = "row") Then
                                fi = pivot.GetRowField(fieldTypeIndex, field)
                            Else
                                If fieldType.StartsWith("col") Then
                                    fi = pivot.GetColumnField(fieldTypeIndex, field)
                                Else
                                    If fieldType.StartsWith("val") Then
                                        fi = pivot.GetValueField(fieldTypeIndex, field)
                                    Else
                                        Return
                                    End If
                                End If
                            End If
                            'check properties
                            Dim i As Integer = 0
                            Do While (i < additionalProperties.Count)
                                Dim kvp As KeyValuePair(Of String, Object) = additionalProperties.ElementAt(i)
                                Dim remove As Boolean = true
                                Dim [property] As String = kvp.Key
                                If m_SupportedModes.Contains([property]) Then
                                    fi.Mode = [property]
                                Else
                                    If m_SupportedBuckets.Contains([property]) Then
                                        fi.Bucket = [property]
                                    Else
                                        If ([property] = "date") Then
                                            'expand auto date
                                            Dim newPivotID As Integer = (Integer.MaxValue  _
                                                        - (pivotID * 6))
                                            Dim newTag As String = (String.Format("pivotgroup{0}-", pivotID)  _
                                                        + (Regex.Replace(tag, "pivot(\d+?)-|row(\d+?)|date|all", String.Empty) + "-pivot"))
                                            field.Tag = ((newTag + newPivotID.ToString())  _
                                                        + "-row1-year ")
                                            AddPivotField(field)
                                            newPivotID = (newPivotID + 1)
                                            'quarter
                                            field.Tag = ((newTag + newPivotID.ToString())  _
                                                        + "-row1-year ")
                                            field.Tag = (field.Tag  _
                                                        + ((newTag + newPivotID.ToString())  _
                                                        + "-row2-quarter"))
                                            AddPivotField(field)
                                            newPivotID = (newPivotID + 1)
                                            'month
                                            field.Tag = ((newTag + newPivotID.ToString())  _
                                                        + "-row1-year ")
                                            field.Tag = (field.Tag  _
                                                        + ((newTag + newPivotID.ToString())  _
                                                        + "-row2-month"))
                                            AddPivotField(field)
                                            newPivotID = (newPivotID + 1)
                                            'week of year
                                            field.Tag = ((newTag + newPivotID.ToString())  _
                                                        + "-row1-year ")
                                            field.Tag = (field.Tag  _
                                                        + ((newTag + newPivotID.ToString())  _
                                                        + "-row2-month "))
                                            field.Tag = (field.Tag  _
                                                        + ((newTag + newPivotID.ToString())  _
                                                        + "-row3-weekofyear"))
                                            AddPivotField(field)
                                            newPivotID = (newPivotID + 1)
                                            'day
                                            field.Tag = ((newTag + newPivotID.ToString())  _
                                                        + "-row1-year ")
                                            field.Tag = (field.Tag  _
                                                        + ((newTag + newPivotID.ToString())  _
                                                        + "-row2-month "))
                                            field.Tag = (field.Tag  _
                                                        + ((newTag + newPivotID.ToString())  _
                                                        + "-row3-day"))
                                            AddPivotField(field)
                                            newPivotID = (newPivotID + 1)
                                            fi.Bucket = "year"
                                        Else
                                            If ([property] = "pivotgroup") Then
                                                'duplicate properties from original pivot
                                                Dim pivotGroupID As Integer = Convert.ToInt32(kvp.Value)
                                                pivot.Group = pivotGroupID
                                                Dim original As PivotTable = m_Pivots(pivotGroupID)
                                                pivot.ColumnFields = original.ColumnFields
                                                pivot.ValueFields = original.ValueFields
                                                pivot.Properties = original.Properties
                                            Else
                                                If ([property] = "all") Then
                                                    fi.ExpandBuckets = true
                                                    If (fieldType = "row") Then
                                                        pivot.ExpandBucketsInRowCount = (pivot.ExpandBucketsInRowCount + 1)
                                                    Else
                                                        If fieldType.StartsWith("col") Then
                                                            pivot.ExpandBucketsInRowCount = (pivot.ExpandBucketsInRowCount + 1)
                                                        End If
                                                    End If
                                                Else
                                                    If ([property] = "hideblank") Then
                                                        fi.HideBlank = true
                                                    Else
                                                        If ([property] = "top") Then
                                                            fi.ShowTop = Convert.ToInt32(kvp.Value)
                                                        Else
                                                            If ([property] = "first") Then
                                                                fi.ShowFirst = Convert.ToInt32(kvp.Value)
                                                            Else
                                                                If ([property] = "calendar") Then
                                                                    fi.Mode = "calendar"
                                                                Else
                                                                    If [property].StartsWith("sort") Then
                                                                        If [property].Contains("asc") Then
                                                                            fi.SortDirection = SortDirection.Ascending
                                                                        Else
                                                                            fi.SortDirection = SortDirection.Descending
                                                                        End If
                                                                        If [property].Contains("byval") Then
                                                                            fi.SortByValue = true
                                                                        End If
                                                                    Else
                                                                        If ([property] = "other") Then
                                                                            fi.CollapseOther = true
                                                                        Else
                                                                            If ([property] = "format") Then
                                                                                pivot.Formats.Add(String.Format("{0}{1}", fi.Field.Name, pivot.ValueFields.Values.ToList().IndexOf(fi)), CType(kvp.Value,String))
                                                                            Else
                                                                                If ([property] = "raw") Then
                                                                                    fi.Raw = true
                                                                                Else
                                                                                    remove = false
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
                                    End If
                                End If
                                If remove Then
                                    'remove the property
                                    i = (i - 1)
                                    additionalProperties.Remove(kvp.Key)
                                End If
                                i = (i + 1)
                            Loop
                        End If
                        'add special properties
                        For Each kvp As KeyValuePair(Of String, Object) in additionalProperties
                            pivot.AddProperty(kvp.Key, kvp.Value)
                        Next
                    End If
                End If
            Next
        End Sub
        
        Public Overridable Sub AddPivotValues(ByVal values() As Object)
            If ((m_Pivots Is Nothing) OrElse (m_Pivots.Count = 0)) Then
                Return
            End If
            For Each pivot As PivotTable in m_Pivots.Values
                pivot.Insert(values)
            Next
        End Sub
    End Class
    
    Public Class PivotTable
        
        Public Id As Integer
        
        Public Name As String
        
        Friend Group As Integer = 0
        
        Private m_Title As String
        
        Private m_ValuesName As String
        
        Public ChartType As String
        
        Friend ExpandBucketsInRowCount As Integer = 0
        
        Friend ExpandBucketsInColumnCount As Integer = 0
        
        Friend SubtotalsEnabled As Boolean = false
        
        Friend GrandTotalsEnabled As Boolean = false
        
        Public RecordCount As Integer = 0
        
        Private m_Page As ViewPage
        
        Private m_StructureValidated As Boolean = false
        
        Friend RowFields As SortedDictionary(Of Integer, FieldInfo) = New SortedDictionary(Of Integer, FieldInfo)()
        
        Friend ColumnFields As SortedDictionary(Of Integer, FieldInfo) = New SortedDictionary(Of Integer, FieldInfo)()
        
        Friend ValueFields As SortedDictionary(Of Integer, FieldInfo) = New SortedDictionary(Of Integer, FieldInfo)()
        
        Friend Rows As SortedDictionary(Of String, DimensionInfo) = New SortedDictionary(Of String, DimensionInfo)()
        
        Friend Columns As SortedDictionary(Of String, DimensionInfo) = New SortedDictionary(Of String, DimensionInfo)()
        
        Private m_Values As SortedDictionary(Of String, ValueInfo) = New SortedDictionary(Of String, ValueInfo)()
        
        Private m_Properties As SortedDictionary(Of String, Object) = New SortedDictionary(Of String, Object)()
        
        Private m_Formats As SortedDictionary(Of String, Object) = New SortedDictionary(Of String, Object)()
        
        Private m_DataValidated As Boolean = false
        
        Public Sub New(ByVal id As Integer, ByVal page As ViewPage)
            MyBase.New
            Me.Id = id
            Me.Name = ("pivot" + id.ToString())
            Me.m_Page = page
        End Sub
        
        Public Overridable ReadOnly Property Title() As String
            Get
                If String.IsNullOrEmpty(m_Title) Then
                    If Not (m_StructureValidated) Then
                        Return String.Empty
                    End If
                    ValidateData()
                    Dim sb As StringBuilder = New StringBuilder()
                    Dim first As Boolean = true
                    Dim lastFieldLabel As String = String.Empty
                    For Each info As FieldInfo in ColumnFields.Values
                        If Not ((lastFieldLabel = info.Field.Label)) Then
                            lastFieldLabel = info.Field.Label
                            If first Then
                                first = false
                            Else
                                sb.Append(", ")
                            End If
                            If (info.ShowTop > 0) Then
                                sb.Append(String.Format("$Top {0} ", info.ShowTop))
                            End If
                            sb.Append(String.Format("""{0}""", lastFieldLabel))
                        End If
                    Next
                    If Not (first) Then
                        sb.Append(": ")
                    End If
                    sb.Append(ValuesName)
                    sb.Append(" $By ")
                    first = true
                    For Each info As FieldInfo in RowFields.Values
                        If Not ((lastFieldLabel = info.Field.Label)) Then
                            lastFieldLabel = info.Field.Label
                            If first Then
                                first = false
                            Else
                                sb.Append(", ")
                            End If
                            If (info.ShowTop > 0) Then
                                sb.Append(String.Format("$Top {0} ", info.ShowTop))
                            End If
                            sb.Append(String.Format("""{0}""", lastFieldLabel))
                        End If
                    Next
                    m_Title = sb.ToString()
                End If
                Return m_Title
            End Get
        End Property
        
        Public Overridable ReadOnly Property ValuesName() As String
            Get
                If String.IsNullOrEmpty(m_ValuesName) Then
                    Dim sb As StringBuilder = New StringBuilder()
                    Dim first As Boolean = true
                    For Each info As FieldInfo in ValueFields.Values
                        If first Then
                            first = false
                        Else
                            sb.Append(", ")
                        End If
                        sb.Append((("$" + info.Mode.Substring(0, 1).ToUpper())  _
                                        + info.Mode.Substring(1)))
                        sb.Append("Of ")
                        Dim label As String = info.Field.Label
                        If Not ((label = "$CurrentViewLabel")) Then
                            label = String.Format("""{0}""", label)
                        End If
                        sb.Append(label)
                    Next
                    m_ValuesName = sb.ToString()
                End If
                Return m_ValuesName
            End Get
        End Property
        
        Public Overridable ReadOnly Property Data() As Object()
            Get
                Return Serialize()
            End Get
        End Property
        
        Public Overridable ReadOnly Property RowFieldNames() As String()
            Get
                Dim list As List(Of String) = New List(Of String)()
                For Each fi As FieldInfo in RowFields.Values
                    list.Add(fi.Field.Name)
                Next
                Return list.ToArray()
            End Get
        End Property
        
        Public Overridable ReadOnly Property ColumnFieldNames() As String()
            Get
                Dim list As List(Of String) = New List(Of String)()
                For Each fi As FieldInfo in ColumnFields.Values
                    list.Add(fi.Field.Name)
                Next
                Return list.ToArray()
            End Get
        End Property
        
        Public Overridable ReadOnly Property ValueFieldNames() As String()
            Get
                Dim list As List(Of String) = New List(Of String)()
                For Each fi As FieldInfo in ValueFields.Values
                    list.Add(fi.Field.Name)
                Next
                Return list.ToArray()
            End Get
        End Property
        
        Public Overridable Property Properties() As SortedDictionary(Of String, Object)
            Get
                Return m_Properties
            End Get
            Set
                m_Properties = value
            End Set
        End Property
        
        Public Overridable Property Formats() As SortedDictionary(Of String, Object)
            Get
                Return m_Formats
            End Get
            Set
                m_Formats = value
            End Set
        End Property
        
        Private Sub ValidateStructure()
            If m_StructureValidated Then
                Return
            Else
                m_StructureValidated = true
            End If
            'Assign value field
            If (ValueFields.Count = 0) Then
                Dim primaryKeyField As DataField = Nothing
                For Each field As DataField in m_Page.Fields
                    If field.IsPrimaryKey Then
                        primaryKeyField = field
                        Exit For
                    End If
                Next
                If (primaryKeyField Is Nothing) Then
                    primaryKeyField = m_Page.Fields.First()
                End If
                If (Not (primaryKeyField) Is Nothing) Then
                    GetValueField(0, primaryKeyField)
                End If
                primaryKeyField.Label = "$CurrentViewLabel"
            End If
            'Validate aliases
            ValidateAliases(RowFields)
            ValidateAliases(ColumnFields)
            ValidateAliases(ValueFields)
            'Validate calendar fields
            For Each kvp As KeyValuePair(Of Integer, FieldInfo) in ValueFields
                Dim fi As FieldInfo = kvp.Value
                If (fi.Mode = "calendar") Then
                    Dim calendarFields((5) - 1) As DataField
                    For Each df As DataField in m_Page.Fields
                        If df.IsPrimaryKey Then
                            calendarFields(0) = df
                        Else
                            If Not (String.IsNullOrEmpty(df.Tag)) Then
                                For Each prop As String in df.Tag.Split(Global.Microsoft.VisualBasic.ChrW(32))
                                    If prop.StartsWith("calendar-") Then
                                        If (prop = "calendar-date") Then
                                            calendarFields(1) = df
                                        Else
                                            If (prop = "calendar-end") Then
                                                calendarFields(2) = df
                                            Else
                                                If (prop = "calendar-color") Then
                                                    calendarFields(3) = df
                                                Else
                                                    If (prop = "calendar-text") Then
                                                        calendarFields(4) = df
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                Next
                            End If
                        End If
                    Next
                    'Resolve lookups
                    Dim i As Integer = 0
                    Do While (i < 5)
                        Dim df As DataField = calendarFields(i)
                        If ((Not (df) Is Nothing) AndAlso Not (String.IsNullOrEmpty(df.AliasName))) Then
                            For Each [alias] As DataField in m_Page.Fields
                                If ([alias].Name = df.AliasName) Then
                                    calendarFields.SetValue([alias], i)
                                End If
                            Next
                        End If
                        i = (i + 1)
                    Loop
                    fi.CalendarFields = calendarFields
                End If
            Next
        End Sub
        
        Private Sub ValidateData()
            If m_DataValidated Then
                Return
            Else
                m_DataValidated = true
            End If
            'Expand Buckets
            If ((ExpandBucketsInRowCount > 0) AndAlso (Rows.Count > 0)) Then
                Dim rowStack As Stack(Of FieldInfo) = New Stack(Of FieldInfo)(RowFields.Values.Reverse())
                ExpandBuckets(rowStack, Rows, ExpandBucketsInRowCount)
            End If
            If ((ExpandBucketsInColumnCount > 0) AndAlso (Columns.Count > 0)) Then
                Dim columnStack As Stack(Of FieldInfo) = New Stack(Of FieldInfo)(ColumnFields.Values.Reverse())
                ExpandBuckets(columnStack, Columns, ExpandBucketsInColumnCount)
            End If
            'Ensure rows and columns
            If (Rows.Count = 0) Then
                Rows.Add(String.Empty, New DimensionInfo(String.Empty, New Object() {String.Empty}, RowFields.Count, ValueFields))
            End If
            If (Columns.Count = 0) Then
                Columns.Add(String.Empty, New DimensionInfo(String.Empty, New Object() {String.Empty}, ColumnFields.Count, ValueFields))
            End If
            'Verify ShowTop
            For Each fi As FieldInfo in RowFields.Values
                If (fi.ShowTop >= Rows.Count) Then
                    fi.ShowTop = 0
                End If
            Next
            For Each fi As FieldInfo in ColumnFields.Values
                If (fi.ShowTop >= Columns.Count) Then
                    fi.ShowTop = 0
                End If
            Next
        End Sub
        
        Private Overloads Sub ExpandBuckets(ByVal fieldStack As Stack(Of FieldInfo), ByVal dimension As SortedDictionary(Of String, DimensionInfo), ByVal expandBucketsCount As Integer)
            ExpandBuckets(String.Empty, New List(Of Object)(), 1, expandBucketsCount, fieldStack, dimension)
        End Sub
        
        Private Overloads Sub ExpandBuckets(ByVal key As String, ByVal keyValues As List(Of Object), ByVal depth As Integer, ByVal expandBucketsCount As Integer, ByVal fieldStack As Stack(Of FieldInfo), ByVal dimension As SortedDictionary(Of String, DimensionInfo))
            If Not ((depth = 1)) Then
                key = (key + "|")
            End If
            If (fieldStack.Count = 0) Then
                Return
            End If
            Dim fieldInfo As FieldInfo = fieldStack.Pop()
            Dim bucketKeys As List(Of Object) = New List(Of Object)()
            If fieldInfo.ExpandBuckets Then
                'find all possible buckets in range
                expandBucketsCount = (expandBucketsCount - 1)
                If Not (String.IsNullOrEmpty(fieldInfo.Bucket)) Then
                    'Create row for each missing bucket
                    Dim iterator As Object = fieldInfo.Min
                    Dim max As Object = fieldInfo.Max
                    Do While Not (fieldInfo.EqualToMax(iterator))
                        bucketKeys.Add(iterator)
                        FindNextBucket(iterator, fieldInfo.Bucket)
                    Loop
                    bucketKeys.Add(iterator)
                Else
                    'Expand lookup fields with distinct values
                    If (fieldInfo.ValueField Is Nothing) Then
                        Return
                    End If
                    Dim originalField As DataField = fieldInfo.ValueField
                    Dim view As String = "grid1"
                    If Not (String.IsNullOrEmpty(originalField.ItemsDataView)) Then
                        view = originalField.ItemsDataView
                    End If
                    Dim field As String = originalField.ItemsDataTextField
                    If String.IsNullOrEmpty(field) Then
                        Dim config As ControllerConfiguration = DataControllerBase.CreateConfigurationInstance([GetType](), originalField.ItemsDataController)
                        Dim fieldNav As XPathNavigator = config.SelectSingleNode("/c:dataController/c:views/c:view[@type!='Form'][1]/c:dataFields/c:dataField[@fiel"& _ 
                                "dName!=/c:dataController/c:fields/c:field[@isPrimaryKey]/@name][1]/@fieldName")
                        If (Not (fieldNav) Is Nothing) Then
                            field = fieldNav.Value
                        End If
                    End If
                    Dim engine As IDataEngine = ControllerFactory.CreateDataEngine()
                    Dim lookupRequest As PageRequest = New PageRequest()
                    lookupRequest.Controller = originalField.ItemsDataController
                    lookupRequest.PageSize = DataControllerBase.MaximumDistinctValues
                    Using reader As DbDataReader = engine.ExecuteReader(lookupRequest)
                        Do While reader.Read()
                            'Get first string field
                            If String.IsNullOrEmpty(CType(field,String)) Then
                                Dim values((reader.FieldCount) - 1) As Object
                                Dim length As Integer = reader.GetValues(values)
                                Dim i As Integer = 0
                                Do While (i < length)
                                    If (values(i).GetType() Is GetType(String)) Then
                                        field = reader.GetName(i)
                                        Exit Do
                                    End If
                                    i = (i + 1)
                                Loop
                            End If
                            bucketKeys.Add(Convert.ToString(reader(field)))
                        Loop
                    End Using
                End If
            Else
                'Find all columns in this level
                For Each [dim] As DimensionInfo in dimension.Values
                    If ([dim].Depth = depth) Then
                        bucketKeys.Add([dim].Labels.Last())
                    End If
                Next
            End If
            For Each bucketKey As Object in bucketKeys
                Dim newKeyValues As List(Of Object) = New List(Of Object)(keyValues)
                Dim unformattedKey As Object = bucketKey
                Dim newKey As String = (key + FormatPivotValue(unformattedKey, newKeyValues, fieldInfo))
                Dim val As DimensionInfo = Nothing
                'expand buckets
                If Not (dimension.TryGetValue(newKey, val)) Then
                    dimension.Add(newKey, New DimensionInfo(newKey, newKeyValues.ToArray(), depth, ValueFields))
                End If
                'expand lower levels
                If (Not ((fieldStack.Count = 0)) AndAlso Not ((expandBucketsCount = 0))) Then
                    ExpandBuckets(newKey, newKeyValues, (depth + 1), expandBucketsCount, New Stack(Of FieldInfo)(fieldStack), dimension)
                End If
            Next
        End Sub
        
        Private Sub FindNextBucket(ByRef iterator As Object, ByVal bucket As String)
            If (bucket = "timeofday") Then
            Else
                If (bucket = "second") Then
                    CType(iterator,TimeSpan).Add(TimeSpan.FromSeconds(1))
                Else
                    If (bucket = "minute") Then
                        CType(iterator,TimeSpan).Add(TimeSpan.FromMinutes(1))
                    Else
                        If (bucket = "halfhour") Then
                            CType(iterator,TimeSpan).Add(TimeSpan.FromMinutes(30))
                        Else
                            If (bucket = "hour") Then
                                CType(iterator,TimeSpan).Add(TimeSpan.FromHours(1))
                            Else
                                If (iterator.GetType() Is GetType(Integer)) Then
                                    iterator = (CType(iterator,Integer) + 1)
                                Else
                                    If (iterator.GetType() Is GetType(Long)) Then
                                        iterator = (CType(iterator,Long) + 1)
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End Sub
        
        Private Sub ValidateAliases(ByVal fieldList As SortedDictionary(Of Integer, FieldInfo))
            'detect alias fields
            Dim fieldsToReplace As SortedDictionary(Of Integer, DataField) = New SortedDictionary(Of Integer, DataField)()
            For Each kvp As KeyValuePair(Of Integer, FieldInfo) in fieldList
                Dim field As DataField = kvp.Value.Field
                If Not (String.IsNullOrEmpty(field.AliasName)) Then
                    Dim aliasField As DataField = Nothing
                    For Each f As DataField in m_Page.Fields
                        If (f.Name = field.AliasName) Then
                            aliasField = f
                        End If
                    Next
                    If (Not (aliasField) Is Nothing) Then
                        fieldsToReplace(kvp.Key) = aliasField
                    End If
                End If
            Next
            'Replace alias fields
            For Each kvp As KeyValuePair(Of Integer, DataField) in fieldsToReplace
                Dim fi As FieldInfo = fieldList(kvp.Key)
                fi.ValueField = fi.Field
                fi.Field = kvp.Value
            Next
        End Sub
        
        Public Overridable Sub Insert(ByVal values() As Object)
            If Not (m_StructureValidated) Then
                ValidateStructure()
            End If
            'calculate row and column
            Dim keyList As List(Of DimensionInfo) = New List(Of DimensionInfo)()
            Dim rowKey As String = GetPivotKey(values, RowFields, Rows, keyList)
            Dim columnKey As String = GetPivotKey(values, ColumnFields, Columns, keyList)
            If (String.IsNullOrEmpty(rowKey) AndAlso ((RowFields.Count > 1) OrElse ((RowFields.Count = 1) AndAlso RowFields.First().Value.HideBlank))) Then
                Return
            End If
            If (String.IsNullOrEmpty(columnKey) AndAlso ((ColumnFields.Count > 1) OrElse ((ColumnFields.Count = 1) AndAlso ColumnFields.First().Value.HideBlank))) Then
                Return
            End If
            Dim i As Integer = 0
            For Each fi As FieldInfo in ValueFields.Values
                'calculate key
                Dim dataKey As String = String.Format("{0},{1}", rowKey, columnKey)
                If (ValueFields.Count > 1) Then
                    dataKey = String.Format("{0},{1}", dataKey, fi.Field.Name)
                    If Not (String.IsNullOrEmpty(fi.Mode)) Then
                        dataKey = String.Format("{0},{1}", dataKey, fi.Mode)
                    End If
                End If
                'get the value
                Dim mode As String = fi.Mode
                Dim val As Object = Nothing
                If (mode = "calendar") Then
                    Dim valList((5) - 1) As Object
                    Dim f As Integer = 0
                    Do While (f < 5)
                        Dim valueField As DataField = fi.CalendarFields(f)
                        If (Not (valueField) Is Nothing) Then
                            Dim valIndex As Integer = m_Page.Fields.IndexOf(valueField)
                            Dim thisVal As Object = values(valIndex)
                            If TypeOf thisVal Is DateTime Then
                                thisVal = DataControllerBase.EnsureJsonCompatibility(thisVal)
                            End If
                            valList(f) = thisVal
                        End If
                        f = (f + 1)
                    Loop
                    val = valList
                Else
                    Dim valueField As DataField = fi.Field
                    Dim valIndex As Integer = m_Page.Fields.IndexOf(valueField)
                    val = values(valIndex)
                End If
                Dim data As ValueInfo = Nothing
                'find the data in Values
                If Not (m_Values.TryGetValue(dataKey, data)) Then
                    data = New ValueInfo(fi)
                    m_Values.Add(dataKey, data)
                End If
                If ((fi.ShowFirst = 0) OrElse (fi.ShowFirst > data.Values.Count)) Then
                    data.Add(val)
                    For Each dimension As DimensionInfo in keyList
                        dimension.Values(i).Add(val, mode)
                    Next
                End If
                i = (i + 1)
            Next
            RecordCount = (RecordCount + 1)
        End Sub
        
        Public Overridable Function GetRowField(ByVal index As Integer, ByVal field As DataField) As FieldInfo
            Dim info As FieldInfo = Nothing
            If RowFields.ContainsKey(index) Then
                info = RowFields(index)
                If (Not ((info.Field.Name = field.Name)) AndAlso ((info.ValueField Is Nothing) OrElse Not ((info.ValueField.Name = field.Name)))) Then
                    Throw New Exception("Duplicate row field declared in pivot.")
                End If
            Else
                info = New FieldInfo(field)
                RowFields.Add(index, info)
            End If
            Return info
        End Function
        
        Public Overridable Function GetColumnField(ByVal index As Integer, ByVal field As DataField) As FieldInfo
            Dim info As FieldInfo = Nothing
            If ColumnFields.ContainsKey(index) Then
                info = ColumnFields(index)
                If (Not ((info.Field.Name = field.Name)) AndAlso ((info.ValueField Is Nothing) OrElse Not ((info.ValueField.Name = field.Name)))) Then
                    Throw New Exception("Duplicate column field declared in pivot.")
                End If
            Else
                info = New FieldInfo(field)
                ColumnFields.Add(index, info)
            End If
            Return info
        End Function
        
        Public Overridable Function GetValueField(ByVal index As Integer, ByVal field As DataField) As FieldInfo
            Dim info As FieldInfo = Nothing
            If ValueFields.ContainsKey(index) Then
                info = ValueFields(index)
                If (Not ((info.Field.Name = field.Name)) AndAlso ((info.ValueField Is Nothing) OrElse Not ((info.ValueField.Name = field.Name)))) Then
                    Throw New Exception("Duplicate value field declared in pivot.")
                End If
            Else
                info = New FieldInfo(field)
                ValueFields.Add(index, info)
            End If
            Return info
        End Function
        
        Public Overridable Sub AddProperty(ByVal key As String, ByVal value As Object)
            If Not (m_Properties.ContainsKey(key)) Then
                m_Properties.Add(key, value)
            End If
        End Sub
        
        Private Function GetPivotKey(ByVal values() As Object, ByVal fieldList As SortedDictionary(Of Integer, FieldInfo), ByVal dimensionList As SortedDictionary(Of String, DimensionInfo), ByVal keyList As List(Of DimensionInfo)) As String
            Dim pivotKey As String = String.Empty
            Dim pivotValuesList As List(Of Object) = New List(Of Object)()
            Dim depth As Integer = 1
            For Each fieldPair As KeyValuePair(Of Integer, FieldInfo) in fieldList
                Dim fi As FieldInfo = fieldPair.Value
                Dim field As DataField = fi.Field
                If Not ((depth = 1)) Then
                    pivotKey = (pivotKey + "|")
                End If
                'append value
                Dim index As Integer = m_Page.Fields.IndexOf(field)
                Dim value As Object = values(index)
                Dim dimensionKey As Object = FormatPivotValue(value, pivotValuesList, fi)
                pivotKey = (pivotKey + dimensionKey)
                If (fi.HideBlank AndAlso String.IsNullOrEmpty(Convert.ToString(dimensionKey))) Then
                    Return String.Empty
                End If
                'initialize row or column
                Dim dimKey As DimensionInfo = Nothing
                If Not (dimensionList.TryGetValue(pivotKey, dimKey)) Then
                    dimKey = New DimensionInfo(pivotKey, pivotValuesList.ToArray(), depth, ValueFields)
                    dimensionList.Add(pivotKey, dimKey)
                End If
                'update field info
                fi.Add(value)
                keyList.Add(dimKey)
                depth = (depth + 1)
            Next
            Return pivotKey
        End Function
        
        Private Function FormatPivotValue(ByRef value As Object, ByVal pivotValuesList As List(Of Object), ByVal fi As FieldInfo) As Object
            If (value Is Nothing) Then
                Return value
            End If
            Dim addValueToArray As Boolean = true
            Dim ci As CultureInfo = CultureInfo.CurrentCulture
            'form buckets based on bucket mode
            If Not (String.IsNullOrEmpty(fi.Bucket)) Then
                If (value.GetType() Is GetType(DateTime)) Then
                    Dim [date] As DateTime = CType(value,DateTime)
                    If (fi.Bucket = "timeofday") Then
                        value = [date].TimeOfDay
                        addValueToArray = false
                        pivotValuesList.Add(CType(value,TimeSpan).ToString())
                    Else
                        If (fi.Bucket = "minute") Then
                            value = New TimeSpan(0, [date].Minute, 0)
                            addValueToArray = false
                            pivotValuesList.Add(String.Format("{0:d2}", [date].Minute))
                        Else
                            If (fi.Bucket = "halfhour") Then
                                Dim minute As Integer = 0
                                If ([date].Minute >= 30) Then
                                    minute = 30
                                End If
                                value = New System.TimeSpan([date].Hour, minute, 0)
                                addValueToArray = false
                                pivotValuesList.Add(String.Format("{0:d2}:{1:d2}", [date].Hour, minute))
                            Else
                                If (fi.Bucket = "hour") Then
                                    value = New TimeSpan([date].Hour, 0, 0)
                                    addValueToArray = false
                                    pivotValuesList.Add(String.Format("{0:d2}:00", [date].Hour))
                                Else
                                    If ((fi.Bucket = "day") OrElse (fi.Bucket = "dayofmonth")) Then
                                        value = [date].Day
                                    Else
                                        If (fi.Bucket = "dayofweek") Then
                                            value = CType([date].DayOfWeek,Integer)
                                            addValueToArray = false
                                            pivotValuesList.Add([date].DayOfWeek)
                                        Else
                                            If (fi.Bucket = "dayofyear") Then
                                                value = [date].DayOfYear
                                            Else
                                                If (fi.Bucket = "weekofmonth") Then
                                                    value = GetWeekNumberOfMonth([date])
                                                    addValueToArray = false
                                                    pivotValuesList.Add(String.Format("$Week{0}", value))
                                                Else
                                                    If ((fi.Bucket = "week") OrElse (fi.Bucket = "weekofyear")) Then
                                                        value = ci.Calendar.GetWeekOfYear([date], CalendarWeekRule.FirstDay, DayOfWeek.Sunday)
                                                        addValueToArray = false
                                                        pivotValuesList.Add(String.Format("$Week{0}", value))
                                                    Else
                                                        If ((fi.Bucket = "twoweek") OrElse (fi.Bucket = "twoweeks")) Then
                                                        Else
                                                            If (fi.Bucket = "month") Then
                                                                value = [date].Month
                                                                addValueToArray = false
                                                                If (RowFields.Count > 2) Then
                                                                    pivotValuesList.Add(ci.DateTimeFormat.AbbreviatedMonthNames(([date].Month - 1)))
                                                                Else
                                                                    pivotValuesList.Add(ci.DateTimeFormat.MonthNames(([date].Month - 1)))
                                                                End If
                                                            Else
                                                                If (fi.Bucket = "quarter") Then
                                                                    Dim month As Integer = [date].Month
                                                                    addValueToArray = false
                                                                    If (month <= 3) Then
                                                                        value = 1
                                                                    Else
                                                                        If (month <= 6) Then
                                                                            value = 2
                                                                        Else
                                                                            If (month <= 9) Then
                                                                                value = 3
                                                                            Else
                                                                                value = 4
                                                                            End If
                                                                        End If
                                                                    End If
                                                                    pivotValuesList.Add(("$Quarter" + value.ToString()))
                                                                Else
                                                                    If (fi.Bucket = "year") Then
                                                                        value = [date].Year
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
                        End If
                    End If
                Else
                    If (value.GetType() Is GetType(Long)) Then
                        Dim val As Long = CType(value,Long)
                        If (fi.Bucket = "dayofweek") Then
                            addValueToArray = false
                            pivotValuesList.Add(ci.DateTimeFormat.DayNames(val))
                        Else
                            If (fi.Bucket = "month") Then
                                addValueToArray = false
                                pivotValuesList.Add(ci.DateTimeFormat.MonthNames((val - 1)))
                            Else
                                If (fi.Bucket = "quarter") Then
                                    addValueToArray = false
                                    pivotValuesList.Add(("$Quarter" + value.ToString()))
                                End If
                            End If
                        End If
                    Else
                        If (value.GetType() Is GetType(TimeSpan)) Then
                            Dim time As TimeSpan = CType(value,TimeSpan)
                            If (fi.Bucket = "second") Then
                                value = time.Seconds
                            Else
                                If (fi.Bucket = "minute") Then
                                    value = time.Minutes
                                Else
                                    If (fi.Bucket = "hour") Then
                                        value = time.Hours
                                    Else
                                        If (fi.Bucket = "day") Then
                                            value = time.Days
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
            'allow unformatted value printing
            If (fi.Raw AndAlso Not (addValueToArray)) Then
                pivotValuesList.RemoveAt((pivotValuesList.Count - 1))
                pivotValuesList.Add(value)
            End If
            If addValueToArray Then
                pivotValuesList.Add(value)
            End If
            Dim formattedValue As Object = value
            'Format field value in sortable form
            If (value.GetType() Is GetType(DateTime)) Then
                formattedValue = CType(value,DateTime).ToString("s")
            Else
                If (value.GetType() Is GetType(TimeSpan)) Then
                    formattedValue = CType(value,TimeSpan).ToString("hh\:mm\:ss")
                Else
                    If (value.GetType() Is GetType(Integer)) Then
                        formattedValue = CType(value,Integer).ToString("D10")
                    Else
                        If (value.GetType() Is GetType(Short)) Then
                            formattedValue = CType(value,Short).ToString("D5")
                        Else
                            If (value.GetType() Is GetType(Long)) Then
                                formattedValue = CType(value,Long).ToString("D10")
                            Else
                                If (value.GetType() Is GetType(Decimal)) Then
                                    formattedValue = CType(value,Decimal).ToString("########.####")
                                Else
                                    If Not ((value = Nothing)) Then
                                        formattedValue = value.ToString()
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
            Return formattedValue
        End Function
        
        Shared Function GetWeekNumberOfMonth(ByVal [date] As Date) As Integer
            Dim firstDayOfMonth As Date = New Date([date].Year, [date].Month, 1)
            Dim firstDay As Integer = CType(firstDayOfMonth.DayOfWeek,Integer)
            If (firstDay = 0) Then
                firstDay = 7
            End If
            Dim d As Double = ((firstDay  _
                        + ([date].Day - 1))  _
                        / Convert.ToDouble(7))
            Return CType(Math.Ceiling(d),Integer)
        End Function
        
        Function Serialize() As Object()
            ValidateData()
            Dim columnDepth As Integer = ColumnFields.Count
            Dim rowDepth As Integer = RowFields.Count
            'sort the rows and columns
            Dim sortedColumns As List(Of DimensionInfo) = New List(Of DimensionInfo)(Columns.Values)
            Dim columnComparer As DimensionInfoComparer = New DimensionInfoComparer(ColumnFields)
            sortedColumns.Sort(columnComparer)
            Dim sortedRows As List(Of DimensionInfo) = New List(Of DimensionInfo)(Rows.Values)
            Dim rowComparer As DimensionInfoComparer = New DimensionInfoComparer(RowFields)
            rowComparer.ValueFields.AddRange(ValueFields.Values)
            sortedRows.Sort(rowComparer)
            'rows of the pivot
            Dim rowList As List(Of Object) = New List(Of Object)()
            'add header row
            Dim columnHeaderList As List(Of Object) = New List(Of Object)()
            'add row label label
            If Not ((rowDepth = 0)) Then
                columnHeaderList.Add(GetLabel(RowFields.Values.ToArray()))
            Else
                If Not ((columnDepth = 0)) Then
                    columnHeaderList.Add(GetLabel(ColumnFields.Values.ToArray()))
                Else
                    columnHeaderList.Add(ValuesName)
                End If
            End If
            'add column header labels
            For Each [dim] As DimensionInfo in sortedColumns
                'skip group headers
                If (([dim].Depth = columnDepth) OrElse SubtotalsEnabled) Then
                    Dim columnLabel As String = GetLabel([dim].Labels)
                    If (ValueFields.Count = 1) Then
                        If Not (String.IsNullOrEmpty(columnLabel)) Then
                            columnHeaderList.Add(columnLabel)
                        Else
                            If (Columns.Count = 1) Then
                                columnHeaderList.Add(ValuesName)
                            Else
                                columnHeaderList.Add("$Blank")
                            End If
                        End If
                    Else
                        For Each fi As FieldInfo in ValueFields.Values
                            If Not (String.IsNullOrEmpty(columnLabel)) Then
                                columnHeaderList.Add(String.Format("{0}, {1}", columnLabel, fi.Field.Name))
                            Else
                                columnHeaderList.Add(fi.Field.Name)
                            End If
                        Next
                    End If
                End If
            Next
            If GrandTotalsEnabled Then
                columnHeaderList.Add("$GrandTotals")
            End If
            rowList.Add(columnHeaderList.ToArray())
            'add rows
            Dim useRawLabel As Boolean = false
            For Each rowLabelInfo As FieldInfo in RowFields.Values
                If rowLabelInfo.Raw Then
                    useRawLabel = true
                    Exit For
                End If
            Next
            For Each rowInfo As DimensionInfo in sortedRows
                If ((rowInfo.Depth = rowDepth) OrElse SubtotalsEnabled) Then
                    Dim row As String = rowInfo.Key
                    Dim columnList As List(Of Object) = New List(Of Object)()
                    'row label
                    Dim rowLabel As String = String.Empty
                    If useRawLabel Then
                        Dim first As Boolean = true
                        Dim sb As StringBuilder = New StringBuilder()
                        For Each obj As Object in rowInfo.Labels
                            If first Then
                                first = false
                            Else
                                sb.Append(", ")
                            End If
                            sb.Append(obj.ToString())
                        Next
                        rowLabel = sb.ToString()
                    Else
                        rowLabel = GetLabel(rowInfo.Labels)
                    End If
                    If Not (String.IsNullOrEmpty(rowLabel)) Then
                        columnList.Add(rowLabel)
                    Else
                        If (Rows.Count = 1) Then
                            columnList.Add(ValuesName)
                        Else
                            columnList.Add("$Blank")
                        End If
                    End If
                    'columns
                    For Each columnInfo As DimensionInfo in sortedColumns
                        If ((columnInfo.Depth = columnDepth) OrElse SubtotalsEnabled) Then
                            Dim column As String = columnInfo.Key
                            'add values
                            For Each fi As FieldInfo in ValueFields.Values
                                'form value key
                                Dim valueKey As String = String.Format("{0},{1}", row, column)
                                If (ValueFields.Count > 1) Then
                                    valueKey = String.Format("{0},{1}", valueKey, fi.Field.Name)
                                    If Not (String.IsNullOrEmpty(fi.Mode)) Then
                                        valueKey = String.Format("{0},{1}", valueKey, fi.Mode)
                                    End If
                                End If
                                Dim value As ValueInfo = Nothing
                                If m_Values.TryGetValue(valueKey, value) Then
                                    columnList.Add(value.Serialize(fi.Mode))
                                Else
                                    columnList.Add(Nothing)
                                End If
                            Next
                        End If
                    Next
                    'grand total of row
                    If GrandTotalsEnabled Then
                        Dim i As Integer = 0
                        Do While (i < ValueFields.Count)
                            columnList.Add(rowInfo.Values(i).Serialize())
                            i = (i + 1)
                        Loop
                    End If
                    rowList.Add(columnList.ToArray())
                End If
            Next
            If GrandTotalsEnabled Then
                'grand totals for columns
                Dim grandTotalRowList As List(Of Object) = New List(Of Object)()
                grandTotalRowList.Add("$GrandTotals")
                For Each columnInfo As DimensionInfo in sortedColumns
                    Dim i As Integer = 0
                    Do While (i < ValueFields.Count)
                        grandTotalRowList.Add(columnInfo.Values(i).Serialize())
                        i = (i + 1)
                    Loop
                Next
                grandTotalRowList.Add(RecordCount)
                rowList.Add(grandTotalRowList.ToArray())
            End If
            Dim rowCount As Integer = rowList.Count
            Dim columnCount As Integer = CType(rowList(0),Object()).Length
            If GrandTotalsEnabled Then
                rowCount = (rowCount - 1)
                columnCount = (columnCount - 1)
            End If
            Dim newColumnCount As Integer = columnCount
            'eliminate extra columns
            For Each info As FieldInfo in ColumnFields.Values
                If Not ((info.ShowTop = 0)) Then
                    Dim otherColumnIndex As Integer = (info.ShowTop + 1)
                    newColumnCount = otherColumnIndex
                    If GrandTotalsEnabled Then
                        newColumnCount = (newColumnCount + 1)
                    End If
                    If (columnCount > (otherColumnIndex + 1)) Then
                        If info.CollapseOther Then
                            newColumnCount = (newColumnCount + 1)
                        End If
                        Dim i As Integer = 0
                        Do While (i < rowList.Count)
                            Dim row() As Object = CType(rowList(i),Object())
                            Dim newRow((newColumnCount) - 1) As Object
                            Dim j As Integer = 0
                            Do While (j < otherColumnIndex)
                                newRow(j) = row(j)
                                j = (j + 1)
                            Loop
                            If GrandTotalsEnabled Then
                                newRow((newColumnCount - 1)) = row((row.Length - 1))
                            End If
                            If info.CollapseOther Then
                                If (i = 0) Then
                                    newRow(otherColumnIndex) = "$Other"
                                Else
                                    Dim vi As ValueInfo = New ValueInfo()
                                    If (info.Mode = "count") Then
                                        vi.Mode = "sum"
                                    Else
                                        vi.Mode = info.Mode
                                    End If
                                    Dim k As Integer = otherColumnIndex
                                    Do While (k < columnCount)
                                        vi.Add(row(k))
                                        k = (k + 1)
                                    Loop
                                    newRow(otherColumnIndex) = vi.Serialize()
                                End If
                            End If
                            rowList(i) = newRow
                            i = (i + 1)
                        Loop
                    End If
                    Exit For
                End If
            Next
            'Eliminate extra rows
            For Each info As FieldInfo in RowFields.Values
                If Not ((info.ShowTop = 0)) Then
                    Dim otherRowIndex As Integer = (info.ShowTop + 1)
                    If (rowCount > (otherRowIndex + 1)) Then
                        Dim numToRemove As Integer = (rowList.Count - otherRowIndex)
                        Dim removeIndex As Integer = otherRowIndex
                        If GrandTotalsEnabled Then
                            numToRemove = (numToRemove - 1)
                            Dim grandTotalRow As Object = rowList.ElementAt((rowList.Count - 1))
                            rowList.RemoveAt((rowList.Count - 1))
                            rowList.Add(grandTotalRow)
                        End If
                        If info.CollapseOther Then
                            'step through columns
                            Dim otherRowList As List(Of ValueInfo) = New List(Of ValueInfo)()
                            Dim i As Integer = 1
                            Do While (i < newColumnCount)
                                Dim vi As ValueInfo = New ValueInfo()
                                Dim mode As String = ValueFields.Values.ElementAt(0).Mode
                                If (mode = "count") Then
                                    vi.Mode = "sum"
                                Else
                                    vi.Mode = mode
                                End If
                                'step through rows
                                Dim j As Integer = otherRowIndex
                                Do While (j < rowCount)
                                    vi.Add(CType(rowList(j),Object())(i))
                                    j = (j + 1)
                                Loop
                                otherRowList.Add(vi)
                                i = (i + 1)
                            Loop
                            Dim otherRow((newColumnCount) - 1) As Object
                            otherRow(0) = "$Other"
                            Dim k As Integer = 1
                            Do While (k <= otherRowList.Count)
                                otherRow(k) = otherRowList((k - 1)).Serialize()
                                k = (k + 1)
                            Loop
                            rowList.Add(otherRow)
                        End If
                        rowList.RemoveRange(removeIndex, numToRemove)
                    End If
                    Exit For
                End If
            Next
            Return rowList.ToArray()
        End Function
        
        Shared Function GetLabel(ByVal list() As Object) As String
            If (list Is Nothing) Then
                Return String.Empty
            End If
            Dim columnBuilder As StringBuilder = New StringBuilder()
            Dim lastValue As String = String.Empty
            Dim firstRowValue As Boolean = true
            For Each val As Object in list
                If (Not (TypeOf val Is String) OrElse Not (String.IsNullOrEmpty(CType(val,String)))) Then
                    If Not ((val.ToString() = lastValue)) Then
                        lastValue = val.ToString()
                        If firstRowValue Then
                            firstRowValue = false
                        Else
                            columnBuilder.Append(", ")
                        End If
                        columnBuilder.Append(val)
                    End If
                End If
            Next
            Return columnBuilder.ToString()
        End Function
    End Class
    
    Public Class ValueInfo
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Count As Integer
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Sum As Object
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Min As Object
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Max As Object
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Average As Object
        
        Public Field As FieldInfo
        
        Private m_FieldType As Type
        
        Private m_Mode As String
        
        Private m_Values As List(Of Object) = New List(Of Object)()
        
        Public Shared SignedNumberTypes() As System.Type = New System.Type() {GetType(SByte), GetType(Short), GetType(Integer), GetType(Long)}
        
        Public Shared UnsignedNumberTypes() As Type = New System.Type() {GetType(Byte), GetType(UShort), GetType(UInteger), GetType(ULong)}
        
        Public Shared FloatNumberTypes() As Type = New Type() {GetType(Single), GetType(Double), GetType(Decimal)}
        
        Public Sub New()
            MyBase.New
        End Sub
        
        Public Sub New(ByVal fi As FieldInfo)
            MyBase.New
            Me.Field = fi
        End Sub
        
        Private Property Count() As Integer
            Get
                Return Me.m_Count
            End Get
            Set
                Me.m_Count = value
            End Set
        End Property
        
        Public Overridable Property Sum() As Object
            Get
                Return Me.m_Sum
            End Get
            Set
                Me.m_Sum = value
            End Set
        End Property
        
        Public Overridable Property Min() As Object
            Get
                Return Me.m_Min
            End Get
            Set
                Me.m_Min = value
            End Set
        End Property
        
        Public Overridable Property Max() As Object
            Get
                Return Me.m_Max
            End Get
            Set
                Me.m_Max = value
            End Set
        End Property
        
        Public Overridable Property Average() As Object
            Get
                Return Me.m_Average
            End Get
            Set
                Me.m_Average = value
            End Set
        End Property
        
        Public Overridable Property FieldType() As Type
            Get
                If ((m_FieldType Is Nothing) AndAlso (Not (Field) Is Nothing)) Then
                    m_FieldType = Field.FieldType
                End If
                Return m_FieldType
            End Get
            Set
                m_FieldType = value
            End Set
        End Property
        
        Public Overridable Property Mode() As String
            Get
                If Not (String.IsNullOrEmpty(m_Mode)) Then
                    Return m_Mode
                End If
                If (Not (Field) Is Nothing) Then
                    Return Field.Mode
                End If
                Return "count"
            End Get
            Set
                m_Mode = value
            End Set
        End Property
        
        Public Overridable Property Values() As List(Of Object)
            Get
                Return m_Values
            End Get
            Set
                m_Values = value
            End Set
        End Property
        
        Public Overloads Overridable Sub Add(ByVal value As Object)
            Count = (Count + 1)
            If (value Is Nothing) Then
                Return
            End If
            If (FieldType Is Nothing) Then
                FieldType = value.GetType()
            End If
            Values.Add(value)
            'additional processing based on type
            If (FieldType Is GetType(Boolean)) Then
                If (Sum = Nothing) Then
                    Sum = CType(0,Long)
                End If
                If CType(value,Boolean) Then
                    Sum = (CType(Sum,Long) + 1)
                End If
            Else
                If SignedNumberTypes.Contains(FieldType) Then
                    Dim val As Long = Convert.ToInt64(value)
                    If (Sum = Nothing) Then
                        Sum = val
                    Else
                        Sum = (CType(Sum,Long) + val)
                    End If
                    If ((Min = Nothing) OrElse (val < CType(Min,Long))) Then
                        Min = val
                    End If
                    If ((Max = Nothing) OrElse (val > CType(Max,Long))) Then
                        Max = val
                    End If
                    Average = (CType(Sum,Long) / Convert.ToInt64(Count))
                Else
                    If UnsignedNumberTypes.Contains(FieldType) Then
                        Dim val As ULong = Convert.ToUInt64(value)
                        If (Sum = Nothing) Then
                            Sum = val
                        Else
                            Sum = (CType(Sum,ULong) + val)
                        End If
                        If ((Min = Nothing) OrElse (val < CType(Min,ULong))) Then
                            Min = val
                        End If
                        If ((Max = Nothing) OrElse (val > CType(Max,ULong))) Then
                            Max = val
                        End If
                        Average = (CType(Sum,ULong) / Convert.ToUInt64(Count))
                    Else
                        If ((FieldType Is GetType(Single)) OrElse (FieldType Is GetType(Double))) Then
                            Dim val As Double = Convert.ToDouble(value)
                            If (Sum = Nothing) Then
                                Sum = val
                            Else
                                Sum = (CType(Sum,Double) + val)
                            End If
                            If ((Min = Nothing) OrElse (val < CType(Min,Double))) Then
                                Min = val
                            End If
                            If ((Max = Nothing) OrElse (val > CType(Max,Double))) Then
                                Max = val
                            End If
                            Average = (CType(Sum,Double) / Convert.ToDouble(Count))
                        Else
                            If (FieldType Is GetType(Decimal)) Then
                                Dim val As Decimal = Convert.ToDecimal(value)
                                If (Sum = Nothing) Then
                                    Sum = val
                                Else
                                    Sum = (CType(Sum,Decimal) + val)
                                End If
                                If ((Min = Nothing) OrElse (val < CType(Min,Decimal))) Then
                                    Min = val
                                End If
                                If ((Max = Nothing) OrElse (val > CType(Max,Decimal))) Then
                                    Max = val
                                End If
                                Average = (CType(Sum,Decimal) / Convert.ToDecimal(Count))
                            Else
                                If (FieldType Is GetType(DateTime)) Then
                                    Dim val As DateTime = CType(value,DateTime)
                                    If ((Min = Nothing) OrElse (val < CType(Min,DateTime))) Then
                                        Min = val
                                    End If
                                    If ((Max = Nothing) OrElse (val > CType(Max,DateTime))) Then
                                        Max = val
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End Sub
        
        Public Overloads Overridable Sub Add(ByVal value As Object, ByVal mode As String)
            If Not ((Mode = mode)) Then
                Mode = mode
            End If
            Add(value)
        End Sub
        
        Public Overloads Overridable Function Serialize() As Object
            Return Serialize(Mode)
        End Function
        
        Public Overloads Overridable Function Serialize(ByVal mode As String) As Object
            Dim valueList As Dictionary(Of String, Object) = New Dictionary(Of String, Object)()
            valueList.Add("count", Count)
            If (SignedNumberTypes.Contains(FieldType) OrElse (UnsignedNumberTypes.Contains(FieldType) OrElse FloatNumberTypes.Contains(FieldType))) Then
                valueList.Add("sum", Sum)
                valueList.Add("min", Min)
                valueList.Add("max", Max)
                valueList.Add("avg", Average)
            Else
                If (FieldType Is GetType(DateTime)) Then
                    valueList.Add("min", Min)
                    valueList.Add("max", Max)
                End If
            End If
            Dim value As Object = Nothing
            If (mode = "calendar") Then
                Dim calValues As List(Of Object) = New List(Of Object)()
                For Each o As Object in Values
                    If TypeOf o Is Object Then
                        calValues.Add(o)
                    End If
                Next
                value = calValues.ToArray()
            Else
                If (Not (valueList.TryGetValue(mode, value)) OrElse (value Is Nothing)) Then
                    value = valueList("count")
                End If
            End If
            Return value
        End Function
    End Class
    
    Public Enum SortDirection
        
        None
        
        Ascending
        
        Descending
    End Enum
    
    Public Class FieldInfo
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Field As DataField
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_ValueField As DataField
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_FieldType As Type
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Mode As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Bucket As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_ExpandBuckets As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Values As ValueInfo
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_ShowTop As Integer
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_ShowFirst As Integer
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_CalendarFields() As DataField
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_CollapseOther As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_SortDirection As SortDirection
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_SortByValue As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_HideBlank As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Raw As Boolean
        
        Public Sub New(ByVal field As DataField)
            Me.New(field, "Count")
        End Sub
        
        Public Sub New(ByVal field As DataField, ByVal mode As String)
            MyBase.New
            Me.Field = field
            Me.Mode = mode.ToLower()
            ExpandBuckets = false
        End Sub
        
        Public Overridable Property Field() As DataField
            Get
                Return Me.m_Field
            End Get
            Set
                Me.m_Field = value
            End Set
        End Property
        
        Public Overridable Property ValueField() As DataField
            Get
                Return Me.m_ValueField
            End Get
            Set
                Me.m_ValueField = value
            End Set
        End Property
        
        Public Overridable Property FieldType() As Type
            Get
                Return Me.m_FieldType
            End Get
            Set
                Me.m_FieldType = value
            End Set
        End Property
        
        Public Overridable Property Mode() As String
            Get
                Return Me.m_Mode
            End Get
            Set
                Me.m_Mode = value
            End Set
        End Property
        
        Public Overridable Property Bucket() As String
            Get
                Return Me.m_Bucket
            End Get
            Set
                Me.m_Bucket = value
            End Set
        End Property
        
        Public Overridable Property ExpandBuckets() As Boolean
            Get
                Return Me.m_ExpandBuckets
            End Get
            Set
                Me.m_ExpandBuckets = value
            End Set
        End Property
        
        Public Overridable Property Values() As ValueInfo
            Get
                Return Me.m_Values
            End Get
            Set
                Me.m_Values = value
            End Set
        End Property
        
        Public Overridable ReadOnly Property Min() As Object
            Get
                Return m_Values.Min
            End Get
        End Property
        
        Public Overridable ReadOnly Property Max() As Object
            Get
                Return m_Values.Max
            End Get
        End Property
        
        Public Overridable Property ShowTop() As Integer
            Get
                Return Me.m_ShowTop
            End Get
            Set
                Me.m_ShowTop = value
            End Set
        End Property
        
        Public Overridable Property ShowFirst() As Integer
            Get
                Return Me.m_ShowFirst
            End Get
            Set
                Me.m_ShowFirst = value
            End Set
        End Property
        
        Public Overridable Property CalendarFields() As DataField()
            Get
                Return Me.m_CalendarFields
            End Get
            Set
                Me.m_CalendarFields = value
            End Set
        End Property
        
        Public Overridable Property CollapseOther() As Boolean
            Get
                Return Me.m_CollapseOther
            End Get
            Set
                Me.m_CollapseOther = value
            End Set
        End Property
        
        Public Overridable Property SortDirection() As SortDirection
            Get
                Return Me.m_SortDirection
            End Get
            Set
                Me.m_SortDirection = value
            End Set
        End Property
        
        Public Overridable Property SortByValue() As Boolean
            Get
                Return Me.m_SortByValue
            End Get
            Set
                Me.m_SortByValue = value
            End Set
        End Property
        
        Public Overridable Property HideBlank() As Boolean
            Get
                Return Me.m_HideBlank
            End Get
            Set
                Me.m_HideBlank = value
            End Set
        End Property
        
        Public Overridable Property Raw() As Boolean
            Get
                Return Me.m_Raw
            End Get
            Set
                Me.m_Raw = value
            End Set
        End Property
        
        Public Overridable Sub Add(ByVal value As Object)
            If String.IsNullOrEmpty(Bucket) Then
                Return
            End If
            If ((FieldType Is Nothing) AndAlso (Not (value) Is Nothing)) Then
                FieldType = value.GetType()
            End If
            If (m_Values Is Nothing) Then
                m_Values = New ValueInfo(Me)
            End If
            m_Values.Add(value)
        End Sub
        
        Public Overridable Function EqualToMax(ByVal value As Object) As Boolean
            If ((value Is Nothing) OrElse (Max Is Nothing)) Then
                Return true
            End If
            Dim type As Type = value.GetType()
            If (type Is GetType(Long)) Then
                Return CType(value,Long).Equals(CType(Max,Long))
            Else
                If (type Is GetType(ULong)) Then
                    Return CType(value,ULong).Equals(CType(Max,ULong))
                Else
                    If (type Is GetType(Double)) Then
                        Return CType(value,Double).Equals(CType(Max,Double))
                    Else
                        If (type Is GetType(Decimal)) Then
                            Return CType(value,Decimal).Equals(CType(Max,Decimal))
                        Else
                            If (type Is GetType(DateTime)) Then
                                Return CType(value,DateTime).Equals(CType(Max,DateTime))
                            Else
                                If (type Is GetType(TimeSpan)) Then
                                    Return CType(value,TimeSpan).Equals(CType(Max,TimeSpan))
                                End If
                            End If
                        End If
                    End If
                End If
            End If
            Return true
        End Function
        
        Public Overrides Function ToString() As String
            Return Field.Label
        End Function
    End Class
    
    Public Class DimensionInfo
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Key As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Labels() As Object
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Values As List(Of ValueInfo)
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Depth As Integer
        
        Public Sub New(ByVal key As String, ByVal labels() As Object, ByVal depth As Integer, ByVal valueFields As SortedDictionary(Of Integer, FieldInfo))
            MyBase.New
            Me.Key = key
            Me.Labels = labels
            Me.Depth = depth
            Me.Values = New List(Of ValueInfo)()
            For Each fi As FieldInfo in valueFields.Values
                Values.Add(New ValueInfo(fi))
            Next
        End Sub
        
        Public Overridable Property Key() As String
            Get
                Return Me.m_Key
            End Get
            Set
                Me.m_Key = value
            End Set
        End Property
        
        Public Overridable Property Labels() As Object()
            Get
                Return Me.m_Labels
            End Get
            Set
                Me.m_Labels = value
            End Set
        End Property
        
        Public Overridable Property Values() As List(Of ValueInfo)
            Get
                Return Me.m_Values
            End Get
            Set
                Me.m_Values = value
            End Set
        End Property
        
        Public Overridable Property Depth() As Integer
            Get
                Return Me.m_Depth
            End Get
            Set
                Me.m_Depth = value
            End Set
        End Property
        
        Public Overridable ReadOnly Property Count() As Integer
            Get
                Return Labels.Length
            End Get
        End Property
        
        Public Overrides Function ToString() As String
            Return Key
        End Function
    End Class
    
    Public Class DimensionInfoComparer
        Inherits Object
        Implements IComparer(Of DimensionInfo)
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_CompareFields As List(Of FieldInfo)
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_ValueFields As List(Of FieldInfo)
        
        Public Sub New(ByVal fieldList As SortedDictionary(Of Integer, FieldInfo))
            MyBase.New
            CompareFields = New List(Of FieldInfo)(fieldList.Values)
            ValueFields = New List(Of FieldInfo)()
        End Sub
        
        Public Overridable Property CompareFields() As List(Of FieldInfo)
            Get
                Return Me.m_CompareFields
            End Get
            Set
                Me.m_CompareFields = value
            End Set
        End Property
        
        Public Overridable Property ValueFields() As List(Of FieldInfo)
            Get
                Return Me.m_ValueFields
            End Get
            Set
                Me.m_ValueFields = value
            End Set
        End Property
        
        Function IComparer_Compare(ByVal x As DimensionInfo, ByVal y As DimensionInfo) As Integer Implements IComparer(Of DimensionInfo).Compare
            Try 
                Dim result As Integer = 0
                'sort by dimension fields
                Dim i As Integer = 0
                Do While (i < CompareFields.Count)
                    Dim dir As SortDirection = CompareFields(i).SortDirection
                    Dim mode As Boolean = CompareFields(i).SortByValue
                    If Not ((dir = SortDirection.None)) Then
                        Dim xValue As Object = Nothing
                        Dim yValue As Object = Nothing
                        If Not (mode) Then
                            xValue = x.Key.Split(Global.Microsoft.VisualBasic.ChrW(124))(i)
                            yValue = y.Key.Split(Global.Microsoft.VisualBasic.ChrW(124))(i)
                        Else
                            xValue = x.Values(i).Serialize()
                            yValue = y.Values(i).Serialize()
                        End If
                        If ((xValue Is Nothing) OrElse (yValue Is Nothing)) Then
                            Return 0
                        End If
                        If Not ((xValue = yValue)) Then
                            result = CType(xValue,IComparable).CompareTo(CType(yValue,IComparable))
                            If (dir = SortDirection.Descending) Then
                                result = (result * -1)
                            End If
                            If Not ((result = 0)) Then
                                Return result
                            End If
                        End If
                    End If
                    i = (i + 1)
                Loop
                'sort by value fields
                Dim j As Integer = 0
                Do While (j < ValueFields.Count)
                    Dim dir As SortDirection = ValueFields(j).SortDirection
                    Dim mode As Boolean = ValueFields(j).SortByValue
                    If Not ((dir = SortDirection.None)) Then
                        Dim xValue As Object = Nothing
                        Dim yValue As Object = Nothing
                        If Not (mode) Then
                            xValue = x.Key.Split(Global.Microsoft.VisualBasic.ChrW(124))(j)
                            yValue = y.Key.Split(Global.Microsoft.VisualBasic.ChrW(124))(j)
                        Else
                            xValue = x.Values(j).Serialize()
                            yValue = y.Values(j).Serialize()
                        End If
                        If ((xValue Is Nothing) OrElse (yValue Is Nothing)) Then
                            Return 0
                        End If
                        If Not ((xValue = yValue)) Then
                            result = CType(xValue,IComparable).CompareTo(CType(yValue,IComparable))
                            If (dir = SortDirection.Descending) Then
                                result = (result * -1)
                            End If
                            If Not ((result = 0)) Then
                                Return result
                            End If
                        End If
                    End If
                    j = (j + 1)
                Loop
            Catch __exception As Exception
            End Try
            Return x.Key.CompareTo(y.Key)
        End Function
    End Class
End Namespace

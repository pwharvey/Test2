Imports MyCompany.Handlers
Imports MyCompany.Services
Imports MyCompany.Web
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Configuration
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Net.Mail
Imports System.Reflection
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Web
Imports System.Web.Caching
Imports System.Xml
Imports System.Xml.XPath

Namespace MyCompany.Data
    
    Public Enum ActionPhase
        
        Execute
        
        Before
        
        After
    End Enum
    
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=true, Inherited:=true)>  _
    Public Class OverrideWhenAttribute
        Inherits Attribute
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Controller As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_View As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_VirtualView As String
        
        Public Sub New(ByVal controller As String, ByVal view As String, ByVal virtualView As String)
            MyBase.New
            m_Controller = controller
            m_View = view
            m_VirtualView = virtualView
        End Sub
        
        Public Property Controller() As String
            Get
                Return Me.m_Controller
            End Get
            Set
                Me.m_Controller = value
            End Set
        End Property
        
        Public Property View() As String
            Get
                Return Me.m_View
            End Get
            Set
                Me.m_View = value
            End Set
        End Property
        
        Public Property VirtualView() As String
            Get
                Return Me.m_VirtualView
            End Get
            Set
                Me.m_VirtualView = value
            End Set
        End Property
    End Class
    
    ''' <summary>
    ''' Specifies the data controller, view, action command name, and other parameters that will cause execution of the method.
    ''' Method arguments will have a value if the argument name is matched to a field value passed from the client.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=true, Inherited:=true)>  _
    Public Class ControllerActionAttribute
        Inherits Attribute
        
        Private m_CommandName As String
        
        Private m_CommandArgument As String
        
        Private m_Controller As String
        
        Private m_View As String
        
        Private m_Phase As ActionPhase
        
        Public Sub New(ByVal controller As String, ByVal commandName As String, ByVal commandArgument As String)
            Me.New(controller, Nothing, commandName, commandArgument, ActionPhase.Execute)
        End Sub
        
        Public Sub New(ByVal controller As String, ByVal commandName As String, ByVal phase As ActionPhase)
            Me.New(controller, Nothing, commandName, phase)
        End Sub
        
        Public Sub New(ByVal controller As String, ByVal view As String, ByVal commandName As String, ByVal phase As ActionPhase)
            Me.New(controller, view, commandName, String.Empty, phase)
        End Sub
        
        Public Sub New(ByVal controller As String, ByVal view As String, ByVal commandName As String, ByVal commandArgument As String, ByVal phase As ActionPhase)
            MyBase.New
            Me.m_Controller = controller
            Me.m_View = view
            Me.m_CommandName = commandName
            Me.m_CommandArgument = commandArgument
            Me.m_Phase = phase
        End Sub
        
        Public ReadOnly Property CommandName() As String
            Get
                Return m_CommandName
            End Get
        End Property
        
        Public ReadOnly Property CommandArgument() As String
            Get
                Return m_CommandArgument
            End Get
        End Property
        
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
        
        Public ReadOnly Property Phase() As ActionPhase
            Get
                Return m_Phase
            End Get
        End Property
    End Class
    
    Public Enum RowKind
        
        [New]
        
        Existing
    End Enum
    
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=true)>  _
    Public Class RowBuilderAttribute
        Inherits Attribute
        
        Private m_Controller As String
        
        Private m_View As String
        
        Private m_Kind As RowKind
        
        Public Sub New(ByVal controller As String, ByVal kind As RowKind)
            Me.New(controller, Nothing, kind)
        End Sub
        
        Public Sub New(ByVal controller As String, ByVal view As String, ByVal kind As RowKind)
            MyBase.New
            Me.m_Controller = controller
            Me.m_View = view
            Me.m_Kind = kind
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
        
        Public ReadOnly Property Kind() As RowKind
            Get
                Return m_Kind
            End Get
        End Property
    End Class
    
    Public Enum RowFilterOperation
        
        None
        
        Equals
        
        DoesNotEqual
        
        Equal
        
        NotEqual
        
        LessThan
        
        LessThanOrEqual
        
        GreaterThan
        
        GreaterThanOrEqual
        
        Between
        
        [Like]
        
        Contains
        
        BeginsWith
        
        Includes
        
        DoesNotInclude
        
        DoesNotBeginWith
        
        DoesNotContain
        
        EndsWith
        
        DoesNotEndWith
        
        [True]
        
        [False]
        
        Tomorrow
        
        Today
        
        Yesterday
        
        NextWeek
        
        ThisWeek
        
        LastWeek
        
        NextMonth
        
        ThisMonth
        
        LastMonth
        
        NextQuarter
        
        ThisQuarter
        
        LastQuarter
        
        NextYear
        
        ThisYear
        
        YearToDate
        
        LastYear
        
        Past
        
        Future
        
        Quarter1
        
        Quarter2
        
        Quarter3
        
        Quarter4
        
        Month1
        
        Month2
        
        Month3
        
        Month4
        
        Month5
        
        Month6
        
        Month7
        
        Month8
        
        Month9
        
        Month10
        
        Month11
        
        Month12
    End Enum
    
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=true)>  _
    Public Class RowFilterAttribute
        Inherits Attribute
        
        Public Shared ComparisonOperations() As String = New String() {String.Empty, "=", "<>", "=", "<>", "<", "<=", ">", ">=", "$between$", "*", "$contains$", "$beginswith$", "$in$", "$notin$", "$doesnotbeginwith$", "$doesnotcontain$", "$endswith$", "$doesnotendwith$", "$true$", "$false$", "$tomorrow$", "$today$", "$yesterday$", "$nextweek$", "$thisweek$", "$nextmonth$", "$thismonth$", "$lastmonth$", "$nextquarter$", "$thisquarter$", "$lastquarter$", "$nextyear$", "$thisyear$", "$yeartodate$", "$lastyear$", "$past$", "$future$", "$quarter1$", "$quarter2$", "$quarter3$", "$quarter4$", "$month1$", "$month2$", "$month3$", "$month4$", "$month5$", "$month6$", "$month7$", "$month8$", "$month9$", "$month10$", "$month11$", "$month12$"}
        
        Private m_Controller As String
        
        Private m_View As String
        
        Private m_FieldName As String
        
        Private m_Operation As RowFilterOperation
        
        Public Sub New(ByVal controller As String, ByVal view As String)
            Me.New(controller, view, Nothing)
        End Sub
        
        Public Sub New(ByVal controller As String, ByVal view As String, ByVal fieldName As String)
            Me.New(controller, view, fieldName, RowFilterOperation.Equal)
        End Sub
        
        Public Sub New(ByVal controller As String, ByVal view As String, ByVal fieldName As String, ByVal operation As RowFilterOperation)
            MyBase.New
            Me.m_Controller = controller
            Me.m_View = view
            Me.m_FieldName = fieldName
            m_Operation = operation
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
        
        Public ReadOnly Property FieldName() As String
            Get
                Return m_FieldName
            End Get
        End Property
        
        Public ReadOnly Property Operation() As RowFilterOperation
            Get
                Return m_Operation
            End Get
        End Property
        
        Public Function OperationAsText() As String
            Return ComparisonOperations(Convert.ToInt32(Operation))
        End Function
    End Class
    
    Public Class ParameterValue
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Name As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Value As Object
        
        Public Sub New(ByVal name As String, ByVal value As Object)
            MyBase.New
            Me.Name = name
            Me.Value = value
        End Sub
        
        Public Property Name() As String
            Get
                Return Me.m_Name
            End Get
            Set
                Me.m_Name = value
            End Set
        End Property
        
        Public Property Value() As Object
            Get
                Return Me.m_Value
            End Get
            Set
                Me.m_Value = value
            End Set
        End Property
    End Class
    
    Public Class FilterValue
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_FilterOperation As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Name As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Values As List(Of Object)
        
        Public Sub New(ByVal fieldName As String, ByVal operation As RowFilterOperation)
            Me.New(fieldName, operation, DBNull.Value)
        End Sub
        
        Public Sub New(ByVal fieldName As String, ByVal operation As RowFilterOperation, ByVal ParamArray value() as System.[Object])
            Me.New(fieldName, RowFilterAttribute.ComparisonOperations(CType(operation,Integer)), value)
        End Sub
        
        Public Sub New(ByVal fieldName As String, ByVal operation As String, ByVal value As Object)
            MyBase.New
            Me.m_Name = fieldName
            Me.m_FilterOperation = operation
            m_Values = New List(Of Object)()
            If ((Not (value) Is Nothing) AndAlso (GetType(System.Collections.IEnumerable).IsInstanceOfType(value) AndAlso Not (GetType(String).IsInstanceOfType(value)))) Then
                m_Values.AddRange(CType(value,IEnumerable(Of Object)))
            Else
                m_Values.Add(value)
            End If
        End Sub
        
        Public ReadOnly Property FilterOperation() As RowFilterOperation
            Get
                Dim index As Integer = Array.IndexOf(RowFilterAttribute.ComparisonOperations, m_FilterOperation)
                If (index = -1) Then
                    index = 0
                End If
                Return CType(index,RowFilterOperation)
            End Get
        End Property
        
        Public ReadOnly Property Name() As String
            Get
                If (Me.m_FilterOperation = "~") Then
                    Return String.Empty
                End If
                Return m_Name
            End Get
        End Property
        
        Public ReadOnly Property Value() As Object
            Get
                If (m_Values Is Nothing) Then
                    Return Nothing
                End If
                Return Values(0)
            End Get
        End Property
        
        Public ReadOnly Property Values() As Object()
            Get
                Return Me.m_Values.ToArray()
            End Get
        End Property
        
        Public Sub AddValue(ByVal value As Object)
            m_Values.Add(value)
        End Sub
        
        Public Sub Clear()
            m_Values.Clear()
        End Sub
    End Class
    
    Public Class RowFilterContext
        
        Private m_Controller As String
        
        Private m_View As String
        
        Private m_LookupContextController As String
        
        Private m_LookupContextView As String
        
        Private m_LookupContextFieldName As String
        
        Private m_Canceled As Boolean
        
        Public Sub New(ByVal controller As String, ByVal view As String, ByVal lookupContextController As String, ByVal lookupContextView As String, ByVal lookupContextFieldName As String)
            MyBase.New
            Me.Controller = controller
            Me.View = view
            Me.LookupContextController = lookupContextController
            Me.LookupContextView = lookupContextView
            Me.LookupContextFieldName = lookupContextFieldName
        End Sub
        
        Public Property Controller() As String
            Get
                Return m_Controller
            End Get
            Set
                m_Controller = value
            End Set
        End Property
        
        Public Property View() As String
            Get
                Return m_View
            End Get
            Set
                m_View = value
            End Set
        End Property
        
        Public Property LookupContextController() As String
            Get
                Return m_LookupContextController
            End Get
            Set
                m_LookupContextController = value
            End Set
        End Property
        
        Public Property LookupContextView() As String
            Get
                Return m_LookupContextView
            End Get
            Set
                m_LookupContextView = value
            End Set
        End Property
        
        Public Property LookupContextFieldName() As String
            Get
                Return m_LookupContextFieldName
            End Get
            Set
                m_LookupContextFieldName = value
            End Set
        End Property
        
        Public Property Canceled() As Boolean
            Get
                Return m_Canceled
            End Get
            Set
                m_Canceled = value
            End Set
        End Property
    End Class
    
    Public Enum AccessPermission
        
        Allow
        
        Deny
    End Enum
    
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=true)>  _
    Public Class AccessControlAttribute
        Inherits Attribute
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Controller As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_FieldName As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Permission As AccessPermission
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Sql As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Parameters As List(Of SqlParam)
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Restrictions As List(Of Object)
        
        Public Sub New(ByVal fieldName As String)
            Me.New(String.Empty, fieldName)
        End Sub
        
        Public Sub New(ByVal fieldName As String, ByVal permission As AccessPermission)
            Me.New(String.Empty, fieldName, permission)
        End Sub
        
        Public Sub New(ByVal controller As String, ByVal fieldName As String)
            Me.New(controller, fieldName, AccessPermission.Allow)
        End Sub
        
        Public Sub New(ByVal controller As String, ByVal fieldName As String, ByVal permission As AccessPermission)
            Me.New(controller, fieldName, String.Empty, permission)
        End Sub
        
        Public Sub New(ByVal controller As String, ByVal fieldName As String, ByVal sql As String)
            Me.New(controller, fieldName, sql, AccessPermission.Allow)
        End Sub
        
        Public Sub New(ByVal controller As String, ByVal fieldName As String, ByVal sql As String, ByVal permission As AccessPermission)
            MyBase.New
            Me.m_Controller = controller
            Me.m_FieldName = fieldName
            Me.m_Permission = permission
            Me.m_Sql = sql
        End Sub
        
        Public Property Controller() As String
            Get
                Return Me.m_Controller
            End Get
            Set
                Me.m_Controller = value
            End Set
        End Property
        
        Public Property FieldName() As String
            Get
                Return Me.m_FieldName
            End Get
            Set
                Me.m_FieldName = value
            End Set
        End Property
        
        Public Property Permission() As AccessPermission
            Get
                Return Me.m_Permission
            End Get
            Set
                Me.m_Permission = value
            End Set
        End Property
        
        Public Property Sql() As String
            Get
                Return Me.m_Sql
            End Get
            Set
                Me.m_Sql = value
            End Set
        End Property
        
        Public Property Parameters() As List(Of SqlParam)
            Get
                Return Me.m_Parameters
            End Get
            Set
                Me.m_Parameters = value
            End Set
        End Property
        
        Public Property Restrictions() As List(Of Object)
            Get
                Return Me.m_Restrictions
            End Get
            Set
                Me.m_Restrictions = value
            End Set
        End Property
    End Class
    
    Public Class AccessControlRule
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_AccessControl As AccessControlAttribute
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Method As MethodInfo
        
        Public Sub New(ByVal accessControl As AccessControlAttribute, ByVal method As MethodInfo)
            MyBase.New
            Me.m_AccessControl = accessControl
            Me.m_Method = method
        End Sub
        
        Public Property AccessControl() As AccessControlAttribute
            Get
                Return Me.m_AccessControl
            End Get
            Set
                Me.m_AccessControl = value
            End Set
        End Property
        
        Public Property Method() As MethodInfo
            Get
                Return Me.m_Method
            End Get
            Set
                Me.m_Method = value
            End Set
        End Property
    End Class
    
    Public Class AccessControlRuleDictionary
        Inherits SortedDictionary(Of String, List(Of AccessControlRule))
    End Class
    
    Public Class DynamicAccessControlList
        Inherits List(Of DynamicAccessControlRule)
        
        Public Shared RuleRegex As Regex = New Regex("(?'ParamList'^(\s*([\w\-]+)\s*(\:|\=)\s*(.+?)\n)+)", RegexOptions.Multiline)
        
        Public Shared ParamRegex As Regex = New Regex("^(?'Name'[\w\-]+)\s*(\:|\=)\s*(?'Value'.+)$", RegexOptions.Multiline)
        
        Public Overridable Sub Parse(ByVal fileName As String, ByVal rules As String)
            Dim parameters As SortedDictionary(Of String, String) = New SortedDictionary(Of String, String)()
            Dim ruleMatch As Match = RuleRegex.Match(rules)
            Do While ruleMatch.Success
                parameters.Clear()
                Dim paramMatch As Match = ParamRegex.Match(ruleMatch.Groups("ParamList").Value)
                Do While paramMatch.Success
                    parameters(paramMatch.Groups("Name").Value.ToLower().Replace("-", String.Empty)) = paramMatch.Groups("Value").Value.Trim()
                    paramMatch = paramMatch.NextMatch()
                Loop
                Dim v As String = Nothing
                If parameters.TryGetValue("field", v) Then
                    Dim r As DynamicAccessControlRule = New DynamicAccessControlRule()
                    r.Field = v
                    If parameters.TryGetValue("controller", v) Then
                        r.Controller = v
                    End If
                    If parameters.TryGetValue("tags", v) Then
                        r.Tags = BusinessRules.ListRegex.Split(v)
                    End If
                    If parameters.TryGetValue("roles", v) Then
                        r.Roles = BusinessRules.ListRegex.Split(v)
                    End If
                    If parameters.TryGetValue("roleexceptions", v) Then
                        r.RoleExceptions = BusinessRules.ListRegex.Split(v)
                    End If
                    If parameters.TryGetValue("users", v) Then
                        r.Users = BusinessRules.ListRegex.Split(v)
                    End If
                    If parameters.TryGetValue("userexceptions", v) Then
                        r.UserExceptions = BusinessRules.ListRegex.Split(v)
                    End If
                    parameters.TryGetValue("type", v)
                    Dim index As Integer = (ruleMatch.Index + ruleMatch.Length)
                    Dim nextIndex As Integer = rules.Length
                    ruleMatch = ruleMatch.NextMatch()
                    If ruleMatch.Success Then
                        nextIndex = ruleMatch.Index
                    End If
                    Dim sql As String = rules.Substring(index, (nextIndex - index)).Trim()
                    If "deny".Equals(v, StringComparison.OrdinalIgnoreCase) Then
                        r.DenySql = sql
                    Else
                        r.AllowSql = sql
                    End If
                    Add(r)
                Else
                    ruleMatch = ruleMatch.NextMatch()
                End If
            Loop
        End Sub
    End Class
    
    Public Class DynamicAccessControlRule
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Field As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Controller As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Tags() As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Roles() As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_RoleExceptions() As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Users() As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_UserExceptions() As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_AllowSql As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_DenySql As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_FileName As String
        
        Public Overridable Property Field() As String
            Get
                Return Me.m_Field
            End Get
            Set
                Me.m_Field = value
            End Set
        End Property
        
        Public Overridable Property Controller() As String
            Get
                Return Me.m_Controller
            End Get
            Set
                Me.m_Controller = value
            End Set
        End Property
        
        Public Overridable Property Tags() As String()
            Get
                Return Me.m_Tags
            End Get
            Set
                Me.m_Tags = value
            End Set
        End Property
        
        Public Overridable Property Roles() As String()
            Get
                Return Me.m_Roles
            End Get
            Set
                Me.m_Roles = value
            End Set
        End Property
        
        Public Overridable Property RoleExceptions() As String()
            Get
                Return Me.m_RoleExceptions
            End Get
            Set
                Me.m_RoleExceptions = value
            End Set
        End Property
        
        Public Overridable Property Users() As String()
            Get
                Return Me.m_Users
            End Get
            Set
                Me.m_Users = value
            End Set
        End Property
        
        Public Overridable Property UserExceptions() As String()
            Get
                Return Me.m_UserExceptions
            End Get
            Set
                Me.m_UserExceptions = value
            End Set
        End Property
        
        Public Overridable Property AllowSql() As String
            Get
                Return Me.m_AllowSql
            End Get
            Set
                Me.m_AllowSql = value
            End Set
        End Property
        
        Public Overridable Property DenySql() As String
            Get
                Return Me.m_DenySql
            End Get
            Set
                Me.m_DenySql = value
            End Set
        End Property
        
        Public Overridable Property FileName() As String
            Get
                Return Me.m_FileName
            End Get
            Set
                Me.m_FileName = value
            End Set
        End Property
        
        Public Overrides Function ToString() As String
            Dim mode As String = "allow"
            Dim sql As String = AllowSql
            If String.IsNullOrEmpty(AllowSql) Then
                sql = DenySql
                mode = "deny"
            End If
            Dim trigger As String = Controller
            If Not (String.IsNullOrEmpty(trigger)) Then
                trigger = (trigger + ".")
            End If
            trigger = (trigger + Field)
            Return String.Format("{0} ({1}) -> {2}", trigger, mode, sql.Trim())
        End Function
    End Class
    
    Partial Public Class BusinessRules
        Inherits BusinessRulesBase
        
        Public Shared ListRegex As Regex = New Regex("\s*,\s*")
    End Class
    
    Public Class BusinessRulesBase
        Inherits ActionHandlerBase
        Implements MyCompany.Data.IRowHandler, MyCompany.Data.IDataFilter, MyCompany.Data.IDataFilter2
        
        Private m_NewRow() As MethodInfo
        
        Private m_PrepareRow() As MethodInfo
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_EnableResultSet As Boolean
        
        Private m_ResultSet As DataTable
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_ResultSetSize As Integer
        
        Private m_ResultSetCacheDuration As Integer
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_EnableEmailMessages As Boolean
        
        Private m_EmailMessages As DataTable
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Config As ControllerConfiguration
        
        Private m_ControllerName As String
        
        Private m_Row() As Object
        
        Private m_Request As PageRequest
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Page As ViewPage
        
        Private m_RowFilter As RowFilterContext
        
        Private m_ApplyAccessControlRule As Boolean = false
        
        Private m_AccessControlRestrictions As List(Of Object)
        
        Private m_AccessControlCommand As DbCommand
        
        Private m_DynamicAccessControlRules As AccessControlRuleDictionary
        
        Private m_Expressions As SelectClauseDictionary
        
        Public Shared FieldNameRegex As Regex = New Regex("\[(\w+)\]")
        
        Private m_SqlIsValid As Boolean
        
        Public Shared SelectDetectionRegex As Regex = New Regex("^\s*select\s+", RegexOptions.IgnoreCase)
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Navigator As XPathNavigator
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Resolver As XmlNamespaceManager
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_EnableDccTest As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_PendingAlterations As SiteContentFileList
        
        Public Shared AlterMethodRegex As Regex = New Regex("\s*(?'Method'[\w\-]+)\s*\((?'Parameters'[\s\S]*?)\)\s*(?'Terminator'\.|;|\$)")
        
        Public Shared AlterParametersRegex As Regex = New Regex("\s*""(?'Argument'[\s\S]*?)""(\s*(,|$))")
        
        Public Shared TestPendingAlterRegex As Regex = New Regex("\b(when\-?(view|test|sql))\b", RegexOptions.IgnoreCase)
        
        Private m_UserEmail As String
        
        Private m_RequestFilter() As String
        
        Private m_RequestExternalFilter() As FieldValue
        
        Public Shared SqlFieldFilterOperationRegex As Regex = New Regex("^(?'Name'\w+?)_Filter_((?'Operation'\w+?)(?'Index'\d*))?$")
        
        Public Shared SystemSqlParameters() As String = New String() {"BusinessRules_PreventDefault", "Result_Continue", "Result_Refresh", "Result_RefreshChildren", "Result_ShowAlert", "Result_ShowMessage", "Result_ShowViewMessage", "Result_Focus", "Result_Error", "Result_ExecuteOnClient", "Result_NavigateUrl"}
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_RequiresRowCount As Boolean
        
        Public SystemSqlPropertyRegex As Regex = New Regex("^(BusinessRules|Session|Url|Arguments)_")
        
        Private m_ActionParameters As SortedDictionary(Of String, String)
        
        Private m_ActionParametersData As String
        
        Public Property EnableResultSet() As Boolean
            Get
                Return Me.m_EnableResultSet
            End Get
            Set
                Me.m_EnableResultSet = value
            End Set
        End Property
        
        Public Property ResultSet() As DataTable
            Get
                Return m_ResultSet
            End Get
            Set
                Me.m_ResultSet = value
                EnableResultSet = true
            End Set
        End Property
        
        Public Property ResultSetSize() As Integer
            Get
                Return Me.m_ResultSetSize
            End Get
            Set
                Me.m_ResultSetSize = value
            End Set
        End Property
        
        Public Property ResultSetCacheDuration() As Integer
            Get
                Return m_ResultSetCacheDuration
            End Get
            Set
                Me.m_ResultSetCacheDuration = value
                EnableResultSet = true
            End Set
        End Property
        
        Public Property EnableEmailMessages() As Boolean
            Get
                Return Me.m_EnableEmailMessages
            End Get
            Set
                Me.m_EnableEmailMessages = value
            End Set
        End Property
        
        Public Property EmailMessages() As DataTable
            Get
                Return m_EmailMessages
            End Get
            Set
                EnableEmailMessages = true
                m_EmailMessages = value
                If (Not (value) Is Nothing) Then
                    For Each message As DataRow in value.Rows
                        Email(message)
                    Next
                End If
                EnableEmailMessages = false
            End Set
        End Property
        
        Public Property Config() As ControllerConfiguration
            Get
                Return Me.m_Config
            End Get
            Set
                Me.m_Config = value
            End Set
        End Property
        
        Public Property ControllerName() As String
            Get
                Return m_ControllerName
            End Get
            Set
                m_ControllerName = value
            End Set
        End Property
        
        Public ReadOnly Property Row() As Object()
            Get
                Return m_Row
            End Get
        End Property
        
        Public ReadOnly Property Request() As PageRequest
            Get
                Return m_Request
            End Get
        End Property
        
        Public Property Page() As ViewPage
            Get
                Return Me.m_Page
            End Get
            Set
                Me.m_Page = value
            End Set
        End Property
        
        Protected ReadOnly Property Context() As System.Web.HttpContext
            Get
                Return System.Web.HttpContext.Current
            End Get
        End Property
        
        Public ReadOnly Property RowFilter() As RowFilterContext
            Get
                Return m_RowFilter
            End Get
        End Property
        
        Public ReadOnly Property LookupContextController() As String
            Get
                If (Not (PageRequest.Current) Is Nothing) Then
                    Return PageRequest.Current.LookupContextController
                End If
                If (Not (DistinctValueRequest.Current) Is Nothing) Then
                    Return DistinctValueRequest.Current.LookupContextController
                End If
                Return Nothing
            End Get
        End Property
        
        Public ReadOnly Property LookupContextView() As String
            Get
                If (Not (PageRequest.Current) Is Nothing) Then
                    Return PageRequest.Current.LookupContextView
                End If
                If (Not (DistinctValueRequest.Current) Is Nothing) Then
                    Return DistinctValueRequest.Current.LookupContextView
                End If
                Return Nothing
            End Get
        End Property
        
        Public ReadOnly Property LookupContextFieldName() As String
            Get
                If (Not (PageRequest.Current) Is Nothing) Then
                    Return PageRequest.Current.LookupContextFieldName
                End If
                If (Not (DistinctValueRequest.Current) Is Nothing) Then
                    Return DistinctValueRequest.Current.LookupContextFieldName
                End If
                Return Nothing
            End Get
        End Property
        
        Protected Property Navigator() As XPathNavigator
            Get
                Return Me.m_Navigator
            End Get
            Set
                Me.m_Navigator = value
            End Set
        End Property
        
        Protected Property Resolver() As XmlNamespaceManager
            Get
                Return Me.m_Resolver
            End Get
            Set
                Me.m_Resolver = value
            End Set
        End Property
        
        Public Property EnableDccTest() As Boolean
            Get
                Return Me.m_EnableDccTest
            End Get
            Set
                Me.m_EnableDccTest = value
            End Set
        End Property
        
        Public Property PendingAlterations() As SiteContentFileList
            Get
                Return Me.m_PendingAlterations
            End Get
            Set
                Me.m_PendingAlterations = value
            End Set
        End Property
        
        Protected Property TagList() As String()
            Get
                Dim t As String = Tags
                If String.IsNullOrEmpty(t) Then
                    t = String.Empty
                End If
                Return t.Split(New Char() {Global.Microsoft.VisualBasic.ChrW(44)}, StringSplitOptions.RemoveEmptyEntries)
            End Get
            Set
                Dim sb As StringBuilder = New StringBuilder()
                If (Not (value) Is Nothing) Then
                    For Each s As String in value
                        If (sb.Length > 0) Then
                            sb.Append(",")
                        End If
                        sb.Append(s)
                    Next
                End If
                Tags = sb.ToString()
            End Set
        End Property
        
        Public Shared ReadOnly Property UserName() As String
            Get
                Return System.Web.HttpContext.Current.User.Identity.Name
            End Get
        End Property
        
        Public Overridable Property UserEmail() As String
            Get
                If Not (String.IsNullOrEmpty(m_UserEmail)) Then
                    Return m_UserEmail
                End If
                If Not ((System.Web.HttpContext.Current.User.Identity.GetType() Is GetType(System.Security.Principal.WindowsIdentity))) Then
                    Return System.Web.Security.Membership.GetUser().Email
                End If
                Return Nothing
            End Get
            Set
                m_UserEmail = value
            End Set
        End Property
        
        Public Shared ReadOnly Property UserId() As Object
            Get
                If (System.Web.HttpContext.Current.User.Identity.GetType() Is GetType(System.Security.Principal.WindowsIdentity)) Then
                    Return System.Security.Principal.WindowsIdentity.GetCurrent().User.Value
                Else
                    Return System.Web.Security.Membership.GetUser().ProviderUserKey
                End If
            End Get
        End Property
        
        Public ReadOnly Property QuickFindFilter() As String
            Get
                If (Not (Me.m_RequestFilter) Is Nothing) Then
                    For Each filterExpression As String in Me.m_RequestFilter
                        Dim filterMatch As Match = Controller.FilterExpressionRegex.Match(filterExpression)
                        If filterMatch.Success Then
                            Dim valueMatch As Match = Controller.FilterValueRegex.Match(filterMatch.Groups("Values").Value)
                            If (valueMatch.Success AndAlso (valueMatch.Groups("Operation").Value = "~")) Then
                                Return Convert.ToString(Controller.StringToValue(valueMatch.Groups("Value").Value))
                            End If
                        End If
                    Next
                End If
                Return Nothing
            End Get
        End Property
        
        Public Property Tags() As String
            Get
                If (Not (Page) Is Nothing) Then
                    Return Page.Tag
                End If
                If (Not (Arguments) Is Nothing) Then
                    If (Result.Tag Is Nothing) Then
                        Result.Tag = Arguments.Tag
                    End If
                    Return Result.Tag
                End If
                If (Not (DistinctValueRequest.Current) Is Nothing) Then
                    Return DistinctValueRequest.Current.Tag
                End If
                If (Not (PageRequest.Current) Is Nothing) Then
                    Return PageRequest.Current.Tag
                End If
                Return Nothing
            End Get
            Set
                If (Not (Page) Is Nothing) Then
                    Page.Tag = value
                Else
                    If (Not (Result) Is Nothing) Then
                        Result.Tag = value
                    End If
                End If
            End Set
        End Property
        
        ''' <summary>
        ''' Specfies if the the currently processed "Select" action must calculate the number of available data rows.
        ''' </summary>
        Public Property RequiresRowCount() As Boolean
            Get
                Return Me.m_RequiresRowCount
            End Get
            Set
                Me.m_RequiresRowCount = value
            End Set
        End Property
        
        ''' <summary>
        ''' Returns the name of the View that was active when the currently processed action has been invoked.
        ''' </summary>
        Public ReadOnly Property View() As String
            Get
                If (Not (m_Request) Is Nothing) Then
                    Return m_Request.View
                End If
                If (Not (Arguments) Is Nothing) Then
                    Return Arguments.View
                End If
                Return Nothing
            End Get
        End Property
        
        Public ReadOnly Property ActionParameters() As SortedDictionary(Of String, String)
            Get
                If (m_ActionParameters Is Nothing) Then
                    m_ActionParameters = New SortedDictionary(Of String, String)()
                    Dim data As String = m_ActionParametersData
                    If String.IsNullOrEmpty(data) Then
                        data = ActionData
                    End If
                    If Not (String.IsNullOrEmpty(data)) Then
                        data = ReplaceFieldNamesWithValues(Regex.Replace(data, "^(?'Name'[\w-]+)\s*:\s*(?'Value'.+?)\s*$", AddressOf DoReplaceActionParameter, RegexOptions.Multiline))
                        m_ActionParameters.Add(String.Empty, data.Trim())
                    End If
                End If
                Return m_ActionParameters
            End Get
        End Property
        
        ''' <summary>
        ''' The value of the 'Data' property of the currently processed action as defined in the data controller.
        ''' </summary>
        Public ReadOnly Property ActionData() As String
            Get
                If (Not (Arguments) Is Nothing) Then
                    Return Config.ReadActionData(Arguments.Path)
                End If
                Return Nothing
            End Get
        End Property
        
        Public Overridable Function Localize(ByVal token As String, ByVal text As String) As String
            Return Localizer.Replace("Controllers", (ControllerName + ".xml"), token, text)
        End Function
        
        Public Function IsOverrideApplicable(ByVal controller As String, ByVal view As String, ByVal virtualView As String) As Boolean
            For Each p As PropertyInfo in [GetType]().GetProperties(((BindingFlags.Public Or BindingFlags.NonPublic) Or BindingFlags.Instance))
                For Each filter As OverrideWhenAttribute in p.GetCustomAttributes(GetType(OverrideWhenAttribute), true)
                    If (((filter.Controller = controller) AndAlso (filter.View = view)) AndAlso (filter.VirtualView = virtualView)) Then
                        Dim v As Object = p.GetValue(Me, Nothing)
                        Return (TypeOf v Is Boolean AndAlso CType(v,Boolean))
                    End If
                Next
            Next
            Return false
        End Function
        
        Private Function FindRowHandler(ByVal request As PageRequest, ByVal kind As RowKind) As MethodInfo()
            Dim list As List(Of MethodInfo) = New List(Of MethodInfo)()
            For Each method As MethodInfo in [GetType]().GetMethods((BindingFlags.Public Or (BindingFlags.NonPublic Or BindingFlags.Instance)))
                For Each filter As RowBuilderAttribute in method.GetCustomAttributes(GetType(RowBuilderAttribute), true)
                    If (filter.Kind = kind) Then
                        If (((request.Controller = filter.Controller) OrElse Regex.IsMatch(request.Controller, filter.Controller)) AndAlso (String.IsNullOrEmpty(filter.View) OrElse (request.View = filter.View))) Then
                            list.Add(method)
                        End If
                    End If
                Next
            Next
            Return list.ToArray()
        End Function
        
        Function IRowHandler_SupportsNewRow(ByVal request As PageRequest) As Boolean Implements IRowHandler.SupportsNewRow
            m_NewRow = FindRowHandler(request, RowKind.New)
            Return (m_NewRow.Length > 0)
        End Function
        
        Sub IRowHandler_NewRow(ByVal request As PageRequest, ByVal page As ViewPage, ByVal row() As Object) Implements IRowHandler.NewRow
            If (Not (m_NewRow) Is Nothing) Then
                Me.m_Request = request
                Me.m_Page = page
                Me.m_Row = row
                For Each method As MethodInfo in m_NewRow
                    method.Invoke(Me, New Object(-1) {})
                Next
            End If
        End Sub
        
        Function IRowHandler_SupportsPrepareRow(ByVal request As PageRequest) As Boolean Implements IRowHandler.SupportsPrepareRow
            m_PrepareRow = FindRowHandler(request, RowKind.Existing)
            Return (m_PrepareRow.Length > 0)
        End Function
        
        Sub IRowHandler_PrepareRow(ByVal request As PageRequest, ByVal page As ViewPage, ByVal row() As Object) Implements IRowHandler.PrepareRow
            If (Not (m_PrepareRow) Is Nothing) Then
                Me.m_Request = request
                Me.m_Page = page
                Me.m_Row = row
                For Each method As MethodInfo in m_PrepareRow
                    method.Invoke(Me, New Object(-1) {})
                Next
            End If
        End Sub
        
        Public Overridable Sub ProcessPageRequest(ByVal request As PageRequest, ByVal page As ViewPage)
        End Sub
        
        Public Shared Function ValueToList(ByVal v As String) As List(Of String)
            If String.IsNullOrEmpty(v) Then
                Return New List(Of String)()
            End If
            Return New List(Of String)(v.Split(Global.Microsoft.VisualBasic.ChrW(44)))
        End Function
        
        Public Overloads Function SelectFieldValue(ByVal name As String) As Object
            Return SelectFieldValue(name, true)
        End Function
        
        Public Overloads Shared Function ListsAreEqual(ByVal list1 As List(Of String), ByVal list2 As List(Of String)) As Boolean
            If Not ((list1.Count = list2.Count)) Then
                Return false
            End If
            For Each s As String in list1
                If Not (list2.Contains(s)) Then
                    Return false
                End If
            Next
            Return true
        End Function
        
        Public Overloads Shared Function ListsAreEqual(ByVal list1 As String, ByVal list2 As String) As Boolean
            Return ListsAreEqual(ValueToList(list1), ValueToList(list2))
        End Function
        
        Public Overloads Function SelectFieldValue(ByVal name As String, ByVal useExternalValues As Boolean) As Object
            Dim v As Object = Nothing
            If ((Not (m_Page) Is Nothing) AndAlso (Not (m_Row) Is Nothing)) Then
                v = m_Page.SelectFieldValue(name, m_Row)
            Else
                If (Not (Arguments) Is Nothing) Then
                    For Each av As FieldValue in Arguments.Values
                        If av.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) Then
                            Return av.Value
                        End If
                    Next
                End If
            End If
            If ((v Is Nothing) AndAlso useExternalValues) Then
                v = SelectExternalFilterFieldValue(name)
            End If
            Return v
        End Function
        
        Protected Overrides Function BuildingDataRows() As Boolean
            Return ((Not (m_Page) Is Nothing) AndAlso (Not (m_Row) Is Nothing))
        End Function
        
        Public Overrides Function SelectFieldValueObject(ByVal name As String) As FieldValue
            Dim result As FieldValue = Nothing
            If (Not (Me.Arguments) Is Nothing) Then
                result = Me.Arguments(name)
            End If
            If (((result Is Nothing) AndAlso BuildingDataRows()) AndAlso ((Not (Me.Request) Is Nothing) AndAlso Not (Me.Request.Inserting))) Then
                result = m_Page.SelectFieldValueObject(name, m_Row)
            End If
            If (result Is Nothing) Then
                result = SelectExternalFilterFieldValueObject(name)
            End If
            Return result
        End Function
        
        Public Sub UpdateFieldValue(ByVal name As String, ByVal value As Object)
            If DBNull.Value.Equals(value) Then
                value = Nothing
            End If
            If ((Not (m_Page) Is Nothing) AndAlso (Not (m_Row) Is Nothing)) Then
                m_Page.UpdateFieldValue(name, m_Row, value)
            Else
                If (Not (Result) Is Nothing) Then
                    Result.Values.Add(New FieldValue(name, value))
                End If
                If (Not (Arguments) Is Nothing) Then
                    Dim v As FieldValue = SelectFieldValueObject(name)
                    If (Not (v) Is Nothing) Then
                        v.NewValue = value
                        v.Modified = true
                    End If
                End If
            End If
        End Sub
        
        Public Function SelectExternalFilterFieldValue(ByVal name As String) As Object
            Dim v As FieldValue = SelectExternalFilterFieldValueObject(name)
            If (Not (v) Is Nothing) Then
                Return v.Value
            End If
            Return Nothing
        End Function
        
        Public Function SelectExternalFilterFieldValueObject(ByVal name As String) As FieldValue
            Dim values() As FieldValue = Nothing
            If (Not (Request) Is Nothing) Then
                values = Request.ExternalFilter
            Else
                If (Not (Arguments) Is Nothing) Then
                    values = Arguments.ExternalFilter
                End If
            End If
            If (values Is Nothing) Then
                values = Me.m_RequestExternalFilter
            End If
            If (Not (values) Is Nothing) Then
                Dim i As Integer = 0
                Do While (i < values.Length)
                    If values(i).Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) Then
                        Return values(i)
                    End If
                    i = (i + 1)
                Loop
            End If
            Return Nothing
        End Function
        
        Public Sub PopulateManyToManyField(ByVal fieldName As String, ByVal primaryKeyField As String, ByVal targetController As String, ByVal targetForeignKey1 As String, ByVal targetForeignKey2 As String)
            'Deprecated in 8.5.9.0. See DataControllerBase.PopulateManyToManyFields()
        End Sub
        
        Public Sub UpdateManyToManyField(ByVal fieldName As String, ByVal primaryKeyField As String, ByVal targetController As String, ByVal targetForeignKey1 As String, ByVal targetForeignKey2 As String)
            'Deprecated in 8.5.9.0. See DataControllerBase.ProcessManyToManyFields()
        End Sub
        
        Public Sub ClearManyToManyField(ByVal fieldName As String, ByVal primaryKeyField As String, ByVal targetController As String, ByVal targetForeignKey1 As String, ByVal targetForeignKey2 As String)
            'Deprecated in 8.5.9.0. See DataControllerBase.ProcessManyToManyFields()
        End Sub
        
        Sub IDataFilter_Filter(ByVal filter As SortedDictionary(Of String, Object)) Implements IDataFilter.Filter
            'do nothing
        End Sub
        
        Sub IDataFilter2_Filter(ByVal controller As String, ByVal view As String, ByVal filter As SortedDictionary(Of String, Object)) Implements IDataFilter2.Filter
            Me.Filter(controller, view, filter)
        End Sub
        
        Protected Overridable Sub Filter(ByVal controller As String, ByVal view As String, ByVal filter As SortedDictionary(Of String, Object))
            For Each p As PropertyInfo in [GetType]().GetProperties((BindingFlags.Public Or (BindingFlags.NonPublic Or BindingFlags.Instance)))
                For Each rowFilter As RowFilterAttribute in p.GetCustomAttributes(GetType(RowFilterAttribute), true)
                    If ((controller = rowFilter.Controller) AndAlso (String.IsNullOrEmpty(rowFilter.View) OrElse (view = rowFilter.View))) Then
                        Me.RowFilter.Canceled = false
                        Dim v As Object = p.GetValue(Me, Nothing)
                        Dim fieldName As String = rowFilter.FieldName
                        If String.IsNullOrEmpty(fieldName) Then
                            fieldName = p.Name
                        End If
                        If Not (Me.RowFilter.Canceled) Then
                            If (GetType(System.Collections.IEnumerable).IsInstanceOfType(v) AndAlso Not (GetType([String]).IsInstanceOfType(v))) Then
                                Dim sb As StringBuilder = New StringBuilder()
                                For Each item As Object in CType(v,System.Collections.IEnumerable)
                                    If (sb.Length > 0) Then
                                        sb.AppendFormat(rowFilter.OperationAsText())
                                    End If
                                    sb.Append(item)
                                    sb.Append(Convert.ToChar(0))
                                Next
                                v = sb.ToString()
                            End If
                            If (v Is Nothing) Then
                                v = "null"
                            End If
                            Dim filterExpression As String = String.Format("{0}{1}", rowFilter.OperationAsText(), v)
                            If Not (filter.ContainsKey(fieldName)) Then
                                filter.Add(fieldName, filterExpression)
                            Else
                                filter(fieldName) = String.Format("{0}{1}{2}", filter(fieldName), Convert.ToChar(0), filterExpression)
                            End If
                        End If
                    End If
                Next
            Next
        End Sub
        
        Sub IDataFilter2_AssignContext(ByVal controller As String, ByVal view As String, ByVal lookupContextController As String, ByVal lookupContextView As String, ByVal lookupContextFieldName As String) Implements IDataFilter2.AssignContext
            m_RowFilter = New RowFilterContext(controller, view, lookupContextController, lookupContextView, lookupContextFieldName)
        End Sub
        
        Protected Function LastEnteredValue(ByVal controller As String, ByVal fieldName As String) As Object
            If (Context Is Nothing) Then
                Return Nothing
            End If
            Dim values() As FieldValue = CType(Context.Session(String.Format("{0}$LEVs", controller)),FieldValue())
            If (Not (values) Is Nothing) Then
                For Each v As FieldValue in values
                    If v.Name.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase) Then
                        Return v.Value
                    End If
                Next
            End If
            Return Nothing
        End Function
        
        ''' <summary>
        ''' Creates a controller node set to manipulate the XML definition of data controller.
        ''' </summary>
        ''' <returns>Returns an empty node set.</returns>
        Public Overloads Function NodeSet() As ControllerNodeSet
            If (Navigator Is Nothing) Then
                Return New ControllerNodeSet(Config.Navigator, CType(Config.Resolver,XmlNamespaceManager))
            End If
            Return New ControllerNodeSet(Navigator, Resolver)
        End Function
        
        ''' <summary>
        ''' Creates a controller node set matched to XPath selector. Controller node set allows manipulating the XML definition of data controller.
        ''' </summary>
        ''' <param name="selector">XPath expression evaluated against the definition of the data controller. May contain variables.</param>
        ''' <param name="args">Optional values of variables. If variables are specified then the expression is evaluated for each variable or group of variables specified in the selector.</param>
        ''' <example>field[@name=$name]</example>
        ''' <returns>A node set containing selected data controller nodes.</returns>
        Protected Overloads Function NodeSet(ByVal selector As String, ByVal ParamArray args() as System.[String]) As ControllerNodeSet
            Return New ControllerNodeSet(Navigator, Resolver).Select(selector, args)
        End Function
        
        Protected Sub UnrestrictedAccess()
            m_ApplyAccessControlRule = false
        End Sub
        
        Protected Overridable Function UserIsInRole(ByVal ParamArray rules() as System.[String]) As Boolean
            Return DataControllerBase.UserIsInRole(rules)
        End Function
        
        Protected Overloads Sub RestrictAccess()
            m_ApplyAccessControlRule = true
        End Sub
        
        Protected Overloads Sub RestrictAccess(ByVal value As Object)
            m_AccessControlRestrictions.Add(value)
            m_ApplyAccessControlRule = true
        End Sub
        
        Protected Overloads Sub RestrictAccess(ByVal parameterName As String, ByVal value As Object)
            Dim parameter As DbParameter = Nothing
            For Each p As DbParameter in m_AccessControlCommand.Parameters
                If (p.ParameterName = parameterName) Then
                    parameter = p
                    Exit For
                End If
            Next
            If (parameter Is Nothing) Then
                parameter = m_AccessControlCommand.CreateParameter()
                parameter.ParameterName = parameterName
                m_AccessControlCommand.Parameters.Add(parameter)
            End If
            parameter.Value = value
            m_ApplyAccessControlRule = true
        End Sub
        
        Private Function CreateDynamicAccessControlAttribute(ByVal fieldName As String) As AccessControlAttribute
            If (m_DynamicAccessControlRules Is Nothing) Then
                m_DynamicAccessControlRules = New AccessControlRuleDictionary()
            End If
            Dim a As AccessControlAttribute = New AccessControlAttribute(fieldName)
            Dim attributes As List(Of AccessControlRule) = Nothing
            If Not (m_DynamicAccessControlRules.TryGetValue(fieldName, attributes)) Then
                attributes = New List(Of AccessControlRule)()
                m_DynamicAccessControlRules.Add(fieldName, attributes)
            End If
            attributes.Add(New AccessControlRule(a, Nothing))
            Return a
        End Function
        
        ''' <summary>
        ''' Registers the access control rule that will be active only while processing the current request.
        ''' </summary>
        ''' <param name="fieldName">The name of the data field that must be present in the data view for the rule to be activated.</param>
        ''' <param name="sql">The arbitrary SQL expression that will filter data. Names of the data fields can be referenced if enclosed in square brackets.</param>
        ''' <param name="permission">The permission to allow or deny access to data. Access control rules are combined according to this formula: (List of  “Allow” restrictions) and Not (List of “Deny” restrictions).</param>
        ''' <param name="parameters">Values of parameters references in SQL expression.</param>
        Protected Overloads Sub RegisterAccessControlRule(ByVal fieldName As String, ByVal sql As String, ByVal permission As AccessPermission, ByVal ParamArray parameters() as SqlParam)
            If Not (m_Page.ContainsField(fieldName)) Then
                Return
            End If
            Dim a As AccessControlAttribute = CreateDynamicAccessControlAttribute(fieldName)
            a.Sql = sql
            a.Permission = permission
            a.Parameters = New List(Of SqlParam)()
            If (parameters.Length = 0) Then
                Using query As SqlText = New SqlText(sql)
                    Dim m As Match = New Regex((Regex.Escape(query.ParameterMarker) + "([\w_]+)")).Match(sql)
                    Do While m.Success
                        If Not (query.Parameters.Contains(m.Value)) Then
                            IsSystemSqlParameter(query, m.Value)
                        End If
                        m = m.NextMatch()
                    Loop
                    For Each p As DbParameter in query.Parameters
                        a.Parameters.Add(New SqlParam(p.ParameterName, p.Value))
                    Next
                End Using
            Else
                For Each p As SqlParam in parameters
                    a.Parameters.Add(p)
                Next
            End If
        End Sub
        
        ''' <summary>
        ''' Registers the access control rule that will be active only while processing the current request.
        ''' </summary>
        ''' <param name="fieldName">The name of the data field that must be present in the data view for the rule to be activated.</param>
        ''' <param name="permission">The permission to allow or deny access to data. Access control rules are combined according to this formula: (List of  “Allow” restrictions) and Not (List of “Deny” restrictions).</param>
        ''' <param name="values">The list of values that will be matched to the SQL expression corresponding to the name of the field triggering the activation of the access control rule.</param>
        Protected Overloads Sub RegisterAccessControlRule(ByVal fieldName As String, ByVal permission As AccessPermission, ByVal ParamArray values() as System.[Object])
            If Not (m_Page.ContainsField(fieldName)) Then
                Return
            End If
            Dim a As AccessControlAttribute = CreateDynamicAccessControlAttribute(fieldName)
            a.Permission = permission
            If (values Is Nothing) Then
                values = New Object() {Nothing}
            End If
            a.Restrictions = New List(Of Object)(values)
        End Sub
        
        ''' <summary>
        ''' Enumerates the list of all access control rules that must be activated when processing the current request.
        ''' </summary>
        ''' <param name="controllerName">The name of the data controller that is requiring processing via the business rules.</param>
        Protected Overridable Sub EnumerateDynamicAccessControlRules(ByVal controllerName As String)
        End Sub
        
        Protected Overridable Function CreateAppDacl(ByVal controllerName As String) As DynamicAccessControlList
            Dim dacl As DynamicAccessControlList = CType(Context.Cache("DynamicAccessControlList"),DynamicAccessControlList)
            If (dacl Is Nothing) Then
                Dim rulesPath As String = Context.Server.MapPath("~/dacl")
                dacl = New DynamicAccessControlList()
                Dim files() As String = Nothing
                If Directory.Exists(rulesPath) Then
                    files = Directory.GetFiles(rulesPath, "*.txt")
                    For Each fileName As String in files
                        dacl.Parse(Path.GetFileName(fileName), File.ReadAllText(fileName))
                    Next
                Else
                    files = New String() {rulesPath}
                End If
                Context.Cache.Add("DynamicAccessControlList", dacl, New CacheDependency(files), Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, Nothing)
            End If
            Return dacl
        End Function
        
        Protected Overridable Function CreateDbDacl(ByVal controllerName As String) As DynamicAccessControlList
            Dim dacl As DynamicAccessControlList = CType(Context.Items("DbDacl"),DynamicAccessControlList)
            If (dacl Is Nothing) Then
                dacl = New DynamicAccessControlList()
                Context.Items("DbDacl") = dacl
                If ApplicationServices.IsSiteContentEnabled Then
                    Dim files As SiteContentFileList = ApplicationServices.Current.ReadSiteContent("sys/dacl%", "%")
                    For Each f As SiteContentFile in files
                        If (Not (f.Data) Is Nothing) Then
                            dacl.Parse(f.PhysicalName, f.Text)
                        End If
                    Next
                End If
            End If
            Return dacl
        End Function
        
        Protected Overridable Sub EnumerateRulesFromDACL(ByVal controllerName As String)
            If ApplicationServices.IsSafeMode Then
                Return
            End If
            Dim fields As SortedDictionary(Of String, DataField) = New SortedDictionary(Of String, DataField)()
            For Each field As DataField in m_Page.Fields
                fields(field.Name) = field
            Next
            'create DACL
            Dim dacl As DynamicAccessControlList = New DynamicAccessControlList()
            dacl.AddRange(CreateAppDacl(controllerName))
            dacl.AddRange(CreateDbDacl(controllerName))
            'evaluate rules for this user
            Dim userName As String = String.Empty
            If (Not (Context.User) Is Nothing) Then
                userName = Context.User.Identity.Name.ToLower()
            End If
            For Each r As DynamicAccessControlRule in dacl
                If fields.ContainsKey(r.Field) Then
                    If (String.IsNullOrEmpty(r.Controller) OrElse r.Controller.Equals(controllerName, StringComparison.OrdinalIgnoreCase)) Then
                        If ((r.Tags Is Nothing) OrElse IsTagged(r.Tags)) Then
                            If ((r.Users Is Nothing) OrElse Not ((Array.IndexOf(r.Users, userName) = -1))) Then
                                If ((r.Roles Is Nothing) OrElse UserIsInRole(r.Roles)) Then
                                    If ((r.RoleExceptions Is Nothing) OrElse Not (UserIsInRole(r.RoleExceptions))) Then
                                        If ((r.UserExceptions Is Nothing) OrElse (Array.IndexOf(r.UserExceptions, userName) = -1)) Then
                                            If Not (String.IsNullOrEmpty(r.AllowSql)) Then
                                                RegisterAccessControlRule(r.Field, r.AllowSql, AccessPermission.Allow)
                                            End If
                                            If Not (String.IsNullOrEmpty(r.DenySql)) Then
                                                RegisterAccessControlRule(r.Field, r.DenySql, AccessPermission.Deny)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            Next
        End Sub
        
        Public Function EnumerateAccessControlRules(ByVal command As DbCommand, ByVal controllerName As String, ByVal parameterMarker As String, ByVal page As ViewPage, ByVal expressions As SelectClauseDictionary) As String
            Dim rules As AccessControlRuleDictionary = Nothing
            For Each m As MethodInfo in [GetType]().GetMethods((BindingFlags.Public Or (BindingFlags.NonPublic Or BindingFlags.Instance)))
                For Each accessControl As AccessControlAttribute in m.GetCustomAttributes(GetType(AccessControlAttribute), true)
                    If (((controllerName = accessControl.Controller) OrElse Regex.IsMatch(controllerName, accessControl.Controller)) OrElse String.IsNullOrEmpty(accessControl.Controller)) Then
                        If page.ContainsField(accessControl.FieldName) Then
                            If (rules Is Nothing) Then
                                rules = New AccessControlRuleDictionary()
                            End If
                            Dim attributes As List(Of AccessControlRule) = Nothing
                            If Not (rules.TryGetValue(accessControl.FieldName, attributes)) Then
                                attributes = New List(Of AccessControlRule)()
                                rules.Add(accessControl.FieldName, attributes)
                            End If
                            attributes.Add(New AccessControlRule(accessControl, m))
                        End If
                    End If
                Next
            Next
            Me.m_Page = page
            EnumerateRulesFromDACL(controllerName)
            EnumerateDynamicAccessControlRules(controllerName)
            If (Not (m_DynamicAccessControlRules) Is Nothing) Then
                If (rules Is Nothing) Then
                    rules = m_DynamicAccessControlRules
                Else
                    For Each fieldName As String in m_DynamicAccessControlRules.Keys
                        If page.ContainsField(fieldName) Then
                            Dim attributes As List(Of AccessControlRule) = Nothing
                            If Not (rules.TryGetValue(fieldName, attributes)) Then
                                attributes = New List(Of AccessControlRule)()
                                rules.Add(fieldName, attributes)
                            End If
                            attributes.AddRange(m_DynamicAccessControlRules(fieldName))
                        End If
                    Next
                End If
                m_DynamicAccessControlRules = Nothing
            End If
            If (rules Is Nothing) Then
                Return Nothing
            End If
            Dim allowRules As StringBuilder = New StringBuilder()
            ProcessAccessControlList(rules, AccessPermission.Allow, allowRules, command, parameterMarker, page, expressions)
            Dim denyRules As StringBuilder = New StringBuilder()
            ProcessAccessControlList(rules, AccessPermission.Deny, denyRules, command, parameterMarker, page, expressions)
            rules.Clear()
            If (allowRules.Length = 0) Then
                If (denyRules.Length = 0) Then
                    Return String.Empty
                Else
                    Return String.Format("not({0})", denyRules.ToString())
                End If
            Else
                If (denyRules.Length = 0) Then
                    Return allowRules.ToString()
                Else
                    Return String.Format("({0})and not({1})", allowRules.ToString(), denyRules.ToString())
                End If
            End If
        End Function
        
        Protected Function ValidateSql(ByVal sql As String, ByVal expressions As SelectClauseDictionary) As String
            If String.IsNullOrEmpty(sql) Then
                Return Nothing
            End If
            m_Expressions = expressions
            m_SqlIsValid = true
            sql = FieldNameRegex.Replace(sql, AddressOf DoReplaceFieldNames)
            If Not (m_SqlIsValid) Then
                Return Nothing
            End If
            Return sql
        End Function
        
        Private Function DoReplaceFieldNames(ByVal m As Match) As String
            Dim s As String = Nothing
            If m_Expressions.TryGetValue(m.Groups(1).Value, s) Then
                Return s
            Else
                m_SqlIsValid = false
            End If
            Return m.Value
        End Function
        
        Private Sub ProcessAccessControlList(ByVal rules As AccessControlRuleDictionary, ByVal permission As AccessPermission, ByVal sb As StringBuilder, ByVal command As DbCommand, ByVal parameterMarker As String, ByVal page As ViewPage, ByVal expressions As SelectClauseDictionary)
            Dim firstField As Boolean = true
            For Each fieldName As String in rules.Keys
                Dim fieldExpression As String = expressions(page.FindField(fieldName).ExpressionName())
                Dim accessControlList As List(Of AccessControlRule) = rules(fieldName)
                Dim firstRule As Boolean = true
                For Each info As AccessControlRule in accessControlList
                    If (info.AccessControl.Permission = permission) Then
                        Me.m_ApplyAccessControlRule = false
                        Me.m_AccessControlRestrictions = New List(Of Object)()
                        Me.m_AccessControlCommand = command
                        If (info.Method Is Nothing) Then
                            If (Not (info.AccessControl.Restrictions) Is Nothing) Then
                                m_AccessControlRestrictions.AddRange(info.AccessControl.Restrictions)
                            Else
                                If (Not (info.AccessControl.Parameters) Is Nothing) Then
                                    For Each p As SqlParam in info.AccessControl.Parameters
                                        RestrictAccess(p.Name, p.Value)
                                    Next
                                End If
                            End If
                            m_ApplyAccessControlRule = true
                        Else
                            info.Method.Invoke(Me, New Object(-1) {})
                        End If
                        Dim sql As String = ValidateSql(info.AccessControl.Sql, expressions)
                        If (Me.m_ApplyAccessControlRule AndAlso ((m_AccessControlRestrictions.Count > 0) OrElse Not (String.IsNullOrEmpty(sql)))) Then
                            If firstField Then
                                firstField = false
                                sb.Append("(")
                            Else
                                If firstRule Then
                                    sb.Append("and")
                                End If
                            End If
                            If firstRule Then
                                firstRule = false
                                sb.Append("(")
                            Else
                                sb.Append("or")
                            End If
                            sb.Append("(")
                            If Not (String.IsNullOrEmpty(sql)) Then
                                If SelectDetectionRegex.IsMatch(sql) Then
                                    sb.AppendFormat("{0} in({1})", fieldExpression, sql)
                                Else
                                    sb.Append(sql)
                                End If
                            Else
                                If (m_AccessControlRestrictions.Count > 1) Then
                                    Dim hasNull As Boolean = false
                                    Dim firstRestriction As Boolean = true
                                    For Each item As Object in m_AccessControlRestrictions
                                        If ((item Is Nothing) OrElse DBNull.Value.Equals(item)) Then
                                            hasNull = true
                                        Else
                                            If firstRestriction Then
                                                firstRestriction = false
                                                sb.AppendFormat("{0} in(", fieldExpression)
                                            Else
                                                sb.Append(",")
                                            End If
                                            Dim p As DbParameter = command.CreateParameter()
                                            p.ParameterName = String.Format("{0}p{1}", parameterMarker, command.Parameters.Count)
                                            p.Value = item
                                            command.Parameters.Add(p)
                                            sb.Append(p.ParameterName)
                                        End If
                                    Next
                                    If Not (firstRestriction) Then
                                        sb.Append(")")
                                    End If
                                    If hasNull Then
                                        If Not (firstRestriction) Then
                                            sb.AppendFormat("or {0}", fieldExpression)
                                        Else
                                            sb.Append(fieldExpression)
                                        End If
                                        sb.Append(" is null")
                                    End If
                                Else
                                    Dim item As Object = m_AccessControlRestrictions(0)
                                    If ((item Is Nothing) OrElse DBNull.Value.Equals(item)) Then
                                        sb.AppendFormat("{0} is null", fieldExpression)
                                    Else
                                        Dim p As DbParameter = command.CreateParameter()
                                        p.ParameterName = String.Format("{0}p{1}", parameterMarker, command.Parameters.Count)
                                        p.Value = item
                                        command.Parameters.Add(p)
                                        sb.AppendFormat("{0}={1}", fieldExpression, p.ParameterName)
                                    End If
                                End If
                            End If
                            sb.Append(")")
                        End If
                        m_AccessControlCommand = Nothing
                        m_AccessControlRestrictions.Clear()
                    End If
                Next
                If Not (firstRule) Then
                    sb.Append(")")
                End If
            Next
            If Not (firstField) Then
                sb.Append(")")
            End If
        End Sub
        
        Public Overridable Function SupportsVirtualization(ByVal controllerName As String) As Boolean
            Return ApplicationServices.IsSiteContentEnabled
        End Function
        
        Protected Overloads Overridable Sub VirtualizeController(ByVal controllerName As String)
        End Sub
        
        Public Overridable Function VirtualizeControllerConditionally(ByVal controllerName As String) As Boolean
            Return false
        End Function
        
        Public Overloads Overridable Sub VirtualizeController(ByVal controllerName As String, ByVal navigator As XPathNavigator, ByVal resolver As XmlNamespaceManager)
            Me.Navigator = navigator
            Me.Resolver = resolver
            AlterController(controllerName)
            VirtualizeController(controllerName)
        End Sub
        
        Public Overridable Function CompleteConfiguration() As Boolean
            Dim result As Boolean = false
            Dim saveRow() As Object = m_Row
            If (Not (Page.NewRow) Is Nothing) Then
                m_Row = Page.NewRow
            Else
                If ((Not (Page.Rows) Is Nothing) AndAlso (Page.Rows.Count > 0)) Then
                    m_Row = Page.Rows(0)
                End If
            End If
            If VirtualizeControllerConditionally(Page.Controller) Then
                result = true
            End If
            If (Not (Config.PendingAlterations) Is Nothing) Then
                result = AlterController(Config.PendingAlterations, true)
            End If
            m_Row = saveRow
            Return result
        End Function
        
        Public Overloads Overridable Sub AlterController(ByVal controllerName As String)
            If (ApplicationServices.IsSiteContentEnabled AndAlso Not ((controllerName = ApplicationServices.SiteContentControllerName))) Then
                Dim alterations As SiteContentFileList = ApplicationServices.Current.ReadSiteContent("sys/controllers%", (controllerName + ".Alter%"))
                AlterController(alterations, false)
            End If
        End Sub
        
        Public Overloads Overridable Function AlterController(ByVal alterations As SiteContentFileList, ByVal immediately As Boolean) As Boolean
            Dim changed As Boolean = false
            For Each f As SiteContentFile in alterations
                Dim nodeSet As ControllerNodeSet = Me.NodeSet()
                Dim alter As String = f.Text
                If Not (String.IsNullOrEmpty(alter)) Then
                    If (Not (immediately) AndAlso TestPendingAlterRegex.IsMatch(alter)) Then
                        If (PendingAlterations Is Nothing) Then
                            PendingAlterations = New SiteContentFileList()
                        End If
                        PendingAlterations.Add(f)
                    Else
                        Dim m As Match = AlterMethodRegex.Match(alter)
                        Dim skipInvoke As Boolean = false
                        Do While m.Success
                            Dim method As String = m.Groups("Method").Value
                            Dim parameters As String = m.Groups("Parameters").Value
                            Dim terminator As String = m.Groups("Terminator").Value
                            Dim sb As StringBuilder = New StringBuilder()
                            For Each s As String in method.Split(New Char() {Global.Microsoft.VisualBasic.ChrW(45)}, StringSplitOptions.RemoveEmptyEntries)
                                sb.Append(([Char].ToUpper(s(0)) + s.Substring(1)))
                            Next
                            method = sb.ToString()
                            Dim args As List(Of String) = New List(Of String)()
                            Dim p As Match = AlterParametersRegex.Match(parameters)
                            Do While p.Success
                                args.Add(p.Groups("Argument").Value)
                                p = p.NextMatch()
                            Loop
                            Try 
                                Dim tested As Boolean = false
                                If (args.Count > 0) Then
                                    If (method = "WhenTagged") Then
                                        If Not (IsTagged(args.ToArray())) Then
                                            skipInvoke = true
                                            If (terminator = ";") Then
                                                Exit Do
                                            End If
                                        End If
                                        tested = true
                                    End If
                                    If (method = "WhenUrl") Then
                                        Dim urlRegex As Regex = New Regex(args(0), RegexOptions.IgnoreCase)
                                        If ((Not (Context.Request.UrlReferrer) Is Nothing) AndAlso Not (urlRegex.IsMatch(Context.Request.UrlReferrer.ToString()))) Then
                                            skipInvoke = true
                                            If (terminator = ";") Then
                                                Exit Do
                                            End If
                                        End If
                                        tested = true
                                    End If
                                    If (method = "WhenUserInterface") Then
                                        Dim userInterface As String = args(0).ToLower()
                                        If (((userInterface = "touch") AndAlso Not (ApplicationServices.IsTouchClient)) OrElse ((userInterface = "desktop") AndAlso ApplicationServices.IsTouchClient)) Then
                                            skipInvoke = true
                                            If (terminator = ";") Then
                                                Exit Do
                                            End If
                                        End If
                                        tested = true
                                    End If
                                    If (method = "WhenView") Then
                                        If Not (skipInvoke) Then
                                            Dim viewRegex As Regex = New Regex(args(0), RegexOptions.IgnoreCase)
                                            If ((Me.View Is Nothing) OrElse Not (viewRegex.IsMatch(Me.View))) Then
                                                skipInvoke = true
                                                If (terminator = ";") Then
                                                    Exit Do
                                                End If
                                            End If
                                        End If
                                        tested = true
                                    End If
                                    If (method = "WhenTest") Then
                                        If Not (skipInvoke) Then
                                            Dim dt As DataTable = Page.ToDataTable()
                                            dt.DefaultView.RowFilter = args(0).Trim()
                                            If (dt.DefaultView.Count = 0) Then
                                                skipInvoke = true
                                                If (terminator = ";") Then
                                                    Exit Do
                                                End If
                                            End If
                                        End If
                                        tested = true
                                    End If
                                    If (method = "WhenSql") Then
                                        If Not (skipInvoke) Then
                                            Dim q As String = args(0).Trim()
                                            Dim sqlText As String = "select 1"
                                            Dim css As ConnectionStringSettings = ConnectionStringSettingsFactory.Create(Nothing)
                                            If css.ProviderName.Contains("Oracle") Then
                                                sqlText = (sqlText + " from dual")
                                            End If
                                            sqlText = (sqlText  _
                                                        + (" where " + q))
                                            EnableDccTest = true
                                            skipInvoke = (Sql(sqlText) = 0)
                                            EnableDccTest = false
                                            If skipInvoke Then
                                                If (terminator = ";") Then
                                                    Exit Do
                                                End If
                                            End If
                                        End If
                                        tested = true
                                    End If
                                End If
                                If (Not (skipInvoke) AndAlso Not (tested)) Then
                                    nodeSet = CType(nodeSet.GetType().InvokeMember(method, BindingFlags.InvokeMethod, Nothing, nodeSet, args.ToArray()),ControllerNodeSet)
                                    changed = true
                                End If
                            Catch ex As Exception
                                Throw New Exception(String.Format("{0}){1}: {2}", method, parameters, ex.Message))
                            End Try
                            m = m.NextMatch()
                            If (terminator = ";") Then
                                nodeSet = Me.NodeSet()
                                skipInvoke = false
                            End If
                        Loop
                    End If
                End If
            Next
            Return changed
        End Function
        
        ''' <summary>
        ''' Returns true if the data view on the page is tagged with any of the values specified in the arguments.
        ''' </summary>
        ''' <param name="tags">The collection of string values representing tag names.</param>
        ''' <returns>Returns true if at least one tag specified in the arguments is assigned to the data view.</returns>
        Protected Function IsTagged(ByVal ParamArray tags() as System.[String]) As Boolean
            Dim list() As String = TagList
            For Each t As String in tags
                If (Array.IndexOf(list, t) >= 0) Then
                    Return true
                End If
            Next
            Return false
        End Function
        
        Protected Sub AddTag(ByVal ParamArray tags() as System.[String])
            Dim list As List(Of String) = New List(Of String)(TagList)
            For Each t As String in tags
                If Not (list.Contains(t)) Then
                    list.Add(t)
                End If
            Next
            TagList = list.ToArray()
        End Sub
        
        Protected Sub RemoveTag(ByVal ParamArray tags() as System.[String])
            Dim list As List(Of String) = New List(Of String)(TagList)
            For Each t As String in tags
                list.Remove(t)
            Next
            TagList = list.ToArray()
        End Sub
        
        Protected Overloads Sub AddFieldValue(ByVal v As FieldValue)
            If (Not (Arguments) Is Nothing) Then
                Dim values As List(Of FieldValue) = New List(Of FieldValue)(Arguments.Values)
                values.Add(v)
                Arguments.Values = values.ToArray()
            End If
        End Sub
        
        Protected Overloads Sub AddFieldValue(ByVal name As String, ByVal newValue As Object)
            AddFieldValue(New FieldValue(name, newValue))
        End Sub
        
        Public Overloads Sub BeforeSelect(ByVal request As DistinctValueRequest)
            ExecuteServerRules(request, ActionPhase.Before)
            ExecuteSelect(request.Controller, request.View, request.Filter, request.ExternalFilter, ActionPhase.Before, "SelectDistinct")
        End Sub
        
        Public Overloads Sub AfterSelect(ByVal request As DistinctValueRequest)
            ExecuteServerRules(request, ActionPhase.Before)
            ExecuteSelect(request.Controller, request.View, request.Filter, request.ExternalFilter, ActionPhase.After, "SelectDistinct")
        End Sub
        
        Public Overloads Sub BeforeSelect(ByVal request As PageRequest)
            ExecuteServerRules(request, ActionPhase.Before)
            ExecuteSelect(request.Controller, request.View, request.Filter, request.ExternalFilter, ActionPhase.Before, "Select")
        End Sub
        
        Public Overloads Sub AfterSelect(ByVal request As PageRequest)
            ExecuteServerRules(request, ActionPhase.After)
            ExecuteSelect(request.Controller, request.View, request.Filter, request.ExternalFilter, ActionPhase.After, "Select")
        End Sub
        
        Public Function IsFiltered(ByVal fieldName As String, ByVal ParamArray operations() as RowFilterOperation) As Boolean
            Dim fv As FilterValue = SelectFilterValue(fieldName)
            If (Not (fv) Is Nothing) Then
                For Each op As RowFilterOperation in operations
                    If (fv.FilterOperation = op) Then
                        Return true
                    End If
                Next
            End If
            Return (Not (fv) Is Nothing)
        End Function
        
        Public Function SelectFilterValue(ByVal fieldName As String) As FilterValue
            Dim fv As FilterValue = Nothing
            Dim filters() As String = m_RequestFilter
            If ((filters Is Nothing) OrElse (filters.Length = 0)) Then
                filters = Result.Filter
            End If
            If (Not (filters) Is Nothing) Then
                For Each filterExpression As String in filters
                    Dim filterMatch As Match = Controller.FilterExpressionRegex.Match(filterExpression)
                    If filterMatch.Success Then
                        Dim valueMatch As Match = Controller.FilterValueRegex.Match(filterMatch.Groups("Values").Value)
                        Dim [alias] As String = filterMatch.Groups("Alias").Value
                        Dim operation As String = valueMatch.Groups("Operation").Value
                        If (valueMatch.Success AndAlso ([alias].Equals(fieldName, StringComparison.InvariantCultureIgnoreCase) AndAlso Not ((operation = "~")))) Then
                            Dim filterValue As String = valueMatch.Groups("Value").Value
                            Dim v As Object = Nothing
                            If Not (Controller.StringIsNull(filterValue)) Then
                                If Regex.IsMatch(filterValue, "\$(or|and)\$") Then
                                    Dim list() As String = filterValue.Split(New String() {"$or$", "$and$"}, StringSplitOptions.RemoveEmptyEntries)
                                    Dim values As List(Of Object) = New List(Of Object)()
                                    For Each s As String in list
                                        If Controller.StringIsNull(s) Then
                                            values.Add(Nothing)
                                        Else
                                            values.Add(Controller.StringToValue(s))
                                        End If
                                    Next
                                    v = values.ToArray()
                                Else
                                    v = Controller.StringToValue(filterValue)
                                End If
                            End If
                            fv = New FilterValue([alias], operation, v)
                            Exit For
                        End If
                    End If
                Next
            End If
            If ((fv Is Nothing) AndAlso (Not (m_RequestExternalFilter) Is Nothing)) Then
                For Each v As FieldValue in m_RequestExternalFilter
                    If v.Name.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase) Then
                        fv = New FilterValue(fieldName, "=", v.Value)
                        Exit For
                    End If
                Next
            End If
            Return fv
        End Function
        
        Private Sub ExecuteSelect(ByVal controllerName As String, ByVal viewId As String, ByVal filter() As String, ByVal externalFilter() As FieldValue, ByVal phase As ActionPhase, ByVal commandName As String)
            Me.m_RequestFilter = filter
            Me.m_RequestExternalFilter = externalFilter
            Dim methods() As MethodInfo = [GetType]().GetMethods((BindingFlags.Public Or (BindingFlags.NonPublic Or BindingFlags.Instance)))
            For Each method As MethodInfo in methods
                Dim filters() As Object = method.GetCustomAttributes(GetType(ControllerActionAttribute), true)
                For Each action As ControllerActionAttribute in filters
                    If ((String.IsNullOrEmpty(action.Controller) OrElse ((action.Controller = controllerName) OrElse Regex.IsMatch(controllerName, action.Controller))) AndAlso (String.IsNullOrEmpty(action.View) OrElse ((action.View = viewId) OrElse Regex.IsMatch(viewId, action.View)))) Then
                        If ((action.CommandName = commandName) AndAlso (action.Phase = phase)) Then
                            Dim parameters() As ParameterInfo = method.GetParameters()
                            Dim arguments((parameters.Length) - 1) As Object
                            Dim i As Integer = 0
                            Do While (i < parameters.Length)
                                Dim p As ParameterInfo = parameters(i)
                                Dim fv As FilterValue = SelectFilterValue(p.Name)
                                If (Not (fv) Is Nothing) Then
                                    If p.ParameterType.Equals(GetType(FilterValue)) Then
                                        arguments(i) = fv
                                    Else
                                        Try 
                                            If p.ParameterType.IsArray Then
                                                Dim list As ArrayList = New ArrayList()
                                                For Each o As Object in fv.Values
                                                    Dim elemValue As Object = Nothing
                                                    Try 
                                                        elemValue = Controller.ConvertToType(p.ParameterType.GetElementType(), o)
                                                    Catch __exception As Exception
                                                    End Try
                                                    list.Add(elemValue)
                                                Next
                                                arguments(i) = list.ToArray(p.ParameterType.GetElementType())
                                            Else
                                                arguments(i) = Controller.ConvertToType(p.ParameterType, fv.Value)
                                            End If
                                        Catch __exception As Exception
                                        End Try
                                    End If
                                End If
                                i = (i + 1)
                            Loop
                            method.Invoke(Me, arguments)
                        End If
                    End If
                Next
            Next
        End Sub
        
        Protected Sub ChangeFilter(ByVal ParamArray filter() as FilterValue)
            ApplyFilter(false, filter)
        End Sub
        
        Protected Sub AssignFilter(ByVal ParamArray filter() as FilterValue)
            ApplyFilter(true, filter)
        End Sub
        
        Private Sub ApplyFilter(ByVal replace As Boolean, ByVal ParamArray filter() as FilterValue)
            Dim newFilter As List(Of String) = New List(Of String)()
            If Not (replace) Then
                Dim currentFilter As List(Of String) = New List(Of String)()
                If (Not (Page) Is Nothing) Then
                    currentFilter.AddRange(Page.Filter)
                Else
                    If (Not (Result) Is Nothing) Then
                        currentFilter.AddRange(Result.Filter)
                    End If
                End If
                For Each fv As FilterValue in filter
                    Dim i As Integer = 0
                    Do While (i < currentFilter.Count)
                        If currentFilter(i).StartsWith((fv.Name + ":")) Then
                            currentFilter.RemoveAt(i)
                            Exit Do
                        Else
                            i = (i + 1)
                        End If
                    Loop
                    newFilter = New List(Of String)(currentFilter)
                Next
            End If
            For Each fv As FilterValue in filter
                Dim filterValue As String = "%js%null"
                If Not (DBNull.Value.Equals(fv.Value)) Then
                    Dim sb As StringBuilder = New StringBuilder()
                    Dim separator As String = "$or$"
                    If (fv.FilterOperation = RowFilterOperation.Between) Then
                        separator = "$and$"
                    End If
                    For Each o As Object in fv.Values
                        If (sb.Length > 0) Then
                            sb.Append(separator)
                        End If
                        sb.Append(Controller.ValueToString(o))
                    Next
                    filterValue = sb.ToString()
                End If
                newFilter.Add(String.Format("{0}:{1}{2}", fv.Name, RowFilterAttribute.ComparisonOperations(CType(fv.FilterOperation,Integer)), filterValue))
            Next
            If (Not (m_RequestExternalFilter) Is Nothing) Then
                For Each v As FieldValue in m_RequestExternalFilter
                    newFilter.Add(String.Format("{0}:={1}", v.Name, Controller.ValueToString(v.Value)))
                Next
            End If
            If (Not (Page) Is Nothing) Then
                Page.ChangeFilter(newFilter.ToArray())
                m_RequestFilter = Page.Filter
            End If
            If (Not (Result) Is Nothing) Then
                Result.Filter = newFilter.ToArray()
            End If
        End Sub
        
        Public Shared Function Create(ByVal config As ControllerConfiguration) As BusinessRules
            Dim t As Type = GetType(BusinessRules)
            Dim rules As BusinessRules = CType(t.Assembly.CreateInstance(t.FullName),BusinessRules)
            rules.Config = config
            Return rules
        End Function
        
        Protected Overridable Function ResolveFieldValuesForMultipleSelection(ByVal args As ActionArgs) As Boolean
            Return true
        End Function
        
        Public Overloads Sub ProcessSpecialActions(ByVal args As ActionArgs, ByVal result As ActionResult)
            Me.Arguments = args
            Me.Result = result
            Dim multipleSelection As Boolean = ((Not (args.SelectedValues) Is Nothing) AndAlso (args.SelectedValues.Length > 1))
            Dim fields As List(Of DataField) = Nothing
            If (multipleSelection AndAlso Not (((args.LastCommandName = "Edit") OrElse (args.LastCommandName = "New")))) Then
                Dim keyFields As List(Of String) = New List(Of String)()
                Dim keyFieldIterator As XPathNodeIterator = Config.Select("/c:dataController/c:fields/c:field[@isPrimaryKey='true']/@name")
                Do While keyFieldIterator.MoveNext()
                    keyFields.Add(keyFieldIterator.Current.Value)
                Loop
                For Each key As String in args.SelectedValues
                    ClearBlackAndWhiteLists()
                    Dim keyValues() As String = key.Split(Global.Microsoft.VisualBasic.ChrW(44))
                    Dim filter As List(Of String) = New List(Of String)()
                    Dim index As Integer = 0
                    For Each fieldName As String in keyFields
                        Dim fv As FieldValue = SelectFieldValueObject(fieldName)
                        If (Not (fv) Is Nothing) Then
                            fv.NewValue = keyValues(index)
                            fv.OldValue = fv.NewValue
                            fv.Modified = false
                            filter.Add(String.Format("{0}:={1}", fieldName, DataControllerBase.ValueToString(fv.Value)))
                        End If
                        index = (index + 1)
                    Next
                    If (multipleSelection AndAlso ResolveFieldValuesForMultipleSelection(args)) Then
                        Dim r As PageRequest = New PageRequest(0, 1, String.Empty, filter.ToArray())
                        r.Controller = args.Controller
                        r.View = args.View
                        r.Tag = args.Tag
                        r.RequiresMetaData = (fields Is Nothing)
                        r.DisableJSONCompatibility = true
                        Dim p As ViewPage = ControllerFactory.CreateDataController().GetPage(r.Controller, r.View, r)
                        If (fields Is Nothing) Then
                            fields = p.Fields
                        End If
                        If (p.Rows.Count = 1) Then
                            Dim i As Integer = 0
                            Do While (i < fields.Count)
                                Dim f As DataField = fields(i)
                                If Not (f.IsPrimaryKey) Then
                                    Dim fv As FieldValue = SelectFieldValueObject(f.Name)
                                    If (Not (fv) Is Nothing) Then
                                        fv.NewValue = p.Rows(0)(i)
                                        fv.OldValue = fv.NewValue
                                        fv.Modified = false
                                    End If
                                End If
                                i = (i + 1)
                            Loop
                        End If
                    End If
                    ProcessSpecialActions(args)
                Next
            Else
                ProcessSpecialActions(args)
            End If
        End Sub
        
        Protected Overloads Overridable Sub ProcessSpecialActions(ByVal args As ActionArgs)
            If args.IgnoreBusinessRules Then
                Return
            End If
            ExecuteServerRules(args, Result, ActionPhase.Before)
            If Not (Result.Canceled) Then
                If Not (String.IsNullOrEmpty(ActionData)) Then
                    If (args.CommandName = "SQL") Then
                        Sql(ActionData)
                    End If
                    If (args.CommandName = "Email") Then
                        Email(ActionData)
                    End If
                End If
                ExecuteServerRules(args, Result, ActionPhase.After)
            End If
        End Sub
        
        ''' <summary>
        ''' Executes the SQL statements specified in the 'text' argument. Any parameter referenced in the text is provided with a value if the parameter name is matched to the name of a data field.
        ''' </summary>
        ''' <param name="text">The text composed of valid SQL statements.
        ''' Parameter names can reference data fields as @FieldName, @FieldName_Value, @FieldName_OldValue, and @FieldName_NewValue.
        ''' Use the parameter marker supported by the database server.</param>
        ''' <param name="parameters">Optional list of parameter values used if a matching data field is not found.</param>
        ''' <returns>The number of records affected by execute of SQL statements</returns>
        Protected Overloads Function Sql(ByVal text As String, ByVal ParamArray parameters() as ParameterValue) As Integer
            Return Sql(text, String.Empty, parameters)
        End Function
        
        Protected Overridable Sub CreateSqlParameter(ByVal query As SqlText, ByVal parameterName As String, ByVal parameterValue As Object, ByVal fieldType As String, ByVal fieldLen As String)
            Dim p As DbParameter = query.AddParameter(parameterName, parameterValue)
            If Not (String.IsNullOrEmpty(fieldType)) Then
                p.Direction = ParameterDirection.InputOutput
                DataControllerBase.AssignParameterValue(p, fieldType, parameterValue)
                If Not (String.IsNullOrEmpty(fieldLen)) Then
                    p.Size = Convert.ToInt32(fieldLen)
                Else
                    If (fieldType = "String") Then
                        p.Direction = ParameterDirection.Input
                    Else
                        If (fieldType = "Decimal") Then
                            CType(p,IDbDataParameter).Precision = 38
                            CType(p,IDbDataParameter).Scale = 10
                        End If
                    End If
                End If
            End If
        End Sub
        
        ''' <summary>
        ''' Executes the SQL statements specified in the 'text' argument. Any parameter referenced in the text is provided with a value if the parameter name is matched to the name of a data field.
        ''' </summary>
        ''' <param name="text">The text composed of valid SQL statements.
        ''' Parameter names can reference data fields as @FieldName, @FieldName_Value, @FieldName_OldValue, and @FieldName_NewValue.
        ''' Use the parameter marker supported by the database server.</param>
        ''' <param name="connectionStringName">The name of the database connection string.</param>
        ''' <param name="parameters">Optional list of parameter values used if a matching data field is not found.</param>
        ''' <returns>The number of records affected by execute of SQL statements</returns>
        Protected Overloads Function Sql(ByVal text As String, ByVal connectionStringName As String, ByVal ParamArray parameters() as ParameterValue) As Integer
            Dim resultSetCacheVar As String = Nothing
            If (EnableResultSet AndAlso (ResultSetCacheDuration > 0)) Then
                resultSetCacheVar = (("ResultSet_" + m_Page.Controller)  _
                            + ("_" + m_Page.View))
                ResultSet = CType(HttpContext.Current.Cache(resultSetCacheVar),DataTable)
                If (Not (ResultSet) Is Nothing) Then
                    Return 0
                End If
            End If
            text = Regex.Replace(text, "(^|\n).*?Debug\s+([\s\S]+?)End Debug(\s+|$)", String.Empty, RegexOptions.IgnoreCase)
            Dim buildingRow As Boolean = ((Not (m_Page) Is Nothing) AndAlso (Not (m_Row) Is Nothing))
            Dim names As List(Of String) = New List(Of String)()
            Using query As SqlText = New SqlText(text, connectionStringName)
                Dim paramRegex As Regex = New Regex(String.Format("({0}(?'FieldName'\w+?)_(?'ValueType'OldValue|NewValue|Value|Modified|FilterValue|"& _ 
                            "FilterOperation|Filter_\w+))|({0}(?'FieldName'\w+))", Regex.Escape(query.ParameterMarker)), RegexOptions.IgnoreCase)
                Dim m As Match = paramRegex.Match(text)
                Do While m.Success
                    Dim fieldName As String = m.Groups("FieldName").Value
                    Dim valueType As String = m.Groups("ValueType").Value
                    Dim paramName As String = m.Value
                    If Not (names.Contains(paramName)) Then
                        names.Add(paramName)
                        Dim fieldType As String = Nothing
                        Dim fieldLen As String = Nothing
                        If (Not (Config) Is Nothing) Then
                            Dim fieldNav As XPathNavigator = Config.SelectSingleNode("/c:dataController/c:fields/c:field[@name='{0}']", fieldName)
                            If (Not (fieldNav) Is Nothing) Then
                                fieldType = fieldNav.GetAttribute("type", String.Empty)
                                fieldLen = fieldNav.GetAttribute("length", String.Empty)
                            End If
                        End If
                        Dim fv As FieldValue = SelectFieldValueObject(fieldName)
                        If (Not (fv) Is Nothing) Then
                            Dim v As Object = fv.Value
                            If (valueType = "OldValue") Then
                                v = fv.OldValue
                            Else
                                If (valueType = "NewValue") Then
                                    v = fv.NewValue
                                Else
                                    If (valueType = "Modified") Then
                                        fieldType = "Boolean"
                                        fieldLen = Nothing
                                        v = fv.Modified
                                    End If
                                End If
                            End If
                            CreateSqlParameter(query, paramName, v, fieldType, fieldLen)
                        Else
                            If fieldName.StartsWith("Parameters_") Then
                                CreateSqlParameter(query, paramName, Nothing, "String", Nothing)
                            Else
                                If (valueType.StartsWith("Filter") AndAlso Not (String.IsNullOrEmpty(fieldType))) Then
                                    Dim v As Object = Nothing
                                    Dim filter As FilterValue = SelectFilterValue(fieldName)
                                    If (Not (filter) Is Nothing) Then
                                        If (valueType = "FilterValue") Then
                                            v = filter.Value
                                        End If
                                        If (valueType = "FilterOperation") Then
                                            v = Convert.ToString(filter.FilterOperation)
                                        End If
                                    End If
                                    CreateSqlParameter(query, paramName, v, fieldType, fieldLen)
                                Else
                                    Dim field As DataField = Nothing
                                    If buildingRow Then
                                        field = Page.FindField(fieldName)
                                        If (Not (field) Is Nothing) Then
                                            CreateSqlParameter(query, paramName, m_Row(Page.Fields.IndexOf(field)), fieldType, fieldLen)
                                        End If
                                    End If
                                    If ((field Is Nothing) AndAlso Not (IsSystemSqlParameter(query, paramName))) Then
                                        For Each pv As ParameterValue in parameters
                                            If pv.Name.Equals(paramName) Then
                                                query.AddParameter(pv.Name, pv.Value).Direction = ParameterDirection.InputOutput
                                                Exit For
                                            End If
                                        Next
                                    End If
                                End If
                            End If
                        End If
                    End If
                    m = m.NextMatch()
                Loop
                ConfigureSqlQuery(query)
                If EnableDccTest Then
                    If query.Read() Then
                        Return 1
                    Else
                        Return 0
                    End If
                Else
                    If EnableResultSet Then
                        ResultSet = New DataTable()
                        ResultSet.Load(query.ExecuteReader())
                        For Each c As DataColumn in ResultSet.Columns
                            Dim columnName As String = c.ColumnName
                            If Not ([Char].IsLetter(columnName(0))) Then
                                columnName = ("n" + columnName)
                            End If
                            columnName = Regex.Replace(columnName, "\W", "")
                            c.ColumnName = columnName
                        Next
                        ResultSetSize = ResultSet.Rows.Count
                        If (ResultSetCacheDuration > 0) Then
                            HttpContext.Current.Cache.Add(resultSetCacheVar, ResultSet.Copy(), Nothing, DateTime.Now.AddSeconds(ResultSetCacheDuration), Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, Nothing)
                        End If
                        Return 0
                    Else
                        If EnableEmailMessages Then
                            Dim messages As DataTable = New DataTable()
                            messages.Load(query.ExecuteReader())
                            EmailMessages = messages
                            Return 0
                        Else
                            Dim rowsAffected As Integer = query.ExecuteNonQuery()
                            Dim clearedFilters As List(Of String) = New List(Of String)()
                            For Each p As DbParameter in query.Parameters
                                Dim fieldName As String = p.ParameterName.Substring(1)
                                Dim fm As Match = SqlFieldFilterOperationRegex.Match(fieldName)
                                If fm.Success Then
                                    Dim name As String = fm.Groups("Name").Value
                                    Dim operation As String = fm.Groups("Operation").Value
                                    Dim value As Object = p.Value
                                    If Not (DBNull.Value.Equals(value)) Then
                                        Dim filter As FilterValue = SelectFilterValue(name)
                                        If "null".Equals(Convert.ToString(value), StringComparison.OrdinalIgnoreCase) Then
                                            value = Nothing
                                        End If
                                        If (Not (filter) Is Nothing) Then
                                            If Not (clearedFilters.Contains(filter.Name)) Then
                                                filter.Clear()
                                                clearedFilters.Add(filter.Name)
                                            End If
                                            filter.AddValue(value)
                                        Else
                                            filter = New FilterValue(name, CType(TypeDescriptor.GetConverter(GetType(RowFilterOperation)).ConvertFromString(operation),RowFilterOperation), value)
                                            clearedFilters.Add(filter.Name)
                                        End If
                                        ChangeFilter(filter)
                                    End If
                                Else
                                    If fieldName.EndsWith("_Modified", StringComparison.OrdinalIgnoreCase) Then
                                        fieldName = fieldName.Substring(0, (fieldName.Length - 9))
                                        Dim fv As FieldValue = SelectFieldValueObject(fieldName)
                                        If (Not (fv) Is Nothing) Then
                                            fv.Modified = Convert.ToBoolean(p.Value)
                                        End If
                                    Else
                                        Dim fv As FieldValue = SelectFieldValueObject(fieldName)
                                        If ((Not (fv) Is Nothing) AndAlso (Convert.ToString(fv.Value) <> Convert.ToString(p.Value))) Then
                                            UpdateFieldValue(fv.Name, p.Value)
                                        End If
                                        Dim field As DataField = Nothing
                                        If buildingRow Then
                                            field = Page.FindField(fieldName)
                                            If (Not (field) Is Nothing) Then
                                                Dim v As Object = p.Value
                                                If DBNull.Value.Equals(v) Then
                                                    v = Nothing
                                                End If
                                                m_Row(Page.Fields.IndexOf(field)) = v
                                            End If
                                        End If
                                        If ((field Is Nothing) AndAlso Not (ProcessSystemSqlParameter(query, p.ParameterName))) Then
                                            For Each pv As ParameterValue in parameters
                                                If pv.Name.Equals(p.ParameterName, StringComparison.InvariantCultureIgnoreCase) Then
                                                    pv.Value = p.Value
                                                End If
                                            Next
                                        End If
                                    End If
                                End If
                            Next
                            Return rowsAffected
                        End If
                    End If
                End If
            End Using
        End Function
        
        ''' <summary>
        ''' Returns the maximum length of SQL Parameter
        ''' </summary>
        ''' <param name="parameterName">The name of SQL parameter without a leading "parameter marker" symbol.</param>
        ''' <returns>The integer value representing the maximum size of SQL parameter.</returns>
        Protected Overridable Function MaximumSizeOfSqlParameter(ByVal parameterName As String) As Integer
            If parameterName.StartsWith("Result_") Then
                Return 512
            End If
            Return 255
        End Function
        
        Private Function IsSystemSqlProperty(ByVal propertyName As String) As Boolean
            Return SystemSqlPropertyRegex.IsMatch(propertyName)
        End Function
        
        ''' <summary>
        ''' Gets a property of a business rule class instance, session variable, or URL parameter.
        ''' </summary>
        ''' <param name="propertyName">The name of a business rule property, session variable, or URL parameter.</param>
        ''' <returns>The value of the property.</returns>
        Public Overridable Function GetProperty(ByVal propertyName As String) As Object
            If propertyName.StartsWith("Parameters_") Then
                Return SelectFieldValue(propertyName)
            End If
            If propertyName.StartsWith("ContextFields_") Then
                Return SelectExternalFilterFieldValue(propertyName.Substring(14))
            End If
            If propertyName.StartsWith("Url_") Then
                propertyName = propertyName.Substring(4)
                If (Not (Context.Request.UrlReferrer) Is Nothing) Then
                    Dim m As Match = Regex.Match(Context.Request.UrlReferrer.PathAndQuery, String.Format("(\?|&){0}=(?'Value'.*?)(&|$)", propertyName))
                    If m.Success Then
                        Return m.Groups("Value").Value
                    End If
                End If
                If (Not (Context.Request.QueryString) Is Nothing) Then
                    Return Context.Request.QueryString(propertyName)
                End If
                Return Nothing
            Else
                If propertyName.StartsWith("Session_") Then
                    propertyName = propertyName.Substring(8)
                    Return Context.Session(propertyName)
                Else
                    Dim t As Type = [GetType]()
                    Dim target As Object = Me
                    If propertyName.StartsWith("BusinessRules_") Then
                        propertyName = propertyName.Substring(14)
                    Else
                        If propertyName.StartsWith("Arguments_") Then
                            propertyName = propertyName.Substring(10)
                            t = GetType(ActionArgs)
                            target = Me.Arguments
                            If (target Is Nothing) Then
                                Return Nothing
                            End If
                        End If
                    End If
                    Return t.InvokeMember(propertyName, (((BindingFlags.GetProperty Or BindingFlags.GetField) Or BindingFlags.Public) Or (((BindingFlags.Instance Or BindingFlags.Static) Or BindingFlags.FlattenHierarchy) Or BindingFlags.IgnoreCase)), Nothing, target, New Object((0) - 1) {})
                End If
            End If
        End Function
        
        ''' <summary>
        ''' Sets the property of the business rule class instance or the session variable value.
        ''' </summary>
        ''' <param name="propertyName">The name of the property or session variable.</param>
        ''' <param name="value">The value of the property.</param>
        Public Overridable Sub SetProperty(ByVal propertyName As String, ByVal value As Object)
            If propertyName.StartsWith("Url_") Then
                'URL properties are read-only.
                Return
            Else
                If (propertyName.StartsWith("Session_") OrElse propertyName.StartsWith("Arguments_")) Then
                    propertyName = propertyName.Substring(8)
                    If TypeOf value Is String Then
                        Dim s As String = CType(value,String)
                        Dim tempGuid As Guid
                        If Guid.TryParse(s, tempGuid) Then
                            value = tempGuid
                        Else
                            Dim tempInt As Integer
                            If Integer.TryParse(s, tempInt) Then
                                value = tempInt
                            Else
                                Dim tempDouble As Double
                                If Double.TryParse(s, tempDouble) Then
                                    value = tempDouble
                                Else
                                    Dim tempDateTime As Date
                                    If DateTime.TryParse(s, tempDateTime) Then
                                        value = tempDateTime
                                    End If
                                End If
                            End If
                        End If
                    End If
                    Context.Session(propertyName) = value
                Else
                    If propertyName.StartsWith("BusinessRules_") Then
                        propertyName = propertyName.Substring(14)
                    End If
                    [GetType]().InvokeMember(propertyName, (((BindingFlags.SetProperty Or BindingFlags.SetField) Or BindingFlags.Public) Or (((BindingFlags.Instance Or BindingFlags.Static) Or BindingFlags.FlattenHierarchy) Or BindingFlags.IgnoreCase)), Nothing, Me, New Object() {value})
                End If
            End If
        End Sub
        
        Protected Overridable Function IsSystemSqlParameter(ByVal sql As SqlText, ByVal parameterName As String) As Boolean
            Dim nameWithoutMarker As String = parameterName.Substring(1)
            Dim isProperty As Boolean = IsSystemSqlProperty(nameWithoutMarker)
            Dim systemParameterIndex As Integer = Array.IndexOf(SystemSqlParameters, nameWithoutMarker)
            If ((systemParameterIndex = -1) AndAlso Not (isProperty)) Then
                Return false
            End If
            If ((systemParameterIndex >= 0) AndAlso (systemParameterIndex <= 3)) Then
                sql.AddParameter(parameterName, 0).Direction = ParameterDirection.InputOutput
            Else
                Dim value As Object = String.Empty
                If isProperty Then
                    value = GetProperty(nameWithoutMarker)
                End If
                Dim p As DbParameter = sql.AddParameter(parameterName, value)
                If (IsSystemSqlProperty(nameWithoutMarker) AndAlso (value Is Nothing)) Then
                    value = String.Empty
                End If
                If ((Not (value) Is Nothing) AndAlso Not (DBNull.Value.Equals(value))) Then
                    p.Direction = ParameterDirection.InputOutput
                    If (TypeOf value Is String AndAlso (CType(value,String).Length < MaximumSizeOfSqlParameter(nameWithoutMarker))) Then
                        p.Size = MaximumSizeOfSqlParameter(nameWithoutMarker)
                    End If
                End If
            End If
            Return true
        End Function
        
        Protected Overridable Function ProcessSystemSqlParameter(ByVal sql As SqlText, ByVal parameterName As String) As Boolean
            Dim nameWithoutMarker As String = parameterName.Substring(1)
            Dim isProperty As Boolean = IsSystemSqlProperty(nameWithoutMarker)
            If ((Array.IndexOf(SystemSqlParameters, nameWithoutMarker) = -1) AndAlso Not (isProperty)) Then
                Return false
            End If
            Dim p As DbParameter = sql.Parameters(parameterName)
            If (nameWithoutMarker = "BusinessRules_PreventDefault") Then
                'prevent standard processing
                If Not (0.Equals(p.Value)) Then
                    PreventDefault()
                End If
            Else
                If (nameWithoutMarker = "Result_Continue") Then
                    'continue standard processing on the client
                    If Not (0.Equals(p.Value)) Then
                        Result.Continue()
                    End If
                Else
                    If isProperty Then
                        Dim currentValue As Object = GetProperty(nameWithoutMarker)
                        If Not ((Convert.ToString(currentValue) = Convert.ToString(p.Value))) Then
                            SetProperty(nameWithoutMarker, p.Value)
                        End If
                    Else
                        Dim s As String = Convert.ToString(p.Value)
                        If Not (String.IsNullOrEmpty(s)) Then
                            If (nameWithoutMarker = "Result_Focus") Then
                                Dim m As Match = Regex.Match(s, "^\s*(?'FieldName'\w+)\s*(,\s*(?'Message'.+))?$")
                                Result.Focus(m.Groups("FieldName").Value, m.Groups("Message").Value)
                            End If
                            If (nameWithoutMarker = "Result_ShowViewMessage") Then
                                Result.ShowViewMessage(s)
                            End If
                            If (nameWithoutMarker = "Result_ShowMessage") Then
                                Result.ShowMessage(s)
                            End If
                            If (nameWithoutMarker = "Result_ShowAlert") Then
                                Result.ShowAlert(s)
                            End If
                            If (nameWithoutMarker = "Result_Error") Then
                                Throw New Exception(s)
                            End If
                            If (nameWithoutMarker = "Result_ExecuteOnClient") Then
                                Result.ExecuteOnClient(s)
                            End If
                            If (nameWithoutMarker = "Result_NavigateUrl") Then
                                Result.NavigateUrl = s
                                Result.Continue()
                            End If
                            If (nameWithoutMarker = "Result_Refresh") Then
                                Result.Refresh()
                            End If
                            If (nameWithoutMarker = "Result_RefreshChildren") Then
                                Result.RefreshChildren()
                            End If
                        End If
                    End If
                End If
            End If
            Return true
        End Function
        
        Protected Overrides Sub ExecuteMethod(ByVal args As ActionArgs, ByVal result As ActionResult, ByVal phase As ActionPhase)
            ExecuteServerRules(args, result, phase)
        End Sub
        
        Public Overloads Sub ExecuteServerRules(ByVal args As ActionArgs, ByVal result As ActionResult, ByVal phase As ActionPhase)
            If (Result.Canceled OrElse args.IgnoreBusinessRules) Then
                Return
            End If
            Me.Arguments = args
            Me.Result = result
            ExecuteServerRules(phase, args.View, args.CommandName, args.CommandArgument)
            If ((phase = ActionPhase.Before) AndAlso Not (Result.Canceled)) Then
                ExecuteServerRules(ActionPhase.Execute, args.View, args.CommandName, args.CommandArgument)
            End If
        End Sub
        
        Public Overloads Sub ExecuteServerRules(ByVal request As PageRequest, ByVal phase As ActionPhase)
            ExecuteServerRules(request, phase, "Select", Nothing)
        End Sub
        
        Public Overloads Sub ExecuteServerRules(ByVal request As PageRequest, ByVal phase As ActionPhase, ByVal commandName As String, ByVal row() As Object)
            m_Request = request
            m_RequestFilter = request.Filter
            m_RequestExternalFilter = request.ExternalFilter
            m_Row = row
            If ((phase = ActionPhase.Execute) AndAlso (commandName = "Select")) Then
                BlobAdapterFactory.InitializeRow(Me.Page, row)
            End If
            ExecuteServerRules(phase, request.View, commandName, String.Empty)
        End Sub
        
        Public Overloads Sub ExecuteServerRules(ByVal request As DistinctValueRequest, ByVal phase As ActionPhase)
            m_RequestFilter = request.Filter
            m_RequestExternalFilter = request.ExternalFilter
            ExecuteServerRules(phase, request.View, "Select", String.Empty)
        End Sub
        
        Protected Overloads Sub ExecuteServerRules(ByVal phase As ActionPhase, ByVal view As String, ByVal commandName As String, ByVal commandArgument As String)
            InternalExecuteServerRules(phase, view, commandName, commandArgument)
        End Sub
        
        Public Function SupportsCommand(ByVal type As String, ByVal commandName As String) As Boolean
            Dim types() As String = type.Split(New Char() {Global.Microsoft.VisualBasic.ChrW(124)}, StringSplitOptions.RemoveEmptyEntries)
            Dim commandNames() As String = commandName.Split(New Char() {Global.Microsoft.VisualBasic.ChrW(124)}, StringSplitOptions.RemoveEmptyEntries)
            For Each t As String in types
                For Each c As String in commandNames
                    Dim ruleIterator As XPathNodeIterator = Config.Select("/c:dataController/c:businessRules/c:rule[@type='{0}']", t)
                    Do While ruleIterator.MoveNext()
                        Dim ruleCommandName As String = ruleIterator.Current.GetAttribute("commandName", String.Empty)
                        If ((ruleCommandName = c) OrElse Regex.IsMatch(c, ruleCommandName)) Then
                            Return true
                        End If
                    Loop
                Next
            Next
            If (commandName = "Select") Then
                Return (Not (Config.SelectSingleNode("/c:dataController/c:fields/c:field[@onDemandHandler!='']")) Is Nothing)
            End If
            Return false
        End Function
        
        Protected Overridable Sub InternalExecuteServerRules(ByVal phase As ActionPhase, ByVal view As String, ByVal commandName As String, ByVal commandArgument As String)
            If (Not (Me.Arguments) Is Nothing) Then
                MyBase.ExecuteMethod(Me.Arguments, Me.Result, phase)
            End If
            Dim iterator As XPathNodeIterator = Config.Select("/c:dataController/c:businessRules/c:rule[@phase='{0}']", phase)
            Do While iterator.MoveNext()
                Dim ruleType As String = iterator.Current.GetAttribute("type", String.Empty)
                Dim ruleView As String = iterator.Current.GetAttribute("view", String.Empty)
                Dim ruleCommandName As String = iterator.Current.GetAttribute("commandName", String.Empty)
                Dim ruleCommandArgument As String = iterator.Current.GetAttribute("commandArgument", String.Empty)
                Dim ruleName As String = iterator.Current.GetAttribute("name", String.Empty)
                If String.IsNullOrEmpty(ruleName) Then
                    ruleName = iterator.Current.GetAttribute("id", String.Empty)
                End If
                Dim skip As Boolean = false
                If Not ((String.IsNullOrEmpty(ruleView) OrElse ((ruleView = view) OrElse Regex.IsMatch(view, ruleView)))) Then
                    skip = true
                End If
                If Not ((String.IsNullOrEmpty(ruleCommandName) OrElse ((ruleCommandName = commandName) OrElse Regex.IsMatch(commandName, ruleCommandName)))) Then
                    skip = true
                End If
                If Not ((String.IsNullOrEmpty(ruleCommandArgument) OrElse ((ruleCommandArgument = commandArgument) OrElse (Not (String.IsNullOrEmpty(commandArgument)) AndAlso Regex.IsMatch(commandArgument, ruleCommandArgument))))) Then
                    skip = true
                End If
                If (Not (skip) AndAlso Not (String.IsNullOrEmpty(ruleName))) Then
                    If (Not (String.IsNullOrEmpty(Whitelist)) AndAlso (Array.IndexOf(Whitelist.Split(New Char() {Global.Microsoft.VisualBasic.ChrW(59), Global.Microsoft.VisualBasic.ChrW(44)}, StringSplitOptions.RemoveEmptyEntries), ruleName) = -1)) Then
                        skip = true
                    End If
                    If (Not (String.IsNullOrEmpty(Blacklist)) AndAlso Not ((Array.IndexOf(Blacklist.Split(New Char() {Global.Microsoft.VisualBasic.ChrW(59), Global.Microsoft.VisualBasic.ChrW(44)}, StringSplitOptions.RemoveEmptyEntries), ruleName) = -1))) Then
                        skip = true
                    End If
                End If
                If Not (skip) Then
                    If (ruleType = "Sql") Then
                        Sql(iterator.Current.Value)
                    End If
                    If (ruleType = "Code") Then
                        ExecuteRule(iterator.Current)
                    End If
                    If (ruleType = "Email") Then
                        Email(iterator.Current.Value)
                    End If
                    BlockRule(ruleName)
                    If Result.Canceled Then
                        Exit Do
                    End If
                End If
            Loop
        End Sub
        
        Private Function ReplaceFieldNamesWithValues(ByVal text As String) As String
            Return Regex.Replace(text, "\{(?'ParameterMarker':|@)?(?'Name'\w+)(\s*,\s*(?'Format'.+?)\s*)?\}", AddressOf DoReplaceFieldNameInText)
        End Function
        
        Private Function DoReplaceFieldNameInText(ByVal m As Match) As String
            Dim v As Object = Nothing
            Dim name As String = m.Groups("Name").Value
            If Not (String.IsNullOrEmpty(m.Groups("ParameterMarker").Value)) Then
                v = GetProperty(name)
            Else
                Dim m2 As Match = Regex.Match(name, "^(?'Name'\w+?)(_(?'ValueType'NewValue|OldValue|Value|Modified))?$")
                name = m2.Groups("Name").Value
                Dim valueType As String = m2.Groups("ValueType").Value
                Dim fv As FieldValue = SelectFieldValueObject(name)
                If (fv Is Nothing) Then
                    Return m.Value
                End If
                v = fv.Value
                If (valueType = "NewValue") Then
                    v = fv.NewValue
                Else
                    If (valueType = "OldValue") Then
                        v = fv.OldValue
                    Else
                        If (valueType = "Modified") Then
                            v = fv.Modified
                        End If
                    End If
                End If
            End If
            Dim format As String = m.Groups("Format").Value
            If Not (String.IsNullOrEmpty(format)) Then
                If Not (format.Contains("}")) Then
                    format = String.Format("{{0:{0}}}", format.Trim())
                End If
                Return String.Format(format, v)
            End If
            Return Convert.ToString(v)
        End Function
        
        Private Function DoReplaceActionParameter(ByVal m As Match) As String
            Dim name As String = m.Groups("Name").Value.ToLower()
            Dim value As String = ReplaceFieldNamesWithValues(m.Groups("Value").Value)
            If Not (m_ActionParameters.ContainsKey(name)) Then
                m_ActionParameters.Add(name, value)
            End If
            Return String.Empty
        End Function
        
        Protected Sub AssignActionParameters(ByVal data As String)
            If Not (EnableEmailMessages) Then
                m_ActionParameters = Nothing
                m_ActionParametersData = data
            End If
        End Sub
        
        Public Overloads Function GetActionParameterByName(ByVal name As String) As String
            Return GetActionParameterByName(name, Nothing)
        End Function
        
        Public Overloads Function GetActionParameterByName(ByVal name As String, ByVal defaultValue As Object) As String
            Dim v As String = Nothing
            If Not (ActionParameters.TryGetValue(name.ToLower(), v)) Then
                Return Convert.ToString(defaultValue)
            End If
            Return v
        End Function
        
        Protected Overloads Overridable Sub Email(ByVal message As DataRow)
            m_ActionParameters = New SortedDictionary(Of String, String)()
            For Each c As DataColumn in message.Table.Columns
                Dim v As Object = message(c.ColumnName)
                If Not (DBNull.Value.Equals(v)) Then
                    Dim loweredName As String = c.ColumnName.ToLower()
                    If (loweredName = "body") Then
                        loweredName = String.Empty
                    End If
                    m_ActionParameters(loweredName) = Convert.ToString(v)
                End If
            Next
            'require "To" and "Subject" to be present
            If (m_ActionParameters.ContainsKey("to") AndAlso m_ActionParameters.ContainsKey("subject")) Then
                Email(String.Empty)
            End If
        End Sub
        
        Protected Overloads Overridable Sub Email(ByVal data As String)
            Email(data, Nothing)
        End Sub
        
        Protected Overloads Overridable Sub Email(ByVal data As String, ByVal message As MailMessage)
            AssignActionParameters(data)
            Dim smtp As SmtpClient = New SmtpClient()
            'configure SMTP properties
            Dim host As String = GetActionParameterByName("Host")
            If Not (String.IsNullOrEmpty(host)) Then
                smtp.Host = host
            End If
            Dim port As String = GetActionParameterByName("Port")
            If Not (String.IsNullOrEmpty(port)) Then
                smtp.Port = Convert.ToInt32(port)
            End If
            Dim enableSsl As String = GetActionParameterByName("EnableSSL")
            If Not (String.IsNullOrEmpty(enableSsl)) Then
                smtp.EnableSsl = (enableSsl.ToLower() = "true")
            End If
            Dim userName As String = GetActionParameterByName("UserName")
            Dim password As String = GetActionParameterByName("Password")
            If Not (String.IsNullOrEmpty(userName)) Then
                smtp.Credentials = New NetworkCredential(userName, password, GetActionParameterByName("Domain"))
            End If
            'configure message properties
            If (message Is Nothing) Then
                message = New MailMessage()
            End If
            ConfigureMailMessage(smtp, message)
            Dim recepient As String = GetActionParameterByName("To")
            AddMailAddresses(message.To, recepient)
            Dim sender As String = GetActionParameterByName("From")
            If Not (String.IsNullOrEmpty(sender)) Then
                message.From = New MailAddress(sender)
            End If
            Dim cc As String = GetActionParameterByName("Cc")
            If Not (String.IsNullOrEmpty(cc)) Then
                AddMailAddresses(message.CC, cc)
            End If
            Dim bcc As String = GetActionParameterByName("Bcc")
            If Not (String.IsNullOrEmpty(cc)) Then
                AddMailAddresses(message.Bcc, bcc)
            End If
            If String.IsNullOrEmpty(message.Subject) Then
                message.Subject = GetActionParameterByName("Subject")
            End If
            If String.IsNullOrEmpty(message.Body) Then
                message.Body = GetActionParameterByName(String.Empty)
            End If
            m_ActionParameters.Clear()
            If Not (String.IsNullOrEmpty(message.Body)) Then
                message.Body = Regex.Replace(message.Body, "<attachment\s+type\s*=s*""(report|file)""\s*>([\s\S]+?)</attachment>", AddressOf DoExtractAttachment)
            End If
            message.IsBodyHtml = Regex.IsMatch(message.Body, "(</\w+>)|(<\w+>)")
            'produce attachments
            For Each key As String in m_ActionParameters.Keys
                Try 
                    Dim nav As XPathNavigator = New XPathDocument(New StringReader(m_ActionParameters(key))).CreateNavigator()
                    Dim attachmentType As String = CType(nav.Evaluate("string(/attachment/@type)"),String)
                    Dim attachmentName As String = CType(nav.Evaluate("string(/attachment/name)"),String)
                    Dim mediaType As String = Nothing
                    Dim attachmentData() As Byte = Nothing
                    If (attachmentType = "report") Then
                        Dim argValue As String
                        Dim args As ReportArgs = New ReportArgs()
                        'controller
                        argValue = CType(nav.Evaluate("string(/attachment/controller)"),String)
                        If Not (String.IsNullOrEmpty(argValue)) Then
                            args.Controller = argValue
                        End If
                        'view
                        argValue = CType(nav.Evaluate("string(/attachment/view)"),String)
                        If Not (String.IsNullOrEmpty(argValue)) Then
                            args.View = argValue
                        End If
                        'template name
                        argValue = CType(nav.Evaluate("string(/attachment/templateName)"),String)
                        If Not (String.IsNullOrEmpty(argValue)) Then
                            args.TemplateName = argValue
                        End If
                        'format
                        argValue = CType(nav.Evaluate("string(/attachment/format)"),String)
                        If Not (String.IsNullOrEmpty(argValue)) Then
                            args.Format = argValue
                        End If
                        'sort expression
                        argValue = CType(nav.Evaluate("string(/attachment/sortExpression)"),String)
                        If Not (String.IsNullOrEmpty(argValue)) Then
                            args.SortExpression = argValue
                        End If
                        'filter details
                        argValue = CType(nav.Evaluate("string(/attachment/filterDetails)"),String)
                        If Not (String.IsNullOrEmpty(argValue)) Then
                            args.FilterDetails = argValue
                        End If
                        'filter
                        Dim filter As List(Of FieldFilter) = New List(Of FieldFilter)()
                        Dim filterIterator As XPathNodeIterator = nav.Select("/attachment/filter/item")
                        Do While filterIterator.MoveNext()
                            Dim ff As FieldFilter = New FieldFilter()
                            ff.FieldName = CType(filterIterator.Current.Evaluate("string(field)"),String)
                            Dim operatorName As String = CType(filterIterator.Current.Evaluate("string(operator)"),String)
                            If Regex.IsMatch(operatorName, "\w+") Then
                                operatorName = String.Format("${0}$", operatorName)
                            End If
                            Dim operatorIndex As Integer = Array.IndexOf(RowFilterAttribute.ComparisonOperations, operatorName)
                            If Not ((operatorIndex = -1)) Then
                                ff.Operation = CType(operatorIndex,RowFilterOperation)
                                Dim values As List(Of Object) = New List(Of Object)()
                                Dim valueIterator As XPathNodeIterator = filterIterator.Current.Select("value")
                                Do While valueIterator.MoveNext()
                                    Dim v As Object = valueIterator.Current.Value
                                    Dim t As String = valueIterator.Current.GetAttribute("type", String.Empty)
                                    If Not (String.IsNullOrEmpty(t)) Then
                                        v = Convert.ChangeType(v, Type.GetType(("System." + t)))
                                    End If
                                    values.Add(v)
                                Loop
                                If (values.Count = 1) Then
                                    ff.Value = values(0)
                                Else
                                    ff.Value = values.ToArray()
                                End If
                                filter.Add(ff)
                            End If
                        Loop
                        args.Filter = filter.ToArray()
                        attachmentData = Report.Execute(args)
                        mediaType = args.MimeType
                        If String.IsNullOrEmpty(attachmentName) Then
                            attachmentName = (args.Controller + key)
                        End If
                        attachmentName = String.Format("{0}.{1}", attachmentName, args.FileNameExtension)
                    End If
                    If (Not (attachmentData) Is Nothing) Then
                        message.Attachments.Add(New Attachment(New MemoryStream(attachmentData), attachmentName, mediaType))
                    End If
                Catch [error] As Exception
                    Dim errorContent As Stream = New MemoryStream()
                    Dim esw As StreamWriter = New StreamWriter(errorContent)
                    esw.Write([error].Message)
                    esw.Flush()
                    errorContent.Position = 0
                    message.Attachments.Add(New Attachment(errorContent, (key + ".txt"), "text/plain"))
                End Try
            Next
            'send message
            Dim workItem As WaitCallback = AddressOf DoSendEmail
            ThreadPool.QueueUserWorkItem(workItem, New Object() {smtp, message, m_Config.CreateBusinessRules()})
        End Sub
        
        Shared Sub DoSendEmail(ByVal state As Object)
            Dim args() As Object = CType(state,Object())
            Dim smtp As SmtpClient = CType(args(0),SmtpClient)
            Dim message As MailMessage = CType(args(1),MailMessage)
            Try 
                smtp.Send(message)
            Catch [error] As Exception
                CType(args(2),BusinessRules).HandleEmailException(smtp, message, [error])
            End Try
        End Sub
        
        Protected Overridable Sub HandleEmailException(ByVal smtp As SmtpClient, ByVal message As MailMessage, ByVal [error] As Exception)
        End Sub
        
        Private Function DoExtractAttachment(ByVal m As Match) As String
            m_ActionParameters.Add((m_ActionParameters.Count + 1).ToString("D3"), m.Value)
            Return String.Empty
        End Function
        
        ''' <summary>
        ''' Adds email addresses with optional display names from the string list to the mail address collection.
        ''' </summary>
        ''' <param name="list">The collection of mail addresses.</param>
        ''' <param name="addresses">The string of addresses separated with comma and semicolon with optional display names.</param>
        Protected Overridable Sub AddMailAddresses(ByVal list As MailAddressCollection, ByVal addresses As String)
            addresses = Regex.Replace(addresses, "(\s*(,|;)\s*(,|;)\s*)+", ",")
            addresses = Regex.Replace(addresses, "(('|"")\s*('|""))", String.Empty)
            Dim address As Match = Regex.Match(addresses, "\s*(?'Email'(("".+"")|('.+'))?(.+?))\s*(,|;|$)")
            Do While address.Success
                Dim m As Match = Regex.Match(address.Groups("Email").Value.Trim(Global.Microsoft.VisualBasic.ChrW(44), Global.Microsoft.VisualBasic.ChrW(59)), "^\s*(((?'DisplayName'.+?)?\s*<\s*(?'Address'.+?@.+?)\s*>)|(?'Address'.+?@.+?))\s*"& _ 
                        "$")
                If m.Success Then
                    list.Add(New MailAddress(m.Groups("Address").Value, m.Groups("DisplayName").Value.Trim(Global.Microsoft.VisualBasic.ChrW(39), Global.Microsoft.VisualBasic.ChrW(34)), Encoding.UTF8))
                End If
                address = address.NextMatch()
            Loop
        End Sub
        
        ''' <summary>
        ''' Configures a new email message with default parameters.
        ''' </summary>
        ''' <param name="smtp">The SMTP client that will send the message.</param>
        ''' <param name="message">The new message with the default configuration</param>
        Protected Overridable Sub ConfigureMailMessage(ByVal smtp As SmtpClient, ByVal message As MailMessage)
        End Sub
        
        Public Overloads Shared Function JavaScriptString(ByVal value As Object) As String
            Return JavaScriptString(value, false)
        End Function
        
        Public Overloads Shared Function JavaScriptString(ByVal value As Object, ByVal addSingleQuotes As Boolean) As String
            Dim s As String = System.Web.HttpUtility.JavaScriptStringEncode(Convert.ToString(value))
            If addSingleQuotes Then
                s = String.Format("'{0}''", s)
            End If
            Return s
        End Function
        
        Protected Overridable Sub ConfigureSqlQuery(ByVal query As SqlText)
        End Sub
        
        Protected Overrides Sub AfterSqlAction(ByVal args As ActionArgs, ByVal result As ActionResult)
            MyBase.AfterSqlAction(args, result)
            ApplicationServices.Create().AfterAction(args, result)
        End Sub
        
        Protected Overrides Sub BeforeSqlAction(ByVal args As ActionArgs, ByVal result As ActionResult)
            ApplicationServices.Create().BeforeAction(args, result)
            MyBase.BeforeSqlAction(args, result)
        End Sub
    End Class
End Namespace

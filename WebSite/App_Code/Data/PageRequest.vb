Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Data.Common
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
    
    <Serializable()>  _
    Public Class PageRequest
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Tag As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_ViewType As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_SupportsCaching As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_RequiresFirstLetters As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_DisableJSONCompatibility As Boolean
        
        Private m_PageIndex As Integer
        
        Private m_PageSize As Integer
        
        Private m_PageOffset As Integer
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_SortExpression As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_GroupExpression As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Filter() As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_SystemFilter() As String
        
        Private m_FilterDetails As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_SyncKey() As Object
        
        Private m_ContextKey As String
        
        Private m_FilterIsExternal As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Cookie As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_RequiresMetaData As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_FieldFilter() As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_MetadataFilter() As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_RequiresSiteContentText As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_RequiresPivot As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_PivotDefinitions As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_RequiresRowCount As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_DoesNotRequireData As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_DoesNotRequireAggregates As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Controller As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_View As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_LastView As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_LookupContextController As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_LookupContextView As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_LookupContextFieldName As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Inserting As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_LastCommandName As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_LastCommandArgument As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_ExternalFilter() As FieldValue
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_QuickFindHint As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_InnerJoinPrimaryKey As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_InnerJoinForeignKey As String
        
        Private m_Transaction As String
        
        Public Sub New()
            MyBase.New
            If ((Not (HttpContext.Current) Is Nothing) AndAlso (Current Is Nothing)) Then
                HttpContext.Current.Items("PageRequest_Current") = Me
            End If
        End Sub
        
        Public Sub New(ByVal pageIndex As Integer, ByVal pageSize As Integer, ByVal sortExpression As String, ByVal filter() As String)
            MyBase.New
            Me.m_PageIndex = pageIndex
            Me.m_PageSize = pageSize
            Me.m_SortExpression = sortExpression
            Me.m_Filter = filter
        End Sub
        
        Public Property Tag() As String
            Get
                Return Me.m_Tag
            End Get
            Set
                Me.m_Tag = value
            End Set
        End Property
        
        Public Property ViewType() As String
            Get
                Return Me.m_ViewType
            End Get
            Set
                Me.m_ViewType = value
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
        
        Public Property RequiresFirstLetters() As Boolean
            Get
                Return Me.m_RequiresFirstLetters
            End Get
            Set
                Me.m_RequiresFirstLetters = value
            End Set
        End Property
        
        Public Property DisableJSONCompatibility() As Boolean
            Get
                Return Me.m_DisableJSONCompatibility
            End Get
            Set
                Me.m_DisableJSONCompatibility = value
            End Set
        End Property
        
        Public Property PageIndex() As Integer
            Get
                Return m_PageIndex
            End Get
            Set
                m_PageIndex = value
            End Set
        End Property
        
        Public Property PageSize() As Integer
            Get
                Return m_PageSize
            End Get
            Set
                m_PageSize = value
            End Set
        End Property
        
        Public Property PageOffset() As Integer
            Get
                Return m_PageOffset
            End Get
            Set
                m_PageOffset = value
            End Set
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
        
        Public Property FilterDetails() As String
            Get
                Return m_FilterDetails
            End Get
            Set
                m_FilterDetails = value
            End Set
        End Property
        
        Public Property SyncKey() As Object()
            Get
                Return Me.m_SyncKey
            End Get
            Set
                Me.m_SyncKey = value
            End Set
        End Property
        
        Public Property ContextKey() As String
            Get
                Return m_ContextKey
            End Get
            Set
                m_ContextKey = value
            End Set
        End Property
        
        Public Property FilterIsExternal() As Boolean
            Get
                Return m_FilterIsExternal
            End Get
            Set
                m_FilterIsExternal = value
            End Set
        End Property
        
        Public Property Cookie() As String
            Get
                Return Me.m_Cookie
            End Get
            Set
                Me.m_Cookie = value
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
        
        Public Overridable Property FieldFilter() As String()
            Get
                Return Me.m_FieldFilter
            End Get
            Set
                Me.m_FieldFilter = value
            End Set
        End Property
        
        Public Overridable Property MetadataFilter() As String()
            Get
                Return Me.m_MetadataFilter
            End Get
            Set
                Me.m_MetadataFilter = value
            End Set
        End Property
        
        Public Property RequiresSiteContentText() As Boolean
            Get
                Return Me.m_RequiresSiteContentText
            End Get
            Set
                Me.m_RequiresSiteContentText = value
            End Set
        End Property
        
        Public Property RequiresPivot() As Boolean
            Get
                Return Me.m_RequiresPivot
            End Get
            Set
                Me.m_RequiresPivot = value
            End Set
        End Property
        
        Public Property PivotDefinitions() As String
            Get
                Return Me.m_PivotDefinitions
            End Get
            Set
                Me.m_PivotDefinitions = value
            End Set
        End Property
        
        Public Property RequiresRowCount() As Boolean
            Get
                Return Me.m_RequiresRowCount
            End Get
            Set
                Me.m_RequiresRowCount = value
            End Set
        End Property
        
        Public Property DoesNotRequireData() As Boolean
            Get
                Return Me.m_DoesNotRequireData
            End Get
            Set
                Me.m_DoesNotRequireData = value
            End Set
        End Property
        
        Public Property DoesNotRequireAggregates() As Boolean
            Get
                Return Me.m_DoesNotRequireAggregates
            End Get
            Set
                Me.m_DoesNotRequireAggregates = value
            End Set
        End Property
        
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
        
        Public Property LastView() As String
            Get
                Return Me.m_LastView
            End Get
            Set
                Me.m_LastView = value
            End Set
        End Property
        
        Public Property LookupContextController() As String
            Get
                Return Me.m_LookupContextController
            End Get
            Set
                Me.m_LookupContextController = value
            End Set
        End Property
        
        Public Property LookupContextView() As String
            Get
                Return Me.m_LookupContextView
            End Get
            Set
                Me.m_LookupContextView = value
            End Set
        End Property
        
        Public Property LookupContextFieldName() As String
            Get
                Return Me.m_LookupContextFieldName
            End Get
            Set
                Me.m_LookupContextFieldName = value
            End Set
        End Property
        
        Public Property Inserting() As Boolean
            Get
                Return Me.m_Inserting
            End Get
            Set
                Me.m_Inserting = value
            End Set
        End Property
        
        Public Property LastCommandName() As String
            Get
                Return Me.m_LastCommandName
            End Get
            Set
                Me.m_LastCommandName = value
            End Set
        End Property
        
        Public Property LastCommandArgument() As String
            Get
                Return Me.m_LastCommandArgument
            End Get
            Set
                Me.m_LastCommandArgument = value
            End Set
        End Property
        
        Public Property ExternalFilter() As FieldValue()
            Get
                Return Me.m_ExternalFilter
            End Get
            Set
                Me.m_ExternalFilter = value
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
        
        Public ReadOnly Property IsModal() As Boolean
            Get
                Return (Not (String.IsNullOrEmpty(m_ContextKey)) AndAlso m_ContextKey.Contains("_ModalDataView"))
            End Get
        End Property
        
        Public Property Transaction() As String
            Get
                Return m_Transaction
            End Get
            Set
                m_Transaction = value
            End Set
        End Property
        
        Public Shared ReadOnly Property Current() As PageRequest
            Get
                Return CType(HttpContext.Current.Items("PageRequest_Current"),PageRequest)
            End Get
        End Property
        
        Public Sub AssignContext(ByVal controller As String, ByVal view As String, ByVal config As ControllerConfiguration)
            m_Controller = controller
            m_View = view
            Dim referrer As String = String.Empty
            If (PageSize = 1000) Then
                'we are processing a request to retrieve static lookup data
                Dim sortExpressionNode As XPathNavigator = config.SelectSingleNode("c:dataController/c:views/c:view[@id='{0}']/@sortExpression", view)
                If ((Not (sortExpressionNode) Is Nothing) AndAlso Not (String.IsNullOrEmpty(sortExpressionNode.Value))) Then
                    Me.SortExpression = sortExpressionNode.Value
                End If
            End If
            If ((Not (HttpContext.Current) Is Nothing) AndAlso (Not (HttpContext.Current.Request.UrlReferrer) Is Nothing)) Then
                referrer = HttpContext.Current.Request.UrlReferrer.AbsolutePath
            End If
            Me.m_ContextKey = String.Format("{0}/{1}.{2}.{3}", referrer, controller, view, ContextKey)
        End Sub
    End Class
End Namespace

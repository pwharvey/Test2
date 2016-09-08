Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data.Common

Namespace MyCompany.Data
    
    Public Class SelectClauseDictionary
        Inherits SortedDictionary(Of String, String)
        
        Public Shadows Default Property Item(ByVal name As String) As String
            Get
                Dim expression As String = Nothing
                If TryGetValue(name.ToLower(), expression) Then
                    Return expression
                End If
                Return "null"
            End Get
            Set
                MyBase.Item(name.ToLower()) = value
            End Set
        End Property
        
        Public Shadows Function ContainsKey(ByVal name As String) As Boolean
            Return MyBase.ContainsKey(name.ToLower())
        End Function
        
        Public Shadows Sub Add(ByVal key As String, ByVal value As String)
            MyBase.Add(key.ToLower(), value)
        End Sub
        
        Public Shadows Function TryGetValue(ByVal key As String, ByRef value As String) As Boolean
            Return MyBase.TryGetValue(key.ToLower(), value)
        End Function
    End Class
    
    Public Interface IDataController
        
        Function GetPage(ByVal controller As String, ByVal view As String, ByVal request As PageRequest) As ViewPage
        
        Function GetListOfValues(ByVal controller As String, ByVal view As String, ByVal request As DistinctValueRequest) As Object()
        
        Function Execute(ByVal controller As String, ByVal view As String, ByVal args As ActionArgs) As ActionResult
    End Interface
    
    Public Interface IAutoCompleteManager
        
        Function GetCompletionList(ByVal prefixText As String, ByVal count As Integer, ByVal contextKey As String) As String()
    End Interface
    
    Public Interface IActionHandler
        
        Sub BeforeSqlAction(ByVal args As ActionArgs, ByVal result As ActionResult)
        
        Sub AfterSqlAction(ByVal args As ActionArgs, ByVal result As ActionResult)
        
        Sub ExecuteAction(ByVal args As ActionArgs, ByVal result As ActionResult)
    End Interface
    
    Public Interface IRowHandler
        
        Function SupportsNewRow(ByVal requet As PageRequest) As Boolean
        
        Sub NewRow(ByVal request As PageRequest, ByVal page As ViewPage, ByVal row() As Object)
        
        Function SupportsPrepareRow(ByVal request As PageRequest) As Boolean
        
        Sub PrepareRow(ByVal request As PageRequest, ByVal page As ViewPage, ByVal row() As Object)
    End Interface
    
    Public Interface IDataFilter
        
        Sub Filter(ByVal filter As SortedDictionary(Of String, Object))
    End Interface
    
    Public Interface IDataFilter2
        
        Sub Filter(ByVal controller As String, ByVal view As String, ByVal filter As SortedDictionary(Of String, Object))
        
        Sub AssignContext(ByVal controller As String, ByVal view As String, ByVal lookupContextController As String, ByVal lookupContextView As String, ByVal lookupContextFieldName As String)
    End Interface
    
    Public Interface IDataEngine
        
        Function ExecuteReader(ByVal request As PageRequest) As DbDataReader
    End Interface
    
    Public Interface IPlugIn
        
        Property Config() As ControllerConfiguration
        
        Function Create(ByVal config As ControllerConfiguration) As ControllerConfiguration
        
        Sub PreProcessPageRequest(ByVal request As PageRequest, ByVal page As ViewPage)
        
        Sub ProcessPageRequest(ByVal request As PageRequest, ByVal page As ViewPage)
        
        Sub PreProcessArguments(ByVal args As ActionArgs, ByVal result As ActionResult, ByVal page As ViewPage)
        
        Sub ProcessArguments(ByVal args As ActionArgs, ByVal result As ActionResult, ByVal page As ViewPage)
    End Interface
    
    Public Class BusinessObjectParameters
        Inherits SortedDictionary(Of String, Object)
        
        Public Sub New(ByVal ParamArray values() as Object)
            MyBase.New
            Dim parameterMarker As String = Nothing
            Dim i As Integer = 0
            Do While (i < values.Length)
                Dim v As Object = values(i)
                If TypeOf v Is FieldValue Then
                    Dim fv As FieldValue = CType(v,FieldValue)
                    Add(fv.Name, fv.Value)
                Else
                    If String.IsNullOrEmpty(parameterMarker) Then
                        parameterMarker = SqlStatement.GetParameterMarker(String.Empty)
                    End If
                    Add((parameterMarker  _
                                    + ("p" + i.ToString())), v)
                End If
                i = (i + 1)
            Loop
        End Sub
    End Class
    
    Public Interface IBusinessObject
        
        Sub AssignFilter(ByVal filter As String, ByVal parameters As BusinessObjectParameters)
    End Interface
    
    Public Enum CommandConfigurationType
        
        [Select]
        
        Update
        
        Insert
        
        Delete
        
        SelectCount
        
        SelectDistinct
        
        SelectAggregates
        
        SelectFirstLetters
        
        SelectExisting
        
        Sync
        
        None
    End Enum
End Namespace

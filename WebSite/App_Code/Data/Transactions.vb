Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Data.Common
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

Namespace MyCompany.Data
    
    Public Class TransactionManager
        
        Private m_Transaction As String
        
        Private m_Status As String
        
        Private m_Tables As SortedDictionary(Of String, DataTable)
        
        Private m_PrimaryKeys As SortedDictionary(Of String, Object)
        
        Private m_Arguments As List(Of ActionArgs)
        
        Public Sub New(ByVal transaction As String)
            MyBase.New
            m_Transaction = transaction
            m_Tables = New SortedDictionary(Of String, DataTable)()
            m_PrimaryKeys = New SortedDictionary(Of String, Object)()
            m_Arguments = New List(Of ActionArgs)()
        End Sub
        
        Public Property Transaction() As String
            Get
                Return m_Transaction
            End Get
            Set
                m_Transaction = value
            End Set
        End Property
        
        Public Property Status() As String
            Get
                Return m_Status
            End Get
            Set
                m_Status = value
            End Set
        End Property
        
        Public ReadOnly Property Tables() As SortedDictionary(Of String, DataTable)
            Get
                Return m_Tables
            End Get
        End Property
        
        Public ReadOnly Property PrimaryKeys() As SortedDictionary(Of String, Object)
            Get
                Return m_PrimaryKeys
            End Get
        End Property
        
        Public ReadOnly Property Arguments() As List(Of ActionArgs)
            Get
                Return m_Arguments
            End Get
        End Property
        
        Public Function GetTable(ByVal controller As String, ByVal command As DbCommand) As DataTable
            Dim t As DataTable = Nothing
            If Not (Tables.TryGetValue(controller, t)) Then
                Dim p As DbParameter = command.Parameters("@PageRangeLastRowNumber")
                If (Not (p) Is Nothing) Then
                    p.Value = 500
                End If
                t = New DataTable(controller)
                t.Load(command.ExecuteReader())
                For Each c As DataColumn in t.Columns
                    c.AllowDBNull = true
                Next
                Tables.Add(controller, t)
            End If
            Return t
        End Function
        
        Public Sub Delete()
            HttpContext.Current.Session.Remove(("TransactionManager_" + m_Transaction))
        End Sub
        
        Public Shared Function Create(ByVal transaction As String) As TransactionManager
            If String.IsNullOrEmpty(transaction) Then
                Return Nothing
            End If
            Dim transactionInfo() As String = transaction.Split(Global.Microsoft.VisualBasic.ChrW(58))
            transaction = transactionInfo(0)
            Dim key As String = ("TransactionManager_" + transaction)
            Dim tm As TransactionManager = CType(HttpContext.Current.Session(key),TransactionManager)
            If (tm Is Nothing) Then
                tm = New TransactionManager(transaction)
                HttpContext.Current.Session(key) = tm
            End If
            If (transactionInfo.Length = 2) Then
                tm.m_Status = transactionInfo(1)
            End If
            If (tm.Status = "abort") Then
                tm.Delete()
                Return Nothing
            End If
            Return tm
        End Function
        
        Private Shared Function RowIsMatched(ByVal request As PageRequest, ByVal page As ViewPage, ByVal row As DataRow) As Boolean
            For Each f As String in request.Filter
                Dim m As Match = Regex.Match(f, "^(?'FieldName'\w+)\:(?'Operation'=)(?'Value'.+)$")
                If m.Success Then
                    Dim fieldName As String = m.Groups("FieldName").Value
                    If page.ContainsField(fieldName) Then
                        Dim fieldValue As String = m.Groups("Value").Value
                        If (fieldValue = "null") Then
                            fieldValue = String.Empty
                        End If
                        Dim fv As Object = row(fieldName)
                        If Not ((Convert.ToString(fv) = fieldValue)) Then
                            Return false
                        End If
                    End If
                End If
            Next
            Return true
        End Function
        
        Public Shared Function ExecuteReader(ByVal request As PageRequest, ByVal page As ViewPage, ByVal command As DbCommand) As DbDataReader
            Dim tm As TransactionManager = Create(request.Transaction)
            If (tm Is Nothing) Then
                Return command.ExecuteReader()
            End If
            Dim t As DataTable = tm.GetTable(request.Controller, command)
            t.DefaultView.Sort = request.SortExpression
            Dim t2 As DataTable = t.Clone()
            Dim rowsToSkip As Integer = (page.PageIndex * page.PageSize)
            For Each r As DataRowView in t.DefaultView
                If RowIsMatched(request, page, r.Row) Then
                    If (rowsToSkip > 0) Then
                        rowsToSkip = (rowsToSkip + 1)
                    Else
                        Dim r2 As DataRow = t2.NewRow()
                        For Each c As DataColumn in t.Columns
                            r2(c.ColumnName) = r(c.ColumnName)
                        Next
                        t2.Rows.Add(r2)
                        If (t2.Rows.Count = page.PageSize) Then
                            Exit For
                        End If
                    End If
                End If
            Next
            If page.RequiresRowCount Then
                Dim totalRowCount As Integer = 0
                For Each r As DataRowView in t.DefaultView
                    If RowIsMatched(request, page, r.Row) Then
                        totalRowCount = (totalRowCount + 1)
                    End If
                Next
                page.TotalRowCount = totalRowCount
            End If
            Return New DataTableReader(t2)
        End Function
        
        Public Overloads Shared Function ExecuteNonQuery(ByVal args As ActionArgs, ByVal result As ActionResult, ByVal page As ViewPage, ByVal command As DbCommand) As Integer
            Dim tm As TransactionManager = Create(args.Transaction)
            If (tm Is Nothing) Then
                Return command.ExecuteNonQuery()
            Else
                If (tm.Status = "complete") Then
                    Return command.ExecuteNonQuery()
                End If
            End If
            Dim rowsAffected As Integer = tm.ExecuteAction(args, result, page)
            tm.Arguments.Add(args)
            Return rowsAffected
        End Function
        
        Public Overloads Shared Sub Complete(ByVal args As ActionArgs, ByVal result As ActionResult, ByVal page As ViewPage)
            Dim tm As TransactionManager = Create(args.Transaction)
            If ((Not (tm) Is Nothing) AndAlso (tm.Status = "complete")) Then
                tm.Complete(result)
            End If
        End Sub
        
        Public Shared Function InTransaction(ByVal args As ActionArgs) As Boolean
            Return (Not (String.IsNullOrEmpty(args.Transaction)) AndAlso Not (args.Transaction.EndsWith(":complete")))
        End Function
        
        Public Overloads Sub Complete(ByVal result As ActionResult)
            Dim controller As IDataController = ControllerFactory.CreateDataController()
            Dim keys As SortedDictionary(Of String, Object) = New SortedDictionary(Of String, Object)()
            For Each args As ActionArgs in Arguments
                args.Transaction = Nothing
                'resolve foreign keys
                For Each v As FieldValue in result.Values
                    Dim v2 As FieldValue = args.SelectFieldValueObject(v.Name)
                    If (v2 Is Nothing) Then
                        Dim values As List(Of FieldValue) = New List(Of FieldValue)(args.Values)
                        values.Add(New FieldValue(v.Name, v.Value))
                        args.Values = values.ToArray()
                    Else
                        v2.NewValue = v.Value
                        v2.Modified = true
                    End If
                Next
                'resolve virtual primary keys
                If ((args.CommandName = "Update") OrElse (args.CommandName = "Delete")) Then
                    For Each v As FieldValue in args.Values
                        Dim key As Object = Nothing
                        If keys.TryGetValue(String.Format("{0}:{1}:{2}", args.Controller, v.Name, v.Value), key) Then
                            v.OldValue = key
                        End If
                    Next
                End If
                'execute an actual update and raise exception if errors detected
                Dim argValues As SortedDictionary(Of String, Object) = New SortedDictionary(Of String, Object)()
                If (args.CommandName = "Insert") Then
                    For Each v As FieldValue in args.Values
                        argValues.Add(v.Name, v.Value)
                    Next
                End If
                Dim r As ActionResult = controller.Execute(args.Controller, args.View, args)
                r.RaiseExceptionIfErrors()
                'register physical primary keys
                If (args.CommandName = "Insert") Then
                    For Each v As FieldValue in r.Values
                        If argValues.ContainsKey(v.Name) Then
                            keys.Add(String.Format("{0}:{1}:{2}", args.Controller, v.Name, argValues(v.Name)), v.NewValue)
                        End If
                    Next
                End If
            Next
        End Sub
        
        Public Function ExecuteAction(ByVal args As ActionArgs, ByVal result As ActionResult, ByVal page As ViewPage) As Integer
            Dim t As DataTable = GetTable(args.Controller, Nothing)
            If (args.CommandName = "Insert") Then
                Dim r As DataRow = t.NewRow()
                For Each v As FieldValue in args.Values
                    Dim f As DataField = page.FindField(v.Name)
                    If (f.IsPrimaryKey AndAlso f.ReadOnly) Then
                        Dim key As Object = Nothing
                        If (f.Type = "Guid") Then
                            key = Guid.NewGuid()
                        Else
                            If Not (PrimaryKeys.TryGetValue(args.Controller, key)) Then
                                key = -1
                                PrimaryKeys.Add(args.Controller, key)
                            Else
                                key = (Convert.ToInt32(key) - 1)
                                PrimaryKeys(args.Controller) = key
                            End If
                        End If
                        r(v.Name) = key
                        result.Values.Add(New FieldValue(v.Name, key))
                        Dim fv As FieldValue = args.SelectFieldValueObject(v.Name)
                        fv.NewValue = key
                        fv.Modified = true
                    Else
                        If v.Modified Then
                            If (v.NewValue Is Nothing) Then
                                r(v.Name) = DBNull.Value
                            Else
                                r(v.Name) = v.NewValue
                            End If
                        End If
                    End If
                Next
                t.Rows.Add(r)
                Return 1
            Else
                Dim targetRow As DataRow = Nothing
                For Each r As DataRow in t.Rows
                    Dim matched As Boolean = true
                    For Each f As DataField in page.Fields
                        If f.IsPrimaryKey Then
                            Dim kv As Object = r(f.Name)
                            Dim kv2 As Object = args.SelectFieldValueObject(f.Name).OldValue
                            If (((kv Is Nothing) OrElse (kv2 Is Nothing)) OrElse Not ((kv.ToString() = kv2.ToString()))) Then
                                matched = false
                                Exit For
                            End If
                        End If
                    Next
                    If matched Then
                        targetRow = r
                        Exit For
                    End If
                Next
                If (targetRow Is Nothing) Then
                    Return 0
                End If
                If (args.CommandName = "Delete") Then
                    t.Rows.Remove(targetRow)
                Else
                    For Each v As FieldValue in args.Values
                        If v.Modified Then
                            If (v.NewValue Is Nothing) Then
                                targetRow(v.Name) = DBNull.Value
                            Else
                                targetRow(v.Name) = v.NewValue
                            End If
                        End If
                    Next
                End If
                Return 1
            End If
        End Function
        
        Public Overloads Shared Function ExecuteNonQuery(ByVal command As DbCommand) As Integer
            Dim rowsAffected As Integer = command.ExecuteNonQuery()
            For Each p As DbParameter in command.Parameters
                If (p.Direction = ParameterDirection.ReturnValue) Then
                    Integer.TryParse(Convert.ToString(p.Value), rowsAffected)
                    Exit For
                End If
            Next
            If (rowsAffected = -1) Then
                rowsAffected = 1
            End If
            Return rowsAffected
        End Function
    End Class
    
    Public Class SinglePhaseTransactionScope
        Inherits Object
        Implements IDisposable
        
        Private m_Connections As SortedDictionary(Of String, DbConnection)
        
        Private m_Transaction As DbTransaction
        
        Private m_IsRoot As Boolean
        
        Public Sub New()
            MyBase.New
            m_Connections = New SortedDictionary(Of String, DbConnection)()
            If (Current Is Nothing) Then
                m_IsRoot = true
                HttpContext.Current.Items("SinglePhaseTransactionScope_Current") = Me
            End If
        End Sub
        
        Public Shared ReadOnly Property Current() As SinglePhaseTransactionScope
            Get
                If (HttpContext.Current Is Nothing) Then
                    Return Nothing
                End If
                Return CType(HttpContext.Current.Items("SinglePhaseTransactionScope_Current"),SinglePhaseTransactionScope)
            End Get
        End Property
        
        Public ReadOnly Property Connections() As SortedDictionary(Of String, DbConnection)
            Get
                Return m_Connections
            End Get
        End Property
        
        Public ReadOnly Property Transaction() As DbTransaction
            Get
                Return m_Transaction
            End Get
        End Property
        
        Public ReadOnly Property IsRoot() As Boolean
            Get
                Return m_IsRoot
            End Get
        End Property
        
        Public Overloads Sub Enlist(ByVal command As DbCommand)
            Dim connection As DbConnection = Nothing
            If Not (Current.Connections.TryGetValue(command.Connection.ConnectionString, connection)) Then
                connection = command.Connection
                m_Transaction = connection.BeginTransaction()
                command.Transaction = m_Transaction
                Current.Connections.Add(command.Connection.ConnectionString, connection)
            Else
                command.Connection = connection
                command.Transaction = Current.Transaction
            End If
        End Sub
        
        Public Overloads Sub Enlist(ByVal connection As DbConnection)
            If Not (Current.Connections.ContainsKey(connection.ConnectionString)) Then
                m_Transaction = connection.BeginTransaction()
                Current.Connections.Add(connection.ConnectionString, connection)
            End If
        End Sub
        
        Public Sub Complete()
            If (m_IsRoot AndAlso (Not (m_Transaction) Is Nothing)) Then
                m_Transaction.Commit()
            End If
        End Sub
        
        Public Sub Rollback()
            If (m_IsRoot AndAlso (Not (m_Transaction) Is Nothing)) Then
                m_Transaction.Rollback()
            End If
        End Sub
        
        Sub IDisposable_Dispose() Implements IDisposable.Dispose
            m_Connections.Clear()
            If m_IsRoot Then
                HttpContext.Current.Items("SinglePhaseTransactionScope_Current") = Nothing
            End If
        End Sub
    End Class
End Namespace

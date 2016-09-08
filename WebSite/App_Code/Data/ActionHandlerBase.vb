Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Linq
Imports System.Reflection
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml.XPath

Namespace MyCompany.Data
    
    ''' <summary>
    ''' Links a data controller business rule to a method with its implementation.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=true, Inherited:=true)>  _
    Public Class RuleAttribute
        Inherits Attribute
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Id As String
        
        ''' <summary>
        ''' Links the method to the business rule by its Id.
        ''' </summary>
        ''' <param name="id">The Id of the data controller business rule.</param>
        Public Sub New(ByVal id As String)
            MyBase.New
            Me.Id = id
        End Sub
        
        ''' <summary>
        ''' The Id of the data controller business rule.
        ''' </summary>
        Public Property Id() As String
            Get
                Return Me.m_Id
            End Get
            Set
                Me.m_Id = value
            End Set
        End Property
    End Class
    
    Public Class ActionHandlerBase
        Inherits Object
        Implements MyCompany.Data.IActionHandler
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Arguments As ActionArgs
        
        Private m_Result As ActionResult
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Whitelist As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Blacklist As String
        
        Public Property Arguments() As ActionArgs
            Get
                Return Me.m_Arguments
            End Get
            Set
                Me.m_Arguments = value
            End Set
        End Property
        
        Public Property Result() As ActionResult
            Get
                If (m_Result Is Nothing) Then
                    m_Result = New ActionResult()
                End If
                Return m_Result
            End Get
            Set
                m_Result = value
            End Set
        End Property
        
        Public Property Whitelist() As String
            Get
                Return Me.m_Whitelist
            End Get
            Set
                Me.m_Whitelist = value
            End Set
        End Property
        
        Public Property Blacklist() As String
            Get
                Return Me.m_Blacklist
            End Get
            Set
                Me.m_Blacklist = value
            End Set
        End Property
        
        Public Sub PreventDefault()
            If (Not (m_Result) Is Nothing) Then
                m_Result.Canceled = true
            End If
        End Sub
        
        Public Sub ClearBlackAndWhiteLists()
            m_Blacklist = String.Empty
            m_Whitelist = String.Empty
        End Sub
        
        Protected Sub AddToWhitelist(ByVal rule As String)
            m_Whitelist = UpdateNameList(m_Whitelist, rule, true)
        End Sub
        
        Protected Sub RemoveFromWhitelist(ByVal rule As String)
            m_Whitelist = UpdateNameList(m_Whitelist, rule, false)
        End Sub
        
        Protected Sub AddToBlacklist(ByVal rule As String)
            m_Blacklist = UpdateNameList(m_Blacklist, rule, true)
        End Sub
        
        Protected Sub RemoveFromBlacklist(ByVal rule As String)
            m_Blacklist = UpdateNameList(m_Blacklist, rule, false)
        End Sub
        
        Private Function UpdateNameList(ByVal listOfNames As String, ByVal name As String, ByVal add As Boolean) As String
            If (listOfNames Is Nothing) Then
                listOfNames = String.Empty
            End If
            Dim nameList As List(Of String) = New List(Of String)(Regex.Split(listOfNames, "(\s*(,|;)\s*)"))
            If Not (add) Then
                nameList.Remove(name)
            Else
                If Not (nameList.Contains(name)) Then
                    nameList.Add(name)
                End If
            End If
            Dim sb As StringBuilder = New StringBuilder()
            For Each s As String in nameList
                If Not (String.IsNullOrEmpty(s)) Then
                    If (sb.Length > 0) Then
                        sb.Append(",")
                    End If
                    sb.Append(s)
                End If
            Next
            Return sb.ToString()
        End Function
        
        <System.Diagnostics.DebuggerStepThrough()>  _
        Protected Overridable Sub ExecuteMethod(ByVal args As ActionArgs, ByVal result As ActionResult, ByVal phase As ActionPhase)
            Dim match As Boolean = InternalExecuteMethod(args, result, phase, true, true)
            If Not (match) Then
                match = InternalExecuteMethod(args, result, phase, true, false)
            End If
            If Not (match) Then
                match = InternalExecuteMethod(args, result, phase, false, true)
            End If
            If Not (match) Then
                InternalExecuteMethod(args, result, phase, false, false)
            End If
        End Sub
        
        Private Function InternalExecuteMethod(ByVal args As ActionArgs, ByVal result As ActionResult, ByVal phase As ActionPhase, ByVal viewMatch As Boolean, ByVal argumentMatch As Boolean) As Boolean
            m_Arguments = args
            m_Result = result
            Dim success As Boolean = false
            Dim methods() As MethodInfo = [GetType]().GetMethods((BindingFlags.Public Or (BindingFlags.NonPublic Or BindingFlags.Instance)))
            For Each method As MethodInfo in methods
                Dim filters() As Object = method.GetCustomAttributes(GetType(ControllerActionAttribute), true)
                For Each action As ControllerActionAttribute in filters
                    If (((action.Controller = args.Controller) OrElse (Not (String.IsNullOrEmpty(args.Controller)) AndAlso Regex.IsMatch(args.Controller, action.Controller))) AndAlso ((Not (viewMatch) AndAlso String.IsNullOrEmpty(action.View)) OrElse (action.View = args.View))) Then
                        If ((action.CommandName = args.CommandName) AndAlso ((Not (argumentMatch) AndAlso String.IsNullOrEmpty(action.CommandArgument)) OrElse (action.CommandArgument = args.CommandArgument))) Then
                            If (action.Phase = phase) Then
                                Dim parameters() As ParameterInfo = method.GetParameters()
                                If ((parameters.Length = 2) AndAlso ((parameters(0).ParameterType Is GetType(ActionArgs)) AndAlso (parameters(1).ParameterType Is GetType(ActionResult)))) Then
                                    method.Invoke(Me, New Object() {args, result})
                                Else
                                    Dim arguments((parameters.Length) - 1) As Object
                                    Dim i As Integer = 0
                                    Do While (i < parameters.Length)
                                        Dim p As ParameterInfo = parameters(i)
                                        Dim v As FieldValue = SelectFieldValueObject(p.Name)
                                        If (Not (v) Is Nothing) Then
                                            If p.ParameterType.Equals(GetType(FieldValue)) Then
                                                arguments(i) = v
                                            Else
                                                Try 
                                                    arguments(i) = DataControllerBase.ConvertToType(p.ParameterType, v.Value)
                                                Catch __exception As Exception
                                                End Try
                                            End If
                                        End If
                                        i = (i + 1)
                                    Loop
                                    method.Invoke(Me, arguments)
                                    success = true
                                End If
                            End If
                        End If
                    End If
                Next
            Next
            Return success
        End Function
        
        Protected Overridable Sub BeforeSqlAction(ByVal args As ActionArgs, ByVal result As ActionResult)
        End Sub
        
        Protected Overridable Sub AfterSqlAction(ByVal args As ActionArgs, ByVal result As ActionResult)
        End Sub
        
        Protected Overridable Sub ExecuteAction(ByVal args As ActionArgs, ByVal result As ActionResult)
        End Sub
        
        Sub IActionHandler_BeforeSqlAction(ByVal args As ActionArgs, ByVal result As ActionResult) Implements IActionHandler.BeforeSqlAction
            ExecuteMethod(args, result, ActionPhase.Before)
            BeforeSqlAction(args, result)
        End Sub
        
        Sub IActionHandler_AfterSqlAction(ByVal args As ActionArgs, ByVal result As ActionResult) Implements IActionHandler.AfterSqlAction
            ExecuteMethod(args, result, ActionPhase.After)
            AfterSqlAction(args, result)
        End Sub
        
        Sub IActionHandler_ExecuteAction(ByVal args As ActionArgs, ByVal result As ActionResult) Implements IActionHandler.ExecuteAction
            ExecuteMethod(args, result, ActionPhase.Execute)
            ExecuteAction(args, result)
        End Sub
        
        Public Overridable Function SelectFieldValueObject(ByVal name As String) As FieldValue
            Return Nothing
        End Function
        
        Protected Overridable Function BuildingDataRows() As Boolean
            Return false
        End Function
        
        Protected Overloads Overridable Sub ExecuteRule(ByVal rule As XPathNavigator)
            ExecuteRule(rule.GetAttribute("id", String.Empty))
        End Sub
        
        Protected Overridable Sub BlockRule(ByVal id As String)
            If Not (BuildingDataRows()) Then
                AddToBlacklist(id)
            End If
        End Sub
        
        Protected Overloads Overridable Sub ExecuteRule(ByVal ruleId As String)
            Dim methods() As MethodInfo = [GetType]().GetMethods((BindingFlags.Public Or (BindingFlags.NonPublic Or BindingFlags.Instance)))
            For Each method As MethodInfo in methods
                Dim ruleBindings() As Object = method.GetCustomAttributes(GetType(RuleAttribute), true)
                For Each ra As RuleAttribute in ruleBindings
                    If (ra.Id = ruleId) Then
                        BlockRule(ruleId)
                        Dim parameters() As ParameterInfo = method.GetParameters()
                        Dim arguments((parameters.Length) - 1) As Object
                        Dim i As Integer = 0
                        Do While (i < parameters.Length)
                            Dim p As ParameterInfo = parameters(i)
                            If ((parameters.Length = 1) AndAlso p.ParameterType.IsSubclassOf(GetType(BusinessRulesObjectModel))) Then
                                Dim self As Object = p.ParameterType.Assembly.CreateInstance(p.ParameterType.FullName, true, BindingFlags.CreateInstance, Nothing, New Object() {Me}, System.Globalization.CultureInfo.CurrentCulture, Nothing)
                                Dim fields() As System.Reflection.FieldInfo = self.GetType().GetFields((BindingFlags.Instance Or BindingFlags.NonPublic))
                                For Each fi As System.Reflection.FieldInfo in fields
                                    Dim fieldName As String = fi.Name.Substring("m_".Length)
                                    If (fieldName.Length = 1) Then
                                        fieldName = fieldName.ToUpper()
                                    Else
                                        fieldName = (Char.ToUpper(fieldName(0)) + fieldName.Substring(1))
                                    End If
                                    Dim v As FieldValue = SelectFieldValueObject(fieldName)
                                    If (Not (v) Is Nothing) Then
                                        Try 
                                            self.GetType().InvokeMember(fi.Name, (BindingFlags.SetField Or (BindingFlags.Instance Or BindingFlags.NonPublic)), Nothing, self, New Object() {DataControllerBase.ConvertToType(fi.FieldType, v.Value)})
                                        Finally
                                            'release resources here
                                        End Try
                                    End If
                                Next
                                arguments(i) = self
                            Else
                                Dim v As FieldValue = SelectFieldValueObject(p.Name)
                                If (Not (v) Is Nothing) Then
                                    If p.ParameterType.Equals(GetType(FieldValue)) Then
                                        arguments(i) = v
                                    Else
                                        Try 
                                            arguments(i) = DataControllerBase.ConvertToType(p.ParameterType, v.Value)
                                        Catch __exception As Exception
                                        End Try
                                    End If
                                End If
                            End If
                            i = (i + 1)
                        Loop
                        method.Invoke(Me, arguments)
                    End If
                Next
            Next
        End Sub
        
        ''' <summary>
        ''' Returns True if there are no field values with errors.
        ''' </summary>
        ''' <returns>True if all field values have a blank 'Error' property.</returns>
        Protected Function ValidateInput() As Boolean
            If (Not (Arguments) Is Nothing) Then
                For Each v As FieldValue in Arguments.Values
                    If Not (String.IsNullOrEmpty(v.Error)) Then
                        Return false
                    End If
                Next
            End If
            Return true
        End Function
    End Class
    
    Public Class BusinessRulesObjectModel
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Rules As BusinessRules
        
        Public Sub New()
            MyBase.New
        End Sub
        
        Public Sub New(ByVal rules As BusinessRules)
            MyBase.New
            Me.m_Rules = rules
        End Sub
        
        Public Default ReadOnly Property Item(ByVal name As String) As FieldValue
            Get
                If (m_Rules Is Nothing) Then
                    Return Nothing
                End If
                Return m_Rules.SelectFieldValueObject(name)
            End Get
        End Property
        
        Protected Overridable Sub UpdateFieldValue(ByVal fieldName As String, ByVal value As Object)
            If (Not (Me.m_Rules) Is Nothing) Then
                Me.m_Rules.UpdateFieldValue(fieldName, value)
            End If
        End Sub
    End Class
End Namespace

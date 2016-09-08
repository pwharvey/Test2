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
    Public Class FieldValue
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Name As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_OldValue As Object
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_NewValue As Object
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_NoCheck As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Modified As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_ReadOnly As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Error As String
        
        Private m_EnableConversion As Boolean = true
        
        Public Sub New()
            MyBase.New
        End Sub
        
        Public Sub New(ByVal fieldName As String)
            MyBase.New
            Me.m_Name = fieldName
        End Sub
        
        Public Sub New(ByVal fieldName As String, ByVal newValue As Object)
            Me.New(fieldName, Nothing, newValue)
        End Sub
        
        Public Sub New(ByVal fieldName As String, ByVal oldValue As Object, ByVal newValue As Object)
            MyBase.New
            Me.m_Name = fieldName
            Me.m_OldValue = oldValue
            Me.m_NewValue = newValue
            CheckModified()
        End Sub
        
        Public Sub New(ByVal fieldName As String, ByVal oldValue As Object, ByVal newValue As Object, ByVal [readOnly] As Boolean)
            Me.New(fieldName, oldValue, newValue)
            Me.m_ReadOnly = [readOnly]
        End Sub
        
        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set
                m_Name = value
            End Set
        End Property
        
        Public Property OldValue() As Object
            Get
                Return m_OldValue
            End Get
            Set
                If (m_EnableConversion AndAlso TypeOf value Is String) Then
                    m_OldValue = Controller.StringToValue(CType(value,String))
                Else
                    m_OldValue = value
                End If
            End Set
        End Property
        
        Public Property NewValue() As Object
            Get
                Return m_NewValue
            End Get
            Set
                If (m_EnableConversion AndAlso TypeOf value Is String) Then
                    m_NewValue = Controller.StringToValue(CType(value,String))
                Else
                    m_NewValue = value
                End If
            End Set
        End Property
        
        Public Property Modified() As Boolean
            Get
                Return (m_Modified AndAlso Not ([ReadOnly]))
            End Get
            Set
                m_Modified = value
                m_NoCheck = true
            End Set
        End Property
        
        Public Property [ReadOnly]() As Boolean
            Get
                Return m_ReadOnly
            End Get
            Set
                m_ReadOnly = value
            End Set
        End Property
        
        Public Property Value() As Object
            Get
                CheckModified()
                If (Modified OrElse [ReadOnly]) Then
                    Return NewValue
                Else
                    Return OldValue
                End If
            End Get
            Set
                OldValue = value
                Modified = false
            End Set
        End Property
        
        Public Property [Error]() As String
            Get
                Return Me.m_Error
            End Get
            Set
                Me.m_Error = value
            End Set
        End Property
        
        Public Overrides Function ToString() As String
            Dim oldValueInfo As String = String.Empty
            Dim v As Object = Value
            If Me.Modified Then
                Dim ov As Object = OldValue
                If (ov Is Nothing) Then
                    ov = "null"
                End If
                oldValueInfo = String.Format(" (old value = {0})", ov)
            End If
            Dim isReadOnly As String = String.Empty
            If [ReadOnly] Then
                isReadOnly = " (read-only)"
            End If
            If (v Is Nothing) Then
                v = "null"
            End If
            Dim err As String = String.Empty
            If Not (String.IsNullOrEmpty([Error])) Then
                err = String.Format("; Input Error: {0}", [Error])
            End If
            Return String.Format(String.Format("{0} = {1}{2}{3}{4}", Name, v, oldValueInfo, isReadOnly, err))
        End Function
        
        Public Sub CheckModified()
            If m_NoCheck Then
                Return
            End If
            If String.Empty.Equals(NewValue) Then
                NewValue = Nothing
            End If
            If (NewValue Is Nothing) Then
                If (Not (OldValue) Is Nothing) Then
                    m_Modified = true
                Else
                    m_Modified = false
                End If
            Else
                If (Not (OldValue) Is Nothing) Then
                    m_Modified = Not (NewValue.Equals(OldValue))
                Else
                    m_Modified = true
                End If
            End If
        End Sub
        
        Public Sub AssignTo(ByVal instance As Object)
            CheckModified()
            Dim t As Type = instance.GetType()
            Dim propInfo As System.Reflection.PropertyInfo = t.GetProperty(Name)
            Dim v As Object = Value
            If (Not (v) Is Nothing) Then
                If propInfo.PropertyType.IsGenericType Then
                    If propInfo.PropertyType.GetProperty("Value").PropertyType.Equals(GetType(Guid)) Then
                        v = New Guid(Convert.ToString(v))
                    Else
                        v = Convert.ChangeType(v, propInfo.PropertyType.GetProperty("Value").PropertyType)
                    End If
                Else
                    v = Convert.ChangeType(v, propInfo.PropertyType)
                End If
            End If
            t.InvokeMember(Name, System.Reflection.BindingFlags.SetProperty, Nothing, instance, New Object() {v})
        End Sub
        
        Public Sub EnableConversion()
            m_EnableConversion = true
        End Sub
        
        Public Sub DisableConversion()
            m_EnableConversion = false
        End Sub
    End Class
End Namespace

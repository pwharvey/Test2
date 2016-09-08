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
    
    Public Class Action
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Id As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_CommandName As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_CommandArgument As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_HeaderText As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Description As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_CssClass As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Confirmation As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Notify As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_WhenLastCommandName As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_WhenLastCommandArgument As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_WhenKeySelected As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_WhenClientScript As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_CausesValidation As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_WhenTag As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_WhenHRef As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_WhenView As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Key As String
        
        Public Sub New()
            MyBase.New
        End Sub
        
        Public Sub New(ByVal action As XPathNavigator, ByVal resolver As IXmlNamespaceResolver)
            MyBase.New
            Me.m_Id = action.GetAttribute("id", String.Empty)
            Me.m_CommandName = action.GetAttribute("commandName", String.Empty)
            Me.m_CommandArgument = action.GetAttribute("commandArgument", String.Empty)
            Me.m_HeaderText = action.GetAttribute("headerText", String.Empty)
            Me.m_Description = action.GetAttribute("description", String.Empty)
            Me.m_CssClass = action.GetAttribute("cssClass", String.Empty)
            Me.m_Confirmation = action.GetAttribute("confirmation", String.Empty)
            Me.m_Notify = action.GetAttribute("notify", String.Empty)
            Me.m_WhenLastCommandName = action.GetAttribute("whenLastCommandName", String.Empty)
            Me.m_WhenLastCommandArgument = action.GetAttribute("whenLastCommandArgument", String.Empty)
            Me.m_CausesValidation = Not ((action.GetAttribute("causesValidation", String.Empty) = "false"))
            Me.m_WhenKeySelected = (action.GetAttribute("whenKeySelected", String.Empty) = "true")
            Me.m_WhenTag = action.GetAttribute("whenTag", String.Empty)
            Me.m_WhenHRef = action.GetAttribute("whenHRef", String.Empty)
            Me.m_WhenView = action.GetAttribute("whenView", String.Empty)
            Me.m_WhenClientScript = action.GetAttribute("whenClientScript", String.Empty)
            Me.m_Key = action.GetAttribute("key", String.Empty)
        End Sub
        
        Public Property Id() As String
            Get
                Return Me.m_Id
            End Get
            Set
                Me.m_Id = value
            End Set
        End Property
        
        Public Property CommandName() As String
            Get
                Return Me.m_CommandName
            End Get
            Set
                Me.m_CommandName = value
            End Set
        End Property
        
        Public Property CommandArgument() As String
            Get
                Return Me.m_CommandArgument
            End Get
            Set
                Me.m_CommandArgument = value
            End Set
        End Property
        
        Public Property HeaderText() As String
            Get
                Return Me.m_HeaderText
            End Get
            Set
                Me.m_HeaderText = value
            End Set
        End Property
        
        Public Property Description() As String
            Get
                Return Me.m_Description
            End Get
            Set
                Me.m_Description = value
            End Set
        End Property
        
        Public Property CssClass() As String
            Get
                Return Me.m_CssClass
            End Get
            Set
                Me.m_CssClass = value
            End Set
        End Property
        
        Public Property Confirmation() As String
            Get
                Return Me.m_Confirmation
            End Get
            Set
                Me.m_Confirmation = value
            End Set
        End Property
        
        Public Property Notify() As String
            Get
                Return Me.m_Notify
            End Get
            Set
                Me.m_Notify = value
            End Set
        End Property
        
        Public Property WhenLastCommandName() As String
            Get
                Return Me.m_WhenLastCommandName
            End Get
            Set
                Me.m_WhenLastCommandName = value
            End Set
        End Property
        
        Public Property WhenLastCommandArgument() As String
            Get
                Return Me.m_WhenLastCommandArgument
            End Get
            Set
                Me.m_WhenLastCommandArgument = value
            End Set
        End Property
        
        Public Property WhenKeySelected() As Boolean
            Get
                Return Me.m_WhenKeySelected
            End Get
            Set
                Me.m_WhenKeySelected = value
            End Set
        End Property
        
        Public Property WhenClientScript() As String
            Get
                Return Me.m_WhenClientScript
            End Get
            Set
                Me.m_WhenClientScript = value
            End Set
        End Property
        
        Public Property CausesValidation() As Boolean
            Get
                Return Me.m_CausesValidation
            End Get
            Set
                Me.m_CausesValidation = value
            End Set
        End Property
        
        Public Property WhenTag() As String
            Get
                Return Me.m_WhenTag
            End Get
            Set
                Me.m_WhenTag = value
            End Set
        End Property
        
        Public Property WhenHRef() As String
            Get
                Return Me.m_WhenHRef
            End Get
            Set
                Me.m_WhenHRef = value
            End Set
        End Property
        
        Public Property WhenView() As String
            Get
                Return Me.m_WhenView
            End Get
            Set
                Me.m_WhenView = value
            End Set
        End Property
        
        Public Property Key() As String
            Get
                Return Me.m_Key
            End Get
            Set
                Me.m_Key = value
            End Set
        End Property
    End Class
End Namespace

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.DataVisualization.Charting
Imports System.Web.UI.HtmlControls
Imports System.Web.UI.WebControls

Namespace MyCompany.Web
    
    Public Class ChartHost
        Inherits System.Web.UI.Page
        
        Protected Overrides Sub OnInit(ByVal e As EventArgs)
            Controls.Add(New LiteralControl(""&Global.Microsoft.VisualBasic.ChrW(10)&"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.or"& _ 
                        "g/TR/xhtml1/DTD/xhtml1-transitional.dtd"">"&Global.Microsoft.VisualBasic.ChrW(10)&"<html xmlns=""http://www.w3.org/1999/xh"& _ 
                        "tml"" style=""overflow: hidden"">"&Global.Microsoft.VisualBasic.ChrW(10)))
            Dim head As HtmlHead = New HtmlHead()
            Controls.Add(head)
            Controls.Add(New LiteralControl(""&Global.Microsoft.VisualBasic.ChrW(10)&"<body>"&Global.Microsoft.VisualBasic.ChrW(10)&"    "))
            Dim form As HtmlForm = New HtmlForm()
            Controls.Add(form)
            Controls.Add(New LiteralControl(""&Global.Microsoft.VisualBasic.ChrW(10)&"</body>"&Global.Microsoft.VisualBasic.ChrW(10)&"</html>"&Global.Microsoft.VisualBasic.ChrW(10)))
            Dim controlName As String = Request.Params("c")
            If Not (String.IsNullOrEmpty(controlName)) Then
                Dim c As Control = LoadControl(String.Format("~/Controls/Chart_{0}.ascx", controlName))
                form.Controls.Add(c)
            End If
            MyBase.OnInit(e)
            EnableViewState = false
        End Sub
        
        Private Function FindChart(ByVal controls As ControlCollection) As Chart
            For Each c As Control in controls
                If TypeOf c Is Chart Then
                    Return CType(c,Chart)
                Else
                    Dim result As Chart = FindChart(c.Controls)
                    If (Not (result) Is Nothing) Then
                        Return result
                    End If
                End If
            Next
            Return Nothing
        End Function
        
        Protected Overrides Sub OnLoad(ByVal e As EventArgs)
            Dim c As Chart = FindChart(Controls)
            If (Not (c) Is Nothing) Then
                Dim aspectRatio As Double = (c.Height.Value / c.Width.Value)
                Dim w As String = Request.Params("w")
                If Not (String.IsNullOrEmpty(w)) Then
                    c.Width = New Unit(w)
                    c.Height = New Unit((Convert.ToDouble(w) * aspectRatio))
                End If
                DataBindChildren()
                Dim image As MemoryStream = New MemoryStream()
                c.SaveImage(image, ChartImageFormat.Png)
                Response.Clear()
                Response.ContentType = "image/png"
                Response.OutputStream.Write(image.ToArray(), 0, CType(image.Length,Integer))
                Response.End()
            End If
        End Sub
    End Class
End Namespace

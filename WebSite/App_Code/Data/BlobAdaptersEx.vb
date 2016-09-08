Imports MyCompany.Data
Imports MyCompany.Handlers
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web

Namespace MyCompany.Data
    
    Partial Public Class S3BlobAdapter
        Inherits S3BlobAdapterBase
        
        Public Sub New(ByVal controller As String, ByVal arguments As BlobAdapterArguments)
            MyBase.New(controller, arguments)
        End Sub
    End Class
    
    Public Class S3BlobAdapterBase
        Inherits BlobAdapter
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_AccessKeyID As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_SecretAccessKey As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Bucket As String
        
        Public Sub New(ByVal controller As String, ByVal arguments As BlobAdapterArguments)
            MyBase.New(controller, arguments)
        End Sub
        
        Public Overridable Property AccessKeyID() As String
            Get
                Return Me.m_AccessKeyID
            End Get
            Set
                Me.m_AccessKeyID = value
            End Set
        End Property
        
        Public Overridable Property SecretAccessKey() As String
            Get
                Return Me.m_SecretAccessKey
            End Get
            Set
                Me.m_SecretAccessKey = value
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
        
        Protected Overrides Sub Initialize()
            MyBase.Initialize()
            If Arguments.ContainsKey("access-key-id") Then
                AccessKeyID = Arguments("access-key-id")
            End If
            If Arguments.ContainsKey("secret-access-key") Then
                SecretAccessKey = Arguments("secret-access-key")
            End If
            If Arguments.ContainsKey("bucket") Then
                Bucket = Arguments("bucket")
            End If
        End Sub
        
        Public Overrides Function ReadBlob(ByVal keyValue As String) As Stream
            Dim extendedPath As String = KeyValueToPath(keyValue)
            Dim httpVerb As String = "GET"
            Dim [date] As Date = DateTime.UtcNow
            Dim canonicalizedAmzHeaders As String = ("x-amz-date:" + [date].ToString("R", CultureInfo.InvariantCulture))
            Dim canonicalizedResource As String = String.Format("/{0}/{1}", Me.Bucket, extendedPath)
            Dim stringToSign As String = String.Format("{0}"&Global.Microsoft.VisualBasic.ChrW(10)&Global.Microsoft.VisualBasic.ChrW(10)&Global.Microsoft.VisualBasic.ChrW(10)&Global.Microsoft.VisualBasic.ChrW(10)&"{1}"&Global.Microsoft.VisualBasic.ChrW(10)&"{2}", httpVerb, canonicalizedAmzHeaders, canonicalizedResource)
            Dim authorization As String = CreateAuthorizationHeaderForS3(stringToSign)
            Dim uri As Uri = New Uri((("http://" + Me.Bucket)  _
                            + (".s3.amazonaws.com/" + extendedPath)))
            Dim request As HttpWebRequest = CType(WebRequest.Create(uri),HttpWebRequest)
            request.Method = httpVerb
            request.Headers.Add("x-amz-date", [date].ToString("R", CultureInfo.InvariantCulture))
            request.Headers.Add("Authorization", authorization)
            Try 
                Dim tempFileName As String = Path.GetTempFileName()
                Dim stream As Stream = File.Create(tempFileName)
                Using response As HttpWebResponse = CType(request.GetResponse(),HttpWebResponse)
                    Using dataStream As Stream = response.GetResponseStream()
                        CopyData(dataStream, stream)
                    End Using
                End Using
                Return stream
            Catch e As Exception
                Dim message As String = e.Message
                Return Nothing
            End Try
        End Function
        
        Public Overrides Function WriteBlob(ByVal file As HttpPostedFile, ByVal keyValue As String) As Boolean
            Dim extendedPath As String = KeyValueToPath(keyValue)
            Dim stream As Stream = file.InputStream
            Dim blobLength As Integer = CType(stream.Length,Integer)
            Dim blobContent((blobLength) - 1) As Byte
            stream.Read(blobContent, 0, blobLength)
            Dim httpVerb As String = "PUT"
            Dim [date] As Date = DateTime.UtcNow
            Dim canonicalizedAmzHeaders As String = ("x-amz-date:" + [date].ToString("R", CultureInfo.InvariantCulture))
            Dim canonicalizedResource As String = String.Format("/{0}/{1}", Me.Bucket, extendedPath)
            Dim stringToSign As String = String.Format("{0}"&Global.Microsoft.VisualBasic.ChrW(10)&Global.Microsoft.VisualBasic.ChrW(10)&Global.Microsoft.VisualBasic.ChrW(10)&Global.Microsoft.VisualBasic.ChrW(10)&"{1}"&Global.Microsoft.VisualBasic.ChrW(10)&"{2}", httpVerb, canonicalizedAmzHeaders, canonicalizedResource)
            Dim authorization As String = CreateAuthorizationHeaderForS3(stringToSign)
            Dim uri As Uri = New Uri((("http://" + Me.Bucket)  _
                            + (".s3.amazonaws.com/" + extendedPath)))
            Dim request As HttpWebRequest = CType(WebRequest.Create(uri),HttpWebRequest)
            request.Method = httpVerb
            request.ContentLength = blobLength
            request.Headers.Add("x-amz-date", [date].ToString("R", CultureInfo.InvariantCulture))
            request.Headers.Add("Authorization", authorization)
            Try 
                Using requestStream As Stream = request.GetRequestStream()
                    Dim bufferSize As Integer = (1024 * 64)
                    Dim offset As Integer = 0
                    Do While (offset < blobLength)
                        Dim bytesToWrite As Integer = (blobLength - offset)
                        If ((offset + bufferSize) < blobLength) Then
                            bytesToWrite = bufferSize
                        End If
                        requestStream.Write(blobContent, offset, bytesToWrite)
                        offset = (offset + bytesToWrite)
                    Loop
                End Using
                Using response As HttpWebResponse = CType(request.GetResponse(),HttpWebResponse)
                    Dim ETag As String = response.Headers("ETag")
                    If (((response.StatusCode = HttpStatusCode.OK) OrElse (response.StatusCode = HttpStatusCode.Accepted)) OrElse (response.StatusCode = HttpStatusCode.Created)) Then
                        Return true
                    End If
                End Using
            Catch webEx As WebException
                If (Not (webEx) Is Nothing) Then
                    Dim resp As WebResponse = webEx.Response
                    If (Not (resp) Is Nothing) Then
                        Using sr As StreamReader = New StreamReader(resp.GetResponseStream(), true)
                            Throw New Exception(sr.ReadToEnd())
                        End Using
                    End If
                End If
            End Try
            Return false
        End Function
        
        Protected Overridable Function CreateAuthorizationHeaderForS3(ByVal canonicalizedString As String) As String
            Dim ae As Encoding = New UTF8Encoding()
            Dim signature As HMACSHA1 = New HMACSHA1()
            signature.Key = ae.GetBytes(Me.SecretAccessKey)
            Dim bytes() As Byte = ae.GetBytes(canonicalizedString)
            Dim moreBytes() As Byte = signature.ComputeHash(bytes)
            Dim encodedCanonical As String = Convert.ToBase64String(moreBytes)
            Return String.Format(CultureInfo.InvariantCulture, "{0} {1}:{2}", "AWS", Me.AccessKeyID, encodedCanonical)
        End Function
    End Class
End Namespace

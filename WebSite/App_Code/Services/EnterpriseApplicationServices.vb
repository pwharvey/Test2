Imports MyCompany.Data
Imports MyCompany.Security
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Web.Routing
Imports System.Xml
Imports System.Xml.XPath

Namespace MyCompany.Services
    
    Partial Public Class EnterpriseApplicationServices
        Inherits EnterpriseApplicationServicesBase
    End Class
    
    Public Class EnterpriseApplicationServicesBase
        Inherits ApplicationServicesBase
        
        Public Shared AppServicesRegex As Regex = New Regex("/appservices/(?'Controller'\w+?)(/|$)", RegexOptions.IgnoreCase)
        
        Public Shared DynamicResourceRegex As Regex = New Regex("(\.js$|^_(invoke|authenticate)$)", RegexOptions.IgnoreCase)
        
        Public Shared DynamicWebResourceRegex As Regex = New Regex("\.(js|css)$", RegexOptions.IgnoreCase)
        
        Public Overrides Sub RegisterServices()
            RegisterREST()
            MyBase.RegisterServices()
        End Sub
        
        Public Overridable Sub RegisterREST()
            Dim routes As RouteCollection = RouteTable.Routes
            routes.RouteExistingFiles = true
            GenericRoute.Map(routes, New RepresentationalStateTransfer(), "appservices/{Controller}/{Segment1}/{Segment2}/{Segment3}/{Segment4}")
            GenericRoute.Map(routes, New RepresentationalStateTransfer(), "appservices/{Controller}/{Segment1}/{Segment2}/{Segment3}")
            GenericRoute.Map(routes, New RepresentationalStateTransfer(), "appservices/{Controller}/{Segment1}/{Segment2}")
            GenericRoute.Map(routes, New RepresentationalStateTransfer(), "appservices/{Controller}/{Segment1}")
            GenericRoute.Map(routes, New RepresentationalStateTransfer(), "appservices/{Controller}")
        End Sub
        
        Public Overrides Function RequiresAuthentication(ByVal request As HttpRequest) As Boolean
            Dim result As Boolean = MyBase.RequiresAuthentication(request)
            If result Then
                Return true
            End If
            Dim m As Match = AppServicesRegex.Match(request.Path)
            If m.Success Then
                Dim config As ControllerConfiguration = Nothing
                Try 
                    Dim controllerName As String = m.Groups("Controller").Value
                    If (controllerName = "_authenticate") Then
                        Return false
                    End If
                    If Not (DynamicResourceRegex.IsMatch(controllerName)) Then
                        config = DataControllerBase.CreateConfigurationInstance([GetType](), controllerName)
                    End If
                Catch __exception As Exception
                End Try
                If (config Is Nothing) Then
                    Return Not (DynamicWebResourceRegex.IsMatch(request.Path))
                End If
                Return RequiresRESTAuthentication(request, config)
            End If
            Return false
        End Function
        
        Public Overridable Function RequiresRESTAuthentication(ByVal request As HttpRequest, ByVal config As ControllerConfiguration) As Boolean
            Return UriRestConfig.RequiresAuthentication(request, config)
        End Function
    End Class
    
    Public Class ScheduleStatus
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Schedule As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Exceptions As String
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Success As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_NextTestDate As DateTime
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Expired As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_Precision As String
        
        ''' The definition of the schedule.
        Public Overridable Property Schedule() As String
            Get
                Return Me.m_Schedule
            End Get
            Set
                Me.m_Schedule = value
            End Set
        End Property
        
        ''' The defintion of excepetions to the schedule. Exceptions are expressed as another schedule.
        Public Overridable Property Exceptions() As String
            Get
                Return Me.m_Exceptions
            End Get
            Set
                Me.m_Exceptions = value
            End Set
        End Property
        
        ''' True if the schedule is valid at this time.
        Public Overridable Property Success() As Boolean
            Get
                Return Me.m_Success
            End Get
            Set
                Me.m_Success = value
            End Set
        End Property
        
        ''' The next date and time when the schedule is invalid.
        Public Overridable Property NextTestDate() As DateTime
            Get
                Return Me.m_NextTestDate
            End Get
            Set
                Me.m_NextTestDate = value
            End Set
        End Property
        
        ''' True if the schedule has expired. For internal use only.
        Public Overridable Property Expired() As Boolean
            Get
                Return Me.m_Expired
            End Get
            Set
                Me.m_Expired = value
            End Set
        End Property
        
        ''' The precision of the schedule. For internal use only.
        Public Overridable Property Precision() As String
            Get
                Return Me.m_Precision
            End Get
            Set
                Me.m_Precision = value
            End Set
        End Property
    End Class
    
    Partial Public Class Scheduler
        Inherits SchedulerBase
    End Class
    
    Public Class SchedulerBase
        
        Public Shared NodeMatchRegex As Regex = New Regex("(?'Depth'\++)\s*(?'NodeType'\S+)\s*(?'Properties'[^\+]*)")
        
        Public Shared PropertyMatchRegex As Regex = New Regex("\s*(?'Name'[a-zA-Z]*)\s*[:=]?\s*(?'Value'.+?)(\n|;|$)")
        
        Private Shared m_NodeTypes() As String = New String() {"yearly", "monthly", "weekly", "daily", "once"}
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_TestDate As DateTime
        
        Public Overridable Property TestDate() As DateTime
            Get
                Return Me.m_TestDate
            End Get
            Set
                Me.m_TestDate = value
            End Set
        End Property
        
        Public Overridable ReadOnly Property UsePreciseProbe() As Boolean
            Get
                Return false
            End Get
        End Property
        
        ''' Check if a free form text schedule is valid now.
        Public Overloads Shared Function Test(ByVal schedule As String) As ScheduleStatus
            Return Test(schedule, Nothing, DateTime.Now)
        End Function
        
        ''' Check if a free form text schedule is valid on the testDate.
        Public Overloads Shared Function Test(ByVal schedule As String, ByVal testDate As DateTime) As ScheduleStatus
            Return Test(schedule, Nothing, testDate)
        End Function
        
        ''' Check if a free form text schedule with exceptions is valid now.
        Public Overloads Shared Function Test(ByVal schedule As String, ByVal exceptions As String) As ScheduleStatus
            Return Test(schedule, exceptions, DateTime.Now)
        End Function
        
        ''' Check if a free form text schedule with exceptions is valid on the testDate.
        Public Overloads Shared Function Test(ByVal schedule As String, ByVal exceptions As String, ByVal testDate As DateTime) As ScheduleStatus
            Dim s As Scheduler = New Scheduler()
            s.TestDate = testDate
            Dim status As ScheduleStatus = s.CheckSchedule(schedule, exceptions)
            status.Schedule = schedule
            status.Exceptions = exceptions
            Return status
        End Function
        
        Public Overloads Overridable Function CheckSchedule(ByVal schedule As String) As ScheduleStatus
            Return CheckSchedule(StringToXml(schedule), Nothing)
        End Function
        
        Public Overloads Overridable Function CheckSchedule(ByVal schedule As String, ByVal exceptions As String) As ScheduleStatus
            Return CheckSchedule(StringToXml(schedule), StringToXml(exceptions))
        End Function
        
        ''' Check an XML schedule.
        Public Overloads Overridable Function CheckSchedule(ByVal schedule As Stream) As ScheduleStatus
            Return CheckSchedule(schedule, Nothing)
        End Function
        
        ''' Check an XML schedule with exceptions.
        Public Overloads Overridable Function CheckSchedule(ByVal schedule As Stream, ByVal exceptions As Stream) As ScheduleStatus
            Dim sched As ScheduleStatus = New ScheduleStatus()
            sched.Precision = String.Empty
            Dim xSched As ScheduleStatus = New ScheduleStatus()
            xSched.Precision = String.Empty
            Dim nav As XPathNavigator = Nothing
            Dim xNav As XPathNavigator = Nothing
            If ((schedule Is Nothing) OrElse schedule.Equals(Stream.Null)) Then
                sched.Success = true
            Else
                Dim doc As XPathDocument = New XPathDocument(schedule)
                nav = doc.CreateNavigator()
                If (Not (nav.MoveToChild(XPathNodeType.Element)) OrElse Not ((nav.Name = "schedule"))) Then
                    sched.Success = true
                Else
                    CheckNode(nav, DateTime.Now, sched)
                End If
            End If
            If ((Not (exceptions) Is Nothing) AndAlso Not (exceptions.Equals(Stream.Null))) Then
                Dim xDoc As XPathDocument = New XPathDocument(exceptions)
                xNav = xDoc.CreateNavigator()
                If (Not (xNav.MoveToChild(XPathNodeType.Element)) OrElse Not ((xNav.Name = "schedule"))) Then
                    xSched.Success = true
                Else
                    CheckNode(xNav, DateTime.Now, xSched)
                End If
            End If
            If xSched.Success Then
                sched.Success = false
            End If
            If UsePreciseProbe Then
                sched = ProbeScheduleExact(nav, xNav, sched, xSched)
            Else
                sched = ProbeSchedule(nav, xNav, sched, xSched)
            End If
            Return sched
        End Function
        
        ''' Converts plain text schedule format into XML stream.
        Private Function StringToXml(ByVal text As String) As Stream
            If String.IsNullOrEmpty(text) Then
                Return Nothing
            End If
            'check for shorthand "start"
            Dim testDate As DateTime = DateTime.Now
            If DateTime.TryParse(text, testDate) Then
                String.Format("+once start: {0}", text)
            End If
            'compose XML document
            Dim doc As XmlDocument = New XmlDocument()
            Dim dec As XmlDeclaration = doc.CreateXmlDeclaration("1.0", Nothing, Nothing)
            doc.AppendChild(dec)
            Dim schedule As XmlNode = doc.CreateNode(XmlNodeType.Element, "schedule", Nothing)
            doc.AppendChild(schedule)
            'configure nodes
            Dim nodes As MatchCollection = NodeMatchRegex.Matches(text)
            Dim lastNode As XmlNode = schedule
            Dim lastDepth As Integer = 0
            For Each node As Match in nodes
                Dim nodeType As String = node.Groups("NodeType").Value
                Dim depth As Integer = node.Groups("Depth").Value.Length
                Dim properties As String = node.Groups("Properties").Value
                If m_NodeTypes.Contains(nodeType) Then
                    Dim newNode As XmlNode = doc.CreateNode(XmlNodeType.Element, nodeType, Nothing)
                    Dim propertyMatches As MatchCollection = PropertyMatchRegex.Matches(node.Groups("Properties").Value)
                    'populate attributes
                    For Each [property] As Match in propertyMatches
                        Dim name As String = [property].Groups("Name").Value.Trim()
                        Dim val As String = [property].Groups("Value").Value.Trim()
                        'group value
                        If String.IsNullOrEmpty(name) Then
                            name = "value"
                        End If
                        Dim attr As XmlAttribute = doc.CreateAttribute(name)
                        attr.Value = val
                        newNode.Attributes.Append(attr)
                    Next
                    'insert node
                    If (depth > lastDepth) Then
                        lastNode.AppendChild(newNode)
                    Else
                        If (depth < lastDepth) Then
                            Do While (Not ((lastNode.Name = "schedule")) AndAlso Not ((lastNode.Name = nodeType)))
                                lastNode = lastNode.ParentNode
                            Loop
                            If (lastNode.Name = nodeType) Then
                                lastNode = lastNode.ParentNode
                            End If
                            lastNode.AppendChild(newNode)
                        Else
                            lastNode.ParentNode.AppendChild(newNode)
                        End If
                    End If
                    lastNode = newNode
                    lastDepth = depth
                End If
            Next
            'save and return
            Dim stream As MemoryStream = New MemoryStream()
            doc.Save(stream)
            stream.Position = 0
            Return stream
        End Function
        
        ''' Checks the current navigator if the current nodes define an active schedule. An empty schedule will set Match to true.
        Private Function CheckNode(ByVal nav As XPathNavigator, ByVal checkDate As DateTime, ByRef sched As ScheduleStatus) As Boolean
            If (nav Is Nothing) Then
                Return false
            End If
            sched.Precision = nav.Name
            If Not (nav.MoveToFirstChild()) Then
                'no schedule limitation
                sched.Success = true
                Return true
            End If
            Do While true
                'ignore comments
                If Not (nav.NodeType.Equals(XPathNodeType.Comment)) Then
                    Dim name As String = nav.Name
                    If (name = "once") Then
                        If CheckInterval(nav, checkDate) Then
                            sched.Success = true
                        End If
                    Else
                        If CheckInterval(nav, checkDate) Then
                            Dim value As String = nav.GetAttribute("value", String.Empty)
                            Dim every As String = nav.GetAttribute("every", String.Empty)
                            Dim check As Integer = 0
                            If (name = "yearly") Then
                                check = checkDate.Year
                            Else
                                If (name = "monthly") Then
                                    check = checkDate.Month
                                Else
                                    If (name = "weekly") Then
                                        check = GetWeekOfMonth(checkDate)
                                    Else
                                        If (name = "daily") Then
                                            check = CType(checkDate.DayOfWeek,Integer)
                                        End If
                                    End If
                                End If
                            End If
                            If CheckNumberInterval(value, check, every) Then
                                CheckNode(nav, checkDate, sched)
                            End If
                        End If
                    End If
                    'found a match
                    If (sched.Expired OrElse sched.Success) Then
                        Exit Do
                    End If
                End If
                'no more nodes
                If Not (nav.MoveToNext()) Then
                    Exit Do
                End If
            Loop
            Return sched.Success
        End Function
        
        ''' Checks to see if a series of comma-separated numbers and/or dash-separated intervals contain a specific number
        Private Function CheckNumberInterval(ByVal interval As String, ByVal number As Integer, ByVal every As String) As Boolean
            If String.IsNullOrEmpty(interval) Then
                Return true
            End If
            'process numbers and number ranges
            Dim strings() As String = interval.Split(Global.Microsoft.VisualBasic.ChrW(44))
            Dim numbers As List(Of Integer) = New List(Of Integer)()
            For Each str As String in strings
                If str.Contains(Global.Microsoft.VisualBasic.ChrW(45)) Then
                    Dim intervalString() As String = str.Split(Global.Microsoft.VisualBasic.ChrW(45))
                    Dim interval1 As Integer = Convert.ToInt32(intervalString(0))
                    Dim interval2 As Integer = Convert.ToInt32(intervalString(1))
                    Dim i As Integer = interval1
                    Do While (i <= interval2)
                        numbers.Add(i)
                        i = (i + 1)
                    Loop
                Else
                    If Not (String.IsNullOrEmpty(str)) Then
                        numbers.Add(Convert.ToInt32(str))
                    End If
                End If
            Next
            numbers.Sort()
            'check if "every" used
            Dim everyNum As Integer = 1
            If Not (String.IsNullOrEmpty(every)) Then
                everyNum = Convert.ToInt32(every)
            End If
            If (everyNum > 1) Then
                'if "every" is greater than available numbers
                If (everyNum >= numbers.Count) Then
                    Return numbers.First().Equals(number)
                End If
                Dim allNumbers As List(Of Integer) = New List(Of Integer)(numbers)
                numbers.Clear()
                Dim i As Integer = 0
                Do While (i <= (allNumbers.Count / everyNum))
                    numbers.Add(allNumbers.ElementAt((i * everyNum)))
                    i = (i + 1)
                Loop
            End If
            Return numbers.Contains(number)
        End Function
        
        ''' Checks to see if the current node's start and end attributes are valid.
        Private Function CheckInterval(ByVal nav As XPathNavigator, ByVal checkDate As DateTime) As Boolean
            Dim start As DateTime = checkDate
            Dim [end] As DateTime = checkDate
            If Not (DateTime.TryParse(nav.GetAttribute("start", String.Empty), start)) Then
                start = StartOfDay(TestDate)
            End If
            If Not (DateTime.TryParse(nav.GetAttribute("end", String.Empty), [end])) Then
                [end] = DateTime.MaxValue
            End If
            If Not (((start <= checkDate) AndAlso (checkDate <= [end]))) Then
                Return false
            End If
            Return true
        End Function
        
        Private Function ProbeSchedule(ByVal document As XPathNavigator, ByVal exceptionsDocument As XPathNavigator, ByVal schedule As ScheduleStatus, ByVal exceptionsSchedule As ScheduleStatus) As ScheduleStatus
            Dim testSched As ScheduleStatus = New ScheduleStatus()
            Dim testExceptionSched As ScheduleStatus = New ScheduleStatus()
            Dim nextDate As DateTime = DateTime.Now
            Dim initialState As Boolean = schedule.Success
            Dim probeCount As Integer = 0
            Do While (probeCount <= 30)
                nextDate = nextDate.AddSeconds(1)
                'reset variables
                testSched.Success = false
                testSched.Expired = false
                document.MoveToRoot()
                document.MoveToFirstChild()
                If (Not (exceptionsDocument) Is Nothing) Then
                    exceptionsDocument.MoveToRoot()
                    exceptionsDocument.MoveToFirstChild()
                    testExceptionSched.Success = false
                    testExceptionSched.Expired = false
                End If
                Dim valid As Boolean = (CheckNode(document, nextDate, testSched) AndAlso ((exceptionsDocument Is Nothing) OrElse Not (CheckNode(exceptionsDocument, nextDate, testExceptionSched))))
                If Not ((valid = initialState)) Then
                    Return schedule
                End If
                schedule.NextTestDate = nextDate
                probeCount = (probeCount + 1)
            Loop
            Return schedule
        End Function
        
        Private Function ProbeScheduleExact(ByVal document As XPathNavigator, ByVal exceptionsDocument As XPathNavigator, ByVal schedule As ScheduleStatus, ByVal exceptionsSchedule As ScheduleStatus) As ScheduleStatus
            Dim testSched As ScheduleStatus = New ScheduleStatus()
            Dim testExceptionSched As ScheduleStatus = New ScheduleStatus()
            Dim sign As Integer = 1
            Dim nextDate As DateTime = DateTime.Now
            Dim initialState As Boolean = schedule.Success
            Dim jump As Integer = 0
            If (schedule.Precision.Equals("daily") OrElse exceptionsSchedule.Precision.Equals("daily")) Then
                jump = (6 * 60)
            Else
                If (schedule.Precision.Equals("weekly") OrElse exceptionsSchedule.Precision.Equals("weekly")) Then
                    jump = (72 * 60)
                Else
                    If (schedule.Precision.Equals("monthly") OrElse exceptionsSchedule.Precision.Equals("monthly")) Then
                        jump = (360 * 60)
                    Else
                        If (schedule.Precision.Equals("yearly") OrElse exceptionsSchedule.Precision.Equals("yearly")) Then
                            jump = ((720 * 6)  _
                                        * 60)
                        Else
                            jump = (6 * 60)
                        End If
                    End If
                End If
            End If
            Dim probeCount As Integer = 1
            Do While (probeCount <= 20)
                'reset variables
                testSched.Success = false
                testSched.Expired = false
                document.MoveToRoot()
                document.MoveToFirstChild()
                If (Not (exceptionsDocument) Is Nothing) Then
                    exceptionsDocument.MoveToRoot()
                    exceptionsDocument.MoveToFirstChild()
                    testExceptionSched.Success = false
                    testExceptionSched.Expired = false
                End If
                'set next date to check
                nextDate = nextDate.AddMinutes((jump * sign))
                Dim valid As Boolean = (CheckNode(document, nextDate, testSched) AndAlso ((exceptionsDocument Is Nothing) OrElse Not (CheckNode(exceptionsDocument, nextDate, testExceptionSched))))
                If (valid = initialState) Then
                    sign = 1
                Else
                    sign = -1
                End If
                'keep moving forward and expand jump if no border found, otherwise narrow jump
                If (sign = -1) Then
                    jump = (jump / 2)
                Else
                    jump = (jump * 2)
                    probeCount = (probeCount - 1)
                End If
                If (jump < 5) Then
                    jump = (jump + 1)
                End If
                'no border found
                If (nextDate > DateTime.Now.AddYears(5)) Then
                    Exit Do
                End If
                probeCount = (probeCount + 1)
            Loop
            schedule.NextTestDate = nextDate.AddMinutes((jump * -1))
            Return schedule
        End Function
        
        Private Function GetWeekOfMonth(ByVal [date] As DateTime) As Integer
            Dim beginningOfMonth As DateTime = New DateTime([date].Year, [date].Month, 1)
            Do While Not (([date].Date.AddDays(1).DayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek))
                [date] = [date].AddDays(1)
            Loop
            Return (CType((CType([date].Subtract(beginningOfMonth).TotalDays,Double) / 7),Integer) + 1)
        End Function
        
        Private Function StartOfDay(ByVal [date] As DateTime) As DateTime
            Return New DateTime([date].Year, [date].Month, [date].Day, 0, 0, 0, 0)
        End Function
        
        Private Function EndOfDay(ByVal [date] As DateTime) As DateTime
            Return New DateTime([date].Year, [date].Month, [date].Day, 23, 59, 59, 999)
        End Function
    End Class
End Namespace

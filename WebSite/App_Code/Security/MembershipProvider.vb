Imports MyCompany.Data
Imports System
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.Configuration
Imports System.Configuration.Provider
Imports System.Data
Imports System.Data.Common
Imports System.Diagnostics
Imports System.Globalization
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Web.Configuration
Imports System.Web.Security
Imports System.Xml.XPath

Namespace MyCompany.Security
    
    Partial Public Class ApplicationMembershipProvider
        Inherits ApplicationMembershipProviderBase
    End Class
    
    Public Enum MembershipProviderSqlStatement
        
        ChangePassword
        
        ChangePasswordQuestionAndAnswer
        
        CreateUser
        
        DeleteUser
        
        CountAllUsers
        
        GetAllUsers
        
        GetNumberOfUsersOnline
        
        GetPassword
        
        GetUser
        
        UpdateLastUserActivity
        
        GetUserByProviderKey
        
        UpdateUserLockStatus
        
        GetUserNameByEmail
        
        ResetPassword
        
        UpdateUser
        
        UpdateLastLoginDate
        
        UpdateFailedPasswordAttempt
        
        UpdateFailedPasswordAnswerAttempt
        
        LockUser
        
        CountUsersByName
        
        FindUsersByName
        
        CountUsersByEmail
        
        FindUsersByEmail
    End Enum
    
    Public Class ApplicationMembershipProviderBase
        Inherits MembershipProvider
        
        Protected Shared Statements As SortedDictionary(Of MembershipProviderSqlStatement, String) = New SortedDictionary(Of MembershipProviderSqlStatement, String)()
        
        Private m_NewPasswordLength As Integer = 8
        
        Private m_ValidationKey As String
        
        Private m_ConnectionStringSettings As ConnectionStringSettings
        
        Private m_WriteExceptionsToEventLog As Boolean
        
        <System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)>  _
        Private m_ApplicationName As String
        
        Private m_EnablePasswordReset As Boolean
        
        Private m_EnablePasswordRetrieval As Boolean
        
        Private m_RequiresQuestionAndAnswer As Boolean
        
        Private m_RequiresUniqueEmail As Boolean
        
        Private m_MaxInvalidPasswordAttempts As Integer
        
        Private m_PasswordAttemptWindow As Integer
        
        Private m_PasswordFormat As MembershipPasswordFormat
        
        Private m_MinRequiredNonAlphanumericCharacters As Integer
        
        Private m_MinRequiredPasswordLength As Integer
        
        Private m_PasswordStrengthRegularExpression As String
        
        Shared Sub New()
            Statements(MembershipProviderSqlStatement.ChangePassword) = "update Users set Password = @Password, LastPasswordChangedDate = @LastPasswordCha"& _ 
                "ngedDate where UserName = @UserName"
            Statements(MembershipProviderSqlStatement.ChangePasswordQuestionAndAnswer) = "update Users set PasswordQuestion = @PasswordQuestion, PasswordAnswer = @Password"& _ 
                "Answer where UserName = @UserName"
            Statements(MembershipProviderSqlStatement.CreateUser) = "" & ControlChars.CrLf &"insert into Users" & ControlChars.CrLf &"(" & ControlChars.CrLf &"   UserName" & ControlChars.CrLf &"  ,Password" & ControlChars.CrLf &"  ,Email" & ControlChars.CrLf &"  ,PasswordQuestion" & ControlChars.CrLf & _ 
                "  ,PasswordAnswer" & ControlChars.CrLf &"  ,IsApproved" & ControlChars.CrLf &"  ,Comment" & ControlChars.CrLf &"  ,CreationDate" & ControlChars.CrLf &"  ,LastPasswordCh"& _ 
                "angedDate" & ControlChars.CrLf &"  ,LastActivityDate" & ControlChars.CrLf &"  ,LastLoginDate" & ControlChars.CrLf &"  ,IsLockedOut" & ControlChars.CrLf &"  ,LastLockedO"& _ 
                "utDate" & ControlChars.CrLf &"  ,FailedPasswordAttemptCount" & ControlChars.CrLf &"  ,FailedPasswordAttemptWindowStart" & ControlChars.CrLf &"  ,F"& _ 
                "ailedPasswordAnswerAttemptCount" & ControlChars.CrLf &"  ,FailedPasswordAnswerAttemptWindowStart" & ControlChars.CrLf &")" & ControlChars.CrLf &"v"& _ 
                "alues(" & ControlChars.CrLf &"   @UserName" & ControlChars.CrLf &"  ,@Password" & ControlChars.CrLf &"  ,@Email" & ControlChars.CrLf &"  ,@PasswordQuestion" & ControlChars.CrLf &"  ,@Passwor"& _ 
                "dAnswer" & ControlChars.CrLf &"  ,@IsApproved" & ControlChars.CrLf &"  ,@Comments" & ControlChars.CrLf &"  ,@CreationDate" & ControlChars.CrLf &"  ,@LastPasswordChanged"& _ 
                "Date" & ControlChars.CrLf &"  ,@LastActivityDate" & ControlChars.CrLf &"  ,@LastLoginDate" & ControlChars.CrLf &"  ,@IsLockedOut" & ControlChars.CrLf &"  ,@LastLockedOu"& _ 
                "tDate" & ControlChars.CrLf &"  ,@FailedPwdAttemptCount" & ControlChars.CrLf &"  ,@FailedPwdAttemptWindowStart" & ControlChars.CrLf &"  ,@FailedPwd"& _ 
                "AnsAttemptCount" & ControlChars.CrLf &"  ,@FailedPwdAnsAttemptWindowStart" & ControlChars.CrLf &")"
            Statements(MembershipProviderSqlStatement.DeleteUser) = "delete from Users where UserName = @UserName"
            Statements(MembershipProviderSqlStatement.CountAllUsers) = "select count(*) from Users"
            Statements(MembershipProviderSqlStatement.GetAllUsers) = "" & ControlChars.CrLf &"select " & ControlChars.CrLf &"   UserID UserID" & ControlChars.CrLf &"  ,UserName UserName" & ControlChars.CrLf &"  ,Email Email" & ControlChars.CrLf &"  ,PasswordQue"& _ 
                "stion PasswordQuestion" & ControlChars.CrLf &"  ,Comment Comments" & ControlChars.CrLf &"  ,IsApproved IsApproved" & ControlChars.CrLf &"  ,IsLock"& _ 
                "edOut IsLockedOut" & ControlChars.CrLf &"  ,CreationDate CreationDate" & ControlChars.CrLf &"  ,LastLoginDate LastLoginDate" & ControlChars.CrLf &"  ,LastActivityDate LastActivityDate" & ControlChars.CrLf &"  ,LastPasswordChangedDate LastPasswordCh"& _ 
                "angedDate" & ControlChars.CrLf &"  ,LastLockedOutDate LastLockedOutDate" & ControlChars.CrLf &"from Users " & ControlChars.CrLf &"order by UserNam"& _ 
                "e asc"
            Statements(MembershipProviderSqlStatement.GetNumberOfUsersOnline) = "select count(*) from Users where LastActivityDate >= @CompareDate"
            Statements(MembershipProviderSqlStatement.GetPassword) = "select Password Password, PasswordAnswer PasswordAnswer, IsLockedOut IsLockedOut,"& _ 
                " IsApproved IsApproved, IsLockedOut IsLockedOut, FailedPasswordAttemptWindowStar"& _ 
                "t FailedPwdAttemptWindowStart, FailedPasswordAnswerAttemptWindowStart FailedPwdA"& _ 
                "nsAttemptWindowStart from Users where UserName = @UserName"
            Statements(MembershipProviderSqlStatement.GetUser) = "" & ControlChars.CrLf &"select " & ControlChars.CrLf &"   UserID UserID" & ControlChars.CrLf &"  ,UserName UserName" & ControlChars.CrLf &"  ,Email Email" & ControlChars.CrLf &"  ,PasswordQue"& _ 
                "stion PasswordQuestion" & ControlChars.CrLf &"  ,Comment Comments" & ControlChars.CrLf &"  ,IsApproved IsApproved" & ControlChars.CrLf &"  ,IsLock"& _ 
                "edOut IsLockedOut" & ControlChars.CrLf &"  ,CreationDate CreationDate" & ControlChars.CrLf &"  ,LastLoginDate LastLoginDate" & ControlChars.CrLf &"  ,LastActivityDate LastActivityDate" & ControlChars.CrLf &"  ,LastPasswordChangedDate LastPasswordCh"& _ 
                "angedDate" & ControlChars.CrLf &"  ,LastLockedOutDate LastLockedOutDate" & ControlChars.CrLf &"  ,FailedPasswordAttemptCount"& _ 
                " FailedPwdAttemptCount" & ControlChars.CrLf &"  ,FailedPasswordAttemptWindowStart FailedPwdAttemptWind"& _ 
                "owStart" & ControlChars.CrLf &"  ,FailedPasswordAnswerAttemptCount FailedPwdAnsAttemptCount" & ControlChars.CrLf &"  ,Failed"& _ 
                "PasswordAnswerAttemptWindowStart FailedPwdAnsAttemptWindowStart" & ControlChars.CrLf &"from Users " & ControlChars.CrLf &"wh"& _ 
                "ere UserName = @UserName"
            Statements(MembershipProviderSqlStatement.UpdateLastUserActivity) = "update Users set LastActivityDate = @LastActivityDate where UserName = @UserName"
            Statements(MembershipProviderSqlStatement.GetUserByProviderKey) = "" & ControlChars.CrLf &"select " & ControlChars.CrLf &"   UserID UserID" & ControlChars.CrLf &"  ,UserName Username" & ControlChars.CrLf &"  ,Email Email" & ControlChars.CrLf &"  ,PasswordQue"& _ 
                "stion PasswordQuestion" & ControlChars.CrLf &"  ,Comment Comments" & ControlChars.CrLf &"  ,IsApproved IsApproved" & ControlChars.CrLf &"  ,IsLock"& _ 
                "edOut IsLockedOut" & ControlChars.CrLf &"  ,CreationDate CreationDate" & ControlChars.CrLf &"  ,LastLoginDate LastLoginDate" & ControlChars.CrLf &"  ,LastActivityDate LastActivityDate" & ControlChars.CrLf &"  ,LastPasswordChangedDate LastPasswordCh"& _ 
                "angedDate" & ControlChars.CrLf &"  ,LastLockedOutDate LastLockedOutDate" & ControlChars.CrLf &"from Users " & ControlChars.CrLf &"where UserID = @"& _ 
                "UserID"
            Statements(MembershipProviderSqlStatement.UpdateUserLockStatus) = "update Users set IsLockedOut = @IsLockedOut, LastLockedOutDate = @LastLockedOutDa"& _ 
                "te where UserName = @UserName"
            Statements(MembershipProviderSqlStatement.GetUserNameByEmail) = "select UserName Username from Users where Email = @Email"
            Statements(MembershipProviderSqlStatement.ResetPassword) = "update Users set Password = @Password, LastPasswordChangedDate = @LastPasswordCha"& _ 
                "ngedDate where UserName = @UserName and IsLockedOut = @IsLockedOut"
            Statements(MembershipProviderSqlStatement.UpdateUser) = "update Users set Email = @Email, Comment = @Comments, IsApproved = @IsApproved wh"& _ 
                "ere UserName = @UserName"
            Statements(MembershipProviderSqlStatement.UpdateLastLoginDate) = "update Users set LastLoginDate = @LastLoginDate, IsLockedOut = @IsLockedOut where"& _ 
                " UserName = @UserName"
            Statements(MembershipProviderSqlStatement.UpdateFailedPasswordAttempt) = "update Users set FailedPasswordAttemptCount = @FailedPwdAttemptCount, FailedPassw"& _ 
                "ordAttemptWindowStart = @FailedPwdAttemptWindowStart where UserName = @UserName"
            Statements(MembershipProviderSqlStatement.UpdateFailedPasswordAnswerAttempt) = "update Users set FailedPasswordAnswerAttemptCount = @FailedPwdAnsAttemptCount whe"& _ 
                "re UserName = @UserName"
            Statements(MembershipProviderSqlStatement.CountUsersByName) = "select count(*) from Users where UserName like @UserName"
            Statements(MembershipProviderSqlStatement.FindUsersByName) = "" & ControlChars.CrLf &"select " & ControlChars.CrLf &"   UserID UserID" & ControlChars.CrLf &"  ,UserName Username" & ControlChars.CrLf &"  ,Email Email" & ControlChars.CrLf &"  ,PasswordQue"& _ 
                "stion PasswordQuestion" & ControlChars.CrLf &"  ,Comment Comments" & ControlChars.CrLf &"  ,IsApproved IsApproved" & ControlChars.CrLf &"  ,IsLock"& _ 
                "edOut IsLockedOut" & ControlChars.CrLf &"  ,CreationDate CreationDate" & ControlChars.CrLf &"  ,LastLoginDate LastLoginDate" & ControlChars.CrLf &"  ,LastActivityDate LastActivityDate" & ControlChars.CrLf &"  ,LastPasswordChangedDate LastPasswordCh"& _ 
                "angedDate" & ControlChars.CrLf &"  ,LastLockedOutDate LastLockedOutDate" & ControlChars.CrLf &"from Users " & ControlChars.CrLf &"where UserName l"& _ 
                "ike @UserName" & ControlChars.CrLf &"order by UserName asc"
            Statements(MembershipProviderSqlStatement.CountUsersByEmail) = "select count(*) from Users where Email like @Email"
            Statements(MembershipProviderSqlStatement.FindUsersByEmail) = "" & ControlChars.CrLf &"select " & ControlChars.CrLf &"   UserID UserID" & ControlChars.CrLf &"  ,UserName Username" & ControlChars.CrLf &"  ,Email Email" & ControlChars.CrLf &"  ,PasswordQue"& _ 
                "stion PasswordQuestion" & ControlChars.CrLf &"  ,Comment Comments" & ControlChars.CrLf &"  ,IsApproved IsApproved" & ControlChars.CrLf &"  ,IsLock"& _ 
                "edOut IsLockedOut" & ControlChars.CrLf &"  ,CreationDate CreationDate" & ControlChars.CrLf &"  ,LastLoginDate LastLoginDate" & ControlChars.CrLf &"  ,LastActivityDate LastActivityDate" & ControlChars.CrLf &"  ,LastPasswordChangedDate LastPasswordCh"& _ 
                "angedDate" & ControlChars.CrLf &"  ,LastLockedOutDate LastLockedOutDate" & ControlChars.CrLf &"from Users " & ControlChars.CrLf &"where Email like"& _ 
                " @Email" & ControlChars.CrLf &"order by UserName asc"
        End Sub
        
        Public Overridable ReadOnly Property ConnectionStringSettings() As ConnectionStringSettings
            Get
                Return m_ConnectionStringSettings
            End Get
        End Property
        
        Public ReadOnly Property WriteExceptionsToEventLog() As Boolean
            Get
                Return m_WriteExceptionsToEventLog
            End Get
        End Property
        
        Public Overrides Property ApplicationName() As String
            Get
                Return Me.m_ApplicationName
            End Get
            Set
                Me.m_ApplicationName = value
            End Set
        End Property
        
        Public Overrides ReadOnly Property EnablePasswordReset() As Boolean
            Get
                Return m_EnablePasswordReset
            End Get
        End Property
        
        Public Overrides ReadOnly Property EnablePasswordRetrieval() As Boolean
            Get
                Return m_EnablePasswordRetrieval
            End Get
        End Property
        
        Public Overrides ReadOnly Property RequiresQuestionAndAnswer() As Boolean
            Get
                Return m_RequiresQuestionAndAnswer
            End Get
        End Property
        
        Public Overrides ReadOnly Property RequiresUniqueEmail() As Boolean
            Get
                Return m_RequiresUniqueEmail
            End Get
        End Property
        
        Public Overrides ReadOnly Property MaxInvalidPasswordAttempts() As Integer
            Get
                Return m_MaxInvalidPasswordAttempts
            End Get
        End Property
        
        Public Overrides ReadOnly Property PasswordAttemptWindow() As Integer
            Get
                Return m_PasswordAttemptWindow
            End Get
        End Property
        
        Public Overrides ReadOnly Property PasswordFormat() As MembershipPasswordFormat
            Get
                Return m_PasswordFormat
            End Get
        End Property
        
        Public Overrides ReadOnly Property MinRequiredNonAlphanumericCharacters() As Integer
            Get
                Return m_MinRequiredNonAlphanumericCharacters
            End Get
        End Property
        
        Public Overrides ReadOnly Property MinRequiredPasswordLength() As Integer
            Get
                Return m_MinRequiredPasswordLength
            End Get
        End Property
        
        Public Overrides ReadOnly Property PasswordStrengthRegularExpression() As String
            Get
                Return m_PasswordStrengthRegularExpression
            End Get
        End Property
        
        Public Overridable ReadOnly Property DefaultPasswordFormat() As MembershipPasswordFormat
            Get
                Return MembershipPasswordFormat.Hashed
            End Get
        End Property
        
        Protected Overridable Function CreateSqlStatement(ByVal command As MembershipProviderSqlStatement) As SqlStatement
            Dim sql As SqlText = New SqlText(Statements(command), ConnectionStringSettings.Name)
            sql.Command.CommandText = sql.Command.CommandText.Replace("@", sql.ParameterMarker)
            If sql.Command.CommandText.Contains((sql.ParameterMarker + "ApplicationName")) Then
                sql.AssignParameter("ApplicationName", ApplicationName)
            End If
            sql.Name = ("MyCompany Application Membership Provider - " + command.ToString())
            sql.WriteExceptionsToEventLog = WriteExceptionsToEventLog
            Return sql
        End Function
        
        Private Function GetConfigValue(ByVal configValue As String, ByVal defaultValue As String) As String
            If String.IsNullOrEmpty(configValue) Then
                Return defaultValue
            End If
            Return configValue
        End Function
        
        Public Overrides Sub Initialize(ByVal name As String, ByVal config As NameValueCollection)
            If (config Is Nothing) Then
                Throw New ArgumentNullException("config")
            End If
            If String.IsNullOrEmpty(name) Then
                name = "MyCompanyApplicationMembershipProvider"
            End If
            If String.IsNullOrEmpty(config("description")) Then
                config.Remove("description")
                config.Add("description", "MyCompany application membership provider")
            End If
            MyBase.Initialize(name, config)
            m_ApplicationName = config("applicationName")
            If String.IsNullOrEmpty(m_ApplicationName) Then
                m_ApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath
            End If
            m_MaxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config("maxInvalidPasswordAttempts"), "5"))
            m_PasswordAttemptWindow = Convert.ToInt32(GetConfigValue(config("passwordAttemptWindow"), "10"))
            m_MinRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config("minRequiredNonAlphanumericCharacters"), "1"))
            m_MinRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config("minRequiredPasswordLength"), "7"))
            m_PasswordStrengthRegularExpression = Convert.ToString(GetConfigValue(config("passwordStrengthRegularExpression"), String.Empty))
            m_EnablePasswordReset = Convert.ToBoolean(GetConfigValue(config("enablePasswordReset"), "true"))
            m_EnablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config("enablePasswordRetrieval"), "true"))
            m_RequiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config("requiresQuestionAndAnswer"), "false"))
            m_RequiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config("requiresUniqueEmail"), "true"))
            m_WriteExceptionsToEventLog = Convert.ToBoolean(GetConfigValue(config("writeExceptionsToEventLog"), "false"))
            Dim pwdFormat As String = config("passwordFormat")
            If String.IsNullOrEmpty(pwdFormat) Then
                pwdFormat = DefaultPasswordFormat.ToString()
            End If
            If (pwdFormat = "Hashed") Then
                m_PasswordFormat = MembershipPasswordFormat.Hashed
            Else
                If (pwdFormat = "Encrypted") Then
                    m_PasswordFormat = MembershipPasswordFormat.Encrypted
                Else
                    If (pwdFormat = "Clear") Then
                        m_PasswordFormat = MembershipPasswordFormat.Clear
                    Else
                        Throw New ProviderException("Password format is not supported.")
                    End If
                End If
            End If
            m_ConnectionStringSettings = ConfigurationManager.ConnectionStrings(config("connectionStringName"))
            If ((m_ConnectionStringSettings Is Nothing) OrElse String.IsNullOrEmpty(m_ConnectionStringSettings.ConnectionString)) Then
                Throw New ProviderException("Connection string cannot be blank.")
            End If
            m_ValidationKey = ConfigurationManager.AppSettings("MembershipProviderValidationKey")
            If (String.IsNullOrEmpty(m_ValidationKey) OrElse m_ValidationKey.Contains("AutoGenerate")) Then
                m_ValidationKey = "0042DD859F3BBAA43B47D318AE703343D4B9C48B264342B5E764ADC39DB689516D53ED7A2DCBE826B"& _ 
                    "749D9B64C8FFD817B8A974E71F5E69E39F57F26F76B0E22"
            End If
        End Sub
        
        Public Overrides Function ChangePassword(ByVal username As String, ByVal oldPwd As String, ByVal newPwd As String) As Boolean
            If Not (ValidateUser(username, oldPwd)) Then
                Return false
            End If
            Dim args As ValidatePasswordEventArgs = New ValidatePasswordEventArgs(username, newPwd, false)
            OnValidatingPassword(args)
            If args.Cancel Then
                If (Not (args.FailureInformation) Is Nothing) Then
                    Throw args.FailureInformation
                Else
                    Throw New MembershipPasswordException("Change of password canceled due to new password validation failure.")
                End If
            End If
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.ChangePassword)
                sql.AssignParameter("Password", EncodePassword(newPwd))
                sql.AssignParameter("LastPasswordChangedDate", DateTime.Now)
                sql.AssignParameter("UserName", username)
                Return (sql.ExecuteNonQuery() = 1)
            End Using
        End Function
        
        Public Overrides Function ChangePasswordQuestionAndAnswer(ByVal username As String, ByVal password As String, ByVal newPwdQuestion As String, ByVal newPwdAnswer As String) As Boolean
            If Not (ValidateUser(username, password)) Then
                Return false
            End If
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.ChangePasswordQuestionAndAnswer)
                sql.AssignParameter("PasswordQuestion", newPwdQuestion)
                sql.AssignParameter("PasswordAnswer", EncodePassword(newPwdAnswer))
                sql.AssignParameter("UserName", username)
                Return (sql.ExecuteNonQuery() = 1)
            End Using
        End Function
        
        Public Shared Function EncodeUserPassword(ByVal password As String) As String
            Return CType(Membership.Provider,ApplicationMembershipProviderBase).EncodePassword(password)
        End Function
        
        Public Overloads Shared Sub ValidateUserPassword(ByVal username As String, ByVal password As String)
            ValidateUserPassword(username, password, true)
        End Sub
        
        Public Overloads Shared Sub ValidateUserPassword(ByVal username As String, ByVal password As String, ByVal isNewUser As Boolean)
            Dim args As ValidatePasswordEventArgs = New ValidatePasswordEventArgs(username, password, isNewUser)
            CType(Membership.Provider,ApplicationMembershipProviderBase).OnValidatingPassword(args)
            If args.Cancel Then
                If (Not (args.FailureInformation) Is Nothing) Then
                    Throw args.FailureInformation
                End If
            End If
        End Sub
        
        Public Overrides Function CreateUser(ByVal username As String, ByVal password As String, ByVal email As String, ByVal passwordQuestion As String, ByVal passwordAnswer As String, ByVal isApproved As Boolean, ByVal providerUserKey As Object, ByRef status As MembershipCreateStatus) As MembershipUser
            Dim args As ValidatePasswordEventArgs = New ValidatePasswordEventArgs(username, password, true)
            OnValidatingPassword(args)
            If args.Cancel Then
                status = MembershipCreateStatus.InvalidPassword
                Return Nothing
            End If
            If (RequiresUniqueEmail AndAlso Not (String.IsNullOrEmpty(GetUserNameByEmail(email)))) Then
                status = MembershipCreateStatus.DuplicateEmail
                Return Nothing
            End If
            If (Not (GetUser(username, false)) Is Nothing) Then
                status = MembershipCreateStatus.DuplicateUserName
            Else
                Dim creationDate As DateTime = DateTime.Now
                Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.CreateUser)
                    sql.AssignParameter("UserName", username)
                    sql.AssignParameter("Password", EncodePassword(password))
                    sql.AssignParameter("Email", email)
                    sql.AssignParameter("PasswordQuestion", passwordQuestion)
                    sql.AssignParameter("PasswordAnswer", EncodePassword(passwordAnswer))
                    sql.AssignParameter("IsApproved", isApproved)
                    sql.AssignParameter("Comments", String.Empty)
                    sql.AssignParameter("CreationDate", creationDate)
                    sql.AssignParameter("LastPasswordChangedDate", creationDate)
                    sql.AssignParameter("LastActivityDate", creationDate)
                    sql.AssignParameter("LastLoginDate", creationDate)
                    sql.AssignParameter("IsLockedOut", false)
                    sql.AssignParameter("LastLockedOutDate", creationDate)
                    sql.AssignParameter("FailedPwdAttemptCount", 0)
                    sql.AssignParameter("FailedPwdAttemptWindowStart", creationDate)
                    sql.AssignParameter("FailedPwdAnsAttemptCount", 0)
                    sql.AssignParameter("FailedPwdAnsAttemptWindowStart", creationDate)
                    If (sql.ExecuteNonQuery() > 0) Then
                        status = MembershipCreateStatus.Success
                        Return GetUser(username, false)
                    Else
                        status = MembershipCreateStatus.UserRejected
                    End If
                End Using
            End If
            Return Nothing
        End Function
        
        Public Overrides Function DeleteUser(ByVal username As String, ByVal deleteAllRelatedData As Boolean) As Boolean
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.DeleteUser)
                sql.AssignParameter("UserName", username)
                Return (sql.ExecuteNonQuery() > 0)
            End Using
        End Function
        
        Public Overrides Function GetAllUsers(ByVal pageIndex As Integer, ByVal pageSize As Integer, ByRef totalRecords As Integer) As MembershipUserCollection
            totalRecords = 0
            Dim users As MembershipUserCollection = New MembershipUserCollection()
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.CountAllUsers)
                totalRecords = Convert.ToInt32(sql.ExecuteScalar())
                If (totalRecords <= 0) Then
                    Return users
                End If
            End Using
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.GetAllUsers)
                Dim counter As Integer = 0
                Dim startIndex As Integer = (pageSize * pageIndex)
                Dim endIndex As Integer = ((startIndex + pageSize)  _
                            - 1)
                Do While sql.Read()
                    If (counter >= startIndex) Then
                        users.Add(GetUser(sql))
                    End If
                    If (counter >= endIndex) Then
                        Exit Do
                    End If
                    counter = (counter + 1)
                Loop
            End Using
            Return users
        End Function
        
        Public Overrides Function GetNumberOfUsersOnline() As Integer
            Dim onlineSpan As TimeSpan = New TimeSpan(0, Membership.UserIsOnlineTimeWindow, 0)
            Dim compareDate As DateTime = DateTime.Now.Subtract(onlineSpan)
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.GetNumberOfUsersOnline)
                sql.AssignParameter("CompareDate", compareDate)
                Return Convert.ToInt32(sql.ExecuteScalar())
            End Using
        End Function
        
        Public Overrides Function GetPassword(ByVal username As String, ByVal answer As String) As String
            If Not (EnablePasswordRetrieval) Then
                Throw New ProviderException("Password retrieval is not enabled.")
            End If
            If (PasswordFormat = MembershipPasswordFormat.Hashed) Then
                Throw New ProviderException("Cannot retrieve hashed passwords.")
            End If
            Dim password As String = String.Empty
            Dim passwordAnswer As String = String.Empty
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.GetPassword)
                sql.AssignParameter("UserName", username)
                If sql.Read() Then
                    If Convert.ToBoolean(sql("IsLockedOut")) Then
                        Throw New MembershipPasswordException("User is locked out.")
                    End If
                    password = Convert.ToString(sql("Password"))
                    passwordAnswer = Convert.ToString(sql("PasswordAnswer"))
                Else
                    Throw New MembershipPasswordException("User name is not found.")
                End If
            End Using
            If (RequiresQuestionAndAnswer AndAlso Not (CheckPassword(answer, passwordAnswer))) Then
                UpdateFailureCount(username, "PasswordAnswer")
                Throw New MembershipPasswordException("Incorrect password answer.")
            End If
            If (PasswordFormat = MembershipPasswordFormat.Encrypted) Then
                password = DecodePassword(password)
            End If
            Return password
        End Function
        
        Public Overloads Overrides Function GetUser(ByVal username As String, ByVal userIsOnline As Boolean) As MembershipUser
            Dim u As MembershipUser = Nothing
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.GetUser)
                sql.AssignParameter("UserName", username)
                If sql.Read() Then
                    u = GetUser(sql)
                End If
            End Using
            If (userIsOnline AndAlso (Not (u) Is Nothing)) Then
                Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.UpdateLastUserActivity)
                    sql.AssignParameter("LastActivityDate", DateTime.Now)
                    sql.AssignParameter("UserName", username)
                    sql.ExecuteNonQuery()
                End Using
            End If
            Return u
        End Function
        
        Public Overloads Overrides Function GetUser(ByVal providerUserKey As Object, ByVal userIsOnline As Boolean) As MembershipUser
            Dim u As MembershipUser = Nothing
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.GetUserByProviderKey)
                sql.AssignParameter("UserID", providerUserKey)
                If sql.Read() Then
                    u = GetUser(sql)
                End If
            End Using
            If (userIsOnline AndAlso (Not (u) Is Nothing)) Then
                Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.UpdateLastUserActivity)
                    sql.AssignParameter("LastActivityDate", DateTime.Now)
                    sql.AssignParameter("UserName", u.UserName)
                    sql.ExecuteNonQuery()
                End Using
            End If
            Return u
        End Function
        
        Private Overloads Function GetUser(ByVal sql As SqlStatement) As MembershipUser
            Dim providerUserKey As Object = sql("UserID")
            Dim username As String = Convert.ToString(sql("UserName"))
            Dim email As String = Convert.ToString(sql("Email"))
            Dim passwordQuestion As String = Convert.ToString(sql("PasswordQuestion"))
            Dim comment As String = Convert.ToString(sql("Comments"))
            Dim isApproved As Boolean = Convert.ToBoolean(sql("IsApproved"))
            Dim isLockedOut As Boolean = Convert.ToBoolean(sql("IsLockedOut"))
            Dim creationDate As DateTime = Convert.ToDateTime(sql("CreationDate"))
            Dim lastLoginDate As DateTime = Convert.ToDateTime(sql("LastLoginDate"))
            Dim lastActivityDate As DateTime = Convert.ToDateTime(sql("LastActivityDate"))
            Dim lastPasswordChangedDate As DateTime = Convert.ToDateTime(sql("LastPasswordChangedDate"))
            Dim lastLockedOutDate As DateTime = Convert.ToDateTime(sql("LastLockedOutDate"))
            Return New MembershipUser(Me.Name, username, providerUserKey, email, passwordQuestion, comment, isApproved, isLockedOut, creationDate, lastLoginDate, lastActivityDate, lastPasswordChangedDate, lastLockedOutDate)
        End Function
        
        Public Overrides Function UnlockUser(ByVal username As String) As Boolean
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.UpdateUserLockStatus)
                sql.AssignParameter("IsLockedOut", false)
                sql.AssignParameter("LastLockedOutDate", DateTime.Now)
                sql.AssignParameter("UserName", username)
                Return (sql.ExecuteNonQuery() > 0)
            End Using
        End Function
        
        Public Overrides Function GetUserNameByEmail(ByVal email As String) As String
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.GetUserNameByEmail)
                sql.AssignParameter("Email", email)
                Return Convert.ToString(sql.ExecuteScalar())
            End Using
        End Function
        
        Public Overrides Function ResetPassword(ByVal username As String, ByVal answer As String) As String
            If Not (EnablePasswordReset) Then
                Throw New NotSupportedException("Password reset is not enabled.")
            End If
            If (String.IsNullOrEmpty(answer) AndAlso RequiresQuestionAndAnswer) Then
                UpdateFailureCount(username, "PasswordAnswer")
                Throw New ProviderException("Password answer required for password reset.")
            End If
            Dim newPassword As String = Membership.GeneratePassword(Me.m_NewPasswordLength, MinRequiredNonAlphanumericCharacters)
            Dim args As ValidatePasswordEventArgs = New ValidatePasswordEventArgs(username, newPassword, false)
            OnValidatingPassword(args)
            If args.Cancel Then
                If (Not (args.FailureInformation) Is Nothing) Then
                    Throw args.FailureInformation
                Else
                    Throw New MembershipPasswordException("Reset password canceled due to password validation failure.")
                End If
            End If
            Dim passwordAnswer As String = String.Empty
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.GetPassword)
                sql.AssignParameter("UserName", username)
                If sql.Read() Then
                    If Convert.ToBoolean(sql("IsLockedOut")) Then
                        Throw New MembershipPasswordException("User is locked out.")
                    End If
                    If (CType(sql("FailedPwdAnsAttemptWindowStart"),DateTime).AddMinutes(PasswordAttemptWindow) > DateTime.Now) Then
                        Throw New MembershipPasswordException("Password answer attempt is not yet allowed.")
                    End If
                    passwordAnswer = Convert.ToString(sql("PasswordAnswer"))
                Else
                    Throw New MembershipPasswordException("User is not found.")
                End If
            End Using
            If (RequiresQuestionAndAnswer AndAlso Not (CheckPassword(answer, passwordAnswer))) Then
                UpdateFailureCount(username, "PasswordAnswer")
                Throw New MembershipPasswordException("Incorrect password answer.")
            End If
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.ResetPassword)
                sql.AssignParameter("Password", EncodePassword(newPassword))
                sql.AssignParameter("LastPasswordChangedDate", DateTime.Now)
                sql.AssignParameter("UserName", username)
                sql.AssignParameter("IsLockedOut", false)
                If (sql.ExecuteNonQuery() > 0) Then
                    Return newPassword
                Else
                    Throw New MembershipPasswordException("User is not found or locked out. Password has not been reset.")
                End If
            End Using
        End Function
        
        Public Overrides Sub UpdateUser(ByVal user As MembershipUser)
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.UpdateUser)
                sql.AssignParameter("Email", user.Email)
                sql.AssignParameter("Comments", user.Comment)
                sql.AssignParameter("IsApproved", user.IsApproved)
                sql.AssignParameter("UserName", user.UserName)
                sql.ExecuteNonQuery()
            End Using
        End Sub
        
        Public Overrides Function ValidateUser(ByVal username As String, ByVal password As String) As Boolean
            Dim isValid As Boolean = false
            Dim pwd As String = Nothing
            Dim isApproved As Boolean = true
            username = username.Trim()
            password = password.Trim()
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.GetPassword)
                sql.AssignParameter("UserName", username)
                If sql.Read() Then
                    If Convert.ToBoolean(sql("IsLockedOut")) Then
                        If (CType(sql("FailedPwdAttemptWindowStart"),DateTime).AddMinutes(PasswordAttemptWindow) > DateTime.Now) Then
                            Return false
                        End If
                    End If
                    pwd = Convert.ToString(sql("Password"))
                    isApproved = Convert.ToBoolean(sql("IsApproved"))
                Else
                    Return false
                End If
            End Using
            If CheckPassword(password, pwd) Then
                If isApproved Then
                    isValid = true
                    Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.UpdateLastLoginDate)
                        sql.AssignParameter("LastLoginDate", DateTime.Now)
                        sql.AssignParameter("IsLockedOut", false)
                        sql.AssignParameter("UserName", username)
                        sql.ExecuteNonQuery()
                    End Using
                End If
            Else
                UpdateFailureCount(username, "Password")
            End If
            Return isValid
        End Function
        
        Private Sub UpdateFailureCount(ByVal username As String, ByVal failureType As String)
            Dim failureCount As Integer = 0
            Dim windowStart As DateTime = DateTime.Now
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.GetUser)
                sql.AssignParameter("UserName", username)
                If Not (sql.Read()) Then
                    Return
                End If
                If (failureType = "Password") Then
                    failureCount = Convert.ToInt32(sql("FailedPwdAttemptCount"))
                    If Not (DBNull.Value.Equals(sql("FailedPwdAttemptWindowStart"))) Then
                        windowStart = Convert.ToDateTime(sql("FailedPwdAttemptWindowStart"))
                    End If
                End If
                If (failureType = "PasswordAnswer") Then
                    failureCount = Convert.ToInt32(sql("FailedPwdAnsAttemptCount"))
                    If Not (DBNull.Value.Equals(sql("FailedPwdAnsAttemptWindowStart"))) Then
                        windowStart = Convert.ToDateTime(sql("FailedPwdAnsAttemptWindowStart"))
                    End If
                End If
            End Using
            Dim windowEnd As DateTime = windowStart.AddMinutes(PasswordAttemptWindow)
            If ((failureCount = 0) OrElse (DateTime.Now > windowEnd)) Then
                If (failureType = "Password") Then
                    Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.UpdateFailedPasswordAttempt)
                        sql.AssignParameter("FailedPwdAttemptCount", 1)
                        sql.AssignParameter("FailedPwdAttemptWindowStart", DateTime.Now)
                        sql.AssignParameter("UserName", username)
                        If (sql.ExecuteNonQuery() = 0) Then
                            Throw New ProviderException("Unable to update password failure count and window start.")
                        End If
                    End Using
                End If
                If (failureType = "PasswordAnswer") Then
                    Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.UpdateFailedPasswordAnswerAttempt)
                        sql.AssignParameter("FailedPwdAnsAttemptCount", 1)
                        sql.AssignParameter("FailedPwdAnsAttemptWindowStart", DateTime.Now)
                        sql.AssignParameter("UserName", username)
                        If (sql.ExecuteNonQuery() = 0) Then
                            Throw New ProviderException("Unable to update password answer failure count and window start.")
                        End If
                    End Using
                End If
            Else
                failureCount = (failureCount + 1)
                If (failureCount > MaxInvalidPasswordAttempts) Then
                    Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.UpdateUserLockStatus)
                        sql.AssignParameter("IsLockedOut", true)
                        sql.AssignParameter("LastLockedOutDate", DateTime.Now)
                        sql.AssignParameter("UserName", username)
                        If (sql.ExecuteNonQuery() < 1) Then
                            Throw New ProviderException("Unable to lock out user.")
                        End If
                    End Using
                Else
                    If (failureType = "Password") Then
                        Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.UpdateFailedPasswordAttempt)
                            sql.AssignParameter("FailedPwdAttemptCount", failureCount)
                            sql.AssignParameter("FailedPwdAttemptWindowStart", windowStart)
                            sql.AssignParameter("UserName", username)
                            If (sql.ExecuteNonQuery() = 0) Then
                                Throw New ProviderException("Unable to update password failure count and window start.")
                            End If
                        End Using
                    End If
                    If (failureType = "PasswordAnswer") Then
                        Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.UpdateFailedPasswordAnswerAttempt)
                            sql.AssignParameter("FailedPwdAnsAttemptCount", failureCount)
                            sql.AssignParameter("FailedPwdAnsAttemptWindowStart", windowStart)
                            sql.AssignParameter("UserName", username)
                            If (sql.ExecuteNonQuery() = 0) Then
                                Throw New ProviderException("Unable to update password answer failure count and window start.")
                            End If
                        End Using
                    End If
                End If
            End If
        End Sub
        
        Private Function CheckPassword(ByVal password As String, ByVal currentPassword As String) As Boolean
            Dim pass1 As String = password
            Dim pass2 As String = currentPassword
            If (PasswordFormat = MembershipPasswordFormat.Encrypted) Then
                pass2 = DecodePassword(currentPassword)
            Else
                If (PasswordFormat = MembershipPasswordFormat.Hashed) Then
                    pass1 = EncodePassword(password)
                End If
            End If
            Return (pass1 = pass2)
        End Function
        
        Public Overridable Function EncodePassword(ByVal password As String) As String
            Dim encodedPassword As String = password
            If (PasswordFormat = MembershipPasswordFormat.Encrypted) Then
                encodedPassword = Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)))
            Else
                If (PasswordFormat = MembershipPasswordFormat.Hashed) Then
                    Dim hash As HMACSHA1 = New HMACSHA1()
                    hash.Key = HexToByte(m_ValidationKey)
                    encodedPassword = Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)))
                End If
            End If
            Return encodedPassword
        End Function
        
        Public Overridable Function DecodePassword(ByVal encodedPassword As String) As String
            Dim password As String = encodedPassword
            If (PasswordFormat = MembershipPasswordFormat.Encrypted) Then
                password = Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(encodedPassword)))
            Else
                If (PasswordFormat = MembershipPasswordFormat.Hashed) Then
                    Throw New ProviderException("Cannot decode a hashed password.")
                End If
            End If
            Return password
        End Function
        
        Public Shared Function HexToByte(ByVal hexString As String) As Byte()
            Dim returnBytes(((hexString.Length / 2)) - 1) As Byte
            Dim i As Integer = 0
            Do While (i < returnBytes.Length)
                returnBytes(i) = Convert.ToByte(hexString.Substring((i * 2), 2), 16)
                i = (i + 1)
            Loop
            Return returnBytes
        End Function
        
        Public Overrides Function FindUsersByName(ByVal usernameToMatch As String, ByVal pageIndex As Integer, ByVal pageSize As Integer, ByRef totalRecords As Integer) As MembershipUserCollection
            Dim users As MembershipUserCollection = New MembershipUserCollection()
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.CountUsersByName)
                sql.AssignParameter("UserName", usernameToMatch)
                totalRecords = Convert.ToInt32(sql.ExecuteScalar())
            End Using
            If (totalRecords > 0) Then
                Dim counter As Integer = 0
                Dim startIndex As Integer = (pageSize * pageIndex)
                Dim endIndex As Integer = ((startIndex + pageSize)  _
                            - 1)
                Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.FindUsersByName)
                    sql.AssignParameter("UserName", usernameToMatch)
                    Do While sql.Read()
                        If (counter >= startIndex) Then
                            users.Add(GetUser(sql))
                        End If
                        If (counter >= endIndex) Then
                            Exit Do
                        End If
                        counter = (counter + 1)
                    Loop
                End Using
            End If
            Return users
        End Function
        
        Public Overrides Function FindUsersByEmail(ByVal emailToMatch As String, ByVal pageIndex As Integer, ByVal pageSize As Integer, ByRef totalRecords As Integer) As MembershipUserCollection
            Dim users As MembershipUserCollection = New MembershipUserCollection()
            Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.CountUsersByEmail)
                sql.AssignParameter("UserName", emailToMatch)
                totalRecords = Convert.ToInt32(sql.ExecuteScalar())
            End Using
            If (totalRecords > 0) Then
                Dim counter As Integer = 0
                Dim startIndex As Integer = (pageSize * pageIndex)
                Dim endIndex As Integer = ((startIndex + pageSize)  _
                            - 1)
                Using sql As SqlStatement = CreateSqlStatement(MembershipProviderSqlStatement.FindUsersByEmail)
                    sql.AssignParameter("Email", emailToMatch)
                    Do While sql.Read()
                        If (counter >= startIndex) Then
                            users.Add(GetUser(sql))
                        End If
                        If (counter >= endIndex) Then
                            Exit Do
                        End If
                        counter = (counter + 1)
                    Loop
                End Using
            End If
            Return users
        End Function
        
        Protected Overrides Sub OnValidatingPassword(ByVal e As ValidatePasswordEventArgs)
            Try 
                Dim password As String = e.Password
                If (password.Length < MinRequiredPasswordLength) Then
                    Throw New ArgumentException("Invalid password length.")
                End If
                Dim count As Integer = 0
                Dim i As Integer = 0
                Do While (i < password.Length)
                    If Not ([Char].IsLetterOrDigit(password, i)) Then
                        count = (count + 1)
                    End If
                    i = (i + 1)
                Loop
                If (count < MinRequiredNonAlphanumericCharacters) Then
                    Throw New ArgumentException("Password needs more non-alphanumeric characters.")
                End If
                If Not (String.IsNullOrEmpty(PasswordStrengthRegularExpression)) Then
                    If Not (Regex.IsMatch(password, PasswordStrengthRegularExpression)) Then
                        Throw New ArgumentException("Password does not match regular expression.")
                    End If
                End If
                MyBase.OnValidatingPassword(e)
                If e.Cancel Then
                    If (Not (e.FailureInformation) Is Nothing) Then
                        Throw e.FailureInformation
                    Else
                        Throw New ArgumentException("Custom password validation failure.")
                    End If
                End If
            Catch ex As Exception
                e.FailureInformation = ex
                e.Cancel = true
            End Try
        End Sub
    End Class
End Namespace

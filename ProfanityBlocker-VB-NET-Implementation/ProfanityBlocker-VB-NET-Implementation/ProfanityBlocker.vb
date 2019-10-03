Imports Newtonsoft.Json
Imports System.Net
Imports System.Text

Public Class ProfanityBlocker
    Public ErrorCode2Msg = New Dictionary(Of Integer, String) From {
        {0, "There was an error contacting the ProfanityBlocker service. Please try again later."},
        {104, "There was an error with your licence for ProfanityBlocker. Please check your licence is valid and active and try again."},
        {102, "There was an error with your account. Please try again later."},
        {999, "You have sent too many requests than you have paid for in your package, you can either upgrade your package or wait."}}

    Public ApiKey As String
    Public LinkFilter As Boolean
    Public EmailFilter As Boolean
    Public PhoneFilter As Boolean

    Public Sub New(Api, Link, Phone, Email)
        ApiKey = Api
        LinkFilter = Link
        PhoneFilter = Phone
        EmailFilter = Email
    End Sub

    Private Function CallAPI(Text)
        Dim s As HttpWebRequest
        Dim enc As UTF8Encoding
        Dim postdata As String
        Dim postdatabytes As Byte()
        s = HttpWebRequest.Create("http://service.profanity-blocker.co.uk/restServer.php")
        enc = New System.Text.UTF8Encoding()
        postdata = "type=json&link=" + LinkFilter.ToString() + "&phone=" + PhoneFilter.ToString() + "&email=" + EmailFilter.ToString() + "&key=" + ApiKey + "&text=" + Text
        postdatabytes = enc.GetBytes(postdata)
        s.Method = "POST"
        s.ContentType = "application/x-www-form-urlencoded"
        s.ContentLength = postdatabytes.Length

        Using stream = s.GetRequestStream()
            stream.Write(postdatabytes, 0, postdatabytes.Length)
        End Using
        Dim result = s.GetResponse()
        Dim strReader As New IO.StreamReader(result.GetResponseStream(), Encoding.UTF8)
        Dim ServerResponse As String = strReader.ReadToEnd
        result.Close()
        Dim jsonResulttodict = JsonConvert.DeserializeObject(Of Dictionary(Of String, Object))(ServerResponse)
        Return jsonResulttodict
    End Function

    Private Sub ThrowError(Code As Integer)
        Dim Value As String
        If (ErrorCode2Msg.TryGetValue(Code, Value)) Then
            Throw New System.Exception(Value)
        End If
    End Sub

    Public Function ParseText(Text)
        Dim ApiRes As Dictionary(Of String, Object) = CallAPI(Text)
        Dim Value As Integer
        Dim TextResp As String
        If (ApiRes.TryGetValue("errorCode", Value)) Then
            ThrowError(Value)
            Return Text
        End If
        If (ApiRes.TryGetValue("text_parsed", TextResp)) Then
            Return TextResp
        End If
        ThrowError(0)
        Return Text
    End Function

End Class

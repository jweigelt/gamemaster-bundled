'Static Logger
'JW "LeKeks" 05/2014
Namespace Util
    Public Enum LogLevel
        Verbose = 1
        Info = 2
        Warning = 3
        Exception = 4
    End Enum

    Public Class Logger
        Public Shared Property LogToFile As Boolean = False
        Public Shared Property LogFileName As String = "/log.txt"
        Public Shared Property MinLogLevel As LogLevel = LogLevel.Info

        Public Shared Sub Log(ByVal message As String, ByVal level As LogLevel, ParamArray tags() As String)
            If Not level >= MinLogLevel Then Return

            For i = 0 To tags.Length - 1
                message = message.Replace("{" & i.ToString() & "}", tags(i))
            Next

            Select Case level
                Case LogLevel.Verbose
                    message = "DEBUG | " & message
                Case LogLevel.Warning
                    message = "WARN  | " & message
                Case LogLevel.Exception
                    message = "EX    | " & message
                Case LogLevel.Info
                    message = "INFO  | " & message
            End Select

            message = "[" & DateTime.Now().ToString() & "] " & message

            Console.WriteLine(message)
            Debug.WriteLine(message)

            If LogToFile = True Then
                IO.File.AppendAllText(Environment.CurrentDirectory & LogFileName, message & vbCrLf)
            End If

            If level = LogLevel.Exception Then
                Console.WriteLine("The server has crashed.")
                Environment.Exit(-1)
            End If
        End Sub
    End Class
End Namespace
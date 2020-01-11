Imports gamemaster.common.Util
Namespace Config
    Public Class KeyauthConfig
        Public Property UDPHeartbeatPort As Int32 = 29910
        Public Property UDPHeartbeatAddress As String = "0.0.0.0"

        Public Property LogToFile As Boolean = False
        Public Property LogFileName As String = "/log.txt"

        Public Property Loglevel As LogLevel = Loglevel.Info
    End Class
End Namespace
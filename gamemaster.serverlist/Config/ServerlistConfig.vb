Imports gamemaster.common.Util

Namespace Config
    Public Class ServerlistConfig
        Public Property UDPHeartbeatPort As UShort = 27900
        Public Property UDPFWCheckPort As UShort = 27920

        Public Property UDPHeartbeatAddress As String = "0.0.0.0"
        Public Property TCPQueryPort As UShort = 28910
        Public Property TCPQueryAddress As String = "0.0.0.0"

        Public Property GameserverTimeout As Integer = 60
        Public Property PlayerTimeout As Integer = 120

        Public Property LogToFile As Boolean = False
        Public Property LogFileName As String = "/log.txt"

        Public Property Loglevel As LogLevel = LogLevel.Info

        Public Property MySQLHostname As String = "localhost"
        Public Property MySQLPort As UShort = 3306
        Public Property MySQLDatabase As String = "gamemaster"
        Public Property MySQLUsername As String = "root"
        Public Property MySQLPwd As String = ""

        Public Property ServerID As Integer = 0

        Public Property P2PPort As UShort = 14130
        Public Property P2PAddress As String = "0.0.0.0"
        Public Property P2PEnable As Boolean = False
        Public Property P2PKey As String = "abcd"
    End Class
End Namespace
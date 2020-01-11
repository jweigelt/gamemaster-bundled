Imports gamemaster.common.Util

Namespace Config
    Public Class NatnegConfig
        Public Property UDPHeartbeatPort As Int32 = 27901
        Public Property UDPHeartbeatAddress As String = "0.0.0.0"
        Public Property CleanupInterval As Int32 = 3600
        Public Property CleanupTimeout As Int32 = 3600

        Public Property LogToFile As Boolean = False
        Public Property LogFileName As String = "/log.txt"

        Public Property Loglevel As LogLevel = Loglevel.Info

        Public Property MySQLHostname As String = "localhost"
        Public Property MySQLPort As Int32 = 3306
        Public Property MySQLDatabase As String = "gamemaster"
        Public Property MySQLUsername As String = "root"
        Public Property MySQLPwd As String = ""

        Public Property ServerID As Int32 = 0

        Public Property P2PPort As Int32 = 14131
        Public Property P2PAddress As String = "0.0.0.0"
        Public Property P2PEnable As Boolean = False
        Public Property P2PKey As String = "abcd"

    End Class
End Namespace
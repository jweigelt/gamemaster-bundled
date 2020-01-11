
Imports gamemaster.common.Util
Namespace Network.Hearbeat
    Public Class ServerHeartbeatPacket
        Inherits HearbeatUdpPacket

        Sub New(ByVal server As ServerlistServer, ByVal remoteIPEP As Net.IPEndPoint)
            MyBase.New(server, remoteIPEP)
        End Sub

        Public Overrides Sub ManageData()
            Me.SetupIDs()
            'If Me.Server.Server.MySQL.ServerExists(Me.RemoteIPEP) Then
            Me.Server.MySQL.SetHeartBeat(Me.RemoteIPEP)
            Logger.Log("Received UDP heartbeat from {0}", LogLevel.Verbose, Me.RemoteIPEP.Address.ToString)
            'End If
        End Sub
    End Class
End Namespace
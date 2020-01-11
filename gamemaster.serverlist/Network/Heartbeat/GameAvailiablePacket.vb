'Implementation of gamespy's available service
'JW "LeKeks" 06/2014

Imports gamemaster.common.Network
Imports gamemaster.common.Util

Namespace Network.Hearbeat
    Public Class GameAvailiablePacket
        Inherits HearbeatUdpPacket

        Sub New(ByVal server As ServerlistServer, ByVal remoteIPEP As Net.IPEndPoint)
            MyBase.New(server, remoteIPEP)
        End Sub

        Public Overrides Sub ManageData()
            Me.SetupIDs()
            Logger.Log("Got avail.-request from {0}", LogLevel.Verbose, Me.RemoteIPEP.ToString)
            Me.Server.GSUdpServer.Send(Me)
        End Sub

        Public Overrides Function CompileResponse() As Byte()
            'Just gonna tell the client everything is up and ready for that game for now
            Dim buf() As Byte = {&HFE, &HFD, GsConst.GS_MASTER_CMD_AVAILIABLE, &H0, &H0, &H0, &H0}
            Return buf
        End Function
    End Class
End Namespace
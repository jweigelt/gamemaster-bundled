Imports gamemaster.common.Network
Imports gamemaster.common.Util
Imports gamemaster.serverlist.Network

Namespace Network.Hearbeat
    Public Class HeartbeatAckdPacket
        Inherits HearbeatUdpPacket

        Sub New(ByVal server As ServerlistServer, ByVal remoteIPEP As Net.IPEndPoint)
            MyBase.New(server, remoteIPEP)
        End Sub

        Public Overrides Function CompileResponse() As Byte()
            Dim buf() As Byte = {&HFE, &HFD, GsConst.GS_MASTER_CMD_CHALLENGE_ACK}
            ArrayFunctions.ConcatArray(Me.ClientUUID, buf)
            'no idea why they attached these bytes, however the clients wants them -> it gets them
            ArrayFunctions.ConcatArray({0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, buf)
            Return buf
        End Function
    End Class
End Namespace
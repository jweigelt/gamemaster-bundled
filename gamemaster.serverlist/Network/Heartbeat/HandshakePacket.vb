Imports gamemaster.common.Network
Imports gamemaster.common.Util

Namespace Network.Hearbeat

    Public Class HandshakePacket
        Inherits HearbeatUdpPacket

        Sub New(ByVal server As ServerlistServer, ByVal remoteIPEP As Net.IPEndPoint)
            MyBase.New(server, remoteIPEP)
        End Sub

        Public Overrides Sub ManageData()
            Me.SetupIDs()
            Logger.Log("Got response from {0}", LogLevel.Verbose, Me.RemoteIPEP.ToString)
            Me.Server.MySQL.SetHeartBeat(Me.RemoteIPEP, False, True)
        End Sub

        Public Overrides Function CompileResponse() As Byte()
            Dim buf() As Byte = {}
            ArrayFunctions.ConcatArray(GsConst.GS_SERVICE_MASTER_PREFIX, buf)
            ArrayFunctions.ConcatArray({GsConst.GS_MASTER_CMD_HANDSHAKE}, buf)
            ArrayFunctions.ConcatArray(Me.ClientUUID, buf)
            ArrayFunctions.ConcatArray(ArrayFunctions.GetBytes(GsConst.GS_HANDSHAKE_STRING), buf)
            Return buf
        End Function

    End Class
End Namespace
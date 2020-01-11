'Validates a gameserver-connect
'JW "LeKeks" 07/2014

Imports gamemaster.common.Network
Imports gamemaster.common.Util
Imports gamemaster.serverlist.Gameserver

Namespace Network.Hearbeat

    Public Class ChallengeRequestPacket
        Inherits HearbeatUdpPacket

        Public Property GServer As GsGameServer

        Sub New(ByVal server As ServerlistServer, ByVal remoteIPEP As Net.IPEndPoint, Optional ByVal gserver As GsGameServer = Nothing)
            MyBase.New(server, remoteIPEP)

            'Ensure there's a server
            If Not gserver Is Nothing Then
                Me.GServer = gserver
            End If
        End Sub

        Public Overrides Sub ManageData()
            Me.SetupIDs()
            'get the challenge-token
            Dim token As String = ArrayFunctions.GetString(Me.Data)
            Logger.Log("Received token '{0}' from {1}", LogLevel.Verbose, token, Me.RemoteIPEP.ToString)

            'Ack the packet
            Dim hap As New HeartbeatAckdPacket(Me.Server, Me.RemoteIPEP)
            hap.SetupIDs(Me)
            Me.Server.GSUdpServer.Send(hap)
            Logger.Log("Ack'd token from {0}", LogLevel.Verbose, Me.RemoteIPEP.ToString)

            'check if we can directly connect to the server
            Dim hsp As New HandshakePacket(Me.Server, Me.RemoteIPEP)
            hsp.SetupIDs(Me)
            Me.Server.GSUdpServer.SendCheck(hsp)

            Logger.Log("Sending firewall-check-packet to {0}", LogLevel.Verbose, Me.RemoteIPEP.Address.ToString)

            'Also update the server as we ack'd it's last heartbeat
            Me.Server.MySQL.SetHeartBeat(Me.RemoteIPEP, True)
        End Sub

        Public Overrides Function CompileResponse() As Byte()
            Dim buf() As Byte = GsConst.GS_SERVICE_MASTER_PREFIX

            ArrayFunctions.ConcatArray({GsConst.GS_MASTER_CMD_CHALLENGE}, buf)
            ArrayFunctions.ConcatArray(Me.ClientUUID, buf)
            ArrayFunctions.ConcatArray(Me.GServer.GetChallengeToken(), buf)
            ArrayFunctions.ConcatArray({&H30, &H30}, buf)

            Dim addr() As Byte = Me.RemoteIPEP.Address.GetAddressBytes()
            Dim port() As Byte = ArrayFunctions.DumpUInt16LE(CUShort(Me.RemoteIPEP.Port))

            'The client's public ip is attached
            Dim rIPEPHexDmp As String = String.Empty
            rIPEPHexDmp &= GetHexDump(addr)
            rIPEPHexDmp &= GetHexDump(port)

            ArrayFunctions.ConcatArray(ArrayFunctions.GetBytes(rIPEPHexDmp), buf)
            'terminate the string
            ArrayFunctions.ConcatArray({&H0}, buf)
            Return buf
        End Function

        Private Function GetHexDump(ByVal buf() As Byte) As String
            Dim r As String = String.Empty
            For i = 0 To buf.Length - 1
                Dim rb As String = buf(i).ToString("X")
                If rb.Length = 1 Then rb = "0" & rb
                r &= rb
            Next
            Return r
        End Function

    End Class
End Namespace
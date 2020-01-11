'Message-Forward delivery packet
'JW "LeKeks" 08/2014
Imports gamemaster.common.Network
Imports gamemaster.common.Util
Imports gamemaster.serverlist.Network

Namespace Network.Hearbeat

    Public Class ClientMessagePacket
        Inherits HearbeatUdpPacket

        Public Property PeerIPEP As Net.IPEndPoint
        Public Property Payload As Byte()

        Sub New(ByVal server As ServerlistServer, ByVal rIPEP As Net.IPEndPoint)
            MyBase.New(server, rIPEP)
        End Sub

        Public Overrides Sub ManageData()
            Logger.Log("{0} ack'd message", LogLevel.Verbose, Me.RemoteIPEP.ToString)
        End Sub

        Public Overrides Function CompileResponse() As Byte()
            'This packet is requested by another client thus we have to fetch the other client's uuid from
            'the database
            Me.ClientUUID = BitConverter.GetBytes(Me.Server.MySQL.FetchClientID(Me.RemoteIPEP))

            'Generate a random message-ID since we're not using it for
            'message-identification it doesn't have to be unique
            Dim msgID(3) As Byte

            With New Random()
                .NextBytes(msgID)
            End With

            'Build the packet
            Dim buffer() As Byte = {}
            ArrayFunctions.ConcatArray(GsConst.GS_SERVICE_MASTER_PREFIX, buffer)
            ArrayFunctions.ConcatArray({GsConst.GS_MASTER_CMD_MESSAGE}, buffer)
            ArrayFunctions.ConcatArray(Me.ClientUUID, buffer)
            ArrayFunctions.ConcatArray(msgID, buffer)
            ArrayFunctions.ConcatArray(Me.Payload, buffer)

            Return buffer
        End Function

    End Class
End Namespace
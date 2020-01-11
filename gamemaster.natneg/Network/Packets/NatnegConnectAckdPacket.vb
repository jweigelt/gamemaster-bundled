'packet for handling acknowledgements from natneg peers
'JW "LeKeks" 07/2014
Imports gamemaster.common.Util

Namespace Network.Packets

    Public Class NatnegConnectAckdPacket
        Inherits NatnegPacket

        Sub New(ByVal server As NatnegServer, ByVal remoteIPEP As Net.IPEndPoint, ByVal protocolVersion As Byte)
            MyBase.New(server, remoteIPEP, protocolVersion)
        End Sub

        Private cookie As Int32
        Private clientType As Byte

        Public Overrides Sub ManageData()
            Logger.Log(Me.RemoteIPEP.ToString & " ack'd Connect", LogLevel.Verbose)
            Me.cookie = BitConverter.ToInt32(Me.data, Me.bytesParsed)
            Me.bytesParsed += 5 'skip first byte
            Me.clientType = Me.data(Me.bytesParsed)

            'drop the session if the guest ack'd connect (it'll do that after connecting to the host)
            If Me.clientType = GsConst.GS_NATNEG_CLIENTTYPE_GUEST Then
                Logger.Log("Dropping session " & Me.cookie.ToString, LogLevel.Verbose)
                Me.Server.MySQL.DropSession(cookie)
            End If
        End Sub
    End Class
End Namespace

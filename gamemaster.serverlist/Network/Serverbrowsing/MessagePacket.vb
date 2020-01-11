Imports gamemaster.common.Util
Imports gamemaster.serverlist.Cluster
Imports gamemaster.serverlist.Network.Hearbeat

Namespace Network.Serverbrowsing
    Public Class MessagePacket
        Inherits ServerbrowsingPacket

        Private cookie(3) As Byte
        Private payload() As Byte

        Sub New(ByVal client As ServerlistClient, Optional ByVal data() As Byte = Nothing)
            MyBase.New(client, data)
        End Sub

        Public Overrides Sub ManageData()
            Dim peerIPEP As Net.IPEndPoint = ArrayFunctions.GetIPEndPointFromByteArray(Data, Me.bytesParsed)
            Me.bytesParsed += 6

            Dim message(Me.Data.Length - Me.bytesParsed - 1) As Byte
            Me.payload = message
            Array.Copy(Me.Data, bytesParsed, message, 0, message.Length)
            Array.Copy(Me.Data, Me.Data.Length - 4, cookie, 0, cookie.Length)

            If Not Me.Client.Server.Config.P2PEnable Then
                Logger.Log("Forwarding Message-Request to Peer at {0} ", LogLevel.Verbose, peerIPEP.ToString)
                Dim cmp As New ClientMessagePacket(Me.Client.Server, peerIPEP) With {
                    .PeerIPEP = peerIPEP,
                    .Payload = message
                }
                Me.Client.Server.GSUdpServer.Send(cmp)
            Else
                'Check if we're the masterserver handling the client
                Dim ms As MasterServer = Me.Client.Server.MySQL.GetManagingMasterserver(peerIPEP)
                If ms.Id = Me.Client.Server.Config.ServerID Then
                    Logger.Log("Forwarding Message-Request to Peer at {0} ", LogLevel.Verbose, peerIPEP.ToString)
                    Dim cmp As New ClientMessagePacket(Me.Client.Server, peerIPEP) With {
                        .PeerIPEP = peerIPEP,
                        .Payload = message
                    }
                    Me.Client.Server.GSUdpServer.Send(cmp)
                Else
                    'Forward the packet to the proper masterserver which will deliver it to the client
                    'The packet can't be send by the local instance since the client is "connected" to the other server
                    '(Every NAT will block it)
                    Logger.Log("MS-Forward to {0} at {1}", LogLevel.Verbose, ms.MsName, ms.RIPEP.ToString)
                    Dim mfp As New MessageForwardPacket(Me.Client.Server, ms.RIPEP) With {
                        .FwdIPEP = peerIPEP,
                        .FwdPayload = message
                    }
                    Me.Client.Server.MSP2PHandler.Send(mfp)
                End If
            End If
            Me.Client.Send(Me)
        End Sub

        Public Overrides Function CompileResponse() As Byte()
            Dim buffer() As Byte = {}
            ArrayFunctions.ConcatArray(GsConst.GS_SERVICE_NATNEG_PREFIX, buffer)
            ArrayFunctions.ConcatArray(GsConst.GS_NATNEG_HEADER, buffer)
            ArrayFunctions.ConcatArray(Me.cookie, buffer)
            Return buffer
        End Function

    End Class
End Namespace
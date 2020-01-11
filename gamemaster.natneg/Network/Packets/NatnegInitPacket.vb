'packet for handling session requests/updates
'JW "LeKeks" 07/2014
Imports gamemaster.common.Util
Imports gamemaster.natneg.Cluster

Namespace Network.Packets
    Public Class NatnegInitPacket
        Inherits NatnegPacket

        Private cookie As Int32             'session's cookie
        Private sequence As Byte            'sequence id
        Private clienttype As Byte          'client type (guest/host)
        Private usegameport As Byte         'shall we use the game's def.-port?
        Private localIPEP As Net.IPEndPoint 'the client's local IPEP

        Sub New(ByVal server As NatnegServer, ByVal remoteIPEP As Net.IPEndPoint, ByVal protocolVersion As Byte)
            MyBase.New(server, remoteIPEP, protocolVersion)
        End Sub

        Public Property GameName As String

        Public Overrides Sub ManageData()
            Logger.Log("Client init: " & Me.RemoteIPEP.ToString, LogLevel.Verbose)

            'The cookie consists out of 4 bytes
            Me.cookie = BitConverter.ToInt32(Me.Data, Me.bytesParsed)
            Me.bytesParsed += 4

            'The next 3 bytes deliver the further information
            Me.sequence = Me.Data(Me.bytesParsed)
            Me.clienttype = Me.Data(Me.bytesParsed + 1)
            Me.usegameport = Me.Data(Me.bytesParsed + 2)
            Me.bytesParsed += 3

            'The gamename will be attached for some games, however some will not
            'anyhow it seems to be ignored -> we can use it for debugging
            Me.GameName = "protocol #" & Me.ProtocolVersion.ToString()

            'The newer protocol will also send a local IPEP
            If Me.ProtocolVersion > GsConst.GS_NATNEG_OLDPROTOCOL Then
                Me.localIPEP = ArrayFunctions.GetIPEndPointFromByteArray(Me.Data, Me.bytesParsed)
                Me.bytesParsed += 6
                'Me.GameName = Me.FetchString(Me.data)
            Else
                Me.localIPEP = New Net.IPEndPoint(0, 0)
            End If

            Logger.Log("Received Natneg Init Packet #{0}", LogLevel.Verbose, Me.sequence.ToString())

            'Create a new session
            If sequence = 0 Then
                Me.Server.MySQL.RegisterNatnegToken(Me.localIPEP, Me.RemoteIPEP, Me.cookie, Me.GameName, Me.sequence, Me.clienttype)

                If clienttype = GsConst.GS_NATNEG_CLIENTTYPE_GUEST Then
                    Logger.Log("Creating Host Session {0} for {1} / {2}", LogLevel.Verbose, Me.cookie.ToString(), Me.RemoteIPEP.ToString(), Me.localIPEP.ToString())
                Else
                    Logger.Log("Creating Guest Session {0} for {1} / {2}", LogLevel.Verbose, Me.cookie.ToString(), Me.RemoteIPEP.ToString(), Me.localIPEP.ToString())
                End If
            Else 'Update an existing session
                Me.Server.MySQL.RegisterNatnegToken(Me.localIPEP, Me.RemoteIPEP, Me.cookie, GameName, Me.sequence, Me.clienttype, Me.RemoteIPEP.Port)
                Logger.Log("Updating Session {0} for {1} / {2}", LogLevel.Verbose, Me.cookie.ToString(), Me.RemoteIPEP.ToString(), Me.localIPEP.ToString())
            End If

            'ack the packet
            Me.Server.GSUdpServer.Send(Me)

            'If both sessions are ready tell the peers to connect to each other
            If Me.Server.MySQL.NatnegReady(Me.cookie) Then
                Logger.Log("Init Sequence OK for '" & Me.cookie.ToString & "', '" & Me.GameName & "' at " & Me.RemoteIPEP.ToString & "/" & Me.localIPEP.ToString, LogLevel.Verbose)
                Me.PerformConnect()
            End If
        End Sub

        Private Sub ConnectPeers(ByVal peer As NatnegPeer, ByVal remotePeer As NatnegPeer)
            'Check if we can use the local socket
            If Not Me.Server.Config.P2PEnable Or peer.ms.Id = Me.Server.Config.ServerID Then
                Dim cp As New NatnegConnectPacket(Me.Server, peer.comIPEP, Me.ProtocolVersion) With {
                    .Destination = remotePeer.hostIPEP,
                    .Cookie = Me.cookie
                }

                If remotePeer.comIPEP Is Nothing Then               'seems like there's no server
                    cp.Failed = True                                'report error
                    Logger.Log("Ready to connect {0} but no Peer could be found - sending error", LogLevel.Verbose, Me.cookie.ToString())
                ElseIf peer.comIPEP Is Nothing Then 'own peer failed -> no point in sending a report
                    Logger.Log("Ready to connect {0} but no Peer could be found", LogLevel.Verbose, Me.cookie.ToString())
                    Return
                Else
                    Logger.Log("Connecting {0} to peer at {1}", LogLevel.Verbose, peer.comIPEP.ToString(), remotePeer.hostIPEP.ToString())
                End If

                Me.Server.GSUdpServer.Send(cp)
            Else 'Forward the packet using MS-P2P protocol
                Logger.Log("Connecting via {0} ({1}) to Peer at {2}", LogLevel.Verbose, peer.ms.MsName, peer.ms.RIPEP.Port.ToString(), remotePeer.hostIPEP.ToString())
                'setup the packet and send it
                Dim cfp As New ConnectForwardPacket(Me.Server, peer.ms.RIPEP) With {
                    .Cookie = Me.cookie,
                    .FwdIPEP = peer.comIPEP,
                    .RemotePeer = remotePeer.hostIPEP,
                    .ProtocolVersion = Me.ProtocolVersion
                }
                Me.Server.MSP2PHandler.send(cfp)
            End If
        End Sub
        Private Sub PerformConnect()
            Dim hostPeer As NatnegPeer = Me.Server.MySQL.FetchRemotePeer(Me.cookie, GsConst.GS_NATNEG_CLIENTTYPE_HOST)
            Dim guestPeer As NatnegPeer = Me.Server.MySQL.FetchRemotePeer(Me.cookie, GsConst.GS_NATNEG_CLIENTTYPE_GUEST)

            ConnectPeers(guestPeer, hostPeer)
            ConnectPeers(hostPeer, guestPeer)
        End Sub
        Public Overrides Function CompileResponse() As Byte()
            Dim buffer() As Byte = {}
            'Build the response-packet
            ArrayFunctions.ConcatArray(GsConst.GS_SERVICE_NATNEG_PREFIX, buffer)
            ArrayFunctions.ConcatArray(GsConst.GS_NATNEG_HEADER, buffer)
            ArrayFunctions.ConcatArray({Me.ProtocolVersion, GsConst.GS_NATNEG_CMD_INIT_ACK}, buffer)
            ArrayFunctions.ConcatArray(BitConverter.GetBytes(Me.cookie), buffer)
            ArrayFunctions.ConcatArray({Me.sequence, Me.clienttype}, buffer)
            ArrayFunctions.ConcatArray(GsConst.GS_NATNEG_FIN, buffer)
            Return buffer
        End Function
    End Class
End Namespace
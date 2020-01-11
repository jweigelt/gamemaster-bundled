'List Update & Server Reg Packet

Imports gamemaster.common.Util
Imports gamemaster.serverlist.Gameserver

Namespace Network.Hearbeat

    Public Class ServerRegisterPacket
        Inherits HearbeatUdpPacket

        Sub New(ByVal server As ServerlistServer, ByVal remoteIPEP As Net.IPEndPoint)
            MyBase.New(server, remoteIPEP)
        End Sub

        Public Overrides Sub ManageData()
            Me.SetupIDs()
            Dim server As New GsGameServer()
            Dim params(Me.Data.Length - Me.bytesParsed - 1) As Byte
            Array.Copy(Me.Data, Me.bytesParsed, params, 0, params.Length)

            server.ParseParameterArray(params)
            'server.ParseParameterString(System.Text.Encoding.ASCII.GetString(params))   'Let the server-object parse the string
            server.PublicIP = Me.RemoteIPEP.Address.ToString
            server.PublicPort = Me.RemoteIPEP.Port.ToString
            server.ClientID = BitConverter.ToInt32(Me.ClientUUID, 0)    'is required for future communication (e.g. message-forwarding)

            If Not Me.Server.MySQL.ServerExists(Me.RemoteIPEP) Then
                Logger.Log("Registering server {0}:{1}", LogLevel.Info, server.PublicIP, server.PublicPort.ToString)
                Me.Server.MySQL.RegisterServer(server)
                SendChallenge(server)
            Else
                If Not Me.Server.MySQL.ServerActive(Me.RemoteIPEP, Me.Server.Config.GameserverTimeout) Then
                    Me.Server.MySQL.ResetChallenge(Me.RemoteIPEP) 'Reset the Challenge-fields if the server has been down
                    SendChallenge(server)
                ElseIf server.StateChanged = "2" Or server.StateChanged = "3" Then  'some games send this packet and expect a validation
                    SendChallenge(server)
                ElseIf Not Me.Server.MySQL.ChallengeOK(Me.RemoteIPEP) Then
                    SendChallenge(server)
                End If
                Me.Server.MySQL.UpdateServer(server) 'Just a regular heartbeat, just gonna assume everything is ok
                Logger.Log("Received list update from {0}:{1}", LogLevel.Verbose, server.PublicIP, server.PublicPort.ToString)
            End If
        End Sub


        Private Sub SendChallenge(ByVal server As GsGameServer)
            Dim crq As New ChallengeRequestPacket(Me.Server, Me.RemoteIPEP, server)
            crq.SetupIDs(Me)
            Me.Server.GSUdpServer.Send(crq)
            Logger.Log("Requested Challenge Key from " & server.PublicIP & ":" & server.PublicPort.ToString, LogLevel.Verbose)
        End Sub
    End Class
End Namespace
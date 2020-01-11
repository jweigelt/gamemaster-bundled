'UDP-server wrapper for GM Masterservers
'JW "LeKeks" 05/2014
Imports System.Net.Sockets
Imports System.Threading
Imports System.Net
Imports gamemaster.common.Util
Imports gamemaster.common.Network
Imports gamemaster.serverlist.Network.Hearbeat

Namespace Network
    Public Class MSUdpServer
        Inherits UdpServer
        Public Property CheckPort As UInt16 = 27910

        Private ReadOnly server As ServerlistServer

        Sub New(ByVal server As ServerlistServer)
            Me.server = server
        End Sub

        Public Overrides Sub OnDataInput(ByVal data() As Byte, ByVal rIPEP As Net.IPEndPoint)
            MyBase.OnDataInput(data, rIPEP)

            'packet-handling
            Dim p As GsUdpPacket
            Select Case data(0)
                Case GsConst.GS_MASTER_CMD_REGISTER
                    p = New ServerRegisterPacket(server, rIPEP)
                Case GsConst.GS_MASTER_CMD_HEARTBEAT
                    p = New ServerHeartbeatPacket(server, rIPEP)
                Case GsConst.GS_MASTER_CMD_CHALLENGE_RES
                    p = New ChallengeRequestPacket(server, rIPEP)
                Case GsConst.GS_MASTER_CMD_HANDSHAKE_ACK
                    p = New HandshakePacket(server, rIPEP)
                Case GsConst.GS_MASTER_CMD_AVAILIABLE
                    p = New GameAvailiablePacket(server, rIPEP)
                Case GsConst.GS_MASTER_CMD_MESSAGE_ACK
                    p = New ClientMessagePacket(server, rIPEP)
                Case Else 'drop unkown packet
                    Logger.Log("Dropping unknown UDP Packet #{0} from {1}", LogLevel.Verbose, data(0).ToString, rIPEP.ToString)
                    Return
            End Select
            p.Data = data
            p.ManageData()
        End Sub

        '2nd socket for firewall-check
        Private checkClient As UdpClient
        Private checkListenThread As Thread

        Public Overrides Sub Open()
            If Not Me.running Then
                MyBase.Open()
                Try
                    Me.checkClient = New UdpClient(New Net.IPEndPoint(Me.Address, Me.CheckPort))
                Catch ex As Exception
                    Logger.Log("Bind failed [{0}:{1}]", LogLevel.Exception, Me.Address.ToString, Me.CheckPort.ToString)
                End Try
                Logger.Log("Bound Firewall-Test socket [{0}:{1}]", LogLevel.Info, Me.Address.ToString, Me.CheckPort.ToString)
                Me.checkListenThread = New Thread(AddressOf Me.WaitForCheckResponse)
                Me.checkListenThread.Start()
            End If
        End Sub

        Private Sub WaitForCheckResponse()
            While running
                Try
                    Dim rIPEP As IPEndPoint = Nothing
                    Dim data() As Byte = Me.checkClient.Receive(rIPEP)

                    If data.Length > 0 Then
                        Me.OnDataInput(data, rIPEP)
                    End If
                Catch ex As Exception
                    Logger.Log(ex.ToString, LogLevel.Verbose)
                End Try
                Threading.Thread.Sleep(10)
            End While
        End Sub

        Public Sub SendCheck(ByVal p As GsUdpPacket)
            Try
                Dim buf() As Byte = p.CompileResponse
                Me.checkClient.Send(buf, buf.Length, p.RemoteIPEP)
                Logger.Log("Sending to {0}", LogLevel.Verbose, p.RemoteIPEP.ToString)
            Catch ex As Exception
                Logger.Log("Couldn't send UDP-Packet to " & p.RemoteIPEP.Address.ToString & vbCrLf & ex.ToString, LogLevel.Warning)
            End Try
        End Sub

    End Class
End Namespace
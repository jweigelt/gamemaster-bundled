'UdpServer-Wrapper for handling natneg packets
'JW "LeKeks" 07/2014

Imports gamemaster.common.Util
Imports gamemaster.common.Network
Imports gamemaster.natneg.Network.Packets

Namespace Network

    Public Class NatnegUdpServer
        Inherits UdpServer

        Private ReadOnly server As NatnegServer

        Sub New(ByVal server As NatnegServer)
            MyBase.New()
            Me.server = server
        End Sub

        Public Overrides Sub OnDataInput(data() As Byte, rIPEP As Net.IPEndPoint)

            'check the packet's itegrity
            If data(0) <> GsConst.GS_SERVICE_NATNEG_PREFIX(0) Or data(1) <> GsConst.GS_SERVICE_NATNEG_PREFIX(1) Or data.Length < 7 Then
                Logger.Log("Received broken Packet", LogLevel.Verbose)
                Return
            End If

            'get the packet's protocol version
            Dim protocol As Byte = data(6)

            'Handle the packet
            Dim packet As NatnegPacket
            Select Case data(7)
                Case GsConst.GS_NATNEG_CMD_INIT
                    packet = New NatnegInitPacket(server, rIPEP, protocol)
                Case GsConst.GS_NATNEG_CMD_CONNECT_ACK
                    packet = New NatnegConnectAckdPacket(server, rIPEP, protocol)
                Case Else
                    Logger.Log("Unkown UDP Packet #" & data(0) & " (" & rIPEP.Address.ToString & ")", LogLevel.Verbose)
                    Return
            End Select
            packet.data = data
            packet.ManageData()
            MyBase.OnDataInput(data, rIPEP)
        End Sub
    End Class
End Namespace
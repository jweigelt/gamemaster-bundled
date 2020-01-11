'JW "LeKeks" 07/2014

Imports gamemaster.common.Network

Namespace Network.Packets
    Public Class NatnegPacket
        Inherits GsUdpPacket
        Public Property ProtocolVersion As Byte = &H2
        Public ReadOnly Property Server As NatnegServer

        Sub New(ByVal server As NatnegServer, ByVal remoteIPEP As Net.IPEndPoint, ByVal protocolVersion As Byte)
            MyBase.New(remoteIPEP)
            Me.ProtocolVersion = protocolVersion    'Fetch the protocol version
            Me.bytesParsed = 8                      'Already parsed 8 Bytes: 2(prefix) + 5(header) + 1(version)
            Me.Server = server
        End Sub
    End Class
End Namespace
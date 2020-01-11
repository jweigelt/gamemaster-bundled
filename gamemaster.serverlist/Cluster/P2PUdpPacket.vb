Imports gamemaster.common.Network

Namespace Cluster
    Public Class P2PUdpPacket
        Inherits PacketBase
        Public Property Server As ServerlistServer

        Sub New(ByVal server As ServerlistServer, ByVal remoteIPEP As Net.IPEndPoint)
            Me.Server = server
            Me.RemoteIPEP = remoteIPEP
            Me.bytesParsed = 0
        End Sub
    End Class
End Namespace
Imports gamemaster.common.Network

Namespace Cluster
    Public Class NatnegClusterPacket
        Inherits PacketBase
        Public Property Server As NatnegServer

        Sub New(ByVal server As NatnegServer, ByVal remoteIPEP As Net.IPEndPoint)
            Me.Server = server
            Me.RemoteIPEP = remoteIPEP
            Me.bytesParsed = 0
        End Sub
    End Class
End Namespace
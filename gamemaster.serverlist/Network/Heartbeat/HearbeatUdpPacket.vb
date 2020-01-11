Imports System.Net
Imports gamemaster.common.Network

Namespace Network.Hearbeat
    Public Class HearbeatUdpPacket
        Inherits GsUdpPacket

        Public ReadOnly Property Server As ServerlistServer

        Public Sub New(ByVal server As ServerlistServer, ByVal remoteIPEP As IPEndPoint)
            MyBase.New(remoteIPEP)
            Me.Server = server
        End Sub
    End Class
End Namespace
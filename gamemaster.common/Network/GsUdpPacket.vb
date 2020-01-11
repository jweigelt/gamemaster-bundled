'UDP-Packet Base class
'JW "LeKeks" 04/2014
Imports System.Net

Namespace Network
    Public Class GsUdpPacket
        Inherits PacketBase

        Public Property ClientUUID As Byte() = {}

        Sub New(ByVal remoteIPEP As IPEndPoint)
            Me.RemoteIPEP = remoteIPEP
        End Sub

        Public Sub SetupIDs(Optional ByVal packet As GsUdpPacket = Nothing)
            If Not packet Is Nothing Then
                Array.Resize(Me.ClientUUID, 4)
                Array.Copy(packet.Data, 1, Me.ClientUUID, 0, Me.ClientUUID.Length)
            Else
                bytesParsed = 1
                Array.Resize(Me.ClientUUID, 4)
                Array.Copy(Me.Data, Me.bytesParsed, Me.ClientUUID, 0, Me.ClientUUID.Length)
                bytesParsed += 4
            End If
        End Sub
    End Class
End Namespace
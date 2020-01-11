Imports System.Net
Imports gamemaster.common.Util
Imports gamemaster.common.Network
Imports gamemaster.serverlist.Network.Hearbeat

Namespace Cluster
    Public Class MessageForwardPacket
        Inherits P2PUdpPacket

        Public Property FwdIPEP As IPEndPoint
        Public Property FwdPayload As Byte()

        Sub New(ByRef server As ServerlistServer, ByVal rIPEP As Net.IPEndPoint)
            MyBase.New(server, rIPEP)
        End Sub

        Public Overrides Sub ManageData()
            Me.bytesParsed = 1
            Dim peerIPEP As Net.IPEndPoint = ArrayFunctions.GetIPEndPointFromByteArray(Data, Me.bytesParsed)
            Me.bytesParsed += 6

            Dim payload() As Byte = {}
            Array.Resize(payload, Me.Data.Length - Me.bytesParsed)
            Array.Copy(Me.Data, bytesParsed, payload, 0, payload.Length)

            Dim cmp As New ClientMessagePacket(Me.Server, peerIPEP) With {
                .Payload = payload
            }
            Me.Server.GSUdpServer.Send(cmp)
        End Sub

        Public Overrides Function CompileResponse() As Byte()
            Dim buf() As Byte = {GsConst.P2P_CMD_SENDMESSAGE}
            ArrayFunctions.ConcatArray(Me.FwdIPEP.Address.GetAddressBytes, buf)
            ArrayFunctions.ConcatArray(ArrayFunctions.DumpUInt16LE(CUShort(FwdIPEP.Port)), buf)
            ArrayFunctions.ConcatArray(Me.FwdPayload, buf)
            Return buf
        End Function
    End Class
End Namespace
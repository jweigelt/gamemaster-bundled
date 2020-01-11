'packet for forwarding natneg connects
'JW "LeKeks" 08/2014
Imports gamemaster.common.Util
Imports gamemaster.natneg.Network.Packets

Namespace Cluster

    Public Class ConnectForwardPacket
        Inherits NatnegClusterPacket

        Public Property Cookie As Int32
        Public Property FwdIPEP As Net.IPEndPoint
        Public Property RemotePeer As Net.IPEndPoint
        Public Property ProtocolVersion As Byte

        Sub New(ByVal server As NatnegServer, ByVal rIPEP As Net.IPEndPoint)
            MyBase.New(server, rIPEP)
        End Sub

        Public Overrides Sub ManageData()
            Me.bytesParsed += 1
            Me.ProtocolVersion = Me.Data(bytesParsed)
            Me.bytesParsed += 1
            Me.FwdIPEP = ArrayFunctions.GetIPEndPointFromByteArray(Me.Data, Me.bytesParsed)
            Me.bytesParsed += 6
            Me.Cookie = BitConverter.ToInt32(Me.Data, Me.bytesParsed)
            Me.bytesParsed += 4
            Me.RemotePeer = ArrayFunctions.GetIPEndPointFromByteArray(Me.Data, Me.bytesParsed)

            Dim ncp As New NatnegConnectPacket(Me.Server, Me.FwdIPEP, Me.ProtocolVersion) With {
                .Cookie = Cookie,
                .Destination = Me.RemotePeer
            }
            Me.Server.GSUdpServer.send(ncp)
        End Sub

        Public Overrides Function CompileResponse() As Byte()
            Dim buf() As Byte = {GsConst.P2P_CMD_NATNEGCONNECT, Me.ProtocolVersion}
            ArrayFunctions.ConcatArray(Me.FwdIPEP.Address.GetAddressBytes, buf)
            ArrayFunctions.ConcatArray(ArrayFunctions.DumpUInt16LE(CUShort(FwdIPEP.Port)), buf)
            ArrayFunctions.ConcatArray(BitConverter.GetBytes(Me.Cookie), buf)
            ArrayFunctions.ConcatArray(Me.RemotePeer.Address.GetAddressBytes, buf)
            ArrayFunctions.ConcatArray(ArrayFunctions.DumpUInt16LE(CUShort(RemotePeer.Port)), buf)
            Return buf
        End Function
    End Class
End Namespace
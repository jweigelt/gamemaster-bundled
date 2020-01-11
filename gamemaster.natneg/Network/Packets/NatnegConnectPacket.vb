'packet to tell the natneg peer that the session is ready
'or to report any errors that occured while processing the request

Imports gamemaster.common.Util

Namespace Network.Packets
    Public Class NatnegConnectPacket
        Inherits NatnegPacket

        Public Property Failed As Boolean = False       'true if the request failed
        Public Property Destination As Net.IPEndPoint
        Public Property Cookie As Int32

        Sub New(ByVal server As NatnegServer, ByVal remoteIPEP As Net.IPEndPoint, ByVal protocolVersion As Byte)
            MyBase.New(server, remoteIPEP, protocolVersion)
        End Sub

        Public Overrides Function CompileResponse() As Byte()
            Dim buffer() As Byte = {}

            'set request state
            Dim requestState As Byte = GsConst.GS_NATNEG_CONNECTSTATE_OK
            If Me.Failed Then
                requestState = GsConst.GS_NATNEG_CONNECTSTATE_FAIL
                Me.Destination = New Net.IPEndPoint(New Net.IPAddress(0), 0)
            End If

            'Temp
            Dim t As Byte = &H42

            'Build the packet
            ArrayFunctions.ConcatArray(GsConst.GS_SERVICE_NATNEG_PREFIX, buffer)
            ArrayFunctions.ConcatArray(GsConst.GS_NATNEG_HEADER, buffer)
            ArrayFunctions.ConcatArray({Me.ProtocolVersion, GsConst.GS_NATNEG_CMD_CONNECT}, buffer)
            ArrayFunctions.ConcatArray(BitConverter.GetBytes(Me.Cookie), buffer)
            ArrayFunctions.ConcatArray(Me.Destination.Address.GetAddressBytes, buffer)
            ArrayFunctions.ConcatArray(ArrayFunctions.DumpUInt16LE(CUShort(Destination.Port)), buffer)
            ArrayFunctions.ConcatArray({t}, buffer)
            ArrayFunctions.ConcatArray({requestState}, buffer)

            Return buffer
        End Function
    End Class
End Namespace
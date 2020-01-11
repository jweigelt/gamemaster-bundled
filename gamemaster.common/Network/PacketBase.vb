Imports System.Net
Imports gamemaster.common.Util

Namespace Network
    Public Class PacketBase
        Public Property PacketId As Byte
        Public Property Data As Byte()

        Public Property RemoteIPEP As IPEndPoint
        Public bytesParsed As Int32 = 0

        Public Function FetchString(ByVal buffer() As Byte) As String
            Dim strEnd As Int32 = Array.IndexOf(buffer, CByte(0), bytesParsed)
            Dim buf(strEnd - bytesParsed - 1) As Byte
            Array.Copy(buffer, bytesParsed, buf, 0, buf.Length)
            bytesParsed = strEnd + 1
            Return ArrayFunctions.GetString(buf)
        End Function

        'Function stubs
        Public Overridable Function CompileResponse() As Byte()
            Return {}
        End Function

        Public Overridable Sub ManageData()
            Return
        End Sub
    End Class
End Namespace

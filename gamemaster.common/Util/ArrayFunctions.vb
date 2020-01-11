'Useful Array-Functions
'for splitting and building data arrays
'JW "LeKeks" 9/2014
Imports System.Text
Imports System.Net

Namespace Util
    Public Class ArrayFunctions
        Private Shared WIN1252_LATIN1 As Encoding = Encoding.GetEncoding("Windows-1252")

        Public Shared Sub ConcatArray(ByVal source() As Byte, ByRef dest() As Byte)
            Dim newSize As Integer = dest.Length + source.Length
            Dim oldSize As Integer = dest.Length
            Array.Resize(dest, newSize)
            Array.Copy(source, 0, dest, oldSize, source.Length)
        End Sub

        Public Shared Sub ConcatArray(ByVal source() As Byte, ByRef dest() As Byte, ByVal separator As Byte)
            Dim newSize As Integer = dest.Length + source.Length + 1
            Dim oldSize As Integer = dest.Length
            Array.Resize(dest, newSize)
            Array.Copy(source, 0, dest, oldSize, source.Length)
            dest(dest.Length - 1) = separator
        End Sub

        'Doesn't work on Mono -> object is missing reference to Array.Length
        'Public Sub AttachArray_a(ByRef dest() As Byte, ParamArray sources() As Object)
        'Dim newLenght As Int32 = dest.Length
        'Dim offset As Int32 = dest.Length

        'For i = 0 To sources.Length - 1
        '       newLenght += sources(i).Length
        'Next
        '   Array.Resize(dest, newLenght)

        'For i = 0 To sources.Length - 1
        '       Array.Copy(sources(i), 0, dest, offset, sources(i).Length)
        '      offset += sources(i).Length
        'Next
        'End Sub

        Public Shared Sub SliceArray(ByRef haystack() As Byte, ByRef dest() As Byte, ByVal needle() As Byte)
            Dim match As Boolean = False
            Dim begin As Integer = 1

            While Not match And (begin + needle.Length) < haystack.Length And begin > 0
                begin = Array.IndexOf(haystack, needle(0), begin + 1) ' 0 >1<

                If begin < 0 Or (begin + needle.Length) > haystack.Length Then 'No needle found -> return whole array
                    dest = haystack
                    haystack = {}
                    Return
                End If

                For i = begin To begin + needle.Length - 1
                    If haystack(i) <> needle(i Mod begin) Then
                        match = False
                        Exit For
                    Else
                        match = True
                    End If
                Next
                If match Then begin += needle.Length 'Add needle length
            End While
            'begin is at last index of needle -> - needle.lenght will skip to previous one

            If dest Is Nothing Then dest = {}
            'Assume needle at 14 (15.)
            '-> begin = 16
            '0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19
            '1 1 1 1 1 1 1 1 1 1 1  1  1  1  0  0  0  1  1  1
            '16 - 3 = 13
            '20 - 16 = 4 -> 20 -16 -1

            Array.Resize(dest, begin - needle.Length + 1) 'Copy until first one before needle starts
            Array.Copy(haystack, 0, dest, 0, dest.Length)

            'Copy from first Byte after needle to 0
            Array.Copy(haystack, begin, haystack, 0, haystack.Length - begin)
            Array.Resize(haystack, haystack.Length - begin)
        End Sub

        Public Shared Function ReadCString(ByVal arr() As Byte, ByRef offset As Integer) As String
            Dim index As Integer = Array.IndexOf(arr, Convert.ToByte(0), offset)
            If index < 0 Then Return String.Empty
            Dim buf(index - offset - 1) As Byte
            Array.Copy(arr, offset, buf, 0, buf.Length)
            offset = index + 1
            Return GetString(buf)
        End Function

        Public Shared Function GetBytes(ByVal str As String) As Byte()
            Return WIN1252_LATIN1.GetBytes(str)
        End Function

        Public Shared Function GetString(ByVal bytes() As Byte) As String
            Return WIN1252_LATIN1.GetString(bytes)
        End Function

        Public Shared Function DumpUInt16LE(ByVal value As UShort) As Byte()
            Dim buffer() As Byte = BitConverter.GetBytes(value)
            Array.Reverse(buffer)
            Return buffer
        End Function

        Public Shared Function GetUInt16LE(ByVal buffer() As Byte, ByVal offset As Integer) As UShort
            Dim buf() As Byte = {buffer(offset + 1), buffer(offset)}
            Return BitConverter.ToUInt16(buf, 0)
        End Function

        Public Shared Function GetIPEndPointFromByteArray(ByVal buffer() As Byte, ByVal offset As Integer) As IPEndPoint
            Dim queryIP(3) As Byte
            Array.Copy(buffer, offset, queryIP, 0, 4)
            Dim ipa As New IPAddress(queryIP)
            Return New IPEndPoint(ipa, CInt(GetUInt16LE(buffer, offset + 4)))
        End Function

        Public Shared Function TrimArray(ByRef data() As Byte, ByVal shift As Integer) As Byte()
            'trims an array
            Dim rr(data.Length - shift - 1) As Byte
            Array.Copy(data, shift, rr, 0, rr.Length)
            Return rr
        End Function
    End Class
End Namespace
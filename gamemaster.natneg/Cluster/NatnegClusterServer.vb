'Peer-2-Peer UDP-server/client for masterserver-interlinking
'JW "LeKeks" 08/2014
Imports System.Net
Imports System.Security.Cryptography

Imports gamemaster.common.Network
Imports gamemaster.common.Util

Namespace Cluster
    Public Class NatnegClusterServer
        Inherits UdpServer
        Public Property EncKey As Byte() = {1, 2, 3}

        Private ReadOnly server As NatnegServer

        Sub New(ByVal server As NatnegServer)
            MyBase.New()
            Me.server = server
        End Sub

        Public Overrides Sub OnDataInput(ByVal data() As Byte, ByVal rIPEP As IPEndPoint)
            If data.Length = 0 Then Return
            If Not Me.DecryptPacket(data, rIPEP) Then Return

            Dim p As NatnegClusterPacket = Nothing

            'first byte is the command-id
            Select Case data(0)
                Case GsConst.P2P_CMD_NATNEGCONNECT
                    p = New ConnectForwardPacket(server, rIPEP)
                    Logger.Log("Forwarding Message request from " & rIPEP.ToString, LogLevel.Info)
            End Select

            If Not p Is Nothing Then
                p.data = data
                p.ManageData()
            End If
        End Sub

        Public Sub SendToAll(ByVal p As PacketBase)
            Dim servers As List(Of MasterServer) = Me.server.MySQL.GetMasterServers()
            For Each ms As MasterServer In servers
                p.RemoteIPEP = ms.RIPEP
                Me.send(p)
            Next
        End Sub

        Public Overrides Sub Send(p As PacketBase)
            Me.EncryptPacket(p.data)
            MyBase.Send(p)
        End Sub

        'Unimplemented so far
        Private Function DecryptPacket(ByRef data() As Byte, ByVal rIPEP As IPEndPoint) As Boolean
            Dim ms As MasterServer = Me.server.MySQL.FetchMasterserver(rIPEP)
            If ms Is Nothing Then Return False
            Try
                'TODO: IV ändern!
                'Me.AESdec(data, data, Me.EncKey, Nothing)
                Return True
            Catch ex As Exception 'Ungültige Daten -> kein AES-256
                Return False
            End Try
        End Function

        Private Sub EncryptPacket(ByRef data() As Byte)
            'TODO: IV ändern!
            'AESenc(data, data, Me.EncKey, Nothing)
        End Sub

        Private Sub AESenc(ByVal data() As Byte, ByRef buffer() As Byte, ByVal key() As Byte, ByVal IV() As Byte)
            Dim aesAlg As Aes = Aes.Create()
            aesAlg.Key = key
            aesAlg.IV = IV
            Dim stream As New IO.MemoryStream()
            Dim cryptStream As New CryptoStream(stream, aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV), CryptoStreamMode.Write)
            cryptStream.Write(data, 0, data.Length)
            cryptStream.FlushFinalBlock()
            buffer = stream.ToArray()
            aesAlg.Clear()
        End Sub

        Private Sub AESdec(ByVal data() As Byte, ByRef buffer() As Byte, ByVal key() As Byte, ByVal IV() As Byte)
            Dim aesAlg As Aes = Aes.Create()
            aesAlg.Key = key
            aesAlg.IV = IV
            Dim stream As New IO.MemoryStream(data)
            Dim cryptStream As New CryptoStream(stream, aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV), CryptoStreamMode.Read)
            buffer = stream.ToArray()
            aesAlg.Clear()
        End Sub
    End Class
End Namespace
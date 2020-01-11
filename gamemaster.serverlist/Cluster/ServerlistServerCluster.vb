'Class for Multimasterserver-fwd.
Imports System.Net
Imports System.Security.Cryptography

Imports gamemaster.common.Util
Imports gamemaster.common.Network

Namespace Cluster
    Public Class ServerlistServerCluster
        Inherits UdpServer
        Public Property EncKey As Byte() = {1, 2, 3}

        Friend Property Server As ServerlistServer

        Sub New(ByVal server As ServerlistServer)
            Me.Server = server
        End Sub

        Public Overrides Sub OnDataInput(ByVal data() As Byte, ByVal rIPEP As IPEndPoint)
            If data.Length = 0 Then Return
            If Not Me.DecryptPacket(data, rIPEP) Then Return

            Dim p As P2PUdpPacket = Nothing

            Select Case data(0)
                Case GsConst.P2P_CMD_SENDMESSAGE
                    p = New MessageForwardPacket(Server, rIPEP)
                    Logger.Log("Forwarding Message request from {0}", LogLevel.Info, rIPEP.ToString)
            End Select

            If Not p Is Nothing Then
                p.Data = data
                p.ManageData()
            End If
        End Sub

        Public Sub SendToAll(ByVal p As PacketBase)
            Dim servers As List(Of MasterServer) = Me.Server.MySQL.GetMasterServers()
            For Each ms As MasterServer In servers
                p.RemoteIPEP = ms.RIPEP
                Me.Send(p)
            Next
        End Sub

        Public Overrides Sub Send(p As PacketBase)
            Me.EncryptPacket(p.Data)
            MyBase.Send(p)
        End Sub

        Private Function DecryptPacket(ByRef data() As Byte, ByVal rIPEP As IPEndPoint) As Boolean
            Dim ms As MasterServer = Me.Server.MySQL.FetchMasterserver(rIPEP)
            If ms Is Nothing Then Return False

            Try
                'TODO: implement Asym AES
                'Me.AESdec(data, data, Me.EncKey, Nothing)
                Return True
            Catch ex As Exception 'Ungültige Daten -> kein AES-256
                Return False
            End Try
        End Function

        Private Sub EncryptPacket(ByRef data() As Byte)
            'TODO: implement encryption
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
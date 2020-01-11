'TCP-Client management class
'JW "LeKeks" 05/2014

Imports System.Net.Sockets
Imports System.Threading

Imports gamemaster.common.Util
Imports gamemaster.common.Network

Imports gamemaster.serverlist.Cryptography
Imports gamemaster.serverlist.Network.Serverbrowsing

Namespace Network

    Public Class ServerlistClient

        Public Property Server As ServerlistServer
        Public Property RemoteIPEP As Net.IPEndPoint

        Private ReadOnly workthread As Thread
        Private client As TcpClient
        Private stream As NetworkStream
        Private running As Boolean
        Private initOk As Boolean

        Private slist As SapphireII.SBServerList

        Public Property CryptVersion As Byte
        Public Property ProtocolVersion As Byte
        Public Property ServerKey As Byte() = {}
        Public Property ChallengeKey As Byte() = {}
        Public Property GameName As String = String.Empty
        Public Property QueryName As String = String.Empty

        Public Event ConnectionLost(ByVal sender As ServerlistClient)

        Sub New(ByVal server As ServerlistServer, client As TcpClient)
            Me.Server = server
            Me.running = True
            Me.stream = client.GetStream()
            Me.client = client
            Me.RemoteIPEP = DirectCast(client.Client.RemoteEndPoint, Net.IPEndPoint)
            Me.workthread = New Threading.Thread(AddressOf Me.Listen)
            Me.workthread.Start()
        End Sub

        Private Sub Listen()
            Try

                stream.ReadTimeout = GsConst.TCP_CLIENT_TIMEOUT
                Dim buffer() As Byte = Nothing

                While Me.ReadTcpStream(Me.stream, buffer) And Me.running

                    If Not Me.HandlePacket(buffer) Then
                        Exit Try
                    End If

                    Threading.Thread.Sleep(GsConst.TCP_CLIENT_PSH_SLEEP)
                End While

                If Me.running Then
                    Logger.Log("{0}: Reached end of stream - FIN", LogLevel.Verbose, Me.RemoteIPEP.ToString)
                End If

            Catch ex As Exception

                If ex.InnerException Is Nothing Then
                    Logger.Log("{0}: FIN: caused an Exception:" & vbCrLf & ex.ToString, LogLevel.Info, Me.RemoteIPEP.ToString())
                ElseIf ex.InnerException.GetType().IsEquivalentTo(GetType(SocketException)) Then
                    Dim se As SocketException = DirectCast(ex.InnerException, SocketException)

                    Select Case se.ErrorCode
                        Case SocketError.TimedOut
                            Logger.Log("{0}: read timed out after {1} seconds", LogLevel.Verbose, Me.RemoteIPEP.ToString, (stream.ReadTimeout / 1000).ToString())
                        Case SocketError.ConnectionAborted
                            Logger.Log("{0}: sent FIN -> closing connection", LogLevel.Verbose, Me.RemoteIPEP.ToString)
                        Case Else
                            Logger.Log("{0}: connection jammed - closing", LogLevel.Verbose, Me.RemoteIPEP.ToString)
                    End Select
                Else
                    Logger.Log("{0}: FIN: caused an Exception:" & vbCrLf & ex.ToString, LogLevel.Info, Me.RemoteIPEP.ToString())
                End If
            End Try

            Me.Dispose()
        End Sub

        Public Sub Dispose()
            Me.running = False
            Try
                Me.stream.Close()
                Me.client.Close()
            Catch
                'TODO
            End Try
            Me.stream = Nothing
            Me.client = Nothing
            Me.Server = Nothing
            Me.slist = Nothing
            RaiseEvent ConnectionLost(Me)
        End Sub

        Private Function ReadTcpStream(ByVal stream As Net.Sockets.NetworkStream, ByRef buffer() As Byte) As Boolean
            buffer = New Byte(1) {}

            Dim bufferLen As Int32 = 0     'Bytes expected
            Dim bufferRead As Int32 = 0    'Bytes read

            'Read length header
            bufferRead = stream.Read(buffer, 0, 2)
            If (bufferRead = 0) Then Return False
            bufferLen = ArrayFunctions.GetUInt16LE(buffer, 0) - 2
            If bufferLen < 0 Then Return False

            'Read packet body
            Array.Resize(buffer, bufferLen)
            bufferRead = stream.Read(buffer, 0, bufferLen)
            If (bufferRead = 0) Then Return False

            'Limit the number of pushs to prevent DoS
            Dim readCount As Int32 = 0

            'Check for fragmented data
            While (bufferRead < bufferLen And readCount < GsConst.TCP_CLIENT_PSH_MAXCOUNT)
                Logger.Log("{0}:  waiting for next PSH", LogLevel.Verbose, Me.RemoteIPEP.ToString())

                'Wait for the next push
                bufferLen -= bufferRead 'remaining data len
                bufferRead = stream.Read(buffer, bufferRead, bufferLen)
                readCount += 1
                Thread.Sleep(GsConst.TCP_CLIENT_PSH_SLEEP)
            End While

            'Check if the client exceeded max. push packets
            If (readCount = GsConst.TCP_CLIENT_PSH_MAXCOUNT) Then
                Logger.Log("{0}: too much PSHs - dropping client", LogLevel.Verbose, Me.RemoteIPEP.ToString())
                Return False
            End If

            Logger.Log("{0}: fetched {1} bytes from stream", LogLevel.Verbose, Me.RemoteIPEP.ToString(), bufferLen.ToString())
            Return True
        End Function

        Private Function HandlePacket(ByVal buffer() As Byte) As Boolean
            Dim packetID As Byte = buffer(0)
            Dim packet As ServerbrowsingPacket = Nothing

            'client init
            If packetID = GsConst.GS_MS_CLIENT_CMD_LIST_REQ And Not initOk Then
                If Not Me.InitializeClient(buffer) Then Return False
            End If

            Select Case packetID
                Case GsConst.GS_MS_CLIENT_CMD_LIST_REQ
                    packet = New ListRequestPacket(Me, buffer)
                    Logger.Log("Fetched packet #{0} (server list request)", LogLevel.Verbose, packetID.ToString)

                Case GsConst.GS_MS_CLIENT_CMD_SERVERINFO
                    packet = New ServerinfoRequestPacket(Me, buffer)
                    Logger.Log("Fetched packet #{0} (server info request)", LogLevel.Verbose, packetID.ToString)

                Case GsConst.GS_MS_CLIENT_CMD_MESSAGE
                    packet = New MessagePacket(Me, buffer)
                    Logger.Log("Fetched packet #{0} (message forward request)", LogLevel.Verbose, packetID.ToString)

                Case GsConst.GS_MS_CLIENT_CMD_KEEPALIVE
                    Logger.Log("Fetched packet #{0} (keepalive)", LogLevel.Verbose, packetID.ToString)

                Case GsConst.GS_MS_CLIENT_CMD_MAPLOOP
                    Logger.Log("Fetched packet #{0} (maploop request)", LogLevel.Verbose, packetID.ToString)

                Case GsConst.GS_MS_CLIENT_CMD_PLAYERSEARCH
                    Logger.Log("Fetched packet #{0} (playersearch request)", LogLevel.Verbose, packetID.ToString)

                Case Else
                    Logger.Log("Dropping unknown TCP packet (#{0})", LogLevel.Verbose, packetID.ToString)
            End Select

            If Not packet Is Nothing Then
                packet.PacketId = packetID
                packet.ManageData()
            End If

            Return True
        End Function

        Public Sub Send(ByVal packet As ServerbrowsingPacket)
            Dim data() As Byte = packet.CompileResponse()

            If packet.UseCipher Then
                'Encrypt the packet if encryption is set up
                If slist.cryptHeaderOK = True Or (Me.ChallengeKey.Length > 0 And Me.ServerKey.Length > 0) Then
                    SapphireII.GOAEncryptWrapper(Me.slist, data, Me.ServerKey, Me.ChallengeKey)
                Else
                    Logger.Log("{0}: cipher required but no valid header sent", LogLevel.Info, Me.RemoteIPEP.ToString())
                    Return
                End If
            End If

            Me.stream.Write(data, 0, data.Length)
            Me.stream.Flush()
        End Sub

        Private Function InitializeClient(ByRef buffer() As Byte) As Boolean
            Dim offset As Integer = 0

            'fetch protocol info
            Me.ProtocolVersion = buffer(1)
            Me.CryptVersion = buffer(2)

            offset += 3 + 4 'skip 4-byte flag

            'fetch gamenames
            Me.GameName = ArrayFunctions.ReadCString(buffer, offset)
            Me.QueryName = ArrayFunctions.ReadCString(buffer, offset)

            'check for valid challenge
            If buffer.Length - offset < 8 Then
                Logger.Log("{0}: Invalid crypt header - dropping client", LogLevel.Verbose, Me.RemoteIPEP.ToString)
                Return False
            End If

            'copy challenge key
            Array.Resize(Me.ChallengeKey, GsConst.GS_CRYPT_CHALLENGELEN)
            Array.Copy(buffer, offset, Me.ChallengeKey, 0, GsConst.GS_CRYPT_CHALLENGELEN)

            'fetch server's cryptkey
            Me.ServerKey = ArrayFunctions.GetBytes(Me.Server.MySQL.FetchServerKey(Me.GameName))
            offset += GsConst.GS_CRYPT_CHALLENGELEN

            If ServerKey.Length = 0 Then
                Logger.Log("{0}: unknown game '{1}' - dropping client", LogLevel.Verbose, Me.RemoteIPEP.ToString, Me.GameName)
                Return False
            End If

            'remove the init-header to continue normal data processing
            buffer = ArrayFunctions.TrimArray(buffer, offset - 1)

            Me.initOk = True
            Return True
        End Function

    End Class
End Namespace
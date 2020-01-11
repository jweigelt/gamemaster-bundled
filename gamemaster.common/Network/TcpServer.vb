'TCP-server class for MS-services
'JW "LeKeks" 05/2014
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

Imports gamemaster.common.Util

Namespace Network

    Public Class TcpServer
        Public Property Address As IPAddress
        Public Property Port As Integer = 28910

        Private listenThread As Thread
        Private listener As TcpListener
        Private running As Boolean

        Public Event ClientConnected(ByVal sender As TcpServer, ByVal client As TcpClient)
        Public Event BindFailed(ByVal sender As TcpServer, ByVal ex As Exception)

        Public Sub Open()
            If Not running Then
                Try
                    listener = New TcpListener(New Net.IPEndPoint(Me.Address, Me.Port))
                    listener.Start()
                Catch ex As Exception
                    Logger.Log("Bind failed [" & Me.Address.ToString & ":" & Me.Port & "]", LogLevel.Exception)
                    RaiseEvent BindFailed(Me, ex)
                End Try
                listenThread = New Thread(AddressOf Me.Listen)
                listenThread.Start()
                running = True
                Logger.Log("TCP Listener started [" & Me.Address.ToString & ":" & Me.Port & "]", LogLevel.Info)
            End If
        End Sub

        Public Sub Close()
            If running Then
                running = False
                Me.listener.Stop()
            End If
        End Sub

        Private Sub Listen()
            While running
                Try
                    If listener.Pending = True Then
                        Dim client As TcpClient = listener.AcceptTcpClient()
                        Me.OnClientConnect(client)
                    End If
                Catch ex As Exception
                    Logger.Log("Listener Error! " & vbCrLf & ex.ToString, LogLevel.Warning)
                End Try
                Threading.Thread.Sleep(10)
            End While
        End Sub

        Friend Overridable Sub OnClientConnect(ByVal client As TcpClient)
            Logger.Log("Client connected: {0}", LogLevel.Verbose, client.Client.RemoteEndPoint.ToString)
            RaiseEvent ClientConnected(Me, client)
        End Sub

    End Class
End Namespace
Imports gamemaster.common.Util
Imports gamemaster.common.Config
Imports gamemaster.common.Network
Imports gamemaster.keyauth.Config

Public Class KeyauthServer
    Public Property GSUdpServer As UdpServer
    'Public Property MySQL As MySQLHandler

    Public Property Config As KeyauthConfig
    Private ConfigReader As ConfigSerializer

#Region "Programm"
    Public Sub Run()
        Me.PreInit()
        Me.Execute()
        Me.PostInit()
    End Sub

    Private Sub PreInit()
        Me.ConfigReader = New ConfigSerializer(GetType(KeyauthConfig))
        Me.Config = DirectCast(Me.ConfigReader.LoadFromFile("/gamemaster.keyauth.xml", Environment.CurrentDirectory & GsConst.CFG_DIR), KeyauthConfig)

        Me.GSUdpServer = New UdpServer With {
            .Address = Net.IPAddress.Parse(Me.Config.UDPHeartbeatAddress),
            .Port = Me.Config.UDPHeartbeatPort
        }

        Logger.MinLogLevel = Me.Config.Loglevel
        Logger.LogToFile = Me.Config.LogToFile
        Logger.LogFileName = Me.Config.LogFileName
    End Sub

    Private Sub Execute()
        Me.GSUdpServer.Open()

        Logger.Log("Launch OK. Server is up.", LogLevel.Info)
        Logger.Log("Press [return] to exit", LogLevel.Info)
        Console.ReadLine()
        Logger.Log("Shutting down...", LogLevel.Info)
    End Sub
    Private Sub PostInit()
        Me.GSUdpServer.Close()
        'Me.MySQL.Close()
        Me.ConfigReader = Nothing
        Me.Config = Nothing
        Logger.Log("Server stopped.", LogLevel.Info)
    End Sub
#End Region

#Region "key handling"
    Public Function CheckKey(ByVal key As String, ByVal ip As String) As Boolean
        Return True
    End Function
#End Region

End Class
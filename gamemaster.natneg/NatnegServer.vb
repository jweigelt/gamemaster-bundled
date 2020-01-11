
Imports gamemaster.common.Util
Imports gamemaster.common.Config
Imports gamemaster.natneg.Config
Imports gamemaster.natneg.Cluster
Imports gamemaster.natneg.Database
Imports gamemaster.natneg.Network

Public Class NatnegServer
    Public Property GSUdpServer As NatnegUdpServer
    Public Property MySQL As NatnegMySQLHandler

    Public Property MSP2PHandler As NatnegClusterServer
    Public Property Config As NatnegConfig

    Private ConfigReader As ConfigSerializer
    Private DBCleaner As DatabaseCleaner

#Region "Programm"
    Public Sub Run()
        Me.PreInit()
        Me.Execute()
        Me.PostInit()
    End Sub
    Private Sub PreInit()
        Me.ConfigReader = New ConfigSerializer(GetType(NatnegConfig))
        Me.Config = DirectCast(Me.ConfigReader.LoadFromFile("/gamemaster.natneg.xml", Environment.CurrentDirectory & GsConst.CFG_DIR), NatnegConfig)

        Me.GSUdpServer = New NatnegUdpServer(Me) With {
            .Address = Net.IPAddress.Parse(Me.Config.UDPHeartbeatAddress),
            .Port = Me.Config.UDPHeartbeatPort
        }

        Me.MySQL = New NatnegMySQLHandler With {
            .Hostname = Me.Config.MySQLHostname,
            .Port = Me.Config.MySQLPort,
            .DbName = Me.Config.MySQLDatabase,
            .DbUser = Me.Config.MySQLUsername,
            .DbPwd = Me.Config.MySQLPwd,
            .MasterServerID = Me.Config.ServerID
        }

        Me.DBCleaner = New DatabaseCleaner With {
            .CleanupInterval = Me.Config.CleanupInterval,
            .CleanupTimeout = Me.Config.CleanupTimeout,
            .MySQL = Me.MySQL
        }

        If Me.Config.P2PEnable Then
            Me.MSP2PHandler = New NatnegClusterServer(Me) With {
                .Address = Net.IPAddress.Parse(Me.Config.P2PAddress),
                .Port = Me.Config.P2PPort,
                .EncKey = System.Text.Encoding.ASCII.GetBytes(Me.Config.P2PKey)
            }
        End If

        Logger.MinLogLevel = Me.Config.Loglevel
        Logger.LogToFile = Me.Config.LogToFile
        Logger.LogFileName = Me.Config.LogFileName
    End Sub

    Private Sub Execute()
        Me.MySQL.Connect()
        Me.DBCleaner.init()
        Me.GSUdpServer.Open()
        If Me.Config.P2PEnable Then Me.MSP2PHandler.Open()
        Logger.Log("Launch OK. Server is up.", LogLevel.Info)
        Logger.Log("Press [Return] to exit", LogLevel.Info)

        'Debug-code for injecting test packets
        'Dim cfp As New ConnectForwardPacket(Me.MSP2PHandler, New Net.IPEndPoint(Net.IPAddress.Parse("127.0.0.1"), 1234))
        'cfp.Cookie = 121212
        'cfp.FwdIPEP = New Net.IPEndPoint(Net.IPAddress.Parse("1.2.3.4"), 1234)
        'cfp.RemotePeer = New Net.IPEndPoint(Net.IPAddress.Parse("2.2.3.4"), 1234)
        'cfp.ProtocolVersion = 2
        'cfp.data = cfp.CompileResponse
        'cfp.ManageData()
        'protocol1 debug:
        ' Do
        ' Console.ReadLine()
        ' Me.GSUdpServer.InjectPacket({&HFD, &HFC, &H1E, &H66, &H6A, &HB2, &H1, &H0, &H93, &H89, &H37, &H86, &H1, &H0, &H1}, New Net.IPEndPoint(Net.IPAddress.Parse("192.168.178.45"), 1234))
        ' Loop

        Console.ReadLine()
        Logger.Log("Shutting down...", LogLevel.Info)
    End Sub

    Private Sub PostInit()
        Me.GSUdpServer.Close()
        If Me.Config.P2PEnable Then Me.MSP2PHandler.Close()
        Me.DBCleaner.Close()
        Me.MySQL.Close()
        Logger.Log("Server stopped.", LogLevel.Info)
    End Sub
#End Region

End Class
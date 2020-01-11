'ST MySQL-Wrapper for .NET-connector
'JW "LeKeks" 04/2014

Imports MySql.Data.MySqlClient
Imports gamemaster.common.Util
Imports System.Net

Namespace Database
    Public Class MySQLHandler
        Public Property Hostname As String
        Public Property Port As Int32
        Public Property DbName As String
        Public Property DbUser As String
        Public Property DbPwd As String

        Public connection As MySqlConnection

        Public Function Connect() As Boolean
            connection = New MySqlConnection
            Dim connectionString As String = String.Empty
            connectionString = "server=" &
                            Me.Hostname & ";port=" &
                            Me.Port.ToString & ";uid = " &
                            Me.DbUser & ";pwd=" &
                            Me.DbPwd & ";database=" &
                            Me.DbName & ";"

            connection.ConnectionString = connectionString

            Logger.Log("Checking MySQL connection...", LogLevel.Info)

            Try
                connection.Open()
                connection.Close()
                Logger.Log("MySQL OK..", LogLevel.Info)
                Return True
            Catch ex As Exception

                Logger.Log("Unable to connect to the MySQL server!", LogLevel.Exception)
            End Try
            Return False
        End Function

        Public Function DoQuery(ByVal sql As String) As MySqlDataReader
            Dim query As MySqlCommand = Nothing
            Dim reader As MySqlDataReader = Nothing
            SyncLock Me.connection
                Try
                    Logger.Log("Query: " & sql, LogLevel.Verbose)
                    If Not Me.connection.State = Data.ConnectionState.Open Then Me.connection.Open()
                    query = New MySqlCommand(sql) With {
                        .Connection = Me.connection
                    }
                    query.Prepare()

                    reader = query.ExecuteReader()

                    Return reader
                Catch ex As Exception
                    If Not reader Is Nothing Then
                        If reader.IsClosed = False Then reader.Close()
                    End If
                    Logger.Log("Failed to execute query " & sql & vbCrLf & ex.ToString, LogLevel.Warning)
                End Try
                Return Nothing
            End SyncLock

        End Function

        Public Function NonQuery(ByVal sql As String) As Boolean
            Dim query As MySqlCommand = Nothing
            SyncLock Me.connection
                Try
                    Logger.Log("Query: " & sql, LogLevel.Verbose)
                    If Not Me.connection.State = Data.ConnectionState.Open Then Me.connection.Open()

                    query = New MySqlCommand(sql) With {
                        .Connection = Me.connection
                    }
                    query.Prepare()
                    query.ExecuteNonQuery()
                Catch ex As Exception
                    Logger.Log("Failed to execute query " & sql & vbCrLf & ex.ToString, LogLevel.Warning)
                    Return False
                End Try
            End SyncLock
            Return True
        End Function

        Public Function EscapeString(ByVal sql As String) As String 'remove the nasty stuff
            Return MySqlHelper.EscapeString(sql)
        End Function

        Public Sub Close()
            If Not Me.connection.State = Data.ConnectionState.Open Then
                Me.connection.Close()
            End If
            connection = Nothing
        End Sub

        Public Function GetUnixTimestamp(ByVal time As DateTime) As Long
            Return CLng((DateTime.UtcNow - New DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds)
        End Function

        Public Function GetDateTime(ByVal timestamp As Int64) As DateTime
            Dim dt As New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
            dt.AddSeconds(timestamp).ToLocalTime()
            Return dt
        End Function

        Public Function CheckForRows(ByVal res As MySqlDataReader) As Boolean
            If Not res Is Nothing Then
                res.Read()
                If res.HasRows Then
                    res.Close()
                    Return True
                End If
                res.Close()
            End If
            Return False
        End Function


        Public Function FetchMasterserver(ByVal rIPEP As Net.IPEndPoint) As MasterServer
            Dim sql As String =
                "select `id`, `server_name` from `masterserver` " &
                "where `server_address` = '" & rIPEP.Address.ToString & "' and " &
                "`server_port` = " & rIPEP.Port.ToString
            SyncLock Me.connection
                Using res As MySqlDataReader = Me.DoQuery(sql)
                    res.Read()
                    If res.HasRows Then
                        Dim ms As New MasterServer With {
                            .Id = res.GetInt32("id"),
                            .MsName = res.GetString("server_name"),
                            .RIPEP = rIPEP
                        }
                        res.Close()
                        Return ms
                    Else
                        res.Close()
                        Return Nothing
                    End If
                End Using
            End SyncLock
        End Function

        Public Function GetManagingMasterserver(ByVal rIPEP As Net.IPEndPoint) As MasterServer
            SyncLock Me.connection
                Dim sql As String =
                "select `masterserver`, `server_name`, `server_address`, `server_port` " &
                "from `gameserver`, `masterserver` where " &
                "`address` = '" & rIPEP.Address.ToString & "'" & " and " &
                "`port` = " & rIPEP.Port.ToString & " and " &
                "`masterserver` = `masterserver`.`id`"

                Using res As MySqlDataReader = Me.DoQuery(sql)
                    res.Read()

                    If Not res.HasRows Then
                        res.Close()
                        Return Nothing
                    End If

                    Dim ms As New MasterServer With {
                        .Id = res.GetInt32("masterserver"),
                        .MsName = res.GetString("server_name")
                    }

                    Dim ipa As IPAddress = IPAddress.Parse(res("server_address").ToString())
                    ms.RIPEP = New IPEndPoint(ipa, Integer.Parse(res("server_port").ToString()))
                    res.Close()
                    Return ms
                End Using
            End SyncLock
        End Function

        Public Function GetMasterServers() As List(Of MasterServer)
            Dim sql As String =
            "select * from `masterserver`"
            Dim servers As New List(Of MasterServer)
            SyncLock Me.connection
                Using res As MySqlDataReader = Me.DoQuery(sql)
                    If Not res Is Nothing Then
                        While res.Read
                            Dim ms As New MasterServer With {
                                .Id = res.GetInt32("id"),
                                .MsName = res.GetString("server_name")
                            }
                            Dim ipa As IPAddress = IPAddress.Parse(res("server_nataddress").ToString())
                            ms.RIPEP = New IPEndPoint(ipa, Integer.Parse(res("server_natport").ToString()))
                            servers.Add(ms)
                        End While
                        res.Close()
                    End If
                End Using
            End SyncLock
            Return servers
        End Function

    End Class
End Namespace
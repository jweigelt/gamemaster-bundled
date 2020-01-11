'MySQLHandler-Wrapper for Masterserver SQL-actions
'JW "LeKeks" 07/2014
Imports System.Net
Imports MySql.Data.MySqlClient
Imports gamemaster.common.Database
Imports gamemaster.common.Util
Imports gamemaster.serverlist.Gameserver

Namespace Database

    Public Class ServerlistMysqlHandler
        Inherits MySQLHandler

        Public Property MasterServerID As Integer

        Private Function BuildServer(ByVal reader As MySqlDataReader) As GsGameServer
            Dim server As New GsGameServer
            With server
                .InternalId = reader.GetInt32("id")
                .PublicIP = reader.GetString("address")
                .PublicPort = reader.GetString("port")
                .HostPort = reader.GetString("hostport")
                .ServerProtocol = reader.GetString("protocol")
                .GameName = reader.GetString("type")
                .Hostname = reader.GetString("gq_hostname")
                .GameType = reader.GetString("gq_gametype")
                .MapName = reader.GetString("gq_mapname")
                .MaxPlayers = reader.GetString("gq_maxplayers")
                .NumPlayers = reader.GetString("gq_numplayers")
                .Password = reader.GetString("gq_password")
                .GameVer = reader.GetString("gamever")
                .Session = reader.GetString("session")
                .PrevSession = reader.GetString("prevsession")
                .ServerType = reader.GetString("servertype")
                .GameMode = reader.GetString("gamemode")
                .ClientID = reader.GetInt32("clientid")
                'If Not IsDBNull(reader("netregion")) Then .SwbRegion = reader("netregion")
                .LastHeartbeat = Me.GetDateTime(reader.GetInt64("lastseen"))
                .ChallengeOK = (reader("challengeok").Equals(1) Or Not reader("dynamic").Equals(1))
                .HandshakeOK = (reader("handshakeok").Equals(1) Or Not reader("dynamic").Equals(1))
                If Not Convert.IsDBNull(reader("custom")) Then .DynamicStorage.ParseParameterString(reader.GetString("custom"))
            End With
            Return server
        End Function
        Public Sub SetHeartBeat(ByVal ipep As Net.IPEndPoint, Optional ByVal challengeOK As Boolean = False, Optional ByVal handshakeOK As Boolean = False)
            Dim sql As String = "update  `" & GsConst.MYSQL_GAMESERVER_TABLE_NAME & "` set `lastseen` = UNIX_TIMESTAMP()"

            If challengeOK Then sql &= ", `challengeok` = 1"
            If handshakeOK Then sql &= ", `handshakeok` = 1"

            sql &= " where " &
          "`address` = '" & ipep.Address.ToString & "'" & " and " &
          "`port` = " & ipep.Port

            Me.NonQuery(sql)
        End Sub
        Public Function ServerActive(ByVal ipep As Net.IPEndPoint, ByVal timeout As Int32) As Boolean
            SyncLock Me.connection
                Dim sql As String = "select * from  `" & GsConst.MYSQL_GAMESERVER_TABLE_NAME & "` where " &
                               " (UNIX_TIMESTAMP() - `lastseen`) < " & timeout.ToString & " and " &
                               "`address` = '" & ipep.Address.ToString & "'" & " and " &
                               "`port` = " & ipep.Port.ToString

                Using res As MySqlDataReader = Me.DoQuery(sql)
                    Return CheckForRows(res)
                End Using
            End SyncLock
        End Function
        Public Function FetchServerKey(ByVal gamename As String) As String
            gamename = Me.CorrectGameType(gamename)
            SyncLock Me.connection
                Dim sql As String = "select `key_key` from  `" & GsConst.MYSQL_SERVERKEY_TABLE_NAME & "` where " &
                                "`key_gamename` = '" & EscapeString(gamename) & "'"

                Using res As MySqlDataReader = Me.DoQuery(sql)
                    If Not res Is Nothing Then
                        res.Read()
                        If res.HasRows Then
                            Dim key As String = res.GetString("key_key")
                            res.Close()
                            Return key
                        End If
                        res.Close()
                    End If
                    Return String.Empty
                End Using
            End SyncLock
        End Function
        Public Sub RegisterServer(ByVal server As GsGameServer)
            Dim type As String = EscapeString(server.GameName)
            type = Me.CorrectGameType(type)

            Dim sql As String = "insert into `" & GsConst.MYSQL_GAMESERVER_TABLE_NAME & "` set " &
             "`address` = '" & server.PublicIP & "'" & ", " &
             "`port` = " & server.PublicPort & ", " &
             "`hostport` = " & server.HostPort & ", " &
             "`protocol` = '" & server.ServerProtocol & "', " &
             "`type` = '" & type & "', " &
             "`gq_hostname` = '" & EscapeString(server.Hostname) & "', " &
             "`gq_gametype` = '" & EscapeString(server.GameType) & "', " &
             "`gq_mapname` = '" & EscapeString(server.MapName) & "', " &
             "`gq_maxplayers` = " & EscapeString(server.MaxPlayers) & ", " &
             "`gq_numplayers` = " & EscapeString(server.NumPlayers) & ", " &
             "`gq_password` = " & EscapeString(server.Password) & ", " &
             "`gamever` = '" & EscapeString(server.GameVer) & "', " &
             "`lastseen` = " & GetUnixTimestamp(server.LastHeartbeat) & ", " &
             "`session` = '" & EscapeString(server.Session) & "', " &
             "`prevsession` = '" & EscapeString(server.PrevSession) & "', " &
             "`servertype` = '" & EscapeString(server.ServerType) & "', " &
             "`gamemode` = '" & EscapeString(server.GameMode) & "', " &
             "`clientid` = " & server.ClientID.ToString & ", " &
             "`masterserver` = " & Me.MasterServerID.ToString & ", " &
             "`custom` = '" & EscapeString(server.DynamicStorage.ToParameterString()) & "', " &
             "`dynamic` = 1"

            Me.NonQuery(sql)
            Me.InsertServerPlayers(server)
        End Sub
        Public Sub UpdateServer(ByVal server As GsGameServer)
            Dim sql As String = "update `" & GsConst.MYSQL_GAMESERVER_TABLE_NAME & "` set " &
          "`gq_hostname` = '" & EscapeString(server.Hostname) & "', " &
          "`gq_gametype` = '" & EscapeString(server.GameType) & "', " &
          "`gq_mapname` = '" & EscapeString(server.MapName) & "', " &
          "`gq_maxplayers` = " & EscapeString(server.MaxPlayers) & ", " &
          "`gq_numplayers` = " & EscapeString(server.NumPlayers) & ", " &
          "`gq_password` = " & EscapeString(server.Password) & ", " &
          "`gamever` = '" & EscapeString(server.GameVer) & "', " &
          "`type` = '" & EscapeString(server.GameName) & "', " &
          "`session` = '" & EscapeString(server.Session) & "', " &
          "`prevsession` = '" & EscapeString(server.PrevSession) & "', " &
          "`servertype` = '" & EscapeString(server.ServerType) & "', " &
          "`gamemode` = '" & EscapeString(server.GameMode) & "', " &
          "`dynamic` = 1, " &
          "`lastseen` = " & GetUnixTimestamp(server.LastHeartbeat) & ", " &
          "`clientid` = " & server.ClientID.ToString & ", " &
          "`masterserver` = " & Me.MasterServerID.ToString & ", " &
          "`custom` = '" & EscapeString(server.DynamicStorage.ToParameterString()) & "'" &
          " where " &
          "`address` = '" & server.PublicIP & "'" & " and " &
          "`port` = " & server.PublicPort
            Me.NonQuery(sql)
            Me.InsertServerPlayers(server)
        End Sub
        Public Function GetServers(ByVal gamename As String, ByVal timeout As Int32, Optional ByVal filter As String = "") As List(Of GsGameServer)
            'filter = " and gamever = '1.0' "
            gamename = Me.CorrectGameType(gamename)
            SyncLock Me.connection
                Dim sql As String = "select * from  `" & GsConst.MYSQL_GAMESERVER_TABLE_NAME & "` where " &
                                "`type` = '" & EscapeString(gamename) & "' and (UNIX_TIMESTAMP() - `lastseen`) < " & timeout.ToString & filter & " and `gamemode` != 'exiting' order by `gq_numplayers` desc"

                Using res As MySqlDataReader = Me.DoQuery(sql)
                    Dim servers As New List(Of GsGameServer)
                    If Not res Is Nothing Then
                        While res.Read
                            servers.Add(Me.BuildServer(res))
                        End While
                        res.Close()
                    End If

                    Return servers
                End Using
            End SyncLock
        End Function

        Public Sub InsertServerPlayers(ByVal server As GsGameServer)
            Dim players As List(Of GsPlayer) = server.GetPlayers()
            server.InternalId = Me.FetchServerID(server)

            'No players or no server -> exit
            If server.InternalId = -1 Or players Is Nothing Then Return

            For Each p As GsPlayer In players
                Dim sql As String =
                "select `id` from `players` " &
                " where `sid` = " & server.InternalId.ToString() &
                " and `gq_name` = '" & CorrectPlayerName(EscapeString(p.player_)) & "'" &
                " and (UNIX_TIMESTAMP() - `lastseen`) > " & GsConst.MYSQL_PLAYERROW_DELAY.ToString() &
                " limit 1"

                Dim id As Int32 = -1
                Using res As MySqlDataReader = Me.DoQuery(sql)
                    res.Read()
                    If res.HasRows Then id = res.GetInt32("id")
                    res.Close()
                End Using

                If id <> -1 Then
                    sql = "update `players` set " &
                    "`gq_kills` = " & p.kills_.ToString() & ", " &
                    "`gq_deaths` = " & p.deaths_.ToString() & ", " &
                    "`gq_score` = " & p.score_.ToString() & ", " &
                    "`gq_ping` = " & p.ping_.ToString() & ", " &
                    "`gq_team` = " & p.team_ & ", " &
                    "`lastseen` = UNIX_TIMESTAMP()" &
                    " where id=" & id.ToString()
                Else
                    'Dim tst As Int32 = Me.GetUnixTimestamp(Now)

                    sql = "insert into `players` (`sid`, `gq_name`, `gq_kills`, `gq_deaths`, `gq_score`, `gq_ping`, `gq_team`, `lastseen`) values "
                    sql &= String.Format("({0},'{1}',{2},{3},{4},{5},{6},UNIX_TIMESTAMP())",
                                        server.InternalId.ToString(),
                                        CorrectPlayerName(EscapeString(p.player_)),
                                        p.kills_.ToString(),
                                        p.deaths_.ToString(),
                                        p.score_.ToString(),
                                        p.ping_.ToString(),
                                        p.team_.ToString())
                End If
                Me.NonQuery(sql)
            Next
        End Sub

        Public Sub ResetChallenge(ByVal server As Net.IPEndPoint)
            Dim sql As String = "update `gameserver` set  `challengeok` = 0, `handshakeok` = 0 where " &
                              "`address` = '" & server.Address.ToString & "'" & " and " &
                              "`port` = " & server.Port.ToString

            Me.NonQuery(sql)
        End Sub
        Public Function ServerExists(ByVal server As Net.IPEndPoint) As Boolean
            SyncLock Me.connection
                Dim sql As String = "select `id` from  `gameserver` where " &
                                "`address` = '" & server.Address.ToString & "'" & " and " &
                                "`port` = " & server.Port.ToString
                Using res As MySqlDataReader = Me.DoQuery(sql)
                    Return CheckForRows(res)
                End Using
            End SyncLock
        End Function

        Public Function FetchServerID(ByVal server As GsGameServer) As Int32
            SyncLock Me.connection
                Dim sql As String =
          "select `id` from `gameserver` where " &
          "`address` = '" & server.PublicIP & "'" & " and " &
          "`port` = " & server.PublicPort

                Using res As MySqlDataReader = Me.DoQuery(sql)
                    Dim val As Object = FetchValue(res, "id")
                    If val Is Nothing Then Return -1
                    Return DirectCast(val, Int32)
                End Using
            End SyncLock
        End Function

        Public Function FetchClientID(ByVal rIPEP As Net.IPEndPoint) As Int32
            SyncLock Me.connection
                Dim sql As String =
          "select `clientid` from `gameserver` where " &
          "`address` = '" & rIPEP.Address.ToString & "'" & " and " &
          "`port` = " & rIPEP.Port.ToString
                Using res As MySqlDataReader = Me.DoQuery(sql)
                    Return DirectCast(FetchValue(res, "clientid"), Int32)
                End Using
            End SyncLock
        End Function

        Private Function FetchValue(ByVal reader As MySqlDataReader, ByVal fieldName As String) As Object
            reader.Read()
            If Not reader.HasRows Then
                reader.Close()
                Return Nothing
            End If
            Dim clientId As Object = reader(fieldName)
            reader.Close()
            Return clientId
        End Function

        Public Function FetchServerByIPEP(ByVal rIPEP As Net.IPEndPoint) As GsGameServer
            Dim sql As String =
            "select * from `gameserver` where `address` = '" & rIPEP.Address.ToString() & "' and " &
            "`port` = " & rIPEP.Port.ToString()

            SyncLock Me.connection
                Using res As MySqlDataReader = Me.DoQuery(sql)
                    Dim s As GsGameServer = Nothing
                    If res.HasRows Then
                        res.Read()
                        s = Me.BuildServer(res)
                    End If
                    res.Close()
                    Return s
                End Using
            End SyncLock
        End Function

        Public Function FetchPlayers(ByVal sid As String, ByVal timeout As Int32) As ElementTable
            Dim tbl As New ElementTable With {
                .header = {"player_", "score_", "deaths_", "ping_", "team_", "kills_"}
            }

            SyncLock Me.connection
                Dim sql As String = "select * from `players` where `sid` = " & sid & " and `lastseen` > (UNIX_TIMESTAMP() - " & timeout.ToString() & ")"
                Using res As MySqlDataReader = Me.DoQuery(sql)

                    If res.HasRows Then
                        Dim players As New List(Of String())

                        While res.Read()
                            Dim pArr(5) As String
                            pArr(0) = res("gq_name").ToString()
                            pArr(1) = res("gq_score").ToString()
                            pArr(2) = res("gq_deaths").ToString()
                            pArr(3) = res("gq_ping").ToString()
                            pArr(4) = res("gq_team").ToString()
                            pArr(5) = res("gq_kills").ToString()
                            players.Add(pArr)
                        End While

                        Dim data(players.Count - 1, tbl.header.Length - 1) As String

                        For i = 0 To players.Count - 1
                            For j = 0 To tbl.header.Length - 1
                                data(i, j) = players(i)(j)
                            Next
                        Next
                        tbl.rows = CByte(players.Count)
                        tbl.data = data
                    End If
                    res.Close()
                End Using
            End SyncLock

            Return tbl
        End Function

        Public Function ChallengeOK(ByVal rIPEP As Net.IPEndPoint) As Boolean
            Dim sql As String =
          "select `id` from `gameserver` where `address` = '" & rIPEP.Address.ToString() & "' and " &
          "`port` = " & rIPEP.Port.ToString() & " and `challengeok` = 1"

            SyncLock Me.connection
                Using res As MySqlDataReader = Me.DoQuery(sql)
                    Return Me.CheckForRows(res)
                End Using
            End SyncLock
        End Function


        'Corrects wrong Gametypes
        Private Function CorrectGameType(ByVal type As String) As String
            If type = "swbfrontps2p" Then type = "swbfrontps2"
            If type = "swbfront2ps2p" Then type = "swbfront2ps2"
            Return type
        End Function

        'Prevents buggy Playernames from being stored
        Private Function CorrectPlayerName(ByVal name As String) As String
            If String.IsNullOrEmpty(name) Then
                Return GsConst.PARAM_NO_PLAYERNAME
            End If
            Return name
        End Function
    End Class

End Namespace
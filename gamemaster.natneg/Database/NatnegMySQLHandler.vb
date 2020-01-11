Imports System.Net

Imports MySql.Data.MySqlClient

Imports gamemaster.common.Database
Imports gamemaster.common.Util

Public Class NatnegMySQLHandler
    Inherits MySQLHandler

    'The masterserver's own internal ID
    Public Property MasterServerID As Int32

    'Starts a new session or updates an existing one
    Public Sub RegisterNatnegToken(ByVal localIPEP As Net.IPEndPoint,
                                   ByVal rIPEP As Net.IPEndPoint,
                                   ByVal cookie As Int32,
                                   ByVal gamename As String,
                                   ByVal sequence As Byte,
                                   ByVal clienttype As Byte,
                                   Optional ByVal comport As Int32 = -1)

        Dim sql As String = String.Empty

        If (ClientExists(cookie, clienttype)) Then
            If comport <> -1 Then
                'was sent using the client's natneg-comport
                sql =
                "update `natneg` set " &
                "`natneg_sequence` = (`natneg_sequence` + 1), " &
                "`natneg_localport` = " & localIPEP.Port.ToString & ", " &
                "`natneg_comport` = " & comport.ToString &
                " where " &
                "`natneg_clienttype` = '" & clienttype.ToString & "' and " &
                "`natneg_cookie` = " & cookie.ToString
            Else
                'was sent using the client's host-port
                sql =
                "update `natneg` set " &
                "`natneg_sequence` = (`natneg_sequence` + 1), " &
                "`natneg_remoteip` = '" & rIPEP.Address.ToString & "', " &
                "`natneg_remoteport` = " & rIPEP.Port.ToString &
                " where " &
                "`natneg_clienttype` = '" & clienttype.ToString & "' and " &
                "`natneg_cookie` = " & cookie.ToString
            End If
        Else
            'No session for that client -> create a new one
            sql =
             "insert into `natneg` set " &
             "`natneg_cookie` = " & cookie.ToString & ", " &
             "`natneg_gamename` = '" & EscapeString(gamename) & "', " &
             "`natneg_sequence` = 0, " &
             "`natneg_clienttype` = " & clienttype.ToString & ", " &
             "`natneg_remoteip` = '" & rIPEP.Address.ToString & "', " &
             "`natneg_remoteport` = " & rIPEP.Port.ToString & ", " &
             "`natneg_localip` = '" & localIPEP.Address.ToString & "', " &
             "`natneg_localport` = " & localIPEP.Port.ToString & ", " &
             "`natneg_masterserver` = " & MasterServerID.ToString & ", " &
             "`natneg_lastupdate` = UNIX_TIMESTAMP()"
        End If

        'Run the Query
        Me.NonQuery(sql)
    End Sub

    'Checks if both sessions are ready for operation
    Public Function NatnegReady(ByVal cookie As Int32) As Boolean
        SyncLock Me.connection

            'Sum the sequence-IDs to get the total amount of init-packets
            Dim sql As String =
                "select SUM(`natneg_sequence`) as sequence from `natneg` where " &
                "`natneg_cookie` = " & cookie.ToString

            'Run the Query and fetch the sum
            Using res As MySqlDataReader = Me.DoQuery(sql)
                res.Read()
                Dim c As Int32 = res.GetInt32("sequence")
                res.Close()
                'Check if it's >= then the minimum sequence required
                Return (c >= 2 * GsConst.GS_NATNEG_MINSEQUENCE)
            End Using
        End SyncLock
    End Function

    'Checks if there's a session for a client
    Public Function ClientExists(ByVal cookie As Int32, ByVal clientType As Byte) As Boolean
        SyncLock Me.connection
            Dim sql As String =
                "select `id` from `natneg` where " &
                "`natneg_cookie` = " & cookie.ToString & " and " &
                "`natneg_clienttype` = '" & clientType.ToString & "'"

            Return Me.CheckForRows(Me.DoQuery(sql))
        End SyncLock
    End Function

    'Gets a client's remote peer
    Public Function FetchRemotePeer(ByVal cookie As Int32, ByVal ownClientType As Byte) As NatnegPeer
        SyncLock Me.connection
            'select the client, it must be a different client type and has to share the same cookie
            'we also need the masterserver to check if we have to do a server-relay or can send
            'directly
            Dim sql As String =
              "select * from `natneg` " &
              "left join `masterserver` on `masterserver`.`id` = `natneg`.`natneg_masterserver` where " &
              "`natneg_cookie` = " & cookie.ToString & " and " &
              "`natneg_clienttype` != " & ownClientType.ToString

            Using res As MySqlDataReader = Me.DoQuery(sql)
                res.Read()

                If Not res.HasRows Then
                    res.Close()
                    Return Nothing
                End If

                Dim peer As NatnegPeer = New NatnegPeer()
                Try
                    Dim rAddr As Net.IPAddress = Net.IPAddress.Parse(res.GetString("natneg_remoteip"))
                    Dim rPort As UInt16 = res.GetUInt16("natneg_remoteport")
                    Dim cPort As UInt16 = res.GetUInt16("natneg_comport")

                    peer.hostIPEP = New Net.IPEndPoint(rAddr, rPort)
                    peer.comIPEP = New Net.IPEndPoint(rAddr, cPort)

                    If Not DBNull.Value.Equals(res("server_name")) Then
                        Dim ms As New MasterServer With {
                            .Id = res.GetInt32("natneg_masterserver"),
                            .MsName = res.GetString("server_name")
                        }
                        Dim ipa As IPAddress = IPAddress.Parse(res.GetString("server_natnegaddress"))
                        ms.RIPEP = New IPEndPoint(ipa, res.GetInt32("server_natnegport"))
                        peer.ms = ms
                    End If
                    res.Close()
                Catch ex As Exception
                    res.Close()
                    Logger.Log("Failed to fetch Server " & cookie.ToString, LogLevel.Verbose)
                End Try
                Return peer
            End Using
        End SyncLock
    End Function

    Public Sub DropSession(ByVal cookie As Int32)
        Me.NonQuery("delete from `natneg` where `natneg_cookie` = " & cookie.ToString)
    End Sub
End Class
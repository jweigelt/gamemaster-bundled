'Main Listrequest & Cryptheader-initialisation packet
'JW "LeKeks" 05/2014
Imports gamemaster.common.Util
Imports gamemaster.common.Network
Imports gamemaster.serverlist.Gameserver

Namespace Network.Serverbrowsing

    Public Class ListRequestPacket
        Inherits ServerbrowsingPacket

        Public Property ParameterArray As String()
        Public Property Filter As String
        Public Property Options As Byte

        Sub New(ByVal client As ServerlistClient, ByVal data() As Byte)
            MyBase.New(client, data)
            Me.UseCipher = True                                  'turn on encryption
            Me.Filter = FetchString(Me.Data)                     'Fetch the filter-string
            Me.ParameterArray = FetchString(Me.Data).Split("\") 'Get the requested params
            'TODO: might be casted to int32, however doesn't match std. C-style int32-format (LE?)
            Me.Options = Me.Data(bytesParsed + 3) 'BitConverter.ToInt32(Me.data, Me.bytesParsed)
        End Sub

        Public Overrides Sub ManageData()
            Me.Client.Send(Me)
        End Sub

        Private Function BuildServerArray() As Byte()
            'TODO: Implement Serverside filtering
            Dim buffer() As Byte = {}

            'Header
            ArrayFunctions.ConcatArray(Me.client.RemoteIPEP.Address.GetAddressBytes(), buffer)
            'ConcatArray({25, 100}, buffer) 'TODO: fetch port from db
            ArrayFunctions.ConcatArray(ArrayFunctions.DumpUInt16LE(3658), buffer)
            'checking query state via bitwise and

            'If Me.Options And GS_FLAG_SEND_GROUPS Then
            'grouped server query (EaW p.e.)
            'ConcatArray({Me.ParameterArray.Length - 1, 0}, buffer)
            'ConcatArray(Me.BuildParameterArray(), buffer)

            'Dim flags As Byte
            'flags = flags Or GS_FLAG_UNSOLICITED_UDP
            'flags = flags Or GS_FLAG_HAS_FULL_RULES
            'flags = flags Or GS_FLAG_HAS_KEYS

            'ConcatArray({flags}, buffer)
            'ConcatArray({127, 0, 0, 1}, buffer) 'empty ipa
            'PushString(buffer, "LEKEKS")
            'PushString(buffer, "1")
            'PushString(buffer, "10")
            'PushString(buffer, "4")
            'PushString(buffer, "3")
            'PushString(buffer, "0")
            'PushString(buffer, "1", False)

            'ConcatArray({&H0, &HFF, &HFF, &HFF, &HFF}, buffer)

            'Logger.Log("Grouping data values for {0}", LogLevel.Verbose, Me.client.GameName)
            If Not (Me.Options = GsConst.GS_FLAG_NO_SERVER_LIST) Then 'Me.ParameterArray.Count > 1 Then
                ArrayFunctions.ConcatArray({CByte(Me.ParameterArray.Length - 1), 0}, buffer)
                ArrayFunctions.ConcatArray(Me.BuildParameterArray(), buffer)

                Dim servers As List(Of GsGameServer) = Me.Client.Server.MySQL.GetServers(Me.Client.GameName, Me.Client.Server.Config.GameserverTimeout)
                Logger.Log("Fetched {0} active servers from database. ({1})", LogLevel.Verbose, servers.Count.ToString)

                For Each server As GsGameServer In servers
                    If server.ChallengeOK = False Then Continue For 'don't list unauthenticated servers
                    Me.BuildServerEntry(server, buffer)
                Next
                ArrayFunctions.ConcatArray({&H0, &HFF, &HFF, &HFF, &HFF}, buffer) 'set last bytes, \xFF\xFF\xFF\xFF indicates last server
            Else
                Logger.Log("Sending header to {0}.", LogLevel.Verbose, Me.client.RemoteIPEP.ToString())
            End If

            Return buffer
        End Function

        Private Sub BuildServerEntry(ByVal server As GsGameServer, ByRef buffer() As Byte)
            If server.PortClosed And Me.ParameterArray.Length < 2 Then Return
            Dim serverFlags As Byte = 0
            Dim ip0 As Net.IPAddress = Nothing

            Dim hasLocalIP As Boolean = Net.IPAddress.TryParse(server.GetValue("localip0"), ip0)

            ToggleFlag(serverFlags, GsConst.GS_FLAG_CONNECT_NEGOTIATE_FLAG)
            ToggleFlag(serverFlags, GsConst.GS_FLAG_NONSTANDARD_PORT)

            If (Me.Options = GsConst.GS_FLAG_SEND_FIELDS_FOR_ALL) Then
                ToggleFlag(serverFlags, GsConst.GS_FLAG_PRIVATE_IP)
                ToggleFlag(serverFlags, GsConst.GS_FLAG_NONSTANDARD_PRIVATE_PORT)
                ToggleFlag(serverFlags, GsConst.GS_FLAG_HAS_KEYS)
                ToggleFlag(serverFlags, GsConst.GS_FLAG_ICMP_IP)
            Else
                If server.IsNatted And Not server.PortClosed Then '85
                    ToggleFlag(serverFlags, GsConst.GS_FLAG_HAS_KEYS)
                    ToggleFlag(serverFlags, GsConst.GS_FLAG_UNSOLICITED_UDP)

                ElseIf server.PortClosed Then '126
                    ToggleFlag(serverFlags, GsConst.GS_FLAG_PRIVATE_IP)
                    ToggleFlag(serverFlags, GsConst.GS_FLAG_NONSTANDARD_PRIVATE_PORT)
                    ToggleFlag(serverFlags, GsConst.GS_FLAG_HAS_KEYS)
                    ToggleFlag(serverFlags, GsConst.GS_FLAG_ICMP_IP)

                Else '21
                    ToggleFlag(serverFlags, GsConst.GS_FLAG_UNSOLICITED_UDP)
                End If

            End If

            'Don't accept direct querys for "homeservers", they'll only slow down the SBQEngine 
            ArrayFunctions.ConcatArray({serverFlags}, buffer)

            'TODO: add compatibility for peerchat-lobbys
            'This implementation is critical: changing to the localip will cause wrong hash-calculations
            'for the peerchat lobby-system -> maybe detect peerchat games
            If server.PublicIP = Me.client.RemoteIPEP.Address.ToString And server.IsNatted And Not server.PortClosed And hasLocalIP Then
                ArrayFunctions.ConcatArray(ip0.GetAddressBytes, buffer)
                'TODO: hostport might be wrong, check for localport instead
                ArrayFunctions.ConcatArray(ArrayFunctions.DumpUInt16LE(UInt16.Parse(server.HostPort)), buffer)
            Else
                ArrayFunctions.ConcatArray(Net.IPAddress.Parse(server.PublicIP).GetAddressBytes, buffer)
                ArrayFunctions.ConcatArray(ArrayFunctions.DumpUInt16LE(UInt16.Parse(server.PublicPort)), buffer)
            End If

            If (serverFlags And GsConst.GS_FLAG_PRIVATE_IP) > 0 Then
                If Not hasLocalIP Then Return
                Dim lport As UInt16 = UInt16.Parse(server.PublicPort)
                UInt16.TryParse(server.GetValue("localport"), lport)
                ArrayFunctions.ConcatArray(ip0.GetAddressBytes(), buffer)
                ArrayFunctions.ConcatArray(ArrayFunctions.DumpUInt16LE(lport), buffer)
            End If

            If (serverFlags And GsConst.GS_FLAG_ICMP_IP) > 0 Then
                ArrayFunctions.ConcatArray(Net.IPAddress.Parse(server.PublicIP).GetAddressBytes, buffer)
            End If

            If (serverFlags And GsConst.GS_FLAG_HAS_KEYS) > 0 Then
                ArrayFunctions.ConcatArray({255}, buffer)
                For i = 1 To Me.ParameterArray.Length - 1
                    Dim val As String = server.GetValue(Me.ParameterArray(i))
                    Me.PushString(buffer, val, (i = Me.ParameterArray.Length - 1))
                Next
            End If

        End Sub

        Private Sub ToggleFlag(ByRef dest As Byte, ByVal flag As Byte)
            dest = dest Or flag
        End Sub


        Private Sub PushString(ByRef data() As Byte, str As String, Optional ByVal isLast As Boolean = False)
            ArrayFunctions.ConcatArray(ArrayFunctions.GetBytes(str), data)
            If isLast Then
                ArrayFunctions.ConcatArray({0}, data)
            Else
                ArrayFunctions.ConcatArray({0, &HFF}, data)
            End If
        End Sub

        Public Overrides Function CompileResponse() As Byte()
            Logger.Log("Sending Serverlist", LogLevel.Verbose)
            Dim buffer() As Byte = BuildServerArray()
            Return buffer
        End Function

        Private Function BuildParameterArray() As Byte()
            Dim buffer() As Byte = {}
            For Each Str As String In ParameterArray
                If Str <> String.Empty Then
                    ArrayFunctions.ConcatArray(ArrayFunctions.GetBytes(Str), buffer)
                    ArrayFunctions.ConcatArray({0, 0}, buffer)
                End If
            Next
            Return buffer
        End Function
    End Class
End Namespace
'JW "LeKeks" 07/2014
Imports gamemaster.common.Util

Public Structure NatnegPeer
    Dim comIPEP As Net.IPEndPoint   'The client's WAN IPEP
    Dim hostIPEP As Net.IPEndPoint  'The client's local/LAN IPEP
    Dim ms As MasterServer          'The managing masterserver

    'Generic Constructor
    Sub New(ByVal hostIPEP As Net.IPEndPoint, ByVal comIPEP As Net.IPEndPoint, Optional ByVal ms As MasterServer = Nothing)
        Me.hostIPEP = hostIPEP
        Me.comIPEP = comIPEP
        Me.ms = ms
    End Sub
End Structure
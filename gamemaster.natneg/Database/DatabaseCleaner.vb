'class for scheduling cleanup-queries on the MySQL server
'JW "LeKeks" 07/2014
Imports System.Threading
Imports gamemaster.common.Util

Namespace Database
    Public Class DatabaseCleaner
        Public Property CleanupInterval As Integer    'Delay (in s) between the queries
        Public Property CleanupTimeout As Integer     'Time (in s) before a natneg-session gets dropped
        Public Property MySQL As NatnegMySQLHandler

        Private workThread As Thread
        Private running As Boolean = False

        Public Sub init()
            If Not Me.running Then
                Me.running = True
                Me.workThread = New Thread(AddressOf Me.Cleanup)
                Me.workThread.Start()
            End If
        End Sub

        Public Sub Close()
            Me.running = False
            Me.workThread.Join()
        End Sub

        Private Sub Cleanup()
            While Me.running
                Logger.Log("Cleaning up Database...", LogLevel.Verbose)
                'dropping all natneg sessions that are older than NOW() - CleanupTimeout
                '-> ensure we drop failed sessions after a while
                Me.MySQL.NonQuery("delete from `natneg` where `natneg_lastupdate` < (UNIX_TIMESTAMP() - " & Me.CleanupTimeout & ")")
                Thread.Sleep(Me.CleanupInterval * 1000)
            End While
        End Sub
    End Class
End Namespace
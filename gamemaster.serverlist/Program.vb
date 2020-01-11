Module Program
    Private Const PRODUCT_NAME As String = "gamemaster serverlist service"

    Sub Main(args As String())
        Console.WriteLine(PRODUCT_NAME)
        Dim server As New ServerlistServer()
        server.Run()
    End Sub
End Module
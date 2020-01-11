Module Program
    Private Const PRODUCT_NAME As String = "gamemaster keyauth service"

    Sub Main(args As String())
        Console.WriteLine(PRODUCT_NAME)
        Dim server As New KeyauthServer()
        server.Run()
    End Sub
End Module
Module Program
    Private Const PRODUCT_NAME As String = "gamemaster natneg service"

    Sub Main(args As String())
        Console.WriteLine(PRODUCT_NAME)
        Dim server As New NatnegServer()
        server.Run()
    End Sub
End Module
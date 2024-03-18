Imports System.Data.SqlClient

Module connectionDatabase
    Public con As New SqlConnection
    Public cmd As New SqlConnection

    Sub opencon()
        con.ConnectionString = "Data Source=VALIARES;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;Database=db_urs"
    End Sub
End Module

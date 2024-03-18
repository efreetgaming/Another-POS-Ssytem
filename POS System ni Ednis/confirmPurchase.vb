Imports System.ComponentModel
Imports System.Data.SqlClient

Public Class confirmPurchase
    Public Sub confirmPurchase_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Dim deleteQuery As String = "Delete from tbl_basket"

        Using cmd As SqlCommand = New SqlCommand(deleteQuery, con)
            con.Open()
            cmd.ExecuteNonQuery()
            con.Close()
            Form1.refreshDataBasket()
        End Using
    End Sub
End Class
Imports System.Data.SqlClient
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.Tab

Public Class Form1
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        opencon()
        MsgBox("Connected")
        con.Close()

        refreshDataCart()
        refreshDataBasket()
    End Sub

    ' Refresh the Data in 2 DataGridView Items
    Public Sub refreshDataCart()
        Dim query As String = "Select * From tbl_cart"

        Using cmd As New SqlCommand(query, con)
            Using da As New SqlDataAdapter
                da.SelectCommand = cmd
                Using dt As New DataTable
                    da.Fill(dt)
                    dgvCart.DataSource = dt
                End Using
            End Using
        End Using
    End Sub

    Public Sub refreshDataBasket()
        Dim query As String = "Select * from tbl_basket"

        Using cmd As New SqlCommand(query, con)
            Using da As New SqlDataAdapter
                da.SelectCommand = cmd
                Using dt As New DataTable
                    da.Fill(dt)
                    dgvBasket.DataSource = dt
                End Using
            End Using
        End Using
    End Sub

    ' Click event where the data will be input at textbox
    Private Sub dgvCart_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvCart.CellClick
        txtBarcode.Text = dgvCart.Rows(e.RowIndex).Cells(0).Value.ToString
        txtProductName.Text = dgvCart.Rows(e.RowIndex).Cells(1).Value.ToString
        txtProductLeft.Text = dgvCart.Rows(e.RowIndex).Cells(2).Value.ToString
        txtPrice.Text = dgvCart.Rows(e.RowIndex).Cells(3).Value.ToString


    End Sub

    ' Add the data to the Basket and Updates the quantity in the Cart
    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        Dim Barcode As String = txtBarcode.Text
        Dim productName As String = txtProductName.Text
        Dim productLeft As String = txtProductLeft.Text
        Dim Price As Integer = txtPrice.Text
        Dim quantity As Integer = txtQuantity.Text

        ' Computation of within the product
        ' This Inserts at the User's Basket together with the final price
        ' Changes
        ' - Missing Values

        ' Bug
        ' - When you type 10 and then add 
        Dim currentQuantity As Integer = 0
        Dim rowIndex As Integer = dgvCart.CurrentCell.RowIndex
        If Integer.TryParse(dgvCart.Rows(rowIndex).Cells("Quantity").Value.ToString(), currentQuantity) Then
            If currentQuantity = 0 Then
                MsgBox("Cannot purchase this item because there is no stock available.")
                Return ' Exit the method if the item quantity is zero
            ElseIf quantity > currentQuantity Then
                MsgBox($"Only {currentQuantity} left in stock for this item.")
                Return ' Exit the method if the requested quantity exceeds the available stock
            End If
        Else
            MsgBox("Error: Invalid quantity for the selected item.")
            Return ' Exit the method if the quantity is invalid
        End If


        Dim FinalPrice As Integer = Price * quantity

        Dim query As String = "Insert Into tbl_basket Values (@Barcode, @ProductName, @Quantity, @Price)"

        Using cmd As SqlCommand = New SqlCommand(query, con)
            cmd.Parameters.AddWithValue("Barcode", Barcode)
            cmd.Parameters.AddWithValue("ProductName", productName)
            cmd.Parameters.AddWithValue("Quantity", quantity)
            cmd.Parameters.AddWithValue("Price", FinalPrice)

            con.Open()

            cmd.ExecuteNonQuery()
            con.Close()
            refreshDataBasket()
        End Using


        ' Updates the Quantity of the product in the Database(Cart)
        ' This will Minus the quantity of the product
        Dim updateQuantityCart As Integer = productLeft - quantity

        Dim updateCartQuery As String = "Update tbl_cart set Quantity=@Quantity where Barcode=@Barcode"

        Using cmd As SqlCommand = New SqlCommand(updateCartQuery, con)
            cmd.Parameters.AddWithValue("Quantity", CInt(updateQuantityCart).ToString)
            cmd.Parameters.AddWithValue("Barcode", Barcode)

            con.Open()
            cmd.ExecuteNonQuery()
            con.Close()
            refreshDataCart()
        End Using

        txtBarcode.Clear()
        txtProductName.Clear()
        txtProductLeft.Clear()
        txtQuantity.Clear()
        txtPrice.Clear()
    End Sub

    ' Compute the whole price inside the Basket
    Private Sub btnCompute_Click(sender As Object, e As EventArgs) Handles btnCompute.Click

        ' Computation in Total Quantity
        Dim totalQuantity As Integer = 0

        For Each row As DataGridViewRow In dgvBasket.Rows
            If Not row.IsNewRow Then
                totalQuantity += Convert.ToInt32(row.Cells("Quantity").Value)
            End If
        Next
        txtTotalQuantatity.Text = totalQuantity

        ' Computation in Total Price
        Dim totalPriceNoVat As Integer = 0

        For Each row As DataGridViewRow In dgvBasket.Rows
            If Not row.IsNewRow Then
                totalPriceNoVat += Convert.ToInt32(row.Cells("Price").Value)
            End If
        Next

        ' Computation of VAT x Total Price
        Dim VAT As Double = totalPriceNoVat * 0.12
        txtVAT.Text = VAT

        ' Computation of Total Amount or Bayad
        Dim totalPriceWithVat As Double = CDbl(totalPriceNoVat) + CDbl(VAT)
        txtTotalPrice.Text = totalPriceWithVat
    End Sub

    ' Calculates the Change of the customer at the same time
    Private Sub txtTotalAmount_TextChanged(sender As Object, e As EventArgs) Handles txtTotalAmount.TextChanged
        If txtTotalAmount.Text = "" Then
            ' If the Amount is Empty then it will add error to prevent the error occurred
            Dim defaultNumber As Double = 0
            txtTotalAmount.Text = defaultNumber.ToString()
            txtSukli.Text = 0
        Else
            ' It will automatically compute the sukli
            Dim bayadNiCustomer As Double = 0
            Dim totalPrice As Double = 0

            If Double.TryParse(txtTotalAmount.Text, bayadNiCustomer) AndAlso Double.TryParse(txtTotalPrice.Text, totalPrice) Then
                txtSukli.Text = Math.Round(bayadNiCustomer - totalPrice, 2).ToString()

            End If
        End If
    End Sub

    ' This will prints the receipt of the user
    Private Sub btnPrintReceipt_Click(sender As Object, e As EventArgs) Handles btnPrintReceipt.Click
        Dim amount As String = txtTotalAmount.Text
        Dim price As String = txtTotalPrice.Text
        Dim sukli As String = txtSukli.Text

        If price = "" And sukli = "" Then
            MsgBox("You need to click compute")
        ElseIf price = "" Then
            MsgBox("You need to compute the whole total price")
        Else
            If amount < price Then
                MsgBox("Not Enough Money, Scammaz")
            Else
                MsgBox("Salamat sa iyong pera")
                confirmPurchase.Show()
                refreshDataBasket()
                txtTotalAmount.Clear()
                txtTotalPrice.Clear()
                txtSukli.Clear()
                txtVAT.Clear()
                txtTotalQuantatity.Clear()
            End If
        End If

    End Sub

    Private Sub txtQuantity_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtQuantity.KeyPress
        ' Allow only digits and control keys
        If Not Char.IsControl(e.KeyChar) AndAlso Not Char.IsDigit(e.KeyChar) Then
            e.Handled = True ' Suppress the key press if it's not a digit or a control key
        End If

        ' Limit the length to five digits
        If txtQuantity.Text.Length >= 5 AndAlso Not Char.IsControl(e.KeyChar) Then
            e.Handled = True ' Suppress the key press if it exceeds the length limit
        End If
    End Sub

    Private Sub txtTotalAmount_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtTotalAmount.KeyPress
        ' Allow only digits and control keys
        If Not Char.IsControl(e.KeyChar) AndAlso Not Char.IsDigit(e.KeyChar) Then
            e.Handled = True ' Suppress the key press if it's not a digit or a control key
        End If

        ' Limit the length to six digits
        If txtTotalAmount.Text.Length >= 6 AndAlso Not Char.IsControl(e.KeyChar) Then
            e.Handled = True ' Suppress the key press if it exceeds the length limit
        End If
    End Sub
End Class
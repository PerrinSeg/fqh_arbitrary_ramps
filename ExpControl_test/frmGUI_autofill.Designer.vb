<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmGUI_autofill
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Append_Button = New System.Windows.Forms.Button
        Me.N_updownBox = New System.Windows.Forms.NumericUpDown
        Me.Label1 = New System.Windows.Forms.Label
        Me.autofill_textBox = New System.Windows.Forms.TextBox
        Me.Clear_table_button = New System.Windows.Forms.Button
        Me.Text_useful_expr = New System.Windows.Forms.Label
        Me.prevScript_button = New System.Windows.Forms.Button
        Me.defaultScript_button = New System.Windows.Forms.Button
        CType(Me.N_updownBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Append_Button
        '
        Me.Append_Button.Location = New System.Drawing.Point(336, 417)
        Me.Append_Button.Name = "Append_Button"
        Me.Append_Button.Size = New System.Drawing.Size(69, 45)
        Me.Append_Button.TabIndex = 0
        Me.Append_Button.Text = "Append"
        Me.Append_Button.UseVisualStyleBackColor = True
        '
        'N_updownBox
        '
        Me.N_updownBox.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.N_updownBox.Location = New System.Drawing.Point(135, 18)
        Me.N_updownBox.Name = "N_updownBox"
        Me.N_updownBox.Size = New System.Drawing.Size(48, 23)
        Me.N_updownBox.TabIndex = 2
        Me.N_updownBox.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(22, 20)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(116, 17)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "For i = 1: N,  N = "
        '
        'autofill_textBox
        '
        Me.autofill_textBox.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.autofill_textBox.Location = New System.Drawing.Point(25, 47)
        Me.autofill_textBox.Multiline = True
        Me.autofill_textBox.Name = "autofill_textBox"
        Me.autofill_textBox.Size = New System.Drawing.Size(380, 337)
        Me.autofill_textBox.TabIndex = 4
        '
        'Clear_table_button
        '
        Me.Clear_table_button.Location = New System.Drawing.Point(261, 417)
        Me.Clear_table_button.Name = "Clear_table_button"
        Me.Clear_table_button.Size = New System.Drawing.Size(69, 45)
        Me.Clear_table_button.TabIndex = 5
        Me.Clear_table_button.Text = "Clear Table"
        Me.Clear_table_button.UseVisualStyleBackColor = True
        '
        'Text_useful_expr
        '
        Me.Text_useful_expr.AutoSize = True
        Me.Text_useful_expr.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Text_useful_expr.Location = New System.Drawing.Point(22, 387)
        Me.Text_useful_expr.Name = "Text_useful_expr"
        Me.Text_useful_expr.Size = New System.Drawing.Size(30, 17)
        Me.Text_useful_expr.TabIndex = 6
        Me.Text_useful_expr.Text = "text"
        '
        'prevScript_button
        '
        Me.prevScript_button.Location = New System.Drawing.Point(25, 417)
        Me.prevScript_button.Name = "prevScript_button"
        Me.prevScript_button.Size = New System.Drawing.Size(69, 45)
        Me.prevScript_button.TabIndex = 7
        Me.prevScript_button.Text = "Previous" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Script"
        Me.prevScript_button.UseVisualStyleBackColor = True
        '
        'defaultScript_button
        '
        Me.defaultScript_button.Location = New System.Drawing.Point(100, 417)
        Me.defaultScript_button.Name = "defaultScript_button"
        Me.defaultScript_button.Size = New System.Drawing.Size(69, 45)
        Me.defaultScript_button.TabIndex = 8
        Me.defaultScript_button.Text = "Default" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Script"
        Me.defaultScript_button.UseVisualStyleBackColor = True
        '
        'frmGUI_autofill
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(428, 484)
        Me.Controls.Add(Me.defaultScript_button)
        Me.Controls.Add(Me.prevScript_button)
        Me.Controls.Add(Me.Text_useful_expr)
        Me.Controls.Add(Me.Clear_table_button)
        Me.Controls.Add(Me.autofill_textBox)
        Me.Controls.Add(Me.N_updownBox)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Append_Button)
        Me.Name = "frmGUI_autofill"
        Me.Text = "autofill"
        CType(Me.N_updownBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Append_Button As System.Windows.Forms.Button
    Friend WithEvents N_updownBox As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents autofill_textBox As System.Windows.Forms.TextBox
    Friend WithEvents Clear_table_button As System.Windows.Forms.Button
    Friend WithEvents Text_useful_expr As System.Windows.Forms.Label
    Friend WithEvents prevScript_button As System.Windows.Forms.Button
    Friend WithEvents defaultScript_button As System.Windows.Forms.Button
End Class

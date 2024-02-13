<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmGUI_interactive
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
        Me.ClearButton = New System.Windows.Forms.Button()
        Me.ReloadChannelMapButton = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'ClearButton
        '
        Me.ClearButton.Location = New System.Drawing.Point(50, 25)
        Me.ClearButton.Name = "ClearButton"
        Me.ClearButton.Size = New System.Drawing.Size(100, 40)
        Me.ClearButton.TabIndex = 0
        Me.ClearButton.Text = "Clear"
        Me.ClearButton.UseVisualStyleBackColor = True
        '
        'ReloadChannelMapButton
        '
        Me.ReloadChannelMapButton.Location = New System.Drawing.Point(190, 25)
        Me.ReloadChannelMapButton.Name = "ReloadChannelMapButton"
        Me.ReloadChannelMapButton.Size = New System.Drawing.Size(125, 40)
        Me.ReloadChannelMapButton.TabIndex = 1
        Me.ReloadChannelMapButton.Text = "Reload ChannelsWithCards"
        Me.ReloadChannelMapButton.UseVisualStyleBackColor = True
        '
        'frmGUI_interactive
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(502, 736)
        Me.Controls.Add(Me.ReloadChannelMapButton)
        Me.Controls.Add(Me.ClearButton)
        Me.Name = "frmGUI_interactive"
        Me.Text = "frmGUI_interactive"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ClearButton As System.Windows.Forms.Button
    Friend WithEvents ReloadChannelMapButton As System.Windows.Forms.Button
End Class

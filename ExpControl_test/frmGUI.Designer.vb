<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmGUI
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmGUI))
        Me.RunButton = New System.Windows.Forms.Button()
        Me.ChooseProgramButton = New System.Windows.Forms.Button()
        Me.StatusLabel = New System.Windows.Forms.Label()
        Me.OpenFileDialog = New System.Windows.Forms.OpenFileDialog()
        Me.FolderBrowserDialog = New System.Windows.Forms.FolderBrowserDialog()
        Me.StopButton = New System.Windows.Forms.Button()
        Me.randomizeButton = New System.Windows.Forms.Button()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.logarithmicRadio = New System.Windows.Forms.RadioButton()
        Me.linearRadio = New System.Windows.Forms.RadioButton()
        Me.noPointsField = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.stopSeqField = New System.Windows.Forms.TextBox()
        Me.startSeqField = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.writeSeqButton = New System.Windows.Forms.Button()
        Me.noToAddField = New System.Windows.Forms.TextBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.AddExpsButton = New System.Windows.Forms.Button()
        Me.ExpSeqPanel = New System.Windows.Forms.Panel()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.interactiveCmdText = New System.Windows.Forms.TextBox()
        Me.BatchButton = New System.Windows.Forms.Button()
        Me.StopBatchButton = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.LoopingPanel = New System.Windows.Forms.Panel()
        Me.currentExpFileLabel = New System.Windows.Forms.Label()
        Me.interGUI_Button = New System.Windows.Forms.Button()
        Me.logExp_Button = New System.Windows.Forms.Button()
        Me.AutoFill = New System.Windows.Forms.Button()
        Me.RunVirtualButton = New System.Windows.Forms.Button()
        Me.loadLog_button = New System.Windows.Forms.Button()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'RunButton
        '
        Me.RunButton.Enabled = False
        Me.RunButton.Image = CType(resources.GetObject("RunButton.Image"), System.Drawing.Image)
        Me.RunButton.Location = New System.Drawing.Point(12, 78)
        Me.RunButton.Name = "RunButton"
        Me.RunButton.Size = New System.Drawing.Size(112, 34)
        Me.RunButton.TabIndex = 0
        Me.RunButton.Text = "  &Run"
        Me.RunButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.RunButton.UseVisualStyleBackColor = True
        '
        'ChooseProgramButton
        '
        Me.ChooseProgramButton.Enabled = False
        Me.ChooseProgramButton.Image = CType(resources.GetObject("ChooseProgramButton.Image"), System.Drawing.Image)
        Me.ChooseProgramButton.Location = New System.Drawing.Point(488, 78)
        Me.ChooseProgramButton.Name = "ChooseProgramButton"
        Me.ChooseProgramButton.Size = New System.Drawing.Size(113, 34)
        Me.ChooseProgramButton.TabIndex = 1
        Me.ChooseProgramButton.Text = "  &Load program"
        Me.ChooseProgramButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.ChooseProgramButton.UseVisualStyleBackColor = True
        '
        'StatusLabel
        '
        Me.StatusLabel.AutoSize = True
        Me.StatusLabel.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.StatusLabel.Location = New System.Drawing.Point(12, 28)
        Me.StatusLabel.Name = "StatusLabel"
        Me.StatusLabel.Size = New System.Drawing.Size(313, 25)
        Me.StatusLabel.TabIndex = 2
        Me.StatusLabel.Text = "Waiting for clients to connect ..."
        Me.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'OpenFileDialog
        '
        Me.OpenFileDialog.InitialDirectory = "C:/software/experiments"
        '
        'StopButton
        '
        Me.StopButton.Enabled = False
        Me.StopButton.Image = CType(resources.GetObject("StopButton.Image"), System.Drawing.Image)
        Me.StopButton.Location = New System.Drawing.Point(130, 78)
        Me.StopButton.Name = "StopButton"
        Me.StopButton.Size = New System.Drawing.Size(112, 34)
        Me.StopButton.TabIndex = 15
        Me.StopButton.Text = " &Stop"
        Me.StopButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.StopButton.UseVisualStyleBackColor = True
        '
        'randomizeButton
        '
        Me.randomizeButton.Enabled = False
        Me.randomizeButton.Location = New System.Drawing.Point(488, 828)
        Me.randomizeButton.Name = "randomizeButton"
        Me.randomizeButton.Size = New System.Drawing.Size(90, 55)
        Me.randomizeButton.TabIndex = 29
        Me.randomizeButton.Text = "Randomize experiments"
        Me.randomizeButton.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.GroupBox1.Controls.Add(Me.logarithmicRadio)
        Me.GroupBox1.Controls.Add(Me.linearRadio)
        Me.GroupBox1.Location = New System.Drawing.Point(369, 821)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(89, 63)
        Me.GroupBox1.TabIndex = 28
        Me.GroupBox1.TabStop = False
        '
        'logarithmicRadio
        '
        Me.logarithmicRadio.AutoSize = True
        Me.logarithmicRadio.Location = New System.Drawing.Point(6, 38)
        Me.logarithmicRadio.Name = "logarithmicRadio"
        Me.logarithmicRadio.Size = New System.Drawing.Size(79, 17)
        Me.logarithmicRadio.TabIndex = 1
        Me.logarithmicRadio.TabStop = True
        Me.logarithmicRadio.Text = "Logarithmic"
        Me.logarithmicRadio.UseVisualStyleBackColor = True
        '
        'linearRadio
        '
        Me.linearRadio.AutoSize = True
        Me.linearRadio.Location = New System.Drawing.Point(6, 15)
        Me.linearRadio.Name = "linearRadio"
        Me.linearRadio.Size = New System.Drawing.Size(54, 17)
        Me.linearRadio.TabIndex = 0
        Me.linearRadio.TabStop = True
        Me.linearRadio.Text = "Linear"
        Me.linearRadio.UseVisualStyleBackColor = True
        '
        'noPointsField
        '
        Me.noPointsField.Location = New System.Drawing.Point(322, 860)
        Me.noPointsField.Name = "noPointsField"
        Me.noPointsField.Size = New System.Drawing.Size(34, 20)
        Me.noPointsField.TabIndex = 27
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(295, 863)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(24, 13)
        Me.Label5.TabIndex = 26
        Me.Label5.Text = "No."
        '
        'stopSeqField
        '
        Me.stopSeqField.Location = New System.Drawing.Point(254, 860)
        Me.stopSeqField.Name = "stopSeqField"
        Me.stopSeqField.Size = New System.Drawing.Size(35, 20)
        Me.stopSeqField.TabIndex = 25
        '
        'startSeqField
        '
        Me.startSeqField.Location = New System.Drawing.Point(178, 860)
        Me.startSeqField.Name = "startSeqField"
        Me.startSeqField.Size = New System.Drawing.Size(35, 20)
        Me.startSeqField.TabIndex = 24
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(219, 864)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(29, 13)
        Me.Label6.TabIndex = 23
        Me.Label6.Text = "Stop"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(143, 864)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(29, 13)
        Me.Label7.TabIndex = 22
        Me.Label7.Text = "Start"
        '
        'writeSeqButton
        '
        Me.writeSeqButton.Enabled = False
        Me.writeSeqButton.Location = New System.Drawing.Point(146, 828)
        Me.writeSeqButton.Name = "writeSeqButton"
        Me.writeSeqButton.Size = New System.Drawing.Size(210, 26)
        Me.writeSeqButton.TabIndex = 21
        Me.writeSeqButton.Text = "Write sequence"
        Me.writeSeqButton.UseVisualStyleBackColor = True
        '
        'noToAddField
        '
        Me.noToAddField.Location = New System.Drawing.Point(76, 861)
        Me.noToAddField.Name = "noToAddField"
        Me.noToAddField.Size = New System.Drawing.Size(36, 20)
        Me.noToAddField.TabIndex = 20
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(13, 867)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(57, 13)
        Me.Label8.TabIndex = 19
        Me.Label8.Text = "No. to add"
        '
        'AddExpsButton
        '
        Me.AddExpsButton.Enabled = False
        Me.AddExpsButton.Location = New System.Drawing.Point(16, 828)
        Me.AddExpsButton.Name = "AddExpsButton"
        Me.AddExpsButton.Size = New System.Drawing.Size(98, 27)
        Me.AddExpsButton.TabIndex = 18
        Me.AddExpsButton.Text = "Add experiments"
        Me.AddExpsButton.UseVisualStyleBackColor = True
        '
        'ExpSeqPanel
        '
        Me.ExpSeqPanel.Location = New System.Drawing.Point(17, 344)
        Me.ExpSeqPanel.Name = "ExpSeqPanel"
        Me.ExpSeqPanel.Size = New System.Drawing.Size(1590, 471)
        Me.ExpSeqPanel.TabIndex = 17
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label9.Location = New System.Drawing.Point(12, 133)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(161, 20)
        Me.Label9.TabIndex = 31
        Me.Label9.Text = "Interactive command:"
        '
        'interactiveCmdText
        '
        Me.interactiveCmdText.Location = New System.Drawing.Point(179, 133)
        Me.interactiveCmdText.Name = "interactiveCmdText"
        Me.interactiveCmdText.Size = New System.Drawing.Size(1132, 20)
        Me.interactiveCmdText.TabIndex = 32
        '
        'BatchButton
        '
        Me.BatchButton.Enabled = False
        Me.BatchButton.Image = CType(resources.GetObject("BatchButton.Image"), System.Drawing.Image)
        Me.BatchButton.Location = New System.Drawing.Point(248, 78)
        Me.BatchButton.Name = "BatchButton"
        Me.BatchButton.Size = New System.Drawing.Size(112, 34)
        Me.BatchButton.TabIndex = 33
        Me.BatchButton.Text = "  Run batch"
        Me.BatchButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.BatchButton.UseVisualStyleBackColor = True
        '
        'StopBatchButton
        '
        Me.StopBatchButton.Enabled = False
        Me.StopBatchButton.Image = CType(resources.GetObject("StopBatchButton.Image"), System.Drawing.Image)
        Me.StopBatchButton.Location = New System.Drawing.Point(366, 78)
        Me.StopBatchButton.Name = "StopBatchButton"
        Me.StopBatchButton.Size = New System.Drawing.Size(112, 34)
        Me.StopBatchButton.TabIndex = 34
        Me.StopBatchButton.Text = "  Stop batch"
        Me.StopBatchButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.StopBatchButton.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(13, 308)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(262, 20)
        Me.Label1.TabIndex = 35
        Me.Label1.Text = "Batch mode experiment parameters"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(12, 176)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(277, 20)
        Me.Label2.TabIndex = 36
        Me.Label2.Text = "Looping mode experiment parameters"
        '
        'LoopingPanel
        '
        Me.LoopingPanel.Location = New System.Drawing.Point(16, 211)
        Me.LoopingPanel.Name = "LoopingPanel"
        Me.LoopingPanel.Size = New System.Drawing.Size(1591, 80)
        Me.LoopingPanel.TabIndex = 37
        '
        'currentExpFileLabel
        '
        Me.currentExpFileLabel.AutoSize = True
        Me.currentExpFileLabel.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.currentExpFileLabel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.currentExpFileLabel.Location = New System.Drawing.Point(656, 28)
        Me.currentExpFileLabel.Name = "currentExpFileLabel"
        Me.currentExpFileLabel.Size = New System.Drawing.Size(255, 25)
        Me.currentExpFileLabel.TabIndex = 38
        Me.currentExpFileLabel.Text = "Current experiment: none"
        Me.currentExpFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'interGUI_Button
        '
        Me.interGUI_Button.Location = New System.Drawing.Point(1355, 125)
        Me.interGUI_Button.Name = "interGUI_Button"
        Me.interGUI_Button.Size = New System.Drawing.Size(109, 34)
        Me.interGUI_Button.TabIndex = 39
        Me.interGUI_Button.Text = "Interactive GUI"
        Me.interGUI_Button.UseVisualStyleBackColor = True
        '
        'logExp_Button
        '
        Me.logExp_Button.Location = New System.Drawing.Point(1497, 828)
        Me.logExp_Button.Name = "logExp_Button"
        Me.logExp_Button.Size = New System.Drawing.Size(110, 51)
        Me.logExp_Button.TabIndex = 40
        Me.logExp_Button.Text = "Log Experiment"
        Me.logExp_Button.UseVisualStyleBackColor = True
        '
        'AutoFill
        '
        Me.AutoFill.Enabled = False
        Me.AutoFill.Location = New System.Drawing.Point(599, 828)
        Me.AutoFill.Name = "AutoFill"
        Me.AutoFill.Size = New System.Drawing.Size(89, 55)
        Me.AutoFill.TabIndex = 41
        Me.AutoFill.Text = "AutoFill"
        Me.AutoFill.UseVisualStyleBackColor = True
        '
        'RunVirtualButton
        '
        Me.RunVirtualButton.Enabled = False
        Me.RunVirtualButton.Image = CType(resources.GetObject("RunVirtualButton.Image"), System.Drawing.Image)
        Me.RunVirtualButton.Location = New System.Drawing.Point(748, 78)
        Me.RunVirtualButton.Name = "RunVirtualButton"
        Me.RunVirtualButton.Size = New System.Drawing.Size(112, 33)
        Me.RunVirtualButton.TabIndex = 42
        Me.RunVirtualButton.Text = "  Run Virtual"
        Me.RunVirtualButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.RunVirtualButton.UseVisualStyleBackColor = True
        '
        'loadLog_button
        '
        Me.loadLog_button.Location = New System.Drawing.Point(1381, 828)
        Me.loadLog_button.Name = "loadLog_button"
        Me.loadLog_button.Size = New System.Drawing.Size(110, 51)
        Me.loadLog_button.TabIndex = 43
        Me.loadLog_button.Text = "Load log file"
        Me.loadLog_button.UseVisualStyleBackColor = True
        '
        'frmGUI
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange
        Me.ClientSize = New System.Drawing.Size(1634, 900)
        Me.Controls.Add(Me.loadLog_button)
        Me.Controls.Add(Me.RunVirtualButton)
        Me.Controls.Add(Me.AutoFill)
        Me.Controls.Add(Me.logExp_Button)
        Me.Controls.Add(Me.interGUI_Button)
        Me.Controls.Add(Me.currentExpFileLabel)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.LoopingPanel)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.StopBatchButton)
        Me.Controls.Add(Me.BatchButton)
        Me.Controls.Add(Me.interactiveCmdText)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.randomizeButton)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.noPointsField)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.stopSeqField)
        Me.Controls.Add(Me.startSeqField)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.writeSeqButton)
        Me.Controls.Add(Me.noToAddField)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.AddExpsButton)
        Me.Controls.Add(Me.ExpSeqPanel)
        Me.Controls.Add(Me.StopButton)
        Me.Controls.Add(Me.StatusLabel)
        Me.Controls.Add(Me.ChooseProgramButton)
        Me.Controls.Add(Me.RunButton)
        Me.Name = "frmGUI"
        Me.Text = "ExpControl"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents RunButton As System.Windows.Forms.Button
    Friend WithEvents ChooseProgramButton As System.Windows.Forms.Button
    Friend WithEvents StatusLabel As System.Windows.Forms.Label
    Friend WithEvents OpenFileDialog As System.Windows.Forms.OpenFileDialog
    Friend WithEvents FolderBrowserDialog As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents StopButton As System.Windows.Forms.Button
    Friend WithEvents randomizeButton As System.Windows.Forms.Button
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents logarithmicRadio As System.Windows.Forms.RadioButton
    Friend WithEvents linearRadio As System.Windows.Forms.RadioButton
    Friend WithEvents noPointsField As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents stopSeqField As System.Windows.Forms.TextBox
    Friend WithEvents startSeqField As System.Windows.Forms.TextBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents writeSeqButton As System.Windows.Forms.Button
    Friend WithEvents noToAddField As System.Windows.Forms.TextBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents AddExpsButton As System.Windows.Forms.Button
    Friend WithEvents ExpSeqPanel As System.Windows.Forms.Panel
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents interactiveCmdText As System.Windows.Forms.TextBox
    Friend WithEvents BatchButton As System.Windows.Forms.Button
    Friend WithEvents StopBatchButton As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents LoopingPanel As System.Windows.Forms.Panel
    Friend WithEvents currentExpFileLabel As System.Windows.Forms.Label
    Friend WithEvents interGUI_Button As System.Windows.Forms.Button
    Friend WithEvents logExp_Button As System.Windows.Forms.Button
    Friend WithEvents AutoFill As System.Windows.Forms.Button
    Friend WithEvents RunVirtualButton As System.Windows.Forms.Button
    Friend WithEvents loadLog_button As System.Windows.Forms.Button
End Class

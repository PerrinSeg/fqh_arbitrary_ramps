Imports System.Text.RegularExpressions

Public Class frmGUI_autofill

    Dim gui As frmGUI
    Dim currentProgramName As String = ""
    Dim prevScript As String = ""

    Public Sub New(ByRef mygui As frmGUI)
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        'passed by reference
        gui = mygui

        'useful expressions to display
        Me.Text_useful_expr.Text = "Useful VB script expressions: (i Mod 3),int(x/y),..."

    End Sub

    Public Sub ShowAutofill(ByVal ss As String)

        'update fields only if running a different experiment
        If ss <> currentProgramName Then
            If currentProgramName <> "" Then
                'changed program, save previous script
                prevScript = Me.autofill_textBox.Text
            End If

            '(re)initial parameter values as in ExpVariables, update program name
            Me.autofill_textBox.Text = modUtilities.GetExpVariablesAllText()
            Me.N_updownBox.Value = 1
            currentProgramName = ss
        End If

        Me.ShowDialog()
        Me.Refresh()

    End Sub

    Private Sub Append_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Append_Button.Click

        'Number of experiments to append to current table
        Dim N As Integer = Me.N_updownBox.Value()

        Dim aufofill_text As String = Me.autofill_textBox.Text()

        Dim objSC As New MSScriptControl.ScriptControl
        objSC.Language = "VBScript"
        'objSC.Reset()

        'load the variable names into the ScriptControl
        Dim varList As ArrayList = modUtilities.GetExpVariables()
        Dim var As Object
        'For Each var In varList
        '    objSC.AddCode("Dim " & var.ToString())
        'Next
        'objSC.AddCode("Dim i")

        Try
            Dim i As Integer
            For i = 1 To N Step 1
                objSC.ExecuteStatement("i = " & i.ToString())
                objSC.ExecuteStatement(aufofill_text)
                Dim row As DataRow = gui.dt.NewRow()
                Dim j As Integer = 0
                For Each var In varList
                    row(j) = CDbl(objSC.Eval(var.ToString()))
                    j = j + 1
                Next
                gui.dt.Rows.Add(row)
            Next i
            'Me.Close()

        Catch exc As Exception
            MsgBox("Invalid VBScript code.", vbExclamation, "Error")
        End Try

    End Sub

    Private Sub Clear_table_button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Clear_table_button.Click
        gui.dt.Rows.Clear()
    End Sub

    Private Sub prevScript_button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles prevScript_button.Click
        'MessageBox.Show(prevScript, "Previously used script:", MessageBoxButtons.OK, MessageBoxIcon.None)
        If prevScript <> "" Then
            Me.autofill_textBox.Text = prevScript
        End If
    End Sub

    Private Sub defaultScript_button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles defaultScript_button.Click
        If Me.autofill_textBox.Text <> modUtilities.GetExpVariablesAllText Then
            prevScript = Me.autofill_textBox.Text
            Me.autofill_textBox.Text = modUtilities.GetExpVariablesAllText()
        End If
    End Sub

End Class
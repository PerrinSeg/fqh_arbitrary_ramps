Imports System.Text.RegularExpressions
Imports System.IO

Public Class frmGUI_interactive

    Dim gui As frmGUI

    Private _IsLoaded As Boolean = False
    Public Function IsLoaded() As Boolean
        Return _IsLoaded
    End Function

    Public Sub New(ByRef mygui As frmGUI)
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        Me.TopMost = True

        'pass clsInsteractive from frmGUI, passed by reference
        gui = mygui
    End Sub

    Public Sub LoadMacros()

        'first clear the MacroButtons
        For Each foundButton As Button In Me.Controls
            If (foundButton.Name.StartsWith("MacroButton")) Then
                Me.Controls.Remove(foundButton)
            End If
        Next

        Dim MacroIndex As Integer = 0

        For Each foundMacro As String In My.Computer.FileSystem.GetFiles(Path.Combine(modCodeGenerator.dynacode_dir, "macros"), Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, "*.txt")
            Dim ss As String()
            ss = Regex.Split(foundMacro, "\\")
            Dim MacroName As String = ss(ss.Length - 1)
            If MacroName.Substring(MacroName.Length - 4, 4) = ".txt" Then
                MacroName = MacroName.Remove(MacroName.Length - 4, 4)
                createMacroButton(MacroName, MacroIndex)
                MacroIndex += 1
            End If
        Next
        _IsLoaded = True

    End Sub

    Private Sub createMacroButton(ByVal Name As String, ByVal Index As Integer)
        Dim button As New Button()
        Me.Controls.Add(button)
        button.Name = "MacroButton_" & Index.ToString
        button.Text = Name
        button.Top = 100 + 55 * Math.Floor(Index / 3)
        button.Left = 50 + 150 * (Index Mod 3)
        button.Size = New System.Drawing.Size(100, 40)
        button.BackColor = Color.White

        AddHandler button.Click, AddressOf General_MacroButton_Click

    End Sub

    Private Sub ClearButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ClearButton.Click
        gui.inter.parse("clear")
        'undo all highlighting
        For Each ctl As Control In Me.Controls
            If (ctl.Name.StartsWith("MacroButton")) Then
                ctl.BackColor = Color.White
            End If
        Next ctl
    End Sub

    Private Sub General_MacroButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If sender.Name.StartsWith("MacroButton") Then
            gui.inter.parse(String.Concat("_", sender.Text))
            sender.BackColor = Color.GreenYellow
        End If
    End Sub

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        gui.inter.parse("clear")
        _IsLoaded = False
        'Hide instead of close
        Me.Hide()
        e.Cancel = True
    End Sub

    Private Sub ReloadChannelMapButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ReloadChannelMapButton.Click
        'MessageBox.Show("hi world")
        gui.inter.LoadChannelMap(modCodeGenerator.dynacode_dir + "\variables\ChannelsWithCard.txt")
    End Sub
End Class
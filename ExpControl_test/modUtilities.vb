Imports System.Text.RegularExpressions
Module modUtilities

    Public Function GetExpVariablesAllText() As String
        Dim text, s As String
        Dim t As String()
        's = My.Computer.FileSystem.ReadAllText(modMain.programLocation)
        s = My.Computer.FileSystem.ReadAllText(modMain.programLocation)
        t = Regex.Split(s, "'=====Variables=====")

        'clean up
        t = Regex.Split(t(1).Trim(), "\n")
        text = t(0).Trim()
        For i As Integer = 1 To (t.Length() - 1)
            text = text + vbNewLine + t(i).Trim()
        Next

        Return text
    End Function

    Public Function GetExpVariables() As ArrayList
        Dim expVariables As String = GetExpVariablesAllText()
        Dim s, t As String()
        s = Regex.Split(expVariables, "\n")
        Dim arrList As ArrayList
        arrList = New ArrayList()
        For i As Integer = 0 To (s.Length() - 1)
            t = Regex.Split(s(i), "=")
            arrList.Add(t(0))
        Next
        Return arrList
    End Function

    Public Function GetExpVariablesDefVals() As ArrayList
        Dim expVariables As String = GetExpVariablesAllText()
        Dim s, t As String()
        s = Regex.Split(expVariables, "\n")
        Dim arrList As ArrayList
        arrList = New ArrayList()
        For i As Integer = 0 To (s.Length() - 1)
            t = Regex.Split(s(i), "=")
            arrList.Add(t(1))
        Next
        Return arrList
    End Function

End Module

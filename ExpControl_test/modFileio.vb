Imports System
Imports System.IO

Module fileio
    Public Sub read1Darb(ByVal filename As String, ByVal wfmpoints() As Double)
        Try
            Dim sr As StreamReader
            sr = New StreamReader(filename)
            Dim line As String
            Dim length As Integer = 0
            Do
                line = sr.ReadLine()
                length = length + 1
            Loop
            sr = New StreamReader(filename)
            Dim nn As Integer
            ReDim wfmpoints(length)
            For nn = 1 To length
                wfmpoints(nn) = Val(sr.ReadLine())
            Next nn
            sr.Close()
        Catch E As Exception
            Console.WriteLine("The file could not be read:")
            Console.WriteLine(E.Message)
        End Try
    End Sub

    Public Function fixsign(ByVal number As Short) As Integer
        Dim retval As Integer = 0
        If number < 0 Then
            retval = retval + 32768
        End If
        retval = retval + (number And 32767)
        Return retval
    End Function

End Module

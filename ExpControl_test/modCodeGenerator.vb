Imports System.Text.RegularExpressions
Imports System.IO
Imports controldll
Imports controldll.SpectronServiceReference
Imports System.Collections.Generic
Imports System.Reflection

Module modCodeGenerator
    Dim experiment As Object
    Public dynacode_dir As String = Path.Combine(modMain.repo_dir, "dynacode")

    Function generateCode(ByVal programLocation As String) As Integer
        'Load files for dynamic code creation
        Dim header, footer, program, userprogram As String
        Dim channels, expConstants, expVariables, auxRoutines As String
        Dim variables As String = ""
        header = My.Computer.FileSystem.ReadAllText(Path.Combine(dynacode_dir, "backbone\header.txt"))
        footer = My.Computer.FileSystem.ReadAllText(Path.Combine(dynacode_dir, "backbone\footer.txt"))
        'MsgBox("(0) generate Code called sucessfully")
        'If the variables part is not found
        If InStr(My.Computer.FileSystem.ReadAllText(programLocation), "'=====Variables=====") = 0 Then
            MsgBox("Error: Invalid exp. variables in the program file.")
            Return -1
        End If
        'MsgBox("(1) valid exp variables")

        Dim userprogramAllSplit As String()
        userprogramAllSplit = Regex.Split(My.Computer.FileSystem.ReadAllText(programLocation), "'=====Variables=====")

        userprogram = userprogramAllSplit(0) + userprogramAllSplit(2)
        expVariables = userprogramAllSplit(1).Trim()

        'program = vbNewLine + "Public Sub build(ByRef cp As clsControlParams, " _
        '                                    & "ByRef analogdata As SpectronClient, ByRef analogdata2 As SpectronClient, ByRef analogdata3 As SpectronClient, " _
        '                                    & "ByRef digitaldata As digitalDAQdata, ByRef digitaldata2 As digitalDAQdata, " _
        '                                    & "ByRef gpib As GPIBControl, ByRef Hermes As KeithleyControl, ByRef dds As AD9959Ev)" + vbNewLine
        program = vbNewLine + "Public Sub build(ByRef cp As clsControlParams, " & "ByRef analogdata As SpectronClient)" + vbNewLine
        channels = My.Computer.FileSystem.ReadAllText(Path.Combine(dynacode_dir, "variables\ChannelsWithCard.txt"))
        expConstants = My.Computer.FileSystem.ReadAllText(Path.Combine(dynacode_dir, "variables\ExpConstants.txt"))
        auxRoutines = My.Computer.FileSystem.ReadAllText(Path.Combine(dynacode_dir, "variables\AuxRoutines.txt"))
        'MsgBox("(2) found everything needed in dynacode directory")

        'Declare variables and add experiment variables to hashtable
        Dim s, t, u As String()
        s = Regex.Split(channels, "\n")
        For i As Integer = 0 To (s.Length() - 1)
            If String.IsNullOrWhiteSpace(s(i)) <> True Then
                If (s(i).Contains("=")) Then
                    t = Regex.Split(s(i), "=")
                    u = Regex.Split(t(1), ",")  'discard card number
                    variables = variables + "Dim " + t(0) + " As Integer = " + u(0) + vbNewLine
                End If
            End If
        Next
        s = Regex.Split(expConstants, "\n")
        For i As Integer = 0 To (s.Length() - 1)
            If String.IsNullOrWhiteSpace(s(i)) <> True Then
                t = Regex.Split(s(i), "=")
                variables = variables + "Dim " + t(0) + " As Double = " + t(1) + vbNewLine
            End If
        Next
        s = Regex.Split(expVariables, "\n")
        For i As Integer = 0 To (s.Length() - 1)
            t = Regex.Split(s(i), "=")
            variables = variables + "Dim " + t(0) + " As Double = " + t(1) + vbNewLine
            program = program + "If cp.IsDefined(" + Chr(34) + t(0) + Chr(34) + ") Then" _
                        + vbNewLine + t(0) + "= cp.GetItem(" + Chr(34) + t(0) + Chr(34) + ")" _
                        + vbNewLine + "Else" + vbNewLine + "cp.Put(" + Chr(34) + t(0) + Chr(34) _
                        + "," + t(0) + ")" + vbNewLine + "End If" + vbNewLine
        Next

        program = program + userprogram + vbNewLine

        s = Regex.Split(auxRoutines, "\n")
        For i As Integer = 0 To (s.Length() - 1)
            program = program + s(i) + vbNewLine
        Next

        program = program + "End Sub" + vbNewLine

        'MsgBox("(3) split into subs")

        'Declare subroutines
        Dim subs As String = ""
        Dim subfiles As System.Collections.ObjectModel.ReadOnlyCollection(Of String) = My.Computer.FileSystem.GetFiles(Path.Combine(dynacode_dir, "subs"), Microsoft.VisualBasic.FileIO.SearchOption.SearchAllSubDirectories, "*.vb")
        Dim subfilesEnum As System.Collections.Generic.IEnumerator(Of String) = subfiles.GetEnumerator()
        While subfilesEnum.MoveNext()
            Dim currentSubfilename As String = subfilesEnum.Current()
            Dim currentSubcode As String = My.Computer.FileSystem.ReadAllText(currentSubfilename)

            Dim v As System.Text.RegularExpressions.Match
            Dim subName As String
            Dim w As System.Text.RegularExpressions.MatchCollection

            v = Regex.Match(currentSubcode, "((Protected|Friend|Private|Public)\s+){0,2}((Class|Sub|Function)\s+){1}([A-Za-z0-9]+)")
            subName = v.Groups(v.Groups.Count - 1).Value
            w = Regex.Matches(program, "(" + subName + ")+")

            'Only add the subroutine if it actually appears in the experiment file
            If w.Count > 0 Then
                currentSubcode = vbNewLine + currentSubcode + vbNewLine
                subs = subs + currentSubcode
            End If
        End While
        'MsgBox("(4) found all subs, cleaning up code")

        Dim code As String = header + variables + program + subs + footer
        'Clean up other return/newline types: replace with vbNewLine
        'Note: "\n" includes vbNewLine
        Dim codeLines As String() = Regex.Split(code, "\n")
        For i As Integer = 0 To codeLines.Length - 1
            codeLines(i) = codeLines(i).Trim()
        Next
        code = String.Join(vbNewLine, codeLines)
        'lines that match the line number in the errors
        codeLines = Regex.Split(code, vbNewLine)

        'My.Computer.FileSystem.WriteAllText(".\temp_vb\exp_code.vb", code, False)

        'MsgBox("(5) code clean, attempting to compile")

        'Compile the code
        Dim loCompiler As CodeDomProvider
        loCompiler = CodeDomProvider.CreateProvider("VisualBasic")
        Dim loParameters As CompilerParameters = New CompilerParameters()
        loParameters.ReferencedAssemblies.Add("C:\Users\Rb Lab\Documents\controldll\controldll\bin\x86\Debug\controldll.dll")
        loParameters.ReferencedAssemblies.Add("C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.dll")
        loParameters.ReferencedAssemblies.Add("C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.ServiceModel.dll")
        loParameters.GenerateInMemory = False
        'loParameters.TempFiles = New TempFileCollection(".\temp_vb", True)

        'MsgBox("(5.5) Added assemblies co loParameters, attempting CompileAssemblyFromSource")
        Dim loCompiled As CompilerResults = loCompiler.CompileAssemblyFromSource(loParameters, code)
        'Show compiler errors if available
        If (loCompiled.Errors.HasErrors) Then
            MsgBox("(6) loCompiled has an error (failed to compile)")
            Dim lcErrorMsg As String = ""
            lcErrorMsg = loCompiled.Errors.Count.ToString() + " Errors:"
            For x As Integer = 0 To Math.Min(loCompiled.Errors.Count - 1, 4)
                lcErrorMsg = lcErrorMsg + vbNewLine + vbNewLine + "Line: " + loCompiled.Errors(x).Line.ToString() + " - " + loCompiled.Errors(x).ErrorText
                If codeLines(loCompiled.Errors(x).Line - 1).Length > 100 Then
                    lcErrorMsg = lcErrorMsg + vbNewLine + "      [ " + Left$(codeLines(loCompiled.Errors(x).Line - 1), 100) + "   ... ]"
                Else
                    lcErrorMsg = lcErrorMsg + vbNewLine + "      [ " + codeLines(loCompiled.Errors(x).Line - 1) + " ]"
                End If
            Next
            MsgBox(lcErrorMsg)
            Return -1
        Else
            'Instantiate Experiment class 
            Dim loAssembly As Assembly = loCompiled.CompiledAssembly
            'MsgBox("(6) Able to compile")
            experiment = loAssembly.CreateInstance("Experiment")
            'MsgBox("experiment created")
        End If
        'MsgBox("(7) Finished loading experiment")
        Return 0
    End Function

    'Public Function runCode(ByRef cp As clsControlParams,
    '                    ByRef analogdata As SpectronClient, ByRef analogdata2 As SpectronClient, ByRef analogdata3 As SpectronClient,
    '                    ByRef digitaldata As digitalDAQdata, ByRef digitaldata2 As digitalDAQdata,
    '                    ByRef gpib As GPIBControl, ByRef Hermes As KeithleyControl, ByRef dds As AD9959Ev) As Object
    '    cp.Put("starter", 0.0)
    '    Dim loCodeParms() As Object = {cp, analogdata, analogdata2, analogdata3, digitaldata, digitaldata2, gpib, Hermes, dds}
    '    Try

    '        experiment.GetType().InvokeMember("build", BindingFlags.InvokeMethod, Nothing, experiment, loCodeParms)

    '        Return experiment
    '    Catch ex As Exception
    '        MsgBox(ex.Message)
    '        Return 0
    '    End Try
    'End Function

    'Public Function runCode(ByRef cp As clsControlParams,
    '                        ByRef analogdata As SpectronClient, ByRef analogdata2 As SpectronClient,
    '                        ByRef gpib As GPIBControl, ByRef Hermes As KeithleyControl, ByRef dds As AD9959Ev) As Object
    '    cp.Put("starter", 0.0)
    '    Dim loCodeParms() As Object = {cp, analogdata, analogdata2, gpib, Hermes, dds}
    '    Try

    '        experiment.GetType().InvokeMember("build", BindingFlags.InvokeMethod, Nothing, experiment, loCodeParms)

    '        Return experiment
    '    Catch ex As Exception
    '        MsgBox(ex.Message)
    '        Return 0
    '    End Try
    'End Function

    Public Function runCode(ByRef cp As clsControlParams,
                            ByRef analogdata As SpectronClient) As Object
        cp.Put("starter", 0.0)
        'MsgBox("run code called")
        Dim loCodeParms() As Object = {cp, analogdata}
        Try
            experiment.GetType().InvokeMember("build", BindingFlags.InvokeMethod, Nothing, experiment, loCodeParms)
            Return experiment
        Catch ex As Exception
            MsgBox("runCode failed to build experiment")
            MsgBox(ex.Message)
            MsgBox(ex.GetBaseException.Message)
            Return 0
        End Try
    End Function

    Public Sub logCode(ByVal programLocation As String, ByVal log_Dir As String, ByVal dt As DataTable)

        Dim userprogramName, logExpParam As String
        'Dim channels, expConstants, expVariables As String
        'Dim variables As String = ""
        'header = My.Computer.FileSystem.ReadAllText(rootpath + "backbone/header.txt")
        'footer = My.Computer.FileSystem.ReadAllText(rootpath + "backbone/footer.txt")

        Dim t As String()
        Dim sep(3) As Char
        sep(0) = "\"
        sep(1) = "."
        t = programLocation.Split(sep)
        userprogramName = t(t.GetLength(0) - 2)

        'Copy the files to data folder, without overwriting.
        My.Computer.FileSystem.CopyFile(programLocation, Path.Combine(log_Dir, userprogramName + ".txt"))
        My.Computer.FileSystem.CopyFile(Path.Combine(dynacode_dir, "variables", "ChannelsWithCard.txt"), Path.Combine(log_Dir, "ChannelsWithCard.txt"))
        My.Computer.FileSystem.CopyFile(Path.Combine(dynacode_dir, "variables", "ExpConstants.txt"), Path.Combine(log_Dir, "ExpConstants.txt"))
        My.Computer.FileSystem.CopyFile(Path.Combine(dynacode_dir, "variables", "AuxRoutines.txt"), Path.Combine(log_Dir, "AuxRoutines.txt"))
        'My.Computer.FileSystem.CopyFile(rootpath + "variables/ExpVariables.txt", log_Dir + "ExpVariables.txt")

        Dim subfiles As System.Collections.ObjectModel.ReadOnlyCollection(Of String) = My.Computer.FileSystem.GetFiles(Path.Combine(dynacode_dir, "subs"), Microsoft.VisualBasic.FileIO.SearchOption.SearchAllSubDirectories, "*.vb")
        Dim subfilesEnum As System.Collections.Generic.IEnumerator(Of String) = subfiles.GetEnumerator()
        While subfilesEnum.MoveNext()
            Dim currentSubfilename As String = subfilesEnum.Current()
            Dim currentSubcode As String = My.Computer.FileSystem.ReadAllText(currentSubfilename)
            My.Computer.FileSystem.CopyFile(currentSubfilename, Path.Combine(log_Dir, "subs", Path.GetFileName(currentSubfilename)))
        End While

        'Log the time, program
        Dim currentTime As String = System.DateTime.Now.ToString()
        logExpParam = "Experiment Log" + vbNewLine
        logExpParam = logExpParam + log_Dir + vbNewLine + vbNewLine
        logExpParam = logExpParam + currentTime + vbNewLine + vbNewLine
        logExpParam = logExpParam + "Experiment Program Used:" + vbNewLine + programLocation + vbNewLine + vbNewLine

        'Log batch parameters from gui.dt
        logExpParam = logExpParam + "Batch Length:  " + vbNewLine + dt.Rows.Count().ToString() + vbNewLine + vbNewLine
        logExpParam = logExpParam + "==========================" + vbNewLine

        Dim col As DataColumn
        For Each col In dt.Columns
            logExpParam = logExpParam + col.ColumnName().ToString() + ",  "
        Next col
        logExpParam = logExpParam + vbNewLine + "--------------------------" + vbNewLine

        Dim row As DataRow
        For Each row In dt.Rows
            For Each col In dt.Columns
                logExpParam = logExpParam + row(col).ToString() + ",  "
            Next col
            logExpParam = logExpParam + vbNewLine
        Next row

        Using outfile As New IO.StreamWriter(Path.Combine(log_Dir, "logExpParam.txt"), False)
            outfile.Write(logExpParam)
        End Using

    End Sub
End Module

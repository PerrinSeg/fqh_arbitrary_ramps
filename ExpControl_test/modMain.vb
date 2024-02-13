Imports controldll
Imports System.ServiceModel
Imports System.Threading
Imports System.Diagnostics
Imports controldll.SpectronServiceReference
Imports System.IO

Module modMain
    Public analogdata As SpectronClient
    'Public analogdata2 As SpectronClient
    'Public analogdata3 As SpectronClient

    'Public digitaltask As DIO64
    ' Public digitaltask2 As DIO64

    'Public interactiveDigital As DIO64
    'Public interactiveDigital2 As DIO64

    'Dim epAddress As New EndpointAddress("http://192.168.1.194:8000/SpectronService/Spectron/SpectronService")
    Dim epAddress As New EndpointAddress("http://192.168.1.239:8000/SpectronService/Spectron/SpectronService")
    ''Dim epAddress2 As New EndpointAddress("http://192.168.1.194:8000/SpectronService/Spectron/SpectronService")
    'Dim epAddress2 As New EndpointAddress("http://192.168.1.239:8000/SpectronService/Spectron/SpectronService")
    'if server is down this causes a problem
    'Dim expLogAddress As String = "Z:/Data"
    'temp for when server is down
    Dim expLogAddress As String = "C:\Users\Rb Lab\Documents"
    Public repo_dir As String = "..\..\..\"

    'Public digitalstate(3) As Short
    Public analogstate(31) As Double
    'Public digitalstate2(3) As Short
    'Public analogstate2(31) As Double
    'Public analogstate3(31) As Double
    'the bit shift values correspond to the channels (check 'exp_def.h' in 'ni653x.sln')
    Public validNIchan As Integer = (1 << 13) _
                                   + (1 << 15) _
                                   + (1 << 9) _
                                   + (1 << 11) _
                                   + (1 << 5) _
                                   + (1 << 7) _
                                   + (1 << 1) _
                                   + (1 << 3) _
                                   + (1 << 4) _
                                   + (1 << 2) _
                                   + (1 << 8) _
                                   + (1 << 6) _
                                   + (1 << 24) _
                                   + (1 << 22) _
                                   + (1 << 28) _
                                   + (1 << 26) _
                                   + (1 << 29) _
                                   + (1 << 31) _
                                   + (1 << 25) _
                                   + (1 << 27) _
                                   + (1 << 21) _
                                   + (1 << 23) _
                                   + (1 << 17) _
                                   + (1 << 19)


    Dim SetASmpClkRate_Hz As Integer = 10000 'Declare and initialize Analog Sample Clock Rate(PCI6733)
    Dim SetASmpClkRate2_Hz As Integer = 40000
    Public DSmpClkRate_Hz As Integer = 20000000 'SET IN HARDWARE Declare and initialize Digital Sample Clock Rate(Viewpoint Card) 
    Dim gui As New frmGUI

    Public gui_inter As New frmGUI_interactive(frmGUI)
    'Public gui_autofill As New frmGUI_autofill(frmGUI)

    Dim s As clsControlParamServer
    Dim cp As clsControlParams
    Public programLocation As String
    Private NIthread As Thread
    'Private NIthread2 As Thread
    'Private NIthread3 As Thread

    Private NITransPThread As Thread
    'Private NITransPThread2 As Thread
    'Private NITransPThread3 As Thread
    Dim experiment As Object

#Region "Declarations"
    'Dim DIO_board As Integer = 0

    Dim AClk_Divisor As Integer = (DSmpClkRate_Hz / SetASmpClkRate_Hz)
    Dim ASmpClkRate_Hz As Double = DSmpClkRate_Hz / AClk_Divisor
    'Dim AClk_Divisor2 As Integer = (DSmpClkRate_Hz / SetASmpClkRate2_Hz)
    'Dim ASmpClkRate2_Hz As Double = DSmpClkRate_Hz / AClk_Divisor2
    'make max_words an integer multiple of BLOCKSIZE/BPW.
    Dim BLOCKSIZE As Integer = 7650000
    Dim BPW As Integer = 34
    'Dim SMP_CLK As Integer = 25000
    Dim SMP_CLK As Integer = 12500
    Dim NUMVCOCALCYCLES As Integer = 3000
    Dim NEDITS As Integer = 50
    Dim header_words As Integer = NUMVCOCALCYCLES + 2 * NEDITS
    'BLOCKSIZE, BPW, NUMVCOCALCYCLES, NEDITS, and SMP_CLK all need to match what's defined in 'exp_def.h' in 'ni653x.sln'
    'SMP_CLK is measured in 1/ms.
    'Dim max_words As Long = BLOCKSIZE * 180 / BPW
    Dim max_words As Long = BLOCKSIZE * 90 / BPW
    Dim exp_words As Long = max_words - header_words
    'maximum experiment length based on 'max_words*BPW*1/SMP_CLK = 51.097784 seconds'
    '100 words are needed to  program the ad9522. 3000 words are needed for VCO 
    'calibration of the AD9522 (probably could do with fewer) and 
    Public interactive_tot_words As Double = header_words + 10
    '10 is an arbitrary small number. this is just the number of words for the interactive write.

    Dim sWatch As New System.Diagnostics.Stopwatch()

    'Dim gpib As GPIBControl
    'Dim Hermes As KeithleyControl
    'Dim dds As AD9959Ev

    'Dim digitaldata As digitalDAQdata
    'Dim digitaldata2 As digitalDAQdata
#End Region

    Sub main()
        'Dim stopWatch As New Stopwatch()
        'stopWatch.Start()
        Console.WriteLine("Starting program")
        'MsgBox("Started Program")
        Dim cb As New AsyncCallback(AddressOf ClientsConnected)
        Dim del As New startParamServerDelegate(AddressOf startParamServer)
        Dim binding As New WSHttpBinding()
        With binding
            .Name = "binding1"
            .HostNameComparisonMode = HostNameComparisonMode.StrongWildcard
            .ReliableSession.Enabled = True
            .ReliableSession.InactivityTimeout = New TimeSpan(4, 0, 0, 0, 0)
            .ReceiveTimeout = New TimeSpan(4, 0, 0, 0, 0)
        End With

        ''instead of this there is line of code that you are going to execute
        'stopWatch.Stop()
        ''Get the elapsed time as a TimeSpan value.
        'MsgBox(stopWatch.ElapsedMilliseconds)
        'MsgBox("instantiating spectron clients")

        Control.CheckForIllegalCrossThreadCalls = False
        'stopWatch.Start()
        analogdata = New SpectronClient(binding, epAddress)
        analogdata.Open()
        analogdata.allocate_NI_waveform(max_words)
        'MsgBox("created analog data 1")

        'analogdata2 = New SpectronClient(binding, epAddress)
        'analogdata2.Open()
        'analogdata2.allocate_NI_waveform(max_words)
        'MsgBox("created analog data 2")

        'analogdata3 = New SpectronClient(binding, epAddress2)
        'analogdata3.Open()
        'analogdata3.allocate_NI_waveform(max_words)
        'MsgBox(stopWatch.ElapsedMilliseconds)
        'MsgBox("allocating memory")

        del.BeginInvoke(cb, del)
        gui.ShowDialog()
    End Sub

    Sub startParamServer()
        s = New clsControlParamServer(0)
        cp = New clsControlParams()
    End Sub

    Delegate Sub startParamServerDelegate()

    Sub ClientsConnected(ByVal ar As IAsyncResult)
        If gui.InvokeRequired Then
            Dim d As New EnableProgramButtonCallback(AddressOf EnableProgramButton)
            gui.Invoke(d)
        Else
            gui.ChooseProgramButton.Enabled = True
            gui.StatusLabel.Text = "Load experiment program..."
            gui.Refresh()
        End If
    End Sub

    Delegate Sub EnableProgramButtonCallback()

    Private Sub EnableProgramButton()
        gui.ChooseProgramButton.Enabled = True
        gui.StatusLabel.Text = "Load experiment program..."
        gui.Refresh()
    End Sub

    Sub prepareToRun(ByVal fileName As String)
        programLocation = fileName

        'draw dt, dtloop
        gui.buildDataTables()
        gui.AutoFill.Enabled = True
        gui.AddExpsButton.Enabled = True
        gui.writeSeqButton.Enabled = True
        gui.randomizeButton.Enabled = True

        gui.RunButton.Enabled = True

        gui.RunVirtualButton.Enabled = True

        gui.StatusLabel.Text = "Ready to start next experiment..."
        Dim t As String()
        Dim sep(3) As Char
        sep(0) = "\"
        sep(1) = "."
        t = fileName.Split(sep)
        Dim x As String
        x = "Current experiment: "
        x = String.Concat(x, t(t.GetLength(0) - 2))
        gui.currentExpFileLabel.Text = x
        gui.Refresh()

    End Sub

    Sub experimentCompleted(ByVal ar As IAsyncResult)
        If gui.InvokeRequired Then
            Dim d As New expCompletedCallback(AddressOf expCompleted)
            gui.Invoke(d)
        Else
            gui.StatusLabel.Text = "Ready to start next experiment..."
            gui.ChooseProgramButton.Enabled = True
            gui.RunButton.Enabled = True

            gui.RunVirtualButton.Enabled = True

            gui.BatchButton.Enabled = False
            gui.Refresh()
        End If
    End Sub

    Delegate Sub expCompletedCallback()

    Private Sub expCompleted()
        gui.StatusLabel.Text = "Ready to start next experiment..."
        gui.ChooseProgramButton.Enabled = True
        gui.RunButton.Enabled = True

        gui.RunVirtualButton.Enabled = True

        gui.BatchButton.Enabled = False
        gui.Refresh()
    End Sub

    Sub runExperiment()
        Dim stopWatch As New Stopwatch()
        'stopWatch.Start()
        'MsgBox("run experiment")
        If (generateCode(programLocation) = -1) Then 'test compilation of the code to make sure there are no bugs in it
            MsgBox("failed to compile code")
            Return
        End If
        s.SendRunSignal()
        gui.runningState = gui.continuous
        gui.BatchButton.Enabled = True
        gui.RunButton.Enabled = False
        gui.RunVirtualButton.Enabled = False
        gui.StopButton.Enabled = True
        gui.ChooseProgramButton.Enabled = False

        'disable interactive cmds during experiment
        gui.interactiveCmdText.BackColor = Color.Gray
        gui.interactiveCmdText.Enabled = False
        gui.interGUI_Button.BackColor = Color.Gray
        gui.interGUI_Button.Enabled = False

        gui.Refresh()
        cp.Empty()
        Console.WriteLine("starting experiment")
        'interactiveDigital.Close()
        'interactiveDigital2.Close()

        'stopWatch.Start()
        'digitaltask = New DIO64("PXI7::0::INSTR", 0, DSmpClkRate_Hz, 1)
        'digitaltask2 = New DIO64("PXI7::1::INSTR", 1, DSmpClkRate_Hz, 0)
        'digitaltask.load()
        'digitaltask2.load()
        'stopWatch.Stop()
        'MsgBox("done with instantiating digital tasks")

        While True
            If gui.runningState = gui.batch Then
                gui.StatusLabel.Text = "Running batch..."
                If gui.currentExpNo < gui.dt.Rows.Count Then
                    gui.currentExpNo = gui.currentExpNo + 1
                    updateBatchHighlighting()
                    updateControlParamsForBatch()
                Else
                    gui.dgv.Rows(gui.currentExpNo - 1).DefaultCellStyle.BackColor = Color.White
                    gui.currentExpNo = 1
                    updateBatchHighlighting()
                    updateControlParamsForBatch()
                End If
            End If
            If gui.runningState = gui.continuous Then
                gui.StatusLabel.Text = "Running in loop..."
                updateLoopHighlighting()
                updateControlParamsForLoop()
            End If
            generateCode(programLocation)
            gui.Refresh()

            'digitaldata = New digitalDAQdata(DSmpClkRate_Hz)
            'digitaldata2 = New digitalDAQdata(DSmpClkRate_Hz)

            'gpib = New GPIBControl()
            'controller for Keithley generators
            'Hermes = New KeithleyControl()
            'dds = New AD9959Ev()

            'Console.WriteLine("initializing data")
            analogdata.initialize_data("/dev1/line0:31", max_words)
            'Console.WriteLine("data initialized. starting runCode")
            'analogdata2.initialize_data("/dev2/line0:31", max_words)
            'analogdata3.initialize_data("/dev3/line0:31", max_words)

            'dds.ClearArray()

            'dds.InitCommand()

            'experiment = runCode(cp, analogdata, analogdata2, analogdata3, digitaldata, digitaldata2, gpib, Hermes, dds)
            'experiment = runCode(cp, analogdata, analogdata2, gpib, Hermes, dds)
            experiment = runCode(cp, analogdata)


            'stopWatch.Stop()
            'MsgBox(stopWatch.ElapsedMilliseconds)
            'MsgBox("run code")
            'stopWatch.Start()  

            'shift all the digital data to account for the 'header_words' 
            'digitaldata.AddGlobalShift((header_words + 1 + 5) * BPW * 1 / SMP_CLK)
            'digitaldata2.AddGlobalShift((header_words + 1 + 5) * BPW * 1 / SMP_CLK)

            Dim totalTime As Double
            'totalTime = Math.Max(analogdata.GetTotalTime, digitaldata.GetTotalTime())
            totalTime = analogdata.GetTotalTime
            'totalTime = Math.Max(totalTime, digitaldata2.GetTotalTime())
            'totalTime = Math.Max(totalTime, analogdata2.GetTotalTime)
            'totalTime = Math.Max(totalTime, analogdata3.GetTotalTime)


            'Try
            '    dds.SendArray()
            'Catch ex As Exception
            '    'okay this is to try and reset arduino if something goes wrong
            '    'current shot will still be messed up
            'End Try
            'digitaldata2.AddPulse(62, totalTime - 1, totalTime)

            'digitaldata.AddPulse(5, header_words * BPW * 1 / SMP_CLK, totalTime) 'gets rid of glitch at runtime
            'make the experiment length an integer number of 'BLOCKSIZE' (which is a quantity of uInt32's)
            exp_words = Math.Ceiling((totalTime * SMP_CLK + header_words * BPW) / BLOCKSIZE + 2) * BLOCKSIZE / BPW

            NITransPThread = New Thread(AddressOf analogdata.transpose_data)
            'NITransPThread2 = New Thread(AddressOf analogdata2.transpose_data)
            'NITransPThread3 = New Thread(AddressOf analogdata3.transpose_data)
            NITransPThread.Start()
            'NITransPThread2.Start()
            'NITransPThread3.Start()
            NITransPThread.Join()
            'NITransPThread2.Join()
            'NITransPThread3.Join()

            'analogdata.transpose_data()
            'analogdata2.transpose_data()

            'analogdata3.transpose_data()

            'both analog cards are triggered on RTSI2 (trigger obtained from DIO64 cards)
            'analogdata sources clock from PFI4 and exports this clock on RTSI7
            'analogdata2 picks up clock from RTSI7
            'Console.WriteLine("configuring analogdata...")
            'analogdata.configure_653x("/dev1/line0:31", "RTSI2", "PFI4", exp_words)
            analogdata.configure_653x("/dev1/line0:31", Nothing, "OnboardClock", exp_words)
            'Console.WriteLine("...analogdata configured")
            'analogdata2.configure_653x("/dev2/line0:31", "RTSI2", Nothing, exp_words)
            'analogdata3.configure_653x("/dev3/line0:31", Nothing, "PFI4", exp_words)

            'digitaltask.Configure()
            'digitaltask2.Configure()

            'sWatch.Start()

            NIthread = New Thread(AddressOf analogdata.write_to_653x)
            'NIthread2 = New Thread(AddressOf analogdata2.write_to_653x)

            'NIthread3 = New Thread(AddressOf analogdata3.write_to_653x)
            NIthread.Start()
            'NIthread2.Start()
            'Console.WriteLine("after thread start")
            'NIthread3.Start()
            Thread.Sleep(100)
            'digitaltask2.Start(digitaldata2.data)
            'sWatch.Stop()

            'digitaltask.Start(digitaldata.data)
            'MsgBox("after digital task start")
            NIthread.Join()
            'NIthread2.Join()
            'NIthread3.Join()

            Thread.Sleep(100)
            'MsgBox("after sleep")
            s.SendControlParam(cp)
            'MsgBox("after send control param")

            analogdata.release_task()
            'analogdata2.release_task()
            'analogdata3.release_task()

            'digitaltask.Out_Stop()
            'digitaltask2.Out_Stop()

            If gui.runningState = gui.stopping Then
                s.SendMoreExpAvailable(False)
                Exit While
            Else
                If gui.runningState = gui.batchstopping Then
                    gui.runningState = gui.continuous
                    gui.StopBatchButton.Enabled = False
                    gui.BatchButton.Enabled = True
                    gui.dgv.Rows(gui.currentExpNo - 1).DefaultCellStyle.BackColor = Color.White
                    gui.currentExpNo = 0
                    gui.Refresh()
                End If
                s.SendMoreExpAvailable(True)
            End If

            'MsgBox(sWatch.ElapsedMilliseconds)
        End While
        gui.runningState = gui.stopped
        'digitaltask.Close()
        'digitaltask2.Close()

        'interactiveDigital.initialize()
        'interactiveDigital2.initialize()

        'enable interactive cmds
        gui.interactiveCmdText.Enabled = True
        gui.interactiveCmdText.BackColor = Color.White
        gui.interGUI_Button.Enabled = True
        gui.interGUI_Button.BackColor = Color.White
        gui.Refresh()

    End Sub

    Sub runBatch()
        gui.runningState = gui.batch
        gui.BatchButton.Enabled = False
        gui.StopBatchButton.Enabled = True
        gui.Refresh()
    End Sub

    'added oct 2011 alex: copy from runExperiment, for softscope
    Sub runVirtualExperiment()

        If (generateCode(programLocation) = -1) Then 'test compilation of the code to make sure there are no bugs in it
            Return
        End If

        'save folder name, saved to C:\softscope_output\..\.. 
        Dim saveFolderName As String = (InputBox("Save traces to a folder named:", "Softscope output folder", "default")).Trim()
        If saveFolderName.Length = 0 Then 'cancel pressed or empty input
            Return
        End If

        'just run once, i.e. equiv. to gui.runningState = gui.stopping
        gui.BatchButton.Enabled = False
        gui.RunButton.Enabled = False
        gui.RunVirtualButton.Enabled = False
        gui.RunVirtualButton.Text = "  Running..."
        gui.StopButton.Enabled = False
        gui.ChooseProgramButton.Enabled = False
        gui.StatusLabel.Text = "Running virtual experiment..."

        'disable interactive cmds during experiment
        gui.interactiveCmdText.BackColor = Color.Gray
        gui.interactiveCmdText.Enabled = False
        gui.interGUI_Button.BackColor = Color.Gray
        gui.interGUI_Button.Enabled = False

        'interactiveDigital.Close()
        'interactiveDigital2.Close()
        'interactiveAnalog.finish()
        'interactiveAnalog2.finish()

        generateCode(programLocation)
        gui.Refresh()

        'digitaldata = New digitalDAQdata(DSmpClkRate_Hz)
        'digitaldata2 = New digitalDAQdata(DSmpClkRate_Hz)
        'gpib = New GPIBControl()
        'controller for Keithley generators
        'Hermes = New KeithleyControl()

        'dds = New AD9959Ev()

        'runCode(cp, analogdata, analogdata2, analogdata3, digitaldata, digitaldata2, gpib, Hermes, dds)
        'runCode(cp, analogdata, analogdata2, gpib, Hermes, dds)
        runCode(cp, analogdata)

        Dim totalTime As Double
        'totalTime = Math.Max(exp_words * 34 * 40 / Math.Pow(10, 6), digitaldata.GetTotalTime())
        totalTime = exp_words * 34 * 40 / Math.Pow(10, 6)
        'totalTime = Math.Max(totalTime, digitaldata2.GetTotalTime())
        Dim numsamples As Integer = Int(totalTime * ASmpClkRate_Hz / 1000)
        'Dim numsamples2 As Integer = Int(totalTime * ASmpClkRate2_Hz / 1000)
        'digitaldata.AddPulse(5, 0, totalTime) 'gets rid of glitch at runtime

        gui.RunVirtualButton.Text = "  Writing..."

        'alex: softscope
        'Dim softScope As New clsSoftscope(totalTime, saveFolderName)
        'softScope.writeSoftscopeData(analogdata, analogdata2, digitaldata, digitaldata2, Hermes)

        gui.RunVirtualButton.Text = "  Finishing..."

        gui.runningState = gui.stopped
        'interactiveDigital.initialize()
        'interactiveDigital2.initialize()

        'enable interactive cmds
        gui.interactiveCmdText.Enabled = True
        gui.interactiveCmdText.BackColor = Color.White
        gui.interGUI_Button.Enabled = True
        gui.interGUI_Button.BackColor = Color.White

        gui.RunVirtualButton.Text = "  Run Virtual"
        gui.Refresh()
    End Sub

    'added dec28 2010 by alex: program logging
    Sub logExperiment(ByVal log_Dir As String)

        log_Dir = log_Dir + "\"
        logCode(programLocation, log_Dir, gui.dt)

    End Sub

    Delegate Sub runExperimentDelegate()

    Sub updateBatchHighlighting()
        gui.dgv.FirstDisplayedScrollingRowIndex = Math.Floor((gui.currentExpNo - 1) / 20.0) * 20
        gui.dgv.Rows(gui.currentExpNo - 1).DefaultCellStyle.BackColor = Color.LightGreen
        If gui.currentExpNo > 1 Then
            gui.dgv.Rows(gui.currentExpNo - 2).DefaultCellStyle.BackColor = Color.White
        End If
        gui.dgv.Refresh()
    End Sub

    Sub updateLoopHighlighting()
        gui.dgvloop.Rows(0).DefaultCellStyle.BackColor = Color.LightGreen
        gui.dgvloop.Refresh()
    End Sub

    Sub updateControlParamsForBatch()
        Dim arrList As ArrayList
        arrList = modUtilities.GetExpVariables()
        Dim var As Object
        Dim counter As Integer = 0
        For Each var In arrList
            cp.Put(var.ToString(), Double.Parse(gui.dt.Rows(gui.currentExpNo - 1).Item(counter).ToString()))
            counter = counter + 1
        Next
        logControlParams(programLocation, modCodeGenerator.dynacode_dir, gui.dt, gui.currentExpNo - 1)
    End Sub

    Sub updateControlParamsForLoop()
        Dim arrList As ArrayList
        arrList = modUtilities.GetExpVariables()
        Dim var As Object
        Dim counter As Integer = 0
        For Each var In arrList
            cp.Put(var.ToString(), Double.Parse(gui.dtloop.Rows(0).Item(counter).ToString()))
            counter = counter + 1
        Next
        logControlParams(programLocation, expLogAddress, gui.dt, 0)
    End Sub

    Sub logControlParams(ByVal programLocation As String, ByVal log_Dir As String, ByVal dt As DataTable, ByVal expNo As Integer)
        Dim userprogramName, logExpParam, varName As String
        Dim arrList As ArrayList
        arrList = modUtilities.GetExpVariables()
        Dim var As Object

        Dim t As String()
        Dim sep(3) As Char
        sep(0) = "\"
        sep(1) = "."
        t = programLocation.Split(sep)
        userprogramName = t(t.GetLength(0) - 2)
        logExpParam = userprogramName + vbNewLine

        For Each var In arrList
            varName = var.ToString()
            logExpParam = logExpParam + varName + " = " + cp.GetItem(varName).ToString() + vbNewLine
        Next

        Using outfile As New IO.StreamWriter(Path.Combine(log_Dir, "currentExpParameters.txt"))
            outfile.Write(logExpParam)
        End Using
    End Sub

End Module

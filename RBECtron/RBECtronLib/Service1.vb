'Service1.vb
Imports System
Imports System.ServiceModel
Imports System.Runtime.InteropServices
Imports System.Threading



Module modSpectron
    Const DLLpath As String = "C:\Users\Rb Lab\Documents\NI_653x_routines\ni653x\x64\Debug\ni653x.dll"

    <DllImport(DLLpath, EntryPoint:="configure_653x")> _
    Public Function WCFconfigure_653x(ByVal deviceName As String, ByVal trigger As String, ByVal clock As String, ByVal words As Long) As IntPtr
    End Function

    <DllImport(DLLpath, EntryPoint:="initialize_data")> _
    Public Sub WCFinitialize_data(ByVal deviceName As String, ByVal NI_waveform As IntPtr, ByVal max_words As Long)
    End Sub

    <DllImport(DLLpath, EntryPoint:="transpose_data")> _
    Public Sub WCFtranspose_data(ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath, EntryPoint:="write_to_653x")> _
    Public Function WCFwrite_to_653x(ByVal taskHandle As IntPtr, ByVal NI_waveform As IntPtr) As Integer
    End Function

    <DllImport(DLLpath, EntryPoint:="release_data")> _
    Public Sub WCFrelease_data()
    End Sub

    <DllImport(DLLpath, EntryPoint:="release_task")> _
    Public Sub WCFrelease_task(ByVal taskHandle As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub disable_clk_dist(ByVal t_start As Double, ByVal t_stop As Double, _
                                ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub set_dds_freq(ByVal dds_chan As Integer, ByVal freq As Double, _
                           ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub set_dds_freq_sweep(ByVal dds_chan As Integer, _
                           ByVal start_freq As Double, ByVal stop_freq As Double, _
                           ByVal sweep_duration As Double, _
                           ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub set_dds_freq_sweep_fast(ByVal dds_chan As Integer, _
                           ByVal start_freq As Double, ByVal stop_freq As Double, _
                           ByVal sweep_duration As Double, _
                           ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub set_freq(ByVal freq As Double, _
                           ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub set_freq_time(ByVal freq As Double, ByVal set_time As Double, _
                           ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub set_pllRN(ByVal RCounter As Integer, ByVal NCounter As Integer, _
                           ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub set_pllRN_time(ByVal RCounter As Integer, ByVal NCounter As Integer, ByVal set_time As Double, _
                           ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub set_voltage(ByVal voltage As Double, _
                           ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_step(ByVal step_volts As Double, _
                           ByVal t_start As Double, ByVal t_stop As Double, _
                           ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_exp(ByVal base_volts As Double, ByVal offset_volts As Double, _
                           ByVal t_start As Double, ByVal t_stop As Double, ByVal time_constant As Double, _
                           ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub


    <DllImport(DLLpath)> _
    Public Sub insert_exp_sane(ByVal v0 As Double, ByVal vf As Double, ByVal tau_ms As Double, _
                           ByVal t_start As Double, ByVal t_stop As Double, _
                           ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath, _
            CallingConvention:=CallingConvention.StdCall)> _
    Public Sub insert_exp_and_ramp(ByVal base_volts As Double, ByVal offset_volts As Double, _
                           ByVal t_start As Double, ByVal t_stop As Double, _
                           ByVal tramp As Double, ByVal time_const As Double, _
                           ByVal start_volts As Double, ByVal stop_volts As Double, _
                           ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_ramp(ByVal start_volts As Double, ByVal stop_volts As Double, _
                           ByVal t_start As Double, ByVal t_stop As Double, _
                           ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_tunnel_sine(ByVal offset_depth As Double, ByVal rel_amp As Double, _
                           ByVal freq_Hz As Double, ByVal phase As Double, _
                           ByVal t_start As Double, ByVal t_stop As Double, _
                           ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_tunnel_sine_ramp(ByVal offset_depth As Double, ByVal rel_amp As Double, _
                           ByVal freq_start As Double, ByVal freq_stop As Double, _
                           ByVal phase_start As Double, ByVal t_start As Double, ByVal t_stop As Double, _
                           ByVal calib_volt As Double, ByVal slope As Double, _
                           ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_axial_exp_ramp(ByVal start_2Ddepth As Double, ByVal stop_2Ddepth As Double, _
                           ByVal t_start As Double, ByVal t_stop As Double, _
                           ByVal axial_calib_depth As Double, ByVal axial_calib_volt As Double, _
                           ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_sine(ByVal offset As Double, ByVal amp As Double, ByVal freq As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)>
    Public Sub insert_ramp_log_sine3_return(ByVal lattice_depth As Double,
                    ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                    ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                    ByVal phase_pi As Double, ByVal t_start As Double, ByVal ramp_dur As Double, ByVal hold_dur As Double,
                    ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)>
    Public Sub insert_ramp_log_sine3_no_return(ByVal lattice_depth As Double,
                    ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                    ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                    ByVal phase_pi As Double, ByVal t_start As Double, ByVal ramp_dur As Double, ByVal hold_dur As Double,
                    ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)>
    Public Sub insert_ramp_log_sine3_phase_return(ByVal lattice_depth As Double, ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                    ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                    ByVal phase_pi_start As Double, ByVal phase_pi_stop As Double, ByVal phase_ramp_dur As Double,
                    ByVal t_start As Double, ByVal ramp_dur As Double, ByVal hold_dur As Double,
                    ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)>
    Public Sub insert_ramp_log_sine3_phase_no_return(ByVal lattice_depth As Double, ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                    ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                    ByVal phase_pi_start As Double, ByVal phase_pi_stop As Double, ByVal phase_ramp_dur As Double,
                    ByVal t_start As Double, ByVal ramp_dur As Double, ByVal hold_dur As Double,
                    ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_log_sine3(ByVal lattice_depth As Double, _
                    ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double, _
                    ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double, _
                    ByVal phase_pi As Double, ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)>
    Public Sub insert_log_sine3_phase(ByVal lattice_depth As Double,
                    ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                    ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                    ByVal phase_pi As Double, ByVal t_start As Double, ByVal t_stop As Double, ByVal t_ref As Double,
                    ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)>
    Public Sub insert_ramp_log_sine3(ByVal lattice_depth As Double,
                    ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                    ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                    ByVal phase_pi As Double, ByVal tunneling_init As Double, ByVal tunneling_final As Double, ByVal detuning_Hz_init As Double, ByVal detuning_Hz_final As Double,
                    ByVal t_start As Double, ByVal t_stop As Double, ByVal t_ref As Double,
                    ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_sine3(ByVal offset As Double, _
                    ByVal amp1 As Double, ByVal amp2 As Double, ByVal amp3 As Double, _
                    ByVal freq1 As Double, ByVal freq2 As Double, ByVal freq3 As Double, _
                    ByVal phase_pi As Double, ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_sine_phase(ByVal offset As Double, ByVal amp As Double, ByVal freq As Double, _
                    ByVal phase_pi As Double, ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_sine_flip(ByVal offset As Double, ByVal amp As Double, ByVal freq As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, ByVal fract As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_sine_ramp(ByVal offset As Double, ByVal amp As Double, _
                    ByVal freq_start As Double, ByVal freq_stop As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_log_ramp(ByVal start_volts As Double, ByVal stop_volts As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_log_ramp_red_dipole(ByVal red_start_volts As Double, ByVal red_start_freq As Double, _
                                          ByVal lattice_start_depth As Double, ByVal lattice_stop_depth As Double, _
                                          ByVal t_start As Double, ByVal t_stop As Double, _
                                          ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_tunneling_ramp(ByVal conversion_coeffs As Double(), _
                                     ByVal start_tunneling As Double, ByVal stop_tunneling As Double, _
                                     ByVal t_start As Double, ByVal t_stop As Double, _
                                     ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double, _
                                     ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_tunneling_gauge_ramp2(ByVal conversion_coeffs As Double(), _
                                            ByVal start_tunneling As Double, ByVal stop_tunneling As Double, _
                                            ByVal t_start As Double, ByVal t_stop As Double, _
                                            ByVal calib_volt As Double, ByVal calib_depth As Double, _
                                            ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

     <DllImport(DLLpath)> _
    Public Sub insert_tunneling_gauge_ramp(ByVal conversion_coeffs As Double(), _
                                           ByVal start_tunneling As Double, ByVal stop_tunneling As Double, _
                                           ByVal t_start As Double, ByVal t_stop As Double, _
                                           ByVal calib_volt As Double, ByVal calib_depth As Double, _
                                           ByVal dat_chan As Double, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)>
    Public Sub insert_interpolated_ramp_using_file(ByVal filename As String,
                    ByVal start_volts As Double, ByVal stop_volts As Double,
                    ByVal t_start As Double, ByVal t_stop As Double,
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_smooth_ramp(ByVal start_volts As Double, ByVal stop_volts As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_afm_ramp(ByVal start_volts As Double, ByVal stop_volts As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, ByVal t_ramp As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_smooth_box(ByVal start_volts As Double, ByVal stop_volts As Double, _
                    ByVal t_start As Double, ByVal t_dur As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Sub insert_s(ByVal background_volts As Double, ByVal final_volts As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)> _
    Public Function insert_ramp_red_dipole(ByVal red_start_volts As Double, ByVal red_start_freq As Double, _
                    ByVal lattice_start_depth As Double, ByVal lattice_stop_depth As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr) As Double
    End Function

    <DllImport(DLLpath)> _
    Public Function insert_ramp_red_dipole_bkwd(ByVal red_start_volts As Double, ByVal red_start_freq As Double, _
                    ByVal lattice_start_depth As Double, ByVal lattice_stop_depth As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr) As Double
    End Function

    <DllImport(DLLpath)> _
    Public Function insert_s_red_dipole(ByVal red_start_volts As Double, ByVal red_start_freq As Double, _
                    ByVal lattice_start_depth As Double, ByVal lattice_stop_depth As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr) As Double
    End Function

    <DllImport(DLLpath)>
    Public Sub insert_fancy_smooth_ramp(ByVal start_volts As Double, ByVal stop_volts As Double, _
                    ByVal alpha As Double, ByVal beta As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)>
    Public Sub insert_from_file(ByVal filename As String, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal max_current As Double, ByVal num_psu As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)>
    Public Sub insert_from_transport_file(ByVal filename As String, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal max_current As Double, ByVal num_psu As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)>
    Public Sub insert_from_binary_transport_file(ByVal filename As String, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer, ByVal NI_waveform As IntPtr)
    End Sub

    <DllImport(DLLpath)>
    Public Function get_total_time() As Double
    End Function

End Module

' NOTE: You can use the "Rename" command on the context menu to change the class name "Service1" in both code and config file together.
Public Class SpectronService
    Implements ISpectron
    Private _NI_waveform As IntPtr
    Private _TaskHandle As IntPtr
    Private _BPW As Integer = 34
    Private _NUMCHANNELS As Integer = 32
    '50 is just the maximum allowed number of registry edits. this number can be increased, but should match that in ni653x.c
    Private _n_edits As Integer = 50
    Private _card_number As Integer
    Private _errout As Integer
    Private _SMP_CLK As Integer
    Private _num_vco_cal_cycles = 3000 'taken from exp_def.h for use with ni653x.c

    Public Sub allocate_NI_waveform(ByVal words As Long) Implements ISpectron.allocate_NI_waveform
        'Dim total_bytes As Int64 = (2 * _n_edits + _num_vco_cal_cycles + total_cycles) * _BPW * Marshal.SizeOf(GetType(UInteger)) 'this is a hack.
        Dim total_bytes As Int64 = words * _BPW * Marshal.SizeOf(GetType(UInteger))
        'Console.WriteLine("words: {0}", words)
        'Console.WriteLine("size: {0}", Marshal.SizeOf(GetType(UInteger)))
        'Console.WriteLine("bpw: {0}", _BPW)
        Console.WriteLine("total: {0}", total_bytes)
        Dim myp As New IntPtr(total_bytes)
        _NI_waveform = Marshal.AllocHGlobal(myp)

        Console.WriteLine("allocating memory in spectronservice")
    End Sub

    Public Sub configure_653x(ByVal deviceName As String, ByVal trigger As String, ByVal clock As String, ByVal words As Long) Implements ISpectron.configure_653x
        Dim TaskHandle As IntPtr
        'Dim nsamples As Int64 = 2 * _n_edits + _num_vco_cal_cycles + total_cycles 'this is a hack.
        TaskHandle = WCFconfigure_653x(deviceName, trigger, clock, words)
        _TaskHandle = TaskHandle
    End Sub

    Public Sub initialize_data(ByVal deviceName As String, ByVal max_words As Long) Implements ISpectron.initialize_data
        Console.WriteLine("Service1 initialize_data, words: {0}", max_words)
        'MsgBox("Service1 initialize_data:\n\tdevice: {1}; words: {0}", max_words, deviceName)
        WCFinitialize_data(deviceName, _NI_waveform, max_words)
        _card_number = Integer.Parse(deviceName(4))
        'Console.WriteLine("card_number: {0}", _card_number)
    End Sub

    Public Sub transpose_data() Implements ISpectron.transpose_data
        WCFtranspose_data(_NI_waveform)
    End Sub

    Public Function write_to_653x() As Integer Implements ISpectron.write_to_653x
        Dim retVal As Integer
        Console.WriteLine("Service1 starting write_to_653x()")
        retVal = WCFwrite_to_653x(_TaskHandle, _NI_waveform)
        Console.WriteLine("...finished write_to_653x()")
        _errout = retVal
        Return retVal
    End Function

    Public Sub release_data() Implements ISpectron.release_data
        Console.WriteLine("deallocating memory in spectronservice")
        Marshal.FreeHGlobal(_NI_waveform)
        WCFrelease_data()
        Console.WriteLine("...done deallocating memory in spectronservice")
    End Sub

    Public Sub release_task() Implements ISpectron.release_task
        WCFrelease_task(_TaskHandle)
    End Sub

    Public Sub DisableClkDist(ByVal t_start As Double, ByVal t_stop As Double) Implements ISpectron.DisableClkDist
        disable_clk_dist(t_start, t_stop, _NI_waveform)
    End Sub


    Public Sub SetDDSFreqSweep(ByVal dds_chan As Integer,
                           ByVal start_freq As Double, ByVal stop_freq As Double, _
                           ByVal sweep_duration As Double, _
                           ByVal dat_chan As Integer) Implements ISpectron.SetDDSFreqSweep
        set_dds_freq_sweep(dds_chan, start_freq, stop_freq, _
                            sweep_duration, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub SetDDSFreqSweepFast(ByVal dds_chan As Integer, _
                           ByVal start_freq As Double, ByVal stop_freq As Double, _
                           ByVal sweep_duration As Double, _
                           ByVal dat_chan As Integer) Implements ISpectron.SetDDSFreqSweepFast
        set_dds_freq_sweep_fast(dds_chan, start_freq, stop_freq, _
                            sweep_duration, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub SetDDSFreq(ByVal dds_chan As Integer, ByVal freq As Double, _
                           ByVal dat_chan As Integer) Implements ISpectron.SetDDSFreq
        set_dds_freq(dds_chan, freq, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub SetFreq(ByVal freq As Double, _
                           ByVal dat_chan As Integer) Implements ISpectron.SetFreq
        set_freq(freq, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
        'Console.WriteLine("card_number: {0}", _card_number)
        'Console.WriteLine("dat_chan: {0}", dat_chan)
    End Sub
    Public Sub SetFreqTime(ByVal freq As Double, ByVal set_time As Double, _
                        ByVal dat_chan As Integer) Implements ISpectron.SetFreqTime
        set_freq_time(freq, set_time, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub SetPLLRN(ByVal RCounter As Integer, ByVal NCounter As Integer, _
                           ByVal dat_chan As Integer) Implements ISpectron.SetPLLRN
        set_pllRN(RCounter, NCounter, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub SetPLLRNTime(ByVal RCounter As Integer, ByVal NCounter As Integer, ByVal set_time As Double, _
                       ByVal dat_chan As Integer) Implements ISpectron.SetPLLRNTime
        set_pllRN_time(RCounter, NCounter, set_time, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub SetVoltage(ByVal voltage As Double, _
                           ByVal dat_chan As Integer) Implements ISpectron.SetVoltage
        'Console.WriteLine("setting voltage on card number, {0}", _card_number)
        set_voltage(voltage, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddStep(ByVal step_volts As Double, _
                           ByVal t_start As Double, ByVal t_stop As Double, _
                           ByVal dat_chan As Integer) Implements ISpectron.AddStep
        'Console.WriteLine("add step on channel, {0}", dat_chan)
        insert_step(step_volts, _
                            t_start, t_stop, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddDebugStep(ByVal step_volts As Double, _
                           ByVal t_start As Double, ByVal t_stop As Double, _
                           ByVal dat_chan As Integer) Implements ISpectron.AddDebugStep

        insert_step(step_volts, _
                            t_start, t_stop, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddExpSane(ByVal v0 As Double, ByVal vf As Double, ByVal tau_ms As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddExpSane
        insert_exp_sane(v0, vf, tau_ms, _
                            t_start, t_stop, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddExp(ByVal base_volts As Double, ByVal offset_volts As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, ByVal time_constant As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddExp
        insert_exp(base_volts, offset_volts, _
                            t_start, t_stop, time_constant, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddExpAndRamp(ByVal base_volts As Double, ByVal offset_volts As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal tramp As Double, ByVal time_const As Double, _
                    ByVal start_volts As Double, ByVal stop_volts As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddExpAndRamp
        insert_exp_and_ramp(base_volts, offset_volts, _
                            t_start, t_stop, _
                            tramp, time_const, _
                            start_volts, stop_volts, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddRamp(ByVal start_volts As Double, ByVal stop_volts As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddRamp
        insert_ramp(start_volts, stop_volts, t_start, t_stop, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddTunnelSine(ByVal offset_depth As Double, ByVal rel_amp As Double, _
                    ByVal freq_Hz As Double, ByVal phase As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddTunnelSine
        insert_tunnel_sine(offset_depth, rel_amp, _
                            freq_Hz, phase, _
                            t_start, t_stop, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddTunnelSineRamp(ByVal offset_depth As Double, ByVal rel_amp As Double, _
                    ByVal freq_start As Double, ByVal freq_stop As Double, _
                    ByVal phase_start As Double, ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal calib_volt As Double, ByVal slope As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddTunnelSineRamp
        insert_tunnel_sine_ramp(offset_depth, rel_amp, freq_start, _
                            freq_stop, phase_start, _
                            t_start, t_stop, _
                            calib_volt, slope, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddAxialExpRamp(ByVal start_2Ddepth As Double, ByVal stop_2Ddepth As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal axial_calib_depth As Double, ByVal axial_calib_volt As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddAxialExpRamp
        insert_axial_exp_ramp(start_2Ddepth, stop_2Ddepth, _
                            t_start, t_stop, _
                            axial_calib_depth, axial_calib_volt, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)

    End Sub

    Public Sub AddSine(ByVal offset As Double, ByVal amp As Double, ByVal freq As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddSine
        insert_sine(offset, amp, freq, _
                            t_start, t_stop, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddRampLogSine3Return(ByVal lattice_depth As Double,
                    ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                    ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                    ByVal phase_pi As Double, ByVal t_start As Double, ByVal ramp_dur As Double, ByVal hold_dur As Double,
                    ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                    ByVal dat_chan As Integer) Implements ISpectron.AddRampLogSine3Return
        insert_ramp_log_sine3_return(lattice_depth, rel_amp_1, rel_amp_2, rel_amp_3,
                            freq_Hz_1, freq_Hz_2, freq_Hz_3,
                            phase_pi, t_start, ramp_dur, hold_dur,
                            voltage_offset, calib_volt, calib_depth,
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddRampLogSine3NoReturn(ByVal lattice_depth As Double,
                    ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                    ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                    ByVal phase_pi As Double, ByVal t_start As Double, ByVal ramp_dur As Double, ByVal hold_dur As Double,
                    ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                    ByVal dat_chan As Integer) Implements ISpectron.AddRampLogSine3NoReturn
        insert_ramp_log_sine3_no_return(lattice_depth, rel_amp_1, rel_amp_2, rel_amp_3,
                            freq_Hz_1, freq_Hz_2, freq_Hz_3,
                            phase_pi, t_start, ramp_dur, hold_dur,
                            voltage_offset, calib_volt, calib_depth,
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddRampLogSine3PhaseReturn(ByVal lattice_depth As Double, ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                    ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                    ByVal phase_pi_start As Double, ByVal phase_pi_stop As Double, ByVal phase_ramp_dur As Double,
                    ByVal t_start As Double, ByVal ramp_dur As Double, ByVal hold_dur As Double,
                    ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                    ByVal dat_chan As Integer) Implements ISpectron.AddRampLogSine3PhaseReturn
        insert_ramp_log_sine3_phase_return(lattice_depth, rel_amp_1, rel_amp_2, rel_amp_3,
                            freq_Hz_1, freq_Hz_2, freq_Hz_3,
                            phase_pi_start, phase_pi_stop, phase_ramp_dur,
                            t_start, ramp_dur, hold_dur,
                            voltage_offset, calib_volt, calib_depth,
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddRampLogSine3PhaseNoReturn(ByVal lattice_depth As Double, ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                    ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                    ByVal phase_pi_start As Double, ByVal phase_pi_stop As Double, ByVal phase_ramp_dur As Double,
                    ByVal t_start As Double, ByVal ramp_dur As Double, ByVal hold_dur As Double,
                    ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                    ByVal dat_chan As Integer) Implements ISpectron.AddRampLogSine3PhaseNoReturn
        insert_ramp_log_sine3_phase_return(lattice_depth, rel_amp_1, rel_amp_2, rel_amp_3,
                            freq_Hz_1, freq_Hz_2, freq_Hz_3,
                            phase_pi_start, phase_pi_stop, phase_ramp_dur,
                            t_start, ramp_dur, hold_dur,
                            voltage_offset, calib_volt, calib_depth,
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddLogSine3(ByVal lattice_depth As Double, _
                    ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double, _
                    ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double, _
                    ByVal phase_pi As Double, ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddLogSine3
        insert_log_sine3(lattice_depth, rel_amp_1, rel_amp_2, rel_amp_3, _
                         freq_Hz_1, freq_Hz_2, freq_Hz_3, _
                         phase_pi, t_start, t_stop, _
                         voltage_offset, calib_volt, calib_depth, _
                         dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddLogSine3Phase(ByVal lattice_depth As Double,
                    ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                    ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                    ByVal phase_pi As Double, ByVal t_start As Double, ByVal t_stop As Double, ByVal t_ref As Double,
                    ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                    ByVal dat_chan As Integer) Implements ISpectron.AddLogSine3Phase
        insert_log_sine3_phase(lattice_depth, rel_amp_1, rel_amp_2, rel_amp_3,
                               freq_Hz_1, freq_Hz_2, freq_Hz_3,
                               phase_pi, t_start, t_stop, t_ref,
                               voltage_offset, calib_volt, calib_depth,
                               dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddRampLogSine3(ByVal lattice_depth As Double,
                    ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                    ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                    ByVal phase_pi As Double, ByVal tunneling_init As Double, ByVal tunneling_final As Double, ByVal detuning_Hz_init As Double, ByVal detuning_Hz_final As Double,
                    ByVal t_start As Double, ByVal t_stop As Double, ByVal t_ref As Double,
                    ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                    ByVal dat_chan As Integer) Implements ISpectron.AddRampLogSine3
        insert_ramp_log_sine3(lattice_depth, rel_amp_1, rel_amp_2, rel_amp_3,
                              freq_Hz_1, freq_Hz_2, freq_Hz_3,
                              phase_pi, tunneling_init, tunneling_final, 
                              detuning_Hz_init, detuning_Hz_final,
                              t_start, t_stop, t_ref,
                              voltage_offset, calib_volt, calib_depth,
                              dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddSine3(ByVal offset As Double, _
                    ByVal amp1 As Double, ByVal amp2 As Double, ByVal amp3 As Double, _
                    ByVal freq1 As Double, ByVal freq2 As Double, ByVal freq3 As Double, _
                    ByVal phase_pi As Double, ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddSine3
        insert_sine3(offset, amp1, amp2, amp3, _
                     freq1, freq2, freq3, _
                     phase_pi, t_start, t_stop, _
                     dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddSinePhase(ByVal offset As Double, ByVal amp As Double, ByVal freq As Double, _
                    ByVal phase_pi As Double, ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddSinePhase
        insert_sine_phase(offset, amp, freq, _
                          phase_pi, t_start, t_stop, _
                          dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddSineFlip(ByVal offset As Double, ByVal amp As Double, ByVal freq As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, ByVal fract As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddSineFlip
        insert_sine_flip(offset, amp, freq, _
                         t_start, t_stop, fract, _
                         dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddSineRamp(ByVal offset As Double, ByVal amp As Double, _
                    ByVal freq_start As Double, ByVal freq_stop As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddSineRamp
        insert_sine_ramp(offset, amp, _
                         freq_start, freq_stop, _
                         t_start, t_stop, _
                         dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddLogRamp(ByVal start_volts As Double, ByVal stop_volts As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddLogRamp
        insert_log_ramp(start_volts, stop_volts, _
                        t_start, t_stop, _
                        dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddLogRampRedDipole(ByVal red_start_volts As Double, ByVal red_start_freq As Double, _
                    ByVal lattice_start_depth As Double, ByVal lattice_stop_depth As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddLogRampRedDipole
        insert_log_ramp_red_dipole(red_start_volts, red_start_freq, _
                            lattice_start_depth, lattice_stop_depth, _
                            t_start, t_stop, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddTunnelRamp(ByVal conversion_coeffs As Double(), _
                             ByVal start_tunneling As Double, ByVal stop_tunneling As Double, _
                             ByVal t_start As Double, ByVal t_stop As Double, _
                             ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double, _
                             ByVal dat_chan As Integer) Implements ISpectron.AddTunnelRamp
        insert_tunneling_ramp(conversion_coeffs, _
                              start_tunneling, stop_tunneling, _
                              t_start, t_stop, _
                              voltage_offset, calib_volt, calib_depth, _
                              dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

     Public Sub AddTunnelGaugeRamp2(ByVal conversion_coeffs As Double(), _
                                    ByVal start_tunneling As Double, ByVal stop_tunneling As Double, _
                                    ByVal t_start As Double, ByVal t_stop As Double, _
                                    ByVal calib_volt As Double, ByVal calib_depth As Double, _
                                    ByVal dat_chan As Integer) Implements ISpectron.AddTunnelGaugeRamp2
        insert_tunneling_gauge_ramp2(conversion_coeffs, _
                                     start_tunneling, stop_tunneling, _
                                     t_start, t_stop, _
                                     calib_volt, calib_depth, _
                                     dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddTunnelGaugeRamp(ByVal conversion_coeffs As Double(), _
                                  ByVal start_tunneling As Double, ByVal stop_tunneling As Double, _
                                  ByVal t_start As Double, ByVal t_stop As Double, _
                                  ByVal calib_volt As Double, ByVal calib_depth As Double, _
                                  ByVal dat_chan As Integer) Implements ISpectron.AddTunnelGaugeRamp
        insert_tunneling_gauge_ramp(conversion_coeffs, _
                                    start_tunneling, stop_tunneling, _
                                    t_start, t_stop, _
                                    calib_volt, calib_depth, _
                                    dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddInterpolatedRampUsingFile(ByVal filename As String,
                    ByVal start_volts As Double, ByVal stop_volts As Double,
                    ByVal t_start As Double, ByVal t_stop As Double,
                    ByVal dat_chan As Integer) Implements ISpectron.AddInterpolatedRampUsingFile
        insert_interpolated_ramp_using_file(filename,
                            start_volts, stop_volts,
                            t_start, t_stop,
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddSmoothRamp(ByVal start_volts As Double, ByVal stop_volts As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddSmoothRamp
        insert_smooth_ramp(start_volts, stop_volts, _
                           t_start, t_stop, _
                           dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub


    Public Sub AddAFMRamp(ByVal start_volts As Double, ByVal stop_volts As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, ByVal t_ramp As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddAFMRamp
        insert_afm_ramp(start_volts, stop_volts, _
                        t_start, t_stop, t_ramp, _
                        dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddSmoothBox(ByVal start_volts As Double, ByVal stop_volts As Double, _
                    ByVal t_start As Double, ByVal t_dur As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddSmoothBox
        'Console.WriteLine("insert smooth box routine")
        insert_smooth_box(start_volts, stop_volts, _
                            t_start, t_dur, t_stop, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddS(ByVal background_volts As Double, ByVal final_volts As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer) Implements ISpectron.AddS
        insert_s(background_volts, final_volts, _
                 t_start, t_stop, _
                 dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Function AddRampRedDipole(ByVal red_start_volts As Double, ByVal red_start_freq As Double, _
                    ByVal lattice_start_depth As Double, ByVal lattice_stop_depth As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer) As Double Implements ISpectron.AddRampRedDipole
        Dim end_dipole_voltage As Double = insert_ramp_red_dipole(red_start_volts, red_start_freq, _
                            lattice_start_depth, lattice_stop_depth, _
                            t_start, t_stop, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
        Return end_dipole_voltage
    End Function

    Public Function AddRampRedDipoleBkwd(ByVal red_start_volts As Double, ByVal red_start_freq As Double, _
                    ByVal lattice_start_depth As Double, ByVal lattice_stop_depth As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer) As Double Implements ISpectron.AddRampRedDipoleBkwd
        Dim end_dipole_voltage As Double = insert_ramp_red_dipole_bkwd(red_start_volts, red_start_freq, _
                            lattice_start_depth, lattice_stop_depth, _
                            t_start, t_stop, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
        Return end_dipole_voltage
    End Function

    Public Function AddSRedDipole(ByVal red_start_volts As Double, ByVal red_start_freq As Double, _
                    ByVal lattice_start_depth As Double, ByVal lattice_stop_depth As Double, _
                    ByVal t_start As Double, ByVal t_stop As Double, _
                    ByVal dat_chan As Integer) As Double Implements ISpectron.AddSRedDipole
        Dim end_dipole_voltage As Double = insert_s_red_dipole(red_start_volts, red_start_freq, _
                            lattice_start_depth, lattice_stop_depth, _
                            t_start, t_stop, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
        Return end_dipole_voltage
    End Function

    Public Sub AddFancySmoothRamp(ByVal start_volts As Double, ByVal stop_volts As Double, _
                ByVal alpha As Double, ByVal beta As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer) Implements ISpectron.AddFancySmoothRamp
        insert_fancy_smooth_ramp(start_volts, stop_volts, _
                            alpha, beta, _
                            t_start, t_stop, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub
    Public Sub AddFromFile(ByVal filename As String, _
            ByVal t_start As Double, ByVal t_stop As Double, _
            ByVal max_current As Double, ByVal num_psu As Double, _
            ByVal dat_chan As Integer) Implements ISpectron.AddFromFile
        insert_from_file(filename, _
                            t_start, t_stop, _
                            max_current, num_psu, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddFromTransportFile(ByVal filename As String, _
        ByVal t_start As Double, ByVal t_stop As Double, _
        ByVal max_current As Double, ByVal num_psu As Double, _
        ByVal dat_chan As Integer) Implements ISpectron.AddFromTransportFile
        insert_from_transport_file(filename, _
                            t_start, t_stop, _
                            max_current, num_psu, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Sub AddFromBinaryTransportFile(ByVal filename As String, _
        ByVal t_start As Double, ByVal t_stop As Double, _
        ByVal dat_chan As Integer) Implements ISpectron.AddFromBinaryTransportFile
        insert_from_binary_transport_file(filename, _
                            t_start, t_stop, _
                            dat_chan + _NUMCHANNELS * (_card_number - 1), _NI_waveform)
    End Sub

    Public Function Add(ByVal n1 As Double, ByVal n2 As Double) As Double Implements ISpectron.Add
        Dim result As Double = n1 + n2
        ' Code added to write output to the console window.
        Console.WriteLine("Received Add({0},{1})", n1, n2)
        Console.WriteLine("Return: {0}", result)
        Return result
    End Function

    'Public ReadOnly Property errout As Integer
    '    Get
    '        Return _errout
    '    End Get
    'End Property

    Public Function GetTotalTime() As Double Implements ISpectron.GetTotalTime
        Dim last_time As Double
        last_time = get_total_time()
        Return last_time
    End Function
End Class

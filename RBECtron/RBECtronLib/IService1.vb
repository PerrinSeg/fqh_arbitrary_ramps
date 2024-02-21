'IService.vb
Imports System
Imports System.ServiceModel

' NOTE: You can use the "Rename" command on the context menu to change the interface name "IService1" in both code and config file together.
<ServiceContract()>
Public Interface ISpectron

    <OperationContract()> _
    Sub allocate_NI_waveform(ByVal words As Long)

    <OperationContract()> _
    Sub configure_653x(ByVal deviceName As String, ByVal trigger As String, ByVal clock As String, ByVal words As Long)

    <OperationContract()> _
    Sub initialize_data(ByVal deviceName As String, ByVal max_words As Long)

    <OperationContract()> _
    Sub transpose_data()

    <OperationContract()> _
    Function write_to_653x() As Integer

    <OperationContract()> _
    Sub release_data()

    <OperationContract()> _
    Sub release_task()

    <OperationContract()> _
    Sub DisableClkDist(ByVal t_start As Double, ByVal t_stop As Double)

    <OperationContract()> _
    Sub SetDDSFreqSweep(ByVal dds_chan As Integer, _
                ByVal start_freq As Double, ByVal stop_freq As Double, _
                ByVal sweep_duration As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub SetDDSFreqSweepFast(ByVal dds_chan As Integer, _
                ByVal start_freq As Double, ByVal stop_freq As Double, _
                ByVal sweep_duration As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub SetDDSFreq(ByVal dds_chan As Integer, ByVal voltage As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub SetFreq(ByVal freq As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub SetFreqTime(ByVal freq As Double, ByVal set_time As Double, _
                    ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub SetPLLRN(ByVal RCounter As Integer, ByVal NCounter As Integer, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub SetPLLRNTime(ByVal RCounter As Integer, ByVal NCounter As Integer, ByVal set_time As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub SetVoltage(ByVal voltage As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddStep(ByVal step_volts As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddDebugStep(ByVal step_volts As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddExp(ByVal base_volts As Double, ByVal offset_volts As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, ByVal time_constant As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddExpSane(ByVal v0 As Double, ByVal vf As Double, ByVal tau_ms As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddExpAndRamp(ByVal base_volts As Double, ByVal offset_volts As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal tramp As Double, ByVal time_const As Double, _
                ByVal start_volts As Double, ByVal stop_volts As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddRamp(ByVal start_volts As Double, ByVal stop_volts As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddTunnelSine(ByVal offset_depth As Double, ByVal rel_amp As Double, _
                ByVal freq_Hz As Double, ByVal phase As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddTunnelSineRamp(ByVal offset_depth As Double, ByVal rel_amp As Double, _
                ByVal freq_start As Double, ByVal freq_stop As Double, _
                ByVal phase_start As Double, ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal calib_volt As Double, ByVal slope As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddAxialExpRamp(ByVal start_2Ddepth As Double, ByVal stop_2Ddepth As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal axial_calib_depth As Double, ByVal axial_calib_volt As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddSine(ByVal offset As Double, ByVal amp As Double, ByVal freq As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()>
    Sub AddRampLogSine3Return(ByVal lattice_depth As Double,
                ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                ByVal phase_pi As Double, ByVal t_start As Double, ByVal ramp_dur As Double, ByVal hold_dur As Double,
                ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                ByVal dat_chan As Integer)

    <OperationContract()>
    Sub AddRampLogSine3NoReturn(ByVal lattice_depth As Double,
                ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                ByVal phase_pi As Double, ByVal t_start As Double, ByVal ramp_dur As Double, ByVal hold_dur As Double,
                ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                ByVal dat_chan As Integer)

    <OperationContract()>
    Sub AddRampLogSine3PhaseReturn(ByVal lattice_depth As Double,
                ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                ByVal phase_pi_start As Double, ByVal phase_pi_stop As Double, ByVal phase_ramp_dur As Double,
                ByVal t_start As Double, ByVal ramp_dur As Double, ByVal hold_dur As Double,
                ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                ByVal dat_chan As Integer)

    <OperationContract()>
    Sub AddRampLogSine3PhaseNoReturn(ByVal lattice_depth As Double,
                ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                ByVal phase_pi_start As Double, ByVal phase_pi_stop As Double, ByVal phase_ramp_dur As Double,
                ByVal t_start As Double, ByVal ramp_dur As Double, ByVal hold_dur As Double,
                ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddLogSine3(ByVal lattice_depth As Double, _
                ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double, _
                ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double, _
                ByVal phase_pi As Double, ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()>
    Sub AddLogSine3Phase(ByVal lattice_depth As Double,
                ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                ByVal phase_pi As Double, ByVal t_start As Double, ByVal t_stop As Double, ByVal t_ref As Double,
                ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                ByVal dat_chan As Integer)

    <OperationContract()>
    Sub AddRampLogSine3(ByVal lattice_depth As Double,
                ByVal rel_amp_1 As Double, ByVal rel_amp_2 As Double, ByVal rel_amp_3 As Double,
                ByVal freq_Hz_1 As Double, ByVal freq_Hz_2 As Double, ByVal freq_Hz_3 As Double,
                ByVal phase_pi As Double, ByVal tunneling_init As Double, ByVal tunneling_final As Double, ByVal detuning_Hz_init As Double, ByVal detuning_Hz_final As Double,
                ByVal t_start As Double, ByVal t_stop As Double, ByVal t_ref As Double,
                ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double,
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddSine3(ByVal offset As Double, _
                ByVal amp1 As Double, ByVal amp2 As Double, ByVal amp3 As Double, _
                ByVal freq1 As Double, ByVal freq2 As Double, ByVal freq3 As Double, _
                ByVal phase_pi As Double, ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddSinePhase(ByVal offset As Double, ByVal amp As Double, ByVal freq As Double, _
                ByVal phase_pi As Double, ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddSineFlip(ByVal offset As Double, ByVal amp As Double, ByVal freq As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, ByVal fract As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddSineRamp(ByVal offset As Double, ByVal amp As Double, _
                ByVal freq_start As Double, ByVal freq_stop As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddLogRamp(ByVal start_volts As Double, ByVal stop_volts As Double, _
                   ByVal t_start As Double, ByVal t_stop As Double, _
                   ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddLogRampRedDipole(ByVal red_start_volts As Double, ByVal red_start_freq As Double, _
                ByVal lattice_start_depth As Double, ByVal lattice_stop_depth As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddTunnelRamp(ByVal conversion_coeffs As Double(), _
                      ByVal start_tunneling As Double, ByVal stop_tunneling As Double, _
                      ByVal t_start As Double, ByVal t_stop As Double, _
                      ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double, _
                      ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddTunnelGaugeRamp2(ByVal conversion_coeffs As Double(), _
                            ByVal start_tunneling As Double, ByVal stop_tunneling As Double, _
                            ByVal t_start As Double, ByVal t_stop As Double, _
                            ByVal voltage_offset As Double, ByVal calib_volt As Double, ByVal calib_depth As Double, _
                            ByVal dat_chan As Integer)


    <OperationContract()> _
    Sub AddTunnelGaugeRamp(ByVal conversion_coeffs As Double(), _
                           ByVal start_tunneling As Double, ByVal stop_tunneling As Double, _
                           ByVal t_start As Double, ByVal t_stop As Double, _
                           ByVal calib_volt As Double, ByVal calib_depth As Double, _
                           ByVal dat_chan As Integer)

    <OperationContract()>
    Sub AddInterpolatedRampUsingFile(ByVal filename As String,
                ByVal start_volts As Double, ByVal stop_volts As Double,
                ByVal t_start As Double, ByVal t_stop As Double,
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddSmoothRamp(ByVal start_volts As Double, ByVal stop_volts As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddAFMRamp(ByVal start_volts As Double, ByVal stop_volts As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, ByVal t_ramp As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddSmoothBox(ByVal start_volts As Double, ByVal stop_volts As Double, _
                ByVal t_start As Double, ByVal t_dur As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddS(ByVal background_volts As Double, ByVal final_volts As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Function AddRampRedDipole(ByVal red_start_volts As Double, ByVal red_start_freq As Double, _
                ByVal lattice_start_depth As Double, ByVal lattice_stop_depth As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer) As Double

    <OperationContract()> _
    Function AddRampRedDipoleBkwd(ByVal red_start_volts As Double, ByVal red_start_freq As Double, _
                ByVal lattice_start_depth As Double, ByVal lattice_stop_depth As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer) As Double

    <OperationContract()> _
    Function AddSRedDipole(ByVal red_start_volts As Double, ByVal red_start_freq As Double, _
                ByVal lattice_start_depth As Double, ByVal lattice_stop_depth As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer) As Double

    <OperationContract()> _
    Sub AddFancySmoothRamp(ByVal start_volts As Double, ByVal stop_volts As Double, _
                ByVal alpha As Double, ByVal beta As Double, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddFromFile(ByVal filename As String, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal max_current As Double, ByVal num_psu As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddFromTransportFile(ByVal filename As String, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal max_current As Double, ByVal num_psu As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Sub AddFromBinaryTransportFile(ByVal filename As String, _
                ByVal t_start As Double, ByVal t_stop As Double, _
                ByVal dat_chan As Integer)

    <OperationContract()> _
    Function Add(ByVal n1 As Double, ByVal n2 As Double) As Double

    <OperationContract()> _
    Function GetTotalTime() As Double

End Interface

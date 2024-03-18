'optimize adiabatic parameters to FQH state
'uses smooth ramps, consider updating to ramp files used in '2D_box_SaR_SF_gauge_vx.vb' (for QUIC, QUAD, Raman beams)
'===================
'=====Variables=====
lattice1_max  = 45
lattice2_max  = 45
red_freq = 18
round_dimple_voltage = 4.05
anticonfine_dur_vert = 0
anticonfine_dur_horz = 0
walls2_volt = 2.7
walls1_volt = 3.1
use_gauge = 1
flux = 0
evolution_dur = 0
override_default = 0
dur_idx = 7
var_idx = 5
dur_value = 200
var_value = 0.2
stop_index = 11
is_return = 0
reverse_ramp = 0
return_half = 0
is_cleanup = 0
is_counting = 0
free_var = 0
'=====Variables=====
'===================
	
'----------------------------------------------------- Constants -----------------------------------------------------------------------------
Dim shunt_switch_dur As Double = 10
Dim coil_ramp_dur As Double = 30
'Dim pinning_hold_time As Double = 1000

'loading line constants
Dim gravity_offsets_switch_dur As Double = 10
Dim twod_ramp_dur As Double = 5
Dim anticonfine_volt As Double = 2.4 '2.4
Dim anticonfine_ramp_dur As Double = 1
Dim lattice1_amb_volt As Double = 1.4
Dim lattice2_amb_volt As Double = 1.8

'DMD specific durations
'DMD triggers
Dim DMD_hw_delay As Double = -0.16 'Matthew hack to put this in earlier
Dim PID_response_dur As Double = 1 'needed to fix spikes in line_DMD
Dim loadline_DMD_volt_vert As Double = 3.3
Dim loadline_DMD_volt_horz As Double = 3.1

'gauge field constants
Dim gauge1_pzX As Double = 4.52
Dim gauge1_pzY As Double = 7.34
Dim gauge2_pzX As Double = 4.37
Dim gauge2_pzY As Double = 8.65
Dim piezo_ramp_dur As Double = 2000

'Sequence constants
Dim gauge_ramp_dur As Double = 80
Dim freeze_ramp_dur As Double = 1
Dim cleanup_dur As Double = 40
Dim kick_dur As Double = 2
Dim expansion_dur As Double = 2
Dim Berlin_wall_kick_volt As Double = 2.8 '2.8
Dim lattice1_kick_volt As Double = 3.2 '3.2

'Arbitrary ramp constants 
'Dim rampSegPath As String = "Z:\\Timeline\\Analysis\\2024_02_26_fqh_arb_ramp\\ramp_segments.txt"
'Dim rampSegPath_return As String = "Z:\\Timeline\\Analysis\\2024_02_26_fqh_arb_ramp\\ramp_segments_adiabatic.txt"
'Dim lattice2_JtoDepth_coeff_path As String = "Z:\\Timeline\\Analysis\\2024_02_26_fqh_arb_ramp\\j_to_depth_2d2lattice.txt"
'Dim gauge_JtoVolt_coeff_path As String = "Z:\\Timeline\\Analysis\\2024_02_26_fqh_arb_ramp\\j_to_depth_gaugepower.txt"
Dim rampSegPath As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_02_26_fqh_arb_ramp\\ramp_segments_files\\ramp_segments.txt"
Dim rampSegPath_return As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_02_26_fqh_arb_ramp\\ramp_segments_files\\ramp_segments_adiabatic.txt"
Dim lattice2_JtoDepth_coeff_path As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_02_26_fqh_arb_ramp\\ramp_segments_files\\j_to_depth_2d2lattice.txt"
Dim gauge_JtoDepth_coeff_path As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_02_26_fqh_arb_ramp\\ramp_segments_files\\j_to_depth_gaugepower.txt"

Dim n_variables As Integer = 6 ' number of channels used in the arbitrary ramp
Dim half_index As Integer = 5 ' point at which y delocalization finishes and x delocalization starts
Dim half_index_return As Integer = 4 ' point at which y delocalization finishes and x delocalization starts for return ramp
If reverse_ramp > 0 Then
    half_index_return = half_index
End If

Dim gauge_calib_volt As Double = 3.34 'accuracy of depth to volt calibration doesn't matter much as long as consistant values are used here and in matlab file. Accuracy of conversion is still limited by quantum walk calibration.
Dim gauge_calib_depth As Double = 7.4/1.24 ' same for this value...


'----------------------------------------------------- Import  constants for arbitrary ramp ---------------------------------------------------------------

Dim lattice2_JtoDepth_coeffs As Double() = LoadArrayFromFile(lattice2_JtoDepth_coeff_path)
Dim nterms_2D2 As Double = lattice2_JtoDepth_coeffs.GetUpperBound(0)
Console.WriteLine("number of lattice depth expansion terms (minus 1): {0}", nterms_2D2)

Dim gauge_JtoDepth_coeffs As Double() = LoadArrayFromFile(gauge_JtoDepth_coeff_path)
Dim nterms_gauge As Double = gauge_JtoDepth_coeffs.GetUpperBound(0)
Console.WriteLine("number of gauge power expansion terms (minus 1): {0}", nterms_gauge)

Dim ramp_variables = LoadRampSegmentsFromFile(rampSegPath, n_variables)
Dim n_times As Integer = ramp_variables(0).GetUpperBound(0)
Console.WriteLine("N times: {0}", n_times)

' overwrite appropriate variables
If override_default > 0 Then
    If dur_idx <= n_times Then
        ramp_variables(0)(dur_idx) = dur_value
        Console.WriteLine("overwriting ramp_variables(0)({0}) = {1}.", dur_idx, dur_value)
        If var_idx <= n_variables Then
            ramp_variables(var_idx)(dur_idx) = var_value
            Console.WriteLine("overwriting ramp_variables({0})({1}) = {2}.", var_idx, dur_idx, var_value)
        Else
            Console.WriteLine("Bad value for var_idx = {0}, must be <= {1}. Default voltage value will not be overwritten.", var_idx, n_variables)
            Microsoft.VisualBasic.Interaction.MsgBox("Bad value for var_idx. Default voltage value will not be overwritten.")
        End If
    Else
        Console.WriteLine("Bad value for dur_idx = {0}, must be <= {1}. Default values will not be overwritten.", dur_idx, n_times)
        Microsoft.VisualBasic.Interaction.MsgBox("Bad value for dur_idx. Default values will not be overwritten.")
    End If
End If

' stop the ramp early
If return_half > 0 Then
    Console.WriteLine("truncating ramp at half index {0}", half_index)
    n_times = half_index
ElseIf stop_index < n_times Then
    Console.WriteLine("truncating ramp at point {0}", stop_index)
    n_times = stop_index
End If

Dim ramp_durs As Double() = ramp_variables(0)
Dim lattice1_ramp_v(n_times) As Double
Dim gauge_power_ramp_j(n_times) As Double
Dim lattice2_ramp_j(n_times) As Double
Dim gauge_freq_ramp_v(n_times) As Double
For index As Integer = 0 To n_times
    lattice1_ramp_v(index) = DepthToVolts(ramp_variables(1)(index), lattice1_calib_depth, lattice1_calib_volt, lattice1_voltage_offset)
    gauge_power_ramp_j(index) = ramp_variables(2)(index)/1240 'units of E_r    
    lattice2_ramp_j(index) = ramp_variables(3)(index)/1240 'units of E_r    
    gauge_freq_ramp_v(index) = BeatVolt(ramp_variables(4)(index))
Next

Dim quad_ramp_v As Double() = ramp_variables(5)  ' 0th value is "quad_init"
Dim quic_ramp_v As Double() = ramp_variables(6)  ' 0th value is "quic_init"

Dim lattice1_ramp_forward_v As Double = lattice1_ramp_v(n_times)
Dim lattice2_ramp_forward_j As Double = lattice2_ramp_j(n_times)
Dim lattice2_ramp_forward_v As Double = JToVolts(lattice2_ramp_forward_j, lattice2_JtoDepth_coeffs, nterms_2D2, lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)
Dim gauge_power_ramp_forward_j As Double = gauge_power_ramp_j(n_times)
Dim gauge_power_ramp_forward_v As Double = GaugeJToVolts(gauge_power_ramp_forward_j, gauge_JtoDepth_coeffs, nterms_gauge, gauge_calib_depth, gauge_calib_volt)
Dim gauge_freq_ramp_forward_v As Double = gauge_freq_ramp_v(n_times)
Dim quad_ramp_forward_v As Double = quad_ramp_v(n_times)
Dim quic_ramp_forward_v As Double = quic_ramp_v(n_times)

Dim lattice1_ramp_end_v As Double
Dim lattice2_ramp_end_j As Double
Dim gauge_power_ramp_end_j As Double
Dim gauge_freq_ramp_end_v As Double
Dim quad_ramp_end_v As Double
Dim quic_ramp_end_v As Double

'return
Dim ramp_durs_return As Double()
Dim lattice1_ramp_v_return(-1) As Double
Dim lattice2_ramp_j_return(-1) As Double
Dim gauge_power_ramp_j_return(-1) As Double
Dim gauge_freq_ramp_v_return(-1) As Double 
Dim quad_ramp_v_return(-1) As Double
Dim quic_ramp_v_return(-1) As Double
Dim n_times_return As Integer

If (is_return > 0) Then 
    'get times for return ramp
    Dim ramp_variables_return As Double()()   
  
    If (reverse_ramp > 0) Then
        ramp_variables_return = ramp_variables
        n_times_return = n_times
        ramp_durs_return = ramp_durs
    Else
        ramp_variables_return = LoadRampSegmentsFromFile(rampSegPath_return, n_variables)
        n_times_return = ramp_variables_return(0).GetUpperBound(0)  
        ramp_durs_return = ramp_variables_return(0)
    End If
    Console.WriteLine("n_times_return: {0}", n_times_return) 
    
    If return_half > 0 Then
        n_times_return = half_index_return
        Console.WriteLine("new n_times_return = {0}", half_index_return)
    End If

    'ramp_durs_return = ramp_variables_return(0)
    ReDim lattice1_ramp_v_return(n_times_return)
    ReDim lattice2_ramp_j_return(n_times_return)
    ReDim gauge_power_ramp_j_return(n_times_return)
    ReDim gauge_freq_ramp_v_return(n_times_return)  
    ReDim quad_ramp_v_return(n_times_return)
    ReDim quic_ramp_v_return(n_times_return)

    For index As Integer = 0 To n_times_return
        lattice1_ramp_v_return(index) = DepthToVolts(ramp_variables_return(1)(n_times_return - index), lattice1_calib_depth, lattice1_calib_volt, lattice1_voltage_offset)  
        gauge_power_ramp_j_return(index) = ramp_variables_return(2)(n_times_return - index)/1240 'in units of E_r       
        lattice2_ramp_j_return(index) = ramp_variables_return(3)(n_times_return - index)/1240 'in units of E_r
        gauge_freq_ramp_v_return(index) = BeatVolt(ramp_variables(4)(n_times_return - index))
        quad_ramp_v_return(index) = ramp_variables_return(5)(n_times_return - index)
        quic_ramp_v_return(index) = ramp_variables_return(6)(n_times_return - index)
    Next

    lattice1_ramp_end_v = lattice1_ramp_v_return(n_times_return)
    lattice2_ramp_end_j = lattice2_ramp_j_return(n_times_return)
    gauge_power_ramp_end_j = gauge_power_ramp_j_return(n_times_return)
    gauge_freq_ramp_end_v = gauge_freq_ramp_v_return(n_times_return)
    quad_ramp_end_v = quad_ramp_v_return(n_times_return)
    quic_ramp_end_v = quic_ramp_v_return(n_times_return)

Else    
    lattice1_ramp_end_v = lattice1_ramp_forward_v
    lattice2_ramp_end_j = lattice2_ramp_forward_j
    gauge_power_ramp_end_j = gauge_power_ramp_forward_j
    gauge_freq_ramp_end_v = gauge_freq_ramp_forward_v
    quad_ramp_end_v = quad_ramp_forward_v
    quic_ramp_end_v = quic_ramp_forward_v
End If

Dim lattice1_ramp_start_v = lattice1_ramp_v(0)
Dim gauge_power_ramp_start_v As Double = GaugeJToVolts(gauge_power_ramp_j(0), gauge_JtoDepth_coeffs, nterms_gauge, gauge_calib_depth, gauge_calib_volt)
Dim gauge_power_ramp_end_v As Double = GaugeJToVolts(gauge_power_ramp_end_j, gauge_JtoDepth_coeffs, nterms_gauge, gauge_calib_depth, gauge_calib_volt)
Dim lattice2_ramp_start_v As Double = JToVolts(lattice2_ramp_j(0), lattice2_JtoDepth_coeffs, nterms_2D2, lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)
Dim lattice2_ramp_end_v As Double = JToVolts(lattice2_ramp_end_j, lattice2_JtoDepth_coeffs, nterms_2D2, lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)
Dim gauge_freq_ramp_start_v As Double = gauge_freq_ramp_v(0)
Dim quad_ramp_start_v As Double = quad_ramp_v(0)
Dim quic_ramp_start_v As Double = quic_ramp_v(0)

' Final values:
' lattice1_ramp_end_v
' gauge_power_ramp_end_v
' lattice2_ramp_end_v
' gauge_freq_ramp_end_v
' quad_ramp_end_v
' quic_ramp_end_v

'----------------------------------------------------- MOT Creation ---------------------------------------------------------------------------------------

digitaldata2.AddPulse(clock_resynch,1,4)
analogdata.DisableClkDist(0.95, 4.05)
analogdata2.DisableClkDist(0.95, 4.05)

Dim MOT_end_time As Double
MOT_end_time = Me.AddMOTSequenceUpgrade(0, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim transport_start_time As Double = MOT_end_time


'----------------------------------------------------- Transport ------------------------------------------------------------------------------------------

Dim transport_end_time As Double
transport_end_time = Me.AddTransportSequence(transport_start_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)


'----------------------------------------------------- Evaporation ----------------------------------------------------------------------------------------

Dim evaporation_end_time As Double
evaporation_end_time = Me.AddEvaporationSequence(transport_end_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)


'----------------------------------------------------- Make Mott Insulator --------------------------------------------------------------------------------

Dim MI_variables As Double() = Me.AddMottInsulatorSequence(evaporation_end_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim twodphysics_start_time As Double = MI_variables(0)
Dim lattice1_max_volt As Double = MI_variables(1)
Dim lattice2_max_volt As Double = MI_variables(2)
Dim end_dipole_voltage As Double = MI_variables(3) 


'----------------------------------------------------- Time Definitions for Cutting -----------------------------------------------------------------------

' time constants for cutting vertical line
Dim line_load_vert_start_time = twodphysics_start_time + gravity_offsets_switch_dur
Dim cutting1_turnon_start_time As Double = line_load_vert_start_time
Dim cutting1_turnon_end_time As Double = cutting1_turnon_start_time + 5
Dim twod1_rampdown_start_time As Double = cutting1_turnon_end_time
Dim twod1_rampdown_end_time As Double = twod1_rampdown_start_time + twod_ramp_dur
Dim expand_vert_start_time As Double = twod1_rampdown_end_time
Dim expand_vert_end_time As Double = expand_vert_start_time + anticonfine_ramp_dur + anticonfine_dur_vert
Dim twod1_reload_start_time As Double = expand_vert_end_time
Dim twod1_reload_end_time As Double = twod1_reload_start_time + twod_ramp_dur
Dim cutting1_turnoff_start_time As Double = twod1_reload_end_time 'also ramp down anticonfine here, so can ramp back on for the next direction
Dim cutting1_turnoff_end_time As Double = cutting1_turnoff_start_time + 5
Dim line_load_mid_time As Double = cutting1_turnoff_end_time
' skip if zero
If (anticonfine_dur_vert = 0) Then
	line_load_mid_time = line_load_vert_start_time + 20 + PID_response_dur
End If

' time constants for cutting horizontal line
Dim line_load_horz_start_time As Double = line_load_mid_time + PID_response_dur
Dim cutting2_turnon_start_time As Double = line_load_horz_start_time
Dim cutting2_turnon_end_time As Double = cutting2_turnon_start_time + 5
Dim twod2_rampdown_start_time As Double = cutting2_turnon_end_time
Dim twod2_rampdown_end_time As Double = twod2_rampdown_start_time + twod_ramp_dur
Dim expand_horz_start_time As Double = twod2_rampdown_end_time
Dim expand_horz_end_time As Double = expand_horz_start_time + anticonfine_ramp_dur + anticonfine_dur_horz
Dim twod2_reload_start_time As Double = expand_horz_end_time
Dim twod2_reload_end_time As Double = twod2_reload_start_time + twod_ramp_dur
Dim cutting2_turnoff_start_time As Double = twod2_reload_end_time  'also ramp down anticonfine here, so can ramp back on for the next direction
Dim cutting2_turnoff_end_time As Double = cutting2_turnoff_start_time + 5
Dim line_load_end_time As Double = cutting2_turnoff_end_time + gravity_offsets_switch_dur ' ramp gravity_offset to zero for expel 
'skip if zero
If (anticonfine_dur_horz = 0) Then
    line_load_end_time = line_load_mid_time + 20 + PID_response_dur
End If


'----------------------------------------------------- Time Definitions for Physics -----------------------------------------------------------------------

'(1) Turn on walls2
Dim walls2_turnon_start_time As Double = line_load_end_time
Dim walls2_turnon_end_time As Double
If (walls2_volt > 0) Then
    walls2_turnon_end_time = walls2_turnon_start_time + 5
Else
    walls2_turnon_end_time = walls2_turnon_start_time
End If

'() Turn on walls1
'turns on at the same time as walls2
Dim walls1_turnon_start_time As Double = line_load_end_time
Dim walls1_turnon_end_time As Double
If (walls1_volt > 0) Then
    walls1_turnon_end_time = walls1_turnon_start_time + 5
Else
    walls1_turnon_end_time = walls1_turnon_start_time
End If

'() Turn on quic gradient
Dim quic_turnon_start_time As Double = walls1_turnon_end_time
Dim quic_turnon_end_time As Double 
If (quic_ramp_start_v > 0) Then
    quic_turnon_end_time = quic_turnon_start_time + coil_ramp_dur
Else 
    quic_turnon_end_time = quic_turnon_start_time
End If

'() Turn on quad gradient
Dim quad_turnon_start_time As Double = quic_turnon_start_time
Dim quad_turnon_end_time As Double 
If (quad_ramp_start_v > 0) Then
    quad_turnon_end_time = quad_turnon_start_time + coil_ramp_dur
Else
    quad_turnon_end_time = quad_turnon_start_time
End If

'() Turn on gauge beams
Dim piezo_rampup_time As Double = walls1_turnon_start_time - piezo_ramp_dur - 1000
Dim piezo_ready_time As Double = piezo_rampup_time + piezo_ramp_dur


'----------------------------------------------------- Time Definitions for Arb Ramp ----------------------------------------------------------------------

'() forward ramp
Dim ramp_start_time As Double = quic_turnon_end_time
Dim ramp_end_time As Double
Dim ramp_t(n_times) As Double
ramp_t(0) = ramp_start_time
For index As Integer = 1 To n_times
    ramp_t(index) = ramp_t(index - 1) + ramp_durs(index)
Next
Dim ramp_forward_end_time As Double = ramp_t(n_times)
'Console.WriteLine("ramp forward end time = {0}", ramp_forward_end_time)

'() Hold time
Dim hold_start_time As Double = ramp_forward_end_time
Dim hold_end_time As Double = hold_start_time + evolution_dur

'() return ramp
Dim ramp_t_return(-1) As Double
If is_return > 0 Then
    ReDim ramp_t_return(n_times_return)
    ramp_t_return(0) = hold_end_time
    For index As Integer = 1 To n_times_return
        ramp_t_return(index) = ramp_t_return(index - 1) + ramp_durs_return(n_times_return + 1 - index)
        'Console.WriteLine("ramp return index: {0}, ramp duration index: {1}", index, n_times_return+1-index)
    Next
    ramp_end_time = ramp_t_return(n_times_return)
Else
    ramp_end_time = hold_end_time
End If
'Console.WriteLine("ramp end time = {0}", ramp_end_time)


'----------------------------------------------------- Time Definitions for Physics Cont. -----------------------------------------------------------------

'() raise lattice depth (same for both??)
Dim lattice_quench_end_time As Double  = ramp_end_time + 1

'() freeze 2D2 lattice 
Dim lattice2_freeze_start_time As Double = ramp_end_time
Dim lattice2_freeze_end_time As Double = lattice2_freeze_start_time + freeze_ramp_dur

'() turn off walls 
'same variable for both walls
Dim walls_turnoff_start_time As Double = lattice_quench_end_time 
Dim walls_turnoff_end_time As Double = walls_turnoff_start_time + 5

'() Turn off quic and quad gradients
Dim grad_turnoff_start_time As Double = lattice_quench_end_time
Dim grad_turnoff_end_time As Double = grad_turnoff_start_time + coil_ramp_dur

'() remove atoms outside the walls (cleanup)
Dim cleanup_start_time As Double = walls_turnoff_end_time
Dim cutting1_turnon2_start_time As Double = cleanup_start_time
Dim cutting1_turnon2_end_time As Double = cutting1_turnon2_start_time + (is_cleanup * 5)
Dim twod1_rampdown2_start_time As Double = cutting1_turnon2_end_time
Dim twod1_rampdown2_end_time As Double = twod1_rampdown2_start_time + (is_cleanup * twod_ramp_dur)
Dim twod1_reload2_start_time As Double = twod1_rampdown2_end_time + (is_cleanup * cleanup_dur)
Dim twod1_reload2_end_time As Double = twod1_reload2_start_time + (is_cleanup * twod_ramp_dur)
Dim cutting1_turnoff2_start_time As Double = twod1_reload2_end_time 
Dim cutting1_turnoff2_end_time As Double = cutting1_turnoff2_start_time + (is_cleanup * 5)
Dim cleanup_end_time As Double = cutting1_turnoff2_end_time
If (is_cleanup > 0) Then
    cleanup_end_time 
Else
    cleanup_end_time = cleanup_start_time
End If

'() kick and capture for full counting
Dim full_counting_start_time As Double = cleanup_end_time
Dim Berlin_wall_turnon_start_time As Double = full_counting_start_time + 5 'fixed 5ms pause such that DMD trigger for wall pattern is always separated from DMD trigger for cutting (cleanup) pattern
Dim Berlin_wall_turnon_end_time As Double = Berlin_wall_turnon_start_time + (is_counting * 5)
Dim twod1_rampdown3_start_time As Double = Berlin_wall_turnon_end_time
Dim twod1_rampdown3_end_time As Double = Berlin_wall_turnon_end_time + (is_counting * 1)
Dim twod1_reload3_start_time As Double = twod1_rampdown3_end_time + (is_counting * kick_dur)
Dim twod1_reload3_end_time As Double = twod1_reload3_start_time + (is_counting * 1)
Dim Berlin_wall_turnoff_start_time As Double = twod1_reload3_end_time
Dim Berlin_wall_turnoff_end_time As Double = Berlin_wall_turnoff_start_time + (is_counting * 5)
Dim full_counting_end_time As Double = twod1_reload3_end_time

'() freeze 2D1 lattice, turn off the rest of the gradients
Dim lattice1_freeze_start_time As Double = full_counting_end_time
Dim lattice1_freeze_end_time As Double = lattice1_freeze_start_time + freeze_ramp_dur

'() Pinning, image
Dim twodphysics_end_time As Double = full_counting_end_time + free_var
Dim pinning_start_time As Double = twodphysics_end_time

'() Turn off gauge beams
Dim piezo_rampdown_time As Double = pinning_start_time
Dim piezo_end_time As Double = piezo_rampdown_time + piezo_ramp_dur


'------------------------------------------------------------------------ Pinning -------------------------------------------------------------------------

Dim pinning_times As Double() = Me.AddPinningSequence(pinning_start_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim pinning_end_time As Double = pinning_times(0)
Dim pinning_ready_time As Double = pinning_times(1)
Dim molasses_start_time As Double = pinning_times(2)

Dim IT As Double = pinning_end_time + TOF
Dim last_time As Double = IT


'------------------------------------------------------------------------ Tracking ------------------------------------------------------------------------

Dim delay_tracking As Double = 2000 '1000
Dim tracking_end_time As Double = Me.AddTrackingSequence2(delay_tracking, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)


'--------------------------------------------------------------------- Invert signals ---------------------------------------------------------------------

digitaldata.AddPulse(mot_low_current, transport_start_time, last_time) 'MOT Current Supply
digitaldata.AddPulse(ta_shutter, transport_start_time, last_time) 'TA Shutter
digitaldata.AddPulse(repump_shutter, transport_start_time, last_time) 'Repump Shutter


'----------------------------------------------------- Lattice Depth Conversion to Volts ------------------------------------------------------------------

'Dim lattice1_low_volt As Double = DepthToVolts(lattice1_low_depth, lattice1_calib_depth, lattice1_calib_volt, lattice1_voltage_offset)
'Dim lattice2_low_volt As Double = DepthToVolts(lattice2_low_depth, lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)


'----------------------------------------------------- Hold Mott Insulator --------------------------------------------------------------------------------

digitaldata.AddPulse(bias_enable, twodphysics_start_time, pinning_ready_time) 'Enables bias coils
digitaldata.AddPulse(ps5_enable, twodphysics_start_time, pinning_ready_time)
digitaldata2.AddPulse(ioffe_mirror_fet, twodphysics_start_time, pinning_ready_time)
digitaldata.AddPulse(quad_shim, twodphysics_start_time, pinning_ready_time)
digitaldata2.AddPulse(single_quad_shim, twodphysics_start_time, pinning_ready_time)
digitaldata2.AddPulse(quad_shim2, quad_turnon_start_time - shunt_switch_dur, grad_turnoff_end_time + shunt_switch_dur)
digitaldata2.AddPulse(ps8_shunt, twodphysics_start_time, quad_turnon_start_time - 2*shunt_switch_dur)


'----------------------------------------------------- Hold Lattices --------------------------------------------------------------------------------------

digitaldata2.AddPulse(lattice2D765_ttl, twodphysics_start_time, molasses_start_time)
digitaldata2.AddPulse(lattice2D765_ttl2, twodphysics_start_time, molasses_start_time)
digitaldata2.AddPulse(lattice2D765_shutter, twodphysics_start_time, molasses_start_time)
digitaldata2.AddPulse(lattice2D765_shutter2, twodphysics_start_time, molasses_start_time)
digitaldata.AddPulse(ttl_axial_lattice, twodphysics_start_time, molasses_start_time+1) 'JK: why +1?
digitaldata.AddPulse(axial_lattice_shutter, twodphysics_start_time, molasses_start_time)


'----------------------------------------------------- Blue Donut -----------------------------------------------------------------------------------------

'ramp down Blue Donut for physics 
'off until the end of sequence
digitaldata2.AddPulse(anticonfin_ttl, twodphysics_start_time, line_load_vert_start_time)
digitaldata2.AddPulse(anticonfin_shutter, twodphysics_start_time, line_load_vert_start_time)

'turn off confinement for line loading
analogdata.AddSmoothRamp(end_dipole_voltage, 1.44, twodphysics_start_time, line_load_vert_start_time, red_dipole_power) 


'----------------------------------------------------- Anticonfine Beam -----------------------------------------------------------------------------------

'anticonfinement during DMD line loading
digitaldata.AddPulse(blue_dipole_shutter, line_load_vert_start_time - 20, line_load_end_time)
If (anticonfine_dur_vert > 0) Then
	digitaldata2.AddPulse(blue_dipole_ttl, line_load_vert_start_time, line_load_mid_time) 
	analogdata.AddSmoothRamp(1, anticonfine_volt, twod1_rampdown_start_time, twod1_rampdown_end_time, red_dipole_power)
	analogdata.AddStep(anticonfine_volt, twod1_rampdown_end_time, twod1_reload_start_time, red_dipole_power)
	analogdata.AddSmoothRamp(anticonfine_volt, 1, twod1_reload_start_time, twod1_reload_end_time, red_dipole_power)
End If

If (anticonfine_dur_horz > 0) Then
    digitaldata2.AddPulse(blue_dipole_ttl, line_load_horz_start_time, line_load_end_time)
    analogdata.AddSmoothRamp(1, anticonfine_volt, twod2_rampdown_start_time, twod2_rampdown_end_time, red_dipole_power)
    analogdata.AddStep(anticonfine_volt, twod2_rampdown_end_time, twod2_reload_start_time, red_dipole_power)
    analogdata.AddSmoothRamp(anticonfine_volt, 1, twod2_reload_start_time, twod2_reload_end_time, red_dipole_power)
End If

'anticonfinement during cleanup prior to full counting
If (is_cleanup > 0) Then 
    digitaldata.AddPulse(blue_dipole_shutter, cleanup_start_time - 20, twod1_reload2_end_time)
    digitaldata2.AddPulse(blue_dipole_ttl, cutting1_turnon2_start_time, cutting1_turnoff2_end_time)
    analogdata.AddSmoothRamp(1, anticonfine_volt, twod1_rampdown2_start_time, twod1_rampdown2_end_time, red_dipole_power)
    analogdata.AddStep(anticonfine_volt, twod1_rampdown2_end_time,  twod1_reload2_start_time, red_dipole_power)
    analogdata.AddSmoothRamp(anticonfine_volt, 1, twod1_reload2_start_time, twod1_reload2_end_time, red_dipole_power)
End If


'----------------------------------------------------- Lattice Drop and Expulsion for DMD -----------------------------------------------------------------

analogdata.AddStep(lattice1_max_volt, twodphysics_start_time, line_load_vert_start_time, lattice2D765_power)
analogdata.AddStep(lattice2_max_volt, twodphysics_start_time, line_load_vert_start_time, lattice2D765_power2)

If (anticonfine_dur_vert > 0) Then
	'2D1
    analogdata.AddStep(lattice1_max_volt, line_load_vert_start_time, twod1_rampdown_start_time, lattice2D765_power)
	analogdata.AddSmoothRamp(lattice1_max_volt, lattice1_amb_volt, twod1_rampdown_start_time,twod1_rampdown_end_time,lattice2D765_power)
    analogdata.AddStep(lattice1_amb_volt, twod1_rampdown_end_time, twod1_reload_start_time, lattice2D765_power)
    analogdata.AddSmoothRamp(lattice1_amb_volt, lattice1_max_volt, twod1_reload_start_time, twod1_reload_end_time,lattice2D765_power)
    analogdata.AddStep(lattice1_max_volt, twod1_reload_end_time, line_load_mid_time, lattice2D765_power)
    '2D2
    analogdata.AddStep(lattice2_max_volt, line_load_vert_start_time, line_load_mid_time, lattice2D765_power2)
Else
    analogdata.AddStep(lattice1_max_volt, line_load_vert_start_time, line_load_mid_time, lattice2D765_power)
    analogdata.AddStep(lattice2_max_volt, line_load_vert_start_time, line_load_mid_time, lattice2D765_power2)
End If

If (anticonfine_dur_horz > 0) Then
    '2D1
    analogdata.AddStep(lattice1_max_volt, line_load_mid_time, line_load_end_time, lattice2D765_power)
    '2D2
    analogdata.AddStep(lattice2_max_volt, line_load_mid_time, twod2_rampdown_start_time, lattice2D765_power2)
    analogdata.AddSmoothRamp(lattice2_max_volt, lattice2_amb_volt, twod2_rampdown_start_time, twod2_rampdown_end_time, lattice2D765_power2)
    analogdata.AddStep(lattice2_amb_volt, twod2_rampdown_end_time, twod2_reload_start_time, lattice2D765_power2)
    analogdata.AddSmoothRamp(lattice2_amb_volt, lattice2_max_volt, twod2_reload_start_time, twod2_reload_end_time, lattice2D765_power2)
    analogdata.AddStep(lattice2_max_volt, twod2_reload_end_time, line_load_end_time, lattice2D765_power2)
Else
    analogdata.AddStep(lattice1_max_volt, line_load_mid_time, line_load_end_time, lattice2D765_power)
    analogdata.AddStep(lattice2_max_volt, line_load_mid_time, line_load_end_time, lattice2D765_power2)
End If


'----------------------------------------------------- Gauge Field ----------------------------------------------------------------------------------------

If (use_gauge > 0) Then
	'ramp piezos to the proper location
	analogdata2.AddRamp(0, gauge1_pzX, piezo_rampup_time, piezo_ready_time, gauge1_PZTx)
	analogdata2.AddStep(gauge1_pzX, piezo_ready_time, piezo_rampdown_time, gauge1_PZTx)
	analogdata2.AddRamp(gauge1_pzX, 0, piezo_rampdown_time, piezo_end_time, gauge1_PZTx)

	analogdata2.AddRamp(0, gauge1_flux_calib*flux/0.25 + gauge1_pzY, piezo_rampup_time, piezo_ready_time, gauge1_PZTy) 
	analogdata2.AddStep(gauge1_pzY, piezo_ready_time, piezo_rampdown_time, gauge1_PZTy)
	analogdata2.AddRamp(gauge1_pzY, 0, piezo_rampdown_time, piezo_end_time, gauge1_PZTy)

	analogdata2.AddRamp(0, gauge2_pzX, piezo_rampup_time, piezo_ready_time, gauge2_PZTx)
	analogdata2.AddStep(gauge2_pzX, piezo_ready_time, piezo_rampdown_time, gauge2_PZTx)
	analogdata2.AddRamp(gauge2_pzX, 0, piezo_rampdown_time, piezo_end_time, gauge2_PZTx)

	analogdata2.AddRamp(0, gauge2_flux_calib*flux/0.25 + gauge2_pzY, piezo_rampup_time, piezo_ready_time, gauge2_PZTy)
	analogdata2.AddStep(gauge2_pzY, piezo_ready_time, piezo_rampdown_time, gauge2_PZTy)
	analogdata2.AddRamp(gauge2_pzY, 0, piezo_rampdown_time, piezo_end_time, gauge2_PZTy)

    digitaldata2.AddPulse(gauge_shutter, ramp_start_time - 5,  ramp_end_time + 10)
    'digitaldata2.AddPulse(gauge_ttl, ramp_start_time - 10, ramp_end_time)
End If


'----------------------------------------------------- Lattices I -----------------------------------------------------------------------------------------

'hold axial lattice
analogdata.AddStep(axial_voltage,twodphysics_start_time,pinning_ready_time,axial_lattice_power)
analogdata.AddSmoothRamp(axial_voltage,1.76,pinning_ready_time,molasses_start_time,axial_lattice_power)

analogdata.AddStep(lattice2_max_volt, line_load_end_time, ramp_start_time, lattice2D765_power2) '2D2
analogdata.AddStep(lattice1_max_volt, line_load_end_time, ramp_start_time, lattice2D765_power) '2D1


'----------------------------------------------------- Magnetic Fields I ----------------------------------------------------------------------------------

'keep constant at values from MI sequence
analogdata.AddStep(quad_gravity_offset, twodphysics_start_time, pinning_ready_time, ps1_ao)
analogdata.AddStep(ps6_scaler * quic_gravity_offset + ps6_offset, twodphysics_start_time, pinning_ready_time, ps6_ao)

If (quic_ramp_start_v > 0) Then 
    'turn on quic
    analogdata2.AddSmoothRamp(0, quic_ramp_start_v * ps5_scaler, quic_turnon_start_time, ramp_start_time, ps5_ao)
End If 

If (quad_ramp_start_v > 0) Then
    'turn on quad
    analogdata2.AddSmoothRamp(0, quad_ramp_start_v, quad_turnon_start_time, ramp_start_time, ps8_ao)
End If


'----------------------------------------------------- Arbitrary Ramps ------------------------------------------------------------------------------------
'Linear interpolation to connect the ramp end points

'2D1 lattice power
For index As Integer = 0 To n_times - 1
    analogdata.AddLogRamp(lattice1_ramp_v(index), lattice1_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), lattice2D765_power)
Next
analogdata.AddStep(lattice1_ramp_forward_v, hold_start_time, hold_end_time, lattice2D765_power)

'gauge power
For index As Integer = 0 To n_times - 1  
    analogdata2.AddTunnelGaugeRamp(gauge_JtoDepth_coeffs, gauge_power_ramp_j(index), gauge_power_ramp_j(index + 1), ramp_t(index), ramp_t(index + 1), gauge_calib_volt, gauge_calib_depth, gauge1_power) 'gauge1_power  
    If (gauge_power_ramp_j(index + 1) > 0) Then
        digitaldata2.AddPulse(gauge_ttl, ramp_t(index) - 1, ramp_t(index + 1))
    Else If (gauge_power_ramp_j(index) > 0) Then
        digitaldata2.AddPulse(gauge_ttl, ramp_t(index) - 1, ramp_t(index + 1))  
    End If
Next
analogdata2.AddStep(gauge_power_ramp_forward_v, hold_start_time, hold_end_time, gauge1_power)
If gauge_power_ramp_end_j > 0 Then
    digitaldata2.AddPulse(gauge_ttl, hold_start_time - 1, hold_end_time)
End If

'2D2 lattice power
For index As Integer = 0 To n_times - 1
    analogdata.AddTunnelRamp(lattice2_JtoDepth_coeffs, lattice2_ramp_j(index), lattice2_ramp_j(index + 1), ramp_t(index), ramp_t(index + 1), lattice2_voltage_offset, lattice2_calib_volt, lattice2_calib_depth, lattice2D765_power2)
Next
analogdata.AddStep(lattice2_ramp_forward_v, hold_start_time, hold_end_time, lattice2D765_power2)

'quic grad
For index As Integer = 0 To n_times - 1
    analogdata2.AddRamp(quic_ramp_v(index) * ps5_scaler, quic_ramp_v(index + 1) * ps5_scaler, ramp_t(index), ramp_t(index + 1), ps5_ao) 'ps5_ao
Next
analogdata2.AddStep(quic_ramp_forward_v * ps5_scaler, hold_start_time, hold_end_time, ps5_ao)

'quad grad
For index As Integer = 0 To n_times - 1
    analogdata2.AddRamp(quad_ramp_v(index), quad_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), ps8_ao) 'ps8_ao
Next
analogdata2.AddStep(quad_ramp_forward_v, hold_start_time, hold_end_time, ps8_ao)

'gauge detuning
For index As Integer = 0 To n_times - 1
    analogdata2.AddRamp(gauge_freq_ramp_v(index), gauge_freq_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), gauge2_rf_fm)
Next
analogdata2.AddStep(gauge_freq_ramp_forward_v, hold_start_time, hold_end_time, gauge2_rf_fm)

If (is_return > 0) Then
    ' ramp backward for return
    '2D1 lattice power
    For index As Integer = 0 To n_times_return - 1
        analogdata.AddLogRamp(lattice1_ramp_v_return(index), lattice1_ramp_v_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), lattice2D765_power)
    Next

    'gauge power
    For index As Integer = 0 To n_times_return - 1            
        analogdata2.AddTunnelGaugeRamp(gauge_JtoDepth_coeffs, gauge_power_ramp_j_return(index), gauge_power_ramp_j_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), gauge_calib_volt, gauge_calib_depth, gauge1_power)
        If (gauge_power_ramp_j_return(index + 1) > 0) Then
            digitaldata2.AddPulse(gauge_ttl, ramp_t_return(index) - 1, ramp_t_return(index + 1))
        Else If (gauge_power_ramp_j_return(index) > 0) Then
            digitaldata2.AddPulse(gauge_ttl, ramp_t_return(index) - 1, ramp_t_return(index + 1))  
        End If
    Next

    '2D2 lattice power
    For index As Integer = 0 To n_times_return - 1
        analogdata.AddTunnelRamp(lattice2_JtoDepth_coeffs, lattice2_ramp_j_return(index), lattice2_ramp_j_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), lattice2_voltage_offset, lattice2_calib_volt, lattice2_calib_depth, lattice2D765_power2)
    Next

    'quic grad
    For index As Integer = 0 To n_times_return - 1
        analogdata2.AddRamp(quic_ramp_v_return(index) * ps5_scaler, quic_ramp_v_return(index + 1) * ps5_scaler, ramp_t_return(index), ramp_t_return(index + 1), ps5_ao) 'ps5_ao
    Next

    'quad grad
    For index As Integer = 0 To n_times_return - 1
        analogdata2.AddRamp(quad_ramp_v_return(index), quad_ramp_v_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), ps8_ao) 'ps8_ao
    Next

    'gauge detuning
    For index As Integer = 0 To n_times_return - 1
        analogdata2.AddRamp(gauge_freq_ramp_v_return(index), gauge_freq_ramp_v_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), gauge2_rf_fm)
    Next
End If


'----------------------------------------------------- Lattices II ----------------------------------------------------------------------------------------

'Quench to deep lattice
'analogdata.AddSmoothRamp(lattice2_ramp_end_v, lattice2_max_volt, ramp_end_time, lattice_quench_end_time, lattice2D765_power2)
analogdata.AddSmoothRamp(lattice1_ramp_end_v, lattice1_max_volt, ramp_end_time, lattice_quench_end_time, lattice2D765_power)
analogdata.AddStep(lattice1_max_volt, lattice_quench_end_time, cleanup_start_time, lattice2D765_power)

'clean up
If (is_cleanup > 0) Then
    '2D1
    analogdata.AddStep(lattice1_max_volt, cleanup_start_time, twod1_rampdown2_start_time, lattice2D765_power)
    analogdata.AddSmoothRamp(lattice1_max_volt, lattice1_amb_volt, twod1_rampdown2_start_time, twod1_rampdown2_end_time, lattice2D765_power)
    analogdata.AddStep(lattice1_amb_volt, twod1_rampdown2_end_time, twod1_reload2_start_time, lattice2D765_power) 
    analogdata.AddSmoothRamp(lattice1_amb_volt, lattice1_max_volt, twod1_reload2_start_time, twod1_reload2_end_time, lattice2D765_power)
    analogdata.AddStep(lattice1_max_volt, twod1_reload2_end_time, full_counting_start_time, lattice2D765_power)
Else
    analogdata.AddStep(lattice1_max_volt, cleanup_start_time, full_counting_start_time, lattice2D765_power)
End If

'quench lattice1 for full counting
If (is_counting > 0) Then
    analogdata.AddStep(lattice1_max_volt, full_counting_start_time, twod1_rampdown3_start_time, lattice2D765_power)
    analogdata.AddSmoothRamp(lattice1_max_volt, lattice1_kick_volt, twod1_rampdown3_start_time, twod1_rampdown3_end_time, lattice2D765_power)
    analogdata.AddStep(lattice1_kick_volt, twod1_rampdown3_end_time, twod1_reload3_start_time, lattice2D765_power) 
    analogdata.AddSmoothRamp(lattice1_kick_volt, lattice1_max_volt, twod1_reload3_start_time, twod1_reload3_end_time, lattice2D765_power)
    'analogdata.AddStep(lattice1_max_volt, twod1_reload3_end_time, lattice1_freeze_start_time, lattice2D765_power)
Else
    analogdata.AddStep(lattice1_max_volt, full_counting_start_time, lattice1_freeze_start_time, lattice2D765_power)
End If


'freeze 2D1 lattice
analogdata.AddSmoothRamp(lattice1_max_volt, lattice1_deepest_volt, lattice1_freeze_start_time, lattice1_freeze_end_time, lattice2D765_power)
analogdata.AddStep(lattice1_deepest_volt, lattice1_freeze_end_time, pinning_ready_time, lattice2D765_power)
'freeze 2D2 lattice
analogdata.AddSmoothRamp(lattice2_ramp_end_v, lattice2_deepest_volt, lattice2_freeze_start_time, lattice2_freeze_end_time, lattice2D765_power2)
analogdata.AddStep(lattice2_deepest_volt, lattice2_freeze_end_time, pinning_ready_time, lattice2D765_power2)


'----------------------------------------------------- Magnetic Fields II ---------------------------------------------------------------------------------

'turn off quic
If (quic_ramp_end_v > 0) Then
    analogdata2.AddStep(quic_ramp_end_v * ps5_scaler, ramp_end_time, grad_turnoff_start_time, ps5_ao)
    analogdata2.AddSmoothRamp(quic_ramp_end_v * ps5_scaler, 0, grad_turnoff_start_time, grad_turnoff_end_time, ps5_ao)
End If

'turn off quad
If (quad_ramp_start_v > 0) Then
    analogdata2.AddStep(quad_ramp_end_v, ramp_end_time, grad_turnoff_start_time, ps8_ao)
    analogdata2.AddSmoothRamp(quad_ramp_end_v, 0, grad_turnoff_start_time, grad_turnoff_end_time, ps8_ao)
End If


'----------------------------------------------------- DMD Code -------------------------------------------------------------------------------------------

'Alex hack
'this line to bring trigger active high, but also serve as the tracking line trigger (on its falling edge, with 6ms delay)
digitaldata2.AddPulse(line_DMD_trigger, evaporation_end_time, molasses_start_time + fluo_image_wait ) 'tracking line
digitaldata2.AddPulse(hor_DMD_trigger, evaporation_end_time, molasses_start_time + fluo_image_wait ) 'tracking hor

'now all these are inverted and triggers on the second time stamp (real rising edge)
'pattern switching after a 160us delay, switching itself takes few us or less
digitaldata2.AddPulse(line_DMD_trigger, DMD_hw_delay + line_load_vert_start_time - 0.5, DMD_hw_delay + line_load_vert_start_time) 'cut first direction (line DMD)
digitaldata2.AddPulse(hor_DMD_trigger, DMD_hw_delay + line_load_horz_start_time - 0.5, DMD_hw_delay + line_load_horz_start_time) 'cut second direction (hor DMD)
digitaldata2.AddPulse(line_DMD_trigger, DMD_hw_delay + walls1_turnon_start_time - 0.5, DMD_hw_delay + walls1_turnon_start_time) 'walls1 (line DMD)
digitaldata2.AddPulse(hor_DMD_trigger, DMD_hw_delay + walls2_turnon_start_time - 0.5, DMD_hw_delay + walls2_turnon_start_time) 'walls2 (hor DMD)
digitaldata2.AddPulse(line_DMD_trigger, DMD_hw_delay + cutting1_turnon2_start_time - 0.5, DMD_hw_delay + cutting1_turnon2_start_time) 'cut for cleanup (line DMD)
digitaldata2.AddPulse(line_DMD_trigger, DMD_hw_delay + Berlin_wall_turnon_start_time - 0.5, DMD_hw_delay + Berlin_wall_turnon_start_time) 'Berlin wall (line DMD)

'shutter
digitaldata2.AddPulse(line_DMD_shutter, cutting1_turnon_start_time - 20, molasses_start_time)
digitaldata2.AddPulse(hor_DMD_shutter, cutting2_turnon_start_time - 20, molasses_start_time)

'cut one direction with line DMD
If (anticonfine_dur_vert > 0) Then
    digitaldata2.AddPulse(line_DMD_ttl, cutting1_turnon_start_time - PID_response_dur/2, line_load_end_time + PID_response_dur/2) 
	analogdata2.AddSmoothRamp(line_DMD_start_volt, loadline_DMD_volt_vert, cutting1_turnon_start_time, cutting1_turnon_end_time, line_DMD_power)
	analogdata2.AddStep(loadline_DMD_volt_vert, cutting1_turnon_end_time, cutting1_turnoff_start_time, line_DMD_power)
	analogdata2.AddSmoothRamp(loadline_DMD_volt_vert, line_DMD_start_volt, cutting1_turnoff_start_time, cutting1_turnoff_end_time, line_DMD_power)
End If

'cut second direction with hor DMD
If (anticonfine_dur_horz > 0) Then
    digitaldata2.AddPulse(hor_DMD_ttl, cutting2_turnon_start_time - PID_response_dur / 2, line_load_end_time + PID_response_dur / 2)
    analogdata2.AddSmoothRamp(line_DMD_start_volt, loadline_DMD_volt_horz, cutting2_turnon_start_time, cutting2_turnon_end_time, hor_DMD_power)
    analogdata2.AddStep(loadline_DMD_volt_horz, cutting2_turnon_end_time, cutting2_turnoff_start_time, hor_DMD_power)
    analogdata2.AddSmoothRamp(loadline_DMD_volt_horz, line_DMD_start_volt, cutting2_turnoff_start_time, cutting2_turnoff_end_time, hor_DMD_power)
End If

'walls1 (line DMD)
If (walls1_volt > 0) Then
    digitaldata2.AddPulse(line_DMD_ttl, walls1_turnon_start_time - PID_response_dur / 2, walls_turnoff_end_time + PID_response_dur / 2)
    'turn on and hold
    analogdata2.AddSmoothRamp(line_DMD_start_volt, walls1_volt, walls1_turnon_start_time, walls1_turnon_end_time, line_DMD_power)
    analogdata2.AddStep(walls1_volt, walls1_turnon_end_time, walls_turnoff_start_time, line_DMD_power)
    'turn off
    analogdata2.AddSmoothRamp(walls1_volt, line_DMD_start_volt, walls_turnoff_start_time, walls_turnoff_end_time, line_DMD_power)
End If

'walls2 (hor DMD)
If (walls2_volt > 0) Then
    digitaldata2.AddPulse(hor_DMD_ttl, walls2_turnon_start_time - PID_response_dur / 2, walls_turnoff_end_time + PID_response_dur / 2)
    'turn on and hold
    analogdata2.AddSmoothRamp(line_DMD_start_volt, walls2_volt, walls2_turnon_start_time, walls2_turnon_end_time, hor_DMD_power)
    analogdata2.AddStep(walls2_volt, walls2_turnon_end_time, walls_turnoff_start_time, hor_DMD_power)
    'turn off
    analogdata2.AddSmoothRamp(walls2_volt, line_DMD_start_volt, walls_turnoff_start_time, walls_turnoff_end_time, hor_DMD_power)
End If

'cleanup (line DMD)
If (is_cleanup > 0) Then 
    digitaldata2.AddPulse(line_DMD_ttl, cutting1_turnon2_start_time - PID_response_dur / 2, cutting1_turnoff2_end_time + PID_response_dur / 2) ' CORRECTED ON 2024/02/21 (was not present)
    analogdata2.AddSmoothRamp(line_DMD_start_volt, loadline_DMD_volt_vert, cutting1_turnon2_start_time, cutting1_turnon2_end_time, line_DMD_power)
    analogdata2.AddStep(loadline_DMD_volt_vert, cutting1_turnon2_end_time, cutting1_turnoff2_start_time, line_DMD_power)
    analogdata2.AddSmoothRamp(loadline_DMD_volt_vert, line_DMD_start_volt, cutting1_turnoff2_start_time, cutting1_turnoff2_end_time, line_DMD_power)
End If

'Berlin wall (line DMD)
If (is_counting > 0) Then 
    digitaldata2.AddPulse(line_DMD_ttl, Berlin_wall_turnon_start_time - PID_response_dur / 2, Berlin_wall_turnoff_end_time + PID_response_dur / 2) ' CORRECTED ON 2024/02/21 (was not present)
    analogdata2.AddSmoothRamp(line_DMD_start_volt, Berlin_wall_kick_volt, Berlin_wall_turnon_start_time, Berlin_wall_turnon_end_time, line_DMD_power)
    analogdata2.AddStep(Berlin_wall_kick_volt, Berlin_wall_turnon_end_time, Berlin_wall_turnoff_start_time, line_DMD_power)
    analogdata2.AddSmoothRamp(Berlin_wall_kick_volt, line_DMD_start_volt, Berlin_wall_turnoff_start_time, Berlin_wall_turnoff_end_time, line_DMD_power)
End If


'----------------------------------------------------- Scope trigger --------------------------------------------------------------------------------------

Dim scope_trigger As Double = pinning_start_time
digitaldata.AddPulse(64, scope_trigger, scope_trigger + 10)
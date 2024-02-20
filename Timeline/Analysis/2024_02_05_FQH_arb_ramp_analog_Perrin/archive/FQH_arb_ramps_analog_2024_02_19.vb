'===================
'=====Variables=====
quic_init = 0.5
quad_init = 0
override_default = 1
dur_idx = 1
var_idx = 4
var_value = 3.66
dur_value = 75.981
is_return = 0
reverse_ramp = 0
return_half = 0
'=====Variables=====

'----------------------------------------------------- Arbitrary ramp constants --------------------------------------------------------------
'---------------------------------------------------------------------------------------------------------------------------------------------

'arb ramp constants

'Dim rampSegPath As String = "Z:\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\ramp_segments.txt"
'Dim rampSegPath_return As String = "Z:\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\ramp_segments_adiabatic.txt"
'Dim lattice2_JtoDepth_coeff_path As String = "Z:\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\j_to_depth_2d2lattice.txt"
'Dim gauge_JtoVolt_coeff_path As String = "Z:\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\j_to_v_gaugepower.txt"

Dim rampSegPath As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\ramp_segments.txt"
Dim rampSegPath_return As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\ramp_segments_adiabatic.txt"
Dim lattice2_JtoDepth_coeff_path As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\j_to_depth_2d2lattice.txt"
Dim gauge_JtoVolt_coeff_path As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\j_to_v_gaugepower.txt"

Dim n_variables As Integer = 6 ' number of channels used in the arbitrary ramp

'----------------------------------------------------- End arbitrary ramp constants --------------------------------------------------------------
'---------------------------------------------------------------------------------------------------------------------------------------------

Dim lattice1_max As Double = 45
Dim lattice2_max As Double = 45
Dim lattice1_max_volt As Double = lattice1_voltage_offset+lattice1_calib_volt+.5*Log10(lattice1_max/lattice1_calib_depth)
Dim lattice2_max_volt As Double = lattice2_voltage_offset+lattice2_calib_volt+.5*Log10(lattice2_max/lattice2_calib_depth)

'former variables
'Dim is_return As Boolean = 0
Dim quic_ramp_dur As Double = 60
Dim walls2_volt As Double = 2.4
Dim lattice2_ramp_dur As Double = 50
Dim lowered_lattice2_depth As Double = 8
Dim lattice2_final_depth As Double = 8
Dim lowered_lattice1_depth As Double = 5
Dim lower_lattice1_ramp_dur As Double = 50
Dim initial_walls1_volt As Double = 3.1
Dim lowered_walls1_volt As Double = 3.1

'----------------------------------------------------- constants to sort --------------------------------------------------------------------

Dim shunt_switch_dur As Double = 10
Dim coil_ramp_dur As Double = 30

'Cutting constants
Dim gravity_offsets_switch_dur As Double = 10
Dim twod_ramp_dur As Double = 5
Dim anticonfine_volt As Double = 2.5
Dim anticonfine_ramp_dur As Double = 1
Dim lattice1_amb_volt As Double = 1.4
Dim lattice2_amb_volt As Double = 1.8
Dim anticonfine_dur_vert As Double = 40
Dim anticonfine_dur_horz As Double = 40

'DMD specific durations
Dim PID_response_dur As Double = 1 'needed to fix spikes in line_DMD
Dim loadline_DMD_volt_vert As Double = 3.3
Dim loadline_DMD_volt_horz As Double = 3.1 '3.1

Dim lowered_lattice1_volt As Double 
lowered_lattice1_volt = DepthToVolts(lowered_lattice1_depth, lattice1_calib_depth, lattice1_calib_volt, lattice1_voltage_offset)
Dim lowered_lattice2_volt As Double 
lowered_lattice2_volt = DepthToVolts(lowered_lattice2_depth, lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)

'----------------------------------------------------- Time Definitions PLACEHOLDER ----------------------------------------------------------

Dim twodphysics_start_time As Double = 0

'----------------------------------------------------- Time Definitions for Sequence ---------------------------------------------------------
'initialize values of everything needed for ramp...should end on values for start of ramp
'(1) Ramp up everything to the initial values (may need to remove for final sequence, just for testing purposes)
' Turn on quic gradient
Dim quic_turnon_start_time As Double = twodphysics_start_time
Dim quic_turnon_end_time As Double = quic_turnon_start_time + quic_ramp_dur ' raise from 0 to quic_init
' Turn on quad gradient
Dim quad_turnon_start_time As Double = quic_turnon_start_time
Dim quad_turnon_end_time As Double = quic_turnon_end_time ' raise from 0 to quad_init

'---------------------------------------------------------------------------------------------------------------------------------------------
'----------------------------------------------------- Arb ramp time definitions -------------------------------------------------------------
'(3) Arbitrary ramp
'TO DO: move ramp variable definitions to top, keep time definitions here!

Dim lattice2_JtoDepth_coeffs As Double() = LoadArrayFromFile(lattice2_JtoDepth_coeff_path) 'TO DO: write this sub!
Dim nterms_2D2 As Double = lattice2_JtoDepth_coeffs.GetUpperBound(0)
Console.WriteLine("number of lattice depth expansion terms (minus 1): {0}", nterms_2D2)

Dim gauge_JtoVolt_coeffs As Double() = LoadArrayFromFile(gauge_JtoVolt_coeff_path)
Dim nterms_gauge As Double = gauge_JtoVolt_coeffs.GetUpperBound(0)
Console.WriteLine("number of gauge power expansion terms (minus 1): {0}", nterms_gauge)

Dim ramp_start_time As Double = quic_turnon_end_time
Dim ramp_variables = LoadRampSegmentsFromFile(rampSegPath, n_variables)
Dim n_times As Integer = ramp_variables(0).GetUpperBound(0)
Console.WriteLine("N times: {0}", n_times)

' overwrite appropriate variables
If override_default > 0
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

Dim ramp_durs As Double() = ramp_variables(0)
Dim ramp_t(n_times) As Double
ramp_t(0) = ramp_start_time
For index As Integer = 1 To n_times
    ramp_t(index) = ramp_t(index - 1) + ramp_durs(index)
Next
Dim ramp_forward_end_time As Double = ramp_t(n_times)

Console.WriteLine("ramp forward end time = {0}", ramp_forward_end_time)

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
Dim gauge_power_ramp_forward_j As Double = gauge_power_ramp_j(n_times)
Dim gauge_freq_ramp_forward_v As Double = gauge_freq_ramp_v(n_times)
Dim quad_ramp_forward_v As Double = quad_ramp_v(n_times)
Dim quic_ramp_forward_v As Double = quic_ramp_v(n_times)

Dim ramp_end_time As Double
Dim lattice1_ramp_end_v As Double
Dim lattice2_ramp_end_j As Double
Dim gauge_power_ramp_end_j As Double
Dim gauge_freq_ramp_end_v As Double
Dim quad_ramp_end_v As Double
Dim quic_ramp_end_v As Double

Dim ramp_t_return(-1) As Double
Dim lattice1_ramp_v_return(-1) As Double
Dim lattice2_ramp_j_return(-1) As Double
Dim gauge_power_ramp_j_return(-1) As Double
Dim gauge_freq_ramp_v_return(-1) As Double  
Dim quad_ramp_v_return(-1) As Double
Dim quic_ramp_v_return(-1) As Double
Dim n_times_return As Integer

If (is_return > 0) Then 'return ramp different? Replace with separate file?
    'get times for return ramp
    Dim ramp_variables_return As Double()()   
    Dim ramp_durs_return As Double()

    If (reverse_ramp > 0)
        ramp_variables_return = ramp_variables
        n_times_return = n_times
        ramp_durs_return = ramp_durs
    Else
        ramp_variables_return = LoadRampSegmentsFromFile(rampSegPath_return, n_variables)
        n_times_return = ramp_variables_return(0).GetUpperBound(0)        
        ramp_durs_return = ramp_variables_return(0)
    End If
    Console.WriteLine("N times return: {0}", n_times_return) 
    ReDim ramp_t_return(n_times_return)
    ramp_t_return(0) = ramp_forward_end_time
    For index As Integer = 1 To n_times_return
        ramp_t_return(index) = ramp_t_return(index - 1) + ramp_durs_return(n_times_return + 1 - index)
    Next
    
    ReDim lattice1_ramp_v_return(n_times_return)
    ReDim lattice2_ramp_j_return(n_times_return)
    ReDim gauge_power_ramp_j_return(n_times_return)
    ReDim gauge_freq_ramp_v_return(n_times_return)  
    ReDim quad_ramp_v_return(n_times_return)
    ReDim quic_ramp_v_return(n_times_return)

    For index As Integer = 0 To n_times_return
        lattice1_ramp_v_return(index) = DepthToVolts(ramp_variables_return(1)(n_times_return - index), lattice1_calib_depth, lattice1_calib_volt, lattice1_voltage_offset)  
        gauge_power_ramp_j_return(index) = ramp_variables_return(2)(n_times_return - index)/1240 'in E_r       
        lattice2_ramp_j_return(index) = ramp_variables_return(3)(n_times_return - index)/1240 'in E_r
        gauge_freq_ramp_v_return(index) = BeatVolt(ramp_variables(4)(n_times_return - index))
        quad_ramp_v_return(index) = ramp_variables_return(5)(n_times_return - index)
        quic_ramp_v_return(index) = ramp_variables_return(6)(n_times_return - index)
    Next
    
    ramp_end_time = ramp_t_return(n_times_return)
    lattice1_ramp_end_v = lattice1_ramp_v_return(n_times_return)
    lattice2_ramp_end_j = lattice2_ramp_j_return(n_times_return)
    gauge_power_ramp_end_j = gauge_power_ramp_j_return(n_times_return)
    gauge_freq_ramp_end_v = gauge_freq_ramp_v_return(n_times_return)
    quad_ramp_end_v = quad_ramp_v_return(n_times_return)
    quic_ramp_end_v = quic_ramp_v_return(n_times_return)

Else
    ramp_end_time = ramp_forward_end_time
    lattice1_ramp_end_v = lattice1_ramp_forward_v
    lattice2_ramp_end_j = lattice2_ramp_forward_j
    gauge_power_ramp_end_j = gauge_power_ramp_forward_j
    gauge_freq_ramp_end_v = gauge_freq_ramp_forward_v
    quad_ramp_end_v = quad_ramp_forward_v
    quic_ramp_end_v = quic_ramp_forward_v
End If

Dim lattice1_ramp_start_v = lattice1_ramp_v(0)

'Dim gauge_power_start_v As Double = GaugeJToVolt(gauge_power_ramp_j(0), gauge_JtoVolt_coeffs,n_variables+1)
'Dim gauge_power_ramp_end_v As Double = GaugeJToVolt(gauge_power_ramp_end_j, gauge_JtoVolt_coeffs,n_variables+1)

'Dim lattice2_ramp_start_v As Double = JToVolt(lattice2_ramp_j(0), lattice2_JtoDepth_coeffs, 11, lattice1_calib_depth, lattice1_calib_volt, lattice1_voltage_offset)
'Dim lattice2_ramp_end_v As Double = JToVolt(lattice2_ramp_end_j, lattice2_JtoDepth_coeffs, 11, lattice1_calib_depth, lattice1_calib_volt, lattice1_voltage_offset)

Dim gauge_freq_ramp_start_v As Double = gauge_freq_ramp_v(0)
Dim quad_ramp_start_v As Double = quad_ramp_v(0)
Dim quic_ramp_start_v As Double = quic_ramp_v(0)

'Console.WriteLine("quad_ramp_start_v: {0}", quad_ramp_start_v)
'Console.WriteLine("quad_ramp_end_v: {0}", quad_ramp_end_v)
'Console.WriteLine("quad_ramp_v(n_times-4): {0}", quad_ramp_v(n_times-4))

' Final values:
' ramp_end_time
' lattice1_ramp_end_v
' gauge_power_ramp_end_v
' lattice2_ramp_end_v
' gauge_freq_ramp_end_v
' quad_ramp_end_v
' quic_ramp_end_v

'ramp ends on last value: need to ramp down to turn off
'need to add freeze lattice

Console.WriteLine("ramp end time = {0}", ramp_end_time)
'---------------------------------------------------------------------------------------------------------------------------------------------
'----------------------------------------------------- End arb ramp time defs ----------------------------------------------------------------


'----------------------------------------------------- Pinning -------------------------------------------------------------------------------

'(3) pinning
Dim freeze_lattice_ramp_dur As Double = 1000
Dim pinning_dur As Double = 10000
Dim pinning_start_time As Double = ramp_end_time + freeze_lattice_ramp_dur
Dim pinning_end_time As Double = pinning_start_time + pinning_dur

Dim IT As Double = pinning_end_time


'---------------------------------------------------------------------------------------------------------------------------------------------
'---------------------------------------------------------------------------------------------------------------------------------------------

analogdata.DisableClkDist(0.95, 4.05)
'analogdata2.DisableClkDist(0.95, 4.05)


'----------------------------------------------------- Arbitrary Ramps -----------------------------------------------------------------------
'---------------------------------------------------------------------------------------------------------------------------------------------
'Linear interpolation to connect the ramp end points

'2D1 lattice power
analogdata.AddLogRamp(lattice1_max, lattice1_ramp_v(1), ramp_start_time, ramp_t(1), lattice2D765_power)
For index As Integer = 1 To n_times - 1
    analogdata.AddLogRamp(lattice1_ramp_v(index), lattice1_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), lattice2D765_power)
Next

'gauge power
'analogdata.AddTunnelGaugeRamp(gauge_JtoVolt_coeffs, gauge_power_ramp_j(0), gauge_power_ramp_j(1), ramp_start_time, ramp_t(1), gauge1_power) 'needs its own function
'For index As Integer = 1 To n_times - 1
'    analogdata.AddTunnelGaugeRamp(gauge_JtoVolt_coeffs, gauge_power_ramp_j(index), gauge_power_ramp_j(index + 1), ramp_t(index), ramp_t(index + 1), gauge1_power)
'    Console.WriteLine("new gauge power ramp J: {0}", gauge_power_ramp_j(index + 1))
'Next
For index As Integer = 0 To n_times - 1
    analogdata.AddRamp(gauge_power_ramp_j(index), gauge_power_ramp_j(index + 1), ramp_t(index), ramp_t(index + 1), gauge1_power)
    Console.WriteLine("new gauge power ramp J: {0}", gauge_power_ramp_j(index + 1))
Next

'2D2 lattice power
analogdata.AddTunnelRamp(lattice2_JtoDepth_coeffs, lattice2_max, lattice2_ramp_j(1), ramp_start_time, ramp_t(1), lattice2_voltage_offset, lattice2_calib_volt, lattice2_calib_depth, lattice2D765_power2)
For index As Integer = 1 To n_times - 1
    analogdata.AddTunnelRamp(lattice2_JtoDepth_coeffs, lattice2_ramp_j(index), lattice2_ramp_j(index + 1), ramp_t(index), ramp_t(index + 1), lattice2_voltage_offset, lattice2_calib_volt, lattice2_calib_depth, lattice2D765_power2)
Next

'quic grad
analogdata.AddRamp(quic_ramp_v(0), quic_ramp_v(1), ramp_start_time, ramp_t(1), ps1_ao) 'ps5_ao
For index As Integer = 1 To n_times - 1
    analogdata.AddRamp(quic_ramp_v(index), quic_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), ps1_ao) 'ps5_ao
Next

'quad grad
analogdata.AddRamp(quad_ramp_v(0), quad_ramp_v(1), ramp_start_time, ramp_t(1), ps3_ao) 'ps8_ao
For index As Integer = 1 To n_times - 1
    analogdata.AddRamp(quad_ramp_v(index), quad_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), ps3_ao) 'ps8_ao
    'Console.WriteLine("new quad ramp V: {0}", quad_ramp_v(index+1))
Next

'gauge detuning
analogdata.AddRamp(0, gauge_freq_ramp_v(1), ramp_start_time, ramp_t(1), gauge2_rf_fm)
For index As Integer = 1 To n_times - 1
    analogdata.AddRamp(gauge_freq_ramp_v(index), gauge_freq_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), gauge2_rf_fm)
Next

If (is_return > 0) Then
    ' ramp backward for return
    '2D1 lattice power
    For index As Integer = 0 To n_times_return - 1
        analogdata.AddLogRamp(lattice1_ramp_v_return(index), lattice1_ramp_v_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), lattice2D765_power)
    Next

    'gauge power
    'For index As Integer = 0 To n_times_return - 1
    '    analogdata.AddTunnelGaugeRamp(gauge_JtoVolt_coeffs, gauge_power_ramp_j_return(index), gauge_power_ramp_j_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), gauge1_power)
    'Next
    For index As Integer = 0 To n_times_return - 1
        analogdata.AddRamp(gauge_power_ramp_j_return(index), gauge_power_ramp_j_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), gauge1_power)
    Next

    '2D2 lattice power
    For index As Integer = 0 To n_times_return - 1
        analogdata.AddTunnelRamp(lattice2_JtoDepth_coeffs, lattice2_ramp_j_return(index), lattice2_ramp_j_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), lattice2_voltage_offset, lattice2_calib_volt, lattice2_calib_depth, lattice2D765_power2)
    Next

    'quic grad
    For index As Integer = 0 To n_times_return - 1
        analogdata.AddRamp(quic_ramp_v_return(index), quic_ramp_v_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), ps1_ao) 'ps5_ao
    Next

    'quad grad
    For index As Integer = 0 To n_times_return - 1
        analogdata.AddRamp(quad_ramp_v_return(index), quad_ramp_v_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), ps3_ao) 'ps8_ao
    Next

    'gauge detuning
    For index As Integer = 0 To n_times_return - 1
        analogdata.AddRamp(gauge_freq_ramp_v_return(index), gauge_freq_ramp_v_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), gauge2_rf_fm)
    Next
End If

'----------------------------------------------------- End Arbitrary Ramps -------------------------------------------------------------------
'---------------------------------------------------------------------------------------------------------------------------------------------

'----------------------------------------------------- Pinning -------------------------------------------------------------------------------
analogdata.AddRamp(quad_ramp_end_v, 0, ramp_end_time, pinning_start_time, ps3_ao)
analogdata.AddRamp(quic_ramp_end_v, 0, ramp_end_time, pinning_start_time, ps1_ao)

'pinning_start_time = ramp_end_time
'Bring everything back up for pinning
' 2D1 
'analogdata.AddSmoothRamp(lattice1_ramp_v(n_times), lattice1_max_volt, ramp_end_time, pinning_start_time, lattice2D765_power)
analogdata.AddStep(lattice1_max_volt, pinning_start_time, pinning_end_time, lattice2D765_power)
' 2D2 raise to final depth and hold
'analogdata.AddSmoothRamp(lattice2_ramp_j(n_times), lattice2_max_volt, ramp_end_time, pinning_start_time, lattice2D765_power2)
analogdata.AddStep(lattice2_max_volt, pinning_start_time, pinning_end_time, lattice2D765_power2)


'---------------------------------------------------------------------------------------------------------------------------------------------
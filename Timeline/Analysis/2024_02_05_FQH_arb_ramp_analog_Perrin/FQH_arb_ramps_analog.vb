'===================
'=====Variables=====
quic_init = 0.5
quad_init = 0
override_default = 0
dur_idx = 7
var_idx = 5
var_value = 0.2
dur_value = 200
stop_index = 10
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
Dim half_index As Integer = 4 ' point at which y delocalization finishes and x delocalization starts
Dim half_index_return As Integer = 4 ' point at which y delocalization finishes and x delocalization starts for return ramp
If reverse_ramp > 0 Then
    half_index_return = half_index
End If

Dim gauge_calib_volt As Double = 3.34 'accuracy of depth to volt calibration doesn't matter much as long as consistant values are used here and in matlab file. Accuracy of conversion is still limited by quantum walk calibration.
Dim gauge_calib_depth As Double = 7.4/1.24 ' same for this value...


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

Dim lowered_lattice1_volt As Double 
lowered_lattice1_volt = DepthToVolts(lowered_lattice1_depth, lattice1_calib_depth, lattice1_calib_volt, lattice1_voltage_offset)
Dim lowered_lattice2_volt As Double 
lowered_lattice2_volt = DepthToVolts(lowered_lattice2_depth, lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)

'----------------------------------------------------- Variable definitions for sequence -----------------------------------------------------

Dim lattice2_JtoDepth_coeffs As Double() = LoadArrayFromFile(lattice2_JtoDepth_coeff_path)
Dim nterms_2D2 As Double = lattice2_JtoDepth_coeffs.GetUpperBound(0)
Console.WriteLine("number of lattice depth expansion terms (minus 1): {0}", nterms_2D2)

Dim gauge_JtoVolt_coeffs As Double() = LoadArrayFromFile(gauge_JtoVolt_coeff_path)
Dim nterms_gauge As Double = gauge_JtoVolt_coeffs.GetUpperBound(0)
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
Dim gauge_power_ramp_forward_j As Double = gauge_power_ramp_j(n_times)
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
'Dim ramp_durs_return(-1) As Double
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

Dim gauge_power_ramp_start_v As Double = GaugeJToVolts(gauge_power_ramp_j(0), gauge_JtoVolt_coeffs, nterms_gauge, gauge_calib_depth, gauge_calib_volt)
Dim gauge_power_ramp_end_v As Double = GaugeJToVolts(gauge_power_ramp_end_j, gauge_JtoVolt_coeffs, nterms_gauge, gauge_calib_depth, gauge_calib_volt)
Console.WriteLine("GaugeJToVolts: {0} -> {1}", gauge_power_ramp_j(0), gauge_power_ramp_start_v)
Console.WriteLine("GaugeJToVolts: {0} -> {1}", gauge_power_ramp_end_j, gauge_power_ramp_end_v)
Dim lattice2_ramp_start_v As Double = JToVolts(lattice2_ramp_j(0), lattice2_JtoDepth_coeffs, nterms_2D2, lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)
Dim lattice2_ramp_end_v As Double = JToVolts(lattice2_ramp_end_j, lattice2_JtoDepth_coeffs, nterms_2D2, lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)
Console.WriteLine("2D2 JToVolts: {0} -> {1}", lattice2_ramp_j(0), lattice2_ramp_start_v)
Console.WriteLine("2D2 JToVolts: {0} -> {1}", lattice2_ramp_end_j, lattice2_ramp_end_v)


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


'----------------------------------------------------- Time Definitions PLACEHOLDER ----------------------------------------------------------

Dim twodphysics_start_time As Double = 0

'----------------------------------------------------- Time Definitions for Sequence ---------------------------------------------------------
'initialize values of everything needed for ramp...should end on values for start of ramp

'Ramp up everything to the initial values (may need to remove for final sequence, just for testing purposes)
' Turn on quic gradient
Dim quic_turnon_start_time As Double = twodphysics_start_time
Dim quic_turnon_end_time As Double = quic_turnon_start_time + quic_ramp_dur ' raise from 0 to quic_init
' Turn on quad gradient
Dim quad_turnon_start_time As Double = quic_turnon_start_time
Dim quad_turnon_end_time As Double = quic_turnon_end_time ' raise from 0 to quad_init


'---------------------------------------------------------------------------------------------------------------------------------------------
'----------------------------------------------------- Arb ramp time definitions -------------------------------------------------------------

'Arbitrary ramp
Dim ramp_start_time As Double = quic_turnon_end_time
Dim ramp_end_time As Double
Dim ramp_t(n_times) As Double
ramp_t(0) = ramp_start_time
For index As Integer = 1 To n_times
    ramp_t(index) = ramp_t(index - 1) + ramp_durs(index)
Next
Dim ramp_forward_end_time As Double = ramp_t(n_times)
Console.WriteLine("ramp forward end time = {0}", ramp_forward_end_time)

'return
Dim ramp_t_return(-1) As Double
If is_return > 0 Then
    ReDim ramp_t_return(n_times_return)
    ramp_t_return(0) = ramp_forward_end_time
    For index As Integer = 1 To n_times_return
        ramp_t_return(index) = ramp_t_return(index - 1) + ramp_durs_return(n_times_return + 1 - index)
        'Console.WriteLine("ramp return index: {0}, ramp duration index: {1}", index, n_times_return+1-index)
    Next
    ramp_end_time = ramp_t_return(n_times_return)
Else
    ramp_end_time = ramp_forward_end_time
End If
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
For index As Integer = 0 To n_times - 1
    'analogdata.AddTunnelGaugeRamp(gauge_JtoVolt_coeffs, 0, 0.0323, ramp_t(index), ramp_t(index + 1), gauge_calib_volt, gauge_calib_depth, ps1_ao) 'gauge1_power
    'analogdata.AddTunnelGaugeRamp(gauge_JtoVolt_coeffs, gauge_power_ramp_j(0), gauge_power_ramp_j(0), ramp_t(index), ramp_t(index + 1), gauge_calib_volt, gauge_calib_depth, ps1_ao) 'gauge1_power
    analogdata.AddTunnelGaugeRamp(gauge_JtoVolt_coeffs, gauge_power_ramp_j(index), gauge_power_ramp_j(index + 1), ramp_t(index), ramp_t(index + 1), gauge_calib_volt, gauge_calib_depth, ps1_ao) 'gauge1_power
    'analogdata.AddTunnelRamp(lattice2_JtoDepth_coeffs, gauge_power_ramp_j(index), gauge_power_ramp_j(index + 1), ramp_t(index), ramp_t(index + 1), lattice2_voltage_offset, lattice2_calib_volt, lattice2_calib_depth, ps1_ao)
    'Console.WriteLine("    new gauge power ramp J: {0}", gauge_power_ramp_j(index + 1))
    'Console.WriteLine("                     and V: {0}", GaugeJToVolts(gauge_power_ramp_j(index + 1), gauge_JtoVolt_coeffs, nterms_gauge, gauge_calib_depth, gauge_calib_volt))
Next

'2D2 lattice power
For index As Integer = 0 To n_times - 1
    analogdata.AddTunnelRamp(lattice2_JtoDepth_coeffs, lattice2_ramp_j(index), lattice2_ramp_j(index + 1), ramp_t(index), ramp_t(index + 1), lattice2_voltage_offset, lattice2_calib_volt, lattice2_calib_depth, ps3_ao) 'lattice2D765_power2
Next

'quic grad
For index As Integer = 0 To n_times - 1
    analogdata.AddRamp(quic_ramp_v(index), quic_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), ps5_ao) 'ps5_ao
Next

'quad grad
'analogdata.AddRamp(quad_ramp_v(0), quad_ramp_v(1), ramp_start_time, ramp_t(1), ps8_ao) 'ps8_ao
For index As Integer = 0 To n_times - 1
    analogdata.AddRamp(quad_ramp_v(index), quad_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), ps8_ao) 'ps8_ao
    'Console.WriteLine("new quad ramp V: {0}", quad_ramp_v(index+1))
Next

'gauge detuning
'analogdata.AddRamp(0, gauge_freq_ramp_v(1), ramp_start_time, ramp_t(1), gauge2_rf_fm)
For index As Integer = 0 To n_times - 1
    analogdata.AddRamp(gauge_freq_ramp_v(index), gauge_freq_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), gauge2_rf_fm)
Next

If (is_return > 0) Then
    ' ramp backward for return
    '2D1 lattice power
    For index As Integer = 0 To n_times_return - 1
        analogdata.AddLogRamp(lattice1_ramp_v_return(index), lattice1_ramp_v_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), lattice2D765_power)
    Next

    'gauge power
    For index As Integer = 0 To n_times_return - 1
        analogdata.AddTunnelGaugeRamp(gauge_JtoVolt_coeffs, gauge_power_ramp_j_return(index), gauge_power_ramp_j_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), gauge_calib_volt, gauge_calib_depth, ps1_ao) 'gauge1_power
    Next
    'For index As Integer = 0 To n_times_return - 1
    '    analogdata.AddTunnelRamp(lattice2_JtoDepth_coeffs, gauge_power_ramp_j_return(index), gauge_power_ramp_j_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), lattice2_voltage_offset, gauge_calib_volt, gauge_calib_depth, gauge1_power) 'gauge1_power
    'Next

    '2D2 lattice power
    For index As Integer = 0 To n_times_return - 1
        analogdata.AddTunnelRamp(lattice2_JtoDepth_coeffs, lattice2_ramp_j_return(index), lattice2_ramp_j_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), lattice2_voltage_offset, lattice2_calib_volt, lattice2_calib_depth, ps3_ao) 'lattice2D765_power2
    Next

    'quic grad
    For index As Integer = 0 To n_times_return - 1
        analogdata.AddRamp(quic_ramp_v_return(index), quic_ramp_v_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), ps5_ao) 'ps5_ao
    Next

    'quad grad
    For index As Integer = 0 To n_times_return - 1
        analogdata.AddRamp(quad_ramp_v_return(index), quad_ramp_v_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), ps8_ao) 'ps8_ao
    Next

    'gauge detuning
    For index As Integer = 0 To n_times_return - 1
        analogdata.AddRamp(gauge_freq_ramp_v_return(index), gauge_freq_ramp_v_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), gauge2_rf_fm)
    Next
End If

'----------------------------------------------------- End Arbitrary Ramps -------------------------------------------------------------------
'---------------------------------------------------------------------------------------------------------------------------------------------

'----------------------------------------------------- Pinning -------------------------------------------------------------------------------
analogdata.AddRamp(quad_ramp_end_v, 0, ramp_end_time, pinning_start_time, ps8_ao) 'ps8_ao
analogdata.AddRamp(quic_ramp_end_v, 0, ramp_end_time, pinning_start_time, ps5_ao) 'ps5_ao

'pinning_start_time = ramp_end_time
'Bring everything back up for pinning
' 2D1 
analogdata.AddRamp(lattice1_ramp_end_v, lattice1_max_volt, ramp_end_time, pinning_start_time, lattice2D765_power)
analogdata.AddStep(lattice1_max_volt, pinning_start_time, pinning_end_time, lattice2D765_power)
' 2D2 raise to final depth and hold
'analogdata.AddRamp(lattice2_ramp_end_v, lattice2_max_volt, ramp_end_time, pinning_start_time, ps3_ao)  'lattice2D765_power2
'analogdata.AddStep(lattice2_max_volt, pinning_start_time, pinning_end_time, ps3_ao) 'lattice2D765_power2


'---------------------------------------------------------------------------------------------------------------------------------------------

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
'=====Variables=====

'----------------------------------------------------- Arbitrary ramp constants --------------------------------------------------------------
'---------------------------------------------------------------------------------------------------------------------------------------------

'arb ramp constants
Dim rampSegPath As String = "Z:\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\ramp_segments.txt"
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

'----------------------------------------------------- Time Definitions for Cutting ----------------------------------------------------------

'(0) cutting
'Dim cutting_dur As Double = 1000
'Dim cutting_start_time As Double = 0
'Dim cutting_end_time As Double = cutting_start_time + cutting_dur 

'Time constants for cutting vertical line
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
'skip if zero
If (anticonfine_dur_vert = 0) Then
	line_load_mid_time = line_load_vert_start_time + 20 + PID_response_dur 
End If

'time constants for cutting horizontal line
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
Dim walls2_start_time as Double = cutting2_turnoff_end_time + 5
Dim line_load_end_time As Double = cutting2_turnoff_end_time + gravity_offsets_switch_dur 'ramp gravity_offset to zero for expel
'skip if zero
If (anticonfine_dur_horz = 0) Then
	line_load_end_time = line_load_mid_time + 20 + PID_response_dur
End If

'----------------------------------------------------- Time Definitions for Sequence ---------------------------------------------------------

'(1) Ramp up everything to the initial values (may need to remove for final sequence, just for testing purposes)
' Turn on quic gradient
Dim quic_turnon_start_time As Double = line_load_end_time
Dim quic_turnon_end_time As Double = quic_turnon_start_time + quic_ramp_dur ' raise from 0 to quic_init
' Turn on quad gradient
Dim quad_turnon_start_time As Double = line_load_end_time
Dim quad_turnon_end_time As Double = quic_turnon_end_time ' raise from 0 to quad_init

'---------------------------------------------------------------------------------------------------------------------------------------------
'----------------------------------------------------- Arb ramp time definitions -------------------------------------------------------------
'(3) Arbitrary ramp
Dim ramp_start_time As Double = quic_turnon_end_time 'ramp delocalizes along y first, so it can start as soon as y tilt is ramped up
Dim ramp_variables = LoadRampSegmentsFromFile(rampSegPath, n_variables)
Dim n_times As Integer = ramp_variables(0).GetUpperBound(0)
Console.WriteLine("N times: {0}", n_times)

' overwrite appropriate variables
If override_default > 0
    If dur_idx <= n_times Then
        ramp_variables(0)(dur_idx) = dur_value
        If var_idx <= n_variables Then
            ramp_variables(var_idx)(dur_idx) = var_value
        Else
            'Console.WriteLine("Bad value for var_idx = {0}, must be <= {1}. Default voltage value will not be overwritten.", var_idx, n_variables)
            Microsoft.VisualBasic.Interaction.MsgBox("Bad value for var_idx. Default voltage value will not be overwritten.")
        End If
    Else
        'Console.WriteLine("Bad value for dur_idx = {0}, must be <= {1}. Default values will not be overwritten.", dur_idx, n_times)
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

Dim lattice1_ramp_v(n_times) As Double
Dim lattice2_ramp_v(n_times) As Double
Dim gauge_freq_ramp_v(n_times) As Double
For index As Integer = 0 To n_times
    lattice1_ramp_v(index) = DepthToVolts(ramp_variables(1)(index), lattice1_calib_depth, lattice1_calib_volt, lattice1_voltage_offset)
    lattice2_ramp_v(index) = DepthToVolts(ramp_variables(3)(index), lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)
    gauge_freq_ramp_v(index) = BeatVolt(ramp_variables(4)(index))
Next

Dim gauge_power_ramp_v As Double() = ramp_variables(2)
Dim quad_ramp_v As Double() = ramp_variables(5)  ' 0th value is "quad_init"
Dim quic_ramp_v As Double() = ramp_variables(6)  ' 0th value is "quic_init"

Dim lattice1_ramp_forward_v As Double = lattice1_ramp_v(n_times)
Dim lattice2_ramp_forward_v As Double = lattice2_ramp_v(n_times)
Dim gauge_power_ramp_forward_v As Double = gauge_power_ramp_v(n_times)
Dim gauge_freq_ramp_forward_v As Double = gauge_freq_ramp_v(n_times)
Dim quad_ramp_forward_v As Double = quad_ramp_v(n_times)
Dim quic_ramp_forward_v As Double = quic_ramp_v(n_times)

If (is_return > 0) Then 'return ramp different? Replace with separate file?
'get times for return ramp
    Dim ramp_t_return(n_times) As Double
    ramp_t_return(0) = ramp_forward_end_time
    For index As Integer=1 To n_times
        ramp_t_return(index) = ramp_t_return(index - 1) + ramp_durs(n_times + 1 - index)
    Next
    Dim ramp_end_time As Double = ramp_t_return(n_times)

    Dim lattice1_ramp_v_return(n_times) As Double
    Dim lattice2_ramp_v_return(n_times) As Double
    Dim gauge_power_ramp_v_return(n_times) As Double
    Dim gauge_freq_ramp_v_return(n_times) As Double    
    Dim quad_ramp_v_return(n_times) As Double
    Dim quic_ramp_v_return(n_times) As Double

    For index As Integer = 0 To n_times
        lattice1_ramp_v_return(index) = DepthToVolts(ramp_variables(1)(n_times - index), lattice1_calib_depth, lattice1_calib_volt, lattice1_voltage_offset)
        lattice2_ramp_v_return(index) = DepthToVolts(ramp_variables(3)(n_times - index), lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)
        gauge_power_ramp_v_return(index) = gauge_power_ramp_v(n_times-index)
        gauge_freq_ramp_v_return(index) = BeatVolt(ramp_variables(4)(n_times - index))
        quad_ramp_v_return(index) = quad_ramp_v(n_times-index)
        quic_ramp_v_return(index) = quic_ramp_v(n_times-index)
    Next
    
    Dim lattice1_ramp_end_v As Double = lattice1_ramp_v_return(n_times)
    Dim lattice2_ramp_end_v As Double = lattice2_ramp_v_return(n_times)
    Dim gauge_power_ramp_end_v As Double = gauge_power_ramp_v_return(n_times)
    Dim gauge_freq_ramp_end_v As Double = gauge_freq_ramp_v_return(n_times)
    Dim quad_ramp_end_v As Double = quad_ramp_v_return(n_times)
    Dim quic_ramp_end_v As Double = quic_ramp_v_return(n_times)

Else
    Dim ramp_end_time As Double = ramp_forward_end_time
    Dim lattice1_ramp_end_v As Double = lattice1_ramp_forward_v
    Dim lattice2_ramp_end_v As Double = lattice2_ramp_forward_v
    Dim gauge_power_ramp_end_v As Double = gauge_power_ramp_forward_v
    Dim gauge_freq_ramp_end_v As Double = gauge_freq_ramp_forward_v
    Dim quad_ramp_end_v As Double = quad_ramp_forward_v
    Dim quic_ramp_end_v As Double = quic_ramp_forward_v

End If


' Final values:
' ramp_end_time
' lattice1_ramp_end_v
' gauge_power_ramp_end_v
' lattice2_ramp_end_v
' gauge_freq_ramp_end_v
' quad_ramp_end_v
' quic_ramp_end_v
'---------------------------------------------------------------------------------------------------------------------------------------------
'----------------------------------------------------- End arb ramp time defs ----------------------------------------------------------------


'----------------------------------------------------- Pinning -------------------------------------------------------------------------------

'(3) pinning
Dim freeze_lattice_ramp_dur As Double = 100
Dim pinning_dur As Double = 100
Dim pinning_start_time As Double = ramp_end_time + freeze_lattice_ramp_dur
Dim pinning_end_time As Double = pinning_start_time + pinning_dur

Dim IT As Double = pinning_end_time

'---------------------------------------------------------------------------------------------------------------------------------------------
'---------------------------------------------------------------------------------------------------------------------------------------------

'digitaldata2.AddPulse(clock_resynch,1,4)
analogdata.DisableClkDist(0.95, 4.05)
analogdata2.DisableClkDist(0.95, 4.05)


'----------------------------------------------------- Arbitrary Ramps -----------------------------------------------------------------------
'---------------------------------------------------------------------------------------------------------------------------------------------
'Linear interpolation to connect the ramp end points

'2D1 lattice power
analogdata.AddRamp(lattice1_max_volt, lattice1_ramp_v(1), ramp_start_time, ramp_t(1), lattice2D765_power)
For index As Integer = 1 To n_times - 1
    analogdata.AddRamp(lattice1_ramp_v(index), lattice1_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), lattice2D765_power)
Next

'gauge power
analogdata.AddRamp(gauge_power_ramp_v(0), gauge_power_ramp_v(1), ramp_start_time, ramp_t(1), gauge1_power)
For index As Integer = 1 To n_times - 1
    analogdata.AddRamp(gauge_power_ramp_v(index), gauge_power_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), gauge1_power)
Next

'2D2 lattice power
analogdata.AddRamp(lattice2_max_volt, lattice2_ramp_v(1), ramp_start_time, ramp_t(1), lattice2D765_power2) 'Check LogRamp subs, since DtoV already takes log, need to do logrithmic interpolation
For index As Integer = 1 To n_times - 1
    analogdata.AddRamp(lattice2_ramp_v(index), lattice2_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), lattice2D765_power2)
Next

'quic grad
analogdata.AddRamp(quic_ramp_v(0), quic_ramp_v(1), ramp_start_time, ramp_t(1), ps5_ao) 'ps5_ao
For index As Integer = 1 To n_times - 1
    analogdata.AddRamp(quic_ramp_v(index), quic_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), ps5_ao) 'ps5_ao
Next

'quad grad
analogdata.AddRamp(quad_ramp_v(0), quad_ramp_v(1), ramp_start_time, ramp_t(1), ps8_ao) 'ps8_ao
For index As Integer = 1 To n_times - 1
    analogdata.AddRamp(quad_ramp_v(index), quad_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), ps8_ao) 'ps8_ao
Next

'gauge detuning
analogdata.AddRamp(0, gauge_freq_ramp_v(1), ramp_start_time, ramp_t(1), gauge2_rf_fm)
For index As Integer = 1 To n_times - 1
    analogdata.AddRamp(gauge_freq_ramp_v(index), gauge_freq_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), gauge2_rf_fm)
Next

If (is_return > 0) Then
    ' ramp backward for return
    '2D1 lattice power
    'analogdata.AddRamp(lattice1_max_volt, lattice1_ramp_v(1), ramp_start_time, ramp_t(1), lattice2D765_power)
    For index As Integer = 0 To n_times - 1
        analogdata.AddRamp(lattice1_ramp_v_return(index), lattice1_ramp_v_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), lattice2D765_power)
    Next

    'gauge power
    'analogdata.AddRamp(0, gauge_power_ramp_v_return(1), ramp_start_time, ramp_t(1), gauge1_power)
    For index As Integer = 0 To n_times - 1
        analogdata.AddRamp(gauge_power_ramp_v_return(index), gauge_power_ramp_v_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), gauge1_power)
    Next

    '2D2 lattice power
    'analogdata.AddRamp(lattice2_max_volt, lattice2_ramp_v(1), ramp_start_time, ramp_t(1), lattice2D765_power2) 'Check LogRamp subs, since DtoV already takes log, need to do logrithmic interpolation
    For index As Integer = 0 To n_times - 1
        analogdata.AddRamp(lattice2_ramp_v_return(index), lattice2_ramp_v_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), lattice2D765_power2)
    Next

    'quic grad
    'analogdata.AddRamp(quic_init, quic_ramp_v(1), ramp_start_time, ramp_t(1), ps5_ao) 'ps5_ao
    For index As Integer = 0 To n_times - 1
        analogdata.AddRamp(quic_ramp_v_return(index), quic_ramp_v_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), ps5_ao) 'ps5_ao
    Next

    'quad grad
    'analogdata.AddRamp(quad_init, quad_ramp_v(1), ramp_start_time, ramp_t(1), ps8_ao) 'ps8_ao
    For index As Integer = 0 To n_times - 1
        analogdata.AddRamp(quad_ramp_v_return(index), quad_ramp_v_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), ps8_ao) 'ps8_ao
    Next

    'gauge detuning
    'analogdata.AddRamp(0, gauge_freq_ramp_v(1), ramp_start_time, ramp_t(1), gauge2_rf_fm)
    For index As Integer = 0 To n_times - 1
        analogdata.AddRamp(gauge_freq_ramp_v_return(index), gauge_freq_ramp_v_return(index + 1), ramp_t_return(index), ramp_t_return(index + 1), gauge2_rf_fm)
    Next
End If

'----------------------------------------------------- End Arbitrary Ramps -------------------------------------------------------------------
'---------------------------------------------------------------------------------------------------------------------------------------------

'----------------------------------------------------- Pinning -------------------------------------------------------------------------------

''Bring everything back up for pinning
'' 2D1 
''analogdata.AddSmoothRamp(lattice1_ramp_v(n_times), lattice1_max_volt, ramp_end_time, pinning_start_time, lattice2D765_power)
'analogdata.AddStep(lattice1_max_volt, pinning_start_time, pinning_end_time, lattice2D765_power)
'' 2D2 raise to final depth and hold
'analogdata.AddSmoothRamp(lattice2_ramp_v(n_times), lattice2_max_volt, ramp_end_time, pinning_start_time, lattice2D765_power2)
'analogdata.AddStep(lattice2_max_volt, pinning_start_time, pinning_end_time, lattice2D765_power2)


'---------------------------------------------------------------------------------------------------------------------------------------------

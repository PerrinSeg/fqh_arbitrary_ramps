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

'Dim rampSegPath As String = "Z:\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\ramp_segments.txt"
'Dim rampSegPath_return As String = "Z:\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\ramp_segments_adiabatic.txt"
'Dim lattice2_JtoDepth_coeff_path As String = "Z:\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\j_to_depth_2d2lattice.txt"
'Dim gauge_JtoVolt_coeff_path As String = "Z:\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\j_to_depth_gaugepower.txt"

Dim rampSegPath As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\ramp_segments_files\\ramp_segments.txt"
Dim rampSegPath_return As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\ramp_segments_files\\ramp_segments_adiabatic.txt"
Dim lattice2_JtoDepth_coeff_path As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\ramp_segments_files\\j_to_depth_2d2lattice.txt"
Dim gauge_JtoDepth_coeff_path As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\ramp_segments_files\\j_to_depth_gaugepower.txt"

Dim n_variables As Integer = 6 ' number of channels used in the arbitrary ramp
Dim half_index As Integer = 5 ' point at which y delocalization finishes and x delocalization starts
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

Dim lowered_lattice1_volt As Double 
lowered_lattice1_volt = DepthToVolts(lowered_lattice1_depth, lattice1_calib_depth, lattice1_calib_volt, lattice1_voltage_offset)
Dim lowered_lattice2_volt As Double 
lowered_lattice2_volt = DepthToVolts(lowered_lattice2_depth, lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)


'----------------------------------------------------- Time Definitions PLACEHOLDER ----------------------------------------------------------

Dim twodphysics_start_time As Double = 0


'----------------------------------------------------- Time Definitions for Sequence ---------------------------------------------------------

'(1) Ramp up everything to the initial values (may need to remove for final sequence, just for testing purposes)
' Turn on quic gradient
Dim quic_turnon_start_time As Double = twodphysics_start_time
Dim quic_turnon_end_time As Double = quic_turnon_start_time + quic_ramp_dur ' raise from 0 to quic_init
' Turn on quad gradient
Dim quad_turnon_start_time As Double = twodphysics_start_time
Dim quad_turnon_end_time As Double = quic_turnon_end_time ' raise from 0 to quad_init


'---------------------------------------------------------------------------------------------------------------------------------------------
'----------------------------------------------------- Arb ramp time definitions -------------------------------------------------------------




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

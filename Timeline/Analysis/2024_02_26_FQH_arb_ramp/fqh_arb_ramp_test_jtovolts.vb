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
Dim rampSegPath As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\ramp_segments_files\\ramp_segments.txt"
Dim rampSegPath_return As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\ramp_segments_files\\ramp_segments_adiabatic.txt"
Dim lattice2_JtoDepth_coeff_path As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\ramp_segments_files\\j_to_depth_2d2lattice.txt"
Dim gauge_JtoDepth_coeff_path As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_02_05_FQH_arb_ramp_analog_Perrin\\ramp_segments_files\\j_to_depth_gaugepower.txt"

Dim n_variables As Integer = 6 ' number of channels used in the arbitrary ramp

Dim gauge_calib_volt As Double = 3.34 'accuracy of depth to volt calibration doesn't matter much as long as consistant values are used here and in matlab file. Accuracy of conversion is still limited by quantum walk calibration.
Dim gauge_calib_depth As Double = 7.4/1.24 ' same for this value...


Dim lattice1_max As Double = 45
Dim lattice2_max As Double = 45
Dim lattice1_max_volt As Double = lattice1_voltage_offset+lattice1_calib_volt+.5*Log10(lattice1_max/lattice1_calib_depth)
Dim lattice2_max_volt As Double = lattice2_voltage_offset+lattice2_calib_volt+.5*Log10(lattice2_max/lattice2_calib_depth)
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

Dim ramp_durs As Double() = ramp_variables(0)
Dim gauge_power_ramp_j(n_times) As Double
Dim lattice2_ramp_j(n_times) As Double
For index As Integer = 0 To n_times
    gauge_power_ramp_j(index) = ramp_variables(2)(index)/1240 'units of E_r    
    lattice2_ramp_j(index) = ramp_variables(3)(index)/1240 'units of E_r    
Next

Dim lattice2_ramp_forward_j As Double = lattice2_ramp_j(n_times)
Dim gauge_power_ramp_forward_j As Double = gauge_power_ramp_j(n_times)

Dim lattice2_ramp_end_j As Double
Dim gauge_power_ramp_end_j As Double

lattice2_ramp_end_j = lattice2_ramp_forward_j
gauge_power_ramp_end_j = gauge_power_ramp_forward_j

Dim gauge_power_ramp_start_v As Double = GaugeJToVolts(gauge_power_ramp_j(0), gauge_JtoDepth_coeffs, nterms_gauge, gauge_calib_depth, gauge_calib_volt)
Dim gauge_power_ramp_end_v As Double = GaugeJToVolts(gauge_power_ramp_end_j, gauge_JtoDepth_coeffs, nterms_gauge, gauge_calib_depth, gauge_calib_volt)

'Dim lattice2_ramp_start_v As Double = JToVolts(lattice2_ramp_j(0), lattice2_JtoDepth_coeffs, nterms_2D2, lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)
Dim lattice2_ramp_end_v As Double = JToVolts(lattice2_ramp_end_j, lattice2_JtoDepth_coeffs, nterms_2D2, lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)

Dim lattice2_raise_start_time As Double = 20
Dim lattice2_raise_end_time As Double = lattice2_raise_start_time + 30
Dim lattice2_freeze_start_time As Double = lattice2_raise_end_time
Dim lattice2_freeze_end_time As Double = lattice2_freeze_start_time + 10
'----------------------------------------------------- Lattices II ----------------------------------------------------------------------------------------

analogdata2.addstep(gauge_power_ramp_end_v,0,lattice2_raise_start_time,gauge1_power)

'quench to deep lattice
analogdata.AddStep(lattice2_ramp_end_v, lattice2_raise_start_time, lattice2_freeze_start_time-1, lattice2D765_power2)
'analogdata.AddSmoothRamp(lattice2_ramp_end_v, lattice2_max_volt, lattice2_freeze_start_time-1, lattice2_freeze_start_time, lattice2D765_power2)

'quench to deep lattice
analogdata.AddStep(gauge_power_ramp_end_v, lattice2_raise_start_time, lattice2_freeze_start_time - 1, gauge1_power)
analogdata.AddSmoothRamp(gauge_power_ramp_end_v, 0, lattice2_freeze_start_time - 1, lattice2_freeze_start_time, gauge1_power)















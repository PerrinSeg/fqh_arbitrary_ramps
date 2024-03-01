
'===================
'=====Variables=====
lattice1_max = 45
lattice2_max = 45
pinning_hold_time = 1000
red_freq = 17.0
round_dimple_voltage = 4.23
anticonfine_dur_vert = 40
anticonfine_dur_horz = 40
quic_ramp_dur = 60
quic_grad_level = 7.35
walls2_volt = 2.7
do_walls2_cal = 1
walls_ramp_dur_cal = 500
lattice2_ramp_dur = 10
lattice2_low_depth = 6
latticemod_amp_1 = 0.2
latticemod_amp_2 = 0.2
latticemod_amp_3 = 0.2
mod_freq_Hz_1 = 608
mod_freq_Hz_2 = 795
mod_freq_Hz_3 = 992
phase_pi_start = 0
phase_pi_stop = 0
phase_ramp_dur = 0
lattice_mod_ramp_dur = 50
quic_ramp_final_dur = 50
quic_grad_final_level = 6.88
hold_time_aux = 0
tof_floquet = 0
delay_shaking = 0
walls_ramp_dur_tof = 0
wall_bs_volt = 0
mod_freq_Hz_tof_1 = 0
mod_freq_Hz_tof_2 = 0
mod_freq_Hz_tof_3 = 0
phi_pi_tof = 0
ramp_return = 0
tof_dur = 0
full_counting = 0
do_berlin_wall = 0
doublon_splitting = 1
free_var = 0
free_var_2 = 0
'=====Variables=====
'===================

'-------------------------------------------------------------------------------------------------------------------------------------------------- Constants

Dim shunt_switch_dur As Double = 10
Dim coil_ramp_dur As Double = 30

'Loading line constants
Dim gravity_offsets_switch_dur As Double = 10
Dim twod_ramp_dur As Double = 5
Dim anticonfine_volt As Double = 2.35
Dim anticonfine_ramp_dur As Double = 1
Dim lattice1_amb_volt As Double = 1.4
Dim lattice2_amb_volt As Double = 1.8

'DMD specific durations
Dim PID_response_dur As Double = 1 'needed to fix spikes in line_DMD
Dim loadline_DMD_volt_vert As Double = 3.3
Dim loadline_DMD_volt_horz As Double = 3.1 '3.1

'Lattice depth for full counting statistics
Dim lattice1_full_counting_depth As Double = 2.5

'Berlin wall for full counting of two columns separately
Dim Berlin_wall_expansion_volt As Double = 3.05

'For doublon splitting
Dim splitting_lattice_dur As Double = 10
Dim splitting_turnon_dur As Double = 10
Dim splitting_ramp_dur As Double = 200
Dim lattice2_doublon_depth As Double = 15
Dim splitting_quic_start As Double = 5.5
Dim splitting_quic_end As Double = 3


'-------------------------------------------------------------------------------------------------------------------------------------------------- MOT Creation

digitaldata2.AddPulse(clock_resynch,1,4)
analogdata.DisableClkDist(0.95, 4.05)
analogdata2.DisableClkDist(0.95, 4.05)

Dim MOT_end_time As Double
MOT_end_time = Me.AddMOTSequence(0, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim transport_start_time As Double = MOT_end_time


'-------------------------------------------------------------------------------------------------------------------------------------------------- Transport 

Dim transport_end_time As Double
transport_end_time = Me.AddTransportSequence(transport_start_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)


'-------------------------------------------------------------------------------------------------------------------------------------------------- Evaporation

Dim evaporation_end_time As Double
evaporation_end_time = Me.AddEvaporationSequence(transport_end_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)


'-------------------------------------------------------------------------------------------------------------------------------------------------- Make Mott Insulator

Dim MI_variables As Double() = Me.AddMottInsulatorSequence(evaporation_end_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim twodphysics_start_time As Double = MI_variables(0)
Dim lattice1_max_volt As Double = MI_variables(1)
Dim lattice2_max_volt As Double = MI_variables(2)
Dim end_dipole_voltage As Double = MI_variables(3) 


'-------------------------------------------------------------------------------------------------------------------------------------------------- Time Definitions for Cutting

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


'-------------------------------------------------------------------------------------------------------------------------------------------------- Time Definitions for Physics

'(1) Turn on gradient
Dim grad_turnon_start_time As Double = line_load_end_time 
Dim grad_turnon_end_time As Double = grad_turnon_start_time + quic_ramp_dur

'(2) Turn on DMD walls 
Dim walls2_turnon_start_time As Double
Dim walls_ramp_dur as Double = 5
Dim walls2_turnon_end_time As Double

'(3) Lower lattice depth 
Dim lower_lattice_start_time As Double
Dim lower_lattice_end_time As Double

'(4) Perform 3-component modulation - Ramp part
Dim mod_start_time As Double
Dim mod_ramp_end_time As Double

If (do_walls2_cal = 0) Then

	'(2) Turn on DMD walls 
	walls2_turnon_start_time = grad_turnon_end_time
	walls2_turnon_end_time = walls2_turnon_start_time + walls_ramp_dur

	'(3) Lower lattice depth 
	lower_lattice_start_time = walls2_turnon_end_time
	lower_lattice_end_time = lower_lattice_start_time + lattice2_ramp_dur

	'(4) Perform 3-component modulation - Ramp part
	mod_start_time = lower_lattice_end_time
	mod_ramp_end_time = lower_lattice_end_time + lattice_mod_ramp_dur

Else
	
	'(2) Lower lattice depth 
	lower_lattice_start_time = grad_turnon_end_time
	lower_lattice_end_time = lower_lattice_start_time + lattice2_ramp_dur

	'(3) Turn on DMD walls 
	walls2_turnon_start_time = lower_lattice_end_time
	walls2_turnon_end_time = walls2_turnon_start_time + walls_ramp_dur_cal

	'(4) Perform 3-component modulation - Ramp part
	mod_start_time = walls2_turnon_end_time
	mod_ramp_end_time = mod_start_time + lattice_mod_ramp_dur

End If


'(5) Ramp gradient to final value while doing the 3-tone modulation
Dim grad_final_start_time As Double = mod_ramp_end_time
Dim grad_final_end_time As Double = grad_final_start_time + quic_ramp_final_dur

'(6) Ramp statistical phase
Dim stat_phase_ramp_start_time As Double = grad_final_end_time
Dim stat_phase_ramp_end_time As Double = stat_phase_ramp_start_time + phase_ramp_dur

'(6a) Hold before expansion
Dim hold_start_time As Double = stat_phase_ramp_end_time
Dim hold_end_time As Double = hold_start_time + hold_time_aux

'(7) TOF Floquet
Dim mod_tof_floquet_start_time As Double = hold_end_time
Dim mod_tof_floquet_end_time As Double = mod_tof_floquet_start_time + tof_floquet

'(8) End 3-component modulation (possibly return part)
Dim mod_end_time As Double
Dim grad_final_return_start_time As Double
Dim grad_final_return_end_time As Double
Dim hold_mod_dur As Double
If (ramp_return = 0) Then
	hold_mod_dur = quic_ramp_final_dur
	mod_end_time = mod_tof_floquet_end_time
Else	
	hold_mod_dur = quic_ramp_final_dur
	grad_final_return_start_time = grad_final_end_time + phase_ramp_dur + phase_ramp_dur
	grad_final_return_end_time = grad_final_return_start_time + quic_ramp_final_dur
	mod_end_time = grad_final_return_end_time + lattice_mod_ramp_dur
End If
Dim total_ramp_dur_aux as Double = stat_phase_ramp_end_time - mod_start_time
Dim total_ramp_dur as Double = mod_tof_floquet_start_time - mod_start_time - delay_shaking


'(9) Do TOF
Dim tof_start_time As Double = mod_end_time
Dim tof_end_time As Double = tof_start_time + tof_dur

'(10) Ramp to deep lattice OR
Dim freeze_lattice_start_time As Double = tof_end_time
Dim hold_lattice_after_tof_dur As Double = 5
Dim hold_lattice_after_tof_start_time As Double
Dim freeze_lattice_end_time As Double
If (tof_dur > 0) Then
	lattice2_ramp_dur = 1
	hold_lattice_after_tof_start_time = freeze_lattice_start_time + lattice2_ramp_dur
	freeze_lattice_end_time = hold_lattice_after_tof_start_time + hold_lattice_after_tof_dur
Else
	freeze_lattice_end_time = freeze_lattice_start_time + lattice2_ramp_dur
End If

'(11) Turn off DMD walls 
Dim walls2_turnoff_start_time As Double
If (tof_dur > 0) Then
	walls2_turnoff_start_time = mod_end_time
	walls_ramp_dur = 0.1
Else
	If (tof_floquet > 0) Then
		walls_ramp_dur = walls_ramp_dur_tof
		walls2_turnoff_start_time = mod_tof_floquet_start_time
	Else
		walls2_turnoff_start_time = freeze_lattice_end_time
	End If
End If
Dim walls2_turnoff_end_time As Double = walls2_turnoff_start_time + walls_ramp_dur

'() Turn on wall to reflect bound state
Dim wall_bs_ramp_dur as Double = 1
Dim wall_bs_turnon_start_time As Double = walls2_turnoff_end_time + 0.5 + 0.16 
Dim wall_bs_turnon_end_time As Double
If (wall_bs_volt > 0) Then
	wall_bs_turnon_end_time = wall_bs_turnon_start_time + wall_bs_ramp_dur
Else
	wall_bs_turnon_end_time = wall_bs_turnon_start_time
End If

'(12) Turn off gradient
Dim grad_turnoff_start_time As Double
If (tof_dur > 0) Then
	quic_ramp_dur = 0.1
	grad_turnoff_start_time = mod_end_time
Else
	If (tof_floquet > 0) Then
		grad_turnoff_start_time = mod_end_time
	Else
		grad_turnoff_start_time = walls2_turnoff_end_time
	End If
End If
Dim grad_turnoff_end_time As Double = grad_turnoff_start_time + quic_ramp_dur

'(13) Last event time
Dim last_event_time As Double
If (tof_dur > 0) Then
	last_event_time = freeze_lattice_end_time
Else
	last_event_time = grad_turnoff_end_time
End If


'( ) Doublon splitting
Dim quic_turnon_splitting_start_time As Double = last_event_time
Dim quic_turnon_splitting_end_time As Double
If (doublon_splitting = 1) Then
	quic_turnon_splitting_end_time = quic_turnon_splitting_start_time + splitting_turnon_dur
Else
	quic_turnon_splitting_end_time = quic_turnon_splitting_start_time
End If

Dim lower_lattice_splitting_start_time As Double = quic_turnon_splitting_end_time
Dim lower_lattice_splitting_end_time As Double
If (doublon_splitting = 1) Then
	lower_lattice_splitting_end_time = lower_lattice_splitting_start_time + splitting_lattice_dur
Else
	lower_lattice_splitting_end_time = lower_lattice_splitting_start_time
End If

Dim quic_ramp_splitting_start_time As Double = lower_lattice_splitting_end_time
Dim quic_ramp_splitting_end_time As Double
If (doublon_splitting = 1) Then
	quic_ramp_splitting_end_time = quic_ramp_splitting_start_time + splitting_ramp_dur
Else
	quic_ramp_splitting_end_time = quic_ramp_splitting_start_time
End If

Dim freeze_lattice_splitting_start_time As Double = quic_ramp_splitting_end_time
Dim freeze_lattice_splitting_end_time As Double
If (doublon_splitting = 1) Then
	freeze_lattice_splitting_end_time = freeze_lattice_splitting_start_time + splitting_lattice_dur
Else
	freeze_lattice_splitting_end_time = freeze_lattice_splitting_start_time
End If

Dim quic_turnoff_splitting_start_time As Double = freeze_lattice_splitting_end_time
Dim quic_turnoff_splitting_end_time As Double
If (doublon_splitting = 1) Then
	quic_turnoff_splitting_end_time = quic_turnoff_splitting_start_time + splitting_turnon_dur
Else
	quic_turnoff_splitting_end_time = quic_turnoff_splitting_start_time
End If


'(14) Full counting statistics
Dim twodphysics_end_time As Double
Dim full_counting_start_time as Double
Dim full_counting_hold_start_time as Double
Dim full_counting_hold_end_time as Double
Dim full_counting_end_ramp_time as Double
Dim full_counting_end_time As Double
Dim berlin_wall_rampup_start_time As Double
Dim berlin_wall_rampup_end_time As Double
Dim berlin_wall_rampdown_start_time As Double
Dim berlin_wall_rampdown_end_time As Double
If (full_counting = 0) Then
	twodphysics_end_time = quic_turnoff_splitting_end_time + free_var
Else
	full_counting_start_time = quic_turnoff_splitting_end_time
	Dim full_counting_rampdown_dur As Double = 2
	full_counting_hold_start_time = full_counting_start_time + full_counting_rampdown_dur
	Dim full_counting_hold_dur As Double = 2.3
	full_counting_hold_end_time = full_counting_hold_start_time + full_counting_hold_dur
	Dim full_counting_rampup_dur As Double = 1
	full_counting_end_ramp_time = full_counting_hold_end_time + full_counting_rampup_dur
	Dim full_counting_hold_after_dur As Double = 10
	full_counting_end_time = full_counting_end_ramp_time + full_counting_hold_after_dur
	twodphysics_end_time = full_counting_end_time + free_var	
	If (do_berlin_wall = 1) Then
		berlin_wall_rampup_start_time = full_counting_hold_start_time - 5
		berlin_wall_rampup_end_time = berlin_wall_rampup_start_time + 5
		berlin_wall_rampdown_start_time = full_counting_end_ramp_time
		berlin_wall_rampdown_end_time = berlin_wall_rampdown_start_time + 5
	End If	
End If

'(15) Pinning, image
Dim pinning_start_time As Double = twodphysics_end_time


'-------------------------------------------------------------------------------------------------------------------------------------------------- Pinning

Dim pinning_times As Double() = Me.AddPinningSequence(pinning_start_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim pinning_end_time As Double = pinning_times(0)
Dim pinning_ready_time As Double = pinning_times(1)
Dim molasses_start_time As Double = pinning_times(2)

Dim IT As Double = pinning_end_time + TOF


'-------------------------------------------------------------------------------------------------------------------------------------------------- Lattice Depth Conversion to Volts

Dim lattice2_low_volt As Double = lattice2_voltage_offset + lattice2_calib_volt + .5*Log10(lattice2_low_depth/lattice2_calib_depth)
Dim lattice2_doublon_volt As Double = lattice2_voltage_offset + lattice2_calib_volt + .5*Log10(lattice2_doublon_depth/lattice2_calib_depth)


'-------------------------------------------------------------------------------------------------------------------------------------------------- Hold Mott Insulator 

'JK: why are some lines commented out?
'digitaldata.AddPulse(offset_fet, twodphysics_start_time, pinning_ready_time) 'Offset FET
digitaldata.AddPulse(bias_enable, twodphysics_start_time, pinning_ready_time) 'Enables bias coils
'digitaldata.AddPulse(transport_13, twodphysics_start_time, pinning_ready_time) 'Keeps T13 on through remainder of experiment
digitaldata.AddPulse(ps5_enable, twodphysics_start_time, pinning_ready_time)
digitaldata2.AddPulse(ioffe_mirror_fet, twodphysics_start_time, pinning_ready_time)
digitaldata.AddPulse(quad_shim, twodphysics_start_time, pinning_ready_time)
digitaldata2.AddPulse(single_quad_shim, twodphysics_start_time, pinning_ready_time)
'digitaldata.AddPulse(ps4_shunt, twodphysics_start_time, pinning_end_time)
'digitaldata.AddPulse(imaging_coil, twodphysics_start_time, pinning_ready_time)


'-------------------------------------------------------------------------------------------------------------------------------------------------- Hold Lattices 

digitaldata2.AddPulse(lattice2D765_ttl, twodphysics_start_time, molasses_start_time)
digitaldata2.AddPulse(lattice2D765_ttl2, twodphysics_start_time, molasses_start_time)
digitaldata2.AddPulse(lattice2D765_shutter, twodphysics_start_time, molasses_start_time)
digitaldata2.AddPulse(lattice2D765_shutter2, twodphysics_start_time, molasses_start_time)
digitaldata.AddPulse(ttl_axial_lattice, twodphysics_start_time, molasses_start_time+1) 'JK: why +1?
digitaldata.AddPulse(axial_lattice_shutter, twodphysics_start_time, molasses_start_time)


'-------------------------------------------------------------------------------------------------------------------------------------------------- Blue Donut

'ramp down Blue Donut for physics 
'off until the end of sequence
digitaldata2.AddPulse(anticonfin_ttl, twodphysics_start_time, line_load_vert_start_time)
digitaldata2.AddPulse(anticonfin_shutter, twodphysics_start_time, line_load_vert_start_time)

analogdata.AddSmoothRamp(end_dipole_voltage, 1.44, twodphysics_start_time, line_load_vert_start_time, red_dipole_power) 


'-------------------------------------------------------------------------------------------------------------------------------------------------- Anticonfine Beam 

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
	analogdata.AddStep(anticonfine_volt, twod2_rampdown_end_time,  twod2_reload_start_time, red_dipole_power)
	analogdata.AddSmoothRamp(anticonfine_volt, 1, twod2_reload_start_time, twod2_reload_end_time, red_dipole_power)
End If


'-------------------------------------------------------------------------------------------------------------------------------------------------- Lattice Drop and Expulsion for DMD

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


'-------------------------------------------------------------------------------------------------------------------------------------------------- Lattices

'Hold axial lattice
analogdata.AddStep(axial_voltage,twodphysics_start_time,pinning_ready_time,axial_lattice_power)
analogdata.AddSmoothRamp(axial_voltage,1.76,pinning_ready_time,molasses_start_time,axial_lattice_power)

analogdata.AddStep(lattice1_max_volt, line_load_end_time, lower_lattice_start_time, lattice2D765_power) '2D1
analogdata.AddStep(lattice2_max_volt, line_load_end_time, lower_lattice_start_time, lattice2D765_power2) '2D2

If (quic_ramp_dur > 0) Then

	'Lower 2D2
    	analogdata.AddSmoothRamp(lattice2_max_volt, lattice2_low_volt, lower_lattice_start_time, lower_lattice_end_time, lattice2D765_power2) 
	analogdata.AddStep(lattice2_low_volt, lower_lattice_end_time, mod_start_time, lattice2D765_power2)

	'3-component modulation
	If (ramp_return = 0) Then
		analogdata.AddRampLogSine3PhaseNoReturn(lattice2_low_depth, latticemod_amp_1, latticemod_amp_2, latticemod_amp_3, mod_freq_Hz_1, mod_freq_Hz_2, mod_freq_Hz_3, phase_pi_start, phase_pi_stop, phase_ramp_dur, mod_start_time, lattice_mod_ramp_dur, hold_mod_dur, lattice2_voltage_offset, lattice2_calib_volt, lattice2_calib_depth, lattice2D765_power2)
		'JK: add hold time
		analogdata.AddLogSine3Phase(lattice2_low_depth, latticemod_amp_1, latticemod_amp_2, latticemod_amp_3, mod_freq_Hz_1, mod_freq_Hz_2, mod_freq_Hz_3, phase_pi_stop, hold_start_time, hold_end_time, total_ramp_dur_aux, lattice2_voltage_offset, lattice2_calib_volt, lattice2_calib_depth, lattice2D765_power2)
		If (tof_floquet > 0) Then
    			'analogdata.AddLogSine3Phase(lattice2_low_depth, latticemod_amp_1, latticemod_amp_2, latticemod_amp_3, mod_freq_Hz_tof_1, mod_freq_Hz_tof_2, mod_freq_Hz_tof_3, phase_pi_stop, mod_tof_floquet_start_time, mod_tof_floquet_end_time, total_ramp_dur, lattice2_voltage_offset, lattice2_calib_volt, lattice2_calib_depth, lattice2D765_power2) 
			analogdata.AddLogSine3PhaseNew(lattice2_low_depth, latticemod_amp_1, latticemod_amp_2, latticemod_amp_3, mod_freq_Hz_tof_1, mod_freq_Hz_tof_2, mod_freq_Hz_tof_3, phase_pi_stop, phi_pi_tof, mod_tof_floquet_start_time, mod_tof_floquet_end_time, total_ramp_dur, lattice2_voltage_offset, lattice2_calib_volt, lattice2_calib_depth, lattice2D765_power2) 		 
		End If
	Else
		analogdata.AddRampLogSine3PhaseReturn(lattice2_low_depth, latticemod_amp_1, latticemod_amp_2, latticemod_amp_3, mod_freq_Hz_1, mod_freq_Hz_2, mod_freq_Hz_3, phase_pi_start, phase_pi_stop, phase_ramp_dur, mod_start_time, lattice_mod_ramp_dur, hold_mod_dur, lattice2_voltage_offset, lattice2_calib_volt, lattice2_calib_depth, lattice2D765_power2)
	End If

	If (tof_dur > 0) Then
		analogdata.AddSmoothRamp(lattice2_low_volt, lattice2_amb_volt, mod_end_time, tof_start_time, lattice2D765_power2)
		analogdata.AddStep(lattice2_amb_volt, tof_start_time, tof_end_time, lattice2D765_power2)
		analogdata.AddSmoothRamp(lattice2_amb_volt, lattice2_max_volt, tof_end_time, hold_lattice_after_tof_start_time, lattice2D765_power2)
		analogdata.AddStep(lattice2_max_volt, hold_lattice_after_tof_start_time, freeze_lattice_end_time, lattice2D765_power2)
	Else
		analogdata.AddSmoothRamp(lattice2_low_volt, lattice2_max_volt, freeze_lattice_start_time, freeze_lattice_end_time, lattice2D765_power2)
	End If
Else
	'Lower 2D2 for qw in bare lattice
	analogdata.AddSmoothRamp(lattice2_max_volt, lattice2_low_volt, lower_lattice_start_time, lower_lattice_end_time, lattice2D765_power2)	
	analogdata.AddStep(lattice2_low_volt, lower_lattice_end_time, freeze_lattice_start_time, lattice2D765_power2)
	analogdata.AddSmoothRamp(lattice2_low_volt, lattice2_max_volt, freeze_lattice_start_time, freeze_lattice_end_time, lattice2D765_power2)
End If

'Doublon detection
If (doublon_splitting = 1) Then
        analogdata.AddStep(lattice2_max_volt, freeze_lattice_end_time, lower_lattice_splitting_start_time, lattice2D765_power2) 
	analogdata.AddSmoothRamp(lattice2_max_volt, lattice2_doublon_volt, lower_lattice_splitting_start_time, lower_lattice_splitting_end_time, lattice2D765_power2) 
        analogdata.AddStep(lattice2_doublon_volt, quic_ramp_splitting_start_time, quic_ramp_splitting_end_time, lattice2D765_power2) 
        analogdata.AddSmoothRamp(lattice2_doublon_volt, lattice2_max_volt, freeze_lattice_splitting_start_time, freeze_lattice_splitting_end_time, lattice2D765_power2)
        analogdata.AddStep(lattice2_max_volt, quic_turnoff_splitting_start_time, pinning_ready_time, lattice2D765_power2) 
Else
	analogdata.AddStep(lattice2_max_volt, freeze_lattice_end_time, pinning_ready_time, lattice2D765_power2)
End If

'Ramp down 2D1 for full counting statistics
If (full_counting = 0) Then
	analogdata.AddStep(lattice1_max_volt, lower_lattice_start_time, pinning_ready_time, lattice2D765_power)
Else
	analogdata.AddStep(lattice1_max_volt, lower_lattice_start_time, full_counting_start_time, lattice2D765_power)
	analogdata.AddSmoothRamp(lattice1_max_volt, lattice1_full_counting_depth, full_counting_start_time, full_counting_hold_start_time, lattice2D765_power)
	analogdata.AddStep(lattice1_full_counting_depth, full_counting_hold_start_time, full_counting_hold_end_time, lattice2D765_power)
	analogdata.AddSmoothRamp(lattice1_full_counting_depth, lattice1_max_volt, full_counting_hold_end_time, full_counting_end_ramp_time, lattice2D765_power)
	analogdata.AddStep(lattice1_max_volt, full_counting_end_ramp_time, pinning_ready_time, lattice2D765_power)
End If




'-------------------------------------------------------------------------------------------------------------------------------------------------- Magnetic Fields

'Turn on quad mosfets
digitaldata2.AddPulse(quad_shim2, grad_turnon_start_time - shunt_switch_dur, grad_turnoff_end_time + shunt_switch_dur)
digitaldata2.AddPulse(ps8_shunt, twodphysics_start_time, grad_turnon_start_time - 0.5*shunt_switch_dur)
digitaldata2.AddPulse(ps8_shunt, grad_turnoff_end_time + 0.5*shunt_switch_dur, pinning_ready_time + 10)

'Keep constant at values from MI sequence
analogdata.AddStep(quad_gravity_offset, twodphysics_start_time, pinning_ready_time, ps1_ao)
analogdata.AddStep(ps6_scaler*quic_gravity_offset + ps6_offset, twodphysics_start_time, pinning_ready_time, ps6_ao)

If (quic_ramp_dur > 0) Then 
	analogdata2.AddStep(0, twodphysics_start_time, grad_turnon_start_time, ps5_ao)
	analogdata2.AddSmoothRamp(0, quic_grad_level*ps5_scaler, grad_turnon_start_time, grad_turnon_end_time, ps5_ao)
   	analogdata2.AddStep(quic_grad_level*ps5_scaler, grad_turnon_end_time, grad_final_start_time, ps5_ao)
	analogdata2.AddSmoothRamp(quic_grad_level*ps5_scaler, quic_grad_final_level*ps5_scaler, grad_final_start_time, grad_final_end_time, ps5_ao)
	If (ramp_return = 0) Then
		analogdata2.AddStep(quic_grad_final_level*ps5_scaler, grad_final_end_time, grad_turnoff_start_time, ps5_ao)
    		analogdata2.AddSmoothRamp(quic_grad_final_level*ps5_scaler, 0, grad_turnoff_start_time, grad_turnoff_end_time, ps5_ao)
	Else
		analogdata2.AddStep(quic_grad_final_level*ps5_scaler, grad_final_end_time, grad_final_return_start_time, ps5_ao)
    		analogdata2.AddSmoothRamp(quic_grad_final_level*ps5_scaler, quic_grad_level*ps5_scaler, grad_final_return_start_time, grad_final_return_end_time, ps5_ao)
		analogdata2.AddStep(quic_grad_level*ps5_scaler, grad_final_return_end_time, grad_turnoff_start_time, ps5_ao)
    		analogdata2.AddSmoothRamp(quic_grad_level*ps5_scaler, 0, grad_turnoff_start_time, grad_turnoff_end_time, ps5_ao)
	End If
	analogdata2.AddStep(0, grad_turnoff_end_time, pinning_ready_time, ps5_ao)
End If

If (doublon_splitting = 1) Then
	analogdata2.AddSmoothRamp(0, splitting_quic_start*ps5_scaler, quic_turnon_splitting_start_time, quic_turnon_splitting_end_time, ps5_ao)
	analogdata2.AddStep(splitting_quic_start*ps5_scaler, quic_turnon_splitting_end_time, quic_ramp_splitting_start_time, ps5_ao)
	analogdata2.AddSmoothRamp(splitting_quic_start*ps5_scaler, splitting_quic_end*ps5_scaler, quic_ramp_splitting_start_time, quic_ramp_splitting_end_time, ps5_ao)
	analogdata2.AddStep(splitting_quic_end*ps5_scaler, quic_ramp_splitting_end_time, quic_turnoff_splitting_start_time, ps5_ao)
	analogdata2.AddSmoothRamp(splitting_quic_end*ps5_scaler, 0, quic_turnoff_splitting_start_time, quic_turnoff_splitting_end_time, ps5_ao)
End If



'-------------------------------------------------------------------------------------------------------------------------------------------------- DMD Code

'DMD triggers
Dim DMD_hw_delay As Double = -0.16
'Matthew hack to put this in earlier

'Alex hack
'this line to bring trigger active high, but also serve as the tracking line trigger (on its falling edge, with 6ms delay)
digitaldata2.AddPulse(line_DMD_trigger, evaporation_end_time, molasses_start_time + fluo_image_wait ) 'tracking line
digitaldata2.AddPulse(hor_DMD_trigger, evaporation_end_time, molasses_start_time + fluo_image_wait )'tracking hor

'Now all these are inverted and triggers on the second time stamp (real rising edge)
'pattern switching after a 160us delay, switching itself takes few us or less
digitaldata2.AddPulse(line_DMD_trigger, DMD_hw_delay + line_load_vert_start_time - 0.5, DMD_hw_delay + line_load_vert_start_time) 'cut first direction (line DMD)
digitaldata2.AddPulse(hor_DMD_trigger, DMD_hw_delay + line_load_horz_start_time - 0.5, DMD_hw_delay + line_load_horz_start_time) 'cut second direction (hor DMD)
digitaldata2.AddPulse(hor_DMD_trigger, DMD_hw_delay + walls2_start_time - 0.5, DMD_hw_delay + walls2_start_time) 'Walls (hor DMD)
digitaldata2.AddPulse(hor_DMD_trigger, DMD_hw_delay + wall_bs_turnon_start_time - 0.5, DMD_hw_delay + wall_bs_turnon_start_time) 'Wall to reflect bound state (hor DMD)
'digitaldata2.AddPulse(line_DMD_trigger, DMD_hw_delay + wall_bs_turnon_end_time, DMD_hw_delay + wall_bs_turnon_end_time + 0.5) 'Berlin wall (line DMD)
digitaldata2.AddPulse(line_DMD_trigger, DMD_hw_delay + walls2_turnoff_end_time, DMD_hw_delay + walls2_turnoff_end_time + 0.5) 'Berlin wall (line DMD)


'Shutter
digitaldata2.AddPulse(line_DMD_shutter, cutting1_turnon_start_time - 20, molasses_start_time)
digitaldata2.AddPulse(line_DMD_shutter, cutting2_turnon_start_time - 20, molasses_start_time)

'Cut one direction with line DMD
If (anticonfine_dur_vert > 0) Then
	digitaldata2.AddPulse(line_DMD_ttl, cutting1_turnon_start_time - PID_response_dur/2, line_load_mid_time + PID_response_dur/2) 
	analogdata2.AddSmoothRamp(line_DMD_start_volt, loadline_DMD_volt_vert, cutting1_turnon_start_time, cutting1_turnon_end_time, line_DMD_power)
	analogdata2.AddStep(loadline_DMD_volt_vert, cutting1_turnon_end_time, cutting1_turnoff_start_time, line_DMD_power)
	analogdata2.AddSmoothRamp(loadline_DMD_volt_vert, line_DMD_start_volt, cutting1_turnoff_start_time, cutting1_turnoff_end_time, line_DMD_power)
End If

'Cut second direction with hor DMD
If (anticonfine_dur_horz > 0) Then
	digitaldata2.AddPulse(hor_DMD_ttl, cutting2_turnon_start_time - PID_response_dur/2, line_load_end_time + PID_response_dur/2)
	analogdata2.AddSmoothRamp(line_DMD_start_volt, loadline_DMD_volt_horz, cutting2_turnon_start_time, cutting2_turnon_end_time, hor_DMD_power)
	analogdata2.AddStep(loadline_DMD_volt_horz, cutting2_turnon_end_time, cutting2_turnoff_start_time, hor_DMD_power)
	analogdata2.AddSmoothRamp(loadline_DMD_volt_horz, line_DMD_start_volt, cutting2_turnoff_start_time, cutting2_turnoff_end_time, hor_DMD_power)
End If

'Walls2 for hor DMD
If (walls2_volt > 0) Then
	digitaldata2.AddPulse(hor_DMD_ttl, walls2_turnon_start_time - PID_response_dur/2, walls2_turnoff_end_time + PID_response_dur/2)
	If (do_walls2_cal = 0) Then
		analogdata2.AddSmoothRamp(line_DMD_start_volt, walls2_volt, walls2_turnon_start_time, walls2_turnon_end_time, hor_DMD_power)
	Else
		analogdata2.AddLogRamp(line_DMD_start_volt, walls2_volt, walls2_turnon_start_time, walls2_turnon_end_time, hor_DMD_power)
	End If

	analogdata2.AddStep(walls2_volt, walls2_turnon_end_time, walls2_turnoff_start_time, hor_DMD_power)
	'turn off
	analogdata2.AddSmoothRamp(walls2_volt, line_DMD_start_volt, walls2_turnoff_start_time, walls2_turnoff_end_time, hor_DMD_power)
        analogdata2.AddStep(line_DMD_start_volt, walls2_turnoff_end_time, wall_bs_turnon_start_time, hor_DMD_power)
End If

'Wall to reflect bound state
If (wall_bs_volt > 0) Then
	digitaldata2.AddPulse(hor_DMD_ttl, wall_bs_turnon_start_time - PID_response_dur/2, freeze_lattice_end_time + PID_response_dur/2)
	analogdata2.AddSmoothRamp(line_DMD_start_volt, wall_bs_volt, wall_bs_turnon_start_time, wall_bs_turnon_end_time, hor_DMD_power)
	analogdata2.AddStep(wall_bs_volt, wall_bs_turnon_end_time, freeze_lattice_end_time, hor_DMD_power)
'turn off
	analogdata2.AddSmoothRamp(wall_bs_volt, line_DMD_start_volt, freeze_lattice_end_time, freeze_lattice_end_time + wall_bs_ramp_dur, hor_DMD_power)
End If
	
If (do_berlin_wall = 1) Then
	digitaldata2.AddPulse(line_DMD_ttl, berlin_wall_rampup_start_time - PID_response_dur/2, berlin_wall_rampdown_end_time + PID_response_dur/2)
	analogdata2.AddSmoothRamp(line_DMD_start_volt, Berlin_wall_expansion_volt, berlin_wall_rampup_start_time, berlin_wall_rampup_end_time, line_DMD_power)
	analogdata2.AddStep(Berlin_wall_expansion_volt, berlin_wall_rampup_end_time, berlin_wall_rampdown_start_time, line_DMD_power)
	analogdata2.AddSmoothRamp(Berlin_wall_expansion_volt, line_DMD_start_volt, berlin_wall_rampdown_start_time, berlin_wall_rampdown_end_time, line_DMD_power)
End If

'-------------------------------------------------------------------------------------------------------------------------------------------------- Tracking

Dim isDMDTracking As Double = 1
Dim last_time As Double = IT + 1000
If (isDMDTracking = 1) Then 
	last_time = Me.AddTrackingSequence2(pinning_end_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Else 
	last_time = Me.AddTrackingSequence(pinning_end_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, False)
End If


'-------------------------------------------------------------------------------------------------------------------------------------------------- Scope trigger
Dim scope_trigger As Double = mod_start_time
digitaldata.AddPulse(64, scope_trigger, scope_trigger + 10)


























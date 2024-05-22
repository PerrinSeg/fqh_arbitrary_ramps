'optimize adiabatic parameters to FQH state
'v4:
' corrected flux calibration 
' added ps8_scaler to accommodate different power supply
' v4p3: updated timing of walls to turn off AFTER lattices are raised
' v4p3p1: updated timing of lattice quench at point of imaging; in v4p3, lattice2 is raised 1 ms after lattice1
' v4p6: Shortened PS ramp down to end before pinning ready (fix spike at end of sequence)
'v5:
' turns off quic_gravity_offset after cutting starts
' gets rid of "free_var" use in code (now a true dummy variable) 
'v6:
' updated for 5x5N3 system
' v6p2: updated to allow doublon splitting

'===================
'=====Variables=====
lattice1_max  = 45
lattice2_max  = 45
pinning_hold_time = 1000
red_freq = 18
round_dimple_voltage = 4.02
anticonfine_dur_vert = 0
anticonfine_dur_horz = 0
quic_ramp_dur = 0
quic_grad_init = 0
quic_grad_final = 0
quic_mirror_grad = 0
quad_turnonoff_dur = 0
quad_ramp_dur = 0
quad_grad_init = 0
quad_grad_final = 0
lattice2_ramp_dur = 0
lattice2_low_depth = 45
lattice1_ramp_dur = 0
lattice1_low_depth = 45
walls2_volt = 2.4
walls1_volt = 3.1
use_quad_grav_offset = 1
use_quic_grav_offset = 0
use_gauge = 0
gauge_power = 2.88
gauge_turnonoff_dur = 0
gauge_freq_khz = 0.69
flux = 0
doublon_quad_grad = 0
doublon_quad_ramp_dur = 0
doublon_lattice_ramp_dur = 0
doublon_lattice_depth = 0
evolution_dur = 0
is_return = 0
is_cleanup = 0
is_counting = 0
free_var = 0
'=====Variables=====
'===================

'----------------------------------------------------------- Constants --------------------------------------------------------------------------------------- 

Dim freqVoltRampPath As String = "Z:\\experiments\\FQH\\rampfiles\\freqRamp_4x4N2_tiltX10K_Vy2Er.txt"
Dim quicRampPath As String = "Z:\\FQH\\rampfiles\\2024_05_02_ramp_files_5x5\\quic_ramp_5x5.txt"
Dim quadRampPath As String = "Z:\\FQH\\rampfiles\\2024_05_15_5x5N3_updated_ramps\\quad_ramp_5x5.txt" '5.75J tilt

Dim shunt_switch_dur As Double = 10
Dim coil_ramp_dur As Double = 30

'loading line constants
Dim gravity_offsets_switch_dur As Double = 30
Dim twod_ramp_dur As Double = 5
Dim anticonfine_volt As Double = 2.4 '2.4
Dim anticonfine_ramp_dur As Double = 1
Dim lattice1_amb_volt As Double = 1.4
Dim lattice2_amb_volt As Double = 1.8

'DMD specific durations
Dim PID_response_dur As Double = 1 'needed to fix spikes in line_DMD
Dim loadline_DMD_volt_vert As Double = 3.3
Dim loadline_DMD_volt_horz As Double = 3.2

'gauge field constants
Dim gauge1_pzX As Double = 4.52
Dim gauge1_pzY As Double = 7.34
Dim gauge2_pzX As Double = 4.37
Dim gauge2_pzY As Double = 8.65


'Sequence constants
Dim freeze_ramp_dur As Double = 1
Dim cleanup_dur As Double = 40
Dim kick_dur As Double = 2
Dim expansion_dur As Double = 2
Dim Berlin_wall_kick_volt As Double = 2.8 '2.8
Dim lattice1_kick_volt As Double = 3.2 '3.2

'*** For new PS8 (ES015-10)
Dim ps8_scaler As Double = 1 '6


'----------------------------------------------------------- MOT Creation ---------------------------------------------------------------------------------------
digitaldata2.AddPulse(clock_resynch,1,4)
analogdata.DisableClkDist(0.95, 4.05)
analogdata2.DisableClkDist(0.95, 4.05)

Dim MOT_end_time As Double
MOT_end_time = Me.AddMOTSequenceUpgrade(0, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim transport_start_time As Double = MOT_end_time


'----------------------------------------------------------- Transport ------------------------------------------------------------------------------------------

Dim transport_end_time As Double
transport_end_time = Me.AddTransportSequence(transport_start_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)


'----------------------------------------------------------- Evaporation ----------------------------------------------------------------------------------------

Dim evaporation_end_time As Double
evaporation_end_time = Me.AddEvaporationSequence(transport_end_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)


'----------------------------------------------------------- Make Mott Insulator --------------------------------------------------------------------------------

Dim MI_variables As Double() = Me.AddMottInsulatorSequence(evaporation_end_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim twodphysics_start_time As Double = MI_variables(0)
Dim lattice1_max_volt As Double = MI_variables(1)
Dim lattice2_max_volt As Double = MI_variables(2)
Dim end_dipole_voltage As Double = MI_variables(3) 


'----------------------------------------------------------- Time Definitions for Cutting -----------------------------------------------------------------------

''Ramp off gravity offsets
'' ramp gravity_offset to zero for expel 
'Dim go_lower_start_time As Double = twodphysics_start_time
'Dim go_lower_end_time As Double = go_lower_start_time + gravity_offsets_switch_dur



'Cut vertical line
Dim line_load_vert_start_time = twodphysics_start_time
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

'Cut horizontal line
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
Dim line_load_end_time As Double = cutting2_turnoff_end_time
'skip if zero
If (anticonfine_dur_horz = 0) Then
    line_load_end_time = line_load_mid_time + 20 + PID_response_dur
End If

'Ramp gravity offsets back on (if needed)
Dim go_lower_start_time As Double = line_load_end_time
Dim go_lower_end_time As Double
If (use_quad_grav_offset < 1)
    go_lower_end_time = go_lower_start_time + gravity_offsets_switch_dur
Else If (use_quic_grav_offset < 1)
    go_lower_end_time = go_lower_start_time + gravity_offsets_switch_dur
Else
    go_lower_end_time = go_lower_start_time + 10
End If
 

'----------------------------------------------------------- Time Definitions for Physics ---------------------------------------------------------------------

'(1) Turn on walls2
Dim walls2_turnon_start_time As Double = go_lower_end_time + 1 '+1 to avoid overlap of digital signals on same channel
Dim walls2_turnon_end_time As Double
If (walls2_volt > 0) Then
    walls2_turnon_end_time = walls2_turnon_start_time + 5
Else
    walls2_turnon_end_time = walls2_turnon_start_time
End If

'() Turn on walls1
'turns on at the same time as walls2
Dim walls1_turnon_start_time As Double = go_lower_end_time + 1 '+1 to avoid overlap of digital signals on same channel
Dim walls1_turnon_end_time As Double
If (walls1_volt > 0) Then
    walls1_turnon_end_time = walls1_turnon_start_time + 5
Else
    walls1_turnon_end_time = walls1_turnon_start_time
End If

'() Turn on quic gradient
Dim quic_turnon_start_time As Double = walls1_turnon_end_time
Dim quic_turnon_end_time As Double 
If (quic_grad_init > 0) Then
    quic_turnon_end_time = quic_turnon_start_time + coil_ramp_dur
Else 
    quic_turnon_end_time = quic_turnon_start_time
End If

'() Lower 2D2 lattice depth
Dim lattice2_lower_start_time As Double = quic_turnon_end_time
Dim lattice2_lower_end_time As Double = quic_turnon_end_time + lattice2_ramp_dur

'() Lower quic gradient
Dim quic_lower_start_time As Double = lattice2_lower_end_time
Dim quic_lower_end_time As Double = quic_lower_start_time + quic_ramp_dur

'() Turn on quad gradient
Dim quad_turnon_start_time As Double = quic_lower_end_time
Dim quad_turnon_end_time As Double 
If (quad_grad_init > 0) Then
    quad_turnon_end_time = quad_turnon_start_time + quad_turnonoff_dur
Else
    quad_turnon_end_time = quad_turnon_start_time
End If

'() cherp freq to enable dynamics
Dim freq_cherp_start_time As Double = quad_turnon_end_time
Dim freq_cherp_end_time As Double 
If (use_gauge > 0) Then
	freq_cherp_end_time = freq_cherp_start_time + 1
Else
	freq_cherp_end_time = freq_cherp_start_time
End If

'() Turn on Raman beams
Dim raman_rampup_start_time As Double = freq_cherp_end_time
Dim raman_rampup_end_time As Double 
If (use_gauge > 0) Then 
    raman_rampup_end_time = raman_rampup_start_time + gauge_turnonoff_dur
Else
    raman_rampup_end_time = raman_rampup_start_time
End If

'() Lower 2D1 lattice depth 
Dim lattice1_lower_start_time As Double = raman_rampup_end_time
Dim lattice1_lower_end_time As Double = lattice1_lower_start_time + lattice1_ramp_dur

'() Lower quad gradient
Dim quad_lower_start_time As Double = lattice1_lower_end_time
Dim quad_lower_end_time As Double = quad_lower_start_time + quad_ramp_dur

'() Hold time
Dim hold_start_time As Double = quad_lower_end_time
Dim hold_end_time As Double = hold_start_time + evolution_dur

'() Doublon splitting
'turn off Raman beams
'quench lattice2 to 45Er
'ramp/quench lattice1 to 15Er
'ramp quad gradient past U resonance
'quench lattice1 to 45Er

Dim doublon_lattice_raise_start_time As Double = hold_end_time
Dim doublon_lattice_raise_end_time As Double
If (doublon_quad_ramp_dur > 0) Then
    doublon_lattice_raise_end_time =doublon_lattice_raise_start_time + doublon_lattice_ramp_dur
Else
    doublon_lattice_raise_end_time = doublon_lattice_raise_start_time
End If

Dim doublon_quad_lower_start_time As Double = doublon_lattice_raise_end_time
Dim doublon_quad_lower_end_time As Double
If (doublon_quad_ramp_dur > 0) Then
    doublon_quad_lower_end_time = doublon_quad_lower_start_time + doublon_quad_ramp_dur
Else
    doublon_quad_lower_end_time = doublon_quad_lower_start_time
End If

'() RETURN: raise quad gradient
Dim quad_raise_start_time As Double = doublon_quad_lower_end_time
Dim quad_raise_end_time As Double
If (is_return > 0) Then 
    quad_raise_end_time = quad_raise_start_time + quad_ramp_dur
Else
    quad_raise_end_time = quad_raise_start_time
End If

'() RETURN: raise 2D1 lattice depth
Dim lattice1_raise_start_time As Double = quad_raise_end_time
Dim lattice1_raise_end_time As Double 
If (is_return > 0) Then
    lattice1_raise_end_time = lattice1_raise_start_time + lattice1_ramp_dur
Else
    lattice1_raise_end_time = lattice1_raise_start_time
End If

'() RETURN: turn off Raman beams
Dim raman_rampdown_start_time As Double = lattice1_raise_end_time
Dim raman_rampdown_end_time As Double 
If (is_return > 0) Then 
    raman_rampdown_end_time = raman_rampdown_start_time + use_gauge * gauge_turnonoff_dur
Else
    raman_rampdown_end_time = raman_rampdown_start_time 
End If

'() RETURN: turn off quad gradient
Dim quad_turnoff_start_time As Double = raman_rampdown_end_time
Dim quad_turnoff_end_time As Double 
If (is_return > 0) Then 
    quad_turnoff_end_time = quad_turnoff_start_time + quad_turnonoff_dur
Else 
    quad_turnoff_end_time = quad_turnoff_start_time
End If

'() RETURN: raise quic gradient
Dim quic_raise_start_time As Double = quad_turnoff_end_time
Dim quic_raise_end_time As Double
If (is_return > 0) Then 
    quic_raise_end_time = quic_raise_start_time + quic_ramp_dur
Else
    quic_raise_end_time = quic_raise_start_time
End If

'() RETURN: raise 2D2 lattice depth
Dim lattice2_raise_start_time As Double = quic_raise_end_time
Dim lattice2_raise_end_time As Double 
If (is_return > 0) Then 
    lattice2_raise_end_time = lattice2_raise_start_time + lattice2_ramp_dur
Else
    lattice2_raise_end_time = lattice2_raise_start_time
End If

'() RETURN: turn off quic gradient
Dim quic_turnoff_start_time As Double = lattice2_raise_end_time
Dim quic_turnoff_end_time As Double 
If (is_return > 0) Then 
    quic_turnoff_end_time = quic_turnoff_start_time + coil_ramp_dur
Else 
    quic_turnoff_end_time = quic_turnoff_start_time
End If

'() ramp to deep lattice before turning off the walls
'TO DO: Add quench to lattice_max_volt at end of physics ramp, make sure everything afterwards is triggered to happen after the ramp is complete.

'() turn off walls 
'same variable for both walls
Dim walls_turnoff_start_time As Double = quic_turnoff_end_time 
Dim walls_turnoff_end_time As Double = walls_turnoff_start_time + 5

'() remove atoms outside the walls (cleanup)
Dim cutting1_turnon2_start_time As Double = walls_turnoff_end_time
Dim cutting1_turnon2_end_time As Double = cutting1_turnon2_start_time + (is_cleanup * 5)
Dim twod1_rampdown2_start_time As Double = cutting1_turnon2_end_time
Dim twod1_rampdown2_end_time As Double = twod1_rampdown2_start_time + (is_cleanup * twod_ramp_dur)
Dim twod1_reload2_start_time As Double = twod1_rampdown2_end_time + (is_cleanup * cleanup_dur)
Dim twod1_reload2_end_time As Double = twod1_reload2_start_time + (is_cleanup * twod_ramp_dur)
Dim cutting1_turnoff2_start_time As Double = twod1_reload2_end_time 
Dim cutting1_turnoff2_end_time As Double = cutting1_turnoff2_start_time + (is_cleanup * 5)

'() kick and capture for counting (Berlin wall)
Dim Berlin_wall_turnon_start_time As Double = cutting1_turnoff2_end_time + 5 'fixed 5ms pause such that DMD trigger for wall pattern is always separated from DMD trigger for cutting (cleanup) pattern
Dim Berlin_wall_turnon_end_time As Double = Berlin_wall_turnon_start_time + (is_counting * 5)
Dim twod1_rampdown3_start_time As Double = Berlin_wall_turnon_end_time
Dim twod1_rampdown3_end_time As Double = twod1_rampdown3_start_time + (is_counting * 1)
Dim twod1_reload3_start_time As Double = twod1_rampdown3_end_time + (is_counting * kick_dur)
Dim twod1_reload3_end_time As Double = twod1_reload3_start_time + (is_counting * 1)
Dim Berlin_wall_turnoff_start_time As Double = twod1_reload3_end_time
Dim Berlin_wall_turnoff_end_time As Double = Berlin_wall_turnoff_start_time + (is_counting * 5)

'freeze lattices, turn off gradients
Dim grad_turnoff_start_time As Double = quic_turnoff_end_time 'turn off coils when is_return = 0
Dim lattice2_freeze_start_time As Double = lattice2_raise_end_time
Dim lattice2_freeze_end_time As Double = lattice2_freeze_start_time + freeze_ramp_dur
Dim lattice1_freeze_start_time As Double = twod1_reload3_end_time
Dim lattice1_freeze_end_time As Double = lattice1_freeze_start_time + freeze_ramp_dur

'() Pinning, image
Dim twodphysics_end_time As Double = twod1_reload3_end_time
Dim pinning_start_time As Double = twodphysics_end_time


'--------------------------------------------------------------------- Pinning ------------------------------------------------------------------------------------

Dim pinning_times As Double() = Me.AddPinningSequence(pinning_start_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim pinning_end_time As Double = pinning_times(0)
Dim pinning_ready_time As Double = pinning_times(1)
Dim molasses_start_time As Double = pinning_times(2)

Dim IT As Double = pinning_end_time+TOF

Dim grad_turnoff_end_time As Double = pinning_ready_time 'gradient needs to be turned off before pinning sequence activates shunts (causes spikes if supply is shunted while still supplying current)

'--------------------------------------------------------------------- Tracking -----------------------------------------------------------------------------------

Dim last_time As Double = IT
Dim delay_tracking As Double = 2000 '1000
Dim tracking_end_time As Double = Me.AddTrackingSequenceUpgrade(delay_tracking, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)


'--------------------------------------------------------------------- Invert signals -----------------------------------------------------------------------------

digitaldata.AddPulse(mot_low_current, transport_start_time, last_time) 'MOT Current Supply
digitaldata.AddPulse(ta_shutter, transport_start_time, last_time) 'TA Shutter. starts to close early, so the shutter is closed ASAP after the blow away pulse.
digitaldata.AddPulse(repump_shutter, transport_start_time, last_time) 'Repump Shutter
digitaldata2.AddPulse(ixon_flip_mount_ttl, tracking_end_time, last_time)


'--------------------------------------------------------------------- Lattice Depth Conversion to Volts ----------------------------------------------------------

Dim lattice1_low_volt As Double = DepthToVolts(lattice1_low_depth, lattice1_calib_depth, lattice1_calib_volt, lattice1_voltage_offset)
Dim lattice2_low_volt As Double = DepthToVolts(lattice2_low_depth, lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)
Dim doublon_lattice_volt As Double = DepthToVolts(doublon_lattice_depth, lattice1_calib_depth, lattice1_calib_volt, lattice1_voltage_offset)


'--------------------------------------------------------------------- B field MOSFETS ------------------------------------------------------------------------

digitaldata.AddPulse(ps5_enable, twodphysics_start_time, grad_turnoff_end_time + 0.5 * shunt_switch_dur)
digitaldata2.AddPulse(quad_shim2, quad_turnon_start_time - shunt_switch_dur, grad_turnoff_end_time + shunt_switch_dur)
digitaldata2.AddPulse(ps8_shunt, twodphysics_start_time, quad_turnon_start_time - 2*shunt_switch_dur)

'for quad gravity offset
digitaldata.AddPulse(quad_shim, twodphysics_start_time, pinning_ready_time)
digitaldata2.AddPulse(single_quad_shim, twodphysics_start_time, pinning_ready_time)
'for quic gravity offset / ps6
digitaldata2.AddPulse(ioffe_mirror_fet, twodphysics_start_time, pinning_ready_time)
digitaldata.AddPulse(bias_enable, twodphysics_start_time, pinning_ready_time) 'NOT RELATED TO BIAS COILS



'--------------------------------------------------------------------- Hold Lattices ----------------------------------------------------------------------------

digitaldata2.AddPulse(lattice2D765_ttl, twodphysics_start_time, molasses_start_time)
digitaldata2.AddPulse(lattice2D765_ttl2, twodphysics_start_time, molasses_start_time)
digitaldata2.AddPulse(lattice2D765_shutter, twodphysics_start_time, molasses_start_time)
digitaldata2.AddPulse(lattice2D765_shutter2, twodphysics_start_time, molasses_start_time)
digitaldata.AddPulse(ttl_axial_lattice, twodphysics_start_time, molasses_start_time+1) 'JK: why +1?
digitaldata.AddPulse(axial_lattice_shutter, twodphysics_start_time, molasses_start_time)


'--------------------------------------------------------------------- Blue Donut --------------------------------------------------------------------------------

'ramp down Blue Donut for physics 
'off until the end of sequence
digitaldata2.AddPulse(anticonfin_ttl, twodphysics_start_time, line_load_vert_start_time)
digitaldata2.AddPulse(anticonfin_shutter, twodphysics_start_time, line_load_vert_start_time)

'turn off confinement for line loading
analogdata.AddSmoothRamp(end_dipole_voltage, 1.44, twodphysics_start_time, line_load_vert_start_time, red_dipole_power) 


'--------------------------------------------------------------------- Anticonfine Beam --------------------------------------------------------------------------

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
digitaldata.AddPulse(blue_dipole_shutter, cutting1_turnon2_start_time - 20, twod1_reload2_end_time)
If (is_cleanup > 0) Then 
    digitaldata2.AddPulse(blue_dipole_ttl, cutting1_turnon2_start_time, cutting1_turnoff2_end_time)
    analogdata.AddSmoothRamp(1, anticonfine_volt, twod1_rampdown2_start_time, twod1_rampdown2_end_time, red_dipole_power)
    analogdata.AddStep(anticonfine_volt, twod1_rampdown2_end_time,  twod1_reload2_start_time, red_dipole_power)
    analogdata.AddSmoothRamp(anticonfine_volt, 1, twod1_reload2_start_time, twod1_reload2_end_time, red_dipole_power)
End If


'--------------------------------------------------------------------- Lattice Drop and Expulsion for DMD --------------------------------------------------------

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


'--------------------------------------------------------------------- Lattices -------------------------------------------------------------------------------------

'hold axial lattice
analogdata.AddStep(axial_voltage,twodphysics_start_time,pinning_ready_time,axial_lattice_power)
analogdata.AddSmoothRamp(axial_voltage,1.76,pinning_ready_time,molasses_start_time,axial_lattice_power)
analogdata.AddStep(lattice2_max_volt, line_load_end_time, lattice2_lower_start_time, lattice2D765_power2) '2D2
analogdata.AddStep(lattice1_max_volt, line_load_end_time, lattice1_lower_start_time, lattice2D765_power) '2D1

If (lattice2_ramp_dur > 0) Then
    'lower 2D2
    analogdata.AddSmoothRamp(lattice2_max_volt, lattice2_low_volt, lattice2_lower_start_time, lattice2_lower_end_time, lattice2D765_power2)
    'hold 2D2
    analogdata.AddStep(lattice2_low_volt, lattice2_lower_end_time, lattice2_raise_start_time, lattice2D765_power2)
    If (is_return > 0) Then
        'raise 2D2
        analogdata.AddSmoothRamp(lattice2_low_volt, lattice2_max_volt, lattice2_raise_start_time, lattice2_raise_end_time, lattice2D765_power2)
        'hold 2D2 in deep lattice
        analogdata.AddStep(lattice2_max_volt, lattice2_raise_end_time, lattice2_freeze_start_time, lattice2D765_power2)
    Else
        If (doublon_quad_ramp_dur > 0) Then
            'ramp to deep lattice (splitting along lattice1)
            analogdata.AddSmoothRamp(lattice2_low_volt, lattice2_max_volt, doublon_lattice_raise_start_time, doublon_lattice_raise_end_time, lattice2D765_power2)
            'hold in deep lattice until imaging
            analogdata.AddStep(lattice2_max_volt, doublon_lattice_raise_end_time, lattice2_freeze_start_time, lattice2D765_power2)
        Else
            'quench to deep lattice     
            analogdata.AddSmoothRamp(lattice2_low_volt, lattice2_max_volt, lattice2_raise_start_time, lattice2_raise_end_time, lattice2D765_power2)
            analogdata.AddStep(lattice2_max_volt, lattice2_raise_end_time, lattice2_freeze_start_time, lattice2D765_power2)
        End If
    End If
Else
    analogdata.AddStep(lattice2_max_volt, lattice2_lower_start_time, lattice2_freeze_start_time, lattice2D765_power2) 
End If

If (lattice1_ramp_dur > 0) Then
    'lower 2D1
    analogdata.AddSmoothRamp(lattice1_max_volt, lattice1_low_volt, lattice1_lower_start_time, lattice1_lower_end_time, lattice2D765_power)
    'hold 2D1
    analogdata.AddStep(lattice1_low_volt, lattice1_lower_end_time, lattice1_raise_start_time, lattice2D765_power)
    If (is_return > 0) Then
        'raise 2D1
        analogdata.AddSmoothRamp(lattice1_low_volt, lattice1_max_volt, lattice1_raise_start_time, lattice1_raise_end_time, lattice2D765_power)
        'hold 2D1 in deep lattice
        analogdata.AddStep(lattice1_max_volt, lattice1_raise_end_time, twod1_rampdown2_start_time, lattice2D765_power)
    Else
        If (doublon_quad_ramp_dur > 0) Then
            'doublon splitting
            analogdata.AddSmoothRamp(lattice1_low_volt, doublon_lattice_volt, doublon_lattice_raise_start_time, doublon_lattice_raise_end_time, lattice2D765_power)
            analogdata.AddStep(doublon_lattice_volt, doublon_quad_lower_start_time, doublon_quad_lower_end_time, lattice2D765_power)
            analogdata.AddStep(doublon_lattice_volt, doublon_quad_lower_end_time, lattice1_raise_start_time, lattice2D765_power)
            'quench to deep lattice
            analogdata.AddSmoothRamp(doublon_lattice_volt, lattice1_max_volt, lattice1_raise_start_time, lattice1_raise_end_time, lattice2D765_power)
            analogdata.AddStep(lattice1_max_volt, lattice1_raise_end_time, twod1_rampdown2_start_time, lattice2D765_power)
        Else
            'quench to deep lattice
            analogdata.AddSmoothRamp(lattice1_low_volt, lattice1_max_volt, lattice1_raise_start_time, lattice1_raise_end_time, lattice2D765_power)
            analogdata.AddStep(lattice1_max_volt, lattice1_raise_end_time, twod1_rampdown2_start_time, lattice2D765_power)
        End If
    End If
Else
    analogdata.AddStep(lattice1_max_volt, lattice1_lower_start_time, twod1_rampdown2_start_time, lattice2D765_power) 
End If

If (is_cleanup > 0) Then
    '2D1
    analogdata.AddSmoothRamp(lattice1_max_volt, lattice1_amb_volt, twod1_rampdown2_start_time, twod1_rampdown2_end_time, lattice2D765_power)
    analogdata.AddStep(lattice1_amb_volt, twod1_rampdown2_end_time, twod1_reload2_start_time, lattice2D765_power) 
    analogdata.AddSmoothRamp(lattice1_amb_volt, lattice1_max_volt, twod1_reload2_start_time, twod1_reload2_end_time, lattice2D765_power)
    analogdata.AddStep(lattice1_max_volt, twod1_reload2_end_time, twod1_rampdown3_start_time, lattice2D765_power)
Else
    analogdata.AddStep(lattice1_max_volt, twod1_rampdown2_start_time, twod1_rampdown3_start_time, lattice2D765_power)
End If

'quench lattice1 for full counting
If (is_counting > 0) Then
    analogdata.AddSmoothRamp(lattice1_max_volt, lattice1_kick_volt, twod1_rampdown3_start_time, twod1_rampdown3_end_time, lattice2D765_power)
    analogdata.AddStep(lattice1_kick_volt, twod1_rampdown3_end_time, twod1_reload3_start_time, lattice2D765_power) 
    analogdata.AddSmoothRamp(lattice1_kick_volt, lattice1_max_volt, twod1_reload3_start_time, twod1_reload3_end_time, lattice2D765_power)
    analogdata.AddStep(lattice1_max_volt, twod1_reload3_end_time, lattice1_freeze_start_time, lattice2D765_power)
Else
    analogdata.AddStep(lattice1_max_volt, twod1_rampdown3_start_time, lattice1_freeze_start_time, lattice2D765_power)
End If

'freeze lattices
analogdata.AddSmoothRamp(lattice2_max_volt, lattice2_deepest_volt, lattice2_freeze_start_time, lattice2_freeze_end_time, lattice2D765_power2)
analogdata.AddStep(lattice2_deepest_volt, lattice2_freeze_end_time, pinning_ready_time, lattice2D765_power2)
analogdata.AddSmoothRamp(lattice1_max_volt, lattice1_deepest_volt, lattice1_freeze_start_time, lattice1_freeze_end_time, lattice2D765_power)
analogdata.AddStep(lattice1_deepest_volt, lattice1_freeze_end_time, pinning_ready_time, lattice2D765_power)


'--------------------------------------------------------------------- Magnetic Fields ----------------------------------------------------------------------------------------------------------

'hold grav offsets through cutting
analogdata.AddStep(quad_gravity_offset, twodphysics_start_time, go_lower_start_time, ps1_ao)
analogdata.AddStep(ps6_scaler * quic_gravity_offset + ps6_offset, twodphysics_start_time, go_lower_start_time, ps6_ao)

'ramp down offsets after cutting, or hold to end of sequence
' quad offset
If (use_quad_grav_offset > 0) Then
    analogdata.AddStep(quad_gravity_offset, go_lower_start_time, pinning_ready_time, ps1_ao)
Else
    analogdata.AddSmoothRamp(quad_gravity_offset, 0, go_lower_start_time, go_lower_end_time, ps1_ao)
End If

' quic offset
If (use_quic_grav_offset > 0) Then
    analogdata.AddStep(ps6_scaler * quic_gravity_offset + ps6_offset, go_lower_start_time, quad_turnon_start_time, ps6_ao)
Else
    analogdata.AddSmoothRamp(ps6_scaler * quic_gravity_offset + ps6_offset, 0, go_lower_start_time, go_lower_end_time, ps6_ao)
End If

'PS 5
If (quic_grad_init > 0) Then 
    'turn on quic
    analogdata2.AddSmoothRamp(0, quic_grad_init * ps5_scaler, quic_turnon_start_time, quic_turnon_end_time, ps5_ao)
    analogdata2.AddStep(quic_grad_init * ps5_scaler, quic_turnon_end_time, quic_lower_start_time, ps5_ao)
    'lower quic
    analogdata2.AddInterpolatedRampUsingFile(quicRampPath, quic_grad_init * ps5_scaler, quic_grad_final * ps5_scaler, quic_lower_start_time, quic_lower_end_time, ps5_ao)
    'analogdata2.AddSmoothRamp(quic_grad_init * ps5_scaler, quic_grad_final * ps5_scaler, quic_lower_start_time, quic_lower_end_time, ps5_ao)
    analogdata2.AddStep(quic_grad_final * ps5_scaler, quic_lower_end_time, quic_raise_start_time, ps5_ao)
    If (is_return > 0) Then
        'raise quic
        analogdata2.AddInterpolatedRampUsingFile(quicRampPath, quic_grad_final * ps5_scaler, quic_grad_init * ps5_scaler, quic_raise_start_time, quic_raise_end_time, ps5_ao)
        'analogdata2.AddSmoothRamp(quic_grad_final * ps5_scaler, quic_grad_init * ps5_scaler, quic_raise_start_time, quic_raise_end_time, ps5_ao)
        analogdata2.AddStep(quic_grad_init * ps5_scaler, quic_raise_end_time, quic_turnoff_start_time, ps5_ao)
        'turn off
        analogdata2.AddSmoothRamp(quic_grad_init * ps5_scaler, 0, quic_turnoff_start_time, quic_turnoff_end_time, ps5_ao)
    Else
        analogdata2.AddStep(quic_grad_final * ps5_scaler, quic_raise_start_time, grad_turnoff_start_time, ps5_ao)
        'turn off 
        analogdata2.AddSmoothRamp(quic_grad_final * ps5_scaler, 0, grad_turnoff_start_time, grad_turnoff_end_time, ps5_ao)
    End If
    'keep off 
    analogdata2.AddStep(0, grad_turnoff_end_time, pinning_ready_time, ps5_ao)
End If 

'PS 8
If (quad_grad_init > 0) Then
    'turn on quad
    analogdata2.AddSmoothRamp(0, quad_grad_init * ps8_scaler, quad_turnon_start_time, quad_turnon_end_time, ps8_ao)
    analogdata2.AddStep(quad_grad_init * ps8_scaler, quad_turnon_end_time, quad_lower_start_time, ps8_ao)
    'lower quad
    analogdata2.AddInterpolatedRampUsingFile(quadRampPath, quad_grad_init * ps8_scaler, quad_grad_final * ps8_scaler, quad_lower_start_time, quad_lower_end_time, ps8_ao)
    'analogdata2.AddSmoothRamp(quad_grad_init * ps8_scaler, quad_grad_final * ps8_scaler, quad_lower_start_time, quad_lower_end_time, ps8_ao)
    analogdata2.AddStep(quad_grad_final * ps8_scaler, quad_lower_end_time, hold_end_time, ps8_ao)
    If (is_return > 0) Then
        analogdata2.AddStep(quad_grad_final * ps8_scaler, hold_end_time, quad_raise_start_time, ps8_ao)
        'raise quad
        analogdata2.AddInterpolatedRampUsingFile(quadRampPath, quad_grad_final * ps8_scaler, quad_grad_init * ps8_scaler, quad_raise_start_time, quad_raise_end_time, ps8_ao)
        'analogdata2.AddSmoothRamp(quad_grad_final * ps8_scaler, quad_grad_init * ps8_scaler, quad_raise_start_time, quad_raise_end_time, ps8_ao)
        analogdata2.AddStep(quad_grad_init * ps8_scaler, quad_raise_end_time, quad_turnoff_start_time, ps8_ao)
        'turn off
        analogdata2.AddSmoothRamp(quad_grad_init * ps8_scaler, 0, quad_turnoff_start_time, quad_turnoff_end_time, ps8_ao)
    Else
        If (doublon_quad_ramp_dur > 0) Then
            analogdata2.AddStep(quad_grad_final * ps8_scaler, hold_end_time, doublon_quad_lower_start_time, ps8_ao)
            analogdata2.AddSmoothRamp(quad_grad_final * ps8_scaler, doublon_quad_grad * ps8_scaler, doublon_quad_lower_start_time, doublon_quad_lower_end_time, ps8_ao)
            analogdata2.AddStep(doublon_quad_grad * ps8_scaler, doublon_quad_lower_end_time, grad_turnoff_start_time, ps8_ao)
            'turn off
            analogdata2.AddSmoothRamp(doublon_quad_grad * ps8_scaler, 0, grad_turnoff_start_time, grad_turnoff_end_time, ps8_ao)
        Else
            analogdata2.AddStep(quad_grad_final * ps8_scaler, hold_end_time, grad_turnoff_start_time, ps8_ao)
            'turn off 
            analogdata2.AddSmoothRamp(quad_grad_final * ps8_scaler, 0, grad_turnoff_start_time, grad_turnoff_end_time, ps8_ao)
        End If
    End If
    'keep off 
    analogdata2.AddStep(0, grad_turnoff_end_time, pinning_ready_time, ps8_ao)
End If

'PS 6
Dim quic_mirror_init As Double = 0
If (use_quic_grav_offset > 0) Then
    quic_mirror_init = ps6_scaler * quic_gravity_offset + ps6_offset
End If

If (quic_mirror_grad > 0) Then
    'turn on
    analogdata.AddSmoothRamp(quic_mirror_init, ps6_scaler * quic_mirror_grad + ps6_offset, quad_turnon_start_time, quad_turnon_end_time, ps6_ao)
    'hold
    analogdata.AddStep(ps6_scaler * quic_mirror_grad + ps6_offset, quad_turnon_end_time, quad_raise_start_time, ps6_ao)
    If (is_return > 0) Then
        analogdata.AddStep(ps6_scaler * quic_mirror_grad + ps6_offset, quad_raise_start_time, quad_turnoff_start_time, ps6_ao)
        analogdata.AddSmoothRamp(ps6_scaler * quic_mirror_grad + ps6_offset, quic_mirror_init, quad_turnoff_start_time, quad_turnoff_end_time, ps6_ao)
    Else
        analogdata.AddStep(ps6_scaler * quic_mirror_grad + ps6_offset, quad_raise_start_time, grad_turnoff_start_time, ps6_ao)
        analogdata.AddSmoothRamp(ps6_scaler * quic_mirror_grad + ps6_offset, quic_mirror_init, grad_turnoff_start_time, grad_turnoff_end_time, ps6_ao)
    End If
    'turn off
    analogdata.AddStep(quic_mirror_init, grad_turnoff_end_time, pinning_ready_time, ps6_ao)
Else
    analogdata.AddStep(quic_mirror_init, quad_turnon_start_time, grad_turnoff_start_time, ps6_ao)
    analogdata.AddSmoothRamp(quic_mirror_init, 0, grad_turnoff_start_time, grad_turnoff_end_time, ps6_ao)
End If


'--------------------------------------------------------------------- DMD Code ---------------------------------------------------------------------------------------------------------------

'DMD triggers
Dim DMD_hw_delay As Double = -0.16
'Matthew hack to put this in earlier

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


'--------------------------------------------------------------------- Gauge Field ----------------------------------------------------------------------------------------------------------

Dim gauge_freq_volt As Double = BeatVolt(gauge_freq_khz)

If (use_gauge > 0) Then
	'ramp piezos to the proper location
	Dim piezo_ramp_dur As Double = 2000
	Dim piezo_rampup_time As Double = raman_rampup_start_time - piezo_ramp_dur - 1000
	Dim piezo_ready_time As Double = piezo_rampup_time + piezo_ramp_dur
	Dim piezo_rampdown_time As Double = pinning_start_time
	Dim piezo_end_time As Double = piezo_rampdown_time + piezo_ramp_dur

	analogdata2.AddRamp(0, gauge1_pzX, piezo_rampup_time, piezo_ready_time, gauge1_PZTx)
	analogdata2.AddStep(gauge1_pzX, piezo_ready_time, piezo_rampdown_time, gauge1_PZTx)
	analogdata2.AddRamp(gauge1_pzX, 0, piezo_rampdown_time, piezo_end_time, gauge1_PZTx)

	analogdata2.AddRamp(0, gauge1_flux_calib*flux/0.25 + gauge1_pzY, piezo_rampup_time, piezo_ready_time, gauge1_PZTy) 
	analogdata2.AddStep(gauge1_flux_calib*flux/0.25 + gauge1_pzY, piezo_ready_time, piezo_rampdown_time, gauge1_PZTy)
	analogdata2.AddRamp(gauge1_flux_calib*flux/0.25 + gauge1_pzY, 0, piezo_rampdown_time, piezo_end_time, gauge1_PZTy)

	analogdata2.AddRamp(0, gauge2_pzX, piezo_rampup_time, piezo_ready_time, gauge2_PZTx)
	analogdata2.AddStep(gauge2_pzX, piezo_ready_time, piezo_rampdown_time, gauge2_PZTx)
	analogdata2.AddRamp(gauge2_pzX, 0, piezo_rampdown_time, piezo_end_time, gauge2_PZTx)

	analogdata2.AddRamp(0, gauge2_flux_calib*flux/0.25 + gauge2_pzY, piezo_rampup_time, piezo_ready_time, gauge2_PZTy)
	analogdata2.AddStep(gauge2_flux_calib*flux/0.25 + gauge2_pzY, piezo_ready_time, piezo_rampdown_time, gauge2_PZTy)
	analogdata2.AddRamp(gauge2_flux_calib*flux/0.25 + gauge2_pzY, 0, piezo_rampdown_time, piezo_end_time, gauge2_PZTy)

    digitaldata2.AddPulse(gauge_shutter, raman_rampup_start_time - 20, raman_rampdown_end_time)
    digitaldata2.AddPulse(gauge_ttl, raman_rampup_start_time, raman_rampdown_end_time)

	'Ramp Raman power
	analogdata2.AddSmoothRamp(0.6, gauge_power, raman_rampup_start_time, raman_rampup_end_time, gauge1_power)
    analogdata2.AddStep(gauge_power, raman_rampup_end_time, hold_end_time, gauge1_power)
    If (doublon_quad_ramp_dur > 0) Then
        'just turn off
        analogdata2.AddStep(0.6, doublon_lattice_raise_start_time, raman_rampdown_end_time, gauge1_power)
    Else
        analogdata2.AddStep(gauge_power, hold_end_time, raman_rampdown_start_time, gauge1_power)
        analogdata2.AddSmoothRamp(gauge_power, 0.6, raman_rampdown_start_time, raman_rampdown_end_time, gauge1_power)
    End If


	'Raman-assisted tunneling
	analogdata2.AddStep(gauge_freq_volt, freq_cherp_end_time, pinning_end_time, gauge2_rf_fm)
End If


'--------------------------------------------------------------------- Scope trigger ----------------------------------------------------------------------------------------------------------

Dim scope_trigger As Double = walls2_turnon_start_time
digitaldata.AddPulse(64, scope_trigger, scope_trigger + 10)


























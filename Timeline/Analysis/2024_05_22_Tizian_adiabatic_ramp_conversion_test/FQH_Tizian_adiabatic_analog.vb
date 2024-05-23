'===================
'=====Variables=====
quic_init = 0
quic_final = 0
quad_init = 0.0769
quad_final = 0
lattice2_init = 3.04
lattice2_final = 3.0589
lattice1_init = 3.7059
lattice1_final = 3.3515
ramp_gauge_power = 0
ramp_dur = 500
'=====Variables=====

'----------------------------------------------------- Arbitrary ramp constants --------------------------------------------------------------
'---------------------------------------------------------------------------------------------------------------------------------------------

Dim quicTiltRampPath As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_05_22_Tizian_adiabatic_ramp_conversion_test\\convert_ramp_files\\DVy_ramp.txt"
Dim quadTiltRampPath As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_05_22_Tizian_adiabatic_ramp_conversion_test\\convert_ramp_files\\DVx_ramp.txt"

Dim quadTunnelRampPath As String
If ramp_gauge_power > 0
    quadTunnelRampPath = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_05_22_Tizian_adiabatic_ramp_conversion_test\\convert_ramp_files\\GaugeVx_ramp.txt"
Else
    quadTunnelRampPath = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_05_22_Tizian_adiabatic_ramp_conversion_test\\convert_ramp_files\\KVx_ramp.txt"
End If

Dim quicTunnelRampPath As String = "C:\\Users\\Rb Lab\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_05_22_Tizian_adiabatic_ramp_conversion_test\\convert_ramp_files\\JVy_ramp.txt"
'Dim quicTiltRampPath As String = "C:\\Users\\Perrin\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_05_22_Tizian_adiabatic_ramp_conversion_test\\convert_ramp_files\\DVy_ramp.txt"
'Dim quadTiltRampPath As String = "C:\\Users\\Perrin\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_05_22_Tizian_adiabatic_ramp_conversion_test\\convert_ramp_files\\DVx_ramp.txt"
'Dim quadTunnelRampPath As String = "C:\\Users\\Perrin\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_05_22_Tizian_adiabatic_ramp_conversion_test\\convert_ramp_files\\KVx_ramp.txt"
'Dim quicTunnelRampPath As String = "C:\\Users\\Perrin\\Documents\\GitHub\\fqh_arbitrary_ramps\\Timeline\\Analysis\\2024_05_22_Tizian_adiabatic_ramp_conversion_test\\convert_ramp_files\\JVy_ramp.txt"

Dim lattice1_max As Double = 45
Dim lattice2_max As Double = 45
Dim lattice1_max_volt As Double = lattice1_voltage_offset+lattice1_calib_volt+.5*Log10(lattice1_max/lattice1_calib_depth)
Dim lattice2_max_volt As Double = lattice2_voltage_offset+lattice2_calib_volt+.5*Log10(lattice2_max/lattice2_calib_depth)
Dim quic_ramp_dur As Double = 30

Dim lattice1_init_v As Double = lattice1_init 'DepthToVolts(lattice1_init, lattice1_calib_depth, lattice1_calib_volt, lattice1_voltage_offset)
Dim lattice1_final_v As Double = lattice1_final
Dim lattice2_init_v As Double =  lattice2_init 'DepthToVolts(lattice2_init, lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)
Dim lattice2_final_v As Double = lattice2_final 'DepthToVolts(lattice2_final, lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)


'----------------------------------------------------- Time Definitions for Sequence ---------------------------------------------------------

Dim twodphysics_start_time As Double = 0

'(1) Ramp up everything to the initial values (may need to remove for final sequence, just for testing purposes)
' Turn on quic gradient
Dim quic_turnon_start_time As Double = twodphysics_start_time
Dim quic_turnon_end_time As Double = quic_turnon_start_time + quic_ramp_dur ' raise from 0 to quic_init
' Turn on quad gradient
Dim quad_turnon_start_time As Double = twodphysics_start_time
Dim quad_turnon_end_time As Double = quic_turnon_end_time ' raise from 0 to quad_init


'----------------------------------------------------- Arb ramp time definitions -------------------------------------------------------------

Dim ramp_start_time As Double = quic_turnon_end_time
Dim ramp_end_time As Double = ramp_start_time + ramp_dur


'----------------------------------------------------- Pinning -------------------------------------------------------------------------------

'(3) pinning
Dim pinning_dur As Double = 14000
Dim pinning_start_time As Double = ramp_end_time
Dim pinning_end_time As Double = pinning_start_time + pinning_dur

Dim IT As Double = pinning_end_time


'---------------------------------------------------------------------------------------------------------------------------------------------
'---------------------------------------------------------------------------------------------------------------------------------------------

analogdata.DisableClkDist(0.95, 4.05)


'----------------------------------------------------- (1) Turn on gradients -----------------------------------------------------------------

' quic
analogdata.AddSmoothRamp(twodphysics_start_time, quic_init, quic_turnon_start_time, quic_turnon_end_time, ps5_ao) 'ps5_ao
' quad
analogdata.AddRamp(twodphysics_start_time, quad_init, quad_turnon_start_time, quad_turnon_end_time, ps3_ao) 'ps8_ao


'----------------------------------------------------- () Hold on lattices -----------------------------------------------------------------

' quad
analogdata.AddStep(lattice1_max_volt, twodphysics_start_time, ramp_start_time, lattice2D765_power)
' quic
analogdata.AddStep(lattice2_max_volt, twodphysics_start_time, ramp_start_time, lattice2D765_power2)


'----------------------------------------------------- () Ramps -------------------------------------------------------------------

'Linear interpolation to connect the ramp end points

'2D1
If ramp_gauge_power > 0
    analogdata.AddInterpolatedRampUsingFile(quadTunnelRampPath, lattice1_init_v, lattice1_final_v, ramp_start_time, ramp_end_time, ps1_ao) 'gauge1_power
Else
    analogdata.AddInterpolatedRampUsingFile(quadTunnelRampPath, lattice1_init_v, lattice1_final_v, ramp_start_time, ramp_end_time, ps1_ao) 'lattice2D765_power
End If

'2D2
analogdata.AddInterpolatedRampUsingFile(quicTunnelRampPath, lattice2_init_v, lattice2_final_v, ramp_start_time, ramp_end_time, ps3_ao) 'lattice2D765_power2

'quic
analogdata.AddInterpolatedRampUsingFile(quicTiltRampPath, quic_init, quic_final, ramp_start_time, ramp_end_time, ps5_ao) 'ps5_ao

'quad
analogdata.AddInterpolatedRampUsingFile(quadTiltRampPath, quad_init, quad_final, ramp_start_time, ramp_end_time, ps8_ao) 'ps8_ao

Dim ramp2_start_time As Double = ramp_end_time + 100
Dim ramp2_end_time As Double = ramp2_start_time + ramp_dur
analogdata.AddSmoothRamp(quad_final, quad_init, ramp_end_time, ramp2_start_time, ps8_ao) 'ps8_ao
analogdata.AddSmoothRamp(lattice1_final_v, lattice1_init_v, ramp_end_time, ramp2_start_time, ps1_ao) 'lattice2D765_power

'2D1
analogdata.AddInterpolatedRampUsingFile(quadTunnelRampPath, lattice1_init_v, lattice1_final_v, ramp2_start_time, ramp2_end_time, ps1_ao) 'lattice2D765_power

'2D2
analogdata.AddInterpolatedRampUsingFile(quicTunnelRampPath, lattice2_init_v, lattice2_final_v, ramp2_start_time, ramp2_end_time, ps3_ao) 'lattice2D765_power2

'quic
analogdata.AddInterpolatedRampUsingFile(quicTiltRampPath, quic_init, quic_final, ramp2_start_time, ramp2_end_time, ps5_ao) 'ps5_ao

'quad
analogdata.AddInterpolatedRampUsingFile(quadTiltRampPath, quad_init, quad_final, ramp2_start_time, ramp2_end_time, ps8_ao) 'ps8_ao

''----------------------------------------------------- Pinning -------------------------------------------------------------------------------

'Bring everything back up for pinning
' 2D1 
analogdata.AddSmoothRamp(lattice1_final_v, lattice1_max_volt, ramp2_end_time, pinning_end_time, lattice2D765_power)
' 2D2 raise to final depth and hold
analogdata.AddSmoothRamp(lattice2_final_v, lattice2_max_volt, ramp2_end_time, pinning_end_time, lattice2D765_power2)

analogdata.AddSmoothRamp(quad_final, quad_init, ramp2_end_time, pinning_end_time-2000, ps3_ao) 'ps8_ao
analogdata.AddSmoothRamp(quad_init, 0, pinning_end_time-2000, pinning_end_time, ps3_ao) 'ps8_ao

'---------------------------------------------------------------------------------------------------------------------------------------------

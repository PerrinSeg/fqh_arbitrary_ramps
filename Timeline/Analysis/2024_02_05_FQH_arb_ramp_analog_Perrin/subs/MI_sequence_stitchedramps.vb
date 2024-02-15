Public Function AddStitchedMottInsulatorSequence(ByVal t_start As Double, _
    ByRef cp As clsControlParams, _
    ByRef analogdata As SpectronClient, _
    ByRef analogdata2 As SpectronClient, _
    ByRef digitaldata As digitalDAQdata, _
    ByRef digitaldata2 As digitalDAQdata, _
    ByRef gpib As GPIBControl, _
    ByRef Hermes As KeithleyControl, _
    ByRef dds As AD9959Ev, _
    ByVal MI_en As Boolean) As Double ()


'Sequence from evaporation_end_time to twodmax_ready_time
'returns end time and final voltages for lattices and dipole
'*********************************************************************************************************************************************************

'************************************************************ Calculate time for 2D ramp ************************************************************

Dim lattice1_midpoint_volt As Double = lattice1_voltage_offset + lattice1_calib_volt+.5*Log10(twod_ramp_midpoint/lattice1_calib_depth)
Dim lattice2_midpoint_volt As Double = lattice2_voltage_offset + lattice2_calib_volt+.5*Log10(twod_ramp_midpoint/lattice2_calib_depth)

Dim lattice1_endpoint_volt As Double = lattice1_voltage_offset+lattice1_calib_volt+.5*Log10(twod_ramp_endpoint/lattice1_calib_depth)
Dim lattice2_endpoint_volt As Double = lattice2_voltage_offset+lattice2_calib_volt+.5*Log10(twod_ramp_endpoint/lattice2_calib_depth)

Dim lattice1_max_volt As Double = lattice1_voltage_offset+lattice1_calib_volt+.5*Log10(lattice1_max/lattice1_calib_depth)
Dim lattice2_max_volt As Double = lattice2_voltage_offset+lattice2_calib_volt+.5*Log10(lattice2_max/lattice2_calib_depth)

Dim lattice1_start_volt As Double = lattice1_voltage_offset+lattice1_calib_volt+.5*Log10(lattice1_start_depth/lattice1_calib_depth)
Dim lattice2_start_volt As Double = lattice2_voltage_offset+lattice2_calib_volt+.5*Log10(lattice2_start_depth/lattice2_calib_depth)

Dim twod_chopped_rampup As Double
If lattice1_max > lattice2_max Then
twod_chopped_rampup = (lattice1_max_volt - lattice1_midpoint_volt)/(lattice1_endpoint_volt - lattice1_midpoint_volt)*twodmax_rampup
Else
twod_chopped_rampup = (lattice2_max_volt - lattice2_midpoint_volt)/(lattice2_endpoint_volt - lattice2_midpoint_volt)*twodmax_rampup
End If
' Time constant is 54ms in base e, 125ms in base 10
Dim dipole_voltage As Double
Dim end_dipole_voltage As Double
Dim bfield_dur As Double = 20
Dim dimple_max_volt As double = 4.3
'************************************************************ Definition of Time Variables ***************************************************************
Dim cigar_move_end_time As Double = t_start + cigar_move_time
Dim vertical_move_end_time As Double = cigar_move_end_time + vertical_move
Dim dimple_start_time As Double = vertical_move_end_time+1.5*biglattice_ramp_on+spherical_creation_time
Dim dimple_ready_time As Double = dimple_start_time + dimple_ramp_on
Dim bfield_off_time As Double = dimple_ready_time + bfield_off	
Dim axial_start_time As Double = bfield_off_time
Dim axial_ready_time As Double = axial_start_time + axial_ramp_on
Dim cleanup_fild_start_time As Double = axial_ready_time
Dim cleanup_fild_mid_time As Double = cleanup_fild_start_time + bfield_dur
Dim cleanup_wait_time As Double = cleanup_fild_mid_time + 30
Dim cleanup_fild_end_time As Double = cleanup_fild_mid_time + bfield_dur
Dim dipole_start_time As Double = cleanup_fild_end_time + big_lattice_rampdown	
Dim dipole_ready_time As Double = dipole_start_time + reddipole_ramp_on
Dim twod_start_time As Double = dipole_ready_time + dimple_ramp_down
Dim twodmax_ready_time As Double = twod_start_time + twod_chopped_rampup + bandgap_open_rampup*twodmax_rampup/200
'bandgap_open_rampup=50 ramp duration to 0.4 Er
'twodmax_rampup=200 ramp duration from 0.4 Er to 16 Er
Dim t_stop As Double
    
If MI_en = True Then

'************************************* patching MOSFETS from evaporation sequence *************************************
analogdata.AddStep(0.04,t_start, dimple_ready_time, ps1_ao)

analogdata.AddStep(2.342, t_start, dimple_ready_time, ps2_ao) 'Holds cigar trap on during evaporation

digitaldata.AddPulse(offset_fet, t_start, twodmax_ready_time) 'Offset FET
digitaldata.AddPulse(bias_enable, t_start, twodmax_ready_time) 'Enables bias coils
digitaldata.AddPulse(transport_13, t_start, twodmax_ready_time) 'Keeps T13 on through remainder of experiment
digitaldata.AddPulse(ps5_enable, t_start, twodmax_ready_time)
digitaldata2.AddPulse(ioffe_mirror_fet, t_start, twodmax_ready_time)
digitaldata.AddPulse(quad_shim, t_start, twodmax_ready_time)
digitaldata2.AddPulse(single_quad_shim, bfield_off_time, twodmax_ready_time)
digitaldata.AddPulse(ps4_shunt, t_start, twodmax_ready_time)
digitaldata.AddPulse(imaging_coil, t_start, twodmax_ready_time)

'*********************************************** begin xyz movement of atoms in cigar ************************************************
analogdata.AddSmoothRamp(0,quic_push*ps5_scaler, t_start, t_start + cigar_move_time, ps5_ao)
analogdata.AddStep(quic_push*ps5_scaler, t_start+ cigar_move_time, dimple_ready_time, ps5_ao)

analogdata.AddStep(0, t_start - 4, t_start, ps6_ao)
analogdata.AddSmoothRamp(0, ps6_scaler*(quic_push*quic_mirror_ratio/16.0) + ps6_offset, t_start, t_start + cigar_move_time,ps6_ao)
analogdata.AddStep(ps6_scaler*(quic_push*quic_mirror_ratio/16.0) + ps6_offset, t_start + cigar_move_time, cigar_move_end_time + biglattice_ramp_on,ps6_ao)

analogdata.AddSmoothRamp(0.04, quad_axis_position, t_start, t_start + cigar_move_time, ps1_ao)
analogdata.AddStep(quad_axis_position, t_start + cigar_move_time, cigar_move_end_time + biglattice_ramp_on, ps1_ao)
analogdata.AddSmoothRamp(.875, 0.03, t_start, t_start + cigar_move_time, ps3_ao)
analogdata.AddStep(0.03, t_start + cigar_move_time, dimple_ready_time, ps3_ao)


analogdata.AddSmoothRamp(0, black_coil_zed*ps7_scaler, t_start, t_start + cigar_move_time, ps7_ao)
analogdata.AddSmoothRamp(black_coil_zed*ps7_scaler, spherical_black_coil*ps7_scaler, t_start + cigar_move_time, vertical_move_end_time, ps7_ao)
analogdata.AddStep(spherical_black_coil*ps7_scaler, vertical_move_end_time, dimple_ready_time, ps7_ao)

'*************************************beginning of big lattice ramp on*****************************************************************
analogdata2.AddStep(biglattice_galvo_volt, cigar_move_end_time-1000, twodmax_ready_time, biglattice_galvo)

digitaldata2.AddPulse(big_lattice_ttl, cigar_move_end_time-1, dipole_start_time+1)
digitaldata2.AddPulse(big_lattice_shutter, cigar_move_end_time-40, dipole_start_time)
analogdata.AddSmoothRamp(1.9,biglattice_midPower, cigar_move_end_time, cigar_move_end_time+biglattice_ramp_on, big_lattice_power)
analogdata.AddStep(biglattice_midPower,cigar_move_end_time+biglattice_ramp_on,cigar_move_end_time+biglattice_ramp_on+spherical_creation_time,big_lattice_power)
analogdata.AddSmoothRamp(biglattice_midPower,biglattice_maxPower, cigar_move_end_time+biglattice_ramp_on+spherical_creation_time,cigar_move_end_time+1.5*biglattice_ramp_on+spherical_creation_time, big_lattice_power)
analogdata.AddStep(biglattice_maxPower, cigar_move_end_time+1.5*biglattice_ramp_on+spherical_creation_time, axial_ready_time, big_lattice_power)
analogdata.AddSmoothRamp(biglattice_maxPower,1.9, axial_ready_time, dipole_start_time, big_lattice_power)

'conversion to spherical trap here
analogdata.AddSmoothRamp(0,siphoned_current/16.0,cigar_move_end_time+biglattice_ramp_on,cigar_move_end_time+biglattice_ramp_on+spherical_creation_time,bias_siphon)
analogdata.AddStep(siphoned_current/16.0,cigar_move_end_time+biglattice_ramp_on+spherical_creation_time,dimple_ready_time,bias_siphon)


'Ioffe mirror is ramped during conversion to spherical trap to offset the shift in trap location
analogdata.AddSmoothRamp(ps6_scaler*quic_push*quic_mirror_ratio/16.0 + ps6_offset, _
    ps6_scaler*quic_axis_offset + ps6_offset, cigar_move_end_time + biglattice_ramp_on, _
    cigar_move_end_time + biglattice_ramp_on + spherical_creation_time,ps6_ao)
analogdata.AddStep(ps6_scaler*quic_axis_offset + ps6_offset,cigar_move_end_time+biglattice_ramp_on+spherical_creation_time,dimple_ready_time,ps6_ao)
'ps1 is used to offset the shift in quad axis position
analogdata.AddSmoothRamp(quad_axis_position,quad_axis_offset,cigar_move_end_time+biglattice_ramp_on,cigar_move_end_time+biglattice_ramp_on+spherical_creation_time, ps1_ao)
analogdata.AddStep(quad_axis_offset,cigar_move_end_time+biglattice_ramp_on+spherical_creation_time,dimple_ready_time,ps1_ao)

'****************************************Axial lattice***********************************************
digitaldata.AddPulse(ttl_axial_lattice, axial_start_time-1,twodmax_ready_time)
digitaldata.AddPulse(axial_lattice_shutter, axial_start_time-50, twodmax_ready_time)

analogdata.AddSmoothRamp(1.76,intermediate_axial_voltage,axial_start_time,axial_ready_time,axial_lattice_power)
analogdata.AddStep(intermediate_axial_voltage,axial_ready_time,dipole_ready_time,axial_lattice_power)
analogdata.AddSmoothRamp(intermediate_axial_voltage,axial_voltage,dipole_ready_time,twod_start_time,axial_lattice_power)
analogdata.AddStep(axial_voltage,twod_start_time,twodmax_ready_time,axial_lattice_power)

'******************************************** beginning of dimple ramp on ****************************
digitaldata2.AddPulse(round_dimple_shutter, dimple_start_time-20, twod_start_time)
digitaldata2.AddPulse(round_dimple_ttl, dimple_start_time-1,twod_start_time)


analogdata2.AddSmoothRamp(1.7, round_dimple_voltage, dimple_start_time, dimple_ready_time, round_dimple_power)
analogdata2.AddStep(round_dimple_voltage, dimple_ready_time, axial_start_time, round_dimple_power)
analogdata2.AddSmoothRamp(round_dimple_voltage, dimple_max_volt, axial_start_time, axial_ready_time, round_dimple_power)
analogdata2.AddStep(dimple_max_volt,axial_ready_time, dipole_ready_time, round_dimple_power)
analogdata2.AddSmoothRamp(dimple_max_volt,1.7,dipole_ready_time,twod_start_time, round_dimple_power)
'**********************************************Blue Doughnut******************************************
dipole_voltage = red_calib_volt+Log10(red_freq/red_calib_freq)
digitaldata2.AddPulse(anticonfin_ttl, dipole_start_time-1,twodmax_ready_time)
digitaldata2.AddPulse(anticonfin_shutter, dipole_start_time-20, twodmax_ready_time)

'Dim a20hz_voltage As Double = red_calib_volt+Log10(20/red_calib_freq)
'Dim dipole_start_volt As Double = dipole_convert(20, red_calib_freq, red_calib_volt)
Dim dipole_start_volt As Double = dipole_convert(red_freq, red_calib_freq, red_calib_volt)
Dim kink_time As Double = 225
Dim bdt_calib_time As Double = 200
'200ms is original ramp duration for going 0.4 Er to 16 Er
'times to specific lattice depths were measured using 200ms
Dim kink_0p5Er_time As Double = twod_start_time + 55*twodmax_rampup/bdt_calib_time
Dim kink_1p0Er_time As Double = twod_start_time + 88*twodmax_rampup/bdt_calib_time
Dim kink_5p0Er_time As Double = twod_start_time + 173*twodmax_rampup/bdt_calib_time
Dim kink_7p5Er_time As Double = twod_start_time + 198*twodmax_rampup/bdt_calib_time
Dim kink_10p0Er_time As Double = twod_start_time + 213*twodmax_rampup/bdt_calib_time
Dim kink_12p0Er_time As Double = twod_start_time + 222*twodmax_rampup/bdt_calib_time
Dim kink_18p0Er_time As Double = twod_start_time + 243*twodmax_rampup/bdt_calib_time
'Dim kink_20p0Er_time As Double = twod_start_time + 248*twodmax_rampup/bdt_calib_time
Dim kink_20p0Er_time As Double = twod_start_time + 285*twodmax_rampup/bdt_calib_time

'0.5 Er --> 2.604 V --> 55 ms
'1   Er --> 2.755 V --> 88 ms
'5   Er --> 3.104 V --> 173 ms
'7.5 Er --> 3.192 V --> 198 ms
'10  Er --> 3.255 V --> 213 ms
'12  Er --> 3.295 V --> 222 ms
'18  Er --> 3.383 V --> 243 ms
'20  Er --> 3.405 V --> 243 ms

analogdata.AddSmoothRamp(1.44, dipole_start_volt, dipole_start_time, dipole_ready_time, red_dipole_power)
analogdata.AddStep(dipole_start_volt, dipole_ready_time, twod_start_time, red_dipole_power) 'calibration

'Dim kink_0p5Er_volt As Double = dipole_convert(27.5, red_calib_freq, red_calib_volt)
'Dim kink_1p0Er_volt As Double = dipole_convert(30, red_calib_freq, red_calib_volt)
'Dim kink_5p0Er_volt As Double = dipole_convert(37, red_calib_freq, red_calib_volt)
'Dim kink_7p5Er_volt As Double = dipole_convert(41, red_calib_freq, red_calib_volt)
''Dim kink_10p0Er_volt As Double = dipole_convert(41, red_calib_freq, red_calib_volt)
'Dim kink_12p0Er_volt As Double = dipole_convert(47, red_calib_freq, red_calib_volt)
'Dim kink_20p0Er_volt As Double = dipole_convert(47, red_calib_freq, red_calib_volt)
Dim kink_0p5Er_volt As Double = dipole_convert(20 + bdt_scaler*7.5, red_calib_freq, red_calib_volt)
Dim kink_1p0Er_volt As Double = dipole_convert(20 + bdt_scaler*10, red_calib_freq, red_calib_volt)
Dim kink_5p0Er_volt As Double = dipole_convert(20 + bdt_scaler*17, red_calib_freq, red_calib_volt)
Dim kink_7p5Er_volt As Double = dipole_convert(20 + bdt_scaler*21, red_calib_freq, red_calib_volt)
'Dim kink_10p0Er_volt As Double = dipole_convert(41, red_calib_freq, red_calib_volt)
Dim kink_12p0Er_volt As Double = dipole_convert(20 + bdt_scaler*27, red_calib_freq, red_calib_volt)
Dim kink_20p0Er_volt As Double = dipole_convert(20 + bdt_scaler*27, red_calib_freq, red_calib_volt)

If isLinearDoughnut = 0 Then 'PMP
	''uncomment '' for stiched ramp
	''analogdata.AddLogRamp(dipole_start_volt, kink_0p5Er_volt, twod_start_time, kink_0p5Er_time, red_dipole_power)	
	''analogdata.AddLogRamp(kink_0p5Er_volt, kink_1p0Er_volt, kink_0p5Er_time, kink_1p0Er_time, red_dipole_power)
	''analogdata.AddLogRamp(kink_1p0Er_volt, kink_5p0Er_volt, kink_1p0Er_time, kink_5p0Er_time, red_dipole_power)
	''analogdata.AddLogRamp(kink_5p0Er_volt, kink_7p5Er_volt, kink_5p0Er_time, kink_7p5Er_time, red_dipole_power)
	'analogdata.AddLogRamp(kink_7p5Er_volt, kink_10p0Er_volt, kink_7p5Er_time, kink_10p0Er_time, red_dipole_power)
	''analogdata.AddLogRamp(kink_7p5Er_volt, kink_12p0Er_volt, kink_7p5Er_time, kink_12p0Er_time, red_dipole_power)
	''analogdata.AddLogRamp(kink_12p0Er_volt, kink_20p0Er_volt, kink_12p0Er_time, kink_20p0Er_time, red_dipole_power)
	''analogdata.AddStep(kink_20p0Er_volt, kink_20p0Er_time, twodmax_ready_time, red_dipole_power)
	'analogdata.AddStep(dipole_convert(90, red_calib_freq, red_calib_volt), kink_18p0Er_time, twodmax_ready_time, red_dipole_power)

	'for comparing WB ramp to linear ramp, added 150804, AMK
	analogdata.AddLogRampRedDipole(dipole_voltage,red_freq,lattice1_start_depth,twod_ramp_midpoint,twod_start_time,twod_start_time+bandgap_open_rampup,red_dipole_power)
	end_dipole_voltage= analogdata.AddRampRedDipole(dipole_voltage,red_freq,twod_ramp_midpoint,lattice1_max,twod_start_time+bandgap_open_rampup,twodmax_ready_time,red_dipole_power)
Else
	'analogdata.AddRamp(dipole_start_volt, kink_20p0Er_volt, twod_start_time, kink_20p0Er_time, red_dipole_power)
	'analogdata.AddStep(kink_20p0Er_volt, kink_20p0Er_time, twodmax_ready_time, red_dipole_power)
	analogdata.AddRamp(dipole_start_volt, 2.58, twod_start_time, kink_20p0Er_time, red_dipole_power)
	analogdata.AddStep(2.58, kink_20p0Er_time, twodmax_ready_time, red_dipole_power)
End If

'end_dipole_voltage = dipole_voltage
' Red dipole compensates during 2D lattice rampup 
'analogdata.AddLogRampRedDipole(dipole_voltage,red_freq,lattice1_start_depth,twod_ramp_midpoint,twod_start_time,twod_start_time+bandgap_open_rampup,red_dipole_power)
'end_dipole_voltage= analogdata.AddRampRedDipole(dipole_voltage,red_freq,twod_ramp_midpoint,lattice1_max,twod_start_time+bandgap_open_rampup,twodmax_ready_time,red_dipole_power)

'******************************************** ramp down bfields ***********************************************************************
Dim cleanup_B_fild_volt As Double = 0.05
'PS 1
analogdata.AddSmoothRamp(quad_axis_offset, cleanup_B_fild_volt,dimple_ready_time,bfield_off_time, ps1_ao)
analogdata.AddSmoothRamp(cleanup_B_fild_volt, quad_gravity_offset,bfield_off_time,axial_start_time,ps1_ao)
analogdata.AddStep(quad_gravity_offset,axial_start_time,twodmax_ready_time,ps1_ao)

'analogdata.AddSmoothRamp(quad_axis_offset, 0, dimple_ready_time, bfield_off_time, ps1_ao)'Ramp down PS6
'analogdata.AddStep(0 , bfield_off_time, cleanup_fild_start_time, ps1_ao)
'analogdata.AddSmoothRamp(0, cleanup_B_fild_volt, cleanup_fild_start_time, cleanup_fild_mid_time, ps1_ao)
'analogdata.AddStep(cleanup_B_fild_volt, cleanup_fild_mid_time, cleanup_wait_time, ps1_ao)
'analogdata.AddSmoothRamp(cleanup_B_fild_volt, quad_gravity_offset, cleanup_wait_time, cleanup_fild_end_time, ps1_ao)
'analogdata.AddStep(quad_gravity_offset, cleanup_fild_end_time, twodmax_ready_time, ps1_ao)

'PS 2
analogdata.AddSmoothRamp(2.342,0,dimple_ready_time,bfield_off_time, ps2_ao)'Ramp down PS2
'PS 3
analogdata.AddSmoothRamp(0.03,0,dimple_ready_time,bfield_off_time, ps3_ao)'Ramp down PS3
'PS 5
analogdata.AddSmoothRamp(quic_push*ps5_scaler,0,dimple_ready_time,bfield_off_time, ps5_ao)'Ramp down PS5
'PS 6
analogdata.AddSmoothRamp(ps6_scaler*quic_axis_offset + ps6_offset,0 + ps6_offset, dimple_ready_time, bfield_off_time, ps6_ao)'Ramp down PS6
analogdata.AddSmoothRamp(0 + ps6_offset, ps6_scaler*quic_gravity_offset + ps6_offset, bfield_off_time, axial_start_time, ps6_ao)
analogdata.AddStep(ps6_scaler*quic_gravity_offset + ps6_offset,axial_start_time,twodmax_ready_time,ps6_ao)

'analogdata.AddSmoothRamp(ps6_scaler*quic_axis_offset + ps6_offset,0 + ps6_offset, dimple_ready_time, bfield_off_time, ps6_ao)'Ramp down PS6
'analogdata.AddStep(0 + ps6_offset, bfield_off_time, cleanup_fild_start_time, ps6_ao)
'analogdata.AddSmoothRamp(0 + ps6_offset, cleanup_B_fild_volt, cleanup_fild_start_time, cleanup_fild_mid_time, ps6_ao)
'analogdata.AddStep(cleanup_B_fild_volt, cleanup_fild_mid_time, cleanup_wait_time, ps6_ao)
'analogdata.AddSmoothRamp(cleanup_B_fild_volt, ps6_scaler*quic_gravity_offset + ps6_offset, cleanup_wait_time, cleanup_fild_end_time, ps6_ao)
'analogdata.AddStep(ps6_scaler*quic_gravity_offset + ps6_offset,cleanup_fild_end_time,twodmax_ready_time,ps6_ao)
'PS 7
analogdata.AddSmoothRamp(black_coil_zed*ps7_scaler,0,dimple_ready_time,bfield_off_time, ps7_ao)
'Siphon
analogdata.AddSmoothRamp(siphoned_current/16.0,0,dimple_ready_time,bfield_off_time,bias_siphon)

'************************************************************** conservative 2D lattice rampup *********************************

digitaldata2.AddPulse(lattice2D765_ttl,twod_start_time-1, twodmax_ready_time)
digitaldata2.AddPulse(lattice2D765_ttl2,twod_start_time-1,  twodmax_ready_time)
digitaldata2.AddPulse(lattice2D765_shutter,twod_start_time-20, twodmax_ready_time)
digitaldata2.AddPulse(lattice2D765_shutter2,twod_start_time-20, twodmax_ready_time)

analogdata.AddLogRamp(lattice1_start_volt, lattice1_midpoint_volt, twod_start_time, twod_start_time + bandgap_open_rampup*twodmax_rampup/200, lattice2D765_power)'PMP
analogdata.AddLogRamp(lattice2_start_volt, lattice2_midpoint_volt, twod_start_time, twod_start_time + bandgap_open_rampup*twodmax_rampup/200, lattice2D765_power2)
analogdata.AddRamp(lattice1_midpoint_volt, lattice1_max_volt, twod_start_time + bandgap_open_rampup*twodmax_rampup/200, twodmax_ready_time, lattice2D765_power)
analogdata.AddRamp(lattice2_midpoint_volt, lattice2_max_volt, twod_start_time + bandgap_open_rampup*twodmax_rampup/200, twodmax_ready_time, lattice2D765_power2)
'*********************************************End of Mott Inuslator Sequence****************************************

'On at the end of the sequence
'beams: 2D1, 2D2, axial, red dipole
'coils: quad_gravity, quic_gravity

	t_stop = twodmax_ready_time
Else
        t_stop = t_start
        end_dipole_voltage = 1.0
        
End If

    Return {t_stop, lattice1_max_volt, lattice2_max_volt, end_dipole_voltage}
End Function

Public Function dipole_convert(ByVal freq As Double, _
    ByVal calib_freq As Double, _
    ByVal calib_volt As Double) As Double

    Dim dipole_voltage As Double = calib_volt+Log10(freq/calib_freq)

    Return dipole_voltage
End Function

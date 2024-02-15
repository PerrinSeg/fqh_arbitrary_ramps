Public Function AddTrackingSequenceUpgrade(ByVal t_start As Double,
	ByRef cp As clsControlParams,
	ByRef analogdata As SpectronClient,
	ByRef analogdata2 As SpectronClient,
	ByRef digitaldata As digitalDAQdata,
	ByRef digitaldata2 As digitalDAQdata,
	ByRef gpib As GPIBControl,
	ByRef Hermes As KeithleyControl,
	ByRef dds As AD9959Ev,
	ByVal tracking_en As Boolean) As Double


	Dim flip_time As Double = t_start
	Dim IT As Double = flip_time + guppy_wait
	Dim last_time = IT + 3 * guppy_interval + 2000
	Dim t_stop As Double

	If tracking_en = True Then
		'---------------------------------- Invert signals for upgrade ----------------------------------
		'digitaldata2.AddPulse(ixon_flip_mount_ttl, flip_time, IT + 3*guppy_interval)
		digitaldata2.AddPulse(ixon_flip_mount_ttl, IT + 3 * guppy_interval, last_time)
		'------------------------------------------------------------------------------------------------

		analogdata.AddStep(DMD_track_765_power, IT - 10, IT + 3 * guppy_interval, lattice2D765_power)
		analogdata.AddStep(DMD_track_765_power2, IT - 10, IT + 3 * guppy_interval, lattice2D765_power2)
		analogdata2.AddStep(DMD_track_power_line, IT - 10, IT + 3 * guppy_interval, line_DMD_power)
		analogdata2.AddStep(DMD_track_power_hor, IT - 10, IT + 3 * guppy_interval, hor_DMD_power)

		digitaldata2.AddPulse(lattice2D765_shutter, IT - 50, IT + 3 * guppy_interval)
		digitaldata2.AddPulse(lattice2D765_shutter2, IT - 50, IT + 3 * guppy_interval)
		digitaldata2.AddPulse(line_DMD_shutter, IT - 50, IT + 3 * guppy_interval)
		digitaldata2.AddPulse(hor_DMD_shutter, IT - 50, IT + 3 * guppy_interval)


		digitaldata2.AddPulse(lattice2D765_ttl, IT, IT + guppy_exposure) '1
		digitaldata2.AddPulse(lattice2D765_ttl2, IT, IT + guppy_exposure)
		digitaldata2.AddPulse(line_DMD_ttl, IT, IT + guppy_dot_exposure)
		digitaldata2.AddPulse(hor_DMD_ttl, IT, IT + guppy_dot_exposure)

		digitaldata2.AddPulse(lattice2D765_ttl, IT + guppy_interval, IT + guppy_interval + guppy_exposure) '2
		digitaldata2.AddPulse(lattice2D765_ttl2, IT + guppy_interval, IT + guppy_interval + guppy_exposure)
		digitaldata2.AddPulse(line_DMD_ttl, IT + guppy_interval, IT + guppy_interval + guppy_dot_exposure)
		digitaldata2.AddPulse(hor_DMD_ttl, IT + guppy_interval, IT + guppy_interval + guppy_dot_exposure)

		digitaldata2.AddPulse(lattice2D765_ttl, IT + 2 * guppy_interval, IT + 2 * guppy_interval + guppy_exposure) '3
		digitaldata2.AddPulse(lattice2D765_ttl2, IT + 2 * guppy_interval, IT + 2 * guppy_interval + guppy_exposure)
		digitaldata2.AddPulse(line_DMD_ttl, IT + 2 * guppy_interval, IT + 2 * guppy_interval + guppy_dot_exposure)
		digitaldata2.AddPulse(hor_DMD_ttl, IT + 2 * guppy_interval, IT + 2 * guppy_interval + guppy_dot_exposure)

		digitaldata.AddPulse(apogee_camera, IT - 1000 - shutter_time, IT - 1000) 'flush camera
		digitaldata.AddPulse(apogee_camera, IT - shutter_time, IT) '1
		digitaldata.AddPulse(apogee_camera, IT - shutter_time + guppy_interval, IT + guppy_interval) '2
		digitaldata.AddPulse(apogee_camera, IT - shutter_time + 2 * guppy_interval, IT + 2 * guppy_interval) '3

		t_stop = last_time
	Else
		t_stop = t_start
	End If

	Return t_stop
End Function

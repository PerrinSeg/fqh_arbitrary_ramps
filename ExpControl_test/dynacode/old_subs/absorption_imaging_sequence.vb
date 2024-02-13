Public Function AddAbsorptionImagingSequence(ByVal t_start As Double, _
    ByRef cp As clsControlParams, _
    ByRef analogdata As SpectronClient, _
    ByRef analogdata2 As SpectronClient, _
    ByRef digitaldata As digitalDAQdata, _
    ByRef digitaldata2 As digitalDAQdata, _
    ByRef gpib As GPIBControl, _
    ByRef Hermes As KeithleyControl, _
    ByRef dds As AD9959Ev, _
    ByVal absorption_en As Boolean) As Double


    Dim beam_pic_time As Double = t_start + 1000
    Dim dark_pic_time As Double = beam_pic_time + 1000
	Dim t_stop As Double
    
    If absorption_en = True Then

'*********************************************************** Begin Absorption Imaging ********************************************************************
'Atom Image
digitaldata.AddPulse(apogee_camera, t_start-1000-shutter_time, t_start-1000) 'flush camera
digitaldata.AddPulse(apogee_camera, t_start-shutter_time, t_start-10) 'camera atoms image
digitaldata.AddPulse(probe_shutter, t_start-20, t_start) 'imaging Shutter
digitaldata.AddPulse(repump_shutter, t_start-35, t_start)
digitaldata.AddPulse(polarizer_shutter_22, t_start-20, t_start) 'imaging Shutter from side
digitaldata.AddPulse(ttl_78MHz, t_start-40, t_start-.3 )
digitaldata.AddPulse(ttl_78MHz, t_start, t_start + 10) ' pump atoms from 1 to 2
digitaldata.AddPulse(ttl_80MHz, t_start, t_start + imaging_time) '-80MHz TTL
digitaldata.AddPulse(ttl_133MHz,t_start, t_start + imaging_time) 'imaging AOM


'Beam Image
digitaldata.AddPulse(apogee_camera, beam_pic_time - shutter_time, beam_pic_time - 10) 'triggers camera. takes beam pic.
digitaldata.AddPulse(polarizer_shutter_22, beam_pic_time - 20, beam_pic_time + imaging_time) 'imaging shutter from side
digitaldata.AddPulse(probe_shutter, beam_pic_time - 20, beam_pic_time + imaging_time) 'imaging shutter on z axis
digitaldata.AddPulse(ttl_80MHz, beam_pic_time, beam_pic_time + imaging_time)
digitaldata.AddPulse(ttl_133MHz, beam_pic_time, beam_pic_time + imaging_time) 'imaging AOM
digitaldata2.AddPulse(imaging2aom, beam_pic_time, beam_pic_time + imaging_time) 'imaging AOM

'Dark Image
digitaldata.AddPulse(apogee_camera, dark_pic_time, dark_pic_time + 10) 'triggers camera. takes dark pic.

        t_stop = dark_pic_time
    Else
        t_stop = t_start
      
    End If

    Return t_stop
End Function

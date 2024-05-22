Public Function BeatVolt(ByVal beat_freq As Double) As Double
    ' Assuming PLL set to 80 MHz and E4400 set to 80 MHz with FM Dev = 2 kHz
    'Dim beat_volt As Double = 0.6993006993006993*(-0.18490000000781492 + beat_freq)
    'Dim beat_volt As Double = -0.6993006993006993*(-1.8151 + beat_freq)
    'Dim beat_volt As Double = 1.315 - 0.7655 * beat_freq + 0.02292 * ( beat_freq ^ 2 )
    Dim beat_volt As Double = -0.7233 * beat_freq + 1.3002

    ' check to prevent the DAC from getting stuck
    If beat_volt < -1.2 Then 
        beat_volt = -1.2 'if it tries to output less than -1.2V, then set to -1.2V 
    Else If beat_volt > 1.2 'if it tries to output more than the maximum allowed voltage, set to the maximum.
        beat_volt = 1.2  
    End If

    Return beat_volt
End Function

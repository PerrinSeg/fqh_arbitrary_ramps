Public Function BeatVolt(ByVal beat_freq As Double) As Double
    ' Assuming PLL set to 80 MHz and E4400 set to 80 MHz with FM Dev = 2 kHz
    'Dim beat_volt As Double = 0.6993006993006993*(-0.18490000000781492 + beat_freq)
    Dim beat_volt As Double = -0.6993006993006993*(-1.8151 + beat_freq)


    Return beat_volt
End Function

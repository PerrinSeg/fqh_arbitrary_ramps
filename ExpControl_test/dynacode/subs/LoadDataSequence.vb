Public Function LoadRampSegmentsFromFile(ramp_file_name As String, n_variables As Integer) As Double()()

	Dim ramp_variables(n_variables + 1)() As Double
	Dim v_idx As Integer = 0
	Dim t_idx As Integer = 0

	Dim str_array As String() = System.IO.File.ReadAllLines(ramp_file_name)
	Dim line_count As Integer = str_array.Length
	Console.WriteLine("lineCount = {0}", line_count)
	
	For v_idx = 0 To n_variables
		ramp_variables(v_idx) = New Double(line_count - 1) {}
	Next

	For t_idx = 0 To line_count - 1
		'Console.WriteLine("line {0}", t_idx)
		Dim line_str As String = str_array(t_idx)
		'Console.WriteLine("    line {0} = {1}", t_idx, line_str)

		Dim line_str_split As String() = line_str.Split(" ")

		For v_idx = 0 To n_variables
			Dim newstr As String = line_str_split(v_idx)

			'Convert string to double, store in array
			Dim new_val As Double
			If Double.TryParse(newstr, new_val) Then
				ramp_variables(v_idx)(t_idx) = new_val
				'Console.WriteLine("    newVal = {0}", new_val)
			Else
				Microsoft.VisualBasic.Interaction.MsgBox("BAD LINE FILE LINE! (cannot convert text {0} to Double)", line_str_split(v_idx))
			End If
		Next
	Next

	'For v_idx = 0 To n_variables
	'	For t_idx = 0 To line_count - 1
	'		Console.WriteLine("v = {1}, t = {0}, ramp_variables(v_idx)(t_idx) = {2}", t_idx, v_idx, ramp_variables(v_idx)(t_idx))
	'	Next
	'Next

	Return ramp_variables
End Function
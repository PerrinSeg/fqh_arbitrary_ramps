Public Function  LoadArrayFromFile(filename As String) As Double()
	' Load a 1D array from a single column of numbers in a text file.

	Dim idx As Integer = 0

	Dim str_array As String() = System.IO.File.ReadAllLines(filename)
	Dim line_count As Integer = str_array.Length
	'Console.WriteLine("lineCount = {0}", line_count)
	
	Dim array_values(line_count-1) As Double

	For idx = 0 To line_count - 1
		'Console.WriteLine("line {0}", idx)
		Dim line_str As String = str_array(idx)
		'Console.WriteLine("    line {0} = {1}", idx, line_str)
		Dim new_val As Double
		If Double.TryParse(line_str, new_val) Then
			array_values(idx) = new_val
			'Console.WriteLine("    newVal = {0}", new_val)
		Else
			Microsoft.VisualBasic.Interaction.MsgBox("BAD FILE LINE! (cannot convert text {0} to Double)", line_str)
		End If
	Next

	'For v_idx = 0 To n_variables
	'	For t_idx = 0 To line_count - 1
	'		Console.WriteLine("v = {1}, t = {0}, ramp_variables(v_idx)(t_idx) = {2}", t_idx, v_idx, ramp_variables(v_idx)(t_idx))
	'	Next
	'Next

	Return array_values
End Function
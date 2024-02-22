function ramp_variables = read_loadArrayFromFile(filename)
    filename
    % detectImportOptions(filename)
    ramp_variables_aux = readmatrix(filename);
    % size(ramp_variables_aux)
    
    % ramp_variables = num2str(ramp_variables_aux);
    for i = 1:size(ramp_variables_aux)        
        ramp_variables{i} = num2str(ramp_variables_aux(i));
    end
    
    % size(ramp_variables)
    % ramp_variables
    % ramp_variables{:}

end


function ramp_variables = read_loadRampSegmentsFromFile(filename)
    filename
    % detectImportOptions(filename)
    ramp_variables_aux = readmatrix(filename);
    % size(ramp_variables_aux)
    for i = 1:size(ramp_variables_aux,2)
        for j = 1:size(ramp_variables_aux,1)
            ramp_variables{i}{j} = num2str(ramp_variables_aux(j,i));
        end
    end
    % size(ramp_variables)
    % ramp_variables
    % ramp_variables{:}

end


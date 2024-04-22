function logExpParam = import_logExpParam(path_files, batch_line)

    logExpParam_file = fopen([path_files 'logExpParam.txt']);

    % Useless lines
    for i = 1:12
        fgetl(logExpParam_file);
    end
    
    % Line that contains the variable names
    logExpParam_aux = strrep(split(fgetl(logExpParam_file), ","), ' ', '');
    
    % Another useless line
    fgetl(logExpParam_file);

    % The batch lines that we don't consider
    for i = 1:batch_line-1
        fgetl(logExpParam_file);
    end
    
    % Line that contains the corresponding values
    logExpParam_aux_2 = strrep(split(fgetl(logExpParam_file), ","), ' ', '');
    
%     fclose(logExpParam_file)

    % Number of ExpParams (there is nothing after the last comma)
    N_logExpParam = numel(logExpParam_aux) - 1;
    logExpParam = {};
    for i = 1:N_logExpParam
        logExpParam{i} = {lower(logExpParam_aux{i}), lower(logExpParam_aux_2{i})};
%         disp(logExpParam{i})
    end
    
    fclose(logExpParam_file);
end
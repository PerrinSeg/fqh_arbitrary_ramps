function ExpConstants = import_ExpConstants(path_files)
    
    ExpConstants_file = fopen(strcat(path_files, 'ExpConstants.txt')); 
    ExpConstants = {}; % Will contain all the ExpConstants
    ExpConstants_aux = fgetl(ExpConstants_file);
    
    while ischar(ExpConstants_aux)   
        ExpConstants{end+1} = lower(strrep(split(ExpConstants_aux, ["=", "'"]), ' ', ''));
        ExpConstants_aux = fgetl(ExpConstants_file);
    end
end

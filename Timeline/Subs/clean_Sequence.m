function Sequence = clean_Sequence(path_files, name_sequence)
    
    % Open the document, remove the redundant "Variables" part, all lines that
    % are commented, and the empty lines. 
    
    Sequence_file = fopen(strcat(path_files, name_sequence)); 
    Sequence = {}; % Will contain all the relevant lines
    after_variable = 0; % Use this flag to remove the lines with the default variable values
    Sequence_aux = fgetl(Sequence_file);
    
    while ischar(Sequence_aux) % While the file has not be fully read
        if contains(Sequence_aux, "'=====Variables=====")
            after_variable = after_variable + 1;
        end
        if after_variable == 2
            if ~startsWith(Sequence_aux, "'") && ~ (numel(Sequence_aux) == 0)
                Sequence_aux = split(Sequence_aux, "'"); % Remove also possible comments at the end
                Sequence{end+1} = lower(strtrim(Sequence_aux{1}));
            end      
        end
        Sequence_aux = fgetl(Sequence_file);
    end

    % Reconstruct instructions that are wrapped on multiple lines (they finish with "_")
    for i = 1:numel(Sequence)
        
        if numel(Sequence{i}) > 0
            if endsWith(Sequence{i}, ',') || endsWith(Sequence{i}, '=') % This is a mistake (consequence-less) that happens multiple times
                Sequence{i} = strcat(Sequence{i}, '_');
            end
            if endsWith(Sequence{i}, '_')
                Sequence{i} = Sequence{i}(1:end-1);
                i_aux = i;
                flag = 1; % Indicates that the end of the instruction is still to be found
                while flag
                    i_aux = i_aux + 1;
                    if endsWith(Sequence{i_aux}, ',') || endsWith(Sequence{i_aux}, '=')
                        Sequence{i_aux} = strcat(Sequence{i_aux}, '_');
                    end
                    if endsWith(Sequence{i_aux}, '_')
                        Sequence{i} = strcat(Sequence{i}, Sequence{i_aux}(1:end-1));
                    else
                        flag = 0;
                        Sequence{i} = strcat(Sequence{i}, Sequence{i_aux}(1:end));
                    end
                    Sequence{i_aux} = {};
                end
            end
        end
    end
    
    Sequence = Sequence(~cellfun('isempty', Sequence));
    
    fclose(Sequence_file);

end
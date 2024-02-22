function [Sequence, variable_list, arr_variable_list, sub_variable_containers, instruction_list, arguments_list, logExpParam, ExpConstants] = read_Sub_Sequence(variable, path_files, Sequence, variable_list, arr_variable_list, sub_variable_containers, instruction_list, arguments_list, logExpParam, ExpConstants)
       
    % Some functions used below
    firstCell = @(x) x{1};
    findIndex = @(list, element) find(strcmp(cellfun(firstCell, list, 'UniformOutput', false), element));


    %% Read the name and the arguments of the sub-sequence

    % The name
    name_sub_sequence_aux = split(variable{2}, "(");
    name_sub_sequence_aux = name_sub_sequence_aux{1};
    name_sub_sequence = erase(name_sub_sequence_aux, lower("Me."));
%     disp(name_sub_sequence)
    
    % The arguments - We assume that no calculation is performed when passing the argument to the sub-sequence 
    arguments = split(variable{2}, "(");
    arguments = arguments{2};
    arguments = erase(arguments, ")");
    arguments = split(arguments, ", ");

    % Find the corresponding .vb file
    sub_path = strcat(path_files, "subs/");
    if strcmpi(name_sub_sequence, 'AddMOTSequence')
        name_sub_sequence = "mot_sequence.vb";
    elseif strcmpi(name_sub_sequence, 'AddMOTSequenceUpgrade')
        name_sub_sequence = "mot_sequence_upgrade.vb";
    elseif strcmpi(name_sub_sequence, 'AddTransportSequence')
        name_sub_sequence = "transport_sequence.vb";
    elseif strcmpi(name_sub_sequence, 'AddTransportSequenceUpgrade')
        name_sub_sequence = "transport_sequence_upgrade.vb";
    elseif strcmpi(name_sub_sequence, 'AddTransportSequenceRewind')
        name_sub_sequence = "transport_sequence_rewind.vb";
    elseif strcmpi(name_sub_sequence, 'AddTransportSequenceRewindMiddle')
        name_sub_sequence = "transport_sequence_rewind_middle.vb";
    elseif strcmpi(name_sub_sequence, 'AddEvaporationSequence')
        name_sub_sequence = "evaporation_sequence.vb";
    elseif strcmpi(name_sub_sequence, 'AddEvaporationSequenceUpgrade')
        name_sub_sequence = "evaporation_sequence_upgrade.vb";
    elseif strcmpi(name_sub_sequence, 'AddEvaporationSequenceWithEvapTimeAsVariable')
        name_sub_sequence = "evaporation_sequence_evaptimeasvariable.vb";
    elseif strcmpi(name_sub_sequence, 'AddMottInsulatorSequence')
        name_sub_sequence = "MI_sequence.vb";
    elseif strcmpi(name_sub_sequence, 'AddMottInsulatorSequenceUpgrade')
        name_sub_sequence = "MI_sequence_upgrade.vb";
    elseif strcmpi(name_sub_sequence, 'AddMottInsulatorSequenceWithBlackCoilValuesAsVariables')
        name_sub_sequence = "MI_sequence_blackcoilvaluesasvariables.vb";
    elseif strcmpi(name_sub_sequence, 'AddMottInsulatorSequenceWithBlackCoilValuesAsVariables_v2')
        name_sub_sequence = "MI_sequence_blackcoilvaluesasvariables_v2.vb";
    elseif strcmpi(name_sub_sequence, 'AddMottInsulatorSequenceWithBlackCoilValuesAsVariables_v3')
        name_sub_sequence = "MI_sequence_blackcoilvaluesasvariables_v3.vb";
    elseif strcmpi(name_sub_sequence, 'AddMottInsulatorSequenceWithBlackCoilValuesAsVariables_v3p1')
        name_sub_sequence = "MI_sequence_blackcoilvaluesasvariables_v3p1.vb";
    elseif strcmpi(name_sub_sequence, 'MIsub2023')
        name_sub_sequence = "MI_sub_2023.vb";        
    elseif strcmpi(name_sub_sequence, 'AddLoadDimpleSequence')
        name_sub_sequence = "dimple_sequence.vb";
    elseif strcmpi(name_sub_sequence, 'AddBlueDonutSequence')
        name_sub_sequence = "blue_donut_sequence.vb";
    elseif strcmpi(name_sub_sequence, 'AddPinningSequence')
        name_sub_sequence = "pinning_sequence.vb";
    elseif strcmpi(name_sub_sequence, 'AddPinningSequenceUpgrade')
        name_sub_sequence = "pinning_sequence_upgrade.vb";
    elseif strcmpi(name_sub_sequence, 'AddTrackingSequence2')
        name_sub_sequence = "tracking_sequence_2.vb";
    elseif strcmpi(name_sub_sequence, 'AddTrackingSequence')
        name_sub_sequence = "tracking_sequence.vb";
    elseif strcmpi(name_sub_sequence, 'AddTrackingSequenceUpgrade')
        name_sub_sequence = "tracking_sequence_upgrade.vb";
    elseif strcmpi(name_sub_sequence, 'AddAbsorptionImagingSequence')
        name_sub_sequence = "absorption_imaging_sequence.vb";
    elseif strcmpi(name_sub_sequence, 'AddAbsorptionImagingSequenceUpgrade')
        name_sub_sequence = "absorption_imaging_sequence_upgrade.vb";
    else
        disp("Sub-sequence is not known")
    end
    
    [Sub_Sequence, sub_variable_list, sub_arr_variable_list] = clean_Subs(sub_path, name_sub_sequence, arguments, variable_list, arr_variable_list, logExpParam, ExpConstants);


    %% Read the code line-by-line
    
    while numel(Sub_Sequence) > 0 
    
        % The line being read is always the first one
%         disp(Sub_Sequence{1})
      
        % Case A
        if startsWith(lower(Sub_Sequence{1}), lower("If "))
            Sub_Sequence = simplify_If(Sub_Sequence, sub_variable_list, sub_arr_variable_list, logExpParam, ExpConstants);
    
        % Case B
        elseif contains(lower(Sub_Sequence{1}), "=") || startsWith(lower(Sub_Sequence{1}), lower("dim "))     
            [Sub_Sequence, sub_variable_list, sub_arr_variable_list, sub_variable_containers, instruction_list, arguments_list, logExpParam, ExpConstants] = compute_Variable( ...
                Sub_Sequence, sub_variable_list, sub_arr_variable_list, sub_variable_containers, instruction_list, arguments_list, logExpParam, ExpConstants, path_files);
                       
        % Case C
        elseif contains(Sub_Sequence{1}, "analogdata") || contains(lower(Sub_Sequence{1}), "digitaldata")
            [Sub_Sequence, instruction_list, arguments_list] = read_Instruction(Sub_Sequence, sub_variable_list, sub_arr_variable_list, instruction_list, arguments_list, logExpParam, ExpConstants);
         
        elseif startsWith(Sub_Sequence{1}, 'return')

            % Get what is after return
            current_line = erase(Sub_Sequence{1}, 'return');
            current_line = strtrim(split(current_line, ["{", ",", "}"]));
            current_line = strtrim(current_line(~cellfun('isempty', current_line)));
            
            % Get the destination of this function return
            if findIndex(variable_list, variable{1})
                j = findIndex(variable_list, variable{1});
                variable_list{j} = {};
                variable_list = variable_list(~cellfun('isempty', variable_list));
            elseif findIndex(arr_variable_list, variable{1})
                j = findIndex(arr_variable_list, variable{1});
                % disp("variable in array")
                % arr_variable_list{j}{1} = {};
                % arr_variable_list = variable_list(~cellfun('isempty', variable_list));
            else
                disp(['variable ' variable{1} ' not found (read sub sequence'])
            end

            for k = 1:numel(current_line)
                
                % Sometimes the outputs are not used
                l = findIndex(sub_variable_list, current_line{k});
                value = sub_variable_list{l}{2};
                if numel(current_line) == 1
                    variable_list{end+1} = {variable{1}, value};
                else
                    % If there are multiple outputs, rename the ouput without parenthesis
                    % like variable(0), variable(1), because this is
                    % messing with the rest of the reading
                    variable_list{end+1} = {char(strcat(variable{1}, '_', num2str(k-1))), value};
                    for m = 1:numel(Sequence)
                        if contains(Sequence{m}, char(strcat(variable{1}, '(', num2str(k-1), ')')))
                            Sequence{m} = replace(Sequence{m}, char(strcat(variable{1}, '(', num2str(k-1), ')')), char(strcat(variable{1}, '_', num2str(k-1))) );
                        end
                    end
                end
            end
        
            Sub_Sequence{1} = {};
            Sub_Sequence = Sub_Sequence(~cellfun('isempty', Sub_Sequence));

        else
            Sub_Sequence{1} = {};
            Sub_Sequence = Sub_Sequence(~cellfun('isempty', Sub_Sequence));
        end
    end

    % Store the variables computed in containers.Map
    sub_variable_containers(erase(name_sub_sequence_aux, lower("Me."))) = sub_variable_list;

end
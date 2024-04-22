
function [instruction_list, arguments_list, variable_list, arr_variable_list, sub_variable_containers, ExpConstants, logExpParam] = read_Sequence(path_files, name_sequence, batch_line)

    %% Import constants and parameters
        
    ExpConstants = import_ExpConstants(path_files);
    logExpParam = import_logExpParam(path_files, batch_line); 
    Sequence = clean_Sequence(path_files, name_sequence);
    
    
    %% Read the code line-by-line
    
    % We read each line of Sequence, remove it, until Sequence is empty.
    % We use "lower" in most cases to be case-insensitive, "strtrim" to remove
    % useless blank spaces
    
    % We consider three options:
    
    % - Case A: "If ... Then ... (Else ...) End If"
    %       -> keep only the lines that are executed
    
    % - Case B: declaration or computation of a variable
    %       -> store the new variable, compute its value
    %       -> in some cases the new variable is defined from a sub-sequence or
    %       a sub-function, such that we have to go at a deeper level
    variable_list = {};
    arr_variable_list = {};
    
    % - Case C: It's an instruction
    %       -> save it and extract + compute all its arguments
    instruction_list = {}; % Instructions ("digitaldata.something..." or "analogdata.something...")
    arguments_list = {}; % Arguments of each instruction
    sub_variable_containers = containers.Map;
    
    while numel(Sequence) > 0 
    
        % The line being read is always the first one
%         disp(Sequence{1})
      
        % Case A
        if startsWith(Sequence{1}, "if ")
            Sequence = simplify_If(Sequence, variable_list, arr_variable_list, logExpParam, ExpConstants);
        
        % extra case
        elseif startsWith(Sequence{1},"console.")
            Sequence{1} = {};
            Sequence = Sequence(~cellfun('isempty', Sequence));
        
        elseif startsWith(Sequence{1}, "for ")
            Sequence = simplify_For(Sequence, variable_list, arr_variable_list, logExpParam, ExpConstants);
        
        % Case B
        elseif contains(Sequence{1}, "=") || startsWith(Sequence{1}, "dim ")      
            [Sequence, variable_list, arr_variable_list, sub_variable_containers, instruction_list, arguments_list, logExpParam, ExpConstants] = compute_Variable( ...
                Sequence, variable_list, arr_variable_list, sub_variable_containers, instruction_list, arguments_list, logExpParam, ExpConstants, path_files);

        % Case C
        elseif contains(Sequence{1}, "analogdata") || contains(Sequence{1}, "digitaldata")
            [Sequence, instruction_list, arguments_list] = read_Instruction(Sequence, variable_list, arr_variable_list, instruction_list, arguments_list, logExpParam, ExpConstants);

        else
            Sequence{1} = {};
            Sequence = Sequence(~cellfun('isempty', Sequence));
        end
    end
end


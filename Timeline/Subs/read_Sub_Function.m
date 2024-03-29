function [Sequence, variable_list, arr_variable_list, sub_variable_containers] = read_Sub_Function(variable, path_files, Sequence, variable_list, arr_variable_list, sub_variable_containers, instruction_list, arguments_list, logExpParam, ExpConstants)
       
    % Some functions used below
    firstCell = @(x) x{1};
    findIndex = @(list, element) find(strcmp(cellfun(firstCell, list, 'UniformOutput', false), element));
    
    %% Read the name and the arguments of the sub-function
    sub_flag = 1;
    % The name
    name_sub_function_aux = split(variable{2}, "(");
    name_sub_function = name_sub_function_aux{1};

    % disp(' ')
    % disp("ENTERED READ SUB FUNCTION")
    % disp(variable)
    % disp(name_sub_function)
    
    % The arguments - We assume that no calculation is performed when passing the argument to the sub-sequence 
    arguments = split(variable{2}, "("); % TO DO: fix so that it doesn't get rid of indexed variables used as input!!!!!!
    arguments = join({arguments{2:end}}, "(");
    arguments = arguments{1}(1:end-1);
    arguments = split(arguments, ", ");

    % Find the corresponding .vb file
    sub_path = strcat(path_files, "subs/");
    if strcmpi(name_sub_function, "DepthToVolts")
        name_sub_function = "DepthToVoltsConv.vb";
    elseif strcmpi(name_sub_function, "BeatVolt")
        name_sub_function = "e4400_pll_beat_freq.vb";
    elseif strcmpi(name_sub_function, "GaugeJToVolts")
        name_sub_function = "GaugeJToVoltsConv.vb";
    elseif strcmpi(name_sub_function, "JToVolts")
        name_sub_function = "JToVoltsConv.vb";
    else
        disp("Sub-function is not known")
    end
    
    [Sub_Function, sub_variable_list, sub_arr_variable_list] = clean_Subs(sub_path, name_sub_function, arguments, variable_list, arr_variable_list, logExpParam, ExpConstants);
    
    % disp(" sub_variable_list:")
    % sub_variable_list{:}
    % disp(" sub_arr_variable_list:")
    % sub_arr_variable_list{:}
    % disp(' ')

    %% Read the code line-by-line
        
    while numel(Sub_Function) > 0 
    
        % The line being read is always the first one
        % disp(Sub_Function{1})
        % disp(' ')

        % Case A
        if startsWith(lower(Sub_Function{1}), lower("If"))
            Sub_Function = simplify_If(Sub_Function, sub_variable_list, sub_arr_variable_list, logExpParam, ExpConstants);
        
        elseif startsWith(lower(Sub_Function{1}), lower("For"))
            Sub_Function = simplify_For(Sub_Function, sub_variable_list, sub_arr_variable_list, logExpParam, ExpConstants);
    
        % Case B
        elseif contains(lower(Sub_Function{1}), "=") || contains(lower(Sub_Function{1}), lower("Dim"))
            % disp('COMPUTING VARIABLE IN SUB FUNCTION')
            % disp(' Sub_Function{1} before compute_Variable:')
            % Sub_Function{1}

            [Sub_Function, sub_variable_list, sub_arr_variable_list, sub_variable_containers, instruction_list, arguments_list, logExpParam, ExpConstants] = compute_Variable( ...
                Sub_Function, sub_variable_list, sub_arr_variable_list, sub_variable_containers, instruction_list, arguments_list, logExpParam, ExpConstants, path_files);
            
            % disp('')
            % disp(' Sub_Function{1} after compute_Variable:')
            % Sub_Function{1}
            % disp(" sub_variable_list:")
            % sub_variable_list{:}
            % disp(" sub_arr_variable_list:")
            % sub_arr_variable_list{:}
            % disp(' ')


        elseif startsWith(lower(Sub_Function{1}), lower('Return')) % TO DO: incorporate array return values!!(or don't, and treat the new array function like an NI function...)
            % disp('Return value')            
            % Get what is after return
            current_line = erase(Sub_Function{1}, lower('Return'));
            current_line = strtrim(split(current_line, ["{", ",", "}"]));
            current_line = strtrim(current_line(~cellfun('isempty', current_line)));
            
            var_split = split(variable{1},"(");

            % Get the destination of this function return
            if findIndex(variable_list, variable{1})
                i = findIndex(variable_list, variable{1});
                variable_list{i} = {};
                variable_list = variable_list(~cellfun('isempty', variable_list));
                
            elseif findIndex(arr_variable_list, var_split{1})
                % disp("variable in array (read sub function) ")
                % var_split{1}

                i_cell = [];
                for j = 2:numel(var_split)
                    [i_cell_split, i_cell_symbol] = split(var_split{j}(1:end-1), ["+", "-", "*"]);                    
                    % extract list of symbols dividing parts
                    for k = 1:numel(i_cell_split)
                        i_cell_str = i_cell_split{k};
                        if isnan(str2double(i_cell_str)) % array size is set by a variable
                            if findIndex(variable_list, i_cell_str)
                                i_cell_str = variable_list{findIndex(variable_list, i_cell_str)}{2};
                            elseif findIndex(logExpParam, i_cell_str)
                                i_cell_str = logExpParam{findIndex(logExpParam, i_cell_str)}{2};
                            end
                        end
                        i_cell_split{k} = i_cell_str;
                    end
                    i_cell_str = join(i_cell_split, i_cell_symbol);
                    i_cell(j-1) = round(str2sym(i_cell_str));
                end
                sub_flag = 0; 

            else
                disp(['variable ' variable{1} ' not found (read sub function'])
            end

            for k = 1:numel(current_line)
                l = findIndex(sub_variable_list, current_line{k});
                value = sub_variable_list{l}{2};
                if numel(current_line) == 1
                    if sub_flag == 0
                        i = findIndex(arr_variable_list, var_split{1});
                        if numel(i_cell) == 1
                            arr_variable_list{i}{3}{i_cell(1)+1} = value;
                        elseif numel(i_cell) > 1
                            arr_variable_list{i}{3}{i_cell(1)+1}{i_cell(2)+1} = value;
                        else
                            arr_variable_list{i}{3} = value;
                        end
                        % disp([' now setting ' arr_variable_list{i}{1} ' to new value:'])
                        % value
                        % disp(' new arr_variable_list{i}{3}:')
                        % arr_variable_list{i}{3}

                    else
                        variable_list{end+1} = {variable{1}, value};
                        % disp(' ADDED NEW VARIABLE:')
                        % variable_list{end}
                    end
                else
                    % If there are multiple outputs, rename the ouput without parenthesis
                    % like variable(0), variable(1), because this is messing with the rest of the reading
                    % A relic of before reading arrays was implemented more generally.
                    % Should only effect certain variables (the ones not stored in arr_variable_list)
                    variable_list{end+1} = {char(strcat(variable{1}, '_', num2str(k-1))), value};
                    for m = 1:numel(Sequence)
                        if contains(Sequence{m}, char(strcat(variable{1}, '(', num2str(k-1), ')')))
                            Sequence{m} = replace(Sequence{m}, char(strcat(variable{1}, '(', num2str(k-1), ')')), char(strcat(variable{1}, '_', num2str(k-1))) );
                        end
                    end
                end
            end

            Sub_Function{1} = {};
            Sub_Function = Sub_Function(~cellfun('isempty', Sub_Function));

        else
            Sub_Function{1} = {};
            Sub_Function = Sub_Function(~cellfun('isempty', Sub_Function));
        end
    end

    % Store the variables computed in containers.Map
    sub_variable_containers(name_sub_function_aux{1}) = sub_variable_list;
    % disp('END read_Sub_Function')
end
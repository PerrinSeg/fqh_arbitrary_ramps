function [Sequence, instruction_list, arguments_list] = read_Instruction(Sequence, variable_list, arr_variable_list, instruction_list, arguments_list, logExpParam, ExpConstants)
    Sequence{1} = erase(Sequence{1}, " ");  
    % Some functions used below
    firstCell = @(x) x{1};
    findIndex = @(list, element) find(strcmp(cellfun(firstCell, list, 'UniformOutput', false), element));

    flag = 0;
    % problemline = lower('analogdata2.AddStep(gauge_power_ramp_end_v');
    % if contains(Sequence{1}, problemline)
    %     disp(' ')
    %     disp('ENTERED read_Instruction !!!!!!!!!!')
    %     disp('  found problem line:')
    %     disp(problemline)
    %     flag = 1;
    % end

    % Save the line corresponding to this instruction
    instruction_list{end+1} = Sequence{1};
    arguments_list{end+1} = {};

    % Get the name of the function
    % name_fun = split(Sequence{1}, "(");
    % name_fun = name_fun{1};
    if flag
        disp(Sequence{1})
    end
    % And all the arguments
    % arg_fun = split(Sequence{1}, "(");
    % arg_fun = arg_fun{2};
    % arg_fun = erase(arg_fun, ")");
    % arg_fun = split(arg_fun, ", ");

    % Replace opening and closing parenthesis of function by brackets for easier reading
    open_function = strfind(Sequence{1}, '(');
    Sequence{1}(open_function(1)) = '[';
    close_function = strfind(Sequence{1}, ')');
    Sequence{1}(close_function(end)) = ']';

    % Read all the arguments
    instruction = split(Sequence{1}, ["[", ",", "]"]);
    instruction_aux = split(instruction{1}, ".");
    instruction{1} = instruction_aux{2};
    instruction = instruction(~cellfun('isempty', instruction));
    instruction = strtrim(instruction);
    
    for k = 1:numel(instruction)
        instruction{k} = erase(instruction{k}," ");
        [ins_split_aux, ins_split_idx_aux] = split(instruction{k}, ["+", "-", "/", "*"]); 
        if flag
            disp('NEW INSTRUCTION:')
            ins_split_aux
            ins_split_idx_aux
        end

        for j = 1:numel(ins_split_aux)
            % If one piece of the formula is a known variable, replace by its value              
            ins_split_aux{j} = strtrim(ins_split_aux{j});
            if flag
                disp(" Next piece to process: ")
                disp(ins_split_aux{j})
            end
            if isempty(ins_split_aux{j})
                continue
            end
            
            nextvar = split(ins_split_aux{j}, "(");

            if findIndex(arr_variable_list, nextvar{1})
                if flag
                    disp("  RHS HAS ARRAY COMPONENT")
                    nextvar
                end
                i = findIndex(arr_variable_list, nextvar{1});
                val = arr_variable_list{i}{3};
                if numel(nextvar) > 1
                    i_cell_str = '';
                    n = 1;
                    for jj = 2:numel(nextvar)
                        i_cell_str = strcat(i_cell_str, '(', nextvar{jj});
                        while ~contains(i_cell_str(end),")")
                            new_str = ins_split_aux{j+n};
                            i_cell_str = strcat(i_cell_str, ins_split_idx_aux{j + n - 1}, strtrim(new_str));
                            ins_split_aux{j+n} = {};
                            ins_split_idx_aux{j + n - 1} = {};
                            n = n+1;
                        end
                    end
                    i_cell_str_init = i_cell_str;
                    i_cell_str = strcat(nextvar{1}, i_cell_str);
                    nextvar = split(i_cell_str,"(");
                    for jj = 2:(numel(nextvar))  
                        i_cell_str = nextvar{jj}(1:end-1);
                        var_idx = round(str2double(i_cell_str));
                        if isnan(var_idx)
                            [i_cell_split, i_cell_symbol] = split(i_cell_str, ["+", "-", "*"]);
                            for jjj = 1:numel(i_cell_split)
                                i_cell_str = i_cell_split{jjj};
                                if isnan(round(str2double(i_cell_str)))
                                    if findIndex(variable_list, i_cell_str)
                                        i_cell_str = variable_list{findIndex(variable_list, i_cell_str)}{2};
                                    elseif findIndex(logExpParam, i_cell_str)
                                        i_cell_str = logExpParam{findIndex(logExpParam, i_cell_str)}{2};
                                    end
                                end
                                i_cell_split{jjj} = i_cell_str;
                            end
                            i_cell_str = join(i_cell_split,i_cell_symbol);
                            var_idx = round(str2sym(i_cell_str));
                        end
                        val = val{var_idx + 1};
                    end

                    if numel(string(val))>1
                        if flag
                            disp("new value:")
                            val
                        end
                        instruction{k} = val;
                    else
                        if flag
                            disp(" NOW REPLACING ")
                            disp(strcat(nextvar{1}, i_cell_str_init))
                            disp(" WITH")
                            val
                        end
                        instruction{k} = replace(instruction{k}, strcat(nextvar{1},  i_cell_str_init), val);
                    end
                else
                    val = arr_variable_list{i}{3};
                    instruction{k} = val;
                end
            end
        end
        if flag
            disp("instruction with no more array variables")
            instruction{k}
        end
        instruction_aux = instruction{k};
        instruction_length = numel(string(instruction_aux));
        for j = 1:instruction_length
            if instruction_length > 1
                nextvar = instruction_aux{j};
            else
                nextvar = instruction_aux;
            end
            [instruction_split, instruction_split_symbols] = split(nextvar, ["+", "-", "/", '*', '(', ')']);
            if flag
                instruction_split
                instruction_split_symbols
            end
            for ii = 1:numel(instruction_split)    
                instruction_split{ii} = strtrim(instruction_split{ii});   
                if flag
                    disp('next instruction split piece:')
                    instruction_split{ii}
                end
                if findIndex(variable_list, instruction_split{ii})
                    i = findIndex(variable_list, instruction_split{ii});
                    val = variable_list{i}{2};
                    instruction_split{ii} = val;
                    if flag
                        disp('found in variables:')
                        instruction_split
                    end
                elseif findIndex(logExpParam, instruction_split{ii})
                    i = findIndex(logExpParam, instruction_split{ii});
                    val = logExpParam{i}{2};
                    instruction_split{ii} = val;
                    if flag
                        disp('found in logExpParam:')
                        instruction_split
                    end
                elseif findIndex(ExpConstants, instruction_split{ii})
                    i = findIndex(ExpConstants, instruction_split{ii});
                    val = ExpConstants{i}{2};
                    instruction_split{ii} = val;
                    if flag
                        disp('found in ExpConstants:')
                        instruction_split
                    end
                end
            end 
            nextvar = join(instruction_split, instruction_split_symbols);
            
            if flag
                disp(' ')
                disp("final instructions split:")
                instruction_split
                disp("after joining:")
                nextvar
            end

            try
                nextvar = char(str2sym(nextvar));
            catch
                nextvar = nextvar{1};
            end
            if instruction_length > 1
                instruction_aux{j} = nextvar;
            else
                instruction_aux = nextvar;
            end
        end
        instruction{k} = instruction_aux;
        % final step: add the instruction piece to the end of the list of arguments.
        arguments_list{end}{k} = instruction{k};
    end

    if flag == 1
        arguments_list{end}
        arguments_list{end}{2}
        disp('end read_Instruction for flagged line.')
    end

    % Remove all the lines that should be removed
    Sequence{1} = {};
    Sequence = Sequence(~cellfun('isempty', Sequence));

end
    

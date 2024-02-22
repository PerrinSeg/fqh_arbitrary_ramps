function [Sub_Sequence, sub_variable_list, sub_arr_variable_list] = clean_Subs(sub_path, name_sub_sequence, arguments, variable_list, arr_variable_list, logExpParam, ExpConstants)
    disp("BEGIN CLEAN SUBS")
    % Some functions used below
    firstCell = @(x) x{1};
    findIndex = @(list, element) find(strcmp(cellfun(firstCell, list, 'UniformOutput', false), element));
    
    arguments = erase(arguments," ");

    % Open the subsequence file
    Sub_Sequence_file = fopen(strcat(sub_path, name_sub_sequence));
    sub_variable_list = {};
    sub_arr_variable_list = {};
    
    Sub_Sequence_aux = strtrim(fgetl(Sub_Sequence_file));    
    Sub_Sequence_aux = split(Sub_Sequence_aux, "(");
    Sub_Sequence_aux = [Sub_Sequence_aux{2:end}];

    % Read until the function's arguments are exhausted
    counter = 1; % Line number
    not_exhausted = 1;
    while not_exhausted
        % Keep only the values useful for us ("ByVal")
        Sub_Sequence_aux = lower(split(Sub_Sequence_aux, " "));
        if strcmpi(Sub_Sequence_aux{1}, 'ByVal')
            sub_variable_list{end+1} = {Sub_Sequence_aux{2}, erase(arguments{counter}," ")};
        end
        counter = counter + 1;
        not_exhausted = ~contains(Sub_Sequence_aux, ")");
        Sub_Sequence_aux = strtrim(fgetl(Sub_Sequence_file));
    end
    % disp('sub_variable_list = ')
    % sub_variable_list{:}

    % Now replace with the values given the main sequence
    N_arg = numel(sub_variable_list);
    for j = 1:N_arg
        % disp("SEARCHING FOR:")
        % sub_variable_list{j}
        nextsubvar = sub_variable_list{j}{2};
        nextsubvar = strsplit(nextsubvar, "(");
        if findIndex(variable_list, sub_variable_list{j}{2})
            i = findIndex(variable_list, sub_variable_list{j}{2});
            sub_variable_list{j} = {sub_variable_list{j}{1}, variable_list{i}{2}};
        elseif findIndex(arr_variable_list, nextsubvar{1})
            % disp("found subvar argument assignment in array variable list")
            i = findIndex(arr_variable_list, nextsubvar{1});
            dummyarr = arr_variable_list{i}{3};
            for jj = 2:numel(nextsubvar)
                nextsubvar{jj} = nextsubvar{jj}(1:end-1);
                [i_cell_split, i_cell_symbol] = split(nextsubvar{jj}, ["+", "-", "*", "/"]);
                for k = 1:numel(i_cell_split)
                    i_cell_str = i_cell_split{k};
                    i_cell = round(str2double(i_cell_str));
                    if isnan(i_cell) % array size is set by a variable
                        if findIndex(variable_list, i_cell_str)
                            i_cell_str = variable_list{findIndex(variable_list, i_cell_str)}{2};
                        elseif findIndex(logExpParam, i_cell_str)
                            i_cell_str = logExpParam{findIndex(logExpParam, i_cell_str)}{2};
                        end
                    end
                    i_cell_split{k} = i_cell_str;
                end
                i_cell_str = join(i_cell_split,i_cell_symbol);
                i_cell = round(str2sym(i_cell_str));
                dummyarr = string(dummyarr{i_cell+1}); % NOTE: this may not work if it still a cell array...        
            end
            if numel(dummyarr)>1
                % disp("dummy array is still an array")
                sub_arr_variable_list{end+1} = {sub_variable_list{j}{1}, sub_variable_list{j}{2}, dummyarr};
                sub_variable_list{j} = {};
            else
                % j
                % sub_variable_list{j}
                sub_variable_list{j} = {sub_variable_list{j}{1}, dummyarr};
                % sub_variable_list{j}
            end
            % TO DO: So far, this just assumes that you are extracting a single value from an array and assigning it to a regular varialbe. 
             % Needs fixing if you want the new variable to be an array as
             % well.  SOLVED???

        elseif findIndex(ExpConstants, sub_variable_list{j}{2})
            i = findIndex(ExpConstants, sub_variable_list{j}{2});
            sub_variable_list{j} = {sub_variable_list{j}{1}, ExpConstants{i}{2}};
        elseif findIndex(logExpParam, sub_variable_list{j}{2})
            i = findIndex(logExpParam, sub_variable_list{j}{2});
            sub_variable_list{j} = {sub_variable_list{j}{1}, logExpParam{i}{2}};
        end  
    end
    sub_variable_list = sub_variable_list(~cellfun('isempty', sub_variable_list));

    % Simplify the useless parts
    Sub_Sequence = {};
    while ischar(Sub_Sequence_aux)
        Sub_Sequence_aux = strtrim(Sub_Sequence_aux);
        if ~startsWith(Sub_Sequence_aux, "'") && ~ (numel(Sub_Sequence_aux) == 0)
            Sub_Sequence_aux = split(Sub_Sequence_aux, "'");
            Sub_Sequence{end+1} = lower(strtrim(Sub_Sequence_aux{1}));
        end
        Sub_Sequence_aux = fgetl(Sub_Sequence_file);
    end
    
    % Combine lines that are not wrapped on % multiple lines (they finish with "_")
    for i = 1:numel(Sub_Sequence)
            
        if numel(Sub_Sequence{i}) > 0
            
            if endsWith(Sub_Sequence{i}, ',') || endsWith(Sub_Sequence{i}, '=') % This is a mistake (consequence-less) that happens multiple times
                Sub_Sequence{i} = strcat(Sub_Sequence{i}, '_');
            end
            if endsWith(strtrim(Sub_Sequence{i}), '_')
                Sub_Sequence{i} = Sub_Sequence{i}(1:end-1);
                i_aux = i;
                flag = 1;
                while flag
                    i_aux = i_aux + 1;
                    if endsWith(Sub_Sequence{i_aux}, ',') || endsWith(Sub_Sequence{i}, '=')
                        Sub_Sequence{i_aux} = strcat(Sub_Sequence{i_aux}, '_');
                    end
                    if endsWith(Sub_Sequence{i_aux}, '_')
                        Sub_Sequence{i} = strcat(Sub_Sequence{i}, Sub_Sequence{i_aux}(1:end-1));
                    else
                        flag = 0;
                        Sub_Sequence{i} = strcat(Sub_Sequence{i}, Sub_Sequence{i_aux}(1:end));
                    end
                    Sub_Sequence{i_aux} = {};
                end
            end
        end
    end
    
    Sub_Sequence = Sub_Sequence(~cellfun('isempty', Sub_Sequence));
    % disp("  END CLEAN SUBS")
end


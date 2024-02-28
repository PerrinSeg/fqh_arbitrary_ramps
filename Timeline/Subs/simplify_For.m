function Sequence = simplify_For(Sequence, variable_list, arr_variable_list, logExpParam, ExpConstants)

    % disp("STARTING SIMPLIFY FOR")
    % disp('   ')
    % disp(Sequence{1})
    % % disp(Sequence{2})
    % disp('   ')
    % variable_list{:}

    % Some functions used below
    firstCell = @(x) x{1};
    findIndex = @(list, element) find(strcmp(cellfun(firstCell, list, 'UniformOutput', false), element));

    % Extract the two sides of the condition to be tested
    condition = erase(Sequence{1}, "for");
    condition = split(condition, ["as", "=", "to"]);

    % Evaluate the left-hand side
    idx_name = strtrim(condition{1});
    idx_start_str = strtrim(condition{3});
    idx_end_str = strtrim(condition{end});

    idx_start_val = round(str2double(idx_start_str));
    if isnan(idx_start_val)
       [i_cell_split, i_cell_symbol] = split(idx_start_str, ["+", "-", "*"]);
            % extract list of symbols dividing parts
            for k = 1:numel(i_cell_split)
                i_cell_str = i_cell_split{k};
                i_cell = round(str2double(i_cell_str));
                if isnan(i_cell) % array size is set by a variable
                    if findIndex(variable_list, i_cell_str)
                        ii = findIndex(variable_list, i_cell_str);
                        i_cell_str = variable_list{ii}{2};
                    elseif findIndex(logExpParam, i_cell_str)
                        ii = findIndex(logExpParam, i_cell_str);
                        i_cell_str = logExpParam{ii}{2};
                    end
                end
                i_cell_split{k} = i_cell_str;
            end
            idx_start_str = join(i_cell_split,i_cell_symbol);
            idx_start_val = round(double(str2sym(idx_start_str)));
    end
    
    idx_end_val = round(str2double(idx_end_str));
    if isnan(idx_end_val)
       [i_cell_split, i_cell_symbol] = split(idx_end_str, ["+", "-", "*"]);
            % extract list of symbols dividing parts
            for k = 1:numel(i_cell_split)
                % disp("next cell")
                i_cell_str = strtrim(i_cell_split{k});
                i_cell = round(str2double(i_cell_str));
                if isnan(i_cell) % array size is set by a variable
                    if findIndex(variable_list, i_cell_str)
                        % disp("found in var list")
                        ii = findIndex(variable_list, i_cell_str);
                        i_cell_str = variable_list{ii}{2};
                    elseif findIndex(logExpParam, i_cell_str)
                        % disp("found in exp param")
                        ii = findIndex(logExpParam, i_cell_str);
                        i_cell_str = logExpParam{ii}{2};
                    else
                        disp(['variable ' i_cell_str ' not found (simplify_For)'])
                    end
                end
                i_cell_split{k} = i_cell_str;
            end
            idx_end_str = join(i_cell_split, i_cell_symbol);
            idx_end_val = round(double(str2sym(idx_end_str)));
    end
    % idx_name
    % idx_start_val
    % idx_end_val
    % Sequence{1} = {};
    
    sequence_aux = {};
    for j = idx_start_val:idx_end_val
        i = 2;
        idx_str = num2str(j);
        while ~startsWith(Sequence{i}, "next")
            sequence_aux{end+1} = replace(Sequence{i}, idx_name, idx_str);
            i = i + 1;
        end
    end

    for k = 1:i
        Sequence{k} = {};
    end
    Sequence = [sequence_aux, Sequence];    

end
function [Sequence, variable_list, arr_variable_list, sub_variable_containers, instruction_list, arguments_list, logExpParam, ExpConstants] = compute_Variable( ...
    Sequence, variable_list, arr_variable_list, sub_variable_containers, instruction_list, arguments_list, logExpParam, ExpConstants, path_files)
    
    % NOTE: might be nice to add function find index that takes a set of values in
    % parentheis and checks the lists for variables until it can output an
    % integer... or even a more general "replace variables" function to
    % consolidate the "search lists and replace with value" proccess

    % variable_list{:}
    % arr_variable_list{:}
    % Some functions used below
    firstCell = @(x) x{1};
    findIndex = @(list, element) find(strcmp(cellfun(firstCell, list, 'UniformOutput', false), element));

    % List of "simple function" names
    list_simple_function = {lower("DepthToVolts"), lower("BeatVolt")};

    % List of "analytical function" and their Matlab symbolic notation
    list_analytical_function = {{'Log10', 'log10'}};
    
    list_other_function = {lower("LoadRampSegmentsFromFile"), lower("LoadArrayFromFile")};

    i_cell = -1;
    i_cell2 = -1;

    % It can be a new variable (declaration with "Dim") or an old one (no declaration)
    if startsWith(Sequence{1}, 'dim ')
        % disp("NEW DIM")
        variable = erase(Sequence{1}, "dim ");
        variable_init = strtrim(split(variable, '='));
        variable = variable_init;
        variable_aux = split(variable{1}, ' as ');
        variable{1} = strtrim(variable_aux{1});
        variable_aux2 = split(variable{1}, '(');
        if numel(variable_aux) >1
            variable_aux3 = split(variable_aux{2},'(');
        else
            variable_aux3 = {};
        end
        if numel(variable_aux2) > 1
            % disp("  FIXED LENGTH ARRAY")
            variable{1} = variable_aux2{1};            
            n_cells_str = variable_aux2{2}(1:end-1);
            n_cells = round(str2double(n_cells_str));
            if isnan(n_cells) % array size is set by a variable
                if findIndex(variable_list, n_cells_str)
                    i = findIndex(variable_list, n_cells_str);
                    n_cells = round(str2double(variable_list{i}{2}));
                elseif findIndex(logExpParam, n_cells_str)
                    i = findIndex(logExpParam, n_cells_str);
                    n_cells = round(str2double(logExpParam{i}{2}));
                end
            end
            if numel(variable_aux2) == 2
                variable{3} = cell(1,n_cells);
            elseif numel(variable_aux2) == 3
                n_cells_str = variable_aux2{3}(1:end-1);
                n_cells2 = round(str2double(n_cells_str));
                if isnan(n_cells2) % array size is set by a variable
                    if findIndex(variable_list, n_cells_str)
                        i = findIndex(variable_list, n_cells_str);
                        n_cells2 = round(str2double(variable_list{i}{2}));
                    elseif findIndex(logExpParam, n_cells_str)
                        i = findIndex(logExpParam, n_cells_str);
                        n_cells2 = round(str2double(logExpParam{i}{2}));
                    end
                end
                variable{3} = cell(1,n_cells,n_cells2);
            end
            flag = 4; % = 1 if in variable_list ; = 2 if in ExpConstants ; = 3 if in LogExpParams
            arr_variable_list{end+1} = variable;
             
        elseif numel(variable_aux3) == 2
            % disp("  VARIABLE LEN 1D ARRAY")
            variable{3} = {{}};
            flag = 4;
            arr_variable_list{end+1} = variable;

        elseif numel(variable_aux3) == 3
            % disp("  VARIABLE LEN 2D ARRAY")
            variable{3} = {{{}}};
            flag = 4;
            arr_variable_list{end+1} = variable;

        else
            % disp("  REGULAR VARIABLE")
            flag = 1; % = 1 if in variable_list ; = 2 if in ExpConstants ; = 3 if in LogExpParams 
            variable_list{end+1} = variable;
        end
        
        if numel(variable) == 1 % In this case no value is assigned
            variable{2} = {};
        end


    % Otherwise one is overwritting an existing variable
    else 
        % disp("NEW NON DIM")       
        variable_init = strtrim(split(Sequence{1}, '='));
        variable = variable_init;
        % variable
        variable_aux = split(variable{1},'(');
        
        if findIndex(arr_variable_list, variable_aux{1})
            i = findIndex(arr_variable_list, variable_aux{1});

            if numel(variable_aux) > 1
                [i_cell_split, i_cell_symbol] = split(variable_aux{2}(1:end-1), ["+", "-", "*"]);
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
                i_cell_str = join(i_cell_split,i_cell_symbol);
                i_cell = round(str2sym(i_cell_str));

                if numel(variable_aux) == 3
                    [i_cell_split, i_cell_symbol] = split(variable_aux{3}(1:end-1), ["+", "-", "*"]);
                    % extract list of symbols dividing parts
                    for k = 1:numel(i_cell_split)
                        i_cell_str = i_cell_split{k};
                        i_cell2 = round(str2double(i_cell_str));
                        if isnan(i_cell2) % array size is set by a variable
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
                    i_cell_str = join(i_cell_split,i_cell_symbol);
                    i_cell2 = round(str2sym(i_cell_str));
                end
            end

            flag = 4;
            arr_variable_list{i}{2} = variable{2};
            % disp(" ARRAY VARIABLE INITIALLIZED AS ")
            % disp(arr_variable_list{i})
            % disp(arr_variable_list{i}{3})
            % variable

        elseif findIndex(variable_list, variable{1})
            i = findIndex(variable_list, variable{1});
            variable_list{i}{2} = variable{2};
            flag = 1;
        elseif findIndex(ExpConstants, variable{1})
            i = findIndex(ExpConstants, variable{1});
            ExpConstants{i}{2} = variable{2};
            flag = 2;
        elseif findIndex(logExpParam, variable{1})
            i = findIndex(logExpParam, variable{1});
            logExpParam{i}{2} = variable{2};
            flag = 3;
        else
            disp(['variable ' variable{1} ' definition not found'])
        end
    end 
    % disp('  ')
    % disp(['VARIABLE ' variable{1} ' INITIALIZED. RHS IS ' variable{2}])

    % We evaluate the variable if there is an equal sign, ie variable{2} not empty
    if numel(variable_init) > 1
        % disp("SETTING NEW VARIABLE:")
        % disp(variable_init{1})
        % disp(variable{1})
        % disp(variable{2})

        % Sub-sequences and simple functions are assumed to be evaluated separately
        % ie there is nothing else in variable{2}
        
        if contains(variable{2}, 'me.') % Then it's a sub-sequence  
            % disp("VARIABLE SET USING SUB SEQUENCE")
            [Sequence, variable_list, arr_variable_list, sub_variable_containers, instruction_list, arguments_list, logExpParam, ExpConstants] = read_Sub_Sequence( ...
                variable, path_files, Sequence, variable_list, arr_variable_list, sub_variable_containers, ...
                instruction_list, arguments_list, logExpParam, ExpConstants);       

        elseif find(contains(variable{2}, cellfun(firstCell, list_simple_function, 'UniformOutput', false))) % Then it's a sub-function
            % disp("found simple function")
            [Sequence, variable_list, arr_variable_list, sub_variable_containers] = read_Sub_Function( ...
                variable, path_files, Sequence, variable_list, arr_variable_list, ...
                sub_variable_containers, instruction_list, arguments_list, logExpParam, ExpConstants);  
        
        elseif find(contains(variable{2}, cellfun(firstCell, list_other_function, 'UniformOutput', false)))
            name_function_aux = split(variable{2}, "(");
            name_function = name_function_aux{1};
            % disp("FOUND OTHER FUNCTION")
            % disp(variable)
            % disp(name_function)
            
            % The arguments - We assume that no calculation is performed when passing the argument to the sub-sequence             
            arguments = name_function_aux{2};
            arguments = erase(arguments, ")");
            arguments = split(arguments, ",");
        
            % Find the corresponding .vb file
            if strcmpi(name_function, "LoadRampSegmentsFromFile")
                filepath_var_name = strtrim(arguments{1});
                filepathname = variable_list{findIndex(variable_list, filepath_var_name)}{2};
                filepathname = replace(filepathname, '\\', '\');
                filepathname = erase(filepathname, '"');
                filepathname = replace(filepathname, 'c:\users\rblab\documents\github', 'C:\Users\Rb Lab\Documents\GitHub');
                ramp_variable_str = read_loadRampSegmentsFromFile(filepathname);
                variable{3} = ramp_variable_str;
                if findIndex(arr_variable_list, variable{1})
                    i = findIndex(arr_variable_list, variable{1});
                    arr_variable_list{i}{3} = ramp_variable_str;
                elseif findIndex(variable_list, variable{1})
                    %it's not really a regular variable, move it to the array list
                    i = findIndex(variable_list, variable{1});
                    variable_list{i} = {};
                    variable_list = variable_list(~cellfun('isempty', variable_list));
                    arr_variable_list{end + 1} = variable;
                else
                    disp("could not find this variable in the variable lists. ")
                end

            elseif strcmpi(name_function, "LoadArrayFromFile")
                filepath_var_name = strtrim(arguments{1});
                filepathname = variable_list{findIndex(variable_list, filepath_var_name)}{2};
                filepathname = replace(filepathname, '\\', '\');
                filepathname = erase(filepathname, '"');
                filepathname = replace(filepathname, 'c:\users\rblab\documents\github', 'C:\Users\Rb Lab\Documents\GitHub');
                ramp_variable_str = read_loadArrayFromFile(filepathname);
                variable{3} = ramp_variable_str;
                if findIndex(arr_variable_list, variable{1})
                    i = findIndex(arr_variable_list, variable{1});
                    arr_variable_list{i}{3} = ramp_variable_str;
                elseif findIndex(variable_list, variable{1})
                    %it's not really a regular variable, move it to the array list
                    i = findIndex(variable_list, variable{1});
                    variable_list{i} = {};
                    variable_list = variable_list(~cellfun('isempty', variable_list));
                    arr_variable_list{end + 1} = variable;
                else
                    disp("could not find this variable in the variable lists. ")
                end
           
            else
                disp("function (other) not defined in matlab")
            end

        else % Else it's an expression that one can compute directly
            disp("COMPUTING EXPRESSION")
            variable{1} = strtrim(variable{1});
            variable{2} = erase(variable{2}," ")
            
            [variable_split_aux, variable_split_idx_aux] = split(variable{2}, ["+", "-", "/", "*"]); 
            variable_split_aux

            for k = 1:numel(variable_split_aux)
                % If one piece of the formula is a known variable, replace by its value              
                variable_split_aux{k} = strtrim(variable_split_aux{k});
                disp("Next piece to process: ")
                disp(variable_split_aux{k})
                if isempty(variable_split_aux{k})
                    % disp("empty cell")
                    continue
                end
                
                nextvar = split(variable_split_aux{k}, "(");
                nextvar{1} = erase(nextvar{1}, ")"); % HACK TO FIX ISSUE WHEN FEEDING ARGUMENTS FROM SIMPLE FUNCTIONS. FIX LATER...
                if contains(variable{2}, '.getupperbound(0)')
                    disp("REACHED .getuppberbound(0)")
                    var_aux = strsplit(variable{2}, '.getupperbound(0)');
                    var_aux = strsplit(var_aux{1}, ["(",")"]);
                    var_aux = var_aux{1};
                    i = findIndex(arr_variable_list, var_aux);
                    arr_variable_list{i}
                    arr_variable_list{i}{3}
                    val = size(arr_variable_list{i}{3}{1}, 2)-1; % THIS IS A HACK THAT ONLY WORKS FOR THIS PARTICULAR CASE!! fix to be more general
                    variable{2} = num2str(val);  

                elseif findIndex(arr_variable_list, nextvar{1})
                    disp("  RHS HAS ARRAY COMPONENT")
                    nextvar
                    ii = findIndex(arr_variable_list, nextvar{1});
                    val = arr_variable_list{ii}{3};
                    if numel(nextvar)>1                    
                        i_cell_str = '';
                        n = 1;
                        for j = 2:numel(nextvar)                           
                            i_cell_str = strcat(i_cell_str, '(', nextvar{j});
                            while ~contains(i_cell_str(end),")")
                                new_str = variable_split_aux{k+n};
                                i_cell_str = strcat(i_cell_str, variable_split_idx_aux{k + n - 1}, strtrim(new_str));
                                variable_split_aux{k+n} = {};
                                variable_split_idx_aux{k + n - 1} = {};
                                n = n+1;
                            end
                        end
                        i_cell_str_init = i_cell_str;
                        i_cell_str = strcat(nextvar{1}, i_cell_str)
                        nextvar = split(i_cell_str,"(")
                        for j = 2:(numel(nextvar))                            
                            i_cell_str = nextvar{j}(1:end-1);
                            var_idx = round(str2double(i_cell_str));
                            if isnan(var_idx)
                                [i_cell_split, i_cell_symbol] = split(i_cell_str, ["+", "-", "*", "/"]);
                                for jj = 1:numel(i_cell_split)
                                    i_cell_str = i_cell_split{jj};
                                    if isnan(round(str2double(i_cell_str))) % array size is set by a variable
                                        if findIndex(variable_list, i_cell_str)
                                            i_cell_str = variable_list{findIndex(variable_list, i_cell_str)}{2};
                                        elseif findIndex(logExpParam, i_cell_str)
                                            i_cell_str = logExpParam{findIndex(logExpParam, i_cell_str)}{2};
                                        end
                                    end
                                    i_cell_split{jj} = i_cell_str;
                                end
                                i_cell_str = join(i_cell_split,i_cell_symbol);
                                var_idx = round(str2sym(i_cell_str));
                            end
                            val
                            var_idx
                            val = val{var_idx + 1}
                        end
                        val
                        if numel(string(val))>1 % don't have to worry about any additional operations, which have to be done in a loop in vb
                            % val_str = ['{' val{1}];
                            % for jj = 2:numel(val)
                            %     val_str = [val_str ',' val{jj}];
                            % end
                            % val_str = [val_str '}'];
                            % disp("REPLACING ")
                            % disp(strcat(nextvar{1}, i_cell_str_init))
                            % disp(" IN")
                            % variable{2}
                            % disp("WITH")
                            % val_str
                            % variable{2} = replace(variable{2}, strcat(nextvar{1}, i_cell_str_init), val_str);
                            variable{3} = val;
                        else
                            disp("REPLACING ")
                            disp(strcat(nextvar{1}, i_cell_str_init))
                            disp(" IN")
                            variable{2}
                            disp("WITH")
                            val
                            variable{2} = replace(variable{2}, strcat(nextvar{1}, i_cell_str_init), val);
                        end
                    else
                        val
                        % val{1}
                       variable{3} = val;
                       variable{2} = {};
                    end
                end
            end
            nextvar

            
            variable_split = split(variable{2}, ["+", "-", "/", "*", "(", ")"]);
            
            for k = 1:numel(variable_split)

                % If one piece of the formula is a known variable, replace by its value
                nextvar = strtrim(variable_split{k});
                if ~contains(nextvar, '"')
                    if findIndex(list_analytical_function, nextvar)
                        i = findIndex(list_analytical_function, nextvar);
                        val = list_analytical_function{i}{2};
                        variable{2} = replace(variable{2}, nextvar, val);
                        
                    elseif findIndex(variable_list, nextvar)
                        i = findIndex(variable_list, nextvar);
                        val = variable_list{i}{2};
                        variable{2} = replace(variable{2}, nextvar, val);
                        
                    elseif findIndex(logExpParam, nextvar)
                        i = findIndex(logExpParam, nextvar);
                        val = logExpParam{i}{2};
                        variable{2} = replace(variable{2}, nextvar, val);
    
                    elseif findIndex(ExpConstants, nextvar)
                        i = findIndex(ExpConstants, nextvar);
                        val = ExpConstants{i}{2};
                        variable{2} = replace(variable{2}, nextvar, val);
                    end
                end
            end
            
            try
                variable{2} = num2str(double(str2sym(variable{2})));
            catch
            end

            if flag == 1
                i = findIndex(variable_list, variable{1});
                variable_list{i}{2} = variable{2};  
            elseif flag == 2
                i = findIndex(ExpConstants, variable{1});
                ExpConstants{i}{2} = variable{2};
            elseif flag == 3
                i = findIndex(logExpParam, variable{1});
                logExpParam{i}{2} = variable{2};
            elseif flag == 4
                disp("SETTING NEW ARRAY VARIABLE:")
                disp(variable{1})
                disp(" USING STRING")
                disp(variable{2})
                variable_aux = strtrim( split(variable{1}, '(') )
                nextvar = variable_aux{1}
                i = findIndex(arr_variable_list, nextvar);
                if i_cell > -1     
                    i_cell
                    variable
                    if numel(variable_aux) == 2
                        arr_variable_list{i}{3}{i_cell + 1} = variable{2};
                        disp("VARIABLE SET AS ")                      
                        disp(arr_variable_list{i}{3}{i_cell + 1})
                    elseif numel(variable_aux) == 3
                        arr_variable_list{i}{3}{i_cell + 1}{i_cell2 + 1} = variable{2};
                        % variable{3}{i_cell + 1}{i_cell2 + 1} = variable{2};
                        disp("VARIABLE SET AS ")
                        disp(arr_variable_list{i}{3}{i_cell + 1}{i_cell2 + 1})
                    end
                else
                    % split array into individual values, get length of array to assign, search in variables if it isn't already numbers
                    % variable2_split = replace(variable{2}, ["{", "}"], " ");
                    % variable2_split = strtrim(split(variable2_split, ","))
                %     for k = 1:numel(variable2_split)
                %         val = str2double(variable2_split{k});
                %         if isnan(val)
                %             if findIndex(variable_list, variable2_split{k})
                %                 idx = findIndex(variable_list, variable2_split{k});
                %                 variable2_split{k} = variable_list{idx}{2};
                %             elseif findIndex(arr_variable_list, variable2_split{k})
                %                 idx = findIndex(arr_variable_list, variable2_split{k});
                %                 disp("new value:")
                %                 disp(arr_variable_list{idx}{3})
                %                 variable2_split{k} = arr_variable_list{idx}{3};
                %             end
                %         end
                %         arr_variable_list{i}{3}{k} = variable2_split{k};
                %         variable{3}{k} = variable2_split{k};
                %         disp(['SETTING ARRAY VARIABLE ' arr_variable_list{i}{1}])
                %         disp('AS')
                %         disp(arr_variable_list{i}{3})
                %         disp(arr_variable_list{i}{3}{k})
                %     end
                    disp("VARIABLE SET AS ")
                    disp(variable{3})
                    arr_variable_list{i}{3} = variable{3};
                end
                disp(['SETTING ARRAY VARIABLE ' arr_variable_list{i}{1}])
                disp('AS')
                disp(arr_variable_list{i})                
                % arr_variable_list{i}{3} = variable{3};               
                disp(arr_variable_list{i}{3})
                arr_variable_list{i}{2} = variable{2};
            end
        end
    end
    disp('  ')
    % disp(strcat('VARIABLE ', 32, variable{1}, ' SET. RHS IS ', 32, variable{2}, ' NEW VALUE IS:'))
    disp(strcat('VARIABLE ', 32, variable{1}, ' SET.')) 
    disp(' RHS IS:')
    variable{2}
    disp(' NEW VALUE IS:')
    variable{:}
    disp('  ')
    % disp("FINISHED ALL SETTING!!!!!!!!!!!")
    % for jj = 1:numel(arr_variable_list)
    %     disp(arr_variable_list{jj}{1})
    %     disp(arr_variable_list{jj}{3})
    %     % arr_variable_list{jj}{3}{1}
    % end
    % disp("DONE")
    
    % Remove all the lines that should be removed
    Sequence{1} = {};
    Sequence = Sequence(~cellfun('isempty', Sequence));

end
function Sequence = simplify_If(Sequence, variable_list, arr_variable_list, logExpParam, ExpConstants)
       
%     pause(1)
    % disp('   ')
    % disp(Sequence{1})
    % disp(Sequence{2})
    % disp('   ')

    % Some functions used below
    firstCell = @(x) x{1};
    findIndex = @(list, element) find(strcmp(cellfun(firstCell, list, 'UniformOutput', false), element));

    % Extract the two sides of the condition to be tested
    condition = erase(Sequence{1}, ["if", "then"]);
    condition = split(condition, ["<=", ">=", "=", "<", ">"]);

    % Evaluate the left-hand side
    left = strtrim(condition{1});
    if startsWith(left, '(') && (count(left, "(") > count(left, ")")) % Some if-conditions are inside parenthesis
        left = left(2:end);
        left = strtrim(left);
    end
    
    left_split = split(left, ["+", "-", "/", '*', ')', '(']);

    for k = 1:numel(left_split)
        left_split{k} = strtrim(left_split{k});
        if any(isletter(left_split{k}))
            if findIndex(variable_list, left_split{k})
                i = findIndex(variable_list, left_split{k});
                val = variable_list{i}{2};
            elseif findIndex(logExpParam, left_split{k})
                i = findIndex(logExpParam, left_split{k});
                val = logExpParam{i}{2};
            elseif findIndex(ExpConstants, left_split{k})
                i = findIndex(ExpConstants, left_split{k});
                val = ExpConstants{i}{2};
            elseif findIndex(arr_variable_list, left_split{k}) % TO DO: get specific array index if it's not asking for the whole thing
                i = findIndex(arr_variable_list, left_split{k});
                val = variable_list{i}{2}{:}; % gives whole array
            else
                disp("If - Left - This variable is not defined...")
                % left_split{k}
            end
            left = replace(left, left_split{k}, val);
        end
    end

    % Evaluate the right-hand side, which is a Boolean in some cases
    right = strtrim(condition{2});
    if endsWith(right, ')') && (count(right, ")") > count(right, "(")) 
        right = right(1:end-1);
        right = strtrim(right);
    end

    right_split = split(right, ["+", "-", "/", '*', ')', '(']);
    
    if ~strcmpi(right, 'true') && ~strcmpi(right, 'false')
        for k = 1:numel(right_split)
            right_split{k} = strtrim(right_split{k});
            if any(isletter(right_split{k}))
                if findIndex(variable_list, right_split{k})
                    i = findIndex(variable_list, right_split{k});
                    val = variable_list{i}{2};
                elseif findIndex(logExpParam, right_split{k})
                    i = findIndex(logExpParam, right_split{k});
                    val = logExpParam{i}{2};
                elseif findIndex(ExpConstants, right_split{k})
                    i = findIndex(ExpConstants, right_split{k});
                    val = ExpConstants{i}{2};
                else
                    disp("If - Right - This variable is not defined...")
                    disp(right_split{k})
                    disp(right)
                end
                right = replace(right, right_split{k}, val);
            end
        end
    end
  
    % Determine if the "If" condition is satisfied
    if strcmpi(right, 'true') || strcmpi(right, 'false')
        condition_satisfied = strcmp(left, right);
    else
        if contains(Sequence{1}, '>=')
            condition_satisfied = double(str2sym(left) - str2sym(right)) >= 0;
        elseif contains(Sequence{1}, '<=')
            condition_satisfied = double(str2sym(left) - str2sym(right)) <= 0;
        elseif contains(Sequence{1}, '=')
            condition_satisfied = double(str2sym(left) - str2sym(right)) == 0;
        elseif contains(Sequence{1}, '>')
            condition_satisfied = double(str2sym(left) - str2sym(right)) > 0;      
        elseif contains(Sequence{1}, '<')
            condition_satisfied = double(str2sym(left) - str2sym(right)) < 0;       
        end
    end
    
    % If it is satisfied keep all the lines until Else or End If
    Sequence{1} = {};
    i = 2; % Current line
    if condition_satisfied

        nested_if = 0;
        while (nested_if > 0) || ( ~startsWith(Sequence{i}, "else") && ~startsWith(Sequence{i}, "end if"))
            if startsWith(Sequence{i}, "if")
                nested_if = nested_if + 1;
            elseif startsWith(Sequence{i}, "end if")
                nested_if = nested_if - 1;
            end
            i = i + 1;
        end

        nested_if = 0;
        if startsWith(Sequence{i}, "else")
            Sequence{i} = {};
            i = i + 1;
            while (nested_if > 0) || ( ~startsWith(Sequence{i}, "end if") )
                if startsWith(Sequence{i}, "if")
                    nested_if = nested_if + 1;
                elseif startsWith(Sequence{i}, "end if")
                    nested_if = nested_if - 1;
                end
                Sequence{i} = {};
                i = i + 1;
            end
        end

        % disp('   ')
        % disp(Sequence{i})
        % disp('   ')

        Sequence{i} = {};

    % Else keep only after Else if there is any
    else
        nested_if = 0;
        while (nested_if > 0) || (~startsWith(Sequence{i}, "else") && ~startsWith(Sequence{i}, "end if"))
            if startsWith(Sequence{i}, "if")
                nested_if = nested_if + 1;
            elseif startsWith(Sequence{i}, "end if")
                nested_if = nested_if - 1;
            end
            Sequence{i} = {};
            i = i + 1;
        end

        nested_if = 0;
        if startsWith(Sequence{i}, "else")
%             disp('Here2')
%             disp(Sequence{i})
            Sequence{i} = {};
            i = i + 1;
%             disp(startsWith(Sequence{i}, "end if"))
            while (nested_if > 0) || ~startsWith(Sequence{i}, "end if")
%                 disp('Here3')
%                 disp(Sequence{i})
                if startsWith(Sequence{i}, "if")
                    nested_if = nested_if + 1;
                elseif startsWith(Sequence{i}, "end if")
                    nested_if = nested_if - 1;
                end
                i = i + 1;
            end
        end
        
        Sequence{i} = {};

    end

    % Remove all the lines that should be removed
    Sequence = Sequence(~cellfun('isempty', Sequence));
    
    % disp(' ')
    % disp(' ')
    % disp(' New 2 lines ')
    % for i = 1:2
    %     disp(Sequence{i})
    % end
end
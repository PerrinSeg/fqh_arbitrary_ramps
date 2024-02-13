function channel = return_Channel(instruction, arguments)

    instruction_split = split(instruction, '.');

    if contains(instruction_split{1}, 'digitaldata') 
                    
        channel = arguments{2};
        
        if strcmp(channel, '64')

            channel = 'scope_trigger';

        end

    else
        
        if contains(instruction, lower("DisableClkDist"))
        
            channel = 'clock_synch';
        
        else

            channel = arguments{end};

        end
    end
end
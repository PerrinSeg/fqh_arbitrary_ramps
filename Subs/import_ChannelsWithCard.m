function ChannelsWithCard = import_ChannelsWithCard(path_files)
   
    ChannelsWithCard_line = fopen([path_files 'ChannelsWithCard.txt']); 
    ChannelsWithCard = {}; % Dictionary to store each line
    ChannelsWithCard_aux = fgetl(ChannelsWithCard_line);
    
    while ischar(ChannelsWithCard_aux)
        ChannelsWithCard_aux = strrep(split(ChannelsWithCard_aux, ["=", ","]), ' ', '');
        ChannelsWithCard{end+1} = lower(ChannelsWithCard_aux{1});
        ChannelsWithCard_aux = fgetl(ChannelsWithCard_line);  
    end
    
    % The other one:
    ChannelsWithCard{end+1} = 'clock_synch';
    ChannelsWithCard{end+1} = 'scope_trigger'; % Digital channel 64, see e.g. "digitaldata.AddPulse(64, scope_trigger, scope_trigger+10)"
end
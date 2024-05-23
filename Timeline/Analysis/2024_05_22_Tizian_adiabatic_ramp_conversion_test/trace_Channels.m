%% 2024/02/05 - Plot the traces of the different channels
clear
close all

path_files = '';
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
name_sequence = 'FQH_Tizian_adiabatic_analog';
% name_sequence = 'fqh_smooth_ramp_v6.vb';
batch_line = 2;
plot_sth_happens = 0;
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% Where all the functions are defined
addpath('Z:\Timeline\Subs\') 


%% Some functions

% Use this function to extract the value of a variable base on its name, e.g.
% i = findIndex(variable_list, 'name_variable');
% value_variable = eval(variable_list{i}{2});
firstCell = @(x) x{1};
findIndex = @(list, element) find(strcmp(cellfun(firstCell, list, 'UniformOutput', false), element));


%% Import and read sequence line-by-line

[instruction_list, arguments_list, variable_list, sub_variable_containers, ExpConstants, LogExpParam] ...
    = read_Sequence(path_files, name_sequence, batch_line);
N_inst = numel(instruction_list);

% Load start and end times of all instructions
t_start_all = [];
t_stop_all = [];
for i = 1:N_inst
    disp(i)
    disp(instruction_list{i})
    disp(arguments_list{i})
    [time_aux, values_aux] = instruction_Into_Points(arguments_list{i});
    t_start_all = [t_start_all, min(time_aux)];
    t_stop_all = [t_stop_all, max(time_aux)];
end
disp(' ')


%% Assign instruction to each channel

% Import the different channels' names
ChannelsWithCard = import_ChannelsWithCard(path_files);
N_ChannelsWithCard = numel(ChannelsWithCard);

% Object to contain instructions per channel, labelled by channel
channel_instruction = containers.Map;
channel_instruction_bare = containers.Map;
for j = 1:N_ChannelsWithCard
    channel_instruction(ChannelsWithCard{j}) = {};
    channel_instruction_bare(ChannelsWithCard{j}) = {};
end

for i = 1:N_inst
    channel = return_Channel(instruction_list{i}, arguments_list{i});
    channel_instruction(channel) = [channel_instruction(channel), {arguments_list{i}}];
    channel_instruction_bare(channel) = [channel_instruction_bare(channel), {instruction_list{i}}];
end


%% Look at the values of some variables (use lower letters)
% 
% i = findIndex(variable_list, 'mot_end_time');
% mot_end_time = eval(variable_list{i}{2}); % in ms
% 
% i = findIndex(variable_list, 'transport_start_time');
% transport_start_time = eval(variable_list{i}{2}); % in ms
% i = findIndex(variable_list, 'transport_end_time');
% transport_end_time = eval(variable_list{i}{2}); % in ms
% 
% i = findIndex(variable_list, 'evaporation_end_time');
% evaporation_end_time = eval(variable_list{i}{2}); % in ms

i = findIndex(variable_list, 'twodphysics_start_time');
twodphysics_start_time = eval(variable_list{i}{2}); % in ms
i = findIndex(variable_list, 'twodphysics_end_time');
twodphysics_end_time = eval(variable_list{i}{2}); % in ms

i = findIndex(variable_list, 'pinning_start_time');
pinning_start_time = eval(variable_list{i}{2}); % in ms
i = findIndex(variable_list, 'pinning_ready_time');
pinning_ready_time = eval(variable_list{i}{2}); % in ms
i = findIndex(variable_list, 'pinning_end_time');
pinning_end_time = eval(variable_list{i}{2}); % in ms
% 
% i = findIndex(variable_list, 'last_time');
% last_time = eval(variable_list{i}{2}); % in ms

% version specific variables
% i = findIndex(variable_list, 'line_load_end_time');
% line_load_end_time = eval(variable_list{i}{2}); % in ms
% 
% i = findIndex(variable_list, 'lattice2_raise_start_time');
% lattice2_raise_start_time = eval(variable_list{i}{2}); % in ms
% i = findIndex(variable_list, 'lattice2_raise_end_time');
% lattice2_raise_end_time = eval(variable_list{i}{2}); % in ms
% 
% i = findIndex(variable_list, 'lattice2_freeze_start_time');
% lattice2_freeze_start_time = eval(variable_list{i}{2}); % in ms
% i = findIndex(variable_list, 'lattice2_freeze_end_time');
% lattice2_freeze_end_time = eval(variable_list{i}{2}); % in ms
% 
% i = findIndex(variable_list, 'lattice1_freeze_start_time');
% lattice1_freeze_start_time = eval(variable_list{i}{2}); % in ms
% i = findIndex(variable_list, 'lattice1_freeze_end_time');
% lattice1_freeze_end_time = eval(variable_list{i}{2}); % in ms
% 
% i = findIndex(variable_list, 'go_lower_start_time');
% go_lower_start_time = eval(variable_list{i}{2}); % in ms
% i = findIndex(variable_list, 'go_lower_end_time');
% go_lower_end_time = eval(variable_list{i}{2}); % in ms

%%% If it's in a subsequence
% keys_containers = sub_variable_containers.keys;

% sub_variable_list = sub_variable_containers(keys_containers{3});
% i = findIndex(sub_variable_list, 'twod_start_time');
% twod_start_time = eval(sub_variable_list{i}{2}); % in ms

%%% if it's in exp constants

% i = findIndex(ExpConstants, 'mot_load_time');
% mot_load_time = eval(ExpConstants{i}{2}); % in ms


%% List times when something happens

if plot_sth_happens
    t_start_min = min(t_start_all);
    t_stop_max = max(t_stop_all);
    N = 10^5;
    t_start_show = linspace(t_start_min, t_stop_max, N);
    something_happens = zeros([1, N]);
    for i = 1:N_inst    
        t_start_show = [t_start_show, [t_start_all(i), t_stop_all(i)]];
        something_happens = [something_happens, [1, 1]];
    end
    
    [t_start_show, good_index] = sort(t_start_show);
    something_happens = something_happens(good_index);
    %%%%%%%%%%%%%%%%%%
    plot_figure = 0;
    %%%%%%%%%%%%%%%%%%
    if plot_figure
        figure()
        plot(t_start_show, something_happens, '-', 'Linewidth', 0.5)
    end
end


%% List what happens around a particular instant

%%%%%%%%%%%%%%%%%%%%%%%
list_instructions = 0;
%%%%%%%%%%%%%%%%%%%%%%%
if list_instructions
    t_event = full_counting_start_time;
    t_before = t_event - 10;
    t_after = t_event + 10;
    
    counter_instruction = 1;    
    for i = 1:N_inst       
        [time_aux, values_aux] = instruction_Into_Points(arguments_list{i});
        this_instruction_happens = ( time_aux(1) <= time_aux(end) ) ...
            && ( ( (time_aux(1) >= t_before) && (time_aux(1) <= t_after) ) ...
            || ( (time_aux(end) >= t_before) && (time_aux(end) <= t_after) ) );            
        if this_instruction_happens
            disp( ['Number = ', num2str(counter_instruction)] )
            disp( ['    ', instruction_list{i}] )
            disp( arguments_list{i} )
            counter_instruction = counter_instruction + 1;
        end
    end
end


%% Multiple traces at the same time (use lower letters for the channels)

% 'quad_fet'
% 'quad_shim'
% 'offset_fet'
% 'bias_enable'
% 'ps5_enable'
% 'ioffe_mirror_fet'
% 'transport_13'
% 'evap_switch'
% 'evap_ttl'
% 'imaging_coil'
% 'ps1_shunt'
% 'ps4_shunt'
% 'ps5_shunt'
% 'ps7_shunt'
%
% pinning
% 'axial795_power'
% 'lattice2D795_power' (2) (_ttl, _ttl2, _shutter)
% 'bfield_compensation'
% 'bfield_compensation2'
% 'galvo_voltage_small'
% 'galvo_voltage_big'
% 'quic_molasses_power'
% 'ttl_molasses1' (2)
% 'axial_molasses_power'
% 'molasses_shaker' (_axial)
% 'quic_molasses_shutter'
% 'axial_molasses_shutter'
% 'ixon_camera'
% 'ttl_80MHz'
% 'ttl_133MHz'
% 'ttl_78MHz'
%
% channel_list = {'ps6_ao', 'ps8_ao', 'lattice2d765_power', 'lattice2d765_power2', 'red_dipole_power', 'line_dmd_power', 'hor_dmd_power'};
% channel_list = {'lattice2d765_power', 'red_dipole_power', 'line_dmd_power', 'blue_dipole_ttl', 'blue_dipole_shutter', 'anticonfin_ttl', 'anticonfin_shutter'};
% channel_list = {'lattice2d765_power', 'lattice2d765_power2', 'red_dipole_power', 'blue_dipole_ttl', 'blue_dipole_shutter', 'line_dmd_power', 'line_dmd_trigger', 'hor_dmd_trigger', 'hor_dmd_power', 'ps5_ao'};
% channel_list = {'lattice2d765_power', 'lattice2d765_power2', 'line_dmd_power', 'line_dmd_trigger', 'hor_dmd_trigger', 'hor_dmd_power'};
% channel_list = {'lattice2d765_power', 'lattice2d765_power2', 'ps5_ao', 'line_dmd_power', 'line_dmd_trigger', 'hor_dmd_power', 'hor_dmd_trigger'};
% channel_list = {'lattice2d765_power', 'lattice2d765_power2', 'ps5_ao', 'axial_lattice_power', 'big_lattice_power'};
%
% MOT channels full
% channel_list = {'mot_low_current', 'ta_shutter', 'repump_shutter', ...
%     'mot_detuning', 'mot_high_current', 'cap_discharge', ...
%     'ttl_80mhz', 'ttl_97mhz', 'ttl_n133mhz', 'ttl_n78mhz', 'ttl_78mhz', ...
%     'optical_pumping', 'polarizer_shutter_11'}; 
%
% % MOT channels inverted
% channel_list = {'mot_low_current', 'ta_shutter', 'repump_shutter'}; 
%
% MOT channels inverted + DMD tracking
% channel_list = {'mot_low_current', 'ta_shutter', 'repump_shutter', 'apogee_camera', ...
%     'line_dmd_ttl', 'hor_dmd_ttl', 'lattice2d765_ttl', 'lattice2d765_shutter', 'ixon_flip_mount_ttl'}; 
% 
% % DMD tracking
% channel_list = {'mot_low_current', 'apogee_camera', 'line_dmd_ttl', 'hor_dmd_ttl', 'lattice2d765_ttl', 'lattice2d765_shutter', 'ixon_flip_mount_ttl'}; 
%
% FQH
% channel_list = {'lattice2d765_power', 'lattice2d765_power2', 'line_dmd_power', ...
    % 'hor_dmd_power', 'ps6_ao', 'ps5_ao', 'ps8_ao', 'gauge1_power', 'gauge2_power', 'gauge2_rf_fm', 'gauge1_pztx', 'gauge_ttl', 'gauge_shutter'};
% channel_list = {'lattice2d765_power', 'lattice2d765_power2', ...
%     'line_dmd_power', 'hor_dmd_power', ...
%     'ps1_ao', 'ps5_ao', 'ps6_ao', 'ps8_ao', ...
%     'quad_shim','single_quad_shim','quad_shim2'};
% channel_list = {'ps5_ao', 'quad_shim', 'single_quad_shim', 'quad_shim2'};
% channel_list = {'lattice2d765_power', 'lattice2d765_power2', 'line_dmd_power', ...
%     'hor_dmd_power', 'ps6_ao', 'ps5_ao', 'ps8_ao', 'gauge1_power', 'gauge2_power', 'gauge2_rf_fm', 'gauge1_pzty', 'gauge2_pzty', 'gauge_ttl', 'gauge_shutter'};

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
channel_list = {'lattice2d765_power', 'lattice2d765_power2', 'ps1_ao', ...
    'ps3_ao', 'ps5_ao', 'ps8_ao', 'gauge1_power'};
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

time_list = {};
values_list = {};

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Time window to look at
% t_start_plot = min(t_start_all);
t_start_plot = twodphysics_start_time-20;

t_stop_plot = pinning_start_time+20;
% t_stop_plot = max(t_stop_all);
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
N_chan = numel(channel_list);
time_list = cell(1,N_chan);
values_list = cell(1,N_chan);

for k = 1:N_chan
    
    channel = channel_list{k};
    list_instructions = channel_instruction(channel);
    list_instructions_bare = channel_instruction_bare(channel);
    N_inst_2 = numel(list_instructions);

    % Initialization for the time-span variables
    t_start = 0;
    t_stop = 0;
    
    % These will contain the time evolution
    time = [];
    values = [];
    
    for j = 1:N_inst_2
        overwrite_flag = 0;
        digital_flag = 0;
        time_end_seg = [];

        disp(list_instructions_bare(j))
        ramp_type_split = split(list_instructions_bare(j),'.');
        ramp_type = ramp_type_split{1};
        if contains(ramp_type,'digitaldata')
            digital_flag = 1;
        end

        int_aux = list_instructions(j);
        disp(int_aux{1})

        [time_aux, values_aux] = instruction_Into_Points(list_instructions{j});
        if numel(time_aux) > 0
            t_start = min(time_aux);
            t_stop_aux = max(time_aux);
            if time_aux(1) > time_aux(end)
                disp("Do nothing")
            elseif t_start > t_stop % If the new timestep is not stuck to the previous one
                time = [time, t_stop + 10^(-6), t_start - 10^(-6)]; % Put something in between, the 1 nanosecond should not be visible
                values = [values, 0, 0];
            elseif t_stop > t_start
                overwrite_flag = 1;
                disp('Previous step finishes after the new one...')
                good_index = time <= t_start;
                good_index2 = time >= t_stop_aux;
                time_end_seg = time(good_index2);
                values_end_seg = values(good_index2);

                if digital_flag
                    disp(' ')
                    disp('WARNING: potential digital overwrite!!!!')
                    disp(' ')
                    new_values_inter = zeros(size(values_aux));
                    inter_values_idx = (time > t_start) & (time < t_stop_aux);
                    old_values_inter = values(inter_values_idx);
                    new_values_inter(1:numel(old_values_inter)) = old_values_inter;
                    values_aux = xor(new_values_inter, values_aux);
                end

                time = [time(good_index), t_start];
                values = values(good_index);
                values = [values, values(end)];
            end
            if ~(time_aux(1) > time_aux(end))
                time = [time, time_aux];
                values = [values, values_aux];                    
                if overwrite_flag && (numel(time_end_seg) > 0)
                    if time_end_seg(1) > time(end)
                        time = [time, time(end) + 10^(-7), time_end_seg(1) - 10^(-7)]; % Put something in between, the 1 nanosecond should not be visible
                        values = [values, 0, 0];
                    end
                    time = [time,time_end_seg];
                    values = [values, values_end_seg];
                end
                t_stop = max(time);
            end
        end
    end
    
    if t_stop < t_stop_plot
        time = [time, t_stop + 10^(-6), t_stop_plot - 10^(-6)]; 
        values = [values, 0, 0];
    end

    time_list{k} = time;
    values_list{k} = values;

end

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
plot_figure = 1;
save_figure = 1;
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
ax = [];

if plot_figure
    N_chan_tot = N_chan;
    if plot_sth_happens
        N_chan_tot = N_chan+1;
    end 
    figure('Units','normalized', 'OuterPosition', [0.25, 0.03, 0.5, 0.97])
    tl = tiledlayout(N_chan_tot, 1, 'TileSpacing', 'compact', 'Padding', 'compact');
    for k = 1:N_chan
        ax(k) = nexttile;
        hold on
        plot(time_list{k} ./ 1000, values_list{k}, 'Linewidth', 1.5)
        xlim([t_start_plot / 1000, t_stop_plot / 1000])
        if (max(values_list{k}) > 0)
            ylim([0 max(values_list{k})])
        end
        ylabel(replace(channel_list{k}, '_', ' '))
        
        %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        % xline(mot_end_time/1000, '-', 'Linewidth', 1.5, 'DisplayName', 'mot_end') 
        % xline(evaporation_end_time/1000, '-.', 'Linewidth', 1.25, 'DisplayName', 'evap_end')
        xline(pinning_start_time/1000, '-', 'Linewidth', 1.5, 'DisplayName', 'pinning_start')
%         xline(pinning_ready_time/1000, '--', 'Linewidth', 1, 'DisplayName', 'pinning_start')
        % xline(last_time/1000, '-', 'Linewidth', 1.5, 'DisplayName', 'last_time')
        xline(twodphysics_start_time/1000, ':', 'DisplayName', 'twodphysics_start', 'Linewidth', 1.5)
        xline(twodphysics_end_time/1000, ':', 'DisplayName', 'twodphysics_end', 'Linewidth', 1.5)  
        
%         xline(line_load_end_time/1000, '-.r', 'DisplayName', 'line_load_end')  
        
%         xline(lattice2_raise_start_time/1000, '--g', 'DisplayName', 'lattice2_raise_start', 'Linewidth', 1)
%         xline(lattice2_raise_end_time/1000, '-.g', 'DisplayName', 'lattice2_raise_end', 'Linewidth', 1)  

        % xline(lattice2_freeze_start_time/1000, '--b', 'DisplayName', 'lattice2_freeze_start', 'Linewidth', 1)
        % xline(lattice2_freeze_end_time/1000, '-.b', 'DisplayName', 'lattice2_freeze_end', 'Linewidth', 1)

%         xline(go_lower_start_time/1000, '--c', 'DisplayName', 'go_lower_start', 'Linewidth', 1)
%         xline(go_lower_end_time/1000, '-.c', 'DisplayName', 'go_lower_end', 'Linewidth', 1)

        %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

        if ~(k==N_chan)
            xticklabels([])
        end
    end    
    
    if plot_sth_happens
        ax(k+1) = nexttile;
        plot(t_start_show ./ 1000, 5 * something_happens, '-', 'Linewidth', 1.5);
        xlim([t_start_plot / 1000, t_stop_plot / 1000])
        xlabel('Time (s)')
        ylabel('Something happens')
        xline(IT/1000, '--', 'Linewidth', 1.5)
    end
    
    xlabel(tl, 'Time (s)')
    title(tl, replace(name_sequence,'_', '\_'))
    box on
    linkaxes(ax, 'x')
    legend('Location','eastoutside', 'Interpreter','none')
    % ylim([0,5.2])
    hold off

    if save_figure
        print(['timeline_plots'],'-dpng')
    end

end

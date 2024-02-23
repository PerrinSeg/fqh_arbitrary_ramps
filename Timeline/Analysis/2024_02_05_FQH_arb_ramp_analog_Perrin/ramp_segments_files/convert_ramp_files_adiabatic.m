clear 
close all

%% read ramp file

ramp = readmatrix('ramp_points_adiabatic.txt'); 
ramp_time = ramp(:, 1); 
ramp_amps = ramp(:, 2:end); 
% ramp_amps(:,1) = J_x
% ramp_amps(:,2) = J_y
% ramp_amps(:,3) = Delta_x
% ramp_amps(:,4) = Delta_y
% ramp_amps(:,4) = V0_x


%% Constants

tau = 4.3*10^-3;
h = 6.6260695729 * 10^(-34);    % Planck Constant
hbar = h / 2 / pi;
J0 = hbar/tau/h;
ntimes = length(ramp_time);
nvariables = 6;

ramp_time_ms = ramp_time*tau*10^3;
ramp_amps_hz = ramp_amps*J0;


%% Plot raw values

plot_figure = 1;
save_figure = 0;
if plot_figure
    figure
    tl = tiledlayout(2,1,"TileSpacing",'compact','Padding','compact');
    
    ax1 = nexttile;
    plot(ramp_time*tau*10^3, ramp_amps(:,1)*J0, '.-', 'DisplayName','J_{quad}')
    hold on
    plot(ramp_time*tau*10^3, ramp_amps(:,2)*J0, '.-', 'DisplayName','J_{quic}')
    ylabel('J (Hz)')
    legend('location','best')

    ax2 = nexttile;
    plot(ramp_time*tau*10^3, ramp_amps(:,3)*J0, '.-', 'DisplayName','\Delta_{quad}')
    hold on
    plot(ramp_time*tau*10^3, ramp_amps(:,4)*J0, '.-', 'DisplayName','\Delta_{quic}')
    ylabel('\Delta (Hz)')
    legend('location','best')

    xlabel(tl,'time (ms)')
    linkaxes([ax1,ax2],'x')
    if save_figure
        print('ramp_adiabatic_initial_channels','-dpng')
    end
end


%% convert time points to durations

% convert from tunneling times to ms
ramp_duration = [0; ramp_time_ms(2:end)-ramp_time_ms(1:end-1)];


%% apply appropriate conversions to parameters

ramp_amps_convert = zeros(ntimes, nvariables);
% convert trom tunneling along quic to lattice depth in E_r
% convert trom tunneling along quad to Gauge beam voltage
% convert from quic tilt to DAC voltage (ps5)
% convert from quad tilt to either ps8 DAC voltage OR gauge beam freq (voltage)


%% quad lattice depth 

quadDepth = ramp_amps(:,5);
quadDepth
ramp_amps_convert(:,1) = quadDepth;


%% quad tunneling

quadtunneling = ramp_amps_hz(:,1);
ramp_amps_convert(:,2) = quadtunneling;


%% quic tunneling

quictunneling = ramp_amps_hz(:,2);
ramp_amps_convert(:,3) = quictunneling;


%% Gauge beam freq

gauge_freq_hz = 870;

gauge_freq_full = zeros(ntimes,1);
gauge_freq_full = gauge_freq_full + gauge_freq_hz;
ramp_amps_convert(:,4) = gauge_freq_full/1000;


%% Quad magnetic gradient

gauge_on_idx = 7;
quad_ramp_full = zeros(ntimes,1);
quad_ramp_full(1:gauge_on_idx) = (ramp_amps(1:gauge_on_idx,3)*J0 + 63.3)/1631.0; %2024/01/17
quad_ramp_full(gauge_on_idx:end) = (ramp_amps(gauge_on_idx:end,3)*J0 + gauge_freq_full(gauge_on_idx:end) + 63.3)/1631.0; %2024/01/17

ramp_amps_convert(:,5) = quad_ramp_full;


%% quic magnetic gradient

ramp_amps_convert(:,6) = (ramp_amps(:,4)*J0 + 290.3)/134.6; % 2024/01/09


%% save as new file

ramp_new = [ramp_duration, ramp_amps_convert];
ramp_new
% save as txt file 
fid = fopen('ramp_segments_adiabatic.txt','w');
fprintf(fid, '%f %f %f %f %f %f %f\r\n', ramp_new');
fclose(fid);

% ramp_new(:,0) = Delta t (ms)
% ramp_new(:,1) = V0_x (Er)
% ramp_new(:,2) = J_x (Hz)
% ramp_new(:,3) = J_y (Hz)
% ramp_new(:,4) = gauge detuning (Hz)
% ramp_new(:,5) = gradV_x (V)
% ramp_new(:,6) = gradV_y (V)


%% Plot final values

plot_figure = 1;
save_figure = 1;
if plot_figure
    i = 1;

    figure('Units','normalized', 'OuterPosition', [0.25, 0.03, 0.4, 0.97])
    tl2 = tiledlayout(5,1, "TileSpacing", 'compact', 'Padding', 'compact');
    
    ax(i) = nexttile;
    i = i+1;
    plot(ramp_time_ms, ramp_amps_convert(:,1), '.-', 'DisplayName', 'quad')
    ylabel('Quad lattice depth (E_r)')
 
    ax(i) = nexttile;
    i = i+1;
    hold on
    plot(ramp_time_ms, ramp_amps_convert(:,2), '.-', 'DisplayName', 'quad (gauge power)')
    plot(ramp_time_ms, ramp_amps_convert(:,3), '.-', 'DisplayName', 'quic (lattice depth)')
    ylabel('J (Hz)')
    legend('location','best')

    ax(i) = nexttile;
    plot(ramp_time_ms, ramp_amps_convert(:,4), '.-', 'DisplayName', 'gauge freq')
    ylabel('Gauge freq (Hz)')
    i = i+1;

    ax(i) = nexttile;
    plot(ramp_time_ms, ramp_amps_convert(:,5), '.-', 'DisplayName','quad')
    ylabel('Quad magnetic gradient (V)')
    i = i+1;

    ax(i) = nexttile;
    plot(ramp_time_ms, ramp_amps_convert(:,6), '.-', 'DisplayName','quic')
    ylabel('Quic magnetic gradient (V)')
    i = i+1;

    xlabel(tl2,'time (ms)')
    linkaxes(ax,'x')

    if save_figure
        print('ramp_adiabatic_final_channels','-dpng')
    end
end


%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
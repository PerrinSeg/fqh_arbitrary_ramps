clear 
% close all

%% read ramp file
ramp = readmatrix('ramp_points_adiabatic.txt'); 
ramp_time = ramp(:, 1); 
ramp_amps = ramp(:, 2:end); 
% ramp_amps(:,1) = J_x
% ramp_amps(:,2) = J_y
% ramp_amps(:,3) = Delta_x
% ramp_amps(:,4) = Delta_y
% ramp_amps(:,4) = V0_x

tau = 4.3*10^-3;
h = 6.6260695729 * 10^(-34);    % Planck Constant
hbar = h / 2 / pi;
J0 = hbar/tau/h;
ntimes = length(ramp_time);
nvariables = 6;

%% Plot raw values

plot_figure = 1;
save_figure = 1;
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


%% apply appropriate conversions to parameters

ramp_amps_convert = zeros(ntimes, nvariables);
% convert trom tunneling along quic to lattice depth in E_r
% convert trom tunneling along quad to Gauge beam voltage
% convert from quic tilt to DAC voltage (ps5)
% convert from quad tilt to either ps8 DAC voltage OR gauge beam freq (voltage)


%% quad lattice depth 

% quad_latt_depth_init = 45;
% quad_latt_depth_final = 5;
% quad_latt_ramp_start_idx = 6;
% quad_latt_ramp_end_idx = 7;
% 
% quadDepth = zeros(ntimes,1);
% quadDepth = quadDepth + quad_latt_depth_init;
% quadDepth(quad_latt_ramp_end_idx:end) = quad_latt_depth_final; % TO DO: adjust so that if it spans multiple points, it linearly interpolates between start and end depths

quadDepth = ramp_amps(:,5);
quadDepth
ramp_amps_convert(:,1) = quadDepth;


%% quad Gauge beam power (assuming 2D1 lattice depth 5Er)

quadtunneling = ramp_amps(:,1)*J0;
quadvolts = zeros(size(quadtunneling));
syms v
for i = 1:length(quadvolts)
    jx = quadtunneling(i);
    if jx == 0
        val = 0;
    else
        val = vpasolve( jx == 123.8 * besselj(1, 1.37 * 10^(2*v-6)), v, [0, 3.05] );
    end
    quadvolts(i) = val;
end
ramp_amps_convert(:,2) = quadvolts;


%% quic depth

quictunneling = ramp_amps(:,2)*J0;
quicdepth = zeros(size(quictunneling));
syms x
for i = 1:length(quictunneling)
    jy = quictunneling(i);
    if jy == 0
        val = 45;
    else   
        val = vpasolve(jy == 1229*4/sqrt(pi)*124/100*x^(3/4)*exp(-2*x^(1/2)), x, [1,Inf]);
    end
    quicdepth(i) = val;
end
ramp_amps_convert(:,3) = quicdepth;


%% Gauge beam freq

gauge_freq_hz = 700;

gauge_freq_full = zeros(ntimes,1);
gauge_freq_full = gauge_freq_full + gauge_freq_hz;
ramp_amps_convert(:,4) = gauge_freq_full;


%% Quad magnetic gradient

% quad_grad_ramp_start_idx = 4;
% quad_grad_ramp_end_idx = 5;
% quad_grad_hold_end_idx = quad_latt_ramp_end_idx;
% 
% high_grad_level_Hz = 950;
% low_grad_level_Hz = 0;

% high_grad_level_V = (high_grad_level_Hz + 63.3)/1631.0;
% low_grad_level_V = (low_grad_level_Hz + 63.3)/1631.0

gauge_on_idx = 7;
quad_ramp_full = zeros(ntimes,1);
quad_ramp_full(1:gauge_on_idx) = (ramp_amps(1:gauge_on_idx,3)*J0 + 63.3)/1631.0; %2024/01/17
quad_ramp_full(gauge_on_idx:end) = (ramp_amps(gauge_on_idx:end,3)*J0 + gauge_freq_full(gauge_on_idx:end) + 63.3)/1631.0; %2024/01/17

% quad_ramp_full(1:quad_grad_ramp_start_idx) = low_grad_level_V;
% quad_ramp_full(quad_grad_ramp_end_idx:quad_grad_hold_end_idx) = high_grad_level_V; 

ramp_amps_convert(:,5) = quad_ramp_full;


%% quic magnetic gradient

ramp_amps_convert(:,6) = (ramp_amps(:,4)*J0 + 290.3)/134.6; % 2024/01/09


%% convert time points to durations

% convert from tunneling times to ms

ramp_time = ramp_time*tau*10^3;
ramp_duration = [0; ramp_time(2:end)-ramp_time(1:end-1)];


%% save as new file

ramp_new = [ramp_duration, ramp_amps_convert];
ramp_new
% save as txt file 
fid = fopen('ramp_segments_adiabatic.txt','w');
fprintf(fid, '%f %f %f %f %f %f %f\r\n', ramp_new');
fclose(fid);

% ramp_new(:,0) = Delta t
% ramp_new(:,1) = V0_x
% ramp_new(:,2) = V_gauge (voltage corresponding to gauge power)
% ramp_new(:,3) = V0_y
% ramp_new(:,4) = gauge detuning V
% ramp_new(:,5) = gradV_x
% ramp_new(:,6) = gradV_y


%% Plot final values

plot_figure = 1;
save_figure = 0;
if plot_figure
    figure
    tl2 = tiledlayout(5,1,"TileSpacing",'compact','Padding','compact');
    
    ax1 = nexttile;
    plot(ramp_time*tau*10^3, ramp_amps_convert(:,1), '.-', 'DisplayName', 'quad')
    hold on
    plot(ramp_time*tau*10^3, ramp_amps_convert(:,3), '.-', 'DisplayName', 'quic')
    ylabel('Lattice depth (E_r)')
    legend('location','best')

    ax2 = nexttile;
    plot(ramp_time*tau*10^3, ramp_amps_convert(:,2), '.-', 'DisplayName', 'gauge power')
    ylabel('gauge power (V)')

    ax3 = nexttile;
    plot(ramp_time*tau*10^3, ramp_amps_convert(:,4), '.-', 'DisplayName', 'gauge freq')
    ylabel('gauge freq (Hz)')

    ax4 = nexttile;
    plot(ramp_time*tau*10^3, ramp_amps_convert(:,5), '.-', 'DisplayName','quad')
    ylabel('Quad magnetic gradient (V)')

    ax5 = nexttile;
    plot(ramp_time*tau*10^3, ramp_amps_convert(:,6), '.-', 'DisplayName','quic')
    ylabel('Quic magnetic gradient (V)')
   
    xlabel(tl2,'time (ms)')
    linkaxes([ax1,ax2,ax3,ax4,ax5],'x')

    if save_figure
        print('ramp_adiabatic_final_channels','-dpng')
    end
end
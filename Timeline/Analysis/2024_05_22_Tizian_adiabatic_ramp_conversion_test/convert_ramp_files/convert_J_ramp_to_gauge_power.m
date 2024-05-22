% Convert tunneling ramps from .txt file to new file with the interpolated
% ramp in units of Raman voltage

clear
close all


%% Load ramp file

ramp = readmatrix('ramp_points.txt'); 
% times, J_x, J_y, Delta_x, Delta_y
ramp_time_full = ramp(:, 1); 
ramp_amps = ramp(:, 2:end); 

%%%%%%%%%%%%%%
mid_idx = 5;
%%%%%%%%%%%%%%
ramp_Jx = ramp_amps(mid_idx:end, 1);
ramp_time = ramp(mid_idx:end, 1) - ramp(mid_idx);

% Constants
tau = 4.3*10^-3;
h = 6.6260695729 * 10^(-34);
hbar = h / 2 / pi;
J0 = hbar/tau/h;
% nvariables = 6;

ramp_time_ms_aux = ramp_time*tau*10^3;
ramp_Jx_hz = ramp_Jx*J0;


%% Plot raw values

%%%%%%%%%%%%%%%%
plot_figure = 1;
save_figure = 0;
%%%%%%%%%%%%%%%%
if plot_figure
    clear ax
    figure
    tl = tiledlayout('flow', "TileSpacing",'compact', 'Padding','compact');
    
    ax(1) = nexttile;
    hold on
    plot(ramp_time, ramp_Jx_hz, '.-', 'DisplayName','J_{quad}')
    % plot(ramp_time, ramp_amps(:,2)*J0, '.-', 'DisplayName','J_{quic}')
    ylabel('J (Hz)')
    legend('location','best')

    % ax2 = nexttile;
    % plot(ramp_time, ramp_amps(:,3)*J0, '.-', 'DisplayName','\Delta_{quad}')
    % hold on
    % plot(ramp_time, ramp_amps(:,4)*J0, '.-', 'DisplayName','\Delta_{quic}')
    % ylabel('\Delta (Hz)')
    % legend('location','best')

    xlabel(tl,'time (\tau)')
    linkaxes(ax,'x')
    if save_figure
        print('ramp_Jx_initial','-dpng')
    end
end


%% Interpolate ramp with correct number of points

N_pts_new  = 10000; % points needed for NI function
ramp_times_new = linspace(0,1,N_pts_new);
ramp_time_norm = ramp_time/ramp_time(end);
% ramp_Jx_norm = (ramp_Jx - min(ramp_Jx)) / (max(ramp_Jx) - min(ramp_Jx)); % rescale ramp to be between 0 and 1??
% ramp_Jx_new = interp1(ramp_time_norm, ramp_Jx, ramp_times_new, 'makima');
ramp_Jx_new = interp1(ramp_time_norm, ramp_Jx, ramp_times_new, 'linear');
ramp_Jx_combined = [ramp_times_new', ramp_Jx_new'];

%%%%%%%%%%%%%%%%
plot_figure = 1;
save_figure = 0;
%%%%%%%%%%%%%%%%
if plot_figure
    figure()
    tiledlayout('Flow', TileSpacing = 'compact', Padding = 'compact')
    
    nexttile
    hold on
    plot(ramp_Jx_combined(:,1), ramp_Jx_combined(:,2), '-', 'LineWidth', 1.5)
    plot(ramp_time_norm, ramp_Jx, 'o', 'LineWidth', 1.5)
    ylabel('J_{gauge} (J)')
    xlabel('Time')

    if save_figure
        print([save_file_header, 'J_ramp_interpolated'], '-dpng')
    end
end


%% Convert interpolated ramp to voltage

% Calibrated values
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
E_r = 1.24 * 10^3;          % E_r in kHz
b1 = 108.4;                 % 1st fit coeff from V to J calibration         
b2 = 1.399 * 10^-6;         % 2nd fit coeff form V to J calibration
dcal_quad = 7.4*10^3/E_r;   % lattice depth vcal_quad
vcal_quad = 3.34;           % lattice depth calibration voltage
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
b2_full = b2*10^(2*vcal_quad)/dcal_quad;

% Full V -> J equation from exp calibraton
voltToJ = @(v)  b1 * besselj( 1, b2*10.^(2*v) );

% Break into two steps to make numerics easier (i.e. get rid of divergence near J = 0)
voltToDepth = @(v) dcal_quad * 10.^(2*(v-vcal_quad));    % Step 1: convert voltage to lattice depth (in E_r)
depthToJ = @(d)  b1 * besselj( 1, b2_full*d );           % Step 2: convert lattice depth to J
depthToVolt = @(d) (1/2)*log10(d/dcal_quad) + vcal_quad; % Step 1 can be inverted analytically

% Numerically invert depth -> J equation to get best value for each point

% convert Jx (J) -> Raman power d_R (E_r)
tic
d_r = zeros(1,length(ramp_Jx_new));
syms d
parfor i = 1:length(ramp_Jx_new)
    jx = ramp_Jx_new(i);
    if jx == 0
        val = 0;
    else
        val = vpasolve( jx == depthToJ(d), d, [0, 1.5] );
    end
   d_r(i) = val;
end
toc

% convert d_R (E_r) -> input voltage (V)
ramp_Vx = depthToVolt(d_r);
ramp_Vx(ramp_Vx < 0) = 0;


%% Check results

Vx_aux = linspace(0, 3.0, 10000);
Jx_aux = voltToJ(Vx_aux);

% Plot
%%%%%%%%%%%%%%%%%
plot_figure = 1;
%%%%%%%%%%%%%%%%%
if plot_figure
    figure
    tl2 = tiledlayout('flow','tilespacing','compact');
    
    ax1 = nexttile;
    hold on
    plot(ramp_Jx_new, ramp_Vx, '-', 'DisplayName', 'numeric')
    plot(Jx_aux, Vx_aux, '--', 'DisplayName', 'full')
    ylabel('Gauge voltage (V)')
    legend('location','best')

    xlabel(tl2, 'J_x (J)')
    title(tl2, 'Voltage check quad')
    % linkaxes([ax1,ax2],'x')
    xlim([0 1.3])
end


%% Plot results as a function of time

% Normalize V to lie between 0 and 1 (TO DO: is this necessary??? double check...)
ramp_Vx_new = (ramp_Vx - min(ramp_Vx)) / (max(ramp_Vx) - min(ramp_Vx));
ramp_Vx_comb = [ramp_times_new', ramp_Vx_new'];

% Plot
%%%%%%%%%%%%%%%%%
plot_figure = 1;
save_figure = 0;
%%%%%%%%%%%%%%%%%
if plot_figure
    figure
    t = tiledlayout('flow','tilespacing','compact');
    
    nexttile;
    plot(ramp_Vx_comb(:,1), ramp_Vx_comb(:,2), '-', DisplayName = 'numeric', LineWidth=1.5)
    ylabel('Gauge voltage (V)')

    xlabel(t, 'time (\tau)')
    title(t, 'Final voltage ramp J_{quad}')

    if save_figure
        print([save_file_header, 'J_ramp_final'], '-dpng')
    end
end


%% Check by converting back to J and comparing with original ramp

% Plot
%%%%%%%%%%%%%%%%%
plot_figure = 1;
save_figure = 0;
%%%%%%%%%%%%%%%%%
if plot_figure
    figure
    t = tiledlayout('flow','tilespacing','compact');
    nexttile
    hold on
    plot(ramp_Vx_comb(:,1), voltToJ(ramp_Vx), '-', DisplayName = 'converted', LineWidth = 1.5)
    plot(ramp_time/ramp_time(end), ramp_Jx, '.', MarkerSize = 15, DisplayName = 'J_{quad}')
    ylabel('Tunneling (J)')

    xlabel(t, 'time (\tau)')
    title(t, 'Final ramp J_{quad}')

    if save_figure
        print([save_file_header, 'J_ramp_check'], '-dpng')
    end
end


%% Save new interpolated ramp as .txt file

%%%%%%%%%%%%%%%
save_ramp = 1;
%%%%%%%%%%%%%%%
% save as txt file 
if save_ramp
    fid = fopen('Vx_ramp.txt', 'w');
    fprintf(fid, '%f %f\r\n', ramp_Vx_comb');
    fclose(fid);
end

